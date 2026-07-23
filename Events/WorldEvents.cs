using System.Collections;
using Bark.Event;
using Bark.Tool;
using CUCoreLib.Helpers;
using UnityEngine;

namespace Bark.Events;

public static class WorldEvents
{
    private static bool _triggered;

    [ScriptEvent("onWorldGenerated")]
    public class GeneratedWorldEvent : BarkEvent
    {
        public WorldGeneration World { get; set; } = WorldGeneration.world;
    };
    
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
        EventUtil.Trigger(new GeneratedWorldEvent());
        _triggered = true;
    }
}