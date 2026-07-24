using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onWorldGenerated")]
public class WorldReadyEvent : BarkEvent
{
    public WorldGeneration World { get; set; } = WorldGeneration.world;
}
