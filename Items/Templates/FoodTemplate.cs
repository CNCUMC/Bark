using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

// 食物属性数据容器，由 FoodTemplate 从 ItemDef.CustomData 填充。
public class FoodData
{
    // body.Eat() 第一参数，饥饿值恢复
    public float Nutrition = 3.5f;

    // body.Eat() 第二参数，体重增量
    public float WeightOffset = 0.1f;

    // body.Drink() 参数，口渴值恢复
    public float Hydration = 5f;

    // body.happiness 增量
    public float Happiness = 0.5f;

    // 每次使用消耗的 condition（耐久度）
    public float ConditionLoss = 0.5f;

    // 咀嚼音效名称（Sound.Play 使用），空字符串表示不播放
    public string EatSound = "eatCrunch";

    // 是否触发 body.talker.EatGood() 语音
    public bool EatGoodVoice = true;
}

// 食物物品模板：运行时食物注册表 + 查询 API。
//
// ---- 物品 JSON 用法 ----
// "template": { "type": "food" }
// "template": {
//   "type": "food",
//   "nutrition": 3.5,          // body.Eat() 饥饿值（默认 3.5）
//   "weight_offset": 0.1,      // body.Eat() 体重增量（默认 0.1）
//   "hydration": 5.0,          // body.Drink() 口渴值（默认 5.0）
//   "happiness": 0.5,          // body.happiness 增量（默认 0.5）
//   "condition_loss": 0.5,     // 每次使用耐久损耗（默认 0.5）
//   "eat_sound": "eatCrunch",  // 咀嚼音效（默认 "eatCrunch"）
//   "eat_good_voice": true     // 是否触发 EatGood 语音（默认 true）
// }
//
// ---- 可覆盖的顶级字段（完整列表） ----
// "decay_minutes": 12.0,       // 腐烂时间（分钟）
// "weight": 0.75,              // 重量
// "value": 1,                  // 价值
// "ignore_depression": false,  // 抑郁时是否仍可食用（治愈食物设 true）
// "sprite.slot_rotation": 45,  // 物品栏格子旋转角度
// "decay.info": 0,             // 腐烂类型标志：1=NoDecayWithoutContainerItem（罐头）
// "recognition": 3,            // 识别等级
// "qualities": [{"type":"produce"}]
// "tags": "cangetwet"
//
// ---- 脚本端查询 ----
// FoodTemplate.IsFood(itemId)        → bool
// FoodTemplate.GetFoodData(itemId)   → FoodData / null
public class FoodTemplate : ItemTemplate
{
    // ==================== Template ====================

    public override string Name => "food";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            // ---- ItemDef 顶级字段（对标 geofruit 的 ItemInfo） ----
            ["origin_prefab"] = "geofruit",
            ["category"] = "custom",
            ["ignore_depression"] = false,
            ["destroy_at_zero_condition"] = true,
            ["weight"] = 0.75,
            ["scale_weight_with_condition"] = true,
            ["value"] = 1,
            ["recognition"] = 3,
            ["tags"] = "cangetwet",
            ["qualities"] = new JArray
            {
                new JObject { ["type"] = "produce" }
            },

            // ---- DecayDef 嵌套字段 ----
            // info: 位标志 — 1=NoDecayWithoutContainerItem（不放容器不腐烂，适合罐头）
            ["decay"] = new JObject
            {
                ["info"] = 0,
                ["minutes"] = 12.0
            },

            // ---- SpriteDef 嵌套字段 ----
            ["sprite"] = new JObject
            {
                ["slot_rotation"] = 45f
            },

            // ---- template 子对象（食物专属属性） ----
            ["template"] = new JObject
            {
                // 布尔标记 "food": true 是 CacheFoodItem 的类型识别标志，必须保留。
                ["food"] = true,
                ["nutrition"] = 3.5,
                ["weight_offset"] = 0.1,
                ["hydration"] = 5.0,
                ["happiness"] = 0.5,
                ["condition_loss"] = 0.5,
                ["eat_sound"] = "eatCrunch",
                ["eat_good_voice"] = true
            }
        };
    }

    // ==================== Registry ====================

    private static readonly Dictionary<string, FoodData> Registry = new();

    // ItemLoader 回调：检测 template 中 food 标记则缓存。
    public static void CacheFoodItem(string itemId, JObject? template)
    {
        if (template is null) return;
        if (template.TryGetValue("food", out var flag) && flag.Value<bool>())
            Registry[itemId] = FoodDataFromJObject(template);
    }

    // ItemLoader 回调：模组热重载时清除食物条目
    public static void RemoveFoodItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    private static FoodData FoodDataFromJObject(JObject t)
    {
        return new FoodData
        {
            Nutrition = (float?)t["nutrition"] ?? 3.5f,
            WeightOffset = (float?)t["weight_offset"] ?? 0.1f,
            Hydration = (float?)t["hydration"] ?? 5f,
            Happiness = (float?)t["happiness"] ?? 0.5f,
            ConditionLoss = (float?)t["condition_loss"] ?? 0.5f,
            EatSound = (string?)t["eat_sound"] ?? "eatCrunch",
            EatGoodVoice = (bool?)t["eat_good_voice"] ?? true
        };
    }

    // ==================== Query API ====================

    public static bool IsFood(string itemId)
    {
        return Registry.ContainsKey(itemId);
    }

    public static FoodData? GetFoodData(string itemId)
    {
        return Registry.GetValueOrDefault(itemId);
    }
}
