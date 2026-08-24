using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

// 弹药属性数据容器，由 AmmunitionTemplate 从 ItemDef.CustomData 填充。
public class AmmoData
{
    // 子弹口径标签，如 "9mm"、"7_62x51mm"。
    // 枪械/弹匣只接受 ammo_type 匹配的弹药。
    public string AmmoType = "7_62x51mm";

    // 开火后产生的弹壳类型标签，如 "9mm_casing"、"7_62_casing"。
    // 为空/null 表示弹药全消耗不返回弹壳（如炮弹）。
    public string? CasingType;
}

// 弹药物品模板：运行时弹药注册表 + 查询 API。
//
// ---- 物品 JSON 用法 ----
// "template": { "type": "ammo" }
// "template": {
//   "type": "ammo",
//   "ammo_type": "7_62x51mm",
//   "casing_type": "7_62_casing"    // 可选，不填则全消耗不返回弹壳
// }
//
// ---- 脚本端查询 ----
// AmmunitionTemplate.IsAmmo(itemId)        → bool
// AmmunitionTemplate.GetAmmoType(itemId)   → string
// AmmunitionTemplate.GetAmmoData(itemId)   → AmmoData / null
public class AmmunitionTemplate : ItemTemplate
{
    private static readonly Dictionary<string, AmmoData> Registry = new();
    public override string Name => "ammo";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            // ---- 顶级字段 ----
            // "556round" 预制体自带 AmmoScript（itemType=Round, ammoType=Rifle）。
            // 用户可在 JSON 根级覆盖此值以更换预制体。
            ["origin_prefab"] = "9mmround",
            ["category"] = "tool",
            ["combinable"] = true,
            ["destroy_at_zero_condition"] = false,

            // ---- template 子对象 ----
            // 布尔标记 "ammo": true 是 CacheAmmoItem 的类型识别标志，必须保留。
            ["template"] = new JObject
            {
                ["ammo"] = true, // 缓存类型标记（必须）
                ["ammo_type"] = "7_62x51mm",
                ["casing_type"] = "7_62x51mm_casing"
            }
        };
    }

    // ItemLoader 回调：检测 template 中 ammo 标记则缓存。
    public static void CacheAmmoItem(string itemId, JObject? template)
    {
        if (template is null) return;
        if (template.TryGetValue("ammo", out var flag) && flag.Value<bool>())
            Registry[itemId] = AmmoDataFromJObject(template);
    }

    // ItemLoader 回调：模组热重载时清除弹药条目
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
        return [.. from kv in Registry where kv.Value.AmmoType == ammoType select kv.Key];
    }
}