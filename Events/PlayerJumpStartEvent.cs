using Bark.Event;

namespace Bark.Events;

// 起跳事件：按下跳跃键时触发
[ScriptEvent("onPlayerJumpStart")]
public class PlayerJumpStartEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}
