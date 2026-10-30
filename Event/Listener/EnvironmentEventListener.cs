using System.Collections.Generic;
using Bark.Events;
using Bark.Tool;
using HarmonyLib;

namespace Bark.Event.Listener;

public static class EnvironmentEventListener
{
    private static readonly HashSet<int> SpawnedTickSpawners = [];
    private static readonly HashSet<int> CommentedCorpses = [];

    internal static void Listen()
    {
    }

    internal static void Stop()
    {
        SpawnedTickSpawners.Clear();
        CommentedCorpses.Clear();
    }

    [HarmonyPatch(typeof(CaveTickSpawner), "OnTriggerEnter2D")]
    [HarmonyPostfix]
    private static void CaveTickSpawnerOnTriggerEnter2DPostfix(CaveTickSpawner __instance)
    {
        if (__instance == null || !__instance) return;

        var started = Traverse.Create(__instance).Field("started").GetValue<bool>();
        if (!started) return;

        var id = __instance.GetInstanceID();
        if (!SpawnedTickSpawners.Add(id)) return;

        EventUtil.Trigger(new CaveTickSpawnEvent { Position = __instance.transform.position });
    }


    [HarmonyPatch(typeof(Climbable), "Start")]
    [HarmonyPostfix]
    private static void ClimbableStartPostfix(Climbable __instance)
    {
        if (__instance == null || !__instance) return;

        EventUtil.Trigger(new ClimbableRegisterEvent
        {
            Climbable = __instance,
            TotalLength = __instance.totalLength
        });
    }

    [HarmonyPatch(typeof(CoilScript), "Shock")]
    [HarmonyPostfix]
    private static void CoilScriptShockPostfix(CoilScript __instance, Limb limb)
    {
        if (__instance == null || !__instance || limb == null) return;

        EventUtil.Trigger(new CoilShockEvent { Coil = __instance, Limb = limb });
    }

    [HarmonyPatch(typeof(CorpseScript), "OnWillRenderObject")]
    [HarmonyPostfix]
    private static void CorpseScriptOnWillRenderObjectPostfix(CorpseScript __instance)
    {
        if (__instance == null || !__instance) return;
        if (__instance.animalCorpse) return;

        var didComment = Traverse.Create(__instance).Field("didComment").GetValue<bool>();
        if (!didComment) return;

        var id = __instance.GetInstanceID();
        if (!CommentedCorpses.Add(id)) return;

        EventUtil.Trigger(new CorpseSeenEvent { Corpse = __instance, AnimalCorpse = false });
    }

    [HarmonyPatch(typeof(CorpseScript), "OnDestroy")]
    private static void CorpseScriptOnDestroyPostfix(CorpseScript __instance)
    {
        if (__instance == null || !__instance) return;

        var building = __instance.GetComponent<BuildingEntity>();
        if (building != null && building.health > 0f) return;

        EventUtil.Trigger(new CorpseDestroyEvent { Corpse = __instance });
    }
}