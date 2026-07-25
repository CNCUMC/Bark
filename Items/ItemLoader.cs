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
    private static readonly Dictionary<string, Dictionary<string, (ItemScriptDef def, string modDir)>> PendingScripts =
        new();

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
        var sprite = LoadItemSprite(def.OriginPrefab, itemId, assetsDir, def.SpriteImportScale);

        ItemRegistry.Register(itemId, info, sprite);

        // 暂存脚本映射（如有），待引擎就绪后由 RegisterScripts 写入 ItemScriptRegistry
        StashScript(itemId, def.Script, modId, modDir);

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
        var sprite = LoadItemSprite(def.OriginPrefab, itemId, assetsDir, def.SpriteImportScale);

        ItemRegistry.Register(itemId, info, sprite);

        // 暂存脚本映射
        StashScript(itemId, def.Script, modId, modDir);

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
            fullName = def.FullName,
            description = def.Description,
            category = def.Category,
            slotRotation = def.SlotRotation,
            destroyAtZeroCondition = def.DestroyAtZeroCondition,
            weight = def.Weight,
            onlyHoldInHands = def.OnlyHoldInHands,
            wearable = def.Wearable,
            wearableCanBeHeld = def.WearableCanBeHeld,
            wearableArmor = def.WearableArmor,
            wearableIsolation = def.WearableIsolation,
            desiredWearLimb = def.DesiredWearLimb,
            wearSlotId = def.WearSlotId,
            wearableHitDurabilityLossMultiplier = def.WearableHitDurabilityLossMultiplier,
            scaleWeightWithCondition = def.ScaleWeightWithCondition,
            WearableSortingOrder = def.WearableSortingOrder,
            combineable = def.Combinable,
            ignoreDepression = def.IgnoreDepression,
            value = def.Value,
            wearableVisualOffset = def.WearableVisualOffset,
            tags = def.Tags,
            decayInfo = def.DecayInfo,
            decayMinutes = def.DecayMinutes,
            rec = new Recognition(def.Recognition),
            SpawnFrequency = def.SpawnFrequency,
            WorldSpawnPerChunk = def.WorldSpawnPerChunk,
            SpriteScale = def.SpriteScale,
            InventoryIconScale = def.InventoryIconScale,
        };

        // Sprite 缩放维度：优先用 JSON 配置，未配置则回退到 prefab 精灵尺寸
        if (def.SpriteScaleDimensions is { Width: > 0f, Height: > 0f })
        {
            var ssd = def.SpriteScaleDimensions;
            info.SpriteScaleDimensions = new SpriteScaleDimensions(ssd.Width, ssd.Height, ssd.ExpandToFirstMet);
        }
        else
        {
            try
            {
                var prefab = Resources.Load<GameObject>(def.OriginPrefab);
                if (prefab != null)
                {
                    var prefabSprite = prefab.GetComponent<SpriteRenderer>()?.sprite;
                    if (prefabSprite != null)
                        info.SpriteScaleDimensions =
                            new SpriteScaleDimensions(prefabSprite.rect.width, prefabSprite.rect.height, true);
                }
            }
            catch
            {
                // prefab 不存在时忽略
            }
        }

        // 穿戴贴图: Assets/Item/{itemId}_worn.png
        info.WornSprite = ItemUtil.LoadSprite(Path.Combine(assetsDir, itemId + "_worn.png"), def.SpriteImportScale);
        info.WornSpriteOffset = new Vector2(def.WornSpriteOffsetX, def.WornSpriteOffsetY);

        // MultiWorn: Assets/Item/{itemId}_mw_{key}.png
        if (def.MultiWorn != null)
        {
            foreach (var kv in def.MultiWorn)
            {
                var multiSprite = ItemUtil.LoadSprite(Path.Combine(assetsDir, itemId + "_mw_" + kv.Key + ".png"),
                    def.SpriteImportScale);
                if (multiSprite == null) continue;
                info.MultiWornSprites[kv.Key] = multiSprite;
                info.MultiWornSpriteOffsets[kv.Key] = new Vector2(
                    kv.Value.WornSpriteOffsetX, kv.Value.WornSpriteOffsetY);
            }
        }

        // DropPool
        if (def.DropPool is { Length: > 0 })
        {
            try
            {
                info.DropPool = (DropPool)Enum.Parse(typeof(DropPool), string.Join(",", def.DropPool));
            }
            catch
            {
                // 无效的 DropPool 值忽略
            }
        }

        // 腐烂速度
        if (def.RotSpeed.HasValue)
            info.rotSpeed = def.RotSpeed.Value;
        else if (def.DecayMinutes > 0)
            info.rotSpeed = 1f / def.DecayMinutes;

        // 制作特性
        if (def.Qualities != null)
        {
            info.qualities =
            [
                .. def.Qualities
                    .Where(q => !string.IsNullOrEmpty(q.Id))
                    .Select(q => new CraftingQuality(q.Id.ToLowerInvariant(), q.Amount))
            ];
        }

        // 容器
        if (def.ContainerData != null)
        {
            var cd = def.ContainerData;
            info.Container = new ContainerProperties
            {
                Capacity = cd.MaxWeight,
                MaxWeightPerItem = cd.MaxWeightPerItem,
                ItemsVisible = cd.ItemsVisible,
                TagRestriction = cd.TagRestriction,
                EncumbranceReduction = cd.EncumbranceMult,
            };
        }

        // 电池
        if (def.BatteryData == null) return info;
        var bd = def.BatteryData;
        info.Battery = new BatteryProperties
        {
            SpawnWithBattery = bd.SpawnWithBattery,
        };

        switch (bd.Preset.ToLowerInvariant())
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
                if (bd.SpawnWithBattery)
                {
                    info.Battery.BatteryType = bd.BatteryType;
                    info.Battery.MaxCharge = bd.MaxAllowedCharge;
                }

                break;
        }

        return info;
    }

    // ---- Builder: 液体容器 ----

    private static CustomItemInfo BuildLiquidItemInfo(LiquidItemDef def, string itemId, string assetsDir)
    {
        var info = BuildItemInfo(def, itemId, assetsDir);

        info.capacity = def.Capacity;
        info.autoFill = def.AutoFill;

        // 液体填充贴图: Assets/Item/{itemId}_fill.png
        info.LiquidMask = ItemUtil.LoadSprite(Path.Combine(assetsDir, itemId + "_fill.png"), def.SpriteImportScale);

        // 默认液体
        if (def.DefaultLiquid is { Count: > 0 })
        {
            info.defaultContents = [];
            foreach (var kv in def.DefaultLiquid)
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
            description = def.Description,
            color = ItemUtil.HexToColor(def.Color),
            valuePerLiter = def.ValuePerLiter,
            healthUsable = def.HealthUsable,
            injectable = def.Injectable,
            injectionSickness = def.InjectionSicknessMultiplier,
            localeFromItem = def.LocaleFromItem,
        };

        // 制作特性
        if (def.Qualities == null) return info;
        foreach (var kv in def.Qualities)
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
        var isEmpty = scriptDef.Use.Count == 0
                      && scriptDef.Equip.Count == 0
                      && scriptDef.Unequip.Count == 0
                      && scriptDef.UseOnLimb.Count == 0;
        if (isEmpty) return;

        if (!PendingScripts.TryGetValue(modId, out var itemScripts))
        {
            itemScripts = new Dictionary<string, (ItemScriptDef, string)>();
            PendingScripts[modId] = itemScripts;
        }

        itemScripts[itemId] = (scriptDef, modDir);
    }
}