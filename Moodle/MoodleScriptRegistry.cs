using System;
using System.Collections.Generic;
using Bark.Script;

namespace Bark.Moodle;

public static class MoodleScriptRegistry
{
    private static readonly Dictionary<string, MoodleScriptEntry> Entries = new();

    public static void Register(string moodleKey, MoodleScriptDef scriptDef,
        ScriptEngine engine,
        string modId, string modDir)
    {
        if (string.IsNullOrEmpty(moodleKey))
            throw new ArgumentNullException(nameof(moodleKey));
        if (scriptDef is null)
            throw new ArgumentNullException(nameof(scriptDef));
        if (engine is null)
            throw new ArgumentNullException(nameof(engine));
        if (string.IsNullOrEmpty(modId))
            throw new ArgumentNullException(nameof(modId));
        if (string.IsNullOrEmpty(modDir))
            throw new ArgumentNullException(nameof(modDir));

        if (IsEmpty(scriptDef))
            return;

        Entries[moodleKey] = new MoodleScriptEntry(engine, scriptDef, modId, modDir);
    }

    public static void Unregister(string moodleKey)
    {
        Entries.Remove(moodleKey);
    }

    public static MoodleScriptEntry? GetEntry(string moodleKey)
    {
        return Entries.GetValueOrDefault(moodleKey);
    }

    private static bool IsEmpty(MoodleScriptDef def)
    {
        return def.Get.Count == 0
            && def.Iterate.Count == 0
            && def.Lose.Count == 0;
    }
}

public class MoodleScriptEntry(
    ScriptEngine engine,
    MoodleScriptDef scriptDef,
    string modId,
    string modDir)
{
    public readonly List<string> Get = scriptDef.Get;
    public readonly List<string> Iterate = scriptDef.Iterate;
    public readonly List<string> Lose = scriptDef.Lose;
    public readonly string ModDir = modDir;
    public readonly string ModId = modId;
    public ScriptEngine Engine = engine;
}
