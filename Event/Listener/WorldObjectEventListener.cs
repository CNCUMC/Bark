using System;
using System.Collections.Generic;
using Bark.Events;
using Bark.Tool;
using HarmonyLib;
using UnityEngine;

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

    // ============================================================
    // 可损坏物
    // ============================================================

    [HarmonyPatch(typeof(Damageable))]
    [HarmonyPatch("Damage")]
    public static class DamageableDamagedPatch
    {
        private static void Postfix(Damageable __instance, float damage)
        {
            if (__instance == null || !__instance) return;

            EventUtil.Trigger(new DamageableDamagedEvent { Damageable = __instance, Damage = damage });
        }
    }

    // ============================================================
    // 伤害板条箱
    // ============================================================

    [HarmonyPatch(typeof(DamagingCrate))]
    [HarmonyPatch("OnCollisionEnter2D")]
    public static class DamagingCrateHitPatch
    {
        // 用 prefix：方法内会 Destroy(this)，prefix 时实例仍有效
        private static void Prefix(DamagingCrate __instance)
        {
            if (__instance == null || !__instance) return;

            EventUtil.Trigger(new DamagingCrateHitEvent { Crate = __instance, Type = __instance.type });
        }
    }

    // ============================================================
    // 钻探舱
    // ============================================================

    [HarmonyPatch(typeof(DrillPod))]
    [HarmonyPatch("OnUse")]
    public static class DrillPodRepairPatch
    {
        private static void Postfix(DrillPod __instance)
        {
            if (__instance == null || !__instance) return;

            var working = Traverse.Create(__instance).Field("working").GetValue<bool>();
            if (!working) return;

            EventUtil.Trigger(new DrillPodRepairEvent { Pod = __instance });
        }
    }

    [HarmonyPatch(typeof(DrillPod))]
    [HarmonyPatch("Update")]
    public static class DrillPodUsePatch
    {
        private static void Postfix(DrillPod __instance)
        {
            if (__instance == null || !__instance) return;

            var didTeleport = Traverse.Create(__instance).Field("didTeleport").GetValue<bool>();
            if (!didTeleport) return;

            var id = __instance.GetInstanceID();
            if (!TeleportedDrillPods.Add(id)) return;

            EventUtil.Trigger(new DrillPodUseEvent { Pod = __instance });
        }
    }

    // ============================================================
    // 脊背兽长老
    // ============================================================

    [HarmonyPatch(typeof(ElderThornbackBehaviour))]
    [HarmonyPatch("Update")]
    public static class ThornbackUpdatePatch
    {
        private static void Postfix(ElderThornbackBehaviour __instance)
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
    }

    [HarmonyPatch(typeof(ElderThornbackBehaviour))]
    [HarmonyPatch("OnDestroy")]
    public static class ThornbackDeathPatch
    {
        private static void Postfix(ElderThornbackBehaviour __instance)
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
    }

    // ============================================================
    // PDA
    // ============================================================

    [HarmonyPatch(typeof(EPdaScript))]
    [HarmonyPatch("Use")]
    public static class PdaUsePatch
    {
        // 用 prefix：方法内会修改 hasBeenRead，prefix 时还是旧值
        private static void Prefix(EPdaScript __instance)
        {
            if (__instance == null || !__instance) return;

            var item = __instance.GetComponent<Item>();
            EventUtil.Trigger(new PdaUseEvent
            {
                Pda = item,
                FirstRead = !__instance.hasBeenRead
            });
        }
    }

    // ============================================================
    // 间歇泉
    // ============================================================

    [HarmonyPatch(typeof(GeyserScript))]
    [HarmonyPatch("TryRumble")]
    public static class GeyserRumblePatch
    {
        private static void Postfix(GeyserScript __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new GeyserRumbleEvent { Geyser = __instance });
        }
    }

    [HarmonyPatch(typeof(GeyserScript))]
    [HarmonyPatch("Activate")]
    public static class GeyserActivatePatch
    {
        private static void Postfix(GeyserScript __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new GeyserActivateEvent { Geyser = __instance });
        }
    }

    // ============================================================
    // 全局暗幕
    // ============================================================

    [HarmonyPatch(typeof(GlobalDark))]
    [HarmonyPatch("Darken")]
    public static class GlobalDarkPatch
    {
        private static void Postfix(GlobalDark __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new GlobalDarkEvent { Darkening = __instance.IsDarkening() });
        }
    }

    // ============================================================
    // 捕抓植物
    // ============================================================

    [HarmonyPatch(typeof(GrabberPlant))]
    [HarmonyPatch("Update")]
    public static class GrabberPlantGrabPatch
    {
        private static void Postfix(GrabberPlant __instance)
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
    }

    // ============================================================
    // 抓钩
    // ============================================================

    [HarmonyPatch(typeof(GrapplingHook))]
    [HarmonyPatch("Use")]
    public static class GrapplingHookFirePatch
    {
        private static void Postfix(GrapplingHook __instance)
        {
            if (__instance == null || !__instance) return;

            var fired = Traverse.Create(__instance).Field("fired").GetValue<bool>();
            if (!fired) return;

            EventUtil.Trigger(new GrapplingHookFireEvent { Hook = __instance });
        }
    }

    [HarmonyPatch(typeof(GrapplingHook))]
    [HarmonyPatch("HookHit")]
    public static class GrapplingHookHitPatch
    {
        private static void Postfix(GrapplingHook __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new GrapplingHookHitEvent { Hook = __instance });
        }
    }

    [HarmonyPatch(typeof(GrapplingHook))]
    [HarmonyPatch("Update")]
    public static class GrapplingHookReturnPatch
    {
        private static void Postfix(GrapplingHook __instance)
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
    }

    // ============================================================
    // 物品销毁
    // ============================================================

    [HarmonyPatch(typeof(Item))]
    [HarmonyPatch("OnDestroy")]
    public static class ItemDestroyPatch
    {
        // OnDestroy 时 condition<=0 表示因耐久归零被销毁
        private static void Postfix(Item __instance)
        {
            if (__instance == null || !__instance) return;
            if (__instance.condition > 0f) return;

            EventUtil.Trigger(new ItemDestroyEvent
            {
                ItemId = __instance.id ?? string.Empty,
                Item = __instance
            });
        }
    }

    // ============================================================
    // 跳跃平台
    // ============================================================

    [HarmonyPatch(typeof(JumpPadScript))]
    [HarmonyPatch("OnCollisionEnter2D")]
    public static class JumpPadBouncePatch
    {
        private static void Postfix(JumpPadScript __instance)
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
    }

    // ============================================================
    // 救生舱按钮
    // ============================================================

    [HarmonyPatch(typeof(LifepodButton))]
    [HarmonyPatch("OnUse")]
    public static class LifepodButtonPressPatch
    {
        private static void Postfix(LifepodButton __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new LifepodButtonPressEvent { Type = __instance.type });
        }
    }

    // ============================================================
    // 救生舱淋浴
    // ============================================================

    [HarmonyPatch(typeof(LifepodShower))]
    [HarmonyPatch("Activate")]
    public static class LifepodShowerActivatePatch
    {
        private static void Postfix(LifepodShower __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new LifepodShowerActivateEvent { Shower = __instance });
        }
    }

    // ============================================================
    // 医疗站
    // ============================================================

    [HarmonyPatch(typeof(MedStationScript))]
    [HarmonyPatch("OnTriggerEnter2D")]
    public static class MedStationHealPatch
    {
        private static void Postfix(MedStationScript __instance)
        {
            if (__instance == null || !__instance) return;

            var didHeal = Traverse.Create(__instance).Field("didHeal").GetValue<bool>();
            if (!didHeal) return;

            EventUtil.Trigger(new MedStationHealEvent { Station = __instance });
        }
    }

    // ============================================================
    // 地雷
    // ============================================================

    [HarmonyPatch(typeof(MineScript))]
    [HarmonyPatch("OnCollisionEnter2D")]
    public static class MineTriggerPatch
    {
        private static void Postfix(MineScript __instance)
        {
            if (__instance == null || !__instance) return;

            var pressed = Traverse.Create(__instance).Field("pressed").GetValue<bool>();
            if (!pressed) return;

            EventUtil.Trigger(new MineTriggerEvent { Mine = __instance });
        }
    }

    // ============================================================
    // 观察者（邪神）
    // ============================================================

    [HarmonyPatch(typeof(Observer))]
    [HarmonyPatch("RolledLastStand")]
    public static class ObserverLastStandPatch
    {
        private static void Postfix(Observer __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new ObserverLastStandEvent { Observer = __instance });
        }
    }

    [HarmonyPatch(typeof(Observer))]
    [HarmonyPatch("GunSuicide")]
    public static class ObserverGunSuicidePatch
    {
        private static void Postfix(Observer __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new ObserverGunSuicideEvent { Observer = __instance });
        }
    }

    // ============================================================
    // 可开启物
    // ============================================================

    [HarmonyPatch(typeof(Openable))]
    [HarmonyPatch("OnUse")]
    public static class OpenableUsePatch
    {
        private static void Postfix(Openable __instance)
        {
            if (__instance == null || !__instance) return;

            var mode = __instance.instantOpen ? "instant" : (__instance.isKeypad ? "keypad" : "lockpick");
            EventUtil.Trigger(new OpenableUseEvent { Openable = __instance, Mode = mode });
        }
    }

    // ============================================================
    // 毛绒玩具
    // ============================================================

    [HarmonyPatch(typeof(PlushScript))]
    [HarmonyPatch("Squeak")]
    public static class PlushSqueakPatch
    {
        private static void Postfix(PlushScript __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new PlushSqueakEvent { Plush = __instance });
        }
    }

    // ============================================================
    // 开局前
    // ============================================================

    [HarmonyPatch(typeof(PreRunScript))]
    [HarmonyPatch("StartRun")]
    public static class PreRunStartPatch
    {
        private static void Postfix()
        {
            EventUtil.Trigger(new PreRunStartEvent());
        }
    }

    [HarmonyPatch(typeof(PreRunScript))]
    [HarmonyPatch("LoadRun")]
    public static class PreRunLoadPatch
    {
        private static void Postfix()
        {
            EventUtil.Trigger(new PreRunLoadEvent());
        }
    }

    [HarmonyPatch(typeof(PreRunScript))]
    [HarmonyPatch("StartTutorial")]
    public static class PreRunTutorialPatch
    {
        private static void Postfix()
        {
            EventUtil.Trigger(new PreRunTutorialEvent());
        }
    }

    // ============================================================
    // 阿片类药物
    // ============================================================

    [HarmonyPatch(typeof(Painkillers))]
    [HarmonyPatch("Update")]
    public static class PainkillersOverdosePatch
    {
        private static void Postfix(Painkillers __instance)
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
    }

    // ============================================================
    // 玩家相机
    // ============================================================

    [HarmonyPatch(typeof(PlayerCamera))]
    [HarmonyPatch("StartSelfDestruct")]
    public static class SelfDestructPatch
    {
        private static void Postfix()
        {
            EventUtil.Trigger(new SelfDestructEvent());
        }
    }

    [HarmonyPatch(typeof(PlayerCamera))]
    [HarmonyPatch("ToggleWoundView")]
    public static class WoundViewTogglePatch
    {
        private static void Postfix(PlayerCamera __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new WoundViewToggleEvent { Open = __instance.woundView != null && __instance.woundView.activeSelf });
        }
    }

    [HarmonyPatch(typeof(PlayerCamera))]
    [HarmonyPatch("OpenCraftScreen")]
    public static class CraftPanelTogglePatch
    {
        private static void Postfix(PlayerCamera __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new CraftPanelToggleEvent
            {
                Open = __instance.craftingPanel != null && __instance.craftingPanel.activeSelf
            });
        }
    }

    // ============================================================
    // 弹药
    // ============================================================

    [HarmonyPatch(typeof(AmmoScript))]
    [HarmonyPatch("UnloadRound")]
    public static class AmmoUnloadPatch
    {
        private static void Postfix(AmmoScript __instance)
        {
            if (__instance == null || !__instance) return;
            var magazine = __instance.GetComponent<Item>();
            if (!magazine) return;
            EventUtil.Trigger(new AmmoUnloadEvent { Magazine = magazine });
        }
    }

    [HarmonyPatch(typeof(AmmoScript))]
    [HarmonyPatch("LoadRound")]
    public static class AmmoLoadPatch
    {
        private static void Postfix(AmmoScript __instance)
        {
            if (__instance == null || !__instance) return;
            var magazine = __instance.GetComponent<Item>();
            if (!magazine) return;
            EventUtil.Trigger(new AmmoLoadEvent { Magazine = magazine });
        }
    }

    // ============================================================
    // Alt 物品标签
    // ============================================================

    [HarmonyPatch(typeof(AltHoverScript))]
    [HarmonyPatch("Update")]
    public static class AltHoverTogglePatch
    {
        private static void Postfix(AltHoverScript __instance)
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
}
