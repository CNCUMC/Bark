using Bark.Tool;

namespace Bark.ScriptApi;

// 肢体操作 API（通过 bark.limb 访问）
public class LimbApi
{
    public bool HasBrokenBone() => LimbUtil.HasBrokenBone();
    public bool HasDislocation() => LimbUtil.HasDislocation();
    public bool HasInfection() => LimbUtil.HasInfection();
    public bool HasDismemberment() => LimbUtil.HasDismemberment();
    public float GetMaxInfection() => LimbUtil.GetMaxInfection();
    public float GetAveragePain() => LimbUtil.GetAveragePain();
    public float GetTotalBleedSpeed() => LimbUtil.GetTotalBleedSpeed();
    public void HealLimb(int index) => LimbUtil.HealLimb(index);
}
