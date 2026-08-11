using Bark.Event;

namespace Bark.Events;

// 心脏骤停状态变化事件：玩家进入或离开心脏骤停（inCardiacArrest，即 heartRate < 20）时触发
[ScriptEvent("onBodyCardiacArrest")]
public class BodyCardiacArrestEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;

    // 当前是否处于心脏骤停（true = 骤停，false = 恢复心跳）
    public bool IsCardiacArrest { get; set; }
}

// 心室颤动开始事件：fibrillationProgress 从 0 转为 > 0 时触发
[ScriptEvent("onBodyFibrillationStart")]
public class BodyFibrillationStartEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

// 心室颤动结束事件：fibrillationProgress 降回 0 时触发
[ScriptEvent("onBodyFibrillationEnd")]
public class BodyFibrillationEndEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

// 呼吸状态变化事件：玩家停止呼吸或恢复呼吸（breathing 翻转）时触发
[ScriptEvent("onBodyBreathChange")]
public class BodyBreathChangeEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;

    // 当前是否在呼吸（true = 呼吸，false = 呼吸停止）
    public bool IsBreathing { get; set; }
}

// 意识状态变化事件：玩家昏迷或苏醒（conscious 翻转）时触发
[ScriptEvent("onBodyConsciousnessChange")]
public class BodyConsciousnessChangeEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;

    // 当前是否清醒（true = 苏醒，false = 昏迷）
    public bool IsConscious { get; set; }
}

// 濒死状态变化事件：玩家进入或离开濒死（brainDying）时触发
[ScriptEvent("onBodyBrainDying")]
public class BodyBrainDyingEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;

    // 当前是否濒死
    public bool IsBrainDying { get; set; }
}
