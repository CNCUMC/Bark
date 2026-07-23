using Bark.Event;
using Bark.Events;
using Bark.Tool;

namespace Bark.Example;

[EventBusSubscriber(Plugin.Guid)]
public class EventExample
{
    [SubscribeEvent]
    public static void OnMainMenuLoaded(MainMenuEvents.LoadedEvent eve)
    {
        LogUtil.Debug("Main menu loaded!");
    }
    
    [SubscribeEvent]
    public static void OnPlayerJumpFull(PlayerEvents.JumpFullEvent eve)
    {
        LogUtil.Debug("Player jump full!" + eve.Camera.blackAmount);
    }
}