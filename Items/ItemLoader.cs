using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bark.Items.Templates;
using Bark.Liquid;
using Bark.Compat.Wmitf;
using Bark.Script;
using Bark.Tool;
using CUCoreLib.Data;
using CUCoreLib.Registries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using GunRuntimeManager = Bark.Items.Runtime.Gun.GunRuntimeManager;

namespace Bark.Items;

public class ItemEntry(string id)
{
    public string Id = id;
}

public static class ItemLoader
{
    public static readonly Dictionary<string, List<ItemEntry>> LoadedItems = new();
    public static readonly HashSet<string> WearableWithoutWornSprite = [];
    public static readonly Dictionary<string, float> LightRotations = new();

    private static readonly
        Dictionary<string, Dictionary<string, (ItemDef def, string modDir)>>
        PendingScripts =
            new();

    private static readonly MethodInfo? s_clearItemOwnerEntries =
        typeof(ItemRegistry).GetMethod(
            "ClearOwnerEntries", BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly MethodInfo? s_clearLiquidOwnerEntries =
        typeof(LiquidRegistry).GetMethod(
            "ClearOwnerEntries", BindingFlags.NonPublic | BindingFlags.Static);

    public static void RegisterFromMod(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        RegisterFromDirectory(manifest.Id, manifest.Directory);
    }

    public static int RegisterFromDirectory(string modId, string modDir,
        bool allowPendingScripts = true)
    {
        if (modId is null) throw new ArgumentNullException(nameof(modId));
        if (modDir is null) throw new ArgumentNullException(nameof(modDir));

        if (string.IsNullOrWhiteSpace(modId))
            throw new ArgumentException("modId must not be empty", nameof(modId));

        ClearModItems(modId);

        var itemsDir = Path.Combine(modDir, "Item");
        if (!Directory.Exists(itemsDir))
            return 0;

        var jsonFiles =
            Directory.GetFiles(itemsDir, "*.json", SearchOption.AllDirectories);
        if (jsonFiles.Length == 0)
            return 0;

        var assetsItemDir = Path.Combine(modDir, "Assets", "Item");

        var loadedList = new List<ItemEntry>();
        var loadedCount = 0;

        using (ItemRegistry.BeginOwnerRegistration(modId))
        using (LiquidRegistry.BeginOwnerRegistration(modId))
        {
            foreach (var jsonFile in jsonFiles)
                try
                {
                    var entry = LoadAndRegister(jsonFile, assetsItemDir, modId, modDir);
                    if (entry == null) continue;
                    loadedCount++;
                    loadedList.Add(entry);
                }
                catch (Exception ex)
                {
                    LogUtil.Error("items.load_error", jsonFile, modId, ex.Message);
                }
        }

        LoadedItems[modId] = loadedList;

        foreach (var entry in loadedList)
            WmitfPatch.RegisterItem(entry.Id, modId);

        if (PendingScripts.TryGetValue(modId, out var existing) && existing.Count > 0)
        {
            if (allowPendingScripts)
            {
                LogUtil.Info("items.scripts_pending", modId, existing.Count);
            }
            else
            {
                LogUtil.Warning("items.csharp.scripts_ignored", modId, existing.Count);
                PendingScripts.Remove(modId);
            }
        }

        if (loadedCount > 0)
            LogUtil.Info("items.loaded_count", modId, loadedCount);

        GunRuntimeManager.RefreshAllBarrelOffsets();

        return loadedCount;
    }

    public static void UnregisterOwner(string modId)
    {
        if (modId is null) throw new ArgumentNullException(nameof(modId));
        ClearModItems(modId);
    }

    public static void RegisterScripts(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));
        if (manifest.Engine is null)
            return;

        if (!PendingScripts.TryGetValue(manifest.Id, out var itemScripts)
            || itemScripts.Count == 0)
            return;

        foreach (var (itemId, (itemDef, modDir)) in itemScripts)
            ItemScriptRegistry.Register(itemId, itemDef, manifest.Engine, manifest.Id,
                modDir);

        var count = itemScripts.Count;
        PendingScripts.Remove(manifest.Id);
        LogUtil.Info("items.scripts_registered", manifest.Id, count);
    }

    private static ItemEntry? LoadAndRegister(string jsonFile, string assetsDir,
        string modId, string modDir)
    {
        var itemId = $"{modId}.{Path.GetFileNameWithoutExtension(jsonFile)}";

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

        var templateNode = obj["template"];
        if (templateNode is JObject templateObj)
        {
            var merged = TemplateLoader.ResolveAndMerge(obj, templateObj);
            if (merged != null)
            {
                var template = merged["template"] as JObject;
                GunTemplate.CacheGunItem(itemId, template, modDir);
                MagTemplate.CacheMagItem(itemId, template);
                AmmunitionTemplate.CacheAmmoItem(itemId, template);
                CasingTemplate.CacheCasingItem(itemId, template);
                FoodTemplate.CacheFoodItem(itemId, template);
                ClothingTemplate.CacheClothingItem(itemId, template);
                PlushTemplate.CachePlushItem(itemId, template, modDir);

                json = merged.ToString();
                obj = merged;
            }
        }

        if (obj["capacity"] != null)
        {
            var ok = LoadLiquidItem(json, itemId, assetsDir, modId, modDir, jsonFile);
            return ok
                ? new ItemEntry(itemId)
                : null;
        }

        if (obj["color"] != null && obj["weight"] == null)
        {
            var ok = LoadLiquid(json, itemId, modId);
            return ok
                ? new ItemEntry(itemId)
                : null;
        }

        var ok2 = LoadItem(json, itemId, assetsDir, modId, modDir, jsonFile);
        return ok2
            ? new ItemEntry(itemId)
            : null;
    }

    private static bool LoadItem(string json, string itemId, string assetsDir,
        string modId, string modDir, string jsonFilePath)
    {
        var def = ParseItemDef(json, out var wasLegacy);
        if (def is null)
            return false;

        var info = BuildItemInfo(def, itemId, assetsDir);
        var sprite = LoadItemSprite(def.OriginPrefab, itemId, assetsDir,
            def.SpriteDef?.ImportScale ?? 6f);

        ItemRegistry.Register(itemId, info, sprite);

        StashScript(itemId, def, modId, modDir);

        if (wasLegacy)
            MigrateJsonToNewFormat(jsonFilePath, def);

        LogUtil.Info("items.item_registered", itemId, modId);
        return true;
    }

    private static ItemDef? ParseItemDef(string json, out bool wasLegacy)
    {
        wasLegacy = false;
        var obj = JObject.Parse(json);

        if (IsNewFormat(obj))
        {
            var def = obj.ToObject<ItemDef>();
            if (def != null)
                return def;
        }

        wasLegacy = true;
        var legacy = obj.ToObject<LegacyItemDef>();

        return legacy?.ToItemDef();
    }

    private static void MigrateJsonToNewFormat(string jsonFilePath, object def)
    {
        try
        {
            var newJson = JsonConvert.SerializeObject(def, Formatting.Indented);
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

    private static bool IsNewFormat(JObject obj)
    {
        if (obj["wearable"] is JObject)
            return true;
        if (obj["battery"] is JObject)
            return true;
        if (obj["container"] is JObject)
            return true;
        if (obj["sprite"] is JObject)
            return true;
        if (obj["decay"] is JObject)
            return true;
        if (obj["template"] is JObject)
            return true;
        return false;
    }

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
        var sprite = LoadItemSprite(def.OriginPrefab, itemId, assetsDir,
            def.SpriteDef?.ImportScale ?? 6f);

        ItemRegistry.Register(itemId, info, sprite);

        StashScript(itemId, def, modId, modDir);

        if (wasLegacy)
            MigrateJsonToNewFormat(jsonFilePath, def);

        LogUtil.Info("items.item_registered", itemId, modId);
        return true;
    }

    private static bool LoadLiquid(string json, string liquidId, string modId)
    {
        var def = JsonConvert.DeserializeObject<LiquidDef>(json);
        if (def is null)
            return false;

        var info = BuildLiquidInfo(def, liquidId);
        LiquidRegistry.Register(liquidId, info);
        WmitfPatch.RegisterLiquid(liquidId, modId);
        LogUtil.Info("items.liquid_registered", liquidId, modId);
        return true;
    }

    private static CustomItemInfo BuildItemInfo(ItemDef def, string itemId,
        string assetsDir)
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
            InventoryIconScale = spriteDef.InventoryIconScale,
            CustomData = def.CustomData ?? new Dictionary<string, object>()
        };

        if (spriteDef.ScaleDimensions is { Width: > 0f, Height: > 0f })
        {
            var ssd = spriteDef.ScaleDimensions;
            info.SpriteScaleDimensions =
                new SpriteScaleDimensions(ssd.Width, ssd.Height, ssd.ExpandToFirstMet);
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
                            new SpriteScaleDimensions(prefabSprite.rect.width,
                                prefabSprite.rect.height, true);
                }
            }
            catch
            {
                // ignored
            }
        }

        var importScale = spriteDef.ImportScale;
        info.WornSprite =
            LoadSpriteWithFallback(assetsDir, itemId, "_worn.png", importScale);
        if (isWearable && info.WornSprite == null)
        {
            info.WornSprite =
                LoadSpriteWithFallback(assetsDir, itemId, ".png", importScale);
            if (info.WornSprite == null)
            {
                WearableWithoutWornSprite.Add(itemId);
                LogUtil.Warning("item_loader.wearable_no_worn_sprite",
                    def.FullName,
                    $"Assets/Item/{itemId}_worn.png",
                    $"Assets/Item/{itemId}.png");
            }
        }

        info.WornSpriteOffset =
            new Vector2(w?.SpriteOffsetX ?? 0f, w?.SpriteOffsetY ?? 0f);

        if (w?.Multi != null)
            foreach (var kv in w.Multi)
            {
                var multiSprite = LoadSpriteWithFallback(assetsDir, itemId,
                    "_mw_" + kv.Key + ".png",
                    importScale);
                if (multiSprite == null) continue;
                info.MultiWornSprites[kv.Key] = multiSprite;
                info.MultiWornSpriteOffsets[kv.Key] = new Vector2(
                    kv.Value.SpriteOffsetX, kv.Value.SpriteOffsetY);
            }

        if (spawn.DropPool is { Length: > 0 })
            try
            {
                info.DropPool = (DropPool)Enum.Parse(typeof(DropPool),
                    string.Join(",", spawn.DropPool));
            }
            catch
            {
                // ignored
            }

        if (decay.RotSpeed.HasValue)
            info.rotSpeed = decay.RotSpeed.Value;
        else if (decay.Minutes > 0)
            info.rotSpeed = 1f / decay.Minutes;

        if (def.Qualities != null)
            info.qualities =
            [
                .. def.Qualities
                    .Where(q => !string.IsNullOrEmpty(q.Id))
                    .Select(q => new CraftingQuality(q.Id.ToLowerInvariant(), q.Amount))
            ];

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

        if (def.Battery != null)
        {
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
        }

        if (!string.IsNullOrEmpty(def.IconAnimationId))
            info.IconAnimationId = def.IconAnimationId;
        if (!string.IsNullOrEmpty(def.WornSpriteAnimationId))
            info.WornSpriteAnimationId = def.WornSpriteAnimationId;

        if (def.HeldSpriteOffset != null)
        {
            var hs = def.HeldSpriteOffset;
            info.HeldSpriteOffset = new Vector2(hs.X, hs.Y);
        }

        if (def.Light != null)
        {
            var ld = def.Light;
            info.Light = new LightProperties
            {
                Intensity = ld.Intensity,
                Color = ItemUtil.HexToColor(ld.Color),
                Offset = new Vector2(ld.XOffset, ld.YOffset),
                PointLightInnerAngle = ld.PointLightInnerAngle,
                PointLightInnerRadius = ld.PointLightInnerRadius,
                PointLightOuterAngle = ld.PointLightOuterAngle,
                PointLightOuterRadius = ld.PointLightOuterRadius
            };

            if (Enum.TryParse<CustomLightType>(ld.LightType, true, out var lightType))
                info.Light.LightType = lightType;

            LightRotations[itemId] = ld.Rotation;
        }

        if (def.Bandage != null)
        {
            var bd = def.Bandage;
            info.Bandage = new BandageProperties
            {
                Effectiveness = bd.Effectiveness,
                SkinHealAmount = bd.SkinHealAmount,
                BandageSlowAmount = bd.BandageSlowAmount,
                PainReduction = bd.PainReduction,
                BoneHealTimerReduction = bd.BoneHealTimerReduction,
                DislocationTimerReduction = bd.DislocationTimerReduction,
                CreateWrapSprite = bd.CreateWrapSprite,
                WrapSpritePath = bd.WrapSpritePath,
                WrapSpriteColor = ItemUtil.HexToColor(bd.WrapSpriteColor),
                MinigameColor = ItemUtil.HexToColor(bd.MinigameColor)
            };
        }

        if (def.Syringe != null)
        {
            var sd = def.Syringe;
            info.Syringe = new SyringeProperties
            {
                Capacity = sd.Capacity,
                AutoFill = sd.AutoFill,
                AmountPerFullUse = sd.AmountPerFullUse,
                UseAverageColor = sd.UseAverageColor,
                MinigameColor = ItemUtil.HexToColor(sd.MinigameColor)
            };
        }

        if (def.Tool != null)
        {
            var td = def.Tool;
            info.Tool = new ToolProperties
            {
                Damage = td.Damage,
                StructuralDamage = td.StructuralDamage,
                AttackCooldownMultiplier = td.AttackCooldownMultiplier,
                Distance = td.Distance,
                KnockBack = td.KnockBack,
                Cooldown = td.Cooldown,
                AttackAnimation = td.AttackAnimation,
                StaminaUse = td.StaminaUse,
                Piercing = td.Piercing,
                SwingSounds = td.SwingSounds,
                Volume = td.Volume,
                RotateAmount = td.RotateAmount,
                PhysicalSwing = td.PhysicalSwing,
                DoAttackAnimation = td.DoAttackAnimation,
                MetalMoreDamage = td.MetalMoreDamage,
                ConditionLossOnHit = td.ConditionLossOnHit
            };
        }

        if (def.SpawnComponents is { Count: > 0 })
            info.SpawnComponents = [.. def.SpawnComponents];

        if (GunTemplate.IsGun(itemId))
        {
            info.usable = true;
            info.usableWithLMB = true;
            info.autoAttack = true;
            info.tags = string.IsNullOrEmpty(def.Tags)
                ? "gun"
                : def.Tags.Contains("gun")
                    ? def.Tags
                    : def.Tags + ",gun";
            info.useAction = (_, item) =>
            {
                var gs = item.GetComponent<GunScript>();
                if (gs == null) return;
                gs.triggerPressed = true;
            };
        }
        else if (MagTemplate.IsMag(itemId))
        {
            info.usable = true;
            info.useAction = (_, item) =>
            {
                var am = item.GetComponent<AmmoScript>();
                if (am == null) return;
                am.UnloadRound();
            };
        }
        else if (FoodTemplate.IsFood(itemId))
        {
            info.usable = true;
            info.useAction = (body, item) =>
            {
                var foodData = FoodTemplate.GetFoodData(itemId);
                if (foodData is null) return;

                body.Eat(foodData.Nutrition, foodData.WeightOffset);
                body.Drink(foodData.Hydration);
                body.happiness += foodData.Happiness;
                item.condition -= foodData.ConditionLoss;

                if (!string.IsNullOrEmpty(foodData.EatSound))
                    Sound.Play(foodData.EatSound, body.transform.position);
                if (foodData.EatGoodVoice)
                    body.talker.EatGood();
            };
        }

        return FinalizeItemInfo(info, def);
    }

    private static CustomItemInfo FinalizeItemInfo(CustomItemInfo info, ItemDef def)
    {
        var hasUse = def.Use is { Count: > 0 } list
            && list.Any(e => e.Script.Count > 0);

        var hasUseOnLimb = def.Script?.UseOnLimb is { Count: > 0 };

        if (def.Wearable != null)
        {
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

        if (def.Wearable == null) return info;

        if (string.IsNullOrEmpty(def.Wearable.SlotId))
        {
            LogUtil.Warning("item_event.wear_slot_invalid",
                "<Null>",
                def.FullName);
            info.wearable = false;
            LogUtil.Warning("items.wearable_disabled", def.FullName);
        }

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

    private static CustomItemInfo BuildLiquidItemInfo(LiquidItemDef def, string itemId,
        string assetsDir)
    {
        var info = BuildItemInfo(def, itemId, assetsDir);

        info.capacity = def.Capacity;
        info.autoFill = def.AutoFill;

        var importScale = def.SpriteDef?.ImportScale ?? 6f;
        info.LiquidMask =
            LoadSpriteWithFallback(assetsDir, itemId, "_fill.png", importScale);

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

        if (def.Qualities == null) return info;
        foreach (var kv in def.Qualities)
            info.qualities.Add(new CraftingQuality(kv.Key.ToLowerInvariant(), kv.Value));

        return info;
    }

    private static Sprite? LoadSpriteWithFallback(string assetsDir, string itemId,
        string suffix, float importScale)
    {
        var path = Path.Combine(assetsDir, itemId + suffix);
        var sprite = ItemUtil.LoadSprite(path, importScale);
        if (sprite != null) return sprite;

        var dotIndex = itemId.IndexOf('.');
        if (dotIndex > 0 && dotIndex < itemId.Length - 1)
            sprite = ItemUtil.LoadSprite(
                Path.Combine(assetsDir, itemId[(dotIndex + 1)..] + suffix), importScale);

        return sprite;
    }

    private static Sprite? LoadItemSprite(string originPrefab, string itemId,
        string assetsDir, float importScale = 1f)
    {
        var sprite = LoadSpriteWithFallback(assetsDir, itemId, ".png", importScale);
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
            // ignored
        }

        return sprite;
    }

    private static void ClearModItems(string ownerId)
    {
        if (LoadedItems.TryGetValue(ownerId, out var items))
            foreach (var entry in items)
            {
                WearableWithoutWornSprite.Remove(entry.Id);
                GunTemplate.RemoveGunItem(entry.Id);
                MagTemplate.RemoveMagItem(entry.Id);
                AmmunitionTemplate.RemoveAmmoItem(entry.Id);
                CasingTemplate.RemoveCasingItem(entry.Id);
                FoodTemplate.RemoveFoodItem(entry.Id);
                ClothingTemplate.RemoveClothingItem(entry.Id);
                PlushTemplate.RemovePlushItem(entry.Id);
                LightRotations.Remove(entry.Id);
            }

        s_clearItemOwnerEntries?.Invoke(null, [ownerId, null!]);
        s_clearLiquidOwnerEntries?.Invoke(null, [ownerId, null!]);
        LoadedItems.Remove(ownerId);
        PendingScripts.Remove(ownerId);
    }

    private static void StashScript(string itemId, ItemDef def, string modId,
        string modDir)
    {
        var hasScript = def.Script != null
            && (
                def.Script.Attack.Count > 0
                || def.Script.UseOnLimb.Count > 0
                || def.Script.Has.Count > 0
                || def.Script.InHand.Count > 0
                || def.Script.NotInHand.Count > 0
                || def.Script.Durability.Count > 0);

        var hasUse = def.Use is { Count: > 0 } ul && ul.Any(e => e.Script.Count > 0);

        var hasWearableScripts = def.Wearable != null
            && (
                def.Wearable.Equip.Count > 0
                || def.Wearable.Unequip.Count > 0
                || def.Wearable.Attack.Count > 0
                || def.Wearable.Damage.Count > 0
                || def.Wearable.Wearing.Count > 0);

        var hasContainerTrigger = def.Container?.CapacityTrigger is { Count: > 0 } ct
            && ct.Any(t => t.Script.Count > 0);

        var hasBatteryTrigger = def.Battery?.ChargeTrigger is { Count: > 0 } bt
            && bt.Any(t => t.Script.Count > 0);

        if (!hasScript
            && !hasUse
            && !hasWearableScripts
            && !hasContainerTrigger
            && !hasBatteryTrigger)
            return;

        if (!PendingScripts.TryGetValue(modId, out var itemScripts))
        {
            itemScripts = new Dictionary<string, (ItemDef, string)>();
            PendingScripts[modId] = itemScripts;
        }

        itemScripts[itemId] = (def, modDir);
    }
}
