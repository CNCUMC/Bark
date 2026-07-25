using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Items;

// 普通物品 JSON 数据模型。字段与 CUCoreLib CustomItemInfo 一一对应，
// 兼容 ICEnecoCustomItemAPI 的 item.json 格式。
// JSON 字段一律使用 snake_case。

public class ItemDef
{
    // 物品 ID 由 JSON 文件名决定（如 bandage.json → id = "bandage"），
    // 不在 JSON 内指定 id 字段。
    // ---- ItemInfo 基础字段 ----
    [JsonProperty("origin_prefab")]
    public string originPrefab = "geofruit";

    // 精灵图导入放大倍数，默认 6.0
    [JsonProperty("sprite_import_scale")]
    public float spriteImportScale = 6f;

    [JsonProperty("sprite_scale")]
    public float spriteScale;

    [JsonProperty("inventory_icon_scale")]
    public float inventoryIconScale = 4f;

    // 物品栏精灵目标缩放尺寸，宽高是像素数，expand_to_first_met 碰边即停。
    // 不写则回退到 origin_prefab 的精灵尺寸，可能导致自定义精灵图显示过小。
    [JsonProperty("sprite_scale_dimensions")]
    public SpriteScaleDimensionsDef? spriteScaleDimensions;

    [JsonProperty("full_name")]
    public string fullName = string.Empty;

    [JsonProperty("description")]
    public string description = string.Empty;

    [JsonProperty("category")]
    public string category = string.Empty;

    [JsonProperty("slot_rotation")]
    public float slotRotation;

    [JsonProperty("rot_speed")]
    public float? rotSpeed;

    [JsonProperty("destroy_at_zero_condition")]
    public bool destroyAtZeroCondition = true;

    [JsonProperty("weight")]
    public float weight;

    [JsonProperty("scale_weight_with_condition")]
    public bool scaleWeightWithCondition;

    [JsonProperty("only_hold_in_hands")]
    public bool onlyHoldInHands;

    [JsonProperty("combineable")]
    public bool combineable;

    [JsonProperty("ignore_depression")]
    public bool ignoreDepression;

    [JsonProperty("value")]
    public int value;

    [JsonProperty("tags")]
    public string tags = string.Empty;

    [JsonProperty("recognition")]
    public int recognition;

    [JsonProperty("decay_minutes")]
    public float decayMinutes;

    [JsonProperty("decay_info")]
    public byte decayInfo;

    [JsonProperty("qualities")]
    public List<QualitiesDef>? qualities;

    // ---- 可装备字段 ----
    [JsonProperty("wearable")]
    public bool wearable;

    [JsonProperty("wearable_can_be_held")]
    public bool wearableCanBeHeld;

    [JsonProperty("desired_wear_limb")]
    public string desiredWearLimb = string.Empty;

    [JsonProperty("wear_slot_id")]
    public string wearSlotId = string.Empty;

    [JsonProperty("wearable_armor")]
    public float wearableArmor;

    [JsonProperty("wearable_isolation")]
    public float wearableIsolation;

    [JsonProperty("wearable_hit_durability_loss_multiplier")]
    public float wearableHitDurabilityLossMultiplier;

    [JsonProperty("wearable_visual_offset")]
    public int wearableVisualOffset = 5;

    [JsonProperty("wearable_sorting_order")]
    public int? wearableSortingOrder;

    [JsonProperty("worn_sprite_offset_x")]
    public float wornSpriteOffsetX;

    [JsonProperty("worn_sprite_offset_y")]
    public float wornSpriteOffsetY;

    // extra limb → offset
    [JsonProperty("multi_worn")]
    public Dictionary<string, WornSpriteOffset>? multiWorn;

    // ---- 容器字段 ----
    [JsonProperty("container_data")]
    public ItemContainerDef? containerData;

    // ---- 电池字段 ----
    [JsonProperty("battery_data")]
    public ItemBatteryDef? batteryData;

    // ---- 生成字段 ----
    [JsonProperty("drop_pool")]
    public string[]? dropPool;

    [JsonProperty("spawn_frequency")]
    public int spawnFrequency;

    [JsonProperty("world_spawn_per_chunk")]
    public float? worldSpawnPerChunk;

    // ---- 自定义脚本触发 ----
    // 物品动作 → 脚本文件列表（路径相对于模组目录）
    // 支持的动作键：use / equip / unequip / use_on_limb
    [JsonProperty("script")]
    public ItemScriptDef? script;

    // ---- 自定义物品行为（由 ICEnecoCustomItemAPI 的 ItemBehaviour 消费，Bark 仅透传解析） ----
    [JsonProperty("has_custom_item_behaviour")]
    public bool hasCustomItemBehaviour;

    [JsonProperty("custom_item_data")]
    public Dictionary<string, object>? customItemData;
}

// 额外部位已装备贴图偏移
public class WornSpriteOffset
{
    [JsonProperty("worn_sprite_offset_x")]
    public float wornSpriteOffsetX;

    [JsonProperty("worn_sprite_offset_y")]
    public float wornSpriteOffsetY;
}
// 电池/电力物品配置
public class ItemBatteryDef
{
    [JsonProperty("max_allowed_charge")]
    public float maxAllowedCharge;

    [JsonProperty("start_charge")]
    public float startCharge;

    [JsonProperty("preset")]
    public string preset = string.Empty;

    [JsonProperty("battery_type")]
    public string batteryType = string.Empty;

    [JsonProperty("spawn_with_battery")]
    public bool spawnWithBattery = true;

    [JsonProperty("weight_reduction")]
    public bool weightReduction;

    [JsonProperty("explode_at_zero")]
    public bool explodeAtZero;
}

// 容器物品配置
public class ItemContainerDef
{
    [JsonProperty("max_weight")]
    public float maxWeight;

    [JsonProperty("max_weight_per_item")]
    public float maxWeightPerItem;

    [JsonProperty("encumberance_mult")]
    public float encumberanceMult;

    [JsonProperty("items_visible")]
    public bool itemsVisible;

    [JsonProperty("tag_restriction")]
    public string[] tagRestriction = [];
}

// 物品栏精灵目标缩放尺寸：按像素宽高缩放图标，直到达成目标值。
// expandToFirstMet 为 true 时，任意一边先触达目标即停止缩放。
public class SpriteScaleDimensionsDef
{
    [JsonProperty("width")]
    public float width;

    [JsonProperty("height")]
    public float height;

    [JsonProperty("expand_to_first_met")]
    public bool expandToFirstMet;
}

// 物品脚本触发定义：动作名 → 脚本文件列表（路径相对于模组目录）。
// 支持的动作键：use / equip / unequip / use_on_limb
public class ItemScriptDef
{
    [JsonProperty("use")]
    public List<string> use = [];

    [JsonProperty("equip")]
    public List<string> equip = [];

    [JsonProperty("unequip")]
    public List<string> unequip = [];

    [JsonProperty("use_on_limb")]
    public List<string> useOnLimb = [];
}

// 制作特性数据
public class QualitiesDef
{
    [JsonProperty("id")]
    public string id = string.Empty;

    [JsonProperty("amount")]
    public float amount;
}
