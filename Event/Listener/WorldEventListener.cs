using System.Collections;
using Bark.Events;
using Bark.Tool;
using CUCoreLib.Helpers;
using UnityEngine;

namespace Bark.Event.Listener;

public static class WorldEventListener
{
    internal static void Listen(MonoBehaviour runner)
    {
        runner.StartCoroutine(WaitForWorldGeneration());
    }

    private static IEnumerator WaitForWorldGeneration()
    {
        yield return CUCoreUtils.AwaitWorldGeneration();
        EventUtil.Trigger(new WorldReadyEvent());
    }
}