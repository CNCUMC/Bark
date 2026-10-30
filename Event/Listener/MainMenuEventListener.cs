using System.Collections;
using Bark.Events;
using Bark.Tool;
using CUCoreLib.Helpers;
using UnityEngine;

namespace Bark.Event.Listener;

public static class MainMenuEventListener
{
    private static bool _triggered;

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
        EventUtil.Trigger(new MainMenuLoadedEvent());
        _triggered = true;
    }
}
