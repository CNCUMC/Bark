using System.Collections.Generic;
using Bark.Events;
using Bark.Tool;
using HarmonyLib;
using UnityEngine;

namespace Bark.Event.Listener;

public static class WorldObjectEventListener
{
    private static readonly HashSet<int> TeleportedDrillPods = [];
    private static readonly Dictionary<int, int> ThornbackStages = new();
    private static readonly HashSet<int> GrabbedPlants = [];
    private static readonly Dictionary<int, bool> HookPullingStates = new();
    private static readonly Dictionary<int, float> JumpPadActive = new();
    private static readonly HashSet<int> OpiateOverdosed = [];
    private static readonly Dictionary<int, bool> AltHoverActive = new();

    internal static void Listen()
    {
    }

    internal static void Stop()
    {
        TeleportedDrillPods.Clear();
        ThornbackStages.Clear();
        GrabbedPlants.Clear();
        HookPullingStates.Clear();
        JumpPadActive.Clear();
        OpiateOverdosed.Clear();
        AltHoverActive.Clear();
    }

    [HarmonyPatch(typeof(Damageable), "Damage")]
    [HarmonyPostfix]
    private static void DamageableDamagePostfix(Damageable __instance, float damage)
    {
        if (__instance == null || !__instance) return;

        EventUtil.Trigger(new DamageableDamagedEvent
            { Damageable = __instance, Damage = damage });
    }

    [HarmonyPatch(typeof(DamagingCrate), "OnCollisionEnter2D")]
    [HarmonyPrefix]
    private static void DamagingCrateOnCollisionEnter2DPrefix(DamagingCrate __instance)
    {
        if (__instance == null || !__instance) return;

        EventUtil.Trigger(new DamagingCrateHitEvent
            { Crate = __instance, Type = __instance.type });
    }

    [HarmonyPatch(typeof(EPdaScript), "Use")]
    [HarmonyPrefix]
    private static void EPdaScriptUsePrefix(EPdaScript __instance)
    {
        if (__instance == null || !__instance) return;

        var item = __instance.GetComponent<Item>();
        EventUtil.Trigger(new PdaUseEvent
        {
            Pda = item,
            FirstRead = !__instance.hasBeenRead
        });
    }

    [HarmonyPatch(typeof(GlobalDark), "Darken")]
    [HarmonyPostfix]
    private static void GlobalDarkDarkenPostfix(GlobalDark __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new GlobalDarkEvent { Darkening = __instance.IsDarkening() });
    }

    [HarmonyPatch(typeof(GrabberPlant), "Update")]
    [HarmonyPostfix]
    private static void GrabberPlantUpdatePostfix(GrabberPlant __instance)
    {
        if (__instance == null || !__instance) return;

        var grabBody = Traverse.Create(__instance).Field("grabBody").GetValue();
        var id = __instance.GetInstanceID();
        var grabbed = grabBody != null;

        switch (grabbed)
        {
            case true when GrabbedPlants.Add(id):
                EventUtil.Trigger(new GrabberPlantGrabEvent { Plant = __instance });
                break;
            case false:
                GrabbedPlants.Remove(id);
                break;
        }
    }

    [HarmonyPatch(typeof(Item), "OnDestroy")]
    [HarmonyPostfix]
    private static void ItemOnDestroyPostfix(Item __instance)
    {
        if (__instance == null || !__instance) return;
        if (__instance.condition > 0f) return;

        EventUtil.Trigger(new ItemDestroyEvent
        {
            ItemId = __instance.id ?? string.Empty,
            Item = __instance
        });
    }

    [HarmonyPatch(typeof(JumpPadScript), "OnCollisionEnter2D")]
    [HarmonyPostfix]
    private static void JumpPadScriptOnCollisionEnter2DPostfix(JumpPadScript __instance)
    {
        if (__instance == null || !__instance) return;

        var id = __instance.GetInstanceID();
        var cooldown = Traverse.Create(__instance).Field("cooldown").GetValue<float>();
        JumpPadActive.TryGetValue(id, out var prevCooldown);
        JumpPadActive[id] = cooldown;

        if (prevCooldown < 15f && cooldown >= 15f)
            EventUtil.Trigger(new JumpPadBounceEvent { Pad = __instance });
    }

    [HarmonyPatch(typeof(LifepodButton), "OnUse")]
    [HarmonyPostfix]
    private static void LifepodButtonOnUsePostfix(LifepodButton __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new LifepodButtonPressEvent { Type = __instance.type });
    }

    [HarmonyPatch(typeof(LifepodShower), "Activate")]
    [HarmonyPostfix]
    private static void LifepodShowerActivatePostfix(LifepodShower __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new LifepodShowerActivateEvent { Shower = __instance });
    }

    [HarmonyPatch(typeof(MedStationScript), "OnTriggerEnter2D")]
    [HarmonyPostfix]
    private static void MedStationScriptOnTriggerEnter2DPostfix(
        MedStationScript __instance)
    {
        if (__instance == null || !__instance) return;

        var didHeal = Traverse.Create(__instance).Field("didHeal").GetValue<bool>();
        if (!didHeal) return;

        EventUtil.Trigger(new MedStationHealEvent { Station = __instance });
    }

    [HarmonyPatch(typeof(MineScript), "OnCollisionEnter2D")]
    [HarmonyPostfix]
    private static void MineScriptOnCollisionEnter2DPostfix(MineScript __instance)
    {
        if (__instance == null || !__instance) return;

        var pressed = Traverse.Create(__instance).Field("pressed").GetValue<bool>();
        if (!pressed) return;

        EventUtil.Trigger(new MineTriggerEvent { Mine = __instance });
    }

    [HarmonyPatch(typeof(Openable), "OnUse")]
    [HarmonyPostfix]
    private static void OpenableOnUsePostfix(Openable __instance)
    {
        if (__instance == null || !__instance) return;

        var mode = __instance.instantOpen
            ? "instant"
            : __instance.isKeypad
                ? "keypad"
                : "lockpick";
        EventUtil.Trigger(new OpenableUseEvent { Openable = __instance, Mode = mode });
    }

    [HarmonyPatch(typeof(PlushScript), "Squeak")]
    [HarmonyPostfix]
    private static void PlushScriptSqueakPostfix(PlushScript __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new PlushSqueakEvent { Plush = __instance });
    }

    [HarmonyPatch(typeof(Painkillers), "Update")]
    [HarmonyPostfix]
    private static void PainkillersUpdatePostfix(Painkillers __instance)
    {
        if (__instance == null || !__instance) return;

        var reception = Traverse.Create(__instance).Field("opiateReception")
            .GetValue<float>();
        var id = __instance.GetInstanceID();
        if (reception > 45f)
        {
            if (OpiateOverdosed.Add(id))
                EventUtil.Trigger(new OpiateOverdoseEvent());
        }
        else
        {
            OpiateOverdosed.Remove(id);
        }
    }

    [HarmonyPatch(typeof(AltHoverScript), "Update")]
    [HarmonyPostfix]
    private static void AltHoverScriptUpdatePostfix(AltHoverScript __instance)
    {
        if (__instance == null || !__instance) return;

        var active = Traverse.Create(__instance).Field("active").GetValue<bool>();
        var id = __instance.GetInstanceID();

        AltHoverActive.TryGetValue(id, out var prevActive);
        AltHoverActive[id] = active;

        if (active != prevActive)
            EventUtil.Trigger(new AltHoverToggleEvent { Active = active });
    }

    [HarmonyPatch(typeof(DrillPod))]
    public static class DrillPodPatch
    {
        [HarmonyPatch("OnUse")]
        [HarmonyPostfix]
        private static void OnUsePostfix(DrillPod __instance)
        {
            if (__instance == null || !__instance) return;

            var working = Traverse.Create(__instance).Field("working").GetValue<bool>();
            if (!working) return;

            EventUtil.Trigger(new DrillPodRepairEvent { Pod = __instance });
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(DrillPod __instance)
        {
            if (__instance == null || !__instance) return;

            var didTeleport = Traverse.Create(__instance).Field("didTeleport")
                .GetValue<bool>();
            if (!didTeleport) return;

            var id = __instance.GetInstanceID();
            if (!TeleportedDrillPods.Add(id)) return;

            EventUtil.Trigger(new DrillPodUseEvent { Pod = __instance });
        }
    }

    [HarmonyPatch(typeof(ElderThornbackBehaviour))]
    public static class ElderThornbackPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(ElderThornbackBehaviour __instance)
        {
            if (__instance == null || !__instance) return;

            var id = __instance.GetInstanceID();

            var stage = Traverse.Create(__instance).Field("stage").GetValue<int>();
            if (stage > 0)
            {
                ThornbackStages.TryGetValue(id, out var prevStage);
                if (prevStage != stage)
                {
                    ThornbackStages[id] = stage;
                    EventUtil.Trigger(new ThornbackStageEvent
                        { Thornback = __instance, Stage = stage });
                    return;
                }
            }
            else
            {
                ThornbackStages[id] = 0;
            }

            var build = __instance.GetComponent<BuildingEntity>();
            var body = PlayerCamera.main != null
                ? PlayerCamera.main.body
                : null;
            if (stage != 0
                || build == null
                || body == null
                || body.transform == null
                || __instance.transform == null) return;
            var dist = Vector2.Distance(__instance.transform.position,
                body.transform.position);
            if (dist < ElderThornbackBehaviour.maxDistance)
                EventUtil.Trigger(new ThornbackNearEvent { Thornback = __instance });
        }

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        private static void OnDestroyPostfix(ElderThornbackBehaviour __instance)
        {
            if (__instance == null || !__instance) return;

            var build = __instance.GetComponent<BuildingEntity>();
            var body = PlayerCamera.main != null
                ? PlayerCamera.main.body
                : null;
            if (build == null || build.health > 0f) return;
            if (body == null
                || body.transform == null
                || __instance.transform == null) return;

            var dist = Vector2.Distance(__instance.transform.position,
                body.transform.position);
            if (dist >= ElderThornbackBehaviour.maxDistance) return;

            EventUtil.Trigger(new ThornbackDeathEvent { Thornback = __instance });
        }
    }

    [HarmonyPatch(typeof(GeyserScript))]
    public static class GeyserScriptPatch
    {
        [HarmonyPatch("TryRumble")]
        [HarmonyPostfix]
        private static void TryRumblePostfix(GeyserScript __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new GeyserRumbleEvent { Geyser = __instance });
        }

        [HarmonyPatch("Activate")]
        [HarmonyPostfix]
        private static void ActivatePostfix(GeyserScript __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new GeyserActivateEvent { Geyser = __instance });
        }
    }

    [HarmonyPatch(typeof(GrapplingHook))]
    public static class GrapplingHookPatch
    {
        [HarmonyPatch("Use")]
        [HarmonyPostfix]
        private static void UsePostfix(GrapplingHook __instance)
        {
            if (__instance == null || !__instance) return;

            var fired = Traverse.Create(__instance).Field("fired").GetValue<bool>();
            if (!fired) return;

            EventUtil.Trigger(new GrapplingHookFireEvent { Hook = __instance });
        }

        [HarmonyPatch("HookHit")]
        [HarmonyPostfix]
        private static void HookHitPostfix(GrapplingHook __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new GrapplingHookHitEvent { Hook = __instance });
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(GrapplingHook __instance)
        {
            if (__instance == null || !__instance) return;

            var pulling = Traverse.Create(__instance).Field("pulling").GetValue<bool>();
            var fired = Traverse.Create(__instance).Field("fired").GetValue<bool>();
            var id = __instance.GetInstanceID();

            HookPullingStates.TryGetValue(id, out var prevPulling);
            HookPullingStates[id] = pulling;

            if (prevPulling && !pulling && !fired)
                EventUtil.Trigger(new GrapplingHookReturnEvent { Hook = __instance });
        }
    }

    [HarmonyPatch(typeof(Observer))]
    public static class ObserverPatch
    {
        [HarmonyPatch("RolledLastStand")]
        [HarmonyPostfix]
        private static void RolledLastStandPostfix(Observer __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new ObserverLastStandEvent { Observer = __instance });
        }

        [HarmonyPatch("GunSuicide")]
        [HarmonyPostfix]
        private static void GunSuicidePostfix(Observer __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new ObserverGunSuicideEvent { Observer = __instance });
        }
    }

    [HarmonyPatch(typeof(PreRunScript))]
    public static class PreRunScriptPatch
    {
        [HarmonyPatch("StartRun")]
        [HarmonyPostfix]
        private static void StartRunPostfix()
        {
            EventUtil.Trigger(new PreRunStartEvent());
        }

        [HarmonyPatch("LoadRun")]
        [HarmonyPostfix]
        private static void LoadRunPostfix()
        {
            EventUtil.Trigger(new PreRunLoadEvent());
        }

        [HarmonyPatch("StartTutorial")]
        [HarmonyPostfix]
        private static void StartTutorialPostfix()
        {
            EventUtil.Trigger(new PreRunTutorialEvent());
        }
    }

    [HarmonyPatch(typeof(PlayerCamera))]
    public static class PlayerCameraPatch
    {
        [HarmonyPatch("StartSelfDestruct")]
        [HarmonyPostfix]
        private static void StartSelfDestructPostfix()
        {
            EventUtil.Trigger(new SelfDestructEvent());
        }

        [HarmonyPatch("ToggleWoundView")]
        [HarmonyPostfix]
        private static void ToggleWoundViewPostfix(PlayerCamera __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new WoundViewToggleEvent
            {
                Open = __instance.woundView != null && __instance.woundView.activeSelf
            });
        }

        [HarmonyPatch("OpenCraftScreen")]
        [HarmonyPostfix]
        private static void OpenCraftScreenPostfix(PlayerCamera __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new CraftPanelToggleEvent
            {
                Open = __instance.craftingPanel != null
                    && __instance.craftingPanel.activeSelf
            });
        }
    }

    [HarmonyPatch(typeof(AmmoScript))]
    public static class AmmoScriptPatch
    {
        [HarmonyPatch("UnloadRound")]
        [HarmonyPostfix]
        private static void UnloadRoundPostfix(AmmoScript __instance)
        {
            if (__instance == null || !__instance) return;
            var magazine = __instance.GetComponent<Item>();
            if (!magazine) return;
            EventUtil.Trigger(new AmmoUnloadEvent { Magazine = magazine });
        }

        [HarmonyPatch("LoadRound")]
        [HarmonyPostfix]
        private static void LoadRoundPostfix(AmmoScript __instance)
        {
            if (__instance == null || !__instance) return;
            var magazine = __instance.GetComponent<Item>();
            if (!magazine) return;
            EventUtil.Trigger(new AmmoLoadEvent { Magazine = magazine });
        }
    }
}
