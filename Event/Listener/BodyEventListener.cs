using System.Collections;
using Bark.Events;
using Bark.Tool;
using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;

namespace Bark.Event.Listener;

// Body 状态/行为事件监听：
// - 行为动作（攀爬/锻炼/切换手持/切换朝向/拾取/丢弃/毁容/失去眼睛/最后坚持）用 Harmony patch 触发
// - 生命体征临界（心脏骤停/心室颤动/呼吸/意识/濒死）与睡眠、下蹲用协程轮询检测状态翻转触发
[HarmonyPatch(typeof(Body))]
public static class BodyEventListener
{
    private const float PollInterval = 0.3f;

    private static MonoBehaviour? _runner;
    private static Coroutine? _monitorCoroutine;

    // 轮询状态的上次值
    private static bool _wasCardiacArrest;
    private static bool _wasFibrillating;
    private static bool _wasBreathing = true;
    private static bool _wasConscious = true;
    private static bool _wasBrainDying;
    private static bool _wasExercising;
    private static bool _wasCrouching;
    private static bool _wasSleeping;

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;
        _monitorCoroutine = runner.StartCoroutine(MonitorBody());
    }

    internal static void Stop()
    {
        if (_monitorCoroutine != null && _runner != null)
        {
            _runner.StopCoroutine(_monitorCoroutine);
            _monitorCoroutine = null;
        }

        _runner = null;
    }

    // Harmony 前缀：行为动作
    [HarmonyPatch("StartClimbing")]
    [HarmonyPrefix]
    private static void StartClimbingPrefix(Body __instance)
    {
        if (!IsPlayerBody(__instance)) return;
        EventUtil.Trigger(new BodyClimbStartEvent { Body = __instance, Camera = PlayerCamera.main });
    }

    [HarmonyPatch("StopClimbing")]
    [HarmonyPrefix]
    private static void StopClimbingPrefix(Body __instance)
    {
        if (!IsPlayerBody(__instance)) return;
        EventUtil.Trigger(new BodyClimbEndEvent { Body = __instance, Camera = PlayerCamera.main });
    }

    [HarmonyPatch("SwitchHands")]
    [HarmonyPrefix]
    private static void SwitchHandsPrefix(Body __instance)
    {
        if (!IsPlayerBody(__instance)) return;
        EventUtil.Trigger(new BodySwitchHandsEvent { Body = __instance, Camera = PlayerCamera.main });
    }

    [HarmonyPatch("PickUpItem")]
    [HarmonyPrefix]
    private static void PickUpItemPrefix(Body __instance, Item item, int slot)
    {
        if (!IsPlayerBody(__instance)) return;
        EventUtil.Trigger(new BodyPickUpEvent
        {
            Body = __instance,
            Camera = PlayerCamera.main,
            ItemId = item.id ?? string.Empty,
            Slot = slot
        });
    }

    [HarmonyPatch("DropItem", typeof(int))]
    [HarmonyPrefix]
    private static void DropItemPrefix(Body __instance, int slot)
    {
        if (!IsPlayerBody(__instance)) return;
        var item = __instance.GetItem(slot);
        EventUtil.Trigger(new BodyDropEvent
        {
            Body = __instance,
            Camera = PlayerCamera.main,
            ItemId = item?.id ?? string.Empty
        });
    }


    // Harmony 后缀：特殊状态
    [HarmonyPatch("SwitchDir")]
    [HarmonyPostfix]
    private static void SwitchDirPostfix(Body __instance)
    {
        if (!IsPlayerBody(__instance)) return;
        EventUtil.Trigger(new BodySwitchDirEvent
        {
            Body = __instance,
            Camera = PlayerCamera.main,
            IsRight = __instance.isRight
        });
    }

    [HarmonyPatch("Disfigure")]
    [HarmonyPostfix]
    private static void DisfigurePostfix(Body __instance)
    {
        if (!IsPlayerBody(__instance) || !__instance.disfigured) return;
        EventUtil.Trigger(new BodyDisfigureEvent { Body = __instance, Camera = PlayerCamera.main });
    }

    [HarmonyPatch("RemoveEye")]
    [HarmonyPostfix]
    private static void RemoveEyePostfix(Body __instance)
    {
        if (!IsPlayerBody(__instance) || __instance is { eyeGone: false, bothEyesGone: false }) return;
        EventUtil.Trigger(new BodyRemoveEyeEvent
        {
            Body = __instance,
            Camera = PlayerCamera.main,
            BothEyesGone = __instance.bothEyesGone
        });
    }

    [HarmonyPatch("TryLastStand")]
    [HarmonyPostfix]
    private static void TryLastStandPostfix(Body __instance)
    {
        if (!IsPlayerBody(__instance) || !__instance.succesfullyRolledLastStand) return;
        EventUtil.Trigger(new BodyLastStandEvent { Body = __instance, Camera = PlayerCamera.main });
    }

    // 状态轮询：生命体征临界 / 意识 / 睡眠 / 下蹲 / 锻炼
    private static IEnumerator MonitorBody()
    {
        yield return CUCoreUtils.AwaitWorldGeneration();

        var body = BodyUtil.Body;
        if (body)
        {
            _wasBreathing = body.breathing;
            _wasConscious = body.conscious;
        }

        while (_monitorCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);
            PollVitals();
        }
    }

    private static void PollVitals()
    {
        var body = BodyUtil.Body;
        if (!body) return;

        var cam = PlayerCamera.main;
        if (!cam || cam.body != body) return;

        // 心脏骤停
        var cardiacArrest = body.inCardiacArrest;
        if (cardiacArrest != _wasCardiacArrest)
        {
            EventUtil.Trigger(new BodyCardiacArrestEvent
                { Body = body, Camera = cam, IsCardiacArrest = cardiacArrest });
            _wasCardiacArrest = cardiacArrest;
        }

        // 心室颤动
        var fibrillating = body.fibrillationProgress > 0f;
        switch (fibrillating)
        {
            case true when !_wasFibrillating:
                EventUtil.Trigger(new BodyFibrillationStartEvent { Body = body, Camera = cam });
                break;
            case false when _wasFibrillating:
                EventUtil.Trigger(new BodyFibrillationEndEvent { Body = body, Camera = cam });
                break;
        }

        _wasFibrillating = fibrillating;

        // 呼吸
        if (body.breathing != _wasBreathing)
        {
            EventUtil.Trigger(new BodyBreathChangeEvent { Body = body, Camera = cam, IsBreathing = body.breathing });
            _wasBreathing = body.breathing;
        }

        // 意识
        var conscious = body.conscious;
        if (conscious != _wasConscious)
        {
            EventUtil.Trigger(new BodyConsciousnessChangeEvent { Body = body, Camera = cam, IsConscious = conscious });
            _wasConscious = conscious;
        }

        // 濒死
        var brainDying = body.brainDying;
        if (brainDying != _wasBrainDying)
        {
            EventUtil.Trigger(new BodyBrainDyingEvent { Body = body, Camera = cam, IsBrainDying = brainDying });
            _wasBrainDying = brainDying;
        }

        // 锻炼
        if (body.exercising != _wasExercising)
        {
            if (body.exercising)
                EventUtil.Trigger(new BodyExerciseStartEvent { Body = body, Camera = cam });
            else
                EventUtil.Trigger(new BodyExerciseEndEvent { Body = body, Camera = cam });
            _wasExercising = body.exercising;
        }

        // 下蹲
        if (body.crouching != _wasCrouching)
        {
            EventUtil.Trigger(new BodyCrouchChangeEvent { Body = body, Camera = cam, IsCrouching = body.crouching });
            _wasCrouching = body.crouching;
        }

        // 睡眠
        if (body.sleeping != _wasSleeping)
        {
            EventUtil.Trigger(new BodySleepChangeEvent { Body = body, Camera = cam, IsSleeping = body.sleeping });
            _wasSleeping = body.sleeping;
        }
    }

    private static bool IsPlayerBody(Body body)
    {
        var cam = PlayerCamera.main;
        return body != null && cam != null && cam.body == body;
    }
}