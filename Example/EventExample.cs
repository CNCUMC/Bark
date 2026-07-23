using Bark.Event;
using Bark.Events;
using Bark.Tool;

namespace Bark.Example;

[EventBusSubscriber(Plugin.Guid)]
public class EventExample
{
    // [SubscribeEvent]
    // public static void OnPlayerJump(PlayerEvents.JumpEvent eve)
    // {
    //     LogUtil.Debug("Player jump!");
    // }
    
    [SubscribeEvent]
    public static void OnPlayerJumpFull(PlayerEvents.JumpFullEvent eve)
    {
        LogUtil.Debug("Player jump full!");
    }
}