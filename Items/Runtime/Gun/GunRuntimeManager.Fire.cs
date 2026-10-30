using Bark.Items.Templates;
using UnityEngine;

namespace Bark.Items.Runtime.Gun;

public static partial class GunRuntimeManager
{
    private static void OnFirePrefix(GunScript __instance)
    {
        var (gunItem, _) = TryGetTemplateGun(__instance);
        if (gunItem != null && PlayerCamera.main != null)
            _preFireHearingLoss = PlayerCamera.main.body.hearingLoss;
        else
            _preFireHearingLoss = -1f;
    }

    private static void OnFirePostfix(GunScript __instance)
    {
        var (gunItem, gunData) = TryGetTemplateGun(__instance);
        if (gunItem == null || gunData == null) return;

        var state = GunMagTracker.GetOrCreate(gunItem);

        if (state.RoundsInMag > 0)
            state.RoundsInMag--;
        else if (__instance.roundsInMag > 0)
            state.RoundsInMag = __instance.roundsInMag - 1;

        __instance.roundsInMag = state.RoundsInMag;

        var casingType = ResolveAmmoCasingType(state, gunData);
        if (casingType != null) state.PendingCasingType = casingType;

        gunData.SoundProfile?.PlayRandom(gunData.SoundProfile.Fire,
            __instance.transform.position);

        if (!(_preFireHearingLoss >= 0f) || PlayerCamera.main == null) return;
        var preValue = _preFireHearingLoss;
        var postValue = PlayerCamera.main.body.hearingLoss;
        var vanillaDelta = postValue - preValue;
        if (vanillaDelta > 0f)
        {
            var multiplier = GetEffectiveTinnitusMultiplier(gunItem.id);
            if (!Mathf.Approximately(multiplier, 1.0f))
                PlayerCamera.main.body.hearingLoss = preValue + vanillaDelta * multiplier;
        }

        _preFireHearingLoss = -1f;
    }

    private static string? ResolveAmmoCasingType(GunMagTracker.MagState state,
        GunData gunData)
    {
        if (!string.IsNullOrEmpty(state.AmmoItemId))
        {
            var ct = AmmunitionTemplate.GetCasingType(state.AmmoItemId);
            if (ct != null) return ct;
        }

        if (!string.IsNullOrEmpty(state.MagItemId))
        {
            var magAmmoType = MagTemplate.GetAmmoType(state.MagItemId);
            var ammoIds = AmmunitionTemplate.FindAmmoByType(magAmmoType);
            if (ammoIds.Count > 0)
            {
                var ct = AmmunitionTemplate.GetCasingType(ammoIds[0]);
                if (ct != null) return ct;
            }
        }

        {
            var ammoIds = AmmunitionTemplate.FindAmmoByType(gunData.AmmoType);
            if (ammoIds.Count <= 0) return null;
            var ct = AmmunitionTemplate.GetCasingType(ammoIds[0]);
            if (ct != null) return ct;
        }

        return null;
    }

    private static void DoPlayTriggerSound(GunScript gun)
    {
        if (gun == null) return;
        var (_, gunData) = TryGetTemplateGun(gun);
        if (gunData?.SoundProfile?.Trigger is { Count: > 0 })
        {
            gunData.SoundProfile.PlayRandom(gunData.SoundProfile.Trigger,
                gun.transform.position);
            return;
        }

        Sound.Play("guntrigger", gun.transform.position);
    }

    private static void DoPlayJamSound(GunScript gun)
    {
        if (gun == null) return;
        var (_, gunData) = TryGetTemplateGun(gun);
        if (gunData?.SoundProfile?.Jam is { Count: > 0 })
        {
            gunData.SoundProfile.PlayRandom(gunData.SoundProfile.Jam,
                gun.transform.position);
            return;
        }

        Sound.Play("gunjam", gun.transform.position, true);
    }

    private static bool OnToggleSafetyPrefix(GunScript __instance)
    {
        var (_, gunData) = TryGetTemplateGun(__instance);
        if (gunData == null) return true;

        __instance.safe = !__instance.safe;

        if (gunData.SoundProfile?.Safety is { Count: > 0 })
            gunData.SoundProfile.PlayRandom(gunData.SoundProfile.Safety,
                __instance.transform.position);
        else
            Sound.Play("gunsafety", __instance.transform.position);

        return false;
    }
}
