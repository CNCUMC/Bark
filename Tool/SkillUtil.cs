using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum SkillType
{
    Strength = 0,
    Resilience = 1,
    Intelligence = 2
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class SkillUtil
{
    public static float XpMultiplier
    {
        get => Skills.xpGainMult;
        set
        {
            if (WorldGeneration.runSettings != null) WorldGeneration.runSettings["xpgain"] = Mathf.Max(0f, value);
        }
    }

    private static Skills? GetSkills()
    {
        return GameInstances.Body?.skills;
    }

    public static int GetLevel(SkillType s)
    {
        return GetSkills() is { } sk
            ? s switch { SkillType.Strength => sk.STR, SkillType.Resilience => sk.RES, _ => sk.INT }
            : 0;
    }

    public static float GetExperience(SkillType s)
    {
        return GetSkills() is { } sk
            ? s switch { SkillType.Strength => sk.expSTR, SkillType.Resilience => sk.expRES, _ => sk.expINT }
            : 0f;
    }

    public static float GetProgress(SkillType s)
    {
        return GetSkills() is { } sk
            ? s switch
            {
                SkillType.Strength => sk.ToNextNormalized(sk.expSTR, sk.minSTR, sk.maxSTR),
                SkillType.Resilience => sk.ToNextNormalized(sk.expRES, sk.minRES, sk.maxRES),
                _ => sk.ToNextNormalized(sk.expINT, sk.minINT, sk.maxINT)
            }
            : 0f;
    }

    public static float GetExperienceInLevel(SkillType s)
    {
        return GetSkills() is { } sk
            ? s switch
            {
                SkillType.Strength => sk.expSTR - sk.minSTR, SkillType.Resilience => sk.expRES - sk.minRES,
                _ => sk.expINT - sk.minINT
            }
            : 0f;
    }

    public static float GetExperienceForNextLevel(SkillType s)
    {
        return GetSkills() is { } sk
            ? s switch
            {
                SkillType.Strength => sk.maxSTR - sk.minSTR, SkillType.Resilience => sk.maxRES - sk.minRES,
                _ => sk.maxINT - sk.minINT
            }
            : 0f;
    }

    public static int GetExperienceForLevel(int targetLevel)
    {
        return Skills.GetExperienceForLevel(targetLevel);
    }

    public static void AddExperience(SkillType s, float xp)
    {
        GetSkills()?.AddExp((int)s, xp);
    }

    public static void SetLevelRaw(SkillType s, int level)
    {
        if (GetSkills() is not { } sk) return;
        level = Mathf.Max(0, level);
        switch (s)
        {
            case SkillType.Strength: sk.STR = level; break;
            case SkillType.Resilience: sk.RES = level; break;
            case SkillType.Intelligence:
            default: sk.INT = level; break;
        }

        sk.UpdateExpBoundaries();
        switch (s)
        {
            case SkillType.Strength: sk.expSTR = sk.minSTR; break;
            case SkillType.Resilience: sk.expRES = sk.minRES; break;
            case SkillType.Intelligence:
            default: sk.expINT = sk.minINT; break;
        }
    }
}