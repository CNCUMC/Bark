using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onPlayerDeath")]
public class PlayerDeathEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

[ScriptEvent("onPlayerJumpStart")]
public class PlayerJumpStartEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

[ScriptEvent("onPlayerJumpOver")]
public class PlayerJumpOverEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}
