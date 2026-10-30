using System.Collections.Generic;
using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onCommand")]
public class CommandEvent : BarkEvent
{
    public string CommandName { get; set; } = string.Empty;
    public List<string> Args { get; set; } = [];
}

[ScriptEvent("onMainMenuLoaded")]
public class MainMenuLoadedEvent : BarkEvent;

[ScriptEvent("onWorldGenerated")]
public class WorldReadyEvent : BarkEvent
{
    public WorldGeneration World { get; set; } = WorldGeneration.world;
}
