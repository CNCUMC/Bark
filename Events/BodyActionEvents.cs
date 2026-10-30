using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onBodyClimbStart")]
public class BodyClimbStartEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

[ScriptEvent("onBodyClimbEnd")]
public class BodyClimbEndEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

[ScriptEvent("onBodyExerciseStart")]
public class BodyExerciseStartEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

[ScriptEvent("onBodyExerciseEnd")]
public class BodyExerciseEndEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

[ScriptEvent("onBodySwitchHands")]
public class BodySwitchHandsEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
}

[ScriptEvent("onBodySwitchDir")]
public class BodySwitchDirEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
    public bool IsRight { get; set; }
}

[ScriptEvent("onBodyCrouchChange")]
public class BodyCrouchChangeEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
    public bool IsCrouching { get; set; }
}

[ScriptEvent("onBodyPickUp")]
public class BodyPickUpEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
    public string ItemId { get; set; } = string.Empty;
    public int Slot { get; set; }
}

[ScriptEvent("onBodyDrop")]
public class BodyDropEvent : BarkEvent
{
    public Body Body { get; set; } = null!;
    public PlayerCamera Camera { get; set; } = null!;
    public string ItemId { get; set; } = string.Empty;
}
