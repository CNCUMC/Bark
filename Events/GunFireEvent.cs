using Bark.Event;
using JetBrains.Annotations;

namespace Bark.Events;

// 枪械开火事件：Fire() 被调用时触发，包含自杀射击标记
[ScriptEvent("onGunFire")]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GunFireEvent : BarkEvent
{
    // 开火的枪械
    public Item GunItem { get; set; } = null!;

    // 是否为自杀射击（枪口对准自己）
    public bool Suicide { get; set; }
}