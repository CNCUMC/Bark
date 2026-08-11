using Bark.Event;

namespace Bark.Events;

// 肢体骨折事件：肢体骨头断裂时触发
[ScriptEvent("onLimbBroken")]
public class LimbBrokenEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}

// 肢体脱臼事件：肢体关节脱位时触发
[ScriptEvent("onLimbDislocated")]
public class LimbDislocatedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}

// 肢体截断事件：肢体被完全截断（离断）时触发
[ScriptEvent("onLimbDismembered")]
public class LimbDismemberedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}

// 肢体感染事件：肢体出现感染时触发
[ScriptEvent("onLimbInfected")]
public class LimbInfectedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}

// 肢体接骨事件：肢体骨头被接回时触发
[ScriptEvent("onLimbMended")]
public class LimbMendedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}

// 肢体复位事件：脱臼的肢体被复位时触发
[ScriptEvent("onLimbUnDislocated")]
public class LimbUnDislocatedEvent : BarkEvent
{
    public int LimbIndex { get; set; }
    public string LimbName { get; set; } = string.Empty;
}
