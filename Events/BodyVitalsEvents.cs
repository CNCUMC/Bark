using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onBodyCardiacArrest")]
public class BodyCardiacArrestEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
    public bool IsCardiacArrest { get; set; }
}

[ScriptEvent("onBodyFibrillationStart")]
public class BodyFibrillationStartEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

[ScriptEvent("onBodyFibrillationEnd")]
public class BodyFibrillationEndEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

[ScriptEvent("onBodyBreathChange")]
public class BodyBreathChangeEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
    public bool IsBreathing { get; set; }
}

[ScriptEvent("onBodyConsciousnessChange")]
public class BodyConsciousnessChangeEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
    public bool IsConscious { get; set; }
}

[ScriptEvent("onBodyBrainDying")]
public class BodyBrainDyingEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
    public bool IsBrainDying { get; set; }
}
