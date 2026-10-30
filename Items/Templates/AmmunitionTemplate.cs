using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

public class AmmoData
{
    public string AmmoType = "7_62x51mm";
    public string? CasingType;
}

// "template": {
//   "type": "ammo",
//   "ammo_type": "7_62x51mm",
//   "casing_type": "7_62_casing"
// }

public class AmmunitionTemplate : ItemTemplate
{
    private static readonly Dictionary<string, AmmoData> Registry = new();
    public override string Name => "ammo";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            ["origin_prefab"] = "9mmround",
            ["category"] = "tool",
            ["combinable"] = true,
            ["destroy_at_zero_condition"] = false,
            ["template"] = new JObject
            {
                ["ammo"] = true,
                ["ammo_type"] = "7_62x51mm",
                ["casing_type"] = "7_62x51mm_casing"
            }
        };
    }

    public static void CacheAmmoItem(string itemId, JObject? template)
    {
        if (template is null) return;
        if (template.TryGetValue("ammo", out var flag) && flag.Value<bool>())
            Registry[itemId] = AmmoDataFromJObject(template);
    }

    public static void RemoveAmmoItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    private static AmmoData AmmoDataFromJObject(JObject t)
    {
        return new AmmoData
        {
            AmmoType = (string?)t["ammo_type"] ?? "7_62x51mm",
            CasingType = (string?)t["casing_type"]
        };
    }

    public static bool IsAmmo(string itemId)
    {
        return Registry.ContainsKey(itemId);
    }

    public static AmmoData? GetAmmoData(string itemId)
    {
        return Registry.GetValueOrDefault(itemId);
    }

    public static string GetAmmoType(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.AmmoType
            : "7_62x51mm";
    }

    public static string? GetCasingType(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.CasingType
            : null;
    }

    public static List<string> FindAmmoByType(string ammoType)
    {
        return
        [
            ..
            from kv in Registry
            where kv.Value.AmmoType == ammoType
            select kv.Key
        ];
    }
}
