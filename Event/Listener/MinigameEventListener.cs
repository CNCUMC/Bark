using Bark.Events;
using Bark.Tool;
using HarmonyLib;

namespace Bark.Event.Listener;

public static class MinigameEventListener
{
    private static int _lastAedState = -1;
    private static int _lastDislocationReset = -1;
    private static bool _lockpickDone;
    private static bool _lockpickStuck;
    private static bool _manualDefibDone;
    private static int _lastManualDefibShocks;
    private static bool _shrapnelDone;
    private static bool _syringeDone;
    private static bool _amputationDone;

    internal static void Listen()
    {
    }

    internal static void Stop()
    {
        _lastAedState = -1;
        _lastDislocationReset = -1;
        _lockpickDone = false;
        _lockpickStuck = false;
        _manualDefibDone = false;
        _lastManualDefibShocks = 0;
        _shrapnelDone = false;
        _syringeDone = false;
        _amputationDone = false;
    }

    private static Limb? GetDislocationLimb(DislocationMinigame instance)
    {
        return Traverse.Create(instance).Field("limb").GetValue<Limb>();
    }

    private static Limb? GetShrapnelLimb(ShrapnelMinigame instance)
    {
        return Traverse.Create(instance).Field("limb").GetValue<Limb>();
    }

    private static Limb? GetSyringeLimb(SyringeMinigame instance)
    {
        return Traverse.Create(instance).Field("limb").GetValue<Limb>();
    }

    private static int GetLimbIndex(Limb limb)
    {
        if (limb.body == null || limb.body.limbs == null) return -1;
        for (var i = 0; i < limb.body.limbs.Length; i++)
            if (limb.body.limbs[i] == limb)
                return i;
        return -1;
    }

    [HarmonyPatch(typeof(AEDMinigame))]
    public static class AEDMinigamePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPostfix(AEDMinigame __instance)
        {
            if (__instance.limb == null) return;

            _lastAedState = 0;
            EventUtil.Trigger(new AEDMinigameStartEvent
            {
                Limb = __instance.limb,
                LimbIndex = GetLimbIndex(__instance.limb)
            });
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(AEDMinigame __instance)
        {
            if (__instance.limb == null) return;

            var state = Traverse.Create(__instance).Field("state").GetValue<int>();
            if (state == _lastAedState) return;

            var prev = _lastAedState;
            _lastAedState = state;

            switch (state)
            {
                case 4 when prev != 4:
                    EventUtil.Trigger(new AEDMinigameDefibrillateEvent
                    {
                        Limb = __instance.limb,
                        WasFibrillating = __instance.limb.body.fibrillationProgress > 0f
                    });
                    break;
                case 5 when prev != 5:
                    EventUtil.Trigger(new AEDMinigameFailEvent { Limb = __instance.limb });
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(BandageMinigame))]
    public static class BandageMinigamePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPostfix(BandageMinigame __instance)
        {
            if (__instance.limb == null) return;

            EventUtil.Trigger(new BandageMinigameStartEvent
            {
                Limb = __instance.limb,
                BandageAngle = __instance.bandageAngle
            });
        }

        [HarmonyPatch("DoBandageAction")]
        [HarmonyPostfix]
        private static void DoBandageActionPostfix(BandageMinigame __instance)
        {
            if (__instance.limb == null) return;

            EventUtil.Trigger(new BandageMinigameWrapEvent { Limb = __instance.limb });
        }
    }

    [HarmonyPatch(typeof(DislocationMinigame))]
    public static class DislocationMinigamePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPostfix(DislocationMinigame __instance)
        {
            var limb = GetDislocationLimb(__instance);
            if (limb == null) return;

            EventUtil.Trigger(new DislocationMinigameStartEvent
            {
                Limb = limb,
                HasWrench = __instance.hasWrench
            });
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(DislocationMinigame __instance)
        {
            var limb = GetDislocationLimb(__instance);
            if (limb == null) return;

            var timer = limb.dislocationTimer;
            var reset = timer < 3f;

            var id = __instance.GetHashCode();
            switch (reset)
            {
                case true when _lastDislocationReset != id:
                    _lastDislocationReset = id;
                    EventUtil.Trigger(new DislocationMinigameSuccessEvent { Limb = limb });
                    break;
                case false:
                    _lastDislocationReset = -1;
                    break;
            }
        }
    }


    [HarmonyPatch(typeof(HandCrankMinigame))]
    public static class HandCrankMinigamePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPostfix()
        {
            EventUtil.Trigger(new HandCrankMinigameStartEvent());
        }

        [HarmonyPatch("PhysicsUpdate")]
        [HarmonyPostfix]
        private static void PhysicsUpdatePostfix(HandCrankMinigame __instance)
        {
            var held = Traverse.Create(__instance).Field("held").GetValue<bool>();
            if (!held) return;

            EventUtil.Trigger(new HandCrankMinigameChargeEvent { Angle = 0f });
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static void UpdatePrefix()
        {
            if (Minigame.game != null && Minigame.game.body != null && Minigame.game.body.stamina < 15f)
                EventUtil.Trigger(new HandCrankMinigameEndEvent());
        }
    }

    [HarmonyPatch(typeof(KeypadMinigame))]
    public static class KeypadMinigamePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPostfix(KeypadMinigame __instance)
        {
            if (__instance.toDestroy == null) return;
            EventUtil.Trigger(new KeypadMinigameStartEvent { ToDestroy = __instance.toDestroy });
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(KeypadMinigame __instance)
        {
            if (__instance.toDestroy == null) return;

            if (__instance.current == __instance.match)
                EventUtil.Trigger(new KeypadMinigameSuccessEvent { ToDestroy = __instance.toDestroy });
        }
    }

    [HarmonyPatch(typeof(LockpingMinigame))]
    public static class LockpingMinigamePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPostfix(LockpingMinigame __instance)
        {
            if (__instance.toDestroy == null) return;

            _lockpickDone = false;
            _lockpickStuck = false;
            EventUtil.Trigger(new LockpingMinigameStartEvent
            {
                ToDestroy = __instance.toDestroy,
                HasPick = __instance.pickLevel >= 0
            });
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(LockpingMinigame __instance)
        {
            if (__instance.toDestroy == null) return;

            if (__instance.toDestroy.health <= 0f)
            {
                if (_lockpickDone) return;
                _lockpickDone = true;
                EventUtil.Trigger(new LockpingMinigameSuccessEvent { ToDestroy = __instance.toDestroy });
                return;
            }

            var stuck = Traverse.Create(__instance).Field("timeWasStuck").GetValue<float>();
            if (stuck > 0.5f)
            {
                if (_lockpickStuck) return;
                _lockpickStuck = true;
                EventUtil.Trigger(new LockpingMinigameStuckEvent { ToDestroy = __instance.toDestroy });
            }
            else
            {
                _lockpickStuck = false;
            }
        }
    }

    [HarmonyPatch(typeof(ManualDefibMinigame))]
    public static class ManualDefibMinigamePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPostfix(ManualDefibMinigame __instance)
        {
            if (__instance.limb == null) return;

            _manualDefibDone = false;
            EventUtil.Trigger(new ManualDefibMinigameStartEvent
            {
                Limb = __instance.limb,
                OnTorso = Traverse.Create(__instance).Field("onTorso").GetValue<bool>()
            });
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(ManualDefibMinigame __instance)
        {
            if (__instance.limb == null) return;

            var shockCount = Traverse.Create(__instance).Field("shockCount").GetValue<int>();
            if (shockCount <= _lastManualDefibShocks) return;
            _lastManualDefibShocks = shockCount;
            var charge = Traverse.Create(__instance).Field("currentCharge").GetValue<float>();
            EventUtil.Trigger(new ManualDefibMinigameShockEvent { Limb = __instance.limb, Charge = charge });
        }

        [HarmonyPatch("PhysicsUpdate")]
        [HarmonyPrefix]
        private static void PhysicsUpdatePrefix(ManualDefibMinigame __instance)
        {
            if (__instance.limb == null) return;
            if (_manualDefibDone) return;

            if (Minigame.game == null
                || Minigame.game.currentItem == null
                || Minigame.game.currentItem.battery == null
                || Minigame.game.currentItem.battery.hasCharge) return;
            _manualDefibDone = true;
            EventUtil.Trigger(new ManualDefibMinigameEndEvent { Limb = __instance.limb });
        }
    }

    [HarmonyPatch(typeof(ShrapnelMinigame))]
    public static class ShrapnelMinigamePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPostfix(ShrapnelMinigame __instance)
        {
            var limb = GetShrapnelLimb(__instance);
            if (limb == null) return;

            _shrapnelDone = false;
            EventUtil.Trigger(new ShrapnelMinigameStartEvent
            {
                Limb = limb,
                HasTweezers = __instance.hasTweezers
            });
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(ShrapnelMinigame __instance)
        {
            var limb = GetShrapnelLimb(__instance);
            if (limb == null) return;

            if (limb.shrapnel != 0 || _shrapnelDone) return;
            _shrapnelDone = true;
            EventUtil.Trigger(new ShrapnelMinigameSuccessEvent { Limb = limb });
        }

        [HarmonyPatch("BreakGrasp")]
        [HarmonyPrefix]
        private static void BreakGraspPrefix(ShrapnelMinigame __instance)
        {
            var limb = GetShrapnelLimb(__instance);
            if (limb == null) return;

            EventUtil.Trigger(new ShrapnelMinigameFailEvent { Limb = limb });
        }
    }

    [HarmonyPatch(typeof(SyringeMinigame))]
    public static class SyringeMinigamePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPostfix(SyringeMinigame __instance)
        {
            var limb = GetSyringeLimb(__instance);
            if (limb == null) return;

            _syringeDone = false;
            EventUtil.Trigger(new SyringeMinigameStartEvent { Limb = limb });
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(SyringeMinigame __instance)
        {
            var limb = GetSyringeLimb(__instance);
            if (limb == null) return;

            var injecting = Traverse.Create(__instance).Field("wasInjectingBefore").GetValue<bool>();
            if (injecting && !_syringeDone)
            {
                _syringeDone = true;
                EventUtil.Trigger(new SyringeMinigameInjectEvent { Limb = limb });
            }

            if (limb.shrapnel > 0)
                EventUtil.Trigger(new SyringeMinigameFailEvent { Limb = limb });
        }
    }

    [HarmonyPatch(typeof(AmputationMinigame))]
    public static class AmputationMinigamePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPostfix(AmputationMinigame __instance)
        {
            if (__instance.limb == null) return;

            _amputationDone = false;
            EventUtil.Trigger(new AmputationMinigameStartEvent { Limb = __instance.limb });
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(AmputationMinigame __instance)
        {
            if (__instance.limb == null) return;

            if (!(__instance.cutProgress >= 1f) || _amputationDone) return;
            _amputationDone = true;
            EventUtil.Trigger(new AmputationMinigameSuccessEvent { Limb = __instance.limb });
        }
    }
}