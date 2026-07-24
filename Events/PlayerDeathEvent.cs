using Bark.Event;

namespace Bark.Events;

// 死亡事件
[ScriptEvent("onPlayerDeath")]
public class PlayerDeathEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}
