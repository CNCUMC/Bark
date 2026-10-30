using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

public class CasingData
{
    public string CasingType = "7_62x51mm_casing";
}

// "template": {
//   "type": "casing",
//   "casing_type": "7_62_casing"
// }

public class CasingTemplate : ItemTemplate
{
    private static readonly Dictionary<string, CasingData> Registry = new();
    public override string Name => "casing";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            ["category"] = "tool",
            ["weight"] = 0.01,

            ["template"] = new JObject
            {
                ["casing"] = true,
                ["casing_type"] = "7_62x51mm_casing"
            }
        };
    }

    public static void CacheCasingItem(string itemId, JObject? template)
    {
        if (template is null) return;
        if (template.TryGetValue("casing", out var flag) && flag.Value<bool>())
            Registry[itemId] = CasingDataFromJObject(template);
    }

    public static void RemoveCasingItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    private static CasingData CasingDataFromJObject(JObject t)
    {
        return new CasingData
        {
            CasingType = (string?)t["casing_type"] ?? "7_62x51mm_casing"
        };
    }

    public static bool IsCasing(string itemId)
    {
        return Registry.ContainsKey(itemId);
    }

    public static CasingData? GetCasingData(string itemId)
    {
        return Registry.GetValueOrDefault(itemId);
    }

    public static string GetCasingType(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.CasingType
            : "7_62x51mm_casing";
    }

    public static List<string> FindCasingsByType(string casingType)
    {
        return
        [
            ..
            from kv in Registry
            where kv.Value.CasingType == casingType
            select kv.Key
        ];
    }
}
