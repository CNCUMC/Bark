using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bark.Events;
using Bark.Tool;
using CUCoreLib.Registries;
using HarmonyLib;
using UnityEngine;

namespace Bark.Event.Listener;

// Moodle 事件监听器：通过 Harmony 补丁拦截 MoodleRegistry 的 AddMoodle / AddAnimatedMoodle，
// 配合轮询检测到期消失，触发 MoodleGet / MoodleIterate / MoodleLose 事件。
public static class MoodleEventListener
{
    private const float PollInterval = 0.5f;

    private static readonly Dictionary<string, MoodleTracker> ActiveMoodles = new();
    private static Coroutine? _pollCoroutine;
    private static MonoBehaviour? _runner;
    private static Harmony? _harmony;

    // ============================================================
    // 启动 / 停止
    // ============================================================

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;
        _harmony = new Harmony("Bark.MoodleEventListener");

        // 补丁 MoodleRegistry.AddMoodle（两个重载：string iconId / Sprite sprite）
        var addMoodleMethods = typeof(MoodleRegistry)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "AddMoodle")
            .ToArray();

        foreach (var method in addMoodleMethods)
        {
            _harmony.Patch(method,
                prefix: new HarmonyMethod(typeof(MoodleEventListener), nameof(OnAddMoodle)));
        }

        // 补丁 MoodleRegistry.AddAnimatedMoodle
        var addAnimMethod = typeof(MoodleRegistry).GetMethod("AddAnimatedMoodle",
            BindingFlags.Public | BindingFlags.Static);
        if (addAnimMethod != null)
        {
            _harmony.Patch(addAnimMethod,
                prefix: new HarmonyMethod(typeof(MoodleEventListener), nameof(OnAddAnimatedMoodle)));
        }

        _pollCoroutine = runner.StartCoroutine(PollMoodles());
    }

    internal static void Stop()
    {
        if (_pollCoroutine != null && _runner != null)
        {
            _runner.StopCoroutine(_pollCoroutine);
            _pollCoroutine = null;
        }

        _harmony?.UnpatchSelf();
        _harmony = null;
        ActiveMoodles.Clear();
        _runner = null;
    }

    // ============================================================
    // Harmony 前缀钩子
    // ============================================================

    // 三个重载的参数结构一致：
    //   0: int intensity
    //   1: string iconId / Sprite sprite / string animationId
    //   2: string name
    //   3: string description
    //   4: bool critical
    //   5: bool chippedOnly
    //   6: bool important
    //   7: string key
    //   8: float holdSeconds
    private static void OnAddMoodle(object[] __args)
    {
        TrackMoodle(__args);
    }

    private static void OnAddAnimatedMoodle(object[] __args)
    {
        TrackMoodle(__args);
    }

    private static void TrackMoodle(object[] args)
    {
        if (args == null || args.Length < 9)
            return;

        var intensity = args[0] is int i ? i : 0;
        var name = args[2] as string ?? string.Empty;
        var critical = args.Length > 5 && args[5] is true;
        var key = args[7] as string ?? string.Empty;
        var holdSeconds = args[8] is float f ? f : 0.75f;

        if (string.IsNullOrEmpty(key))
            return;

        // 记录或更新追踪信息（同 key 刷新会覆盖旧的过期时间）
        var expireTime = Time.time + holdSeconds;
        ActiveMoodles[key] = new MoodleTracker(key, name, intensity, critical, expireTime);

        EventUtil.Trigger(new MoodleGetEvent
        {
            MoodleKey = key,
            MoodleName = name,
            Intensity = intensity,
            Critical = critical,
            HoldSeconds = holdSeconds
        });
    }

    // ============================================================
    // 轮询：到期消失检测 + 遍历事件
    // ============================================================

    private static IEnumerator PollMoodles()
    {
        while (_pollCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            if (ActiveMoodles.Count == 0)
                continue;

            var now = Time.time;

            // 获取当前活跃 key 列表（在检查过期之前），触发 iterate
            var activeKeys = ActiveMoodles.Keys.ToArray();
            EventUtil.Trigger(new MoodleIterateEvent
            {
                ActiveKeys = activeKeys
            });

            // 检查已过期的 moodle，触发 lose
            var expired = ActiveMoodles
                .Where(kv => kv.Value.ExpireTime <= now)
                .Select(kv => kv.Value)
                .ToList();

            foreach (var tracker in expired)
            {
                ActiveMoodles.Remove(tracker.Key);
                EventUtil.Trigger(new MoodleLoseEvent
                {
                    MoodleKey = tracker.Key,
                    MoodleName = tracker.Name
                });
            }
        }
    }

    // ============================================================
    // 公开查询接口（供 MoodleUtil 等工具使用）
    // ============================================================

    // 检查指定 key 的 moodle 是否当前活跃
    internal static bool HasMoodle(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;
        return ActiveMoodles.ContainsKey(key);
    }

    // 获取当前所有活跃 moodle 的 key 列表
    internal static string[] GetActiveMoodleKeys()
    {
        return ActiveMoodles.Keys.ToArray();
    }

    // 获取当前活跃 moodle 数量
    internal static int GetMoodleCount()
    {
        return ActiveMoodles.Count;
    }

    // 强制到期指定 key 的 moodle，下次轮询时触发 Lose 事件并移除
    // 返回 true 表示该 moodle 存在并已标记到期
    internal static bool ForceExpire(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;
        if (!ActiveMoodles.TryGetValue(key, out var tracker))
            return false;

        // 将过期时间设为已过去，下次 PollMoodles 会触发 lose
        tracker.ExpireTime = Time.time - 1f;
        return true;
    }

    // ============================================================
    // 内部类型
    // ============================================================

    private sealed class MoodleTracker(
        string key,
        string name,
        int intensity,
        bool critical,
        float expireTime)
    {
        public readonly string Key = key;
        public readonly string Name = name;
        public readonly int Intensity = intensity;
        public readonly bool Critical = critical;
        public float ExpireTime = expireTime;
    }
}
