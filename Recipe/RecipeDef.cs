using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Recipe;

// 自定义合成表 JSON 数据模型。
// 放在 ModName/Recipe/*.json，每个文件一个合成表。
// JSON 字段一律使用 snake_case。
public class RecipeDef
{
    // 产物物品 ID（自定义物品或原版物品）
    [JsonProperty("id")]
    public string id = string.Empty;

    // 制作所需智力
    [JsonProperty("int")]
    public int INT;

    // 材料列表
    [JsonProperty("items")]
    public List<RecipeIngredientDef> items;

    // 蓝图分类: Materials, Tools, Medicine, Utilities, Food
    [JsonProperty("category")]
    public string category = "Materials";

    // 产物是否为液体
    [JsonProperty("is_liquid")]
    public bool isLiquid;

    // 产物数量
    [JsonProperty("amount")]
    public int amount = 1;

    // 产物默认耐久（普通物品 1=100%，液体 1=1ml）
    [JsonProperty("result_condition")]
    public float resultCondition = 1f;

    // 是否为修复配方
    [JsonProperty("is_repair")]
    public bool isRepair;

    // 不消耗原料液体
    [JsonProperty("dont_drain_result_liquid")]
    public bool dontDrainResultLiquid;

    // 是否替换原版同名合成表
    [JsonProperty("replase_origion_recipe")]
    public bool replaseOrigionRecipe;
}

// 合成表材料定义
public class RecipeIngredientDef
{
    // 精确匹配物品 ID（与 quality 互斥）
    [JsonProperty("specific")]
    public bool specific;

    [JsonProperty("specific_id")]
    public string specificId = string.Empty;

    // 材料是否为液体
    [JsonProperty("is_liquid")]
    public bool isLiquid;

    // 制作特性关键字（拥有该特性的物品都可作为材料）
    // 可用: foliage, cutting, rippable, dressing, disinfectant, water,
    //       blood, nails, fat, opiate, heatsource, firestarter, flammable,
    //       flour, produce, condiment, hammering
    [JsonProperty("quality")]
    public string quality = string.Empty;

    // 特性消耗量
    [JsonProperty("quality_condition")]
    public float qualityCondition = 1f;

    // 最小耐久度
    [JsonProperty("minimum_condition")]
    public float minimumCondition = 0.9f;

    // 制作完成后是否消除物品
    [JsonProperty("destroy_item")]
    public bool destroyItem = true;

    // 排除的特定物品 ID
    [JsonProperty("ignored_id")]
    public string ignoredId = string.Empty;
}
