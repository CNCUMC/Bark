using System.Collections;
using Bark.Events;
using Bark.Tool;
using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;

namespace Bark.Event.Listener;

public static class PlayerEventListener
{
    private const float JumpCooldown = 0.3f;
    private const float JumpFullTimeout = 5f;
    private const float LandStableThreshold = 0.001f;
    private const int LandStableFrames = 3;

    private static bool _wasAlive;
    private static bool _isJumping;
    private static bool _worldReady;
    private static float _lastJumpTime = float.MinValue;
    private static Coroutine? _monitorCoroutine;
    private static Coroutine? _jumpMonitorCoroutine;
    private static MonoBehaviour? _runner;

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;
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
        _worldReady = false;
        _runner = null;
    }

    [HarmonyPatch(typeof(Body), "Jump")]
    [HarmonyPrefix]
    private static void BodyJumpPrefix(Body __instance)
    {
        if (!_worldReady) return;

        var cam = PlayerCamera.main;
        if (cam == null) return;
        if (cam.body != __instance) return;

        if (_isJumping || Time.time - _lastJumpTime < JumpCooldown)
        {
            _jumpMonitorCoroutine ??=
                _runner!.StartCoroutine(MonitorJump(__instance, cam));
            return;
        }

        _lastJumpTime = Time.time;
        _isJumping = true;

        _jumpMonitorCoroutine ??= _runner!.StartCoroutine(MonitorJump(__instance, cam));

        EventUtil.Trigger(new PlayerJumpStartEvent
        {
            Body = __instance,
            Camera = cam
        });
    }

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
                if (deltaY < 0f)
                    falling = true;
            }
            else
            {
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

        EventUtil.Trigger(new PlayerJumpOverEvent
        {
            Body = body,
            Camera = camera
        });
    }

    private static IEnumerator MonitorPlayer()
    {
        yield return CUCoreUtils.AwaitWorldGeneration();

        var body = BodyUtil.Body;
        _wasAlive = body.alive;
        _worldReady = true;

        while (_monitorCoroutine != null)
        {
            yield return new WaitForSeconds(0.5f);
            PollPlayer();
        }
    }

    private static void PollPlayer()
    {
        var body = BodyUtil.Body;
        if (!body) return;

        var isAlive = body.alive;
        if (_wasAlive && !isAlive)
            EventUtil.Trigger(new PlayerDeathEvent
            {
                Body = body,
                Camera = PlayerCamera.main
            });
        _wasAlive = isAlive;
    }
}
