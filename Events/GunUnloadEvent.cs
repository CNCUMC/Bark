using Bark.Event;
using JetBrains.Annotations;

namespace Bark.Events;

// 枪械卸弹事件：UnloadMag() 被调用且成功卸下弹匣时触发
[ScriptEvent("onGunUnload")]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GunUnloadEvent : BarkEvent
{
    // 卸弹的枪械
    public Item GunItem { get; set; } = null!;
    // 卸下的弹药数量
    public int RoundsUnloaded { get; set; }
}
