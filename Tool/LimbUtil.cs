using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class LimbUtil
{
    public enum Slot
    {
        Head = 0,
        Thorax = 1,
        Pelvis = 2
    }

    public static Limb? GetLimb(int index)
    {
        var body = GameInstances.Body;
        if (body?.limbs == null || index < 0 || index >= body.limbs.Length) return null;
        return body.limbs[index];
    }

    public static Limb? GetLimb(Slot slot)
    {
        return GetLimb((int)slot);
    }

    public static Limb? GetLimbByName(string name)
    {
        return GameInstances.Body?.LimbByName(name);
    }

    public static List<Limb> GetAllLimbs()
    {
        var body = GameInstances.Body;
        return body?.limbs != null ? [..body.limbs] : [];
    }

    public static bool HasBrokenBone()
    {
        var body = GameInstances.Body;
        if (body?.limbs == null) return false;
        foreach (var limb in body.limbs)
            if (limb is { dismembered: false, broken: true })
                return true;
        return false;
    }

    public static bool HasDislocation()
    {
        var body = GameInstances.Body;
        if (body?.limbs == null) return false;
        foreach (var limb in body.limbs)
            if (limb is { dismembered: false, dislocated: true })
                return true;
        return false;
    }

    public static bool HasInfection()
    {
        var body = GameInstances.Body;
        if (body?.limbs == null) return false;
        foreach (var limb in body.limbs)
            if (limb is { dismembered: false, infected: true })
                return true;
        return false;
    }

    public static bool HasDismemberment()
    {
        var body = GameInstances.Body;
        if (body?.limbs == null) return false;
        foreach (var limb in body.limbs)
            if (limb is { dismembered: true })
                return true;
        return false;
    }

    public static float GetMaxInfection()
    {
        var body = GameInstances.Body;
        if (body?.limbs == null) return 0f;
        var max = 0f;
        foreach (var limb in body.limbs)
            if (limb is { dismembered: false } && limb.infectionAmount > max)
                max = limb.infectionAmount;
        return max;
    }

    public static float GetAveragePain()
    {
        return GameInstances.Body?.averagePain ?? 0f;
    }

    public static float GetTotalBleedSpeed()
    {
        return GameInstances.Body?.totalBleedSpeed ?? 0f;
    }

    public static void HealLimb(Limb? limb)
    {
        if (limb == null || limb.dismembered) return;
        limb.skinHealth = limb.muscleHealth = 100f;
        limb.bleedAmount = limb.pain = limb.infectionAmount = 0f;
        limb.infected = false;
        limb.shrapnel = 0;
        if (limb.broken) limb.MendBone();
        if (limb.dislocated) limb.UnDislocate();
    }

    public static void HealLimb(int index)
    {
        HealLimb(GetLimb(index));
    }

    public static void DamageSkin(Limb? limb, float value)
    {
        if (limb != null) limb.skinHealth = Mathf.Clamp(limb.skinHealth - value, 0f, 100f);
    }

    public static void DamageMuscle(Limb? limb, float value)
    {
        if (limb != null) limb.muscleHealth = Mathf.Clamp(limb.muscleHealth - value, 0f, 100f);
    }

    public static void SetSkinHealthRaw(Limb? limb, float value)
    {
        if (limb != null) limb.skinHealth = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetMuscleHealthRaw(Limb? limb, float value)
    {
        if (limb != null) limb.muscleHealth = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetBleedRaw(Limb? limb, float value)
    {
        if (limb != null) limb.bleedAmount = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetPainRaw(Limb? limb, float value)
    {
        if (limb != null) limb.pain = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetInfectionRaw(Limb? limb, float value)
    {
        if (limb == null) return;
        limb.infectionAmount = Mathf.Clamp(value, 0f, 100f);
        limb.infected = value > 0f;
    }
}