using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

// 弹药属性数据容器，由 AmmunitionTemplate 从 ItemDef.CustomData 填充。
public class AmmoData
{
    // 子弹口径标签，如 "9mm"、"7_62x51mm"。
    // 枪械/弹匣只接受 ammo_type 匹配的弹药。
    public string AmmoType = "9mm";

    // 堆叠大小（一叠最多多少发）
    public int StackSize = 50;

    // 开火后产生的弹壳类型标签，如 "9mm_casing"、"7_62_casing"。
    // 为空/null 表示弹药全消耗不返回弹壳（如炮弹）。
    public string? CasingType;
}

// 弹药物品模板：预设 stackable 属性 + 运行时弹药注册表 + 查询 API。
//
// ---- 物品 JSON 用法 ----
// "template": { "type": "ammo" }
// "template": {
//   "type": "ammo",
//   "ammo_type": "7_62x51mm",
//   "stack_size": 60,
//   "casing_type": "7_62_casing"    // 可选，不填则全消耗不返回弹壳
// }
//
// ---- 脚本端查询 ----
// AmmunitionTemplate.IsAmmo(itemId)        → bool
// AmmunitionTemplate.GetAmmoType(itemId)   → string
// AmmunitionTemplate.GetAmmoData(itemId)   → AmmoData / null
public class AmmunitionTemplate : ItemTemplate
{
    // ==================== Template ====================

    public override string Name => "ammo";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            ["category"] = "tool",
            ["combinable"] = true,
            ["custom_data"] = new JObject
            {
                ["ammo"] = true,
                ["ammo_type"] = "9mm",
                ["stack_size"] = 50,
                ["casing_type"] = "9mm_casing"
            }
        };
    }

    // ==================== Registry ====================

    private static readonly Dictionary<string, AmmoData> Registry = new();

    // ItemLoader 回调：检测 ItemDef.CustomData 中 ammo == true 则缓存。
    public static void CacheAmmoItem(string itemId, Dictionary<string, object>? customData)
    {
        if (customData is null) return;
        if (customData.TryGetValue("ammo", out var flag) && flag is true)
            Registry[itemId] = AmmoDataFromDict(customData);
    }

    // ItemLoader 回调：模组热重载时清除弹药条目
    public static void RemoveAmmoItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    private static AmmoData AmmoDataFromDict(Dictionary<string, object> dict)
    {
        return new AmmoData
        {
            AmmoType = Ammo_TryGetString(dict, "ammo_type") ?? "9mm",
            StackSize = Ammo_TryGetInt(dict, "stack_size") ?? 50,
            CasingType = Ammo_TryGetString(dict, "casing_type")
        };
    }

    // ==================== Query API ====================

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
            : "9mm";
    }

    public static int GetStackSize(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.StackSize
            : 50;
    }

    // 返回弹壳类型标签，null 表示不产生弹壳（全消耗）
    public static string? GetCasingType(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.CasingType
            : null;
    }

    // 查询匹配指定 ammo_type 的所有弹药物品 ID
    public static List<string> FindAmmoByType(string ammoType)
    {
        return (from kv in Registry where kv.Value.AmmoType == ammoType select kv.Key).ToList();
    }

    // ==================== Helpers ====================

    private static string? Ammo_TryGetString(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        return value as string ?? value.ToString();
    }

    private static int? Ammo_TryGetInt(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        switch (value)
        {
            case int i: return i;
            case long l: return (int)l;
            case double d: return (int)d;
            default:
                try { return System.Convert.ToInt32(value); }
                catch { return null; }
        }
    }
}
