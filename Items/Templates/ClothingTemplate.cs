using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

// 衣服注册标记。原版的穿戴（armor/isolation/slot_id etc.）、容器（max_weight etc.）、
// 电池（max_allowed_charge etc.）均在 ItemDef 顶层 wearable/container/battery 字段中，
// 无需模板重复。ClothingData 仅充当类型标记，供脚本端 IsClothing() 查询。
// 若后续原版扩展了衣服专属系统，可在此添加对应字段。
public class ClothingData
{
}

// 衣服物品模板：预设可穿戴物品的通用默认值 + 注册表 + 查询 API。
//
// ---- 物品 JSON 用法 ----
// "template": { "type": "clothing" }
//
// ---- 可覆盖的原版顶层/嵌套字段 ----
// "category": "utility",              // 分类
// "wearable.slot_id": "UpTorso",      // 装备槽位（必须）
// "wearable.desired_limb": "UpTorso", // 穿戴贴图目标肢体（必须）
// "wearable.armor": 0.0,              // 护甲值
// "wearable.isolation": 0.0,          // 保暖/隔离值
// "wearable.visual_offset": 5,        // 视觉层级偏移
// "wearable.can_be_held": false,      // 装备后是否仍可手持
// "wearable.multi": {...},            // 多肢体已装备贴图
// "container.max_weight": 0.0,        // 容器最大重量
// "container.encumbrance_mult": 1.0,  // 容器负重倍率
// "battery.max_allowed_charge": 0.0,  // 电池最大容量
// "battery.weight_reduction": false,  // 电池减重
// "battery.explode_at_zero": false,   // 电量归零爆炸
// "weight": 1.0,                      // 重量
// "value": 5,                         // 价值
// "recognition": 0,                   // 识别等级
// "tags": "",                         // 标签
// "decay.info": 0,                    // 腐烂类型标志
// "decay.minutes": 0.0,               // 腐烂时间（分钟，0=不腐烂）
// "sprite.slot_rotation": 0.0,        // 物品栏旋转角度
//
// ---- 脚本端查询 ----
// ClothingTemplate.IsClothing(itemId)        → bool
// ClothingTemplate.GetClothingData(itemId)   → ClothingData / null
public class ClothingTemplate : ItemTemplate
{
    // ==================== Registry ====================

    private static readonly Dictionary<string, ClothingData> Registry = new();
    // ==================== Template ====================

    public override string Name => "clothing";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            // ---- ItemDef 顶级字段 ----
            ["origin_prefab"] = "holidayhat",
            ["category"] = "utility",
            ["destroy_at_zero_condition"] = false,
            ["weight"] = 1.0,
            ["scale_weight_with_condition"] = false,
            ["value"] = 5,
            ["recognition"] = 0,
            ["tags"] = "",

            // ---- DecayDef 嵌套字段 ----
            // 衣服通常不腐烂
            ["decay"] = new JObject
            {
                ["info"] = 0,
                ["minutes"] = 0.0
            },

            // ---- SpriteDef 嵌套字段 ----
            ["sprite"] = new JObject
            {
                ["slot_rotation"] = 0f
            },

            // ---- WearableDef 嵌套字段 ----
            // slot_id 和 desired_limb 必须由用户填写，否则装备系统不会激活
            ["wearable"] = new JObject
            {
                ["slot_id"] = "",
                ["desired_limb"] = "",
                ["armor"] = 0.0,
                ["isolation"] = 0.0,
                ["visual_offset"] = 5,
                ["can_be_held"] = false,
                ["hit_durability_loss_multiplier"] = 0.0,
                ["sprite_offset_x"] = 0.0,
                ["sprite_offset_y"] = 0.0
            },

            // ---- template 子对象（类型标记 + 原版嵌套字段预设） ----
            ["template"] = new JObject
            {
                // "clothing": true 是 CacheClothingItem 的类型识别标志，必须保留。
                ["clothing"] = true,

                // ---- ContainerDef 嵌套字段 ----
                // 容器非零 max_weight 时自动激活
                ["container"] = new JObject
                {
                    ["max_weight"] = 0.0,
                    ["max_weight_per_item"] = 0.0,
                    ["encumbrance_mult"] = 1.0,
                    ["items_visible"] = false,
                    ["tag_restriction"] = new JArray()
                },

                // ---- BatteryDef 嵌套字段 ----
                // max_allowed_charge > 0 时自动激活电池系统
                ["battery"] = new JObject
                {
                    ["battery_type"] = "",
                    ["max_allowed_charge"] = 0.0,
                    ["start_charge"] = 0.0,
                    ["spawn_with_battery"] = true,
                    ["weight_reduction"] = false,
                    ["explode_at_zero"] = false,
                    ["preset"] = ""
                }
            }
        };
    }

    // ItemLoader 回调：检测 template 中 clothing 标记则缓存。
    public static void CacheClothingItem(string itemId, JObject? template)
    {
        if (template is null) return;
        if (template.TryGetValue("clothing", out var flag) && flag.Value<bool>())
            Registry[itemId] = new ClothingData();
    }

    // ItemLoader 回调：模组热重载时清除衣服条目
    public static void RemoveClothingItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    // ==================== Query API ====================

    // 查询某物品是否为衣服模板注册的物品（脚本端使用）
    public static bool IsClothing(string itemId)
    {
        return Registry.ContainsKey(itemId);
    }

    // 获取衣服注册数据。当前 ClothingData 为空标记，保留以备后续原版扩展。
    public static ClothingData? GetClothingData(string itemId)
    {
        return Registry.GetValueOrDefault(itemId);
    }
}