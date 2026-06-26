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
        var b = GameInstances.Body;
        if (b?.limbs == null) return false;
        foreach (var l in b.limbs)
            if (l is { dismembered: false, broken: true })
                return true;
        return false;
    }

    public static bool HasDislocation()
    {
        var b = GameInstances.Body;
        if (b?.limbs == null) return false;
        foreach (var l in b.limbs)
            if (l is { dismembered: false, dislocated: true })
                return true;
        return false;
    }

    public static bool HasInfection()
    {
        var b = GameInstances.Body;
        if (b?.limbs == null) return false;
        foreach (var l in b.limbs)
            if (l is { dismembered: false, infected: true })
                return true;
        return false;
    }

    public static bool HasDismemberment()
    {
        var b = GameInstances.Body;
        if (b?.limbs == null) return false;
        foreach (var l in b.limbs)
            if (l is { dismembered: true })
                return true;
        return false;
    }

    public static float GetMaxInfection()
    {
        var b = GameInstances.Body;
        if (b?.limbs == null) return 0f;
        var max = 0f;
        foreach (var l in b.limbs)
            if (l is { dismembered: false } && l.infectionAmount > max)
                max = l.infectionAmount;
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

    public static void DamageSkin(Limb? l, float a)
    {
        if (l != null) l.skinHealth = Mathf.Clamp(l.skinHealth - a, 0f, 100f);
    }

    public static void DamageMuscle(Limb? l, float a)
    {
        if (l != null) l.muscleHealth = Mathf.Clamp(l.muscleHealth - a, 0f, 100f);
    }

    public static void SetSkinHealthRaw(Limb? l, float v)
    {
        if (l != null) l.skinHealth = Mathf.Clamp(v, 0f, 100f);
    }

    public static void SetMuscleHealthRaw(Limb? l, float v)
    {
        if (l != null) l.muscleHealth = Mathf.Clamp(v, 0f, 100f);
    }

    public static void SetBleedRaw(Limb? l, float v)
    {
        if (l != null) l.bleedAmount = Mathf.Clamp(v, 0f, 100f);
    }

    public static void SetPainRaw(Limb? l, float v)
    {
        if (l != null) l.pain = Mathf.Clamp(v, 0f, 100f);
    }

    public static void SetInfectionRaw(Limb? l, float v)
    {
        if (l == null) return;
        l.infectionAmount = Mathf.Clamp(v, 0f, 100f);
        l.infected = v > 0f;
    }
}