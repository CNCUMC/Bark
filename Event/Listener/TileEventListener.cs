using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private static readonly HashSet<int> KnownCustomIndices = [];
    private static readonly Dictionary<long, int> DestroyTracker = new();

    private static Coroutine? _existCoroutine;
    private static Coroutine? _damageCoroutine;
    private static MonoBehaviour? _runner;

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;

        RefreshKnownIndices();

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

        KnownCustomIndices.Clear();
        DestroyTracker.Clear();
        _runner = null;
    }

    private static void RefreshKnownIndices()
    {
        KnownCustomIndices.Clear();
        foreach (var entry in TileLoader.LoadedTiles.Values.SelectMany(list => list))
            KnownCustomIndices.Add(entry.TileIndex);
    }

    [HarmonyPatch(typeof(WorldGeneration), "SetBlock", typeof(Vector2Int),
        typeof(ushort))]
    [HarmonyPrefix]
    private static void WorldGenerationSetBlockPrefix(WorldGeneration __instance,
        ref Vector2Int pos, ushort block)
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

        if (index < TileRegistry.FirstCustomTileIndex || !IsCustomIndex(index)) return;
        {
            var tileId = FindTileIdByIndex(index);
            if (tileId == null) return;
            EventUtil.Trigger(new TilePlaceEvent
            {
                TileId = tileId,
                TileIndex = index,
                PosX = pos.x,
                PosY = pos.y
            });

            var hash = PackPos(pos.x, pos.y);
            DestroyTracker[hash] = index;
        }
    }

    [HarmonyPatch(typeof(WorldGeneration), "DamageBlock", typeof(Vector2Int),
        typeof(float))]
    [HarmonyPostfix]
    private static void WorldGenerationDamageBlockPostfix(WorldGeneration __instance,
        ref Vector2Int pos, float dmg)
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
        return KnownCustomIndices.Contains(index);
    }

    private static string? FindTileIdByIndex(int tileIndex)
    {
        return (
            from list in TileLoader.LoadedTiles.Values
            from entry in list
            where entry.TileIndex == tileIndex
            select entry.TileId).FirstOrDefault();
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
