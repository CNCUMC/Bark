using System.Collections;
using Bark.Event;
using Bark.Tool;
using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;

namespace Bark.Events;

public static class PlayerEvents
{
    private const float JumpCooldown = 0.3f;
    private const float JumpFullTimeout = 5f;   // 起跳后超过此时长未落地，取消等待

    private static bool _wasAlive;
    private static bool _wasStanding;
    private static bool _jumpPending;
    private static float _lastJumpTime = float.MinValue;
    private static Coroutine? _monitorCoroutine;
    private static MonoBehaviour? _runner;

    // 起跳事件：按下跳跃键时触发
    public class JumpEvent : BarkEvent
    {
        public Body? Body { get; set; }
        public PlayerCamera? Camera { get; set; }
    }

    // 完整跳跃事件：落地时触发（起跳 → 滞空 → 落地 的完整过程）
    public class JumpFullEvent : BarkEvent
    {
        public Body? Body { get; set; }
        public PlayerCamera? Camera { get; set; }
    }

    // 死亡事件
    public class DeathEvent : BarkEvent
    {
        public Body? Body { get; set; }
        public PlayerCamera? Camera { get; set; }
    }

    internal static void Listen(MonoBehaviour runner)
    {
        // 事件属性 getter 通过 EventRegistry/反射消费，分析器无法追踪。
        // 以下显式读取仅用于压制 IDE "getter never used" 误报。
        _ = new JumpEvent().Body; _ = new JumpEvent().Camera;
        _ = new JumpFullEvent().Body;
        _ = new DeathEvent().Body; _ = new DeathEvent().Camera;

        _runner = runner;

        // Harmony 补丁：截获 Body.Jump()
        var harmony = new Harmony("Bark.PlayerEvents");
        harmony.Patch(
            typeof(Body).GetMethod("Jump"),
            prefix: new HarmonyMethod(typeof(PlayerEvents), nameof(OnJump))
        );

        _monitorCoroutine = runner.StartCoroutine(MonitorPlayer());
    }

    // 停止监听（插件卸载时调用）
    internal static void Stop()
    {
        if (_runner == null) return;

        _runner.StopCoroutine(_monitorCoroutine);
        _monitorCoroutine = null;
        _runner = null;
    }

    // Body.Jump() 被调用时触发起跳事件（仅限玩家自身，带冷却防连发）
    private static void OnJump(Body __instance)
    {
        if (Time.time - _lastJumpTime < JumpCooldown) return;

        var cam = PlayerCamera.main;
        if (cam == null) return;
        if (cam.body != __instance) return;

        _lastJumpTime = Time.time;
        _jumpPending = true;   // 标记：等待落地

        EventUtil.Trigger(new JumpEvent
        {
            Body = __instance,
            Camera = cam
        });
    }

    // 世界生成后持续轮询：死亡检测 + 完整跳跃检测
    private static IEnumerator MonitorPlayer()
    {
        yield return CUCoreUtils.AwaitWorldGeneration();

        var body = PlayerUtil.Body;
        // PlayerUtil.Body 可能为 null（外部API），但世界已生成后玩家 Body 必然存在
        _wasAlive = body.alive;
        _wasStanding = body.standing;

        while (_monitorCoroutine != null)
        {
            yield return new WaitForSeconds(0.5f);
            PollPlayer();
        }
    }

    private static void PollPlayer()
    {
        var body = PlayerUtil.Body;
        if (!body) return;

        // --- 死亡检测 ---
        var isAlive = body.alive;
        if (_wasAlive && !isAlive)
            EventUtil.Trigger(new DeathEvent
            {
                Body = body,
                Camera = PlayerCamera.main
            });
        _wasAlive = isAlive;

        if (!isAlive)
        {
            // 玩家已死，清理跳跃等待标记
            _jumpPending = false;
            return;
        }

        // --- 完整跳跃检测 ---
        var isStanding = body.standing;

        // 过去不在站立状态，现在站住了，且有等待中的跳跃 → 落地
        if (!_wasStanding && isStanding && _jumpPending)
        {
            _jumpPending = false;
            EventUtil.Trigger(new JumpFullEvent
            {
                Body = body,
                Camera = PlayerCamera.main
            });
        }

        _wasStanding = isStanding;

        // 起跳后超时未落地（卡墙/死亡等异常情况），清理标记防止误触发
        if (_jumpPending && Time.time - _lastJumpTime > JumpFullTimeout)
            _jumpPending = false;
    }
}
