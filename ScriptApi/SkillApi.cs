using Bark.Tool;

namespace Bark.ScriptApi;

public class SkillApi
{
    public int GetLevel(string skill)
    {
        return SkillUtil.GetLevel(ParseSkillType(skill));
    }

    public float GetExperience(string skill)
    {
        return SkillUtil.GetExperience(ParseSkillType(skill));
    }

    public float GetProgress(string skill)
    {
        return SkillUtil.GetProgress(ParseSkillType(skill));
    }

    public void AddExperience(string skill, float xp)
    {
        SkillUtil.AddExperience(ParseSkillType(skill), xp);
    }

    public void SetLevel(string skill, int level)
    {
        SkillUtil.SetLevelRaw(ParseSkillType(skill), level);
    }

    public float GetExperienceForNextLevel(string skill)
    {
        return SkillUtil.GetExperienceForNextLevel(ParseSkillType(skill));
    }

    private static SkillType ParseSkillType(string skill)
    {
        return skill.ToLower() switch
        {
            "strength" or "str" => SkillType.Strength,
            "resilience" or "res" => SkillType.Resilience,
            _ => SkillType.Intelligence
        };
    }
}