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
    private const float JumpFullTimeout = 5f;
    private const float LandStableThreshold = 0.001f;
    private const int LandStableFrames = 3;

    private static bool _wasAlive;
    private static bool _isJumping;
    private static float _lastJumpTime = float.MinValue;
    private static Coroutine? _monitorCoroutine;
    private static Coroutine? _jumpMonitorCoroutine;
    private static MonoBehaviour? _runner;

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;

        var harmony = new Harmony("Bark.PlayerEvents");
        harmony.Patch(
            typeof(Body).GetMethod("Jump"),
            new HarmonyMethod(typeof(PlayerEvents), nameof(OnJump))
        );

        _monitorCoroutine = runner.StartCoroutine(MonitorPlayer());
    }

    internal static void Stop()
    {
        if (_runner == null) return;

        _runner.StopCoroutine(_monitorCoroutine);
        _monitorCoroutine = null;

        if (_jumpMonitorCoroutine != null)
        {
            _runner.StopCoroutine(_jumpMonitorCoroutine);
            _jumpMonitorCoroutine = null;
        }

        _isJumping = false;
        _runner = null;
    }

    // Body.Jump() 被调用时触发起跳事件（仅限玩家自身，带冷却防连发）
    private static void OnJump(Body __instance)
    {
        var cam = PlayerCamera.main;
        if (cam == null) return;
        if (cam.body != __instance) return;

        // 已有未完成的跳跃，不重复触发 start
        if (_isJumping)
        {
            // 无协程追踪则补启动（极端情况下协程丢失的兜底）
            _jumpMonitorCoroutine ??= _runner!.StartCoroutine(MonitorJump(__instance, cam));
            return;
        }

        if (Time.time - _lastJumpTime < JumpCooldown)
        {
            // 冷却期内起跳，不发 start 但仍需保证有协程追踪落地
            _jumpMonitorCoroutine ??= _runner!.StartCoroutine(MonitorJump(__instance, cam));
            return;
        }

        _lastJumpTime = Time.time;
        _isJumping = true;

        _jumpMonitorCoroutine ??= _runner!.StartCoroutine(MonitorJump(__instance, cam));

        EventUtil.Trigger(new JumpStartEvent
        {
            Body = __instance,
            Camera = cam
        });
    }

    // 通过 transform.position.y 检测完整跳跃：
    // Y 上升 → Y 开始下降 → Y 不再下降（触地）
    private static IEnumerator MonitorJump(Body body, PlayerCamera camera)
    {
        var startTime = Time.time;
        var previousY = body.transform.position.y;
        var stableFrames = 0;
        var falling = false;

        while (true)
        {
            yield return null;

            var currentY = body.transform.position.y;
            var deltaY = currentY - previousY;

            if (!falling)
            {
                // Y 开始下降 → 已过最高点，进入下落阶段
                if (deltaY < 0f)
                    falling = true;
            }
            else
            {
                // 下落阶段中 Y 趋于稳定 → 落地
                if (Mathf.Abs(deltaY) < LandStableThreshold)
                {
                    stableFrames++;
                    if (stableFrames >= LandStableFrames) break;
                }
                else
                {
                    stableFrames = 0;
                }
            }

            if (Time.time - startTime > JumpFullTimeout || !body.alive)
            {
                _jumpMonitorCoroutine = null;
                _isJumping = false;
                yield break;
            }

            previousY = currentY;
        }

        _jumpMonitorCoroutine = null;
        _isJumping = false;

        EventUtil.Trigger(new JumpOverEvent
        {
            Body = body,
            Camera = camera
        });
    }

    // 世界生成后持续轮询：死亡检测
    private static IEnumerator MonitorPlayer()
    {
        yield return CUCoreUtils.AwaitWorldGeneration();

        var body = PlayerUtil.Body;
        _wasAlive = body.alive;

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

        var isAlive = body.alive;
        if (_wasAlive && !isAlive)
            EventUtil.Trigger(new DeathEvent
            {
                Body = body,
                Camera = PlayerCamera.main
            });
        _wasAlive = isAlive;
    }

    // 起跳事件：按下跳跃键时触发
    [ScriptEvent("onPlayerJumpStart")]
    public class JumpStartEvent : BarkEvent
    {
        public Body Body { get; set; } = null!;
        public PlayerCamera Camera { get; set; } = null!;
    }

    // 跳跃结束事件：落地时触发（起跳 → 滞空 → 落地 的完整过程）
    [ScriptEvent("onPlayerJumpOver")]
    public class JumpOverEvent : BarkEvent
    {
        public Body Body { get; set; } = null!;
        public PlayerCamera Camera { get; set; } = null!;
    }

    // 死亡事件
    [ScriptEvent("onPlayerDeath")]
    public class DeathEvent : BarkEvent
    {
        public Body Body { get; set; } = null!;
        public PlayerCamera Camera { get; set; } = null!;
    }
}