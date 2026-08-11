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

// 枪械卡壳事件：拉栓/复位时未能正常上膛/抛壳，通过轮询 racked 状态对比检测
[ScriptEvent("onGunJam")]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GunJamEvent : BarkEvent
{
    // 卡壳的枪械
    public Item GunItem { get; set; } = null!;
}

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
