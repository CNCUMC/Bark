using System.Linq;
using Bark.Items.Templates;
using Bark.ScriptApi;
using UnityEngine;

namespace Bark.Items.Runtime.Gun;

[ScriptApi(Name = "gunRuntime")]
public static class GunRuntimeApi
{
    [ScriptMethod]
    public static int GetRoundsInMag(string gunItemId)
    {
        if (string.IsNullOrWhiteSpace(gunItemId)) return -1;

        var gunItem = FindGunByItemId(gunItemId);
        if (gunItem == null) return -1;

        var state = GunMagTracker.Get(gunItem);
        if (state != null) return state.RoundsInMag;

        var gunScript = gunItem.GetComponent<GunScript>();
        return gunScript != null
            ? gunScript.roundsInMag
            : -1;
    }

    [ScriptMethod]
    public static string? GetMagItemId(string gunItemId)
    {
        if (string.IsNullOrWhiteSpace(gunItemId)) return null;

        var gunItem = FindGunByItemId(gunItemId);
        if (gunItem == null) return null;

        var state = GunMagTracker.Get(gunItem);
        return state?.MagItemId;
    }

    [ScriptMethod]
    public static string? GetAmmoItemId(string gunItemId)
    {
        if (string.IsNullOrWhiteSpace(gunItemId)) return null;

        var gunItem = FindGunByItemId(gunItemId);
        if (gunItem == null) return null;

        var state = GunMagTracker.Get(gunItem);
        return state?.AmmoItemId;
    }

    [ScriptMethod]
    public static string? GetCasingType(string ammoItemId)
    {
        if (string.IsNullOrWhiteSpace(ammoItemId)) return null;
        return AmmunitionTemplate.GetCasingType(ammoItemId);
    }

    [ScriptMethod]
    public static bool IsTemplateGun(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return false;
        return GunTemplate.IsGun(itemId);
    }

    [ScriptMethod]
    public static bool IsTemplateMag(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return false;
        return MagTemplate.IsMag(itemId);
    }

    [ScriptMethod]
    public static bool IsTemplateAmmo(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return false;
        return AmmunitionTemplate.IsAmmo(itemId);
    }

    [ScriptMethod]
    public static bool IsTemplateCasing(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return false;
        return CasingTemplate.IsCasing(itemId);
    }

    [ScriptMethod]
    public static float GetTinnitusMultiplier(string gunItemId)
    {
        if (string.IsNullOrWhiteSpace(gunItemId)) return 1.0f;
        return GunRuntimeManager.GetEffectiveTinnitusMultiplier(gunItemId);
    }

    [ScriptMethod]
    public static void SetTinnitusMultiplier(string gunItemId, float? multiplier = null)
    {
        if (string.IsNullOrWhiteSpace(gunItemId)) return;
        GunRuntimeManager.SetTinnitusMultiplierOverride(gunItemId, multiplier);
    }

    private static Item? FindGunByItemId(string itemId)
    {
        var allGuns = Object.FindObjectsOfType<GunScript>();
        return allGuns.Select(gun => gun.GetComponent<Item>())
            .FirstOrDefault(item => item != null && item.id == itemId);
    }
}
