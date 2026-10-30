using System;
using System.Collections.Generic;
using System.Linq;
using Bark.Events;
using Bark.Tool;

namespace Bark.Moodle;

public static class MoodleScriptRunner
{
    private const string Guid = Plugin.Guid + ".moodles";

    public static void Listen()
    {
        EventUtil.On<MoodleGetEvent>(OnMoodleGet, Guid);
        EventUtil.On<MoodleIterateEvent>(OnMoodleIterate, Guid);
        EventUtil.On<MoodleLoseEvent>(OnMoodleLose, Guid);
    }

    public static void Stop()
    {
        EventUtil.UnregisterAll(Guid);
    }

    private static void OnMoodleGet(MoodleGetEvent evt)
    {
        ExecuteScripts(evt.MoodleKey, "get", e => e.Get);
    }

    private static void OnMoodleIterate(MoodleIterateEvent evt)
    {
        foreach (var key in evt.ActiveKeys)
            ExecuteScripts(key, "iterate", e => e.Iterate);
    }

    private static void OnMoodleLose(MoodleLoseEvent evt)
    {
        ExecuteScripts(evt.MoodleKey, "lose", e => e.Lose);
    }

    private static void ExecuteScripts(string moodleKey, string action,
        Func<MoodleScriptEntry, List<string>> getScriptList)
    {
        if (string.IsNullOrEmpty(moodleKey))
            return;

        var entry = MoodleScriptRegistry.GetEntry(moodleKey);
        if (entry is null)
            return;

        var scripts = getScriptList(entry);
        if (scripts.Count == 0)
            return;

        foreach (var relativePath in scripts.Where(relativePath =>
            !string.IsNullOrEmpty(relativePath)))
            ScriptUtil.Execute(entry.ModId, relativePath, moodleKey, null, action);
    }
}
