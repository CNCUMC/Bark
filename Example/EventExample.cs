using Bark.Event;
using Bark.Events;
using Bark.Tool;

namespace Bark.Example;

[EventBusSubscriber(Plugin.Guid)]
public class EventExample
{
    [SubscribeEvent]
    public static void OnPlayerJumpOver(PlayerEvents.JumpOverEvent eve)
    {
        LogUtil.Debug("Player jump over!");
    }
    
    [SubscribeEvent]
    public static void OnPlayerJumpStart(PlayerEvents.JumpStartEvent eve)
    {
        LogUtil.Debug("Player jump start!");
    }
}