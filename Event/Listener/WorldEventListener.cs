using System.Collections;
using Bark.Events;
using Bark.Tool;
using CUCoreLib.Helpers;
using UnityEngine;

namespace Bark.Event.Listener;

public static class WorldEventListener
{
    private static bool _triggered;

    internal static void Listen(MonoBehaviour runner)
    {
        runner.StartCoroutine(WaitForWorldGeneration());
    }

    private static IEnumerator WaitForWorldGeneration()
    {
        yield return CUCoreUtils.AwaitWorldGeneration();
        Trigger();
    }

    private static void Trigger()
    {
        if (_triggered) return;
        EventUtil.Trigger(new WorldReadyEvent());
        _triggered = true;
    }
}
