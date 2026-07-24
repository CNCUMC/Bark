using Bark.Event;

namespace Bark.Events;

// 跳跃结束事件：落地时触发（起跳 -> 滞空 -> 落地 的完整过程）
[ScriptEvent("onPlayerJumpOver")]
public class PlayerJumpOverEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}
