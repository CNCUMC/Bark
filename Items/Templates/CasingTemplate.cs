using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

// 弹壳属性数据容器，由 CasingTemplate 从 ItemDef.CustomData 填充。
public class CasingData
{
    // 弹壳类型标签，如 "9mm_casing"、"7_62_casing"、"12gauge_hull"。
    // 弹药通过 casing_type 匹配此标签，开火后生成对应弹壳物品。
    public string CasingType = "7_62x51mm_casing";
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
            // ---- 顶级字段 ----
            // 弹壳不指定 origin_prefab，由 ItemRegistry 的默认逻辑处理。
            ["category"] = "tool",
            ["weight"] = 0.01,

            // ---- template 子对象 ----
            // 布尔标记 "casing": true 是 CacheCasingItem 的类型识别标志，必须保留。
            ["template"] = new JObject
            {
                ["casing"] = true,           // 缓存类型标记（必须）
                ["casing_type"] = "7_62x51mm_casing"
            }
        };
    }

    // ==================== Registry ====================

    private static readonly Dictionary<string, CasingData> Registry = new();

    // ItemLoader 回调：检测 template 中 casing 标记则缓存。
    public static void CacheCasingItem(string itemId, JObject? template)
    {
        if (template is null) return;
        if (template.TryGetValue("casing", out var flag) && flag.Value<bool>())
            Registry[itemId] = CasingDataFromJObject(template);
    }

    // ItemLoader 回调：模组热重载时清除弹壳条目
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
            : "7_62x51mm_casing";
    }

    // 查询匹配指定 casing_type 的所有弹壳物品 ID
    public static List<string> FindCasingsByType(string casingType)
    {
        return (from kv in Registry where kv.Value.CasingType == casingType select kv.Key).ToList();
    }

}
