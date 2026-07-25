using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Items;

// 普通物品 JSON 数据模型。字段与 CUCoreLib CustomItemInfo 一一对应，
// JSON 字段一律使用 snake_case。

public class ItemDef
{
    // 物品 ID 由 JSON 文件名决定（如 bandage.json → id = "bandage"），
    // 不在 JSON 内指定 id 字段。
    // ---- ItemInfo 基础字段 ----
    [JsonProperty("origin_prefab")]
    public string OriginPrefab = "geofruit";

    // 精灵图导入放大倍数，默认 6.0
    [JsonProperty("sprite_import_scale")]
    public float SpriteImportScale = 6f;

    [JsonProperty("sprite_scale")]
    public float SpriteScale;

    [JsonProperty("inventory_icon_scale")]
    public float InventoryIconScale = 4f;

    // 物品栏精灵目标缩放尺寸，宽高是像素数，expand_to_first_met 碰边即停。
    // 不写则回退到 origin_prefab 的精灵尺寸，可能导致自定义精灵图显示过小。
    [JsonProperty("sprite_scale_dimensions")]
    public SpriteScaleDimensionsDef? SpriteScaleDimensions;

    [JsonProperty("full_name")]
    public string FullName = string.Empty;

    [JsonProperty("description")]
    public string Description = string.Empty;

    [JsonProperty("category")]
    public string Category = string.Empty;

    [JsonProperty("slot_rotation")]
    public float SlotRotation;

    [JsonProperty("rot_speed")]
    public float? RotSpeed;

    [JsonProperty("destroy_at_zero_condition")]
    public bool DestroyAtZeroCondition = true;

    [JsonProperty("weight")]
    public float Weight;

    [JsonProperty("scale_weight_with_condition")]
    public bool ScaleWeightWithCondition;

    [JsonProperty("only_hold_in_hands")]
    public bool OnlyHoldInHands;

    [JsonProperty("combinable")]
    public bool Combinable;

    [JsonProperty("ignore_depression")]
    public bool IgnoreDepression;

    [JsonProperty("value")]
    public int Value;

    [JsonProperty("tags")]
    public string Tags = string.Empty;

    [JsonProperty("recognition")]
    public int Recognition;

    [JsonProperty("decay_minutes")]
    public float DecayMinutes;

    [JsonProperty("decay_info")]
    public byte DecayInfo;

    [JsonProperty("qualities")]
    public List<QualitiesDef>? Qualities;

    // ---- 可装备字段 ----
    [JsonProperty("wearable")]
    public bool Wearable;

    [JsonProperty("wearable_can_be_held")]
    public bool WearableCanBeHeld;

    [JsonProperty("desired_wear_limb")]
    public string DesiredWearLimb = string.Empty;

    [JsonProperty("wear_slot_id")]
    public string WearSlotId = string.Empty;

    [JsonProperty("wearable_armor")]
    public float WearableArmor;

    [JsonProperty("wearable_isolation")]
    public float WearableIsolation;

    [JsonProperty("wearable_hit_durability_loss_multiplier")]
    public float WearableHitDurabilityLossMultiplier;

    [JsonProperty("wearable_visual_offset")]
    public int WearableVisualOffset = 5;

    [JsonProperty("wearable_sorting_order")]
    public int? WearableSortingOrder;

    [JsonProperty("worn_sprite_offset_x")]
    public float WornSpriteOffsetX;

    [JsonProperty("worn_sprite_offset_y")]
    public float WornSpriteOffsetY;

    // extra limb → offset
    [JsonProperty("multi_worn")]
    public Dictionary<string, WornSpriteOffset>? MultiWorn;

    // ---- 容器字段 ----
    [JsonProperty("container_data")]
    public ItemContainerDef? ContainerData;

    // ---- 电池字段 ----
    [JsonProperty("battery_data")]
    public ItemBatteryDef? BatteryData;

    // ---- 生成字段 ----
    [JsonProperty("drop_pool")]
    public string[]? DropPool;

    [JsonProperty("spawn_frequency")]
    public int SpawnFrequency;

    [JsonProperty("world_spawn_per_chunk")]
    public float? WorldSpawnPerChunk;

    // ---- 自定义脚本触发 ----
    // 物品动作 → 脚本文件列表（路径相对于模组目录）
    // 支持的动作键：use / equip / unequip / use_on_limb
    [JsonProperty("script")]
    public ItemScriptDef? Script;

    [JsonProperty("custom_data")]
    public Dictionary<string, object>? customData;
}

// 额外部位已装备贴图偏移
public class WornSpriteOffset
{
    [JsonProperty("worn_sprite_offset_x")]
    public float WornSpriteOffsetX;

    [JsonProperty("worn_sprite_offset_y")]
    public float WornSpriteOffsetY;
}
// 电池/电力物品配置
public class ItemBatteryDef
{
    [JsonProperty("max_allowed_charge")]
    public float MaxAllowedCharge;

    [JsonProperty("start_charge")]
    public float StartCharge;

    [JsonProperty("preset")]
    public string Preset = string.Empty;

    [JsonProperty("battery_type")]
    public string BatteryType = string.Empty;

    [JsonProperty("spawn_with_battery")]
    public bool SpawnWithBattery = true;

    [JsonProperty("weight_reduction")]
    public bool WeightReduction;

    [JsonProperty("explode_at_zero")]
    public bool ExplodeAtZero;
}

// 容器物品配置
public class ItemContainerDef
{
    [JsonProperty("max_weight")]
    public float MaxWeight;

    [JsonProperty("max_weight_per_item")]
    public float MaxWeightPerItem;

    [JsonProperty("encumbrance_mult")]
    public float EncumbranceMult;

    [JsonProperty("items_visible")]
    public bool ItemsVisible;

    [JsonProperty("tag_restriction")]
    public string[] TagRestriction = [];
}

// 物品栏精灵目标缩放尺寸：按像素宽高缩放图标，直到达成目标值。
// expandToFirstMet 为 true 时，任意一边先触达目标即停止缩放。
public class SpriteScaleDimensionsDef
{
    [JsonProperty("width")]
    public float Width;

    [JsonProperty("height")]
    public float Height;

    [JsonProperty("expand_to_first_met")]
    public bool ExpandToFirstMet;
}

// 物品脚本触发定义：动作名 → 脚本文件列表（路径相对于模组目录）。
// 支持的动作键：use / equip / unequip / use_on_limb
public class ItemScriptDef
{
    [JsonProperty("use")]
    public List<string> Use = [];

    [JsonProperty("equip")]
    public List<string> Equip = [];

    [JsonProperty("unequip")]
    public List<string> Unequip = [];

    [JsonProperty("use_on_limb")]
    public List<string> UseOnLimb = [];
}

// 制作特性数据
public class QualitiesDef
{
    [JsonProperty("id")]
    public string Id = string.Empty;

    [JsonProperty("amount")]
    public float Amount;
}

// 液体容器物品 JSON 数据模型，继承 ItemDef 并增加液体相关字段。
public class LiquidItemDef : ItemDef
{
    [JsonProperty("capacity")]
    public float Capacity;

    [JsonProperty("auto_fill")]
    public bool AutoFill = true;

    // liquidId -> amount (ml)
    [JsonProperty("default_liquid")]
    public Dictionary<string, float>? DefaultLiquid;
}
