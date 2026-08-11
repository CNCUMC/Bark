using System;
using System.Collections.Generic;
using Bark.Events;
using Bark.Tool;
using HarmonyLib;

namespace Bark.Event.Listener;

// 世界对象事件监听器：通过 Harmony 补丁拦截可损坏物、伤害板条箱、钻探舱、
// 脊背兽长老、PDA、间歇泉、全局暗幕、捕抓植物、抓钩等对象的方法，触发对应事件。
public static class WorldObjectEventListener
{
    // 钻探舱已传送标记（instanceId）
    private static readonly HashSet<int> TeleportedDrillPods = [];

    // 长老上次 stage（instanceId → stage）
    private static readonly Dictionary<int, int> ThornbackStages = new();

    // 捕抓植物已抓住标记（instanceId）
    private static readonly HashSet<int> GrabbedPlants = [];

    // 抓钩拉回状态（instanceId → pulling）
    private static readonly Dictionary<int, bool> HookPullingStates = new();

    // 跳跃平台上次冷却（instanceId → cooldown）
    private static readonly Dictionary<int, float> JumpPadActive = new();

    // 阿片过量已触发标记（instanceId）
    private static readonly HashSet<int> OpiateOverdosed = [];

    // Alt 物品标签上次状态（instanceId → active）
    private static readonly Dictionary<int, bool> AltHoverActive = new();

    internal static void Listen()
    {
        PatchDamageable();
        PatchDamagingCrate();
        PatchDrillPod();
        PatchThornback();
        PatchPda();
        PatchGeyser();
        PatchGlobalDark();
        PatchGrabberPlant();
        PatchGrapplingHook();
        PatchItemDestroy();
        PatchJumpPad();
        PatchLifepodButton();
        PatchLifepodShower();
        PatchMedStation();
        PatchMine();
        PatchObserver();
        PatchOpenable();
        PatchPlush();
        PatchPreRun();
        PatchPainkillers();
        PatchPlayerCamera();
        PatchAmmo();
        PatchAltHover();
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

    private static void Patch(Type type, string methodName, string harmonyId, string? prefix, string? postfix)
    {
        var method = AccessTools.Method(type, methodName);
        if (method == null) return;

        try
        {
            var harmony = new Harmony(harmonyId);
            harmony.Patch(method,
                prefix: prefix != null ? new HarmonyMethod(typeof(WorldObjectEventListener), prefix) : null,
                postfix: postfix != null ? new HarmonyMethod(typeof(WorldObjectEventListener), postfix) : null);
        }
        catch
        {
            // ignored
        }
    }

    // ============================================================
    // 可损坏物
    // ============================================================

    private static void PatchDamageable()
    {
        Patch(typeof(Damageable), "Damage", "Bark.Damageable.Damaged", null, nameof(OnDamageableDamagedPostfix));
    }

    private static void OnDamageableDamagedPostfix(Damageable __instance, float damage)
    {
        if (__instance == null || !__instance) return;

        EventUtil.Trigger(new DamageableDamagedEvent { Damageable = __instance, Damage = damage });
    }

    // ============================================================
    // 伤害板条箱
    // ============================================================

    private static void PatchDamagingCrate()
    {
        // 用 prefix：方法内会 Destroy(this)，prefix 时实例仍有效
        Patch(typeof(DamagingCrate), "OnCollisionEnter2D", "Bark.DamagingCrate.Hit", nameof(OnDamagingCrateHitPrefix), null);
    }

    private static void OnDamagingCrateHitPrefix(DamagingCrate __instance)
    {
        if (__instance == null || !__instance) return;

        EventUtil.Trigger(new DamagingCrateHitEvent { Crate = __instance, Type = __instance.type });
    }

    // ============================================================
    // 钻探舱
    // ============================================================

    private static void PatchDrillPod()
    {
        Patch(typeof(DrillPod), "OnUse", "Bark.DrillPod.Repair", null, nameof(OnDrillPodRepairPostfix));
        Patch(typeof(DrillPod), "Update", "Bark.DrillPod.Use", null, nameof(OnDrillPodUsePostfix));
    }

    private static void OnDrillPodRepairPostfix(DrillPod __instance)
    {
        if (__instance == null || !__instance) return;

        var working = Traverse.Create(__instance).Field("working").GetValue<bool>();
        if (!working) return;

        EventUtil.Trigger(new DrillPodRepairEvent { Pod = __instance });
    }

    private static void OnDrillPodUsePostfix(DrillPod __instance)
    {
        if (__instance == null || !__instance) return;

        var didTeleport = Traverse.Create(__instance).Field("didTeleport").GetValue<bool>();
        if (!didTeleport) return;

        var id = __instance.GetInstanceID();
        if (!TeleportedDrillPods.Add(id)) return;

        EventUtil.Trigger(new DrillPodUseEvent { Pod = __instance });
    }

    // ============================================================
    // 脊背兽长老
    // ============================================================

    private static void PatchThornback()
    {
        Patch(typeof(ElderThornbackBehaviour), "Update", "Bark.Thornback.Update", null, nameof(OnThornbackUpdatePostfix));
        Patch(typeof(ElderThornbackBehaviour), "OnDestroy", "Bark.Thornback.Death", null, nameof(OnThornbackDeathPostfix));
    }

    private static void OnThornbackUpdatePostfix(ElderThornbackBehaviour __instance)
    {
        if (__instance == null || !__instance) return;

        var id = __instance.GetInstanceID();

        // 阶段转换检测
        var stage = Traverse.Create(__instance).Field("stage").GetValue<int>();
        if (stage > 0)
        {
            ThornbackStages.TryGetValue(id, out var prevStage);
            if (prevStage != stage)
            {
                ThornbackStages[id] = stage;
                EventUtil.Trigger(new ThornbackStageEvent { Thornback = __instance, Stage = stage });
                return;
            }
        }
        else
        {
            ThornbackStages[id] = 0;
        }

        // 靠近检测（stage==0 且未进入阶段，说明在平静阶段，靠近触发一次）
        var build = __instance.GetComponent<BuildingEntity>();
        var body = PlayerCamera.main != null ? PlayerCamera.main.body : null;
        if (stage != 0 || build == null || body == null || body.transform == null ||
            __instance.transform == null) return;
        var dist = UnityEngine.Vector2.Distance(__instance.transform.position, body.transform.position);
        if (dist < ElderThornbackBehaviour.maxDistance)
            EventUtil.Trigger(new ThornbackNearEvent { Thornback = __instance });
    }

    private static void OnThornbackDeathPostfix(ElderThornbackBehaviour __instance)
    {
        if (__instance == null || !__instance) return;

        var build = __instance.GetComponent<BuildingEntity>();
        var body = PlayerCamera.main != null ? PlayerCamera.main.body : null;
        if (build == null || build.health > 0f) return;
        if (body == null || body.transform == null || __instance.transform == null) return;

        var dist = UnityEngine.Vector2.Distance(__instance.transform.position, body.transform.position);
        if (dist >= ElderThornbackBehaviour.maxDistance) return;

        EventUtil.Trigger(new ThornbackDeathEvent { Thornback = __instance });
    }

    // ============================================================
    // PDA
    // ============================================================

    private static void PatchPda()
    {
        // 用 prefix：方法内会修改 hasBeenRead，prefix 时还是旧值
        Patch(typeof(EPdaScript), "Use", "Bark.Pda.Use", nameof(OnPdaUsePrefix), null);
    }

    private static void OnPdaUsePrefix(EPdaScript __instance)
    {
        if (__instance == null || !__instance) return;

        var item = __instance.GetComponent<Item>();
        EventUtil.Trigger(new PdaUseEvent
        {
            Pda = item,
            FirstRead = !__instance.hasBeenRead
        });
    }

    // ============================================================
    // 间歇泉
    // ============================================================

    private static void PatchGeyser()
    {
        Patch(typeof(GeyserScript), "TryRumble", "Bark.Geyser.Rumble", null, nameof(OnGeyserRumblePostfix));
        Patch(typeof(GeyserScript), "Activate", "Bark.Geyser.Activate", null, nameof(OnGeyserActivatePostfix));
    }

    private static void OnGeyserRumblePostfix(GeyserScript __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new GeyserRumbleEvent { Geyser = __instance });
    }

    private static void OnGeyserActivatePostfix(GeyserScript __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new GeyserActivateEvent { Geyser = __instance });
    }

    // ============================================================
    // 全局暗幕
    // ============================================================

    private static void PatchGlobalDark()
    {
        Patch(typeof(GlobalDark), "Darken", "Bark.GlobalDark.Darken", null, nameof(OnGlobalDarkPostfix));
    }

    private static void OnGlobalDarkPostfix(GlobalDark __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new GlobalDarkEvent { Darkening = __instance.IsDarkening() });
    }

    // ============================================================
    // 捕抓植物
    // ============================================================

    private static void PatchGrabberPlant()
    {
        Patch(typeof(GrabberPlant), "Update", "Bark.GrabberPlant.Grab", null, nameof(OnGrabberPlantUpdatePostfix));
    }

    private static void OnGrabberPlantUpdatePostfix(GrabberPlant __instance)
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

    // ============================================================
    // 抓钩
    // ============================================================

    private static void PatchGrapplingHook()
    {
        Patch(typeof(GrapplingHook), "Use", "Bark.GrapplingHook.Fire", null, nameof(OnHookFirePostfix));
        Patch(typeof(GrapplingHook), "HookHit", "Bark.GrapplingHook.Hit", null, nameof(OnHookHitPostfix));
        Patch(typeof(GrapplingHook), "Update", "Bark.GrapplingHook.Return", null, nameof(OnHookUpdatePostfix));
    }

    private static void OnHookFirePostfix(GrapplingHook __instance)
    {
        if (__instance == null || !__instance) return;

        var fired = Traverse.Create(__instance).Field("fired").GetValue<bool>();
        if (!fired) return;

        EventUtil.Trigger(new GrapplingHookFireEvent { Hook = __instance });
    }

    private static void OnHookHitPostfix(GrapplingHook __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new GrapplingHookHitEvent { Hook = __instance });
    }

    private static void OnHookUpdatePostfix(GrapplingHook __instance)
    {
        if (__instance == null || !__instance) return;

        var pulling = Traverse.Create(__instance).Field("pulling").GetValue<bool>();
        var fired = Traverse.Create(__instance).Field("fired").GetValue<bool>();
        var id = __instance.GetInstanceID();

        HookPullingStates.TryGetValue(id, out var prevPulling);
        HookPullingStates[id] = pulling;

        // pulling 从 true → false 且未在发射状态 → 钩子已收回
        if (prevPulling && !pulling && !fired)
            EventUtil.Trigger(new GrapplingHookReturnEvent { Hook = __instance });
    }

    // ============================================================
    // 物品销毁
    // ============================================================

    private static void PatchItemDestroy()
    {
        // OnDestroy 时 condition<=0 表示因耐久归零被销毁
        Patch(typeof(Item), "OnDestroy", "Bark.Item.Destroyed", null, nameof(OnItemDestroyPostfix));
    }

    private static void OnItemDestroyPostfix(Item __instance)
    {
        if (__instance == null || !__instance) return;
        if (__instance.condition > 0f) return;

        EventUtil.Trigger(new ItemDestroyEvent
        {
            ItemId = __instance.id ?? string.Empty,
            Item = __instance
        });
    }

    // ============================================================
    // 跳跃平台
    // ============================================================

    private static void PatchJumpPad()
    {
        Patch(typeof(JumpPadScript), "OnCollisionEnter2D", "Bark.JumpPad.Bounce", null, nameof(OnJumpPadPostfix));
    }

    private static void OnJumpPadPostfix(JumpPadScript __instance)
    {
        if (__instance == null || !__instance) return;

        var id = __instance.GetInstanceID();
        var cooldown = Traverse.Create(__instance).Field("cooldown").GetValue<float>();
        JumpPadActive.TryGetValue(id, out var prevCooldown);
        JumpPadActive[id] = cooldown;

        // cooldown 从 <15 变为 >=15 → 刚弹跳
        if (prevCooldown < 15f && cooldown >= 15f)
            EventUtil.Trigger(new JumpPadBounceEvent { Pad = __instance });
    }

    // ============================================================
    // 救生舱按钮
    // ============================================================

    private static void PatchLifepodButton()
    {
        Patch(typeof(LifepodButton), "OnUse", "Bark.LifepodButton.Press", null, nameof(OnLifepodButtonPostfix));
    }

    private static void OnLifepodButtonPostfix(LifepodButton __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new LifepodButtonPressEvent { Type = __instance.type });
    }

    // ============================================================
    // 救生舱淋浴
    // ============================================================

    private static void PatchLifepodShower()
    {
        Patch(typeof(LifepodShower), "Activate", "Bark.LifepodShower.Activate", null, nameof(OnLifepodShowerPostfix));
    }

    private static void OnLifepodShowerPostfix(LifepodShower __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new LifepodShowerActivateEvent { Shower = __instance });
    }

    // ============================================================
    // 医疗站
    // ============================================================

    private static void PatchMedStation()
    {
        Patch(typeof(MedStationScript), "OnTriggerEnter2D", "Bark.MedStation.Heal", null, nameof(OnMedStationPostfix));
    }

    private static void OnMedStationPostfix(MedStationScript __instance)
    {
        if (__instance == null || !__instance) return;

        var didHeal = Traverse.Create(__instance).Field("didHeal").GetValue<bool>();
        if (!didHeal) return;

        EventUtil.Trigger(new MedStationHealEvent { Station = __instance });
    }

    // ============================================================
    // 地雷
    // ============================================================

    private static void PatchMine()
    {
        Patch(typeof(MineScript), "OnCollisionEnter2D", "Bark.Mine.Trigger", null, nameof(OnMinePostfix));
    }

    private static void OnMinePostfix(MineScript __instance)
    {
        if (__instance == null || !__instance) return;

        var pressed = Traverse.Create(__instance).Field("pressed").GetValue<bool>();
        if (!pressed) return;

        EventUtil.Trigger(new MineTriggerEvent { Mine = __instance });
    }

    // ============================================================
    // 观察者（邪神）
    // ============================================================

    private static void PatchObserver()
    {
        Patch(typeof(Observer), "RolledLastStand", "Bark.Observer.LastStand", null, nameof(OnObserverLastStandPostfix));
        Patch(typeof(Observer), "GunSuicide", "Bark.Observer.GunSuicide", null, nameof(OnObserverGunSuicidePostfix));
    }

    private static void OnObserverLastStandPostfix(Observer __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new ObserverLastStandEvent { Observer = __instance });
    }

    private static void OnObserverGunSuicidePostfix(Observer __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new ObserverGunSuicideEvent { Observer = __instance });
    }

    // ============================================================
    // 可开启物
    // ============================================================

    private static void PatchOpenable()
    {
        Patch(typeof(Openable), "OnUse", "Bark.Openable.Use", null, nameof(OnOpenablePostfix));
    }

    private static void OnOpenablePostfix(Openable __instance)
    {
        if (__instance == null || !__instance) return;

        var mode = __instance.instantOpen ? "instant" : (__instance.isKeypad ? "keypad" : "lockpick");
        EventUtil.Trigger(new OpenableUseEvent { Openable = __instance, Mode = mode });
    }

    // ============================================================
    // 毛绒玩具
    // ============================================================

    private static void PatchPlush()
    {
        Patch(typeof(PlushScript), "Squeak", "Bark.Plush.Squeak", null, nameof(OnPlushPostfix));
    }

    private static void OnPlushPostfix(PlushScript __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new PlushSqueakEvent { Plush = __instance });
    }

    // ============================================================
    // 开局前
    // ============================================================

    private static void PatchPreRun()
    {
        Patch(typeof(PreRunScript), "StartRun", "Bark.PreRun.Start", null, nameof(OnPreRunStartPostfix));
        Patch(typeof(PreRunScript), "LoadRun", "Bark.PreRun.Load", null, nameof(OnPreRunLoadPostfix));
        Patch(typeof(PreRunScript), "StartTutorial", "Bark.PreRun.Tutorial", null, nameof(OnPreRunTutorialPostfix));
    }

    private static void OnPreRunStartPostfix()
    {
        EventUtil.Trigger(new PreRunStartEvent());
    }

    private static void OnPreRunLoadPostfix()
    {
        EventUtil.Trigger(new PreRunLoadEvent());
    }

    private static void OnPreRunTutorialPostfix()
    {
        EventUtil.Trigger(new PreRunTutorialEvent());
    }

    // ============================================================
    // 阿片类药物
    // ============================================================

    private static void PatchPainkillers()
    {
        Patch(typeof(Painkillers), "Update", "Bark.Painkillers.Overdose", null, nameof(OnPainkillersUpdatePostfix));
    }

    private static void OnPainkillersUpdatePostfix(Painkillers __instance)
    {
        if (__instance == null || !__instance) return;

        var reception = Traverse.Create(__instance).Field("opiateReception").GetValue<float>();
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

    // ============================================================
    // 玩家相机
    // ============================================================

    private static void PatchPlayerCamera()
    {
        Patch(typeof(PlayerCamera), "StartSelfDestruct", "Bark.PlayerCamera.SelfDestruct", null, nameof(OnSelfDestructPostfix));
        Patch(typeof(PlayerCamera), "ToggleWoundView", "Bark.PlayerCamera.WoundView", null, nameof(OnWoundViewPostfix));
        Patch(typeof(PlayerCamera), "OpenCraftScreen", "Bark.PlayerCamera.CraftPanel", null, nameof(OnCraftPanelPostfix));
    }

    private static void OnSelfDestructPostfix()
    {
        EventUtil.Trigger(new SelfDestructEvent());
    }

    private static void OnWoundViewPostfix(PlayerCamera __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new WoundViewToggleEvent { Open = __instance.woundView != null && __instance.woundView.activeSelf });
    }

    private static void OnCraftPanelPostfix(PlayerCamera __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new CraftPanelToggleEvent
        {
            Open = __instance.craftingPanel != null && __instance.craftingPanel.activeSelf
        });
    }

    // ============================================================
    // 弹药
    // ============================================================

    private static void PatchAmmo()
    {
        Patch(typeof(AmmoScript), "UnloadRound", "Bark.Ammo.Unload", null, nameof(OnAmmoUnloadPostfix));
        Patch(typeof(AmmoScript), "LoadRound", "Bark.Ammo.Load", null, nameof(OnAmmoLoadPostfix));
    }

    private static void OnAmmoUnloadPostfix(AmmoScript __instance)
    {
        if (__instance == null || !__instance) return;
        var magazine = __instance.GetComponent<Item>();
        if (!magazine) return;
        EventUtil.Trigger(new AmmoUnloadEvent { Magazine = magazine });
    }

    private static void OnAmmoLoadPostfix(AmmoScript __instance)
    {
        if (__instance == null || !__instance) return;
        var magazine = __instance.GetComponent<Item>();
        if (!magazine) return;
        EventUtil.Trigger(new AmmoLoadEvent { Magazine = magazine });
    }

    // ============================================================
    // Alt 物品标签
    // ============================================================

    private static void PatchAltHover()
    {
        Patch(typeof(AltHoverScript), "Update", "Bark.AltHover.Toggle", null, nameof(OnAltHoverUpdatePostfix));
    }

    private static void OnAltHoverUpdatePostfix(AltHoverScript __instance)
    {
        if (__instance == null || !__instance) return;

        var active = Traverse.Create(__instance).Field("active").GetValue<bool>();
        var id = __instance.GetInstanceID();

        AltHoverActive.TryGetValue(id, out var prevActive);
        AltHoverActive[id] = active;

        if (active != prevActive)
            EventUtil.Trigger(new AltHoverToggleEvent { Active = active });
    }
}
