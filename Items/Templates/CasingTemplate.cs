using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

// 弹壳属性数据容器，由 CasingTemplate 从 ItemDef.CustomData 填充。
public class CasingData
{
    // 弹壳类型标签，如 "9mm_casing"、"7_62_casing"、"12gauge_hull"。
    // 弹药通过 casing_type 匹配此标签，开火后生成对应弹壳物品。
    public string CasingType = "9mm_casing";
}

// 弹壳物品模板：预设 empty 容器（装弹壳）+ 运行时弹壳注册表 + 查询 API。
//
// ---- 物品 JSON 用法 ----
// "template": { "type": "casing" }
// "template": {
//   "type": "casing",
//   "casing_type": "7_62_casing"
// }
//
// ---- 脚本端查询 ----
// CasingTemplate.IsCasing(itemId)       → bool
// CasingTemplate.GetCasingType(itemId)  → string
// CasingTemplate.GetCasingData(itemId)  → CasingData / null
public class CasingTemplate : ItemTemplate
{
    // ==================== Template ====================

    public override string Name => "casing";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            ["category"] = "tool",
            ["combinable"] = true,
            ["weight"] = 0.01,
            ["custom_data"] = new JObject
            {
                ["casing"] = true,
                ["casing_type"] = "9mm_casing"
            }
        };
    }

    // ==================== Registry ====================

    private static readonly Dictionary<string, CasingData> Registry = new();

    // ItemLoader 回调：检测 ItemDef.CustomData 中 casing == true 则缓存。
    public static void CacheCasingItem(string itemId, Dictionary<string, object>? customData)
    {
        if (customData is null) return;
        if (customData.TryGetValue("casing", out var flag) && flag is true)
            Registry[itemId] = CasingDataFromDict(customData);
    }

    // ItemLoader 回调：模组热重载时清除弹壳条目
    public static void RemoveCasingItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    private static CasingData CasingDataFromDict(Dictionary<string, object> dict)
    {
        return new CasingData
        {
            CasingType = Casing_TryGetString(dict, "casing_type") ?? "9mm_casing"
        };
    }

    // ==================== Query API ====================

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
            : "9mm_casing";
    }

    // 查询匹配指定 casing_type 的所有弹壳物品 ID
    public static List<string> FindCasingsByType(string casingType)
    {
        return (from kv in Registry where kv.Value.CasingType == casingType select kv.Key).ToList();
    }

    // ==================== Helpers ====================

    private static string? Casing_TryGetString(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        return value as string ?? value.ToString();
    }
}
