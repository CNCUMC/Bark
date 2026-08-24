using Bark.Event;

namespace Bark.Events;

// 睡眠状态变化事件：玩家入睡或醒来（sleeping 翻转）时触发
[ScriptEvent("onBodySleepChange")]
public class BodySleepChangeEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;

    // 当前是否在睡觉
    public bool IsSleeping { get; set; }
}

// 最后坚持事件：玩家成功触发最后坚持（Body.TryLastStand 成功）时触发
[ScriptEvent("onBodyLastStand")]
public class BodyLastStandEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

// 毁容事件：玩家被毁容（Body.Disfigure）时触发
[ScriptEvent("onBodyDisfigure")]
public class BodyDisfigureEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

// 失去眼睛事件：玩家失去一只或双眼（Body.RemoveEye）时触发
[ScriptEvent("onBodyRemoveEye")]
public class BodyRemoveEyeEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;

    // 当前双眼是否都已失去
    public bool BothEyesGone { get; set; }
}