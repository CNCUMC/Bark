using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onLimbUnDislocated")]
public class LimbUnDislocatedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}
