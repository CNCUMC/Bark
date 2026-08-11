using Bark.Event;

namespace Bark.Events;

// 死亡事件：玩家死亡（brainHealth <= 0）时触发
[ScriptEvent("onPlayerDeath")]
public class PlayerDeathEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

// 起跳事件：按下跳跃键时触发
[ScriptEvent("onPlayerJumpStart")]
public class PlayerJumpStartEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

// 跳跃结束事件：落地时触发（起跳 -> 滞空 -> 落地 的完整过程）
[ScriptEvent("onPlayerJumpOver")]
public class PlayerJumpOverEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}
