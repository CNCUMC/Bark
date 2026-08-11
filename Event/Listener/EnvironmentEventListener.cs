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
        PatchCaveTickSpawner();
        PatchClimbable();
        PatchCoil();
        PatchCorpse();
    }

    internal static void Stop()
    {
        SpawnedTickSpawners.Clear();
        CommentedCorpses.Clear();
    }

    private static void PatchCaveTickSpawner()
    {
        var method = AccessTools.Method(typeof(CaveTickSpawner), "OnTriggerEnter2D");
        if (method == null) return;

        try
        {
            var harmony = new Harmony("Bark.CaveTick.Spawn");
            harmony.Patch(method,
                postfix: new HarmonyMethod(typeof(EnvironmentEventListener), nameof(OnCaveTickSpawnPostfix)));
        }
        catch
        {
            // ignored
        }
    }

    private static void PatchClimbable()
    {
        var method = AccessTools.Method(typeof(Climbable), "Start");
        if (method == null) return;

        try
        {
            var harmony = new Harmony("Bark.Climbable.Register");
            harmony.Patch(method,
                postfix: new HarmonyMethod(typeof(EnvironmentEventListener), nameof(OnClimbableRegisterPostfix)));
        }
        catch
        {
            // ignored
        }
    }

    private static void PatchCoil()
    {
        var method = AccessTools.Method(typeof(CoilScript), "Shock");
        if (method == null) return;

        try
        {
            var harmony = new Harmony("Bark.Coil.Shock");
            harmony.Patch(method,
                postfix: new HarmonyMethod(typeof(EnvironmentEventListener), nameof(OnCoilShockPostfix)));
        }
        catch
        {
            // ignored
        }
    }

    private static void PatchCorpse()
    {
        // OnWillRenderObject：首次看到尸体
        var seen = AccessTools.Method(typeof(CorpseScript), "OnWillRenderObject");
        if (seen != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Corpse.Seen");
                harmony.Patch(seen,
                    postfix: new HarmonyMethod(typeof(EnvironmentEventListener), nameof(OnCorpseSeenPostfix)));
            }
            catch
            {
                // ignored
            }
        }

        // OnDestroy：破坏尸体
        var destroy = AccessTools.Method(typeof(CorpseScript), "OnDestroy");
        if (destroy != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Corpse.Destroy");
                harmony.Patch(destroy,
                    postfix: new HarmonyMethod(typeof(EnvironmentEventListener), nameof(OnCorpseDestroyPostfix)));
            }
            catch
            {
                // ignored
            }
        }
    }

    // ============================================================
    // 洞穴蜘蛛
    // ============================================================

    private static void OnCaveTickSpawnPostfix(CaveTickSpawner __instance)
    {
        if (__instance == null || !__instance) return;

        var started = Traverse.Create(__instance).Field("started").GetValue<bool>();
        if (!started) return;

        var id = __instance.GetInstanceID();
        if (!SpawnedTickSpawners.Add(id)) return;

        EventUtil.Trigger(new CaveTickSpawnEvent { Position = __instance.transform.position });
    }

    // ============================================================
    // 可攀爬物
    // ============================================================

    private static void OnClimbableRegisterPostfix(Climbable __instance)
    {
        if (__instance == null || !__instance) return;

        EventUtil.Trigger(new ClimbableRegisterEvent
        {
            Climbable = __instance,
            TotalLength = __instance.totalLength
        });
    }

    // ============================================================
    // 电线圈
    // ============================================================

    private static void OnCoilShockPostfix(CoilScript __instance, Limb limb)
    {
        if (__instance == null || !__instance || limb == null) return;

        EventUtil.Trigger(new CoilShockEvent { Coil = __instance, Limb = limb });
    }

    // ============================================================
    // 尸体
    // ============================================================

    private static void OnCorpseSeenPostfix(CorpseScript __instance)
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

    private static void OnCorpseDestroyPostfix(CorpseScript __instance)
    {
        if (__instance == null || !__instance) return;

        // OnDestroy 中先判断 building 是否已被破坏，仅当 health<=0 才触发破坏
        var building = __instance.GetComponent<BuildingEntity>();
        if (building != null && building.health > 0f) return;

        EventUtil.Trigger(new CorpseDestroyEvent { Corpse = __instance });
    }
}
