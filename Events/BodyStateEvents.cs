using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onBodySleepChange")]
public class BodySleepChangeEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
    public bool IsSleeping { get; set; }
}

[ScriptEvent("onBodyLastStand")]
public class BodyLastStandEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

[ScriptEvent("onBodyDisfigure")]
public class BodyDisfigureEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

[ScriptEvent("onBodyRemoveEye")]
public class BodyRemoveEyeEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
    public bool BothEyesGone { get; set; }
}
