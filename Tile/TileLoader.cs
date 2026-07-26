using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Bark.Script;
using Bark.Tool;
using CUCoreLib.Data;
using CUCoreLib.Registries;
using Newtonsoft.Json;
using UnityEngine;

namespace Bark.Tile;

// 已加载物块的记录项
public class TileEntry(int tileIndex, string tileId, string fileName)
{
    // 物块索引（与 CustomTileDefinition 绑定的 int 索引）
    public int TileIndex = tileIndex;

    // 物块 ID
    public string TileId = tileId;

    // 来源文件名（如 "marble.json"）
    public string FileName = fileName;
}

// 自定义物块加载器：扫描 ModDir/Tile/*.json，
// 构建 CustomTileDefinition 并调用 TileRegistry.Register。
public static class TileLoader
{
    // 模组已加载的物块列表（modId → 物块记录）
    public static readonly Dictionary<string, List<TileEntry>> LoadedTiles = new();

    // ClearOwnerEntries 是 internal，缓存 MethodInfo 供热重载时清除旧物块
    private static readonly MethodInfo? s_clearOwnerEntries = typeof(TileRegistry).GetMethod(
        "ClearOwnerEntries", BindingFlags.NonPublic | BindingFlags.Static);

    // 从模组目录加载所有自定义物块
    public static void RegisterFromMod(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        // 重载时先清除该模组之前注册的物块
        ClearModTiles(manifest.Id);

        var tilesDir = Path.Combine(manifest.Directory, "Tile");
        if (!Directory.Exists(tilesDir))
            return;

        var jsonFiles = Directory.GetFiles(tilesDir, "*.json", SearchOption.TopDirectoryOnly);
        if (jsonFiles.Length == 0)
            return;

        // 资产目录：ModDir/Assets/Tile/
        var assetsTileDir = Path.Combine(manifest.Directory, "Assets", "Tile");

        var loadedList = new List<TileEntry>();
        var loadedCount = 0;

        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                var entry = LoadAndRegister(jsonFile, assetsTileDir, manifest.Id);
                if (entry == null) continue;
                loadedCount++;
                loadedList.Add(entry);
            }
            catch (Exception ex)
            {
                LogUtil.Error("items.load_error", jsonFile, manifest.Id, ex.Message);
            }
        }

        LoadedTiles[manifest.Id] = loadedList;

        if (loadedCount > 0)
            LogUtil.Info("tiles.loaded_count", manifest.Id, loadedCount);
    }

    // 加载并注册单个物块 JSON，成功时返回记录项，失败返回 null
    private static TileEntry? LoadAndRegister(string jsonFile, string assetsDir, string modId)
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

        if (def is null || string.IsNullOrWhiteSpace(def.Id))
        {
            LogUtil.Warning("tiles.missing_id", jsonFile);
            return null;
        }

        if (def.TileIndex < 36)
        {
            LogUtil.Warning("tiles.index_too_low", jsonFile, def.TileIndex);
            return null;
        }

        // 构建 CustomTileDefinition
        var tileDef = BuildTileDefinition(def, assetsDir);
        if (tileDef == null)
            return null;

        TileRegistry.Register((ushort)def.TileIndex, tileDef);
        LogUtil.Info("tiles.registered", def.Id, def.TileIndex, modId);

        var fileName = Path.GetFileName(jsonFile);
        return new TileEntry(def.TileIndex, def.Id, fileName);
    }

    // 将 JSON TileDef 转换为 CUCoreLib CustomTileDefinition
    private static CustomTileDefinition? BuildTileDefinition(TileDef def, string assetsDir)
    {
        // 精灵图加载：Assets/Tile/{id}.png
        var spritePath = Path.Combine(assetsDir, def.Id + ".png");
        var sprite = ItemUtil.LoadSprite(spritePath, def.SpriteImportScale);
        if (sprite == null)
        {
            LogUtil.Warning("tiles.sprite_not_found", spritePath, def.Id);
            return null;
        }

        var result = new CustomTileDefinition
        {
            ID = def.Id,
            Name = string.IsNullOrWhiteSpace(def.Name) ? def.Id : def.Name,
            Sprite = sprite,
            Health = def.Health,
            HitSound = def.HitSound,
            StepSound = def.StepSound,
            NoVariation = def.NoVariation,
            Metallic = def.Metallic,
            Toxicity = def.Toxicity,
            Slippery = def.Slippery,
            SpawnAmount = def.SpawnAmount,
        };

        if (!string.IsNullOrWhiteSpace(def.TileName))
            result.TileName = def.TileName;

        if (!string.IsNullOrWhiteSpace(def.Color))
            result.Color = ItemUtil.HexToColor(def.Color);

        if (!string.IsNullOrWhiteSpace(def.ColliderType)
            && Enum.TryParse<UnityEngine.Tilemaps.Tile.ColliderType>(def.ColliderType, true, out var colliderType))
        {
            result.ColliderType = colliderType;
        }

        if (!string.IsNullOrWhiteSpace(def.SleepQuality)
            && Enum.TryParse<Body.SleepQuality>(def.SleepQuality, true, out var sleepQuality))
        {
            result.SleepQuality = sleepQuality;
        }

        // 生成层
        if (def.SpawnLayers is { Length: > 0 })
            result.SpawnLayers = TileRegistry.LayersToMask(def.SpawnLayers);

        // 生成样式
        if (def.GenerationStyle is { Length: > 0 }
            && Enum.TryParse<TileGenerationStyle>(
                string.Join(",", def.GenerationStyle), true, out var genStyle))
        {
            result.GenerationStyle = genStyle;
        }

        // 掉落物品
        if (def.Drops is { Length: > 0 })
        {
            var drops = new List<ItemDrop>();
            foreach (var drop in def.Drops)
            {
                if (string.IsNullOrWhiteSpace(drop.Id)) continue;
                drops.Add(BuildingEntityRegistry.AddDrop(
                    drop.Id, drop.Chance, drop.ConditionMin, drop.ConditionMax));
            }

            if (drops.Count > 0)
                result.Drops = drops.ToArray();
        }

        // 自定义元数据
        if (def.CustomData is { Count: > 0 })
            result.CustomData = new Dictionary<string, object>(def.CustomData);

        return result;
    }

    // 清除指定模组之前注册的物块（内部调 TileRegistry.ClearOwnerEntries）
    private static void ClearModTiles(string ownerId)
    {
        s_clearOwnerEntries?.Invoke(null, [ownerId, null!]);
        LoadedTiles.Remove(ownerId);
    }
}
