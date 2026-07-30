using Bark.Event;
using Bark.Events;
using Bark.Tool;

namespace Bark.Example;

[EventBusSubscriber(Plugin.Guid)]
public class EventHandlers
{
    private static bool _updateChecked;

    public static void OnMainMenuLoaded(MainMenuLoadedEvent eve)
    {
        if (_updateChecked)
            return;
        UpdateUtil.Check("CNCUMC/Bark", Plugin.Name, Plugin.Version, Plugin.Logger);
        _updateChecked = true;
    }
}