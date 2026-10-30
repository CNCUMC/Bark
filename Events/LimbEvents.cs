using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onLimbBroken")]
public class LimbBrokenEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}

[ScriptEvent("onLimbDislocated")]
public class LimbDislocatedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}

[ScriptEvent("onLimbDismembered")]
public class LimbDismemberedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}

[ScriptEvent("onLimbInfected")]
public class LimbInfectedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}

[ScriptEvent("onLimbMended")]
public class LimbMendedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}

[ScriptEvent("onLimbUnDislocated")]
public class LimbUnDislocatedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}
