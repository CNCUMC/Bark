using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

// 枪械属性数据容器，由 GunTemplate 从 ItemDef.CustomData 填充，
// 存入内部注册表，供脚本和其他系统查询。
public class GunData
{
    // 子弹口径标签，如 "9mm"、"7_62x51mm"、"5_56x45mm"、"12gauge"。
    // 枪械只接受 ammo_type 标签匹配的弹匣或弹药。
    public string AmmoType = "9mm";

    // 射击模式: semi_auto / auto / pump
    public string FiringMode = "semi_auto";

    // 弹匣类型标签，如 "pistol_mag"、"ar15_mag"。
    // 枪械只接受 mag_type 匹配的弹匣。直装枪将此字段置空。
    public string MagType = "pistol_mag";

    // 是否支持逐颗装填（Direct），无需弹匣
    public bool Direct;

    // 后坐力
    public float Knockback = 0.5f;

    // 对结构的伤害倍率
    public float StructureDamage = 12f;

    // 对生物的伤害倍率
    public float AnimalDamage = 25f;

    // 枪声大小（影响 NPC 反应范围）
    public float Loudness = 60f;

    // 每次开火的弹丸数（霰弹枪 >1）
    public int ShotsPerFire = 1;

    // 垂直散布角度
    public float VerticalSpread = 5f;

    // 每次开火的耐久消耗
    public float ConditionLossPerShot = 0.01f;
}

// 枪械物品模板：预设 tool 类别枪械的通用默认值 + 运行时枪械注册表 + 查询 API。
//
// ---- 物品 JSON 用法 ----
// "template": { "type": "gun" }
// "template": {
//   "type": "gun",
//   "ammo_type": "7_62x51mm",
//   "mag_type": "ar15_mag",
//   "firing_mode": "auto",
//   "origin_prefab": "rifle"
// }
//
// ---- 脚本端查询（JS/Lua 通过 ApiRegistry 代理调用） ----
// GunTemplate.IsGun(itemId)           → bool
// GunTemplate.GetAmmoType(itemId)     → string  ("9mm"、"7_62x51mm")
// GunTemplate.IsDirect(itemId)        → bool
// GunTemplate.GetMagType(itemId)      → string  ("pistol_mag"、"ar15_mag")
// GunTemplate.GetGunData(itemId)      → GunData / null
public class GunTemplate : ItemTemplate
{
    // ==================== Template ====================

    public override string Name => "gun";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            ["category"] = "tool",
            ["only_hold_in_hands"] = true,
            ["origin_prefab"] = "pistol",
            ["wearable"] = new JObject
            {
                ["slot_id"] = "weapon",
                ["desired_limb"] = "HandF",
                ["dual_wielded"] = false,
                ["sorting_order"] = 0,
                ["can_be_held"] = true
            },
            ["custom_data"] = new JObject
            {
                ["gun"] = true,
                ["ammo_type"] = "9mm",
                ["firing_mode"] = "semi_auto",
                ["mag_type"] = "pistol_mag",
                ["direct"] = false,
                ["knockback"] = 0.5,
                ["structure_damage"] = 12.0,
                ["animal_damage"] = 25.0,
                ["loudness"] = 60.0,
                ["shots_per_fire"] = 1,
                ["vertical_spread"] = 5.0,
                ["condition_loss_per_shot"] = 0.01
            }
        };
    }

    // ==================== Registry ====================

    private static readonly Dictionary<string, GunData> Registry = new();

    // ItemLoader 回调：检测 ItemDef.CustomData 中 gun == true 则缓存。
    // customData 可为 null（非枪械物品或未配置）。
    public static void CacheGunItem(string itemId, Dictionary<string, object>? customData)
    {
        if (customData is null) return;
        if (customData.TryGetValue("gun", out var flag) && flag is true)
            Registry[itemId] = GunDataFromDict(customData);
    }

    // ItemLoader 回调：模组热重载时清除枪械条目
    public static void RemoveGunItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    // 将反序列化后的 Dictionary<string, object> 转为强类型 GunData。
    // Newtonsoft 将 JSON 整数反序列化为 long、浮点数为 double，此处做容错转换。
    private static GunData GunDataFromDict(Dictionary<string, object> dict)
    {
        return new GunData
        {
            AmmoType = TryGetString(dict, "ammo_type") ?? "9mm",
            FiringMode = TryGetString(dict, "firing_mode") ?? "semi_auto",
            MagType = TryGetString(dict, "mag_type") ?? "pistol_mag",
            Direct = TryGetBool(dict, "direct"),
            Knockback = TryGetFloat(dict, "knockback") ?? 0.5f,
            StructureDamage = TryGetFloat(dict, "structure_damage") ?? 12f,
            AnimalDamage = TryGetFloat(dict, "animal_damage") ?? 25f,
            Loudness = TryGetFloat(dict, "loudness") ?? 60f,
            ShotsPerFire = TryGetInt(dict, "shots_per_fire") ?? 1,
            VerticalSpread = TryGetFloat(dict, "vertical_spread") ?? 5f,
            ConditionLossPerShot = TryGetFloat(dict, "condition_loss_per_shot") ?? 0.01f
        };
    }

    // ==================== Query API ====================

    public static bool IsGun(string itemId)
    {
        return Registry.ContainsKey(itemId);
    }

    public static GunData? GetGunData(string itemId)
    {
        return Registry.GetValueOrDefault(itemId);
    }

    public static string GetAmmoType(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.AmmoType
            : "9mm";
    }

    public static string GetFiringMode(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.FiringMode
            : "semi_auto";
    }

    public static bool IsDirect(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d) && d.Direct;
    }

    public static string GetMagType(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.MagType
            : "pistol_mag";
    }

    public static float GetKnockback(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.Knockback
            : 0.5f;
    }

    public static float GetStructureDamage(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.StructureDamage
            : 12f;
    }

    public static float GetAnimalDamage(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.AnimalDamage
            : 25f;
    }

    public static float GetLoudness(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.Loudness
            : 60f;
    }

    public static int GetShotsPerFire(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.ShotsPerFire
            : 1;
    }

    public static float GetVerticalSpread(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.VerticalSpread
            : 5f;
    }

    public static float GetConditionLossPerShot(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.ConditionLossPerShot
            : 0.01f;
    }

    // ==================== Helpers ====================

    private static string? TryGetString(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        return value as string ?? value.ToString();
    }

    private static bool TryGetBool(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return false;
        if (value is bool b) return b;
        return false;
    }

    private static int? TryGetInt(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        switch (value)
        {
            case int i:
                return i;
            case long l:
                return (int)l;
            case double d:
                return (int)d;
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

    private static float? TryGetFloat(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        switch (value)
        {
            case float f:
                return f;
            case double d:
                return (float)d;
            case int i:
                return i;
            case long l:
                return l;
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
