using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onLimbInfected")]
public class LimbInfectedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}
