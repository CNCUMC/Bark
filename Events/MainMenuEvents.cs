using System.Collections;
using Bark.Event;
using Bark.Tool;
using CUCoreLib.Helpers;
using UnityEngine;

namespace Bark.Events;

public static class MainMenuEvents
{
    private static bool _triggered;

    [ScriptEvent("onMainMenuLoaded")]
    public class LoadedEvent : BarkEvent;

    internal static void Listen(MonoBehaviour runner)
    {
        runner.StartCoroutine(WaitForMainMenu());
    }

    private static IEnumerator WaitForMainMenu()
    {
        yield return CUCoreUtils.AwaitMainMenu();
        Trigger();
    }

    private static void Trigger()
    {
        if (_triggered) return;
        EventUtil.Trigger(new LoadedEvent());
        _triggered = true;
    }
}
