using Bark.Event;
using JetBrains.Annotations;

namespace Bark.Events;

// 枪械装弹事件：LoadMag() 被调用且成功装填时触发
[ScriptEvent("onGunLoadAmmo")]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GunLoadAmmoEvent : BarkEvent
{
    // 装填的枪械
    public Item GunItem { get; set; } = null!;

    // 装填的弹药或弹匣的物品 ID
    public string AmmoItemId { get; set; } = string.Empty;

    // 装填的弹药数量
    public int Rounds { get; set; }
}