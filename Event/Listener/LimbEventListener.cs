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

    [HarmonyPatch(typeof(Limb))]
    public static class LimbPatch
    {
        [HarmonyPatch("BreakBone")]
        [HarmonyPrefix]
        private static void BreakBonePrefix(Limb __instance)
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

        [HarmonyPatch("MendBone")]
        [HarmonyPrefix]
        private static void MendBonePrefix(Limb __instance)
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

        [HarmonyPatch("Dislocate")]
        [HarmonyPrefix]
        private static void DislocatePrefix(Limb __instance)
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

        [HarmonyPatch("UnDislocate")]
        [HarmonyPrefix]
        private static void UnDislocatePrefix(Limb __instance)
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

        [HarmonyPatch("Dismember")]
        [HarmonyPrefix]
        private static void DismemberPrefix(Limb __instance)
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
    }

    // 感染轮询
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
        var body = BodyUtil.Body;
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

    
    // 辅助
    private static bool IsPlayerLimb(Limb limb)
    {
        return limb != null && limb.body == BodyUtil.Body;
    }

    private static int GetLimbIndex(Limb limb)
    {
        var limbs = BodyUtil.Body.limbs;
        if (limbs == null) return -1;
        return Array.IndexOf(limbs, limb);
    }
}
