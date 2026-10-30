using System;
using Bark.Event.Listener;
using Bark.Moodle;
using Bark.ScriptApi;
using CUCoreLib.Registries;

namespace Bark.Tool;

[ScriptApi]
public static class MoodleUtil
{
    [ScriptMethod]
    public static bool HasMoodle(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;
        return MoodleEventListener.HasMoodle(key);
    }

    [ScriptMethod]
    public static string[] GetActiveMoodles()
    {
        return MoodleEventListener.GetActiveMoodleKeys();
    }

    [ScriptMethod]
    public static int GetMoodleCount()
    {
        return MoodleEventListener.GetMoodleCount();
    }

    [ScriptMethod]
    public static void ApplyMoodle(string moodleKey, float holdSeconds = 0f)
    {
        if (string.IsNullOrEmpty(moodleKey))
            throw new ArgumentNullException(nameof(moodleKey));

        if (!MoodleLoader.LoadedMoodleDefs.TryGetValue(moodleKey, out var def))
        {
            LogUtil.Warning("moodle.apply_not_found", moodleKey);
            return;
        }

        var duration = holdSeconds > 0f
            ? holdSeconds
            : def.HoldSeconds;
        ApplyMoodleInternal(moodleKey, def, duration);
    }

    [ScriptMethod]
    public static bool RemoveMoodle(string moodleKey)
    {
        if (string.IsNullOrEmpty(moodleKey))
            return false;
        return MoodleEventListener.ForceExpire(moodleKey);
    }

    [ScriptMethod]
    public static int ClearMoodlesOnHeal()
    {
        return MoodleEventListener.ClearMoodlesOnHeal();
    }

    [ScriptMethod]
    public static int GetIntensity(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def)
            ? def.Intensity
            : 0;
    }

    [ScriptMethod]
    public static string GetName(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def)
            ? def.Name
            : string.Empty;
    }

    [ScriptMethod]
    public static string GetDescription(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def)
            ? def.Description
            : string.Empty;
    }

    [ScriptMethod]
    public static float GetHoldSeconds(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def)
            ? def.HoldSeconds
            : 0f;
    }

    [ScriptMethod]
    public static bool IsCritical(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def) && def.Critical;
    }

    [ScriptMethod]
    public static bool IsChippedOnly(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def) && def.ChippedOnly;
    }

    [ScriptMethod]
    public static bool IsImportant(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def) && def.Important;
    }

    private static bool TryGetDef(string moodleKey, out MoodleDef def)
    {
        if (!string.IsNullOrEmpty(moodleKey))
            return MoodleLoader.LoadedMoodleDefs.TryGetValue(moodleKey, out def);
        def = null!;
        return false;
    }

    private static void ApplyMoodleInternal(string key, MoodleDef def, float holdSeconds)
    {
        if (def.Animated && !string.IsNullOrWhiteSpace(def.AnimationId))
            MoodleRegistry.AddAnimatedMoodle(
                def.Intensity,
                def.AnimationId,
                def.Name,
                def.Description,
                def.Critical,
                def.ChippedOnly,
                def.Important,
                key,
                holdSeconds);
        else if (!string.IsNullOrWhiteSpace(def.IconId))
            MoodleRegistry.AddMoodle(
                def.Intensity,
                def.IconId,
                def.Name,
                def.Description,
                def.Critical,
                def.ChippedOnly,
                def.Important,
                key,
                holdSeconds);
        else if (MoodleLoader.LoadedMoodleSprites.TryGetValue(key, out var cachedSprite))
            MoodleRegistry.AddMoodle(
                def.Intensity,
                cachedSprite,
                def.Name,
                def.Description,
                def.Critical,
                def.ChippedOnly,
                def.Important,
                key,
                holdSeconds);
        else
            LogUtil.Warning("moodle.apply_no_icon_source", key);
    }
}
