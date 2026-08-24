using System.Collections.Generic;
using Bark.Events;
using Bark.Tool;
using HarmonyLib;

namespace Bark.Event.Listener;

// 环境事件监听器：通过 Harmony 补丁拦截洞穴蜘蛛生成器、可攀爬物、电线圈、
// 尸体等环境组件方法，触发对应事件。
public static class EnvironmentEventListener
{
    // 洞穴蜘蛛生成器已触发生成的实例（避免重复触发）
    private static readonly HashSet<int> SpawnedTickSpawners = [];

    // 尸体已触发"首次看见"评论的实例
    private static readonly HashSet<int> CommentedCorpses = [];

    internal static void Listen()
    {
    }

    internal static void Stop()
    {
        SpawnedTickSpawners.Clear();
        CommentedCorpses.Clear();
    }

    // 洞穴蜱虫
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


    // 可攀爬物
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

    // 线圈
    [HarmonyPatch(typeof(CoilScript), "Shock")]
    [HarmonyPostfix]
    private static void CoilScriptShockPostfix(CoilScript __instance, Limb limb)
    {
        if (__instance == null || !__instance || limb == null) return;

        EventUtil.Trigger(new CoilShockEvent { Coil = __instance, Limb = limb });
    }

    // 尸体
    [HarmonyPatch(typeof(CorpseScript), "OnWillRenderObject")]
    [HarmonyPostfix]
    private static void CorpseScriptOnWillRenderObjectPostfix(CorpseScript __instance)
    {
        if (__instance == null || !__instance) return;
        if (__instance.animalCorpse) return;

        // 只在 didComment 从 false → true（首次评论）时触发
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

        // OnDestroy 中先判断 building 是否已被破坏，仅当 health<=0 才触发破坏
        var building = __instance.GetComponent<BuildingEntity>();
        if (building != null && building.health > 0f) return;

        EventUtil.Trigger(new CorpseDestroyEvent { Corpse = __instance });
    }
}