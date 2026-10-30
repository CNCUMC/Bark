using Bark.Event;

namespace Bark.Script;

public static class EventScriptContext
{
    public static BarkEvent? CurrentEvent { get; internal set; }
}
