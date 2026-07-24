using Bark.ScriptApi;
using UnityEngine;

namespace Bark.Tool;

public enum SkillType
{
    Strength = 0,
    Resilience = 1,
    Intelligence = 2
}

// 技能操作：等级、经验、进度读取/设置
public static class SkillUtil
{
    // ============================================================
    // 全局经验倍率
    // ============================================================

    public static float XpMultiplier
    {
        get => Skills.xpGainMult;
        set
        {
            if (WorldGeneration.runSettings != null) WorldGeneration.runSettings["xpgain"] = Mathf.Max(0f, value);
        }
    }

    // ============================================================
    // 内部辅助 - 统一 Skills 获取入口
    // ============================================================

    private static Skills? GetSkills()
    {
        return PlayerCamera.main?.body?.skills;
    }

    // ============================================================
    // 技能查询（SkillType 枚举版）
    // ============================================================

    public static int GetLevel(SkillType skillType)
    {
        return GetSkills() is { } skill
            ? skillType switch { SkillType.Strength => skill.STR, SkillType.Resilience => skill.RES, _ => skill.INT }
            : 0;
    }

    public static float GetExperience(SkillType skillType)
    {
        return GetSkills() is { } skill
            ? skillType switch
            {
                SkillType.Strength => skill.expSTR, SkillType.Resilience => skill.expRES, _ => skill.expINT
            }
            : 0f;
    }

    public static float GetProgress(SkillType skillType)
    {
        return GetSkills() is { } skill
            ? skillType switch
            {
                SkillType.Strength => skill.ToNextNormalized(skill.expSTR, skill.minSTR, skill.maxSTR),
                SkillType.Resilience => skill.ToNextNormalized(skill.expRES, skill.minRES, skill.maxRES),
                _ => skill.ToNextNormalized(skill.expINT, skill.minINT, skill.maxINT)
            }
            : 0f;
    }

    public static float GetExperienceInLevel(SkillType skillType)
    {
        return GetSkills() is { } skill
            ? skillType switch
            {
                SkillType.Strength => skill.expSTR - skill.minSTR, SkillType.Resilience => skill.expRES - skill.minRES,
                _ => skill.expINT - skill.minINT
            }
            : 0f;
    }

    public static float GetExperienceForNextLevel(SkillType skillType)
    {
        return GetSkills() is { } skill
            ? skillType switch
            {
                SkillType.Strength => skill.maxSTR - skill.minSTR, SkillType.Resilience => skill.maxRES - skill.minRES,
                _ => skill.maxINT - skill.minINT
            }
            : 0f;
    }

    public static int GetExperienceForLevel(int targetLevel)
    {
        return Skills.GetExperienceForLevel(targetLevel);
    }

    public static void AddExperience(SkillType skillType, float xp)
    {
        if (GetSkills() is { } skills) skills.AddExp((int)skillType, xp);
    }

    public static void SetLevelRaw(SkillType skillType, int level)
    {
        if (GetSkills() is not { } skill) return;
        level = Mathf.Max(0, level);
        switch (skillType)
        {
            case SkillType.Strength: skill.STR = level; break;
            case SkillType.Resilience: skill.RES = level; break;
            case SkillType.Intelligence:
            default: skill.INT = level; break;
        }

        skill.UpdateExpBoundaries();
        switch (skillType)
        {
            case SkillType.Strength: skill.expSTR = skill.minSTR; break;
            case SkillType.Resilience: skill.expRES = skill.minRES; break;
            case SkillType.Intelligence:
            default: skill.expINT = skill.minINT; break;
        }
    }

    // ============================================================
    // [ScriptMethod] string 重载：脚本侧用 "str" / "res" / "int"
    // ============================================================

    [ScriptMethod]
    public static int GetLevel(string skill)
    {
        return GetLevel(Parse(skill));
    }

    [ScriptMethod]
    public static float GetExperience(string skill)
    {
        return GetExperience(Parse(skill));
    }

    [ScriptMethod]
    public static float GetProgress(string skill)
    {
        return GetProgress(Parse(skill));
    }

    [ScriptMethod]
    public static void AddExperience(string skill, float xp)
    {
        AddExperience(Parse(skill), xp);
    }

    [ScriptMethod]
    public static void SetLevel(string skill, int level)
    {
        SetLevelRaw(Parse(skill), level);
    }

    [ScriptMethod]
    public static float GetExperienceForNextLevel(string skill)
    {
        return GetExperienceForNextLevel(Parse(skill));
    }

    // ============================================================
    // 内部辅助 - 字符串到 SkillType 解析
    // ============================================================

    private static SkillType Parse(string skill)
    {
        return skill.ToLowerInvariant() switch
        {
            "strength" or "str" => SkillType.Strength,
            "resilience" or "res" => SkillType.Resilience,
            _ => SkillType.Intelligence
        };
    }
}