using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

public class MagData
{
    public string AmmoType = "7_62x51mm";
    public int Capacity = 15;
    public string MagType = "pistol_mag";
    public float MaxWeight = 0.5f;
}

// "template": {
//   "type": "mag",
//   "mag_type": "ar15_mag",
//   "ammo_type": "7_62x51mm",
//   "capacity": 30
// }

public class MagTemplate : ItemTemplate
{
    private static readonly Dictionary<string, MagData> Registry = new();
    public override string Name => "mag";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            ["origin_prefab"] = "riflemagazine",
            ["category"] = "tool",
            ["destroy_at_zero_condition"] = false,

            ["template"] = new JObject
            {
                ["mag"] = true,
                ["mag_type"] = "rifle_mag",
                ["ammo_type"] = "7_62x51mm",
                ["capacity"] = 15
            }
        };
    }

    public static void CacheMagItem(string itemId, JObject? template)
    {
        if (template is null) return;
        if (template.TryGetValue("mag", out var flag) && flag.Value<bool>())
            Registry[itemId] = MagDataFromJObject(template);
    }

    public static void RemoveMagItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    private static MagData MagDataFromJObject(JObject t)
    {
        return new MagData
        {
            MagType = (string?)t["mag_type"] ?? "rifle_mag",
            AmmoType = (string?)t["ammo_type"] ?? "7_62x51mm",
            Capacity = (int?)t["capacity"] ?? 15,
            MaxWeight = (float?)t["max_weight"] ?? (int?)t["capacity"] * 0.03f ?? 0.5f
        };
    }

    public static IEnumerable<string> GetAllMagIds()
    {
        return Registry.Keys;
    }

    public static bool IsMag(string itemId)
    {
        return Registry.ContainsKey(itemId);
    }

    public static MagData? GetMagData(string itemId)
    {
        return Registry.GetValueOrDefault(itemId);
    }

    public static string GetMagType(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.MagType
            : "pistol_mag";
    }

    public static string GetAmmoType(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.AmmoType
            : "7_62x51mm";
    }

    public static int GetCapacity(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.Capacity
            : 15;
    }

    public static List<string> FindMagsByType(string magType)
    {
        return
        [
            ..
            from kv in Registry
            where kv.Value.MagType == magType
            select kv.Key
        ];
    }

    public static List<string> FindMagsByAmmoType(string ammoType)
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
