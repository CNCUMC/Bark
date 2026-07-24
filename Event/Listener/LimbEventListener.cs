using System;
using System.Collections;
using System.Collections.Generic;
using Bark.Events;
using Bark.Tool;
using HarmonyLib;
using UnityEngine;

namespace Bark.Event.Listener;

public static class LimbEventListener
{
    private const float InfectionPollInterval = 0.5f;

    private static Coroutine? _infectionCoroutine;
    private static MonoBehaviour? _runner;
    private static readonly Dictionary<int, bool> WasInfected = new();

    // ============================================================
    // 启动 / 停止
    // ============================================================

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;

        var harmony = new Harmony("Bark.LimbEventListener");
        harmony.Patch(
            typeof(Limb).GetMethod("BreakBone"),
            new HarmonyMethod(typeof(LimbEventListener), nameof(OnBreakBone))
        );
        harmony.Patch(
            typeof(Limb).GetMethod("MendBone"),
            new HarmonyMethod(typeof(LimbEventListener), nameof(OnMendBone))
        );
        harmony.Patch(
            typeof(Limb).GetMethod("Dislocate"),
            new HarmonyMethod(typeof(LimbEventListener), nameof(OnDislocate))
        );
        harmony.Patch(
            typeof(Limb).GetMethod("UnDislocate"),
            new HarmonyMethod(typeof(LimbEventListener), nameof(OnUnDislocate))
        );
        harmony.Patch(
            typeof(Limb).GetMethod("Dismember"),
            new HarmonyMethod(typeof(LimbEventListener), nameof(OnDismember))
        );

        _infectionCoroutine = runner.StartCoroutine(MonitorInfection());
    }

    internal static void Stop()
    {
        if (_infectionCoroutine != null && _runner != null)
        {
            _runner.StopCoroutine(_infectionCoroutine);
            _infectionCoroutine = null;
        }

        WasInfected.Clear();
        _runner = null;
    }

    // ============================================================
    // Harmony 前缀钩子
    // ============================================================

    private static void OnBreakBone(Limb __instance)
    {
        if (!IsPlayerLimb(__instance) || __instance.broken) return;
        var idx = GetLimbIndex(__instance);
        if (idx < 0) return;
        EventUtil.Trigger(new LimbBrokenEvent
        {
            LimbIndex = idx,
            LimbName = __instance.fullName ?? string.Empty
        });
    }

    private static void OnMendBone(Limb __instance)
    {
        if (!IsPlayerLimb(__instance) || !__instance.broken) return;
        var idx = GetLimbIndex(__instance);
        if (idx < 0) return;
        EventUtil.Trigger(new LimbMendedEvent
        {
            LimbIndex = idx,
            LimbName = __instance.fullName ?? string.Empty
        });
    }

    private static void OnDislocate(Limb __instance)
    {
        if (!IsPlayerLimb(__instance) || __instance.dislocated) return;
        var idx = GetLimbIndex(__instance);
        if (idx < 0) return;
        EventUtil.Trigger(new LimbDislocatedEvent
        {
            LimbIndex = idx,
            LimbName = __instance.fullName ?? string.Empty
        });
    }

    private static void OnUnDislocate(Limb __instance)
    {
        if (!IsPlayerLimb(__instance) || !__instance.dislocated) return;
        var idx = GetLimbIndex(__instance);
        if (idx < 0) return;
        EventUtil.Trigger(new LimbUnDislocatedEvent
        {
            LimbIndex = idx,
            LimbName = __instance.fullName ?? string.Empty
        });
    }

    private static void OnDismember(Limb __instance)
    {
        if (!IsPlayerLimb(__instance) || __instance.dismembered) return;
        var idx = GetLimbIndex(__instance);
        if (idx < 0) return;
        EventUtil.Trigger(new LimbDismemberedEvent
        {
            LimbIndex = idx,
            LimbName = __instance.fullName ?? string.Empty
        });
    }

    // ============================================================
    // 感染轮询
    // ============================================================

    private static IEnumerator MonitorInfection()
    {
        while (_infectionCoroutine != null)
        {
            yield return new WaitForSeconds(InfectionPollInterval);
            PollInfection();
        }
    }

    private static void PollInfection()
    {
        var body = PlayerUtil.Body;
        if (!body || body.limbs == null) return;

        for (var i = 0; i < body.limbs.Length; i++)
        {
            var limb = body.limbs[i];
            if (!limb || limb.dismembered) continue;

            var id = limb.GetInstanceID();
            var wasInfected = WasInfected.TryGetValue(id, out var prev) && prev;
            WasInfected[id] = limb.infected;

            if (!wasInfected && limb.infected)
                EventUtil.Trigger(new LimbInfectedEvent
                {
                    LimbIndex = i,
                    LimbName = limb.fullName ?? string.Empty
                });
        }
    }

    // ============================================================
    // 辅助
    // ============================================================

    private static bool IsPlayerLimb(Limb limb)
    {
        return limb != null && limb.body == PlayerUtil.Body;
    }

    private static int GetLimbIndex(Limb limb)
    {
        var limbs = PlayerUtil.Body.limbs;
        if (limbs == null) return -1;
        return Array.IndexOf(limbs, limb);
    }
}
