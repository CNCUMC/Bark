using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bark.Liquid;
using Bark.Script;
using Bark.Tool;
using CUCoreLib.Data;
using CUCoreLib.Registries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Bark.Items;

// 已加载物品的记录项
public class ItemEntry(string id, string type)
{
    // 物品 ID（即 JSON 文件名）
    public string Id = id;
    // 物品类型: item / liquid-item / liquid
    public string Type = type;
}

// 自定义物品加载器：扫描 ModDir/Item/*.json 注册物品，
// 图片等资产从 ModDir/Assets/Item/ 读取。
// 脚本映射分两阶段：RegisterFromMod 先注册物品与暂存脚本定义，
// RegisterScripts 在引擎就绪后写入 ItemScriptRegistry。
public static class ItemLoader
{
    // 模组已加载的物品列表（modId → 物品记录），供外部查询
    public static readonly Dictionary<string, List<ItemEntry>> LoadedItems = new();

    // 暂存的脚本映射（modId → itemId → (scriptDef, modDir)），待引擎创建后注册
    private static readonly Dictionary<string, Dictionary<string, (ItemScriptDef def, string modDir)>> PendingScripts = new();

    // ClearOwnerEntries 是 internal，缓存 MethodInfo 供热重载时清除旧物品
    private static readonly MethodInfo? s_clearItemOwnerEntries = typeof(ItemRegistry).GetMethod(
        "ClearOwnerEntries", BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly MethodInfo? s_clearLiquidOwnerEntries = typeof(LiquidRegistry).GetMethod(
        "ClearOwnerEntries", BindingFlags.NonPublic | BindingFlags.Static);

    // 从模组目录加载所有自定义物品
    public static void RegisterFromMod(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        // 重载时先清除该模组之前注册的物品与液体
        ClearModItems(manifest.Id);

        var itemsDir = Path.Combine(manifest.Directory, "Item");
        if (!Directory.Exists(itemsDir))
            return;

        var jsonFiles = Directory.GetFiles(itemsDir, "*.json", SearchOption.TopDirectoryOnly);
        if (jsonFiles.Length == 0)
            return;

        // 资产目录：ModDir/Assets/Item/
        var assetsItemDir = Path.Combine(manifest.Directory, "Assets", "Item");

        var loadedList = new List<ItemEntry>();
        var loadedCount = 0;

        // 标记物品与液体所有权，以便热重载时清除
        using (ItemRegistry.BeginOwnerRegistration(manifest.Id))
        using (LiquidRegistry.BeginOwnerRegistration(manifest.Id))
        {
            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    var entry = LoadAndRegister(jsonFile, assetsItemDir, manifest.Id);
                    if (entry == null) continue;
                    loadedCount++;
                    loadedList.Add(entry);
                }
                catch (Exception ex)
                {
                    LogUtil.Error("items.load_error", jsonFile, manifest.Id, ex.Message);
                }
            }
        }

        LoadedItems[manifest.Id] = loadedList;

        // 暂存脚本映射，待引擎就绪后由 RegisterScripts 写入 ItemScriptRegistry
        if (PendingScripts.TryGetValue(manifest.Id, out var existing) && existing.Count > 0)
            LogUtil.Info("items.scripts_pending", manifest.Id, existing.Count);

        if (loadedCount > 0)
            LogUtil.Info("items.loaded_count", manifest.Id, loadedCount);
    }

    // 在引擎就绪后，将暂存的物品脚本映射写入 ItemScriptRegistry
    public static void RegisterScripts(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));
        if (manifest.Engine is null)
            return;

        if (!PendingScripts.TryGetValue(manifest.Id, out var itemScripts) || itemScripts.Count == 0)
            return;

        foreach (var (itemId, (scriptDef, modDir)) in itemScripts)
            ItemScriptRegistry.Register(itemId, scriptDef, manifest.Engine, manifest.Id, modDir);

        var count = itemScripts.Count;
        PendingScripts.Remove(manifest.Id);
        LogUtil.Info("items.scripts_registered", manifest.Id, count);
    }

    // 加载单个 JSON 文件，自动检测类型并注册。物品 ID 取自文件名（不含扩展名）。
    // assetsDir: ModDir/Assets/Item/，用于加载图片等资产。
    // 成功时返回记录项并暂存脚本映射（如有），失败返回 null。
    private static ItemEntry? LoadAndRegister(string jsonFile, string assetsDir, string modId)
    {
        var itemId = Path.GetFileNameWithoutExtension(jsonFile);

        string json;
        try
        {
            json = File.ReadAllText(jsonFile);
        }
        catch (Exception ex)
        {
            LogUtil.Warning("items.read_failed", jsonFile, ex.Message);
            return null;
        }

        JObject obj;
        try
        {
            obj = JObject.Parse(json);
        }
        catch (JsonException ex)
        {
            LogUtil.Warning("items.invalid_json", jsonFile, ex.Message);
            return null;
        }

        var modDir = Path.GetDirectoryName(Path.GetDirectoryName(jsonFile)) ?? string.Empty;

        // 自动检测类型：capacity → 液体容器, color 且无 weight → 纯液体, 否则 → 普通物品
        if (obj["capacity"] != null)
        {
            var ok = LoadLiquidItem(json, itemId, assetsDir, modId, modDir);
            return ok ? new ItemEntry(itemId, "liquid-item") : null;
        }

        if (obj["color"] != null && obj["weight"] == null)
        {
            var ok = LoadLiquid(json, itemId, modId);
            return ok ? new ItemEntry(itemId, "liquid") : null;
        }

        var ok2 = LoadItem(json, itemId, assetsDir, modId, modDir);
        return ok2 ? new ItemEntry(itemId, "item") : null;
    }

    // ---- 普通物品 ----

    private static bool LoadItem(string json, string itemId, string assetsDir, string modId, string modDir)
    {
        var def = JsonConvert.DeserializeObject<ItemDef>(json);
        if (def is null)
            return false;

        var info = BuildItemInfo(def, itemId, assetsDir);
        var sprite = LoadItemSprite(def.originPrefab, itemId, assetsDir, def.spriteImportScale);

        ItemRegistry.Register(itemId, info, sprite);

        // 暂存脚本映射（如有），待引擎就绪后由 RegisterScripts 写入 ItemScriptRegistry
        StashScript(itemId, def.script, modId, modDir);

        LogUtil.Info("items.item_registered", itemId, modId);
        return true;
    }

    // ---- 液体容器 ----

    private static bool LoadLiquidItem(string json, string itemId, string assetsDir, string modId, string modDir)
    {
        var def = JsonConvert.DeserializeObject<LiquidItemDef>(json);
        if (def is null)
            return false;

        var info = BuildLiquidItemInfo(def, itemId, assetsDir);
        var sprite = LoadItemSprite(def.originPrefab, itemId, assetsDir, def.spriteImportScale);

        ItemRegistry.Register(itemId, info, sprite);

        // 暂存脚本映射
        StashScript(itemId, def.script, modId, modDir);

        LogUtil.Info("items.item_registered", itemId, modId);
        return true;
    }

    // ---- 纯液体 ----

    private static bool LoadLiquid(string json, string liquidId, string modId)
    {
        var def = JsonConvert.DeserializeObject<LiquidDef>(json);
        if (def is null)
            return false;

        var info = BuildLiquidInfo(def, liquidId);
        LiquidRegistry.Register(liquidId, info);
        LogUtil.Info("items.liquid_registered", liquidId, modId);
        return true;
    }

    // ---- Builder: 普通物品 ----

    private static CustomItemInfo BuildItemInfo(ItemDef def, string itemId, string assetsDir)
    {
        var info = new CustomItemInfo
        {
            fullName = def.fullName,
            description = def.description,
            category = def.category,
            slotRotation = def.slotRotation,
            destroyAtZeroCondition = def.destroyAtZeroCondition,
            weight = def.weight,
            onlyHoldInHands = def.onlyHoldInHands,
            wearable = def.wearable,
            wearableCanBeHeld = def.wearableCanBeHeld,
            wearableArmor = def.wearableArmor,
            wearableIsolation = def.wearableIsolation,
            desiredWearLimb = def.desiredWearLimb,
            wearSlotId = def.wearSlotId,
            wearableHitDurabilityLossMultiplier = def.wearableHitDurabilityLossMultiplier,
            scaleWeightWithCondition = def.scaleWeightWithCondition,
            WearableSortingOrder = def.wearableSortingOrder,
            combineable = def.combineable,
            ignoreDepression = def.ignoreDepression,
            value = def.value,
            wearableVisualOffset = def.wearableVisualOffset,
            tags = def.tags,
            decayInfo = def.decayInfo,
            decayMinutes = def.decayMinutes,
            rec = new Recognition(def.recognition),
            SpawnFrequency = def.spawnFrequency,
            WorldSpawnPerChunk = def.worldSpawnPerChunk,
            SpriteScale = def.spriteScale,
            InventoryIconScale = def.inventoryIconScale,
        };

        // Sprite 缩放维度：优先用 JSON 配置，未配置则回退到 prefab 精灵尺寸
        if (def.spriteScaleDimensions is { width: > 0f, height: > 0f })
        {
            var ssd = def.spriteScaleDimensions;
            info.SpriteScaleDimensions = new SpriteScaleDimensions(ssd.width, ssd.height, ssd.expandToFirstMet);
        }
        else
        {
            try
            {
                var prefab = Resources.Load<GameObject>(def.originPrefab);
                if (prefab != null)
                {
                    var prefabSprite = prefab.GetComponent<SpriteRenderer>()?.sprite;
                    if (prefabSprite != null)
                        info.SpriteScaleDimensions = new SpriteScaleDimensions(prefabSprite.rect.width, prefabSprite.rect.height, true);
                }
            }
            catch
            {
                // prefab 不存在时忽略
            }
        }

        // 穿戴贴图: Assets/Item/{itemId}_worn.png
        info.WornSprite = ItemUtil.LoadSprite(Path.Combine(assetsDir, itemId + "_worn.png"), def.spriteImportScale);
        info.WornSpriteOffset = new Vector2(def.wornSpriteOffsetX, def.wornSpriteOffsetY);

        // MultiWorn: Assets/Item/{itemId}_mw_{key}.png
        if (def.multiWorn != null)
        {
            foreach (var kv in def.multiWorn)
            {
                var multiSprite = ItemUtil.LoadSprite(Path.Combine(assetsDir, itemId + "_mw_" + kv.Key + ".png"), def.spriteImportScale);
                if (multiSprite == null) continue;
                info.MultiWornSprites[kv.Key] = multiSprite;
                info.MultiWornSpriteOffsets[kv.Key] = new Vector2(
                    kv.Value.wornSpriteOffsetX, kv.Value.wornSpriteOffsetY);
            }
        }

        // DropPool
        if (def.dropPool is { Length: > 0 })
        {
            try
            {
                info.DropPool = (DropPool)Enum.Parse(typeof(DropPool), string.Join(",", def.dropPool));
            }
            catch
            {
                // 无效的 DropPool 值忽略
            }
        }

        // 腐烂速度
        if (def.rotSpeed.HasValue)
            info.rotSpeed = def.rotSpeed.Value;
        else if (def.decayMinutes > 0)
            info.rotSpeed = 1f / def.decayMinutes;

        // 制作特性
        if (def.qualities != null)
        {
            info.qualities =
            [
                .. def.qualities
                    .Where(q => !string.IsNullOrEmpty(q.id))
                    .Select(q => new CraftingQuality(q.id.ToLowerInvariant(), q.amount))
            ];
        }

        // 容器
        if (def.containerData != null)
        {
            var cd = def.containerData;
            info.Container = new ContainerProperties
            {
                Capacity = cd.maxWeight,
                MaxWeightPerItem = cd.maxWeightPerItem,
                ItemsVisible = cd.itemsVisible,
                TagRestriction = cd.tagRestriction,
                EncumbranceReduction = cd.encumberanceMult,
            };
        }

        // 电池
        if (def.batteryData == null) return info;
        var bd = def.batteryData;
        info.Battery = new BatteryProperties
        {
            SpawnWithBattery = bd.spawnWithBattery,
        };

        switch (bd.preset.ToLowerInvariant())
        {
            case "small":
                info.Battery.StartCharge = 50f;
                info.Battery.Preset = BatteryItem.BatteryPreset.Small;
                break;
            case "medium":
                info.Battery.StartCharge = 100f;
                info.Battery.Preset = BatteryItem.BatteryPreset.Medium;
                break;
            case "large":
                info.Battery.StartCharge = 300f;
                info.Battery.Preset = BatteryItem.BatteryPreset.Large;
                break;
            default:
                if (bd.spawnWithBattery)
                {
                    info.Battery.BatteryType = bd.batteryType;
                    info.Battery.MaxCharge = bd.maxAllowedCharge;
                }
                break;
        }

        return info;
    }

    // ---- Builder: 液体容器 ----

    private static CustomItemInfo BuildLiquidItemInfo(LiquidItemDef def, string itemId, string assetsDir)
    {
        var info = BuildItemInfo(def, itemId, assetsDir);

        info.capacity = def.capacity;
        info.autoFill = def.autoFill;

        // 液体填充贴图: Assets/Item/{itemId}_fill.png
        info.LiquidMask = ItemUtil.LoadSprite(Path.Combine(assetsDir, itemId + "_fill.png"), def.spriteImportScale);

        // 默认液体
        if (def.defaultLiquid is { Count: > 0 })
        {
            info.defaultContents = [];
            foreach (var kv in def.defaultLiquid)
                info.defaultContents.Add(new LiquidStack(kv.Key, kv.Value));
        }
        else
        {
            info.defaultContents = [];
        }

        return info;
    }

    // ---- Builder: 纯液体 ----

    private static CustomLiquidInfo BuildLiquidInfo(LiquidDef def, string liquidId)
    {
        var info = new CustomLiquidInfo
        {
            name = liquidId,
            description = def.description,
            color = ItemUtil.HexToColor(def.color),
            valuePerLiter = def.valuePerLiter,
            healthUsable = def.healthUsable,
            injectable = def.injectable,
            injectionSickness = def.injectionSicknessMultiplier,
            localeFromItem = def.localeFromItem,
        };

        // 制作特性
        if (def.qualities == null) return info;
        foreach (var kv in def.qualities)
            info.qualities.Add(new CraftingQuality(kv.Key.ToLowerInvariant(), kv.Value));
        
        return info;
    }

    // 从 Assets/Item/ 加载物品精灵图：先查 {itemId}.png，回退到 originPrefab 的 SpriteRenderer
    private static Sprite? LoadItemSprite(string originPrefab, string itemId, string assetsDir, float importScale = 1f)
    {
        var sprite = ItemUtil.LoadSprite(Path.Combine(assetsDir, itemId + ".png"), importScale);
        if (sprite != null)
            return sprite;

        try
        {
            var prefab = Resources.Load<GameObject>(originPrefab);
            if (prefab != null)
                sprite = prefab.GetComponent<SpriteRenderer>()?.sprite;
        }
        catch
        {
            // 忽略
        }

        return sprite;
    }

    // 清除指定模组之前注册的物品与液体（内部调 ItemRegistry / LiquidRegistry 的 ClearOwnerEntries）
    private static void ClearModItems(string ownerId)
    {
        s_clearItemOwnerEntries?.Invoke(null, [ownerId, null!]);
        s_clearLiquidOwnerEntries?.Invoke(null, [ownerId, null!]);
        LoadedItems.Remove(ownerId);
        PendingScripts.Remove(ownerId);
    }

    // 暂存物品脚本映射，待引擎就绪后注册
    private static void StashScript(string itemId, ItemScriptDef? scriptDef, string modId, string modDir)
    {
        if (scriptDef is null) return;
        var isEmpty = scriptDef.use.Count == 0
                      && scriptDef.equip.Count == 0
                      && scriptDef.unequip.Count == 0
                      && scriptDef.useOnLimb.Count == 0;
        if (isEmpty) return;

        if (!PendingScripts.TryGetValue(modId, out var itemScripts))
        {
            itemScripts = new Dictionary<string, (ItemScriptDef, string)>();
            PendingScripts[modId] = itemScripts;
        }

        itemScripts[itemId] = (scriptDef, modDir);
    }
}
