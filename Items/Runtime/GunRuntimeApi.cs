using System.Linq;
using Bark.Items.Templates;
using Bark.ScriptApi;
using UnityEngine;

namespace Bark.Items.Runtime;

// 枪械运行时查询 API，暴露给 Lua/JS 脚本，标注 [ScriptApi] 自动注册到 ApiRegistry。
// 脚本侧通过全局变量 gunRuntime 调用，如 gunRuntime.getRoundsInMag("my_ak47")。
//
// 示例：
//   JS:  var rounds = gunRuntime.getRoundsInMag(gunItem.id);
//   Lua: local rounds = gunRuntime.getRoundsInMag(gunItem.id)
[ScriptApi(Name = "gunRuntime")]
public static class GunRuntimeApi
{
    // 获取指定枪械物品的当前余弹数，未找到返回 -1
    [ScriptMethod]
    public static int GetRoundsInMag(string gunItemId)
    {
        if (string.IsNullOrWhiteSpace(gunItemId)) return -1;

        // 查找持有该物品 ID 的枪械实例
        var gunItem = FindGunByItemId(gunItemId);
        if (gunItem == null) return -1;

        var state = GunMagTracker.Get(gunItem);
        if (state != null) return state.RoundsInMag;

        // 兜底：从 GunScript 读取（非模板枪械 / 状态未初始化）
        var gunScript = gunItem.GetComponent<GunScript>();
        return gunScript != null ? gunScript.roundsInMag : -1;
    }

    // 获取指定枪械当前插入的弹匣物品 ID，
    // 返回弹匣的模板物品 ID（如 "stanag_mag_30"），无弹匣或直装枪械返回 null
    [ScriptMethod]
    public static string? GetMagItemId(string gunItemId)
    {
        if (string.IsNullOrWhiteSpace(gunItemId)) return null;

        var gunItem = FindGunByItemId(gunItemId);
        if (gunItem == null) return null;

        var state = GunMagTracker.Get(gunItem);
        return state?.MagItemId;
    }

    // 获取指定枪械当前装入的弹药物品 ID，
    // 直装枪械直接记录弹药物品 ID，弹匣供弹枪返回 null（弹药类型由弹匣决定）
    [ScriptMethod]
    public static string? GetAmmoItemId(string gunItemId)
    {
        if (string.IsNullOrWhiteSpace(gunItemId)) return null;

        var gunItem = FindGunByItemId(gunItemId);
        if (gunItem == null) return null;

        var state = GunMagTracker.Get(gunItem);
        return state?.AmmoItemId;
    }

    // 查询指定弹药的弹壳类型标签，无弹壳（如炮弹）返回 null
    [ScriptMethod]
    public static string? GetCasingType(string ammoItemId)
    {
        if (string.IsNullOrWhiteSpace(ammoItemId)) return null;
        return AmmunitionTemplate.GetCasingType(ammoItemId);
    }

    // 判断指定物品 ID 是否为模板注册的枪械
    [ScriptMethod]
    public static bool IsTemplateGun(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return false;
        return GunTemplate.IsGun(itemId);
    }

    // 判断指定物品 ID 是否为模板注册的弹匣
    [ScriptMethod]
    public static bool IsTemplateMag(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return false;
        return MagTemplate.IsMag(itemId);
    }

    // 判断指定物品 ID 是否为模板注册的弹药
    [ScriptMethod]
    public static bool IsTemplateAmmo(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return false;
        return AmmunitionTemplate.IsAmmo(itemId);
    }

    // 判断指定物品 ID 是否为模板注册的弹壳
    [ScriptMethod]
    public static bool IsTemplateCasing(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return false;
        return CasingTemplate.IsCasing(itemId);
    }

    // ============================================================
    // 内部辅助
    // ============================================================

    // 在场景中查找持有指定物品 ID 的枪械 Item 实例
    private static Item? FindGunByItemId(string itemId)
    {
        var allGuns = Object.FindObjectsOfType<GunScript>();
        return allGuns.Select(gun => gun.GetComponent<Item>())
            .FirstOrDefault(item => item != null && item.id == itemId);
    }
}
