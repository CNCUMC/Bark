using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bark.Script;
using Bark.Tool;
using CUCoreLib.Data;
using CUCoreLib.Registries;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Tilemaps;

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
// 物块索引自动分配（>= 36），模组无需在 mod.json 中声明 tiles 映射。
public static class TileLoader
{
    // 模组已加载的物块列表（modId → 物块记录）
    public static readonly Dictionary<string, List<TileEntry>> LoadedTiles = new();

    // 暂存的脚本映射（modId → tileId → (scriptDef, modDir)），待引擎创建后注册
    private static readonly Dictionary<string, Dictionary<string, (TileScriptDef def, string modDir)>> PendingScripts =
        new();

    // 下一个可用物块索引（>= 36，0~35 为原版保留）
    private static ushort _nextTileIndex = 36;

    // TileRegistry 内部字典缓存：直接操作以支持热重载（ClearOwnerEntries 在当前 CUCoreLib 版本中不存在）
    private static readonly FieldInfo? s_registeredDefinitionsField = typeof(TileRegistry).GetField(
        "RegisteredDefinitions", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly FieldInfo? s_registeredTilesField = typeof(TileRegistry).GetField(
        "RegisteredTiles", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly FieldInfo? s_registeredDefinitionIdsField = typeof(TileRegistry).GetField(
        "RegisteredDefinitionIds", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly FieldInfo? s_resolvedHitSoundsField = typeof(TileRegistry).GetField(
        "ResolvedHitSounds", BindingFlags.NonPublic | BindingFlags.Static);

    // 从模组目录加载所有自定义物块
    public static void RegisterFromMod(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        var tilesDir = Path.Combine(manifest.Directory, "Tile");
        if (!Directory.Exists(tilesDir))
            return;

        var jsonFiles = Directory.GetFiles(tilesDir, "*.json", SearchOption.TopDirectoryOnly);
        if (jsonFiles.Length == 0)
            return;

        // 热重载：保存旧索引映射以便复用
        var oldIndices = LoadedTiles.TryGetValue(manifest.Id, out var oldTiles)
            ? oldTiles.ToDictionary(e => e.TileId, e => e.TileIndex)
            : null;

        // 清除该模组之前注册的物块
        ClearModTiles(manifest.Id);

        // 资产目录：ModDir/Assets/Tile/
        var assetsTileDir = Path.Combine(manifest.Directory, "Assets", "Tile");

        var loadedList = new List<TileEntry>();
        var loadedCount = 0;

        // 按文件名排序，保证索引分配确定性
        Array.Sort(jsonFiles);

        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                var tileId = Path.GetFileNameWithoutExtension(jsonFile);

                // 自动分配索引：热重载复用旧索引，否则使用下一个可用索引
                ushort tileIndex;
                if (oldIndices != null && oldIndices.TryGetValue(tileId, out var oldIndex))
                {
                    tileIndex = (ushort)oldIndex;
                }
                else
                {
                    tileIndex = _nextTileIndex++;
                }

                var entry = LoadAndRegister(jsonFile, assetsTileDir, manifest.Id, tileId, tileIndex);
                if (entry == null) continue;
                loadedCount++;
                loadedList.Add(entry);
            }
            catch (Exception ex)
            {
                LogUtil.Error("tiles.load_error", jsonFile, manifest.Id, ex.Message);
            }
        }

        LoadedTiles[manifest.Id] = loadedList;

        // 暂存脚本映射，待引擎就绪后由 RegisterScripts 写入 TileScriptRegistry
        if (PendingScripts.TryGetValue(manifest.Id, out var existing) && existing.Count > 0)
            LogUtil.Info("tiles.scripts_pending", manifest.Id, existing.Count);

        if (loadedCount > 0)
            LogUtil.Info("tiles.loaded_count", manifest.Id, loadedCount);
    }

    // 在引擎就绪后，将暂存的物块脚本映射写入 TileScriptRegistry
    public static void RegisterScripts(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));
        if (manifest.Engine is null)
            return;

        if (!PendingScripts.TryGetValue(manifest.Id, out var tileScripts) || tileScripts.Count == 0)
            return;

        foreach (var (tileId, (scriptDef, modDir)) in tileScripts)
            TileScriptRegistry.Register(tileId, scriptDef, manifest.Engine, manifest.Id, modDir);

        var count = tileScripts.Count;
        PendingScripts.Remove(manifest.Id);
        LogUtil.Info("tiles.scripts_registered", manifest.Id, count);
    }

    // 加载并注册单个物块 JSON，成功时返回记录项，失败返回 null
    private static TileEntry? LoadAndRegister(string jsonFile, string assetsDir, string modId,
        string tileId, ushort tileIndex)
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

        // 构建 CustomTileDefinition
        var tileDef = BuildTileDefinition(def, tileId, assetsDir);
        if (tileDef == null)
            return null;

        TileRegistry.Register(tileIndex, tileDef);
        LogUtil.Info("tiles.registered", tileId, tileIndex, modId);

        // 暂存脚本映射（如有），待引擎就绪后由 RegisterScripts 写入 TileScriptRegistry
        StashScript(tileId, def.Script, modId, modDir: Path.GetDirectoryName(Path.GetDirectoryName(jsonFile)) ?? string.Empty);

        var fileName = Path.GetFileName(jsonFile);
        return new TileEntry(tileIndex, tileId, fileName);
    }

    // 将 JSON TileDef 转换为 CUCoreLib CustomTileDefinition
    // tileId: 文件名（不含扩展名），作为物块的稳定 ID
    private static CustomTileDefinition? BuildTileDefinition(TileDef def, string tileId, string assetsDir)
    {
        // 精灵图加载：Assets/Tile/{tileId}.png
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
            Name = string.IsNullOrWhiteSpace(def.Name) ? tileId : def.Name,
            Description = def.Description ?? string.Empty,
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
            var drops = (from drop in def.Drops
                    where !string.IsNullOrWhiteSpace(drop.Id)
                    select BuildingEntityRegistry.AddDrop(drop.Id, drop.Chance, drop.ConditionMin, drop.ConditionMax))
                .ToList();

            if (drops.Count > 0)
                result.Drops = [.. drops];
        }

        // 自定义元数据
        if (def.CustomData is { Count: > 0 })
            result.CustomData = new Dictionary<string, object>(def.CustomData);

        return result;
    }

    // 清除指定模组之前注册的物块（直接操作 TileRegistry 内部字典）
    private static void ClearModTiles(string ownerId)
    {
        if (!LoadedTiles.TryGetValue(ownerId, out var oldTiles))
        {
            LoadedTiles.Remove(ownerId);
            PendingScripts.Remove(ownerId);
            return;
        }

        // 收集该模组的物块索引和 ID
        var indicesToRemove = new HashSet<ushort>(oldTiles.Count);
        var idsToRemove = new HashSet<string>(oldTiles.Count);
        foreach (var entry in oldTiles)
        {
            indicesToRemove.Add((ushort)entry.TileIndex);
            idsToRemove.Add(entry.TileId);
        }

        // 从 TileRegistry 内部字典逐个移除
        if (s_registeredDefinitionsField?.GetValue(null) is Dictionary<ushort, CustomTileDefinition> defs)
            foreach (var index in indicesToRemove)
                defs.Remove(index);

        if (s_registeredTilesField?.GetValue(null) is Dictionary<ushort, TileBase> tiles)
            foreach (var index in indicesToRemove)
                tiles.Remove(index);

        if (s_registeredDefinitionIdsField?.GetValue(null) is Dictionary<string, ushort> ids)
            foreach (var id in idsToRemove)
                ids.Remove(id);

        if (s_resolvedHitSoundsField?.GetValue(null) is Dictionary<ushort, AudioClip> hitSounds)
            foreach (var index in indicesToRemove)
                hitSounds.Remove(index);

        LoadedTiles.Remove(ownerId);
        PendingScripts.Remove(ownerId);
    }

    // 暂存物块脚本映射，待引擎就绪后注册
    private static void StashScript(string tileId, TileScriptDef? scriptDef, string modId, string modDir)
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