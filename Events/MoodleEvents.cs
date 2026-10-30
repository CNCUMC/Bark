using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onMoodleGet")]
public class MoodleGetEvent : BarkEvent
{
    public string MoodleKey { get; set; } = string.Empty;
    public string MoodleName { get; set; } = string.Empty;
    public int Intensity { get; set; }
    public bool Critical { get; set; }
    public float HoldSeconds { get; set; }
}

[ScriptEvent("onMoodleIterate")]
public class MoodleIterateEvent : BarkEvent
{
    public string[] ActiveKeys { get; set; } = [];
}

[ScriptEvent("onMoodleLose")]
public class MoodleLoseEvent : BarkEvent
{
    public string MoodleKey { get; set; } = string.Empty;
    public string MoodleName { get; set; } = string.Empty;
}
