using System.Collections.Generic;
using System.Linq;
using Bark.Events;
using Bark.Tool;

namespace Bark.Moodle;

// Moodle 脚本运行器：监听 Moodle 事件，通过 ScriptUtil 触发对应的脚本文件。
// 在 Plugin.Awake() 中调用 Listen() 注册事件处理器。
// 注意：这里处理的是 JSON 中 script 字段定义的脚本（自动执行），
// 与全局 onMoodleGet/onMoodleIterate/onMoodleLose 钩子（由 ScriptEventScanner 桥接）互补。
public static class MoodleScriptRunner
{
    // 注册事件处理器（应在所有模组加载完成后调用）
    public static void Listen()
    {
        EventUtil.On<MoodleGetEvent>(OnMoodleGet, Plugin.Guid);
        EventUtil.On<MoodleIterateEvent>(OnMoodleIterate, Plugin.Guid);
        EventUtil.On<MoodleLoseEvent>(OnMoodleLose, Plugin.Guid);
    }

    // 停止监听（卸载时调用）
    public static void Stop()
    {
        EventUtil.UnregisterAll(Plugin.Guid);
    }

    private static void OnMoodleGet(MoodleGetEvent evt)
    {
        ExecuteScripts(evt.MoodleKey, "get", e => e.Get);
    }

    private static void OnMoodleIterate(MoodleIterateEvent evt)
    {
        // iterate 遍历每个活跃 moodle key，分别触发其脚本
        foreach (var key in evt.ActiveKeys)
            ExecuteScripts(key, "iterate", e => e.Iterate);
    }

    private static void OnMoodleLose(MoodleLoseEvent evt)
    {
        ExecuteScripts(evt.MoodleKey, "lose", e => e.Lose);
    }

    // 从 MoodleScriptRegistry 查找 Moodle 脚本，通过 ScriptUtil 按顺序执行。
    // moodleKey: Moodle 唯一标识；action: get / iterate / lose
    private static void ExecuteScripts(string moodleKey, string action,
        System.Func<MoodleScriptEntry, List<string>> getScriptList)
    {
        if (string.IsNullOrEmpty(moodleKey))
            return;

        var entry = MoodleScriptRegistry.GetEntry(moodleKey);
        if (entry is null)
            return;

        var scripts = getScriptList(entry);
        if (scripts.Count == 0)
            return;

        foreach (var relativePath in scripts.Where(relativePath => !string.IsNullOrEmpty(relativePath)))
        {
            // 复用 ScriptUtil.Execute，moodleKey 作为 itemId 传入供脚本侧访问
            ScriptUtil.Execute(entry.ModId, relativePath, moodleKey, null, action);
        }
    }
}
