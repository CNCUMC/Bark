using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

// 弹匣属性数据容器，由 MagTemplate 从 ItemDef.CustomData 填充。
public class MagData
{
    // 弹匣类型标签，如 "pistol_mag"、"ar15_mag"。
    // 枪械通过 mag_type 匹配此标签。
    public string MagType = "pistol_mag";

    // 接受的子弹口径标签，如 "9mm"、"7_62x51mm"。
    // 弹匣只接受 ammo_type 匹配的弹药。
    public string AmmoType = "9mm";

    // 弹匣容量（可装多少发）
    public int Capacity = 15;

    // 容器最大负重（内部弹药重量上限）
    public float MaxWeight = 0.5f;
}

// 弹匣物品模板：预设 container 属性 + 运行时弹匣注册表 + 查询 API。
//
// ---- 物品 JSON 用法 ----
// "template": { "type": "mag" }
// "template": {
//   "type": "mag",
//   "mag_type": "ar15_mag",
//   "ammo_type": "7_62x51mm",
//   "capacity": 30
// }
//
// ---- 脚本端查询 ----
// MagTemplate.IsMag(itemId)         → bool
// MagTemplate.GetMagType(itemId)    → string
// MagTemplate.GetAmmoType(itemId)   → string
// MagTemplate.GetCapacity(itemId)   → int
// MagTemplate.GetMagData(itemId)    → MagData / null
public class MagTemplate : ItemTemplate
{
    // ==================== Template ====================

    public override string Name => "mag";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            ["category"] = "tool",
            ["container"] = new JObject
            {
                ["items_visible"] = true,
                ["max_weight"] = 0.5,
                ["tag_restriction"] = new JArray("9mm")
            },
            ["destroy_at_zero_condition"] = false,
            ["custom_data"] = new JObject
            {
                ["mag"] = true,
                ["mag_type"] = "pistol_mag",
                ["ammo_type"] = "9mm",
                ["capacity"] = 15
            }
        };
    }

    // ==================== Registry ====================

    private static readonly Dictionary<string, MagData> Registry = new();

    // ItemLoader 回调：检测 ItemDef.CustomData 中 mag == true 则缓存。
    public static void CacheMagItem(string itemId, Dictionary<string, object>? customData)
    {
        if (customData is null) return;
        if (customData.TryGetValue("mag", out var flag) && flag is true)
            Registry[itemId] = MagDataFromDict(customData);
    }

    // ItemLoader 回调：模组热重载时清除弹匣条目
    public static void RemoveMagItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    private static MagData MagDataFromDict(Dictionary<string, object> dict)
    {
        return new MagData
        {
            MagType = GunTemplate_TryGetString(dict, "mag_type") ?? "pistol_mag",
            AmmoType = GunTemplate_TryGetString(dict, "ammo_type") ?? "9mm",
            Capacity = GunTemplate_TryGetInt(dict, "capacity") ?? 15,
            MaxWeight = GunTemplate_TryGetFloat(dict, "max_weight")
                        ?? GunTemplate_TryGetFloat(dict, "capacity") * 0.03f
                        ?? 0.5f
        };
    }

    // ==================== Query API ====================

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
            : "9mm";
    }

    public static int GetCapacity(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.Capacity
            : 15;
    }

    // 查询匹配指定 mag_type 的所有弹匣 itemId
    public static List<string> FindMagsByType(string magType)
    {
        return [.. from kv in Registry where kv.Value.MagType == magType select kv.Key];
    }

    // 查询接受指定 ammo_type 的所有弹匣 itemId
    public static List<string> FindMagsByAmmoType(string ammoType)
    {
        return [.. from kv in Registry where kv.Value.AmmoType == ammoType select kv.Key];
    }

    // ==================== Helpers（复用 GunTemplate 的静态 helper 逻辑） ====================

    private static string? GunTemplate_TryGetString(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        return value as string ?? value.ToString();
    }

    private static int? GunTemplate_TryGetInt(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        switch (value)
        {
            case int i: return i;
            case long l: return (int)l;
            case double d: return (int)d;
            default:
                try
                {
                    return System.Convert.ToInt32(value);
                }
                catch
                {
                    return null;
                }
        }
    }

    private static float? GunTemplate_TryGetFloat(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        switch (value)
        {
            case float f: return f;
            case double d: return (float)d;
            case int i: return i;
            case long l: return l;
            default:
                try
                {
                    return System.Convert.ToSingle(value);
                }
                catch
                {
                    return null;
                }
        }
    }
}