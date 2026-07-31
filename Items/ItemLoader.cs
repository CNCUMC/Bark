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

    // 已注册为 wearable 但缺少穿戴贴图的物品 ID 集合，供热 Harmony 守卫跳过装备防止 NRE
    public static readonly HashSet<string> WearableWithoutWornSprite = [];

    // 暂存的脚本映射（modId → itemId → (itemDef, modDir)），待引擎创建后注册
    private static readonly Dictionary<string, Dictionary<string, (ItemDef def, string modDir)>> PendingScripts =
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

        foreach (var (itemId, (itemDef, modDir)) in itemScripts)
            ItemScriptRegistry.Register(itemId, itemDef, manifest.Engine, manifest.Id, modDir);

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
            var ok = LoadLiquidItem(json, itemId, assetsDir, modId, modDir, jsonFile);
            return ok ? new ItemEntry(itemId, "liquid-item") : null;
        }

        if (obj["color"] != null && obj["weight"] == null)
        {
            var ok = LoadLiquid(json, itemId, modId);
            return ok ? new ItemEntry(itemId, "liquid") : null;
        }

        var ok2 = LoadItem(json, itemId, assetsDir, modId, modDir, jsonFile);
        return ok2 ? new ItemEntry(itemId, "item") : null;
    }

    // ---- 普通物品 ----

    private static bool LoadItem(string json, string itemId, string assetsDir,
        string modId, string modDir, string jsonFilePath)
    {
        var def = ParseItemDef(json, out var wasLegacy);
        if (def is null)
            return false;

        var info = BuildItemInfo(def, itemId, assetsDir);
        var sprite = LoadItemSprite(def.OriginPrefab, itemId, assetsDir, def.SpriteDef?.ImportScale ?? 6f);

        ItemRegistry.Register(itemId, info, sprite);

        // 暂存脚本映射（如有），待引擎就绪后由 RegisterScripts 写入 ItemScriptRegistry
        StashScript(itemId, def, modId, modDir);

        // 旧格式自动迁移为新格式并覆写 JSON
        if (wasLegacy)
            MigrateJsonToNewFormat(jsonFilePath, def);

        LogUtil.Info("items.item_registered", itemId, modId);
        return true;
    }

    // ---- 格式检测与转换 ----

    // 检测 JSON 并返回 ItemDef（支持新旧格式），wasLegacy 表示是否从旧格式转换而来
    private static ItemDef? ParseItemDef(string json, out bool wasLegacy)
    {
        wasLegacy = false;
        var obj = JObject.Parse(json);

        // 新版格式：wearable 是对象、或 battery/container 无 _data 后缀
        if (IsNewFormat(obj))
        {
            var def = obj.ToObject<ItemDef>();
            if (def != null)
                return def;
        }

        // 旧版格式：flat 字段 + _data 后缀
        wasLegacy = true;
        var legacy = obj.ToObject<LegacyItemDef>();

        return legacy?.ToItemDef();
    }

    // 将旧格式 ItemDef 序列化为新格式 JSON 并覆写文件
    private static void MigrateJsonToNewFormat(string jsonFilePath, object def)
    {
        try
        {
            var newJson = JsonConvert.SerializeObject(def, Formatting.Indented);
            // 备份原文件为 .backup
            var backupPath = jsonFilePath + ".backup";
            if (!File.Exists(backupPath))
                File.Copy(jsonFilePath, backupPath);
            File.WriteAllText(jsonFilePath, newJson);
            LogUtil.Info("items.format_migrated", Path.GetFileName(jsonFilePath));
        }
        catch (Exception ex)
        {
            LogUtil.Warning("items.format_migrate_failed", jsonFilePath, ex.Message);
        }
    }

    // 判断 JSON 是否使用新版分组格式
    private static bool IsNewFormat(JObject obj)
    {
        // wearable 是对象（非 bool）→ 新版
        if (obj["wearable"] is JObject)
            return true;
        // battery 代替 battery_data → 新版
        if (obj["battery"] is JObject)
            return true;
        // container 代替 container_data → 新版
        if (obj["container"] is JObject)
            return true;
        // sprite 是对象 → 新版（旧版 sprite 字段均为顶层 flat）
        if (obj["sprite"] is JObject)
            return true;
        // decay 是对象 → 新版
        if (obj["decay"] is JObject)
            return true;
        return false;
    }

    // ---- 液体容器 ----

    private static bool LoadLiquidItem(string json, string itemId, string assetsDir,
        string modId, string modDir, string jsonFilePath)
    {
        var obj = JObject.Parse(json);
        LiquidItemDef? def;
        var wasLegacy = false;

        if (IsNewFormat(obj))
        {
            def = obj.ToObject<LiquidItemDef>();
        }
        else
        {
            wasLegacy = true;
            var legacy = obj.ToObject<LegacyLiquidItemDef>();
            def = legacy?.ToLiquidItemDef();
        }

        if (def is null)
            return false;

        var info = BuildLiquidItemInfo(def, itemId, assetsDir);
        var sprite = LoadItemSprite(def.OriginPrefab, itemId, assetsDir, def.SpriteDef?.ImportScale ?? 6f);

        ItemRegistry.Register(itemId, info, sprite);

        // 暂存脚本映射
        StashScript(itemId, def, modId, modDir);

        // 旧格式自动迁移为新格式并覆写 JSON
        if (wasLegacy)
            MigrateJsonToNewFormat(jsonFilePath, def);

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
        var isWearable = def.Wearable != null;
        var w = def.Wearable;
        var spriteDef = def.SpriteDef ?? new SpriteDef();
        var decay = def.Decay ?? new DecayDef();
        var spawn = def.Spawn ?? new SpawnDef();

        var info = new CustomItemInfo
        {
            fullName = def.FullName,
            description = def.Description,
            category = def.Category,
            slotRotation = spriteDef.SlotRotation,
            destroyAtZeroCondition = def.DestroyAtZeroCondition,
            weight = def.Weight,
            onlyHoldInHands = def.OnlyHoldInHands,
            wearable = isWearable,
            wearableCanBeHeld = w?.CanBeHeld ?? false,
            wearableArmor = w?.Armor ?? 0f,
            wearableIsolation = w?.Isolation ?? 0f,
            desiredWearLimb = w?.DesiredLimb ?? string.Empty,
            wearSlotId = w?.SlotId ?? string.Empty,
            wearableHitDurabilityLossMultiplier = w?.HitDurabilityLossMultiplier ?? 0f,
            scaleWeightWithCondition = def.ScaleWeightWithCondition,
            WearableSortingOrder = w?.SortingOrder,
            combineable = def.Combinable,
            ignoreDepression = def.IgnoreDepression,
            value = def.Value,
            wearableVisualOffset = w?.VisualOffset ?? 5,
            tags = def.Tags,
            decayInfo = decay.Info,
            decayMinutes = decay.Minutes,
            rec = new Recognition(def.Recognition),
            SpawnFrequency = spawn.Frequency,
            WorldSpawnPerChunk = spawn.WorldPerChunk,
            SpriteScale = spriteDef.Scale,
            InventoryIconScale = spriteDef.InventoryIconScale
        };

        // Sprite 缩放维度：优先用 JSON 配置，未配置则回退到 prefab 精灵尺寸
        if (spriteDef.ScaleDimensions is { Width: > 0f, Height: > 0f })
        {
            var ssd = spriteDef.ScaleDimensions;
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

        // 穿戴贴图: Assets/Item/{itemId}_worn.png，缺失则回退到主贴图
        var importScale = spriteDef.ImportScale;
        info.WornSprite = ItemUtil.LoadSprite(Path.Combine(assetsDir, itemId + "_worn.png"), importScale);
        if (isWearable && info.WornSprite == null)
        {
            // 回退：使用主贴图作为穿戴贴图
            info.WornSprite = ItemUtil.LoadSprite(Path.Combine(assetsDir, itemId + ".png"), importScale);
            if (info.WornSprite == null)
            {
                // 两者都不存在 → 加入黑名单，阻止装备
                WearableWithoutWornSprite.Add(itemId);
                LogUtil.Warning("item_loader.wearable_no_worn_sprite",
                    def.FullName,
                    $"Assets/Item/{itemId}_worn.png",
                    $"Assets/Item/{itemId}.png");
            }
        }

        info.WornSpriteOffset = new Vector2(w?.SpriteOffsetX ?? 0f, w?.SpriteOffsetY ?? 0f);

        // MultiWorn: Assets/Item/{itemId}_mw_{key}.png
        if (w?.Multi != null)
            foreach (var kv in w.Multi)
            {
                var multiSprite = ItemUtil.LoadSprite(Path.Combine(assetsDir, itemId + "_mw_" + kv.Key + ".png"),
                    importScale);
                if (multiSprite == null) continue;
                info.MultiWornSprites[kv.Key] = multiSprite;
                info.MultiWornSpriteOffsets[kv.Key] = new Vector2(
                    kv.Value.SpriteOffsetX, kv.Value.SpriteOffsetY);
            }

        // DropPool
        if (spawn.DropPool is { Length: > 0 })
            try
            {
                info.DropPool = (DropPool)Enum.Parse(typeof(DropPool), string.Join(",", spawn.DropPool));
            }
            catch
            {
                // 无效的 DropPool 值忽略
            }

        // 腐烂速度
        if (decay.RotSpeed.HasValue)
            info.rotSpeed = decay.RotSpeed.Value;
        else if (decay.Minutes > 0)
            info.rotSpeed = 1f / decay.Minutes;

        // 制作特性
        if (def.Qualities != null)
            info.qualities =
            [
                .. def.Qualities
                    .Where(q => !string.IsNullOrEmpty(q.Id))
                    .Select(q => new CraftingQuality(q.Id.ToLowerInvariant(), q.Amount))
            ];

        // 容器
        if (def.Container != null)
        {
            var cd = def.Container;
            info.Container = new ContainerProperties
            {
                Capacity = cd.MaxWeight,
                MaxWeightPerItem = cd.MaxWeightPerItem,
                ItemsVisible = cd.ItemsVisible,
                TagRestriction = cd.TagRestriction,
                EncumbranceReduction = cd.EncumbranceMult
            };
        }

        // 电池
        if (def.Battery == null) return FinalizeItemInfo(info, def);
        var bd = def.Battery;
        info.Battery = new BatteryProperties
        {
            SpawnWithBattery = bd.SpawnWithBattery
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

        return FinalizeItemInfo(info, def);
    }

    // 根据脚本配置自动推断 usable / usableOnLimb，wearable 与 use 互斥
    private static CustomItemInfo FinalizeItemInfo(CustomItemInfo info, ItemDef def)
    {
        // use 字段非空 → usable
        var hasUse = def.Use is { Count: > 0 } list
            && list.Any(e => e.Script.Count > 0);

        // use_on_limb 仍在 script 内
        var hasUseOnLimb = def.Script?.UseOnLimb is { Count: > 0 };

        if (def.Wearable != null)
        {
            // wearable 优先，忽略 use
            info.wearable = true;
            if (hasUse)
                LogUtil.Warning("items.use_wearable_conflict", def.FullName);
        }
        else
        {
            if (hasUse)
                info.usable = true;
        }

        if (hasUseOnLimb)
            info.usableOnLimb = true;

        // 校验 wearable 字段
        if (def.Wearable == null) return info;

        // wear_slot_id 是装备槽位标识（如 "back", "head"），为空则无法装备
        if (string.IsNullOrEmpty(def.Wearable.SlotId))
        {
            LogUtil.Warning("item_event.wear_slot_invalid",
                "<Null>",
                def.FullName);
            info.wearable = false;
            LogUtil.Warning("items.wearable_disabled", def.FullName);
        }

        // desired_wear_limb 是 CCL 穿戴贴图的目标肢体，为空则无法装备
        if (string.IsNullOrEmpty(def.Wearable.DesiredLimb))
        {
            LogUtil.Warning("item_event.wear_slot_invalid",
                "<Null>",
                def.FullName);
            info.wearable = false;
            LogUtil.Warning("items.wearable_disabled", def.FullName);
        }
        else if (!LimbUtil.IsValidLimbName(def.Wearable.DesiredLimb))
        {
            LogUtil.Warning("item_event.wear_slot_invalid",
                def.Wearable.DesiredLimb,
                def.FullName);
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
        var importScale = def.SpriteDef?.ImportScale ?? 6f;
        info.LiquidMask = ItemUtil.LoadSprite(Path.Combine(assetsDir, itemId + "_fill.png"), importScale);

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
            localeFromItem = def.LocaleFromItem
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
        // 清除该 owner 在 WearableWithoutWornSprite 中的条目
        if (LoadedItems.TryGetValue(ownerId, out var items))
            foreach (var entry in items)
                WearableWithoutWornSprite.Remove(entry.Id);

        s_clearItemOwnerEntries?.Invoke(null, [ownerId, null!]);
        s_clearLiquidOwnerEntries?.Invoke(null, [ownerId, null!]);
        LoadedItems.Remove(ownerId);
        PendingScripts.Remove(ownerId);
    }

    // 暂存物品脚本映射，待引擎就绪后注册。检查 ItemDef 所有脚本来源。
    private static void StashScript(string itemId, ItemDef def, string modId, string modDir)
    {
        if (def is null) return;

        // 检查是否有任何脚本需要暂存（复用 ItemScriptRegistry.IsEmpty 逻辑较复杂，
        // 但这里 ItemScriptRegistry.Register 会内部判断；只要 def 有脚本相关字段就暂存）
        var hasScript = (def.Script != null && (
            def.Script.Attack.Count > 0 ||
            def.Script.UseOnLimb.Count > 0 ||
            def.Script.InBackpack.Count > 0 ||
            def.Script.InHand.Count > 0 ||
            def.Script.NotInHand.Count > 0 ||
            def.Script.Durability.Count > 0));

        var hasUse = def.Use is { Count: > 0 } ul && ul.Any(e => e.Script.Count > 0);

        var hasWearableScripts = def.Wearable != null && (
            def.Wearable.Equip.Count > 0 ||
            def.Wearable.Unequip.Count > 0 ||
            def.Wearable.Attack.Count > 0 ||
            def.Wearable.Damage.Count > 0);

        var hasContainerTrigger = def.Container?.CapacityTrigger is { Count: > 0 } ct &&
            ct.Any(t => t.Script.Count > 0);

        var hasBatteryTrigger = def.Battery?.ChargeTrigger is { Count: > 0 } bt &&
            bt.Any(t => t.Script.Count > 0);

        if (!hasScript && !hasUse && !hasWearableScripts && !hasContainerTrigger && !hasBatteryTrigger)
            return;

        if (!PendingScripts.TryGetValue(modId, out var itemScripts))
        {
            itemScripts = new Dictionary<string, (ItemDef, string)>();
            PendingScripts[modId] = itemScripts;
        }

        itemScripts[itemId] = (def, modDir);
    }
}