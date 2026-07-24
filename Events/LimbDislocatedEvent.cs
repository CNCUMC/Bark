using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onLimbDislocated")]
public class LimbDislocatedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}
