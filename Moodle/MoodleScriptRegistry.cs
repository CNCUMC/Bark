using System;
using System.Collections.Generic;
using Bark.Script;

namespace Bark.Moodle;

// Moodle 脚本映射存储：moodleKey → (ScriptEngine, 脚本文件路径列表按动作分组)
// 供 MoodleScriptRunner 在 Moodle 事件触发时查找并执行脚本。
public static class MoodleScriptRegistry
{
    // moodleKey → 脚本映射记录
    private static readonly Dictionary<string, MoodleScriptEntry> Entries = new();

    // 注册一个 Moodle 的脚本映射。moodleKey 为 Moodle 唯一 key，
    // scriptDef 为 JSON 反序列化的脚本定义，engine 为模组的 ScriptEngine，
    // modId 为模组 ID，modDir 为模组目录。
    public static void Register(string moodleKey, MoodleScriptDef scriptDef, ScriptEngine engine,
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

    // 注销指定 Moodle 的脚本映射（热重载时调用）
    public static void Unregister(string moodleKey)
    {
        Entries.Remove(moodleKey);
    }

    // 获取指定 Moodle 的脚本映射，未注册时返回 null
    public static MoodleScriptEntry? GetEntry(string moodleKey)
    {
        return Entries.GetValueOrDefault(moodleKey);
    }

    // 判断脚本定义是否所有动作都为空
    private static bool IsEmpty(MoodleScriptDef def)
    {
        return def.Get.Count == 0
               && def.Iterate.Count == 0
               && def.Lose.Count == 0;
    }
}

// 单个 Moodle 的脚本映射记录：包含引擎引用、模组 ID、脚本文件路径列表（按动作分组）、模组目录
public class MoodleScriptEntry(ScriptEngine engine, MoodleScriptDef scriptDef, string modId, string modDir)
{
    public readonly List<string> Get = scriptDef.Get;
    public readonly List<string> Iterate = scriptDef.Iterate;
    public readonly List<string> Lose = scriptDef.Lose;
    public readonly string ModDir = modDir;
    public readonly string ModId = modId;
    public ScriptEngine Engine = engine;
}