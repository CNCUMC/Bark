using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onLimbMended")]
public class LimbMendedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}
