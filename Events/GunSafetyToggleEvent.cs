using Bark.Event;
using JetBrains.Annotations;

namespace Bark.Events;

// 保险切换事件：ToggleSafety() 被调用时触发
[ScriptEvent("onGunSafetyToggle")]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GunSafetyToggleEvent : BarkEvent
{
    // 操作的枪械
    public Item GunItem { get; set; } = null!;
    // 切换后的保险状态（true = 开启保险，false = 关闭保险）
    public bool Safe { get; set; }
}
