using Bark.Event;
using JetBrains.Annotations;

namespace Bark.Events;

// 枪械卡壳事件：拉栓/复位时未能正常上膛/抛壳，通过轮询 racked 状态对比检测
[ScriptEvent("onGunJam")]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GunJamEvent : BarkEvent
{
    // 卡壳的枪械
    public Item GunItem { get; set; } = null!;
}