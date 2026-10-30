using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

public class ClothingData;

public class ClothingTemplate : ItemTemplate
{
    private static readonly Dictionary<string, ClothingData> Registry = new();
    public override string Name => "clothing";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            ["origin_prefab"] = "holidayhat",
            ["category"] = "utility",
            ["destroy_at_zero_condition"] = false,
            ["weight"] = 1.0,
            ["scale_weight_with_condition"] = false,
            ["value"] = 5,
            ["recognition"] = 0,
            ["tags"] = "",

            ["sprite"] = new JObject
            {
                ["slot_rotation"] = 0f
            },

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

            ["template"] = new JObject
            {
                ["clothing"] = true,

                ["container"] = new JObject
                {
                    ["max_weight"] = 0.0,
                    ["max_weight_per_item"] = 0.0,
                    ["encumbrance_mult"] = 1.0,
                    ["items_visible"] = false,
                    ["tag_restriction"] = new JArray()
                },

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

    public static void CacheClothingItem(string itemId, JObject? template)
    {
        if (template is null) return;
        if (template.TryGetValue("clothing", out var flag) && flag.Value<bool>())
            Registry[itemId] = new ClothingData();
    }

    public static void RemoveClothingItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    public static bool IsClothing(string itemId)
    {
        return Registry.ContainsKey(itemId);
    }

    public static ClothingData? GetClothingData(string itemId)
    {
        return Registry.GetValueOrDefault(itemId);
    }
}
