using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

// 弹匣属性数据容器，由 MagTemplate 从 ItemDef.CustomData 填充。
public class MagData
{
    // 接受的子弹口径标签，如 "9mm"、"7_62x51mm"。
    // 弹匣只接受 ammo_type 匹配的弹药。
    public string AmmoType = "7_62x51mm";

    // 弹匣容量（可装多少发）
    public int Capacity = 15;

    // 弹匣类型标签，如 "pistol_mag"、"ar15_mag"。
    // 枪械通过 mag_type 匹配此标签。
    public string MagType = "pistol_mag";

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
    // ==================== Registry ====================

    private static readonly Dictionary<string, MagData> Registry = new();
    // ==================== Template ====================

    public override string Name => "mag";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            // ---- 顶级字段 ----
            // "riflemagazine" 预制体自带 AmmoScript（itemType=Magazine, ammoType=Pistol）。
            // 用户可在 JSON 根级覆盖此值以更换预制体。
            ["origin_prefab"] = "riflemagazine",
            ["category"] = "tool",
            ["destroy_at_zero_condition"] = false,

            // ---- template 子对象 ----
            // 布尔标记 "mag": true 是 CacheMagItem 的类型识别标志，必须保留。
            ["template"] = new JObject
            {
                ["mag"] = true, // 缓存类型标记（必须）
                ["mag_type"] = "rifle_mag",
                ["ammo_type"] = "7_62x51mm",
                ["capacity"] = 15
            }
        };
    }

    // ItemLoader 回调：检测 template 中 mag 标记则缓存。
    public static void CacheMagItem(string itemId, JObject? template)
    {
        if (template is null) return;
        if (template.TryGetValue("mag", out var flag) && flag.Value<bool>())
            Registry[itemId] = MagDataFromJObject(template);
    }

    // ItemLoader 回调：模组热重载时清除弹匣条目
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

    // ==================== Query API ====================

    // 返回所有已注册弹匣的物品 ID
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
}