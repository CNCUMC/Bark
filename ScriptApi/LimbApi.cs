using Bark.Tool;

namespace Bark.ScriptApi;

// 肢体操作 API（通过 bark.limb 访问）
public class LimbApi
{
    public bool HasBrokenBone()
    {
        return LimbUtil.HasBrokenBone();
    }

    public bool HasDislocation()
    {
        return LimbUtil.HasDislocation();
    }

    public bool HasInfection()
    {
        return LimbUtil.HasInfection();
    }

    public bool HasDismemberment()
    {
        return LimbUtil.HasDismemberment();
    }

    public float GetMaxInfection()
    {
        return LimbUtil.GetMaxInfection();
    }

    public float GetAveragePain()
    {
        return LimbUtil.GetAveragePain();
    }

    public float GetTotalBleedSpeed()
    {
        return LimbUtil.GetTotalBleedSpeed();
    }

    public void HealLimb(int index)
    {
        LimbUtil.HealLimb(index);
    }
}