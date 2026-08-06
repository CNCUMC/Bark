using Bark.Items.Templates;
using Bark.Tool;
using UnityEngine;

namespace Bark.Items.Runtime.Gun;

// Partial：开火、弹壳解析、音效回调、保险、耳鸣自定义
public static partial class GunRuntimeManager
{
    // ============================================================
    // Fire Prefix：保存开火前的 hearingLoss，供 Postfix 做耳鸣倍率校准
    // ============================================================

    private static void OnFirePrefix(GunScript __instance)
    {
        var (gunItem, _) = TryGetTemplateGun(__instance);
        if (gunItem != null && PlayerCamera.main != null)
            _preFireHearingLoss = PlayerCamera.main.body.hearingLoss;
        else
            _preFireHearingLoss = -1f;
    }

    // ============================================================
    // Fire Postfix：消耗弹药、记录弹壳类型、应用耐久损耗
    // ============================================================

    private static void OnFirePostfix(GunScript __instance)
    {
        var (gunItem, gunData) = TryGetTemplateGun(__instance);
        if (gunItem == null || gunData == null) return;

        var state = GunMagTracker.GetOrCreate(gunItem);

        // 消耗一发（如果 Tracker 追踪的余弹为 0，用 GunScript 的值兜底）
        if (state.RoundsInMag > 0)
        {
            state.RoundsInMag--;
        }
        else if (__instance.roundsInMag > 0)
        {
            state.RoundsInMag = __instance.roundsInMag - 1;
        }

        // 同步到 GunScript（游戏原生的 Update 也依赖这个值来膛装下一发）
        __instance.roundsInMag = state.RoundsInMag;

        // 确定弹壳类型
        var casingType = ResolveAmmoCasingType(state, gunData);
        if (casingType != null)
        {
            state.PendingCasingType = casingType;
        }

        // 注意：耐久损耗已在 GunScript.Start Postfix 中完成配置，
        // GunScript.Fire 内部会通过 conditionLossPerShot * 0.01f 自动扣除。

        // 音效档案开火音效：当 profile 接管时 fireSound 为静音 clip，此处播放真实音效
        gunData.SoundProfile?.PlayRandom(gunData.SoundProfile.Fire, __instance.transform.position);

        // 耳鸣倍率校准：Prefix 已记录开火前 hearingLoss，
        // 此处用 tinnitus_multiplier 缩放原版新增的听力损失量。
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

    // 推断当前弹药对应的弹壳类型
    private static string? ResolveAmmoCasingType(GunMagTracker.MagState state, GunData gunData)
    {
        // 优先：直接记录的弹药物品 ID
        if (!string.IsNullOrEmpty(state.AmmoItemId))
        {
            var ct = AmmunitionTemplate.GetCasingType(state.AmmoItemId);
            if (ct != null) return ct;
        }

        // 其次：通过弹匣的 ammo_type 匹配任意弹药
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

        // 兜底：通过枪械的 ammo_type 匹配任意弹药
        {
            var ammoIds = AmmunitionTemplate.FindAmmoByType(gunData.AmmoType);
            if (ammoIds.Count <= 0) return null;
            var ct = AmmunitionTemplate.GetCasingType(ammoIds[0]);
            if (ct != null) return ct;
        }

        return null;
    }

    // Transpiler 回调：播放扳机音效（替代 ldstr "guntrigger"）。
    // 由 TranspileUpdate 动态注入到 GunScript.Update 中。
    // 模板枪械：优先播放 SoundProfile.Trigger 随机条目；否则退回到默认 Sound.Play("guntrigger")。
    private static void DoPlayTriggerSound(GunScript gun)
    {
        if (gun == null) return;
        var (_, gunData) = TryGetTemplateGun(gun);
        if (gunData?.SoundProfile?.Trigger is { Count: > 0 })
        {
            gunData.SoundProfile.PlayRandom(gunData.SoundProfile.Trigger, gun.transform.position);
            return;
        }
        Sound.Play("guntrigger", gun.transform.position);
    }

    // Transpiler 回调：播放卡壳音效（替代 ldstr "gunjam"）。
    // 由 TranspileUpdate/TranspileFire 动态注入到 Update/Fire 方法中。
    // 模板枪械：优先播放 SoundProfile.Jam 随机条目；否则退回到默认 Sound.Play("gunjam")。
    private static void DoPlayJamSound(GunScript gun)
    {
        if (gun == null) return;
        var (_, gunData) = TryGetTemplateGun(gun);
        if (gunData?.SoundProfile?.Jam is { Count: > 0 })
        {
            gunData.SoundProfile.PlayRandom(gunData.SoundProfile.Jam, gun.transform.position);
            return;
        }
        Sound.Play("gunjam", gun.transform.position, true);
    }

    // ToggleSafety Prefix：拦截自定义枪械的保险开关，播放 SoundProfile.Safety 音效。
    // 模板枪械：优先播放 profile Safety 随机条目，否则退回到默认 "gunsafety"。
    // 返回 false 阻止原版 ToggleSafety，手动完成 safe 状态翻转和音效播放。
    private static bool OnToggleSafetyPrefix(GunScript __instance)
    {
        var (_, gunData) = TryGetTemplateGun(__instance);
        if (gunData == null) return true;

        // 手动切换保险状态（替代原方法中的 this.safe = !this.safe）
        __instance.safe = !__instance.safe;

        // 播放音效：profile 优先，否则默认 "gunsafety"
        if (gunData.SoundProfile?.Safety is { Count: > 0 })
            gunData.SoundProfile.PlayRandom(gunData.SoundProfile.Safety, __instance.transform.position);
        else
            Sound.Play("gunsafety", __instance.transform.position);

        return false; // 阻止原版 ToggleSafety
    }
}
