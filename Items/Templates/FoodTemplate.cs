using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

public class FoodData
{
    public float ConditionLoss = 0.5f;
    public bool EatGoodVoice = true;
    public string EatSound = "eatCrunch";
    public float Happiness = 0.5f;
    public float Hydration = 5f;
    public float Nutrition = 3.5f;
    public float WeightOffset = 0.1f;
}

// "template": {
//   "type": "food",
//   "nutrition": 3.5,
//   "weight_offset": 0.1,
//   "hydration": 5.0,
//   "happiness": 0.5,
//   "condition_loss": 0.5,
//   "eat_sound": "eatCrunch",
//   "eat_good_voice": true
// }

public class FoodTemplate : ItemTemplate
{
    private static readonly Dictionary<string, FoodData> Registry = new();
    public override string Name => "food";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
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

            ["decay"] = new JObject
            {
                ["info"] = 0,
                ["minutes"] = 12.0
            },

            ["sprite"] = new JObject
            {
                ["slot_rotation"] = 45f
            },

            ["template"] = new JObject
            {
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

    public static void CacheFoodItem(string itemId, JObject? template)
    {
        if (template is null) return;
        if (template.TryGetValue("food", out var flag) && flag.Value<bool>())
            Registry[itemId] = FoodDataFromJObject(template);
    }

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

    public static bool IsFood(string itemId)
    {
        return Registry.ContainsKey(itemId);
    }

    public static FoodData? GetFoodData(string itemId)
    {
        return Registry.GetValueOrDefault(itemId);
    }
}
