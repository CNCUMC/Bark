using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Recipe;

// 自定义合成表 JSON 数据模型。
// 放在 ModName/Recipe/*.json，每个文件一个合成表。
// JSON 字段一律使用 snake_case。
public class RecipeDef
{
    // 产物数量
    [JsonProperty("amount")] public int Amount = 1;

    // 蓝图分类: Materials, Tools, Medicine, Utilities, Food
    [JsonProperty("category")] public string Category = "Materials";

    // 不消耗原料液体
    [JsonProperty("dont_drain_result_liquid")]
    public bool DontDrainResultLiquid;

    // 制作所需智力
    [JsonProperty("int")] public int INT;

    // 产物物品 ID（自定义物品或原版物品）
    [JsonProperty("id")] public string Id = string.Empty;

    // 产物是否为液体
    [JsonProperty("is_liquid")] public bool IsLiquid;

    // 是否为修复配方
    [JsonProperty("is_repair")] public bool IsRepair;

    // 材料列表
    [JsonProperty("items")] public List<RecipeIngredientDef> Items = null!;

    // 是否替换原版同名合成表
    [JsonProperty("replace_original_recipe")]
    public bool ReplaceOriginalRecipe;

    // 产物默认耐久（普通物品 1=100%，液体 1=1ml）
    [JsonProperty("result_condition")] public float ResultCondition = 1f;
}

// 合成表材料定义
public class RecipeIngredientDef
{
    // 制作完成后是否消除物品
    [JsonProperty("destroy_item")] public bool DestroyItem = true;

    // 排除的特定物品 ID
    [JsonProperty("ignored_id")] public string IgnoredId = string.Empty;

    // 材料是否为液体
    [JsonProperty("is_liquid")] public bool IsLiquid;

    // 最小耐久度
    [JsonProperty("minimum_condition")] public float MinimumCondition = 0.9f;

    // 制作特性关键字（拥有该特性的物品都可作为材料）
    // 可用: foliage, cutting, rippable, dressing, disinfectant, water,
    //       blood, nails, fat, opiate, heatsource, firestarter, flammable,
    //       flour, produce, condiment, hammering
    [JsonProperty("quality")] public string Quality = string.Empty;

    // 特性消耗量
    [JsonProperty("quality_condition")] public float QualityCondition = 1f;

    // 精确匹配物品 ID（与 quality 互斥）
    [JsonProperty("specific")] public bool Specific;

    [JsonProperty("specific_id")] public string SpecificId = string.Empty;
}