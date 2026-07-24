using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onLimbDismembered")]
public class LimbDismemberedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}
