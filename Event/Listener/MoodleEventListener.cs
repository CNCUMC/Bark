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

[HarmonyPatch(typeof(MoodleRegistry))]
public static class MoodleEventListener
{
    private const float PollInterval = 0.5f;

    private static readonly Dictionary<string, MoodleTracker> ActiveMoodles = new();
    private static Coroutine? _pollCoroutine;
    private static MonoBehaviour? _runner;

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;

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

        var canHeal = MoodleLoader.LoadedMoodleDefs.TryGetValue(key, out var def) && def.CanHeal;

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

    private static IEnumerator PollMoodles()
    {
        while (_pollCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            if (ActiveMoodles.Count == 0)
                continue;

            var now = Time.time;

            var activeKeys = ActiveMoodles.Keys.ToArray();
            EventUtil.Trigger(new MoodleIterateEvent
            {
                ActiveKeys = activeKeys
            });

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

    internal static bool HasMoodle(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;
        return ActiveMoodles.ContainsKey(key);
    }

    internal static string[] GetActiveMoodleKeys()
    {
        return [.. ActiveMoodles.Keys];
    }

    internal static int GetMoodleCount()
    {
        return ActiveMoodles.Count;
    }

    internal static bool ForceExpire(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;
        if (!ActiveMoodles.TryGetValue(key, out var tracker))
            return false;

        tracker.ExpireTime = Time.time - 1f;
        return true;
    }

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