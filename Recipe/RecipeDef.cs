using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Recipe;

public class RecipeDef
{
    [JsonProperty("amount")] public int Amount = 1;
    [JsonProperty("category")] public string Category = "Materials";

    [JsonProperty("dont_drain_result_liquid")]
    public bool DontDrainResultLiquid;

    [JsonProperty("int")] public int INT;
    [JsonProperty("id")] public string Id = string.Empty;
    [JsonProperty("is_liquid")] public bool IsLiquid;
    [JsonProperty("is_repair")] public bool IsRepair;
    [JsonProperty("items")] public List<RecipeIngredientDef> Items = null!;

    [JsonProperty("replace_original_recipe")]
    public bool ReplaceOriginalRecipe;

    [JsonProperty("result_condition")] public float ResultCondition = 1f;
}

public class RecipeIngredientDef
{
    [JsonProperty("destroy_item")] public bool DestroyItem = true;
    [JsonProperty("ignored_id")] public string IgnoredId = string.Empty;
    [JsonProperty("is_liquid")] public bool IsLiquid;
    [JsonProperty("minimum_condition")] public float MinimumCondition = 0.9f;
    [JsonProperty("quality")] public string Quality = string.Empty;
    [JsonProperty("quality_condition")] public float QualityCondition = 1f;
    [JsonProperty("specific")] public bool Specific;
    [JsonProperty("specific_id")] public string SpecificId = string.Empty;
}
