using Bark.Event;
using Bark.Events;
using Bark.Tool;

namespace Bark.Example;

[EventBusSubscriber(Plugin.Guid)]
public class EventExample
{
    // public static void OnPlayerJumpStart(PlayerJumpStartEvent eve)
    // {
    //     MoodleUtil.ApplyMoodle("empty_slot_block");
    // }
}