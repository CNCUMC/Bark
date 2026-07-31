using Bark.Event;
using JetBrains.Annotations;

namespace Bark.Events;

// 拉枪栓事件：TryRack() 被调用且 racked 状态发生变化时触发
[ScriptEvent("onGunRack")]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GunRackEvent : BarkEvent
{
    // 操作的枪械
    public Item GunItem { get; set; } = null!;

    // 拉栓后的状态（true = 已拉栓 / 空仓挂机，false = 复位）
    public bool Racked { get; set; }
}