using Bark.Events;
using Bark.Tool;
using HarmonyLib;

namespace Bark.Event.Listener;

// 小游戏事件监听器：通过 Harmony 补丁拦截小游戏类方法，触发小游戏相关事件。
// 覆盖 AED 除颤小游戏（开始/除颤/失败）与包扎小游戏（开始/缠绕完成）。
public static class MinigameEventListener
{
    // 记录 AED 上一帧状态，用于边沿检测
    private static int _lastAedState = -1;

    // 记录脱臼复位成功状态，避免重复触发
    private static int _lastDislocationReset = -1;

    // 撬锁成功标记（避免重复触发）
    private static bool _lockpickDone;

    // 撬锁卡住标记（避免重复触发）
    private static bool _lockpickStuck;

    // 手动除颤结束标记（避免重复触发）
    private static bool _manualDefibDone;

    // 手动除颤上次放电次数
    private static int _lastManualDefibShocks;

    // 取弹片完成标记
    private static bool _shrapnelDone;

    // 注射完成标记
    private static bool _syringeDone;
    // 截肢完成标记
    private static bool _amputationDone;

    internal static void Listen()
    {
        PatchAED();
        PatchBandage();
        PatchDislocation();
        PatchHandCrank();
        PatchKeypad();
        PatchLockping();
        PatchManualDefib();
        PatchShrapnel();
        PatchSyringe();
        PatchAmputation();
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

    private static void PatchAED()
    {
        // AEDMinigame.Start()：小游戏开始
        var start = AccessTools.Method(typeof(AEDMinigame), "Start");
        if (start != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.AEDStart");
                harmony.Patch(start,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnAEDStartPostfix)));
            }
            catch
            {
                // ignored
            }
        }

        // AEDMinigame.Update(List<RaycastResult>)：状态流转（除颤/失败）
        var update = AccessTools.Method(typeof(AEDMinigame), "Update");
        if (update != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.AEDUpdate");
                harmony.Patch(update,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnAEDUpdatePostfix)));
            }
            catch
            {
                // ignored
            }
        }
    }

    private static void PatchBandage()
    {
        // BandageMinigame.Start()：小游戏开始
        var start = AccessTools.Method(typeof(BandageMinigame), "Start");
        if (start != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.BandageStart");
                harmony.Patch(start,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnBandageStartPostfix)));
            }
            catch
            {
                // ignored
            }
        }

        // BandageMinigame.DoBandageAction()：完成一圈缠绕
        var doAction = AccessTools.Method(typeof(BandageMinigame), "DoBandageAction");
        if (doAction != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.BandageWrap");
                harmony.Patch(doAction,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnBandageWrapPostfix)));
            }
            catch
            {
                // ignored
            }
        }
    }

    private static void PatchDislocation()
    {
        // DislocationMinigame.Start()：小游戏开始
        var start = AccessTools.Method(typeof(DislocationMinigame), "Start");
        if (start != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.DislocationStart");
                harmony.Patch(start,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnDislocationStartPostfix)));
            }
            catch
            {
                // ignored
            }
        }

        // DislocationMinigame.Update(List<RaycastResult>)：检测复位成功
        var update = AccessTools.Method(typeof(DislocationMinigame), "Update");
        if (update != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.DislocationUpdate");
                harmony.Patch(update,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnDislocationUpdatePostfix)));
            }
            catch
            {
                // ignored
            }
        }
    }

    // ============================================================
    // AED 除颤小游戏
    // ============================================================

    private static void OnAEDStartPostfix(AEDMinigame __instance)
    {
        if (__instance.limb == null) return;

        _lastAedState = 0;
        EventUtil.Trigger(new AEDMinigameStartEvent
        {
            Limb = __instance.limb,
            LimbIndex = GetLimbIndex(__instance.limb)
        });
    }

    private static void OnAEDUpdatePostfix(AEDMinigame __instance)
    {
        if (__instance.limb == null) return;

        // 读取私有 state 字段
        var state = Traverse.Create(__instance).Field("state").GetValue<int>();
        if (state == _lastAedState) return;

        var prev = _lastAedState;
        _lastAedState = state;

        switch (state)
        {
            // state==4：除颤成功（battery 放电）
            case 4 when prev != 4:
                EventUtil.Trigger(new AEDMinigameDefibrillateEvent
                {
                    Limb = __instance.limb,
                    WasFibrillating = __instance.limb.body.fibrillationProgress > 0f
                });
                break;
            // state==5：分析失败（未检测到可除颤心律）
            case 5 when prev != 5:
                EventUtil.Trigger(new AEDMinigameFailEvent { Limb = __instance.limb });
                break;
        }
    }

    // ============================================================
    // 包扎小游戏
    // ============================================================

    private static void OnBandageStartPostfix(BandageMinigame __instance)
    {
        if (__instance.limb == null) return;

        EventUtil.Trigger(new BandageMinigameStartEvent
        {
            Limb = __instance.limb,
            BandageAngle = __instance.bandageAngle
        });
    }

    private static void OnBandageWrapPostfix(BandageMinigame __instance)
    {
        if (__instance.limb == null) return;

        EventUtil.Trigger(new BandageMinigameWrapEvent { Limb = __instance.limb });
    }

    // ============================================================
    // 脱臼复位小游戏
    // ============================================================

    private static void OnDislocationStartPostfix(DislocationMinigame __instance)
    {
        var limb = GetDislocationLimb(__instance);
        if (limb == null) return;

        EventUtil.Trigger(new DislocationMinigameStartEvent
        {
            Limb = limb,
            HasWrench = __instance.hasWrench
        });
    }

    private static void OnDislocationUpdatePostfix(DislocationMinigame __instance)
    {
        var limb = GetDislocationLimb(__instance);
        if (limb == null) return;

        // 复位成功：dislocationTimer < 3 时方法内调用 UnDislocate 并结束小游戏
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

    private static Limb? GetDislocationLimb(DislocationMinigame instance)
    {
        return Traverse.Create(instance).Field("limb").GetValue<Limb>();
    }

    // ============================================================
    // 手摇曲柄小游戏
    // ============================================================

    private static void PatchHandCrank()
    {
        var start = AccessTools.Method(typeof(HandCrankMinigame), "Start");
        if (start != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.HandCrankStart");
                harmony.Patch(start,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnHandCrankStartPostfix)));
            }
            catch
            {
                // ignored
            }
        }

        // PhysicsUpdate：转动曲柄充电（held=true 时）
        var physics = AccessTools.Method(typeof(HandCrankMinigame), "PhysicsUpdate");
        if (physics != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.HandCrankCharge");
                harmony.Patch(physics,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnHandCrankPhysicsPostfix)));
            }
            catch
            {
                // ignored
            }
        }

        // Update：耐力耗尽结束小游戏
        var update = AccessTools.Method(typeof(HandCrankMinigame), "Update");
        if (update != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.HandCrankEnd");
                harmony.Patch(update,
                    prefix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnHandCrankUpdatePrefix)));
            }
            catch
            {
                // ignored
            }
        }
    }

    private static void OnHandCrankStartPostfix()
    {
        EventUtil.Trigger(new HandCrankMinigameStartEvent());
    }

    private static void OnHandCrankPhysicsPostfix(HandCrankMinigame __instance)
    {
        var held = Traverse.Create(__instance).Field("held").GetValue<bool>();
        if (!held) return;

        // 读取曲柄 z 旋转，计算转动角度（由监听器按帧差异上报，简化用 0）
        EventUtil.Trigger(new HandCrankMinigameChargeEvent { Angle = 0f });
    }

    private static void OnHandCrankUpdatePrefix()
    {
        // Update 开头：耐力不足时 EndMinigame，触发结束事件
        if (Minigame.game != null && Minigame.game.body != null && Minigame.game.body.stamina < 15f)
            EventUtil.Trigger(new HandCrankMinigameEndEvent());
    }

    // ============================================================
    // 键盘密码小游戏
    // ============================================================

    private static void PatchKeypad()
    {
        var start = AccessTools.Method(typeof(KeypadMinigame), "Start");
        if (start != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.KeypadStart");
                harmony.Patch(start,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnKeypadStartPostfix)));
            }
            catch
            {
                // ignored
            }
        }

        var update = AccessTools.Method(typeof(KeypadMinigame), "Update");
        if (update == null) return;

        try
        {
            var harmony = new Harmony("Bark.Minigame.KeypadUpdate");
            harmony.Patch(update,
                postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnKeypadUpdatePostfix)));
        }
        catch
        {
            // ignored
        }
    }

    private static void OnKeypadStartPostfix(KeypadMinigame __instance)
    {
        if (__instance.toDestroy == null) return;
        EventUtil.Trigger(new KeypadMinigameStartEvent { ToDestroy = __instance.toDestroy });
    }

    private static void OnKeypadUpdatePostfix(KeypadMinigame __instance)
    {
        if (__instance.toDestroy == null) return;

        // 密码正确时方法内将 toDestroy.health 置 0 并结束
        if (__instance.current == __instance.match)
            EventUtil.Trigger(new KeypadMinigameSuccessEvent { ToDestroy = __instance.toDestroy });
    }

    // ============================================================
    // 撬锁小游戏
    // ============================================================

    private static void PatchLockping()
    {
        var start = AccessTools.Method(typeof(LockpingMinigame), "Start");
        if (start != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.LockpingStart");
                harmony.Patch(start,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnLockpingStartPostfix)));
            }
            catch
            {
                // ignored
            }
        }

        var update = AccessTools.Method(typeof(LockpingMinigame), "Update");
        if (update == null) return;
        try
        {
            var harmony = new Harmony("Bark.Minigame.LockpingUpdate");
            harmony.Patch(update,
                postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnLockpingUpdatePostfix)));
        }
        catch
        {
            // ignored
        }
    }

    private static void OnLockpingStartPostfix(LockpingMinigame __instance)
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

    private static void OnLockpingUpdatePostfix(LockpingMinigame __instance)
    {
        if (__instance.toDestroy == null) return;

        // 撬锁成功：方法内将 toDestroy.health 置 0
        if (__instance.toDestroy.health <= 0f)
        {
            if (_lockpickDone) return;
            _lockpickDone = true;
            EventUtil.Trigger(new LockpingMinigameSuccessEvent { ToDestroy = __instance.toDestroy });
            return;
        }

        // 卡住：timeWasStuck > 0.5 时损坏工具/手指
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

    // ============================================================
    // 手动除颤小游戏
    // ============================================================

    private static void PatchManualDefib()
    {
        var start = AccessTools.Method(typeof(ManualDefibMinigame), "Start");
        if (start != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.ManualDefibStart");
                harmony.Patch(start,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnManualDefibStartPostfix)));
            }
            catch
            {
                // ignored
            }
        }

        var update = AccessTools.Method(typeof(ManualDefibMinigame), "Update");
        if (update != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.ManualDefibUpdate");
                harmony.Patch(update,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnManualDefibUpdatePostfix)));
            }
            catch
            {
                // ignored
            }
        }

        var physics = AccessTools.Method(typeof(ManualDefibMinigame), "PhysicsUpdate");
        if (physics != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.ManualDefibEnd");
                harmony.Patch(physics,
                    prefix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnManualDefibEndPrefix)));
            }
            catch
            {
                // ignored
            }
        }
    }

    private static void OnManualDefibStartPostfix(ManualDefibMinigame __instance)
    {
        if (__instance.limb == null) return;

        _manualDefibDone = false;
        EventUtil.Trigger(new ManualDefibMinigameStartEvent
        {
            Limb = __instance.limb,
            OnTorso = Traverse.Create(__instance).Field("onTorso").GetValue<bool>()
        });
    }

    private static void OnManualDefibUpdatePostfix(ManualDefibMinigame __instance)
    {
        if (__instance.limb == null) return;

        // 放电检测：shockCount 增加
        var shockCount = Traverse.Create(__instance).Field("shockCount").GetValue<int>();
        if (shockCount <= _lastManualDefibShocks) return;
        _lastManualDefibShocks = shockCount;
        var charge = Traverse.Create(__instance).Field("currentCharge").GetValue<float>();
        EventUtil.Trigger(new ManualDefibMinigameShockEvent { Limb = __instance.limb, Charge = charge });
    }

    private static void OnManualDefibEndPrefix(ManualDefibMinigame __instance)
    {
        if (__instance.limb == null) return;
        if (_manualDefibDone) return;

        // 电池耗尽时 Update 会 EndMinigame，PhysicsUpdate 前置检测
        if (Minigame.game == null
            || Minigame.game.currentItem == null
            || Minigame.game.currentItem.battery == null
            || Minigame.game.currentItem.battery.hasCharge) return;
        _manualDefibDone = true;
        EventUtil.Trigger(new ManualDefibMinigameEndEvent { Limb = __instance.limb });
    }

    // ============================================================
    // 取弹片小游戏
    // ============================================================

    private static void PatchShrapnel()
    {
        var start = AccessTools.Method(typeof(ShrapnelMinigame), "Start");
        if (start != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.ShrapnelStart");
                harmony.Patch(start,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnShrapnelStartPostfix)));
            }
            catch
            {
                // ignored
            }
        }

        var update = AccessTools.Method(typeof(ShrapnelMinigame), "Update");
        if (update != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.ShrapnelUpdate");
                harmony.Patch(update,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnShrapnelUpdatePostfix)));
            }
            catch
            {
                // ignored
            }
        }

        var breakGrasp = AccessTools.Method(typeof(ShrapnelMinigame), "BreakGrasp");
        if (breakGrasp == null) return;

        try
        {
            var harmony = new Harmony("Bark.Minigame.ShrapnelFail");
            harmony.Patch(breakGrasp,
                prefix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnShrapnelFailPrefix)));
        }
        catch
        {
            // ignored
        }
    }

    private static void OnShrapnelStartPostfix(ShrapnelMinigame __instance)
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

    private static void OnShrapnelUpdatePostfix(ShrapnelMinigame __instance)
    {
        var limb = GetShrapnelLimb(__instance);
        if (limb == null) return;

        // 成功：shrapnel == 0（所有弹片取出）
        if (limb.shrapnel != 0 || _shrapnelDone) return;
        _shrapnelDone = true;
        EventUtil.Trigger(new ShrapnelMinigameSuccessEvent { Limb = limb });
    }

    private static void OnShrapnelFailPrefix(ShrapnelMinigame __instance)
    {
        var limb = GetShrapnelLimb(__instance);
        if (limb == null) return;

        EventUtil.Trigger(new ShrapnelMinigameFailEvent { Limb = limb });
    }

    private static Limb? GetShrapnelLimb(ShrapnelMinigame instance)
    {
        return Traverse.Create(instance).Field("limb").GetValue<Limb>();
    }

    // ============================================================
    // 注射小游戏
    // ============================================================

    private static void PatchSyringe()
    {
        var start = AccessTools.Method(typeof(SyringeMinigame), "Start");
        if (start != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.SyringeStart");
                harmony.Patch(start,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnSyringeStartPostfix)));
            }
            catch
            {
                // ignored
            }
        }

        var update = AccessTools.Method(typeof(SyringeMinigame), "Update");
        if (update == null) return;

        try
        {
            var harmony = new Harmony("Bark.Minigame.SyringeUpdate");
            harmony.Patch(update,
                postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnSyringeUpdatePostfix)));
        }
        catch
        {
            // ignored
        }
    }

    private static void OnSyringeStartPostfix(SyringeMinigame __instance)
    {
        var limb = GetSyringeLimb(__instance);
        if (limb == null) return;

        _syringeDone = false;
        EventUtil.Trigger(new SyringeMinigameStartEvent { Limb = limb });
    }

    private static void OnSyringeUpdatePostfix(SyringeMinigame __instance)
    {
        var limb = GetSyringeLimb(__instance);
        if (limb == null) return;

        // 注射推进检测：wasInjectingBefore 从 false → true
        var injecting = Traverse.Create(__instance).Field("wasInjectingBefore").GetValue<bool>();
        if (injecting && !_syringeDone)
        {
            _syringeDone = true;
            EventUtil.Trigger(new SyringeMinigameInjectEvent { Limb = limb });
        }

        // 失败检测：扎偏导致 shrapnel 增加（偏移 > 80）
        if (limb.shrapnel > 0)
            EventUtil.Trigger(new SyringeMinigameFailEvent { Limb = limb });
    }

    private static Limb? GetSyringeLimb(SyringeMinigame instance)
    {
        return Traverse.Create(instance).Field("limb").GetValue<Limb>();
    }

    // ============================================================
    // 截肢小游戏
    // ============================================================

    private static void PatchAmputation()
    {
        var start = AccessTools.Method(typeof(AmputationMinigame), "Start");
        if (start != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.AmputationStart");
                harmony.Patch(start,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnAmputationStartPostfix)));
            }
            catch
            {
                // ignored
            }
        }

        var update = AccessTools.Method(typeof(AmputationMinigame), "Update");
        if (update != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Minigame.AmputationUpdate");
                harmony.Patch(update,
                    postfix: new HarmonyMethod(typeof(MinigameEventListener), nameof(OnAmputationUpdatePostfix)));
            }
            catch
            {
                // ignored
            }
        }
    }

    private static void OnAmputationStartPostfix(AmputationMinigame __instance)
    {
        if (__instance == null || __instance.limb == null) return;

        _amputationDone = false;
        EventUtil.Trigger(new AmputationMinigameStartEvent { Limb = __instance.limb });
    }

    private static void OnAmputationUpdatePostfix(AmputationMinigame __instance)
    {
        if (__instance == null || __instance.limb == null) return;

        // 截断成功：cutProgress >= 1 时方法内调用 Dismember
        if (__instance.cutProgress >= 1f && !_amputationDone)
        {
            _amputationDone = true;
            EventUtil.Trigger(new AmputationMinigameSuccessEvent { Limb = __instance.limb });
        }
    }

    // 获取肢体在 body.limbs 中的索引
    private static int GetLimbIndex(Limb limb)
    {
        if (limb.body == null || limb.body.limbs == null) return -1;
        for (var i = 0; i < limb.body.limbs.Length; i++)
            if (limb.body.limbs[i] == limb)
                return i;
        return -1;
    }
}