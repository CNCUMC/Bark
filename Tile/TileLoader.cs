using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bark.Compat.Wmitf;
using Bark.Script;
using Bark.Tool;
using CUCoreLib.Data;
using CUCoreLib.Registries;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Bark.Tile;

public class TileEntry(int tileIndex, string tileId, string fileName)
{
    public string FileName = fileName;
    public string TileId = tileId;
    public int TileIndex = tileIndex;
}

public static class TileLoader
{
    public static readonly Dictionary<string, List<TileEntry>> LoadedTiles = new();

    private static readonly
        Dictionary<string, Dictionary<string, (TileScriptDef def, string modDir)>>
        PendingScripts =
            new();

    private static ushort _nextTileIndex = TileRegistry.FirstCustomTileIndex;

    private static readonly FieldInfo? s_registeredDefinitionsField =
        typeof(TileRegistry).GetField(
            "RegisteredDefinitions", BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly FieldInfo? s_registeredTilesField =
        typeof(TileRegistry).GetField(
            "RegisteredTiles", BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly FieldInfo? s_registeredDefinitionIdsField =
        typeof(TileRegistry).GetField(
            "RegisteredDefinitionIds", BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly FieldInfo? s_resolvedHitSoundsField =
        typeof(TileRegistry).GetField(
            "ResolvedHitSounds", BindingFlags.NonPublic | BindingFlags.Static);

    public static void RegisterFromMod(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        RegisterFromDirectory(manifest.Id, manifest.Directory);
    }

    public static int RegisterFromDirectory(string modId, string modDir,
        bool allowPendingScripts = true)
    {
        if (modId is null)
            throw new ArgumentNullException(nameof(modId));
        if (modDir is null)
            throw new ArgumentNullException(nameof(modDir));

        var tilesDir = Path.Combine(modDir, "Tile");
        if (!Directory.Exists(tilesDir))
            return 0;

        var jsonFiles =
            Directory.GetFiles(tilesDir, "*.json", SearchOption.AllDirectories);
        if (jsonFiles.Length == 0)
            return 0;

        var oldIndices = LoadedTiles.TryGetValue(modId, out var oldTiles)
            ? oldTiles.ToDictionary(e => e.TileId, e => e.TileIndex)
            : null;

        ClearModTiles(modId);

        var assetsTileDir = Path.Combine(modDir, "Assets", "Tile");

        var loadedList = new List<TileEntry>();
        var loadedCount = 0;

        Array.Sort(jsonFiles);

        foreach (var jsonFile in jsonFiles)
            try
            {
                var tileId = Path.GetFileNameWithoutExtension(jsonFile);

                ushort tileIndex;
                if (oldIndices != null
                    && oldIndices.TryGetValue(tileId, out var oldIndex))
                    tileIndex = (ushort)oldIndex;
                else
                    tileIndex = AllocateTileIndex();

                var entry = LoadAndRegister(jsonFile, assetsTileDir, modId, modDir,
                    tileId, tileIndex);
                if (entry == null) continue;
                loadedCount++;
                loadedList.Add(entry);
            }
            catch (Exception ex)
            {
                LogUtil.Error("tiles.load_error", jsonFile, modId, ex.Message);
            }

        LoadedTiles[modId] = loadedList;

        foreach (var entry in loadedList)
            WmitfPatch.RegisterTile((ushort)entry.TileIndex, modId);

        if (PendingScripts.TryGetValue(modId, out var existing) && existing.Count > 0)
        {
            if (allowPendingScripts)
            {
                LogUtil.Info("tiles.scripts_pending", modId, existing.Count);
            }
            else
            {
                LogUtil.Warning("tiles.csharp.scripts_ignored", modId, existing.Count);
                PendingScripts.Remove(modId);
            }
        }

        if (loadedCount > 0)
            LogUtil.Info("tiles.loaded_count", modId, loadedCount);

        return loadedCount;
    }

    public static void UnregisterOwner(string modId)
    {
        if (modId is null)
            throw new ArgumentNullException(nameof(modId));
        ClearModTiles(modId);
    }

    public static void RegisterScripts(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));
        if (manifest.Engine is null)
            return;

        if (!PendingScripts.TryGetValue(manifest.Id, out var tileScripts)
            || tileScripts.Count == 0)
            return;

        foreach (var (tileId, (scriptDef, modDir)) in tileScripts)
            TileScriptRegistry.Register(tileId, scriptDef, manifest.Engine, manifest.Id,
                modDir);

        var count = tileScripts.Count;
        PendingScripts.Remove(manifest.Id);
        LogUtil.Info("tiles.scripts_registered", manifest.Id, count);
    }

    private static ushort AllocateTileIndex()
    {
        if (s_registeredDefinitionsField?.GetValue(null) is
            Dictionary<ushort, CustomTileDefinition> defs)
            while (_nextTileIndex <= byte.MaxValue
                   && defs.ContainsKey(_nextTileIndex))
                _nextTileIndex++;

        return _nextTileIndex++;
    }

    private static TileEntry? LoadAndRegister(string jsonFile, string assetsDir,
        string modId,
        string modDir, string tileId, ushort tileIndex)
    {
        TileDef? def;
        try
        {
            def = JsonUtil.ReadFile<TileDef>(jsonFile);
        }
        catch (Exception ex)
        {
            LogUtil.Warning("tiles.parse_failed", jsonFile, ex.Message);
            return null;
        }

        if (def is null)
        {
            LogUtil.Warning("tiles.parse_failed", jsonFile, "null result");
            return null;
        }

        var tileDef = BuildTileDefinition(def, tileId, assetsDir);
        if (tileDef == null)
            return null;

        TileRegistry.Register(tileIndex, tileDef);
        LogUtil.Info("tiles.registered", tileId, tileIndex, modId);

        StashScript(tileId, def.Script, modId, modDir);

        var fileName = Path.GetFileName(jsonFile);
        return new TileEntry(tileIndex, tileId, fileName);
    }

    private static CustomTileDefinition? BuildTileDefinition(TileDef def, string tileId,
        string assetsDir)
    {
        var spritePath = Path.Combine(assetsDir, tileId + ".png");
        var sprite = ItemUtil.LoadSprite(spritePath, def.SpriteImportScale);
        if (sprite == null)
        {
            LogUtil.Warning("tiles.sprite_not_found", spritePath, tileId);
            return null;
        }

        var result = new CustomTileDefinition
        {
            ID = tileId,
            Name = string.IsNullOrWhiteSpace(def.Name)
                ? tileId
                : def.Name,
            Description = def.Description ?? string.Empty,
            Sprite = sprite,
            Health = def.Health,
            HitSound = def.HitSound,
            StepSound = def.StepSound,
            NoVariation = def.NoVariation,
            Metallic = def.Metallic,
            Toxicity = def.Toxicity,
            Slippery = def.Slippery,
            SpawnAmount = def.SpawnAmount
        };

        if (!string.IsNullOrWhiteSpace(def.Color))
            result.Color = ItemUtil.HexToColor(def.Color);

        if (!string.IsNullOrWhiteSpace(def.ColliderType)
            && Enum.TryParse<UnityEngine.Tilemaps.Tile.ColliderType>(def.ColliderType,
                true, out var colliderType))
            result.ColliderType = colliderType;

        if (!string.IsNullOrWhiteSpace(def.SleepQuality)
            && Enum.TryParse<Body.SleepQuality>(def.SleepQuality, true,
                out var sleepQuality))
            result.SleepQuality = sleepQuality;

        if (def.SpawnLayers is { Length: > 0 })
            result.SpawnLayers = TileRegistry.LayersToMask(def.SpawnLayers);

        if (def.GenerationStyle is { Length: > 0 }
            && Enum.TryParse<TileGenerationStyle>(
                string.Join(",", def.GenerationStyle), true, out var genStyle))
            result.GenerationStyle = genStyle;

        if (def.Drops is { Length: > 0 })
        {
            var drops = (
                    from drop in def.Drops
                    where !string.IsNullOrWhiteSpace(drop.Id)
                    select BuildingEntityRegistry.AddDrop(drop.Id, drop.Chance,
                        drop.ConditionMin, drop.ConditionMax))
                .ToList();

            if (drops.Count > 0)
                result.Drops = [.. drops];
        }

        if (def.CustomData is { Count: > 0 })
            result.CustomData = new Dictionary<string, object>(def.CustomData);

        return result;
    }

    private static void ClearModTiles(string ownerId)
    {
        if (!LoadedTiles.TryGetValue(ownerId, out var oldTiles))
        {
            LoadedTiles.Remove(ownerId);
            PendingScripts.Remove(ownerId);
            return;
        }

        var indicesToRemove = new HashSet<ushort>(oldTiles.Count);
        var idsToRemove = new HashSet<string>(oldTiles.Count);
        foreach (var entry in oldTiles)
        {
            indicesToRemove.Add((ushort)entry.TileIndex);
            idsToRemove.Add(entry.TileId);
        }

        if (s_registeredDefinitionsField?.GetValue(null) is
            Dictionary<ushort, CustomTileDefinition> defs)
            foreach (var index in indicesToRemove)
                defs.Remove(index);

        if (s_registeredTilesField?.GetValue(null) is Dictionary<ushort, TileBase> tiles)
            foreach (var index in indicesToRemove)
                tiles.Remove(index);

        if (s_registeredDefinitionIdsField?.GetValue(null) is Dictionary<string, ushort>
            ids)
            foreach (var id in idsToRemove)
                ids.Remove(id);

        if (s_resolvedHitSoundsField?.GetValue(null) is Dictionary<ushort, AudioClip>
            hitSounds)
            foreach (var index in indicesToRemove)
                hitSounds.Remove(index);

        LoadedTiles.Remove(ownerId);
        PendingScripts.Remove(ownerId);
    }

    private static void StashScript(string tileId, TileScriptDef? scriptDef, string modId,
        string modDir)
    {
        if (scriptDef is null) return;
        var isEmpty = scriptDef.OnPlace.Count == 0
            && scriptDef.OnExist.Count == 0
            && scriptDef.OnDamaging.Count == 0
            && scriptDef.OnDestroyed.Count == 0;
        if (isEmpty) return;

        if (!PendingScripts.TryGetValue(modId, out var tileScripts))
        {
            tileScripts = new Dictionary<string, (TileScriptDef, string)>();
            PendingScripts[modId] = tileScripts;
        }

        tileScripts[tileId] = (scriptDef, modDir);
    }
}
