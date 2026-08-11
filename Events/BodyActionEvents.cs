using Bark.Event;

namespace Bark.Events;

// 开始攀爬事件：玩家开始攀爬（Body.StartClimbing）时触发
[ScriptEvent("onBodyClimbStart")]
public class BodyClimbStartEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

// 结束攀爬事件：玩家停止攀爬（Body.StopClimbing）时触发
[ScriptEvent("onBodyClimbEnd")]
public class BodyClimbEndEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

// 开始锻炼事件：玩家开始锻炼（exercising 变 true）时触发
[ScriptEvent("onBodyExerciseStart")]
public class BodyExerciseStartEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

// 结束锻炼事件：玩家停止锻炼（exercising 变 false）时触发
[ScriptEvent("onBodyExerciseEnd")]
public class BodyExerciseEndEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

// 切换手持物品事件：玩家交换左右手物品（Body.SwitchHands）时触发
[ScriptEvent("onBodySwitchHands")]
public class BodySwitchHandsEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

// 切换朝向事件：玩家转身（Body.SwitchDir）时触发
[ScriptEvent("onBodySwitchDir")]
public class BodySwitchDirEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;

    // 当前朝向（true = 朝右，false = 朝左）
    public bool IsRight { get; set; }
}

// 下蹲状态变化事件：玩家开始或停止下蹲（crouching 翻转）时触发
[ScriptEvent("onBodyCrouchChange")]
public class BodyCrouchChangeEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;

    // 当前是否在下蹲
    public bool IsCrouching { get; set; }
}

// 拾取物品事件：玩家拾起物品（Body.PickUpItem）时触发
[ScriptEvent("onBodyPickUp")]
public class BodyPickUpEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;

    // 拾起的物品 ID
    public string ItemId { get; set; } = string.Empty;

    // 放入的槽位索引
    public int Slot { get; set; }
}

// 丢弃物品事件：玩家丢弃物品（Body.DropItem）时触发
[ScriptEvent("onBodyDrop")]
public class BodyDropEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;

    // 丢弃的物品 ID（可能为空）
    public string ItemId { get; set; } = string.Empty;
}
