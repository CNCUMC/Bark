using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

// 枪械属性数据容器，由 GunTemplate 从 ItemDef.CustomData 填充，
// 存入内部注册表，供脚本和其他系统查询。
public class GunData
{
    // 子弹口径标签，如 "9mm"、"7_62x51mm"、"5_56x45mm"、"12gauge"。
    // 枪械只接受 ammo_type 标签匹配的弹匣或弹药。
    public string AmmoType = "7_62x51mm";

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
    public float Loudness = 25f;

    // 每次开火的弹丸数（霰弹枪 >1）
    public int ShotsPerFire = 1;

    // 垂直散布角度
    public float VerticalSpread = 5f;

    // 每次开火的耐久消耗
    public float ConditionLossPerShot = 0.01f;

    // 直装枪械（Direct=true）的内部管/仓容量，
    // 如霰弹枪 6 发、杠杆步枪 5 发。
    // 弹匣供弹枪（Direct=false）忽略此字段。
    // 为 0 时使用 GunRuntimeManager 内置默认值（6）。
    public int Capacity;

    // 半自动/全自动的枪机循环延迟（秒）。
    // 控制每次射击后到下一次可击发的时间间隔，
    // 泵动式（pump）忽略此字段。
    public float DesiredGasTime = 0.1f;

    // 出厂时是否开启保险（start_safe）。
    // true 表示捡起后需要手动关保险才能射击，false 表示可以直接射击。
    public bool StartSafe;

    // 枪膛初始状态（start_chambered）。
    // true = 出厂膛内有弹，GunScript.Update() 渲染 HUD 并允许手动击发。
    // false = 出厂膛空，需先拉膛上弹才能击发。
    // 默认 true。注意 RoundInChamber 枚举只有 Round / None 两个值，不含弹药类型。
    public bool StartChambered = true;

    // 供弹方式: "mag"（弹匣）、"direct"（管/仓直装）、"revolver"（转轮）。
    // 与 FiringMode 不同：FiringMode 控制射击行为（semi_auto/auto/pump），
    // FeedType 控制 GunsScript 的内部装填逻辑。
    // 默认 "mag"。
    public string FeedType = "mag";

    // 枪口偏移量（相对于枪身根节点的 localPosition）。
    // X 为正 → 精灵右侧枪口方向，绝对值取决于精灵尺寸。
    // 通用预制体（geofruit/rifle）无自带 barrel 子对象时，GunRuntimeManager
    // 用此偏移量创建枪口 Transform，控制子弹生成位置和射击方向。
    // 默认 (0.5, 0.0) 适合步枪，手枪可设为 (0.2, 0.0)，霰弹枪 (0.6, 0.0)。
    public float BarrelOffsetX = 0.5f;
    public float BarrelOffsetY;
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
//   "origin_prefab": "rifle",
//   "barrel_offset": { "x": 0.5, "y": 0.0 }
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
            // ---- 顶级字段（与 ItemDef 对齐） ----
            // origin_prefab 决定生成的 GameObject 使用哪个 Unity 预制体。
            // "rifle" 预制体自带 GunScript 组件（ammoType=Rifle, feedType=Mag, firingMode=SemiAuto）。
            // 用户可在 JSON 根级覆盖此值以更换预制体。
            ["origin_prefab"] = "rifle",
            ["category"] = "utility",
            ["only_hold_in_hands"] = true,

            // ---- template 子对象（仅供模板系统消费，不参与 ItemDef 反序列化） ----
            // 布尔标记 "gun": true 是 CacheGunItem 的类型识别标志，必须保留。
            ["template"] = new JObject
            {
                ["gun"] = true,              // 缓存类型标记（必须）
                ["ammo_type"] = "7_62x51mm",
                ["firing_mode"] = "semi_auto",
                ["mag_type"] = "rifle_mag",
                ["direct"] = false,
                ["knockback"] = 0.5,
                ["structure_damage"] = 12.0,
                ["animal_damage"] = 25.0,
                ["loudness"] = 25.0,
                ["shots_per_fire"] = 1,
                ["vertical_spread"] = 5.0,
                ["condition_loss_per_shot"] = 0.01,
                ["capacity"] = 0,
                ["desired_gas_time"] = 0.1,
                ["start_safe"] = false,
                ["start_chambered"] = true,
                ["feed_type"] = "mag",
                ["barrel_offset"] = new JObject { ["x"] = 0.5, ["y"] = 0.0 }
            }
        };
    }

    // ==================== Registry ====================

    private static readonly Dictionary<string, GunData> Registry = new();

    // ItemLoader 回调：检测 template 中 gun 标记则缓存。
    // template 可为 null（非模板注册物品）。
    public static void CacheGunItem(string itemId, JObject? template)
    {
        if (template is null) return;
        if (template.TryGetValue("gun", out var flag) && flag.Value<bool>())
            Registry[itemId] = GunDataFromJObject(template);
    }

    // ItemLoader 回调：模组热重载时清除枪械条目
    public static void RemoveGunItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    private static GunData GunDataFromJObject(JObject t)
    {
        return new GunData
        {
            AmmoType = (string?)t["ammo_type"] ?? "7_62x51mm",
            FiringMode = (string?)t["firing_mode"] ?? "semi_auto",
            MagType = (string?)t["mag_type"] ?? "rifle_mag",
            Direct = t.TryGetValue("direct", out var dv) && dv.Value<bool>(),
            Knockback = (float?)t["knockback"] ?? 0.5f,
            StructureDamage = (float?)t["structure_damage"] ?? 12f,
            AnimalDamage = (float?)t["animal_damage"] ?? 25f,
            Loudness = (float?)t["loudness"] ?? 25f,
            ShotsPerFire = (int?)t["shots_per_fire"] ?? 1,
            VerticalSpread = (float?)t["vertical_spread"] ?? 5f,
            ConditionLossPerShot = (float?)t["condition_loss_per_shot"] ?? 0.01f,
            Capacity = (int?)t["capacity"] ?? 0,
            DesiredGasTime = (float?)t["desired_gas_time"] ?? 0.1f,
            StartSafe = t.TryGetValue("start_safe", out var ss) && ss.Value<bool>(),
            StartChambered = !t.TryGetValue("start_chambered", out var sc) || sc.Value<bool>(),
            FeedType = (string?)t["feed_type"] ?? "mag",
            BarrelOffsetX = ParseBarrelOffset(t, "x", 0.5f),
            BarrelOffsetY = ParseBarrelOffset(t, "y", 0f)
        };
    }

    // 解析 barrel_offset.{x|y} 子字段，不存在时返回默认值。
    private static float ParseBarrelOffset(JObject t, string axis, float defaultValue)
    {
        var offset = t["barrel_offset"] as JObject;
        if (offset is null) return defaultValue;
        return (float?)offset[axis] ?? defaultValue;
    }

    // ==================== Query API ====================

    // 返回所有已注册枪械的物品 ID
    public static IEnumerable<string> GetAllGunIds() => Registry.Keys;

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
            : "7_62x51mm";
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
            : 25f;
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

    public static int GetCapacity(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.Capacity
            : 0;
    }

}
