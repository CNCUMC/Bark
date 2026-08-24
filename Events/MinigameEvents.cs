using Bark.Event;

namespace Bark.Events;

// AED 除颤小游戏事件
// AED 小游戏开始事件：玩家对肢体使用 AED，小游戏启动时触发
[ScriptEvent("onAEDMinigameStart")]
public class AEDMinigameStartEvent : BarkEvent
{
    // 接受除颤的目标肢体
    public Limb Limb { get; set; } = null!;

    // 目标肢体在身体上的索引
    public int LimbIndex { get; set; }
}

// AED 除颤成功事件：小游戏进入除颤阶段，成功触发一次放电
[ScriptEvent("onAEDMinigameDefibrillate")]
public class AEDMinigameDefibrillateEvent : BarkEvent
{
    // 接受除颤的目标肢体
    public Limb Limb { get; set; } = null!;

    // 是否发生心室颤动（fibrillationProgress > 0）
    public bool WasFibrillating { get; set; }
}

// AED 失败事件：分析后未检测到可除颤心律，小游戏以失败告终
[ScriptEvent("onAEDMinigameFail")]
public class AEDMinigameFailEvent : BarkEvent
{
    // 失败时接受除颤的目标肢体
    public Limb Limb { get; set; } = null!;
}

// 包扎小游戏事件
// 包扎小游戏开始事件：玩家对肢体使用绷带，小游戏启动时触发
[ScriptEvent("onBandageMinigameStart")]
public class BandageMinigameStartEvent : BarkEvent
{
    // 接受包扎的目标肢体
    public Limb Limb { get; set; } = null!;

    // 包扎的色相（用于区分血液颜色等，可忽略）
    public float BandageAngle { get; set; }
}

// 包扎完成事件：玩家完成一圈缠绕，成功施加一次包扎
[ScriptEvent("onBandageMinigameWrap")]
public class BandageMinigameWrapEvent : BarkEvent
{
    // 接受包扎的目标肢体
    public Limb Limb { get; set; } = null!;
}

// 脱臼复位小游戏事件
// 脱臼复位小游戏开始事件：玩家对脱臼肢体进行复位时触发
[ScriptEvent("onDislocationMinigameStart")]
public class DislocationMinigameStartEvent : BarkEvent
{
    // 接受复位的肢体
    public Limb Limb { get; set; } = null!;

    // 是否使用扳手（减少疼痛）
    public bool HasWrench { get; set; }
}

// 脱臼复位成功事件：肢体成功复位（UnDislocate）时触发
[ScriptEvent("onDislocationMinigameSuccess")]
public class DislocationMinigameSuccessEvent : BarkEvent
{
    // 成功复位的肢体
    public Limb Limb { get; set; } = null!;
}

// 复位中骨折事件：复位过程中用力过猛导致骨头断裂时触发
[ScriptEvent("onDislocationMinigameBreak")]
public class DislocationMinigameBreakEvent : BarkEvent
{
    // 断裂的肢体
    public Limb Limb { get; set; } = null!;
}

// 手摇曲柄小游戏事件
// 手摇曲柄小游戏开始事件：玩家摇动曲柄给设备充电时触发
[ScriptEvent("onHandCrankMinigameStart")]
public class HandCrankMinigameStartEvent : BarkEvent
{
}

// 手摇曲柄转动充电事件：曲柄转动、电池获得电量时触发
[ScriptEvent("onHandCrankMinigameCharge")]
public class HandCrankMinigameChargeEvent : BarkEvent
{
    // 本次转动的角度（度）
    public float Angle { get; set; }
}

// 手摇曲柄小游戏结束事件：耐力耗尽小游戏结束时触发
[ScriptEvent("onHandCrankMinigameEnd")]
public class HandCrankMinigameEndEvent : BarkEvent
{
}

// 键盘密码小游戏事件
// 键盘密码小游戏开始事件：玩家开始输入密码时触发
[ScriptEvent("onKeypadMinigameStart")]
public class KeypadMinigameStartEvent : BarkEvent
{
    // 需要匹配的目标建筑
    public BuildingEntity ToDestroy { get; set; } = null!;
}

// 键盘密码小游戏成功事件：密码输入正确、目标建筑被摧毁时触发
[ScriptEvent("onKeypadMinigameSuccess")]
public class KeypadMinigameSuccessEvent : BarkEvent
{
    // 被解锁/摧毁的目标建筑
    public BuildingEntity ToDestroy { get; set; } = null!;
}

// 撬锁小游戏事件
// 撬锁小游戏开始事件：玩家开始撬锁时触发
[ScriptEvent("onLockpingMinigameStart")]
public class LockpingMinigameStartEvent : BarkEvent
{
    // 目标锁具建筑
    public BuildingEntity ToDestroy { get; set; } = null!;

    // 是否使用撬锁工具（pickLevel >= 0）
    public bool HasPick { get; set; }
}

// 撬锁成功事件：锁被成功撬开、目标建筑被摧毁时触发
[ScriptEvent("onLockpingMinigameSuccess")]
public class LockpingMinigameSuccessEvent : BarkEvent
{
    // 被撬开的锁具建筑
    public BuildingEntity ToDestroy { get; set; } = null!;
}

// 撬锁卡住事件：撬锁过程卡住（损坏工具/手指）时触发
[ScriptEvent("onLockpingMinigameStuck")]
public class LockpingMinigameStuckEvent : BarkEvent
{
    // 卡住的锁具建筑
    public BuildingEntity ToDestroy { get; set; } = null!;
}

// 手动除颤小游戏事件
// 手动除颤小游戏开始事件：玩家开始手动除颤时触发
[ScriptEvent("onManualDefibMinigameStart")]
public class ManualDefibMinigameStartEvent : BarkEvent
{
    // 接受除颤的肢体
    public Limb Limb { get; set; } = null!;

    // 是否在躯干上（能读取心率）
    public bool OnTorso { get; set; }
}

// 手动除颤放电事件：玩家放电进行除颤时触发
[ScriptEvent("onManualDefibMinigameShock")]
public class ManualDefibMinigameShockEvent : BarkEvent
{
    // 接受除颤的肢体
    public Limb Limb { get; set; } = null!;

    // 本次放电的电量
    public float Charge { get; set; }
}

// 手动除颤小游戏结束事件：电池耗尽小游戏结束时触发
[ScriptEvent("onManualDefibMinigameEnd")]
public class ManualDefibMinigameEndEvent : BarkEvent
{
    // 接受除颤的肢体
    public Limb Limb { get; set; } = null!;
}

// 取弹片小游戏事件
// 取弹片小游戏开始事件：玩家开始取出肢体弹片时触发
[ScriptEvent("onShrapnelMinigameStart")]
public class ShrapnelMinigameStartEvent : BarkEvent
{
    // 目标肢体
    public Limb Limb { get; set; } = null!;

    // 是否使用镊子
    public bool HasTweezers { get; set; }
}

// 取弹片成功事件：所有弹片被取出时触发
[ScriptEvent("onShrapnelMinigameSuccess")]
public class ShrapnelMinigameSuccessEvent : BarkEvent
{
    // 目标肢体
    public Limb Limb { get; set; } = null!;
}

// 取弹片失败事件：夹碎弹片导致伤口加深时触发
[ScriptEvent("onShrapnelMinigameFail")]
public class ShrapnelMinigameFailEvent : BarkEvent
{
    // 目标肢体
    public Limb Limb { get; set; } = null!;
}

// 注射小游戏事件
// 注射小游戏开始事件：玩家开始注射时触发
[ScriptEvent("onSyringeMinigameStart")]
public class SyringeMinigameStartEvent : BarkEvent
{
    // 目标肢体
    public Limb Limb { get; set; } = null!;
}

// 注射推进事件：注射器推入药液时触发
[ScriptEvent("onSyringeMinigameInject")]
public class SyringeMinigameInjectEvent : BarkEvent
{
    // 目标肢体
    public Limb Limb { get; set; } = null!;
}

// 注射失败事件：注射位置偏移扎碎弹片时触发
[ScriptEvent("onSyringeMinigameFail")]
public class SyringeMinigameFailEvent : BarkEvent
{
    // 目标肢体
    public Limb Limb { get; set; } = null!;
}

// 截肢小游戏事件
// 截肢小游戏开始事件：玩家开始切除肢体时触发
[ScriptEvent("onAmputationMinigameStart")]
public class AmputationMinigameStartEvent : BarkEvent
{
    // 被切除的肢体
    public Limb Limb { get; set; } = null!;
}

// 截肢完成事件：肢体被完全切断（Dismember）时触发
[ScriptEvent("onAmputationMinigameSuccess")]
public class AmputationMinigameSuccessEvent : BarkEvent
{
    // 被切断的肢体
    public Limb Limb { get; set; } = null!;
}