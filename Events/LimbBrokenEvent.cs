using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onLimbBroken")]
public class LimbBrokenEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}
