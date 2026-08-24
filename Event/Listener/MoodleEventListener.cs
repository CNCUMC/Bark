using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bark.Events;
using Bark.Moodle;
using Bark.Tool;
using CUCoreLib.Registries;
using HarmonyLib;
using UnityEngine;

namespace Bark.Event.Listener;

// Moodle 事件监听器：通过 Harmony 补丁拦截 MoodleRegistry 的 AddMoodle / AddAnimatedMoodle，
// 配合轮询检测到期消失，触发 MoodleGet / MoodleIterate / MoodleLose 事件。
[HarmonyPatch(typeof(MoodleRegistry))]
public static class MoodleEventListener
{
    private const float PollInterval = 0.5f;

    private static readonly Dictionary<string, MoodleTracker> ActiveMoodles = new();
    private static Coroutine? _pollCoroutine;
    private static MonoBehaviour? _runner;

    // 启动 / 停止
    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;

        // AddMoodle(string/Sprite) / AddAnimatedMoodle 补丁已由 [HarmonyPatch] 注解声明，
        // 由 Plugin._harmony.PatchAll() 统一加载，这里只启动轮询协程。
        _pollCoroutine ??= runner.StartCoroutine(PollMoodles());
    }

    internal static void Stop()
    {
        if (_pollCoroutine != null && _runner != null)
        {
            _runner.StopCoroutine(_pollCoroutine);
            _pollCoroutine = null;
        }

        ActiveMoodles.Clear();
        _runner = null;
    }

    // Harmony 前缀钩子（注解形式，由 Plugin._harmony.PatchAll() 自动加载）
    // 三个重载参数结构一致（int, sprite|string|string, string, string, bool, bool, bool, string, float），
    // 仅第 2 个参数类型不同。用 (Type, string, Type[]) 位置参数形式显式声明 argumentTypes，
    // 让 Harmony 正确区分 AddMoodle 的两个重载（命名参数 argumentTypes: 会导致 args=undefined 的歧义异常）。

    [HarmonyPatch("AddMoodle",
        typeof(int),
        typeof(string),
        typeof(string),
        typeof(string),
        typeof(bool),
        typeof(bool),
        typeof(bool),
        typeof(string),
        typeof(float))]
    [HarmonyPrefix]
    private static void AddMoodleStringPrefix(
        int intensity,
        string iconId,
        string name,
        string description,
        bool critical,
        bool chippedOnly,
        bool important,
        string key,
        float holdSeconds)
    {
        TrackMoodle(intensity, name, critical, key, holdSeconds);
    }

    [HarmonyPatch("AddMoodle",
        typeof(int),
        typeof(Sprite),
        typeof(string),
        typeof(string),
        typeof(bool),
        typeof(bool),
        typeof(bool),
        typeof(string),
        typeof(float))]
    [HarmonyPrefix]
    private static void AddMoodleSpritePrefix(
        int intensity,
        Sprite icon,
        string name,
        string description,
        bool critical,
        bool chippedOnly,
        bool important,
        string key,
        float holdSeconds)
    {
        TrackMoodle(intensity, name, critical, key, holdSeconds);
    }

    [HarmonyPatch("AddAnimatedMoodle")]
    [HarmonyPrefix]
    private static void AddAnimatedMoodlePrefix(
        int intensity,
        string animationId,
        string name,
        string description,
        bool critical,
        bool chippedOnly,
        bool important,
        string key,
        float holdSeconds)
    {
        TrackMoodle(intensity, name, critical, key, holdSeconds);
    }


    private static void TrackMoodle(int intensity, string name, bool critical, string key, float holdSeconds)
    {
        if (string.IsNullOrEmpty(key))
            return;

        // 查找自定义 MoodleDef 以获取 can_heal 标记。非自定义 moodle 视为 can_heal=true（heal 时保留）
        var canHeal = MoodleLoader.LoadedMoodleDefs.TryGetValue(key, out var def) && def.CanHeal;

        // 记录或更新追踪信息（同 key 刷新会覆盖旧的过期时间）
        // tracker 只保留轮询检测消失所需的字段；intensity/critical 在下方直接填入 MoodleGetEvent
        var expireTime = Time.time + holdSeconds;
        ActiveMoodles[key] = new MoodleTracker(key, name, expireTime, canHeal);

        EventUtil.Trigger(new MoodleGetEvent
        {
            MoodleKey = key,
            MoodleName = name,
            Intensity = intensity,
            Critical = critical,
            HoldSeconds = holdSeconds
        });
    }

    // 轮询：到期消失检测 + 遍历事件
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

    // 公开查询接口（供 MoodleUtil 等工具使用）
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
        return [.. ActiveMoodles.Keys];
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

    // heal 时清除所有 can_heal=false 的自定义 Moodle（即"可被治疗清除"的状态）
    // can_heal=true 的 moodle 视为需要保留的状态（如永久效果、特殊标记等）
    internal static int ClearMoodlesOnHeal()
    {
        var keysToExpire = ActiveMoodles
            .Where(kv => !kv.Value.CanHeal)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in keysToExpire)
            if (ActiveMoodles.TryGetValue(key, out var tracker))
                tracker.ExpireTime = Time.time - 1f;

        return keysToExpire.Count;
    }

    // 内部类型
    private sealed class MoodleTracker(
        string key,
        string name,
        float expireTime,
        bool canHeal)
    {
        public readonly bool CanHeal = canHeal;
        public readonly string Key = key;
        public readonly string Name = name;
        public float ExpireTime = expireTime;
    }
}