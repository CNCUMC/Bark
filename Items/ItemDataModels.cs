using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Items;

// ---- 新版分组模型（当前格式） ----
// JSON 字段一律使用 snake_case。分组原则：
//   "属性": { "子属性": "值" }  ← 新标准
//   不再使用 flat bool "属性": true 或 "属性_data": { ... } 后缀

public class ItemDef
{
    // ---- 基础字段（顶层） ----
    [JsonProperty("full_name")] public string FullName = string.Empty;
    [JsonProperty("description")] public string Description = string.Empty;
    [JsonProperty("category")] public string Category = string.Empty;
    [JsonProperty("weight")] public float Weight;
    [JsonProperty("value")] public int Value;
    [JsonProperty("tags")] public string Tags = string.Empty;
    [JsonProperty("origin_prefab")] public string OriginPrefab = "geofruit";

    // ---- 布尔标记（顶层） ----
    [JsonProperty("combinable")] public bool Combinable;
    [JsonProperty("destroy_at_zero_condition")] public bool DestroyAtZeroCondition = true;
    [JsonProperty("only_hold_in_hands")] public bool OnlyHoldInHands;
    [JsonProperty("ignore_depression")] public bool IgnoreDepression;
    [JsonProperty("scale_weight_with_condition")] public bool ScaleWeightWithCondition;
    [JsonProperty("recognition")] public int Recognition;

    // ---- 分组字段 ----
    // 可装备（出现此对象即视为 wearable=true）
    [JsonProperty("wearable")] public WearableDef? Wearable;

    // 电池/电力（替代旧 battery_data）
    [JsonProperty("battery")] public BatteryDef? Battery;

    // 容器（替代旧 container_data）
    [JsonProperty("container")] public ContainerDef? Container;

    // 精灵图 / 显示
    [JsonProperty("sprite")] public SpriteDef? SpriteDef;

    // 腐烂
    [JsonProperty("decay")] public DecayDef? Decay;

    // 生成 / 掉落
    [JsonProperty("spawn")] public SpawnDef? Spawn;

    // ---- 已嵌套字段（无需迁移） ----
    [JsonProperty("script")] public ItemScriptDef? Script;
    [JsonProperty("use")] public List<UseEntryDef>? Use;
    [JsonProperty("qualities")] public List<QualitiesDef>? Qualities;
    [JsonProperty("custom_data")] public Dictionary<string, object>? CustomData;
}

// ---- 分组子模型 ----

public class WearableDef
{
    // 装备到哪个槽位（必须为游戏已知 15 个肢体之一）
    [JsonProperty("slot_id")] public string SlotId = string.Empty;

    // 装备到哪个肢体（必须为已知肢体名）
    [JsonProperty("desired_limb")] public string DesiredLimb = string.Empty;

    // 装备后是否仍可手持
    [JsonProperty("can_be_held")] public bool CanBeHeld;

    [JsonProperty("armor")] public float Armor;

    [JsonProperty("isolation")] public float Isolation;

    [JsonProperty("hit_durability_loss_multiplier")]
    public float HitDurabilityLossMultiplier;

    [JsonProperty("sorting_order")] public int? SortingOrder;

    [JsonProperty("visual_offset")] public int VisualOffset = 5;

    [JsonProperty("sprite_offset_x")] public float SpriteOffsetX;

    [JsonProperty("sprite_offset_y")] public float SpriteOffsetY;

    // 额外肢体已装备贴图（替代旧 multi_worn）
    [JsonProperty("multi")] public Dictionary<string, WornSpriteOffset>? Multi;

    // 穿戴脚本
    [JsonProperty("equip")] public List<string> Equip = [];
    [JsonProperty("unequip")] public List<string> Unequip = [];
    [JsonProperty("attack")] public List<string> Attack = [];
    [JsonProperty("damage")] public List<string> Damage = [];
}

public class BatteryDef
{
    [JsonProperty("battery_type")] public string BatteryType = string.Empty;

    [JsonProperty("explode_at_zero")] public bool ExplodeAtZero;

    [JsonProperty("max_allowed_charge")] public float MaxAllowedCharge;

    [JsonProperty("preset")] public string Preset = string.Empty;

    [JsonProperty("spawn_with_battery")] public bool SpawnWithBattery = true;

    [JsonProperty("start_charge")] public float StartCharge;

    [JsonProperty("weight_reduction")] public bool WeightReduction;

    [JsonProperty("charge_trigger")] public List<ConditionTriggerDef> ChargeTrigger = [];
}

public class ContainerDef
{
    [JsonProperty("encumbrance_mult")] public float EncumbranceMult;

    [JsonProperty("items_visible")] public bool ItemsVisible;

    [JsonProperty("max_weight")] public float MaxWeight;

    [JsonProperty("max_weight_per_item")] public float MaxWeightPerItem;

    [JsonProperty("tag_restriction")] public string[] TagRestriction = [];

    [JsonProperty("capacity_trigger")] public List<ConditionTriggerDef> CapacityTrigger = [];
}

public class SpriteDef
{
    // 精灵图导入放大倍数，默认 6.0
    [JsonProperty("import_scale")] public float ImportScale = 6f;

    [JsonProperty("scale")] public float Scale;

    // 物品栏精灵目标缩放尺寸，宽高是像素数，expand_to_first_met 碰边即停
    [JsonProperty("scale_dimensions")]
    public SpriteScaleDimensionsDef? ScaleDimensions;

    // 物品栏图标缩放
    [JsonProperty("inventory_icon_scale")] public float InventoryIconScale = 2f;

    // 物品栏格子旋转
    [JsonProperty("slot_rotation")] public float SlotRotation;
}

public class DecayDef
{
    [JsonProperty("info")] public byte Info;

    [JsonProperty("minutes")] public float Minutes;

    [JsonProperty("rot_speed")] public float? RotSpeed;
}

public class SpawnDef
{
    [JsonProperty("frequency")] public int Frequency;

    [JsonProperty("world_per_chunk")] public float? WorldPerChunk;

    [JsonProperty("drop_pool")] public string[]? DropPool;
}

// 额外部位已装备贴图偏移
public class WornSpriteOffset
{
    [JsonProperty("sprite_offset_x")] public float SpriteOffsetX;

    [JsonProperty("sprite_offset_y")] public float SpriteOffsetY;
}

// 物品栏精灵目标缩放尺寸
public class SpriteScaleDimensionsDef
{
    [JsonProperty("expand_to_first_met")] public bool ExpandToFirstMet;

    [JsonProperty("height")] public float Height;

    [JsonProperty("width")] public float Width;
}

// 条件触发器：复用于 durability / capacity_trigger / charge_trigger
public class ConditionTriggerDef
{
    [JsonProperty("operator")] public string Operator = "==";
    [JsonProperty("value")] public float Value;
    [JsonProperty("script")] public List<string> Script = [];
}

// use 数组中的每项
public class UseEntryDef
{
    [JsonProperty("slot")] public List<object>? Slot;
    [JsonProperty("limb_slot")] public List<string>? LimbSlot;
    [JsonProperty("script")] public List<string> Script = [];
}

// 物品被动脚本 + 条件触发器（被动状态检测）
public class ItemScriptDef
{
    [JsonProperty("attack")] public List<string> Attack = [];
    [JsonProperty("use_on_limb")] public List<string> UseOnLimb = [];
    [JsonProperty("in_backpack")] public List<string> InBackpack = [];
    [JsonProperty("in_hand")] public List<string> InHand = [];
    [JsonProperty("not_in_hand")] public List<string> NotInHand = [];
    [JsonProperty("durability")] public List<ConditionTriggerDef> Durability = [];
}

// 制作特性数据
public class QualitiesDef
{
    [JsonProperty("amount")] public float Amount;
    [JsonProperty("id")] public string Id = string.Empty;
}

// 物品光源配置
public class LightItemDef
{
    [JsonProperty("add_light_item")] public bool AddLightItem;
    [JsonProperty("color")] public string Color = "#FFFFFF";
    [JsonProperty("follow_mouse")] public bool FollowMouse;
    [JsonProperty("intensity")] public float Intensity = 10f;
    [JsonProperty("light_on_zero_condition")] public bool LightOnZeroCondition;
    [JsonProperty("light_type")] public string LightType = "Point";
    [JsonProperty("point_light_inner_angle")] public float PointLightInnerAngle = 360f;
    [JsonProperty("point_light_inner_radius")] public float PointLightInnerRadius;
    [JsonProperty("point_light_outer_angle")] public float PointLightOuterAngle = 360f;
    [JsonProperty("point_light_outer_radius")] public float PointLightOuterRadius = 8f;
    [JsonProperty("rotation")] public float Rotation = -90f;
    [JsonProperty("x_offset")] public float XOffset;
    [JsonProperty("y_offset")] public float YOffset;
}

// 液体容器物品 JSON 数据模型，继承 ItemDef 并增加液体相关字段。
public class LiquidItemDef : ItemDef
{
    [JsonProperty("auto_fill")] public bool AutoFill = true;

    [JsonProperty("capacity")] public float Capacity;

    // liquidId -> amount (ml)
    [JsonProperty("default_liquid")] public Dictionary<string, float>? DefaultLiquid;
}


// ---- 旧版模型（向后兼容，仅在 JSON 检测为旧格式时使用） ----

public class LegacyItemDef
{
    [JsonProperty("battery_data")] public ItemBatteryDef? BatteryData;
    [JsonProperty("category")] public string Category = string.Empty;
    [JsonProperty("combinable")] public bool Combinable;
    [JsonProperty("container_data")] public ItemContainerDef? ContainerData;
    [JsonProperty("decay_info")] public byte DecayInfo;
    [JsonProperty("decay_minutes")] public float DecayMinutes;
    [JsonProperty("description")] public string Description = string.Empty;
    [JsonProperty("desired_wear_limb")] public string DesiredWearLimb = string.Empty;
    [JsonProperty("destroy_at_zero_condition")] public bool DestroyAtZeroCondition = true;
    [JsonProperty("drop_pool")] public string[]? DropPool;
    [JsonProperty("full_name")] public string FullName = string.Empty;
    [JsonProperty("ignore_depression")] public bool IgnoreDepression;
    [JsonProperty("inventory_icon_scale")] public float InventoryIconScale = 4f;
    [JsonProperty("multi_worn")] public Dictionary<string, LegacyWornSpriteOffset>? MultiWorn;
    [JsonProperty("only_hold_in_hands")] public bool OnlyHoldInHands;
    [JsonProperty("origin_prefab")] public string OriginPrefab = "geofruit";
    [JsonProperty("qualities")] public List<QualitiesDef>? Qualities;
    [JsonProperty("recognition")] public int Recognition;
    [JsonProperty("rot_speed")] public float? RotSpeed;
    [JsonProperty("scale_weight_with_condition")] public bool ScaleWeightWithCondition;
    [JsonProperty("script")] public LegacyScriptDef? Script;
    [JsonProperty("slot_rotation")] public float SlotRotation;
    [JsonProperty("spawn_frequency")] public int SpawnFrequency;
    [JsonProperty("sprite_import_scale")] public float SpriteImportScale = 6f;
    [JsonProperty("sprite_scale")] public float SpriteScale;
    [JsonProperty("sprite_scale_dimensions")] public SpriteScaleDimensionsDef? SpriteScaleDimensions;
    [JsonProperty("tags")] public string Tags = string.Empty;
    [JsonProperty("value")] public int Value;
    [JsonProperty("wear_slot_id")] public string WearSlotId = string.Empty;
    [JsonProperty("wearable")] public bool Wearable;
    [JsonProperty("wearable_armor")] public float WearableArmor;
    [JsonProperty("wearable_can_be_held")] public bool WearableCanBeHeld;
    [JsonProperty("wearable_hit_durability_loss_multiplier")] public float WearableHitDurabilityLossMultiplier;
    [JsonProperty("wearable_isolation")] public float WearableIsolation;
    [JsonProperty("wearable_sorting_order")] public int? WearableSortingOrder;
    [JsonProperty("wearable_visual_offset")] public int WearableVisualOffset = 5;
    [JsonProperty("weight")] public float Weight;
    [JsonProperty("world_spawn_per_chunk")] public float? WorldSpawnPerChunk;
    [JsonProperty("worn_sprite_offset_x")] public float WornSpriteOffsetX;
    [JsonProperty("worn_sprite_offset_y")] public float WornSpriteOffsetY;
    [JsonProperty("custom_data")] public Dictionary<string, object>? CustomData;

    // 转换旧格式 → 新格式
    public ItemDef ToItemDef()
    {
        var def = new ItemDef
        {
            FullName = FullName,
            Description = Description,
            Category = Category,
            Weight = Weight,
            Value = Value,
            Tags = Tags,
            OriginPrefab = OriginPrefab,
            Combinable = Combinable,
            DestroyAtZeroCondition = DestroyAtZeroCondition,
            OnlyHoldInHands = OnlyHoldInHands,
            IgnoreDepression = IgnoreDepression,
            ScaleWeightWithCondition = ScaleWeightWithCondition,
            Recognition = Recognition,
            Qualities = Qualities,
            CustomData = CustomData
        };

        // 脚本迁移：旧 ItemScriptDef → 新的三层结构
        MigrateLegacyScripts(def, Script);

        // 可装备
        if (Wearable)
            def.Wearable = new WearableDef
            {
                SlotId = WearSlotId,
                DesiredLimb = DesiredWearLimb,
                CanBeHeld = WearableCanBeHeld,
                Armor = WearableArmor,
                Isolation = WearableIsolation,
                HitDurabilityLossMultiplier = WearableHitDurabilityLossMultiplier,
                SortingOrder = WearableSortingOrder,
                VisualOffset = WearableVisualOffset,
                SpriteOffsetX = WornSpriteOffsetX,
                SpriteOffsetY = WornSpriteOffsetY,
                Multi = ConvertMultiWorn(MultiWorn),
                Equip = Script?.Equip ?? [],
                Unequip = Script?.Unequip ?? []
            };

        // 电池
        if (BatteryData != null)
            def.Battery = new BatteryDef
            {
                BatteryType = BatteryData.BatteryType,
                ExplodeAtZero = BatteryData.ExplodeAtZero,
                MaxAllowedCharge = BatteryData.MaxAllowedCharge,
                Preset = BatteryData.Preset,
                SpawnWithBattery = BatteryData.SpawnWithBattery,
                StartCharge = BatteryData.StartCharge,
                WeightReduction = BatteryData.WeightReduction
            };

        // 容器
        if (ContainerData != null)
            def.Container = new ContainerDef
            {
                EncumbranceMult = ContainerData.EncumbranceMult,
                ItemsVisible = ContainerData.ItemsVisible,
                MaxWeight = ContainerData.MaxWeight,
                MaxWeightPerItem = ContainerData.MaxWeightPerItem,
                TagRestriction = ContainerData.TagRestriction
            };

        // 精灵图
        def.SpriteDef = new SpriteDef
        {
            ImportScale = SpriteImportScale,
            Scale = SpriteScale,
            ScaleDimensions = SpriteScaleDimensions,
            InventoryIconScale = InventoryIconScale,
            SlotRotation = SlotRotation
        };

        // 腐烂
        def.Decay = new DecayDef
        {
            Info = DecayInfo,
            Minutes = DecayMinutes,
            RotSpeed = RotSpeed
        };

        // 生成
        def.Spawn = new SpawnDef
        {
            Frequency = SpawnFrequency,
            WorldPerChunk = WorldSpawnPerChunk,
            DropPool = DropPool
        };

        return def;
    }

    private static Dictionary<string, WornSpriteOffset>? ConvertMultiWorn(
        Dictionary<string, LegacyWornSpriteOffset>? oldMulti)
    {
        if (oldMulti == null) return null;
        var result = new Dictionary<string, WornSpriteOffset>(oldMulti.Count);
        foreach (var kv in oldMulti)
            result[kv.Key] = new WornSpriteOffset
            {
                SpriteOffsetX = kv.Value.WornSpriteOffsetX,
                SpriteOffsetY = kv.Value.WornSpriteOffsetY
            };
        return result;
    }

    // 将旧 ItemScriptDef 迁移到新三层结构（Script / Use / Wearable）
    private static void MigrateLegacyScripts(ItemDef def, LegacyScriptDef? old)
    {
        if (old is null) return;

        // script.attack → script.attack（保留）
        // script.use_on_limb → script.use_on_limb（保留）
        var hasPassive = old.Attack.Count > 0 || old.UseOnLimb.Count > 0;
        if (hasPassive)
            def.Script = new ItemScriptDef
            {
                Attack = old.Attack,
                UseOnLimb = old.UseOnLimb
            };

        // script.use → use[{slot: ["*"], script:[...]}]
        // script.use_in_hand → use[{slot: ["hand"], script:[...]}]
        var useList = new List<UseEntryDef>();
        if (old.Use.Count > 0)
            useList.Add(new UseEntryDef { Script = old.Use }); // null slot = *
        if (old.UseInHand.Count > 0)
            useList.Add(new UseEntryDef { Slot = new List<object> { "hand" }, Script = old.UseInHand });
        if (useList.Count > 0)
            def.Use = useList;

        // script.equip / script.unequip → 交由 ToItemDef 调用方写入 Wearable.Equip/Unequip
    }

    // 旧格式的脚本定义（保留 Equip/Unequip/Use/UseInHand 等已移除字段）
    public class LegacyScriptDef
    {
        [JsonProperty("attack")] public List<string> Attack = [];
        [JsonProperty("equip")] public List<string> Equip = [];
        [JsonProperty("unequip")] public List<string> Unequip = [];
        [JsonProperty("use")] public List<string> Use = [];
        [JsonProperty("use_in_hand")] public List<string> UseInHand = [];
        [JsonProperty("use_on_limb")] public List<string> UseOnLimb = [];
    }
}

// Legacy 子模型

public class ItemBatteryDef
{
    [JsonProperty("battery_type")] public string BatteryType = string.Empty;
    [JsonProperty("explode_at_zero")] public bool ExplodeAtZero;
    [JsonProperty("max_allowed_charge")] public float MaxAllowedCharge;
    [JsonProperty("preset")] public string Preset = string.Empty;
    [JsonProperty("spawn_with_battery")] public bool SpawnWithBattery = true;
    [JsonProperty("start_charge")] public float StartCharge;
    [JsonProperty("weight_reduction")] public bool WeightReduction;
}

public class ItemContainerDef
{
    [JsonProperty("encumbrance_mult")] public float EncumbranceMult;
    [JsonProperty("items_visible")] public bool ItemsVisible;
    [JsonProperty("max_weight")] public float MaxWeight;
    [JsonProperty("max_weight_per_item")] public float MaxWeightPerItem;
    [JsonProperty("tag_restriction")] public string[] TagRestriction = [];
}

public class LegacyWornSpriteOffset
{
    [JsonProperty("worn_sprite_offset_x")] public float WornSpriteOffsetX;
    [JsonProperty("worn_sprite_offset_y")] public float WornSpriteOffsetY;
}

// 旧版液体容器物品（继承旧版 ItemDef 的 flat 字段 + 液体字段）
public class LegacyLiquidItemDef : LegacyItemDef
{
    [JsonProperty("auto_fill")] public bool AutoFill = true;
    [JsonProperty("capacity")] public float Capacity;

    // liquidId -> amount (ml)
    [JsonProperty("default_liquid")] public Dictionary<string, float>? DefaultLiquid;

    public LiquidItemDef ToLiquidItemDef()
    {
        var baseDef = ToItemDef();
        return new LiquidItemDef
        {
            FullName = baseDef.FullName,
            Description = baseDef.Description,
            Category = baseDef.Category,
            Weight = baseDef.Weight,
            Value = baseDef.Value,
            Tags = baseDef.Tags,
            OriginPrefab = baseDef.OriginPrefab,
            Combinable = baseDef.Combinable,
            DestroyAtZeroCondition = baseDef.DestroyAtZeroCondition,
            OnlyHoldInHands = baseDef.OnlyHoldInHands,
            IgnoreDepression = baseDef.IgnoreDepression,
            ScaleWeightWithCondition = baseDef.ScaleWeightWithCondition,
            Recognition = baseDef.Recognition,
            Wearable = baseDef.Wearable,
            Battery = baseDef.Battery,
            Container = baseDef.Container,
            SpriteDef = baseDef.SpriteDef,
            Decay = baseDef.Decay,
            Spawn = baseDef.Spawn,
            Script = baseDef.Script,
            Use = baseDef.Use,
            Qualities = baseDef.Qualities,
            CustomData = baseDef.CustomData,
            AutoFill = AutoFill,
            Capacity = Capacity,
            DefaultLiquid = DefaultLiquid
        };
    }
}
