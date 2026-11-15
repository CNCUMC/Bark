using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bark.Events;
using Bark.Tile;
using Bark.Tool;
using CUCoreLib.Registries;
using HarmonyLib;
using UnityEngine;

namespace Bark.Event.Listener;

public static class TileEventListener
{
    private const float PollInterval = 1f;
    private const int ScanRadius = 10;
    private static readonly Dictionary<long, int> DestroyTracker = new();

    private static Coroutine? _existCoroutine;
    private static Coroutine? _damageCoroutine;
    private static MonoBehaviour? _runner;
    private static Harmony? _tileHarmony;

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;

        ApplyPatches();

        _existCoroutine ??= runner.StartCoroutine(PollExist());
        _damageCoroutine ??= runner.StartCoroutine(PollDamage());
    }

    internal static void Stop()
    {
        if (_runner == null) return;

        if (_existCoroutine != null)
        {
            _runner.StopCoroutine(_existCoroutine);
            _existCoroutine = null;
        }

        if (_damageCoroutine != null)
        {
            _runner.StopCoroutine(_damageCoroutine);
            _damageCoroutine = null;
        }

        DestroyTracker.Clear();
        _runner = null;
    }

    private static void ApplyPatches()
    {
        if (_tileHarmony == null)
        {
            _tileHarmony = new Harmony(Plugin.Guid + ".tile");

            TryPatch(_tileHarmony, "set_block",
                AccessTools.Method(typeof(WorldGeneration), "SetBlock",
                    [typeof(Vector2Int), typeof(ushort)]),
                AccessTools.Method(typeof(TileEventListener),
                    nameof(WorldGenerationSetBlockPrefix)),
                null);

            TryPatch(_tileHarmony, "damage_block",
                AccessTools.Method(typeof(WorldGeneration), "DamageBlock",
                [
                    typeof(Vector2Int), typeof(float), typeof(bool), typeof(bool),
                    typeof(bool)
                ]),
                null,
                AccessTools.Method(typeof(TileEventListener),
                    nameof(WorldGenerationDamageBlockPostfix)));
        }

        LogPatchState();
    }

    private static void TryPatch(
        Harmony harmony,
        string name,
        MethodBase? target,
        MethodInfo? prefix,
        MethodInfo? postfix)
    {
        if (target == null)
        {
            LogUtil.Warning("tile_event.patch_target_missing", name);
            return;
        }

        try
        {
            harmony.Patch(target,
                prefix == null
                    ? null
                    : new HarmonyMethod(prefix),
                postfix == null
                    ? null
                    : new HarmonyMethod(postfix));
            LogUtil.Info("tile_event.patch_applied", name);
        }
        catch (Exception ex)
        {
            LogUtil.Error("tile_event.patch_failed", name, ex.Message);
        }
    }

    private static void LogPatchState()
    {
        var setBlock = AccessTools.Method(
            typeof(WorldGeneration),
            "SetBlock",
            [
                typeof(Vector2Int),
                typeof(ushort)
            ]);
        var damageBlock = AccessTools.Method(
            typeof(WorldGeneration),
            "DamageBlock",
            [
                typeof(Vector2Int),
                typeof(float),
                typeof(bool),
                typeof(bool),
                typeof(bool)
            ]);

        var setBlockPrefixes = setBlock == null
            ? -1
            : Harmony.GetPatchInfo(setBlock)?.Prefixes?.Count ?? 0;
        var damageBlockPostfixes = damageBlock == null
            ? -1
            : Harmony.GetPatchInfo(damageBlock)?.Postfixes?.Count ?? 0;

        LogUtil.Info("tile_event.patch_state", setBlockPrefixes, damageBlockPostfixes);
    }

    private static void WorldGenerationSetBlockPrefix(
        WorldGeneration __instance,
        ref Vector2Int pos,
        ushort block)
    {
        if (__instance == null || !WorldReady()) return;

        var index = (int)block;

        var oldIndex = GetBlockAt(__instance, pos);

        if (oldIndex >= TileRegistry.FirstCustomTileIndex && oldIndex != index)
        {
            var tileId = FindTileIdByIndex(oldIndex);
            if (tileId != null)
            {
                EventUtil.Trigger(new TileDestroyedEvent
                {
                    TileId = tileId,
                    TileIndex = oldIndex,
                    PosX = pos.x,
                    PosY = pos.y
                });

                var hash = PackPos(pos.x, pos.y);
                DestroyTracker.Remove(hash);
            }
        }

        if (!IsCustomIndex(index)) return;

        {
            var placeTileId = FindTileIdByIndex(index);
            if (placeTileId == null) return;
            EventUtil.Trigger(new TilePlaceEvent
            {
                TileId = placeTileId,
                TileIndex = index,
                PosX = pos.x,
                PosY = pos.y
            });

            var placeHash = PackPos(pos.x, pos.y);
            DestroyTracker[placeHash] = index;
        }
    }

    private static void WorldGenerationDamageBlockPostfix(
        WorldGeneration __instance,
        Vector2Int pos,
        float dmg)
    {
        if (__instance == null || !WorldReady()) return;

        var index = GetBlockAt(__instance, pos);
        if (index < TileRegistry.FirstCustomTileIndex || !IsCustomIndex(index)) return;

        var tileId = FindTileIdByIndex(index);
        if (tileId == null) return;

        EventUtil.Trigger(new TileDamagingEvent
        {
            TileId = tileId,
            TileIndex = index,
            PosX = pos.x,
            PosY = pos.y
        });
    }

    private static IEnumerator PollExist()
    {
        yield return new WaitForSeconds(1f);

        while (_existCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            if (!WorldReady()) continue;

            var body = BodyUtil.Body;
            if (!body) continue;

            var world = WorldGeneration.world;
            if (!world) continue;

            var playerPos = world.WorldToBlockPos(body.transform.position);
            var px = playerPos.x;
            var py = playerPos.y;

            for (var x = px - ScanRadius; x <= px + ScanRadius; x++)
                for (var y = py - ScanRadius; y <= py + ScanRadius; y++)
                {
                    var blockIndex = GetBlockAt(world, new Vector2Int(x, y));
                    if (blockIndex < TileRegistry.FirstCustomTileIndex
                        || !IsCustomIndex(blockIndex)) continue;

                    var tileId = FindTileIdByIndex(blockIndex);
                    if (tileId == null) continue;

                    EventUtil.Trigger(new TileExistEvent
                    {
                        TileId = tileId,
                        TileIndex = blockIndex,
                        PosX = x,
                        PosY = y
                    });
                }
        }
    }

    private static IEnumerator PollDamage()
    {
        yield return new WaitForSeconds(1f);

        while (_damageCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            if (!WorldReady()) continue;

            var body = BodyUtil.Body;
            if (!body) continue;

            var world = WorldGeneration.world;
            if (!world) continue;

            var playerPos = world.WorldToBlockPos(body.transform.position);
            var px = playerPos.x;
            var py = playerPos.y;

            var destroyed = new List<long>();
            foreach (var (hash, tileIndex) in DestroyTracker)
            {
                UnpackPos(hash, out var x, out var y);

                if (Math.Abs(x - px) > ScanRadius || Math.Abs(y - py) > ScanRadius)
                {
                    destroyed.Add(hash);
                    continue;
                }

                var currentIndex = GetBlockAt(world, new Vector2Int(x, y));
                if (currentIndex == tileIndex) continue;
                var tileId = FindTileIdByIndex(tileIndex);
                if (tileId != null)
                    EventUtil.Trigger(new TileDestroyedEvent
                    {
                        TileId = tileId,
                        TileIndex = tileIndex,
                        PosX = x,
                        PosY = y
                    });

                destroyed.Add(hash);
            }

            foreach (var hash in destroyed)
                DestroyTracker.Remove(hash);
        }
    }

    private static bool WorldReady()
    {
        var world = WorldGeneration.world;
        return world && world.width > 0f && world.height > 0f;
    }

    private static int GetBlockAt(WorldGeneration world, Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= world.width || pos.y >= world.height)
            return 0;

        return world.GetBlock(pos);
    }

    private static bool IsCustomIndex(int index)
    {
        return index >= TileRegistry.FirstCustomTileIndex
            && TileRegistry.TryGetDefinition((ushort)index, out _);
    }

    private static string? FindTileIdByIndex(int tileIndex)
    {
        var fromLoader = (
            from list in TileLoader.LoadedTiles.Values
            from entry in list
            where entry.TileIndex == tileIndex
            select entry.TileId).FirstOrDefault();

        if (fromLoader != null) return fromLoader;

        return TileRegistry.TryGetDefinition((ushort)tileIndex, out var definition)
            ? definition?.ID
            : null;
    }

    private static long PackPos(int x, int y)
    {
        return ((long)x << 32) | (uint)y;
    }

    private static void UnpackPos(long hash, out int x, out int y)
    {
        x = (int)(hash >> 32);
        y = (int)(hash & 0xFFFFFFFF);
    }
}
