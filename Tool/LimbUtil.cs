using System;
using System.Collections.Generic;
using System.Linq;
using Bark.BetterCCL;
using Bark.ScriptApi;
using UnityEngine;

namespace Bark.Tool;

public static class LimbUtil
{
    // ============================================================
    // 底层逻辑 - Limb 对象级操作（内部实现，不暴露给脚本）
    // ============================================================

    private static Body GetBody()
    {
        var body = PlayerUtil.Body;
        if (body.limbs == null)
            throw new InvalidOperationException(LimbLog("body_limbs_null"));
        return body;
    }

    private static Limb GetLimb(int index)
    {
        var limbs = GetBody().limbs;
        if (limbs == null || index < 0 || index >= limbs.Length)
            throw new ArgumentOutOfRangeException(nameof(index), index,
                LimbLog("index_out_of_range", limbs?.Length ?? 0));
        var limb = limbs[index];
        if (limb == null)
            throw new InvalidOperationException(LimbLog("limb_at_index_null", index));
        return limb;
    }

    private static Limb? GetLimbOrNull(int index)
    {
        var body = PlayerUtil.Body;
        if (body.limbs == null || index < 0 || index >= body.limbs.Length)
            return null;
        return body.limbs[index];
    }

    public static Limb? GetLimbByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        return GetBody().LimbByName(name);
    }

    public static List<Limb> GetAllLimbs()
    {
        var body = PlayerUtil.Body;
        if (body.limbs == null) return [];
        return [.. body.limbs];
    }

    // ============================================================
    // 内部辅助 - Limb 对象版（无 [ScriptMethod]，供 int-index 版复用）
    // ============================================================

    // -- 基础状态查询 --

    private static bool IsBroken(Limb limb)
    {
        return limb is { dismembered: false, broken: true };
    }

    private static bool IsDislocated(Limb limb)
    {
        return limb is { dismembered: false, dislocated: true };
    }

    private static bool IsInfected(Limb limb)
    {
        return limb is { dismembered: false, infected: true };
    }

    private static bool IsDismembered(Limb limb)
    {
        return limb is { dismembered: true };
    }

    private static bool IsSplinted(Limb limb)
    {
        return limb is { dismembered: false, splinted: true };
    }

    private static bool IsVital(Limb limb)
    {
        return limb.isVital;
    }

    private static bool IsHead(Limb limb)
    {
        return limb.isHead;
    }

    private static bool IsAbdomen(Limb limb)
    {
        return limb.isAbdomen;
    }

    private static bool IsArm(Limb limb)
    {
        return limb.isArm;
    }

    private static bool IsLeg(Limb limb)
    {
        return limb.isLegLimb;
    }

    private static bool HasShrapnel(Limb limb)
    {
        return limb.hasShrapnel;
    }

    private static bool IsBlockedBleeding(Limb limb)
    {
        return limb.blockedBleeding;
    }

    private static string GetLimbName(Limb limb)
    {
        return limb.fullName ?? string.Empty;
    }

    private static string GetLimbShortName(Limb limb)
    {
        return limb.shortName ?? string.Empty;
    }

    private static float GetSkinHealth(Limb limb)
    {
        return limb.skinHealth;
    }

    private static float GetMuscleHealth(Limb limb)
    {
        return limb.muscleHealth;
    }

    private static float GetBleedAmount(Limb limb)
    {
        return limb.bleedAmount;
    }

    private static float GetTotalBleedAmount(Limb limb)
    {
        return limb.totalBleedAmount;
    }

    private static float GetPain(Limb limb)
    {
        return limb.pain;
    }

    private static float GetInfectionAmount(Limb limb)
    {
        return limb.infectionAmount;
    }

    private static int GetShrapnelCount(Limb limb)
    {
        return limb.shrapnel;
    }

    private static float GetInjuryHealTime(Limb limb)
    {
        return limb.injuryHealTime;
    }

    // -- 修改操作 --

    private static void HealLimb(Limb limb)
    {
        if (limb == null || limb.dismembered) return;
        limb.skinHealth = 100f;
        limb.muscleHealth = 100f;
        limb.bleedAmount = 0f;
        limb.pain = 0f;
        limb.infectionAmount = 0f;
        limb.infected = false;
        limb.shrapnel = 0;
        if (limb.broken) limb.MendBone();
        if (limb.dislocated) limb.UnDislocate();
    }

    private static void DamageSkin(Limb limb, float value)
    {
        if (limb == null || limb.dismembered) return;
        limb.skinHealth = Mathf.Clamp(limb.skinHealth - value, 0f, 100f);
    }

    private static void DamageMuscle(Limb limb, float value)
    {
        if (limb == null || limb.dismembered) return;
        limb.muscleHealth = Mathf.Clamp(limb.muscleHealth - value, 0f, 100f);
    }

    private static void SetSkinHealthRaw(Limb limb, float value)
    {
        if (limb == null || limb.dismembered) return;
        limb.skinHealth = Mathf.Clamp(value, 0f, 100f);
    }

    private static void SetMuscleHealthRaw(Limb limb, float value)
    {
        if (limb == null || limb.dismembered) return;
        limb.muscleHealth = Mathf.Clamp(value, 0f, 100f);
    }

    private static void SetBleedRaw(Limb limb, float value)
    {
        if (limb == null || limb.dismembered) return;
        limb.bleedAmount = Mathf.Clamp(value, 0f, 100f);
    }

    private static void SetPainRaw(Limb limb, float value)
    {
        if (limb == null || limb.dismembered) return;
        limb.pain = Mathf.Clamp(value, 0f, 100f);
    }

    private static void SetInfectionRaw(Limb limb, float value)
    {
        if (limb == null || limb.dismembered) return;
        limb.infectionAmount = Mathf.Clamp(value, 0f, 100f);
        limb.infected = value > 0f;
    }

    private static void SetShrapnelRaw(Limb limb, int count)
    {
        if (limb == null || limb.dismembered) return;
        limb.shrapnel = Mathf.Max(0, count);
    }

    private static void DislocateLimb(Limb limb)
    {
        if (limb == null || limb.dismembered) return;
        limb.Dislocate();
    }

    private static void UnDislocateLimb(Limb limb)
    {
        if (limb == null || limb.dismembered) return;
        limb.UnDislocate();
    }

    private static void BreakBone(Limb limb)
    {
        if (limb == null || limb.dismembered) return;
        limb.BreakBone();
    }

    private static void MendBone(Limb limb)
    {
        if (limb == null || limb.dismembered) return;
        limb.MendBone();
    }

    private static void SetDisinfect(Limb limb, float time)
    {
        if (limb == null || limb.dismembered) return;
        limb.SetDisinfect(time);
    }

    private static void SetBlockedBleeding(Limb limb, bool blocked)
    {
        if (limb == null || limb.dismembered) return;
        limb.blockedBleeding = blocked;
    }

    // ============================================================
    // 外部接口 [ScriptMethod] - 肢体状态查询（按 int index）
    // ============================================================

    [ScriptMethod]
    public static bool IsBroken(int index)
    {
        return IsBroken(GetLimb(index));
    }

    [ScriptMethod]
    public static bool IsDislocated(int index)
    {
        return IsDislocated(GetLimb(index));
    }

    [ScriptMethod]
    public static bool IsInfected(int index)
    {
        return IsInfected(GetLimb(index));
    }

    [ScriptMethod]
    public static bool IsDismembered(int index)
    {
        return IsDismembered(GetLimb(index));
    }

    [ScriptMethod]
    public static bool IsSplinted(int index)
    {
        return IsSplinted(GetLimb(index));
    }

    [ScriptMethod]
    public static bool IsVital(int index)
    {
        return IsVital(GetLimb(index));
    }

    [ScriptMethod]
    public static bool IsHead(int index)
    {
        return IsHead(GetLimb(index));
    }

    [ScriptMethod]
    public static bool IsAbdomen(int index)
    {
        return IsAbdomen(GetLimb(index));
    }

    [ScriptMethod]
    public static bool IsArm(int index)
    {
        return IsArm(GetLimb(index));
    }

    [ScriptMethod]
    public static bool IsLeg(int index)
    {
        return IsLeg(GetLimb(index));
    }

    [ScriptMethod]
    public static bool HasShrapnel(int index)
    {
        return HasShrapnel(GetLimb(index));
    }

    [ScriptMethod]
    public static bool IsBlockedBleeding(int index)
    {
        return IsBlockedBleeding(GetLimb(index));
    }

    [ScriptMethod]
    public static string GetLimbName(int index)
    {
        return GetLimbName(GetLimb(index));
    }

    [ScriptMethod]
    public static string GetLimbShortName(int index)
    {
        return GetLimbShortName(GetLimb(index));
    }

    // -- 数值查询 --

    [ScriptMethod]
    public static float GetSkinHealth(int index)
    {
        return GetSkinHealth(GetLimb(index));
    }

    [ScriptMethod]
    public static float GetMuscleHealth(int index)
    {
        return GetMuscleHealth(GetLimb(index));
    }

    [ScriptMethod]
    public static float GetBleedAmount(int index)
    {
        return GetBleedAmount(GetLimb(index));
    }

    [ScriptMethod]
    public static float GetTotalBleedAmount(int index)
    {
        return GetTotalBleedAmount(GetLimb(index));
    }

    [ScriptMethod]
    public static float GetPain(int index)
    {
        return GetPain(GetLimb(index));
    }

    [ScriptMethod]
    public static float GetInfectionAmount(int index)
    {
        return GetInfectionAmount(GetLimb(index));
    }

    [ScriptMethod]
    public static int GetShrapnelCount(int index)
    {
        return GetShrapnelCount(GetLimb(index));
    }

    [ScriptMethod]
    public static float GetInjuryHealTime(int index)
    {
        return GetInjuryHealTime(GetLimb(index));
    }

    // ============================================================
    // 外部接口 [ScriptMethod] - 肢体修改操作（按 int index）
    // ============================================================

    [ScriptMethod]
    public static void HealLimb(int index)
    {
        HealLimb(GetLimb(index));
    }

    [ScriptMethod]
    public static void DamageSkin(int index, float value)
    {
        DamageSkin(GetLimb(index), value);
    }

    [ScriptMethod]
    public static void DamageMuscle(int index, float value)
    {
        DamageMuscle(GetLimb(index), value);
    }

    [ScriptMethod]
    public static void SetSkinHealth(int index, float value)
    {
        SetSkinHealthRaw(GetLimb(index), value);
    }

    [ScriptMethod]
    public static void SetMuscleHealth(int index, float value)
    {
        SetMuscleHealthRaw(GetLimb(index), value);
    }

    [ScriptMethod]
    public static void SetBleed(int index, float value)
    {
        SetBleedRaw(GetLimb(index), value);
    }

    [ScriptMethod]
    public static void SetPain(int index, float value)
    {
        SetPainRaw(GetLimb(index), value);
    }

    [ScriptMethod]
    public static void SetInfection(int index, float value)
    {
        SetInfectionRaw(GetLimb(index), value);
    }

    [ScriptMethod]
    public static void SetShrapnel(int index, int count)
    {
        SetShrapnelRaw(GetLimb(index), count);
    }

    [ScriptMethod]
    public static void SetDisinfect(int index, float time)
    {
        SetDisinfect(GetLimb(index), time);
    }

    [ScriptMethod]
    public static void SetBlockedBleeding(int index, bool blocked)
    {
        SetBlockedBleeding(GetLimb(index), blocked);
    }

    // -- 骨折 / 脱臼 --

    [ScriptMethod]
    public static void DislocateLimb(int index)
    {
        DislocateLimb(GetLimb(index));
    }

    [ScriptMethod]
    public static void UnDislocateLimb(int index)
    {
        UnDislocateLimb(GetLimb(index));
    }

    [ScriptMethod]
    public static void BreakBone(int index)
    {
        BreakBone(GetLimb(index));
    }

    [ScriptMethod]
    public static void MendBone(int index)
    {
        MendBone(GetLimb(index));
    }

    // ============================================================
    // 外部接口 [ScriptMethod] - 全局聚合查询（不含参数）
    // ============================================================

    [ScriptMethod]
    public static int GetLimbCount()
    {
        var body = PlayerUtil.Body;
        return body.limbs?.Length ?? 0;
    }

    [ScriptMethod]
    public static bool HasBrokenBone()
    {
        var limbs = GetAllLimbs();
        return limbs.Count > 0 && limbs.Exists(IsBroken);
    }

    [ScriptMethod]
    public static bool HasDislocation()
    {
        var limbs = GetAllLimbs();
        return limbs.Count > 0 && limbs.Exists(IsDislocated);
    }

    [ScriptMethod]
    public static bool HasInfection()
    {
        var limbs = GetAllLimbs();
        return limbs.Count > 0 && limbs.Exists(IsInfected);
    }

    [ScriptMethod]
    public static bool HasDismemberment()
    {
        var limbs = GetAllLimbs();
        return limbs.Count > 0 && limbs.Exists(IsDismembered);
    }

    [ScriptMethod]
    public static bool HasShrapnel()
    {
        var limbs = GetAllLimbs();
        return limbs.Count > 0 && limbs.Exists(HasShrapnel);
    }

    [ScriptMethod]
    public static bool HasBlockedBleeding()
    {
        var limbs = GetAllLimbs();
        return limbs.Count > 0 && limbs.Exists(IsBlockedBleeding);
    }

    [ScriptMethod]
    public static int CountBroken()
    {
        var limbs = GetAllLimbs();
        return limbs.Count(IsBroken);
    }

    [ScriptMethod]
    public static int CountDislocated()
    {
        var limbs = GetAllLimbs();
        return limbs.Count(IsDislocated);
    }

    [ScriptMethod]
    public static int CountInfected()
    {
        var limbs = GetAllLimbs();
        return limbs.Count(IsInfected);
    }

    [ScriptMethod]
    public static int CountDismembered()
    {
        var limbs = GetAllLimbs();
        return limbs.Count(IsDismembered);
    }

    [ScriptMethod]
    public static float GetMaxInfection()
    {
        var limbs = GetAllLimbs();
        var max = 0f;
        foreach (var l in limbs)
            if (l is { dismembered: false } && l.infectionAmount > max)
                max = l.infectionAmount;
        return max;
    }

    [ScriptMethod]
    public static float GetAveragePain()
    {
        return PlayerUtil.Body.averagePain;
    }

    [ScriptMethod]
    public static float GetTotalBleedSpeed()
    {
        return PlayerUtil.Body.totalBleedSpeed;
    }

    [ScriptMethod]
    public static float GetAverageSkinHealth()
    {
        var limbs = GetAllLimbs();
        if (limbs.Count == 0) return 0f;
        var sum = limbs.Sum(l => l.skinHealth);
        return sum / limbs.Count;
    }

    [ScriptMethod]
    public static float GetAverageMuscleHealth()
    {
        var limbs = GetAllLimbs();
        if (limbs.Count == 0) return 0f;
        var sum = limbs.Sum(l => l.muscleHealth);
        return sum / limbs.Count;
    }

    [ScriptMethod]
    public static float GetTotalBloodVolume()
    {
        return PlayerUtil.Body.bloodVolume;
    }

    [ScriptMethod]
    public static float GetBloodOxygen()
    {
        return PlayerUtil.Body.bloodOxygen;
    }

    [ScriptMethod]
    public static float GetBloodPressure()
    {
        return PlayerUtil.Body.bloodPressure;
    }

    // ============================================================
    // 本地化辅助
    // ============================================================

    private static string LimbLog(string key, params object[] args)
    {
        return BetterLocale.GetLog($"limb.{key}", args);
    }
}