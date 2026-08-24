using System;
using Bark.Event.Listener;
using Bark.Moodle;
using Bark.ScriptApi;
using CUCoreLib.Registries;

namespace Bark.Tool;

// Moodle 状态管理工具：给玩家添加/去掉 Moodle、查询 Moodle 属性。
// 所有 Moodle 定义来自 ScriptMod/Moods 下的 JSON 文件（由 MoodleLoader 加载）。
// 方法加 [ScriptMethod] 后自动暴露给 Lua/JS 脚本。
[ScriptApi]
public static class MoodleUtil
{
    // 查询：当前活跃的 Moodle
    // 检查指定 key 的 moodle 是否当前活跃在玩家身上
    [ScriptMethod]
    public static bool HasMoodle(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;
        return MoodleEventListener.HasMoodle(key);
    }

    // 获取当前所有活跃 moodle 的 key 列表
    [ScriptMethod]
    public static string[] GetActiveMoodles()
    {
        return MoodleEventListener.GetActiveMoodleKeys();
    }

    // 获取当前活跃 moodle 数量
    [ScriptMethod]
    public static int GetMoodleCount()
    {
        return MoodleEventListener.GetMoodleCount();
    }

    // 操作：应用 / 移除 Moodle
    // 给玩家应用一个已定义的 Moodle（按 moodleKey 查找 JSON 定义）
    // holdSeconds：可选自定义持续时间，填 0 或负数则使用 JSON 中定义的默认值
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

        var duration = holdSeconds > 0f ? holdSeconds : def.HoldSeconds;
        ApplyMoodleInternal(moodleKey, def, duration);
    }

    // 移除玩家身上指定 key 的 Moodle
    // 返回 true 表示该 moodle 存在并被标记移除（下次轮询时触发 lose 事件）
    [ScriptMethod]
    public static bool RemoveMoodle(string moodleKey)
    {
        if (string.IsNullOrEmpty(moodleKey))
            return false;
        return MoodleEventListener.ForceExpire(moodleKey);
    }

    // heal 时清除所有 can_heal=false 的自定义 Moodle。返回清除数量。
    // can_heal 默认 false，即新 Moodle 默认会被 heal 移除；设为 true 则在 heal 后保留。
    [ScriptMethod]
    public static int ClearMoodlesOnHeal()
    {
        return MoodleEventListener.ClearMoodlesOnHeal();
    }

    // 属性：获取已加载 Moodle 定义的属性
    // 获取 Moodle 的强度
    [ScriptMethod]
    public static int GetIntensity(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def) ? def.Intensity : 0;
    }

    // 获取 Moodle 的名称
    [ScriptMethod]
    public static string GetName(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def) ? def.Name : string.Empty;
    }

    // 获取 Moodle 的描述
    [ScriptMethod]
    public static string GetDescription(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def) ? def.Description : string.Empty;
    }

    // 获取 Moodle 的持续时间（秒）
    [ScriptMethod]
    public static float GetHoldSeconds(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def) ? def.HoldSeconds : 0f;
    }

    // 是否为严重状态
    [ScriptMethod]
    public static bool IsCritical(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def) && def.Critical;
    }

    // 是否仅消耗品显示
    [ScriptMethod]
    public static bool IsChippedOnly(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def) && def.ChippedOnly;
    }

    // 是否重要（显示在主区域 vs 侧边栏）
    [ScriptMethod]
    public static bool IsImportant(string moodleKey)
    {
        return TryGetDef(moodleKey, out var def) && def.Important;
    }

    // 内部辅助
    // 尝试获取已加载的 MoodleDef
    private static bool TryGetDef(string moodleKey, out MoodleDef def)
    {
        if (!string.IsNullOrEmpty(moodleKey)) return MoodleLoader.LoadedMoodleDefs.TryGetValue(moodleKey, out def);
        def = null!;
        return false;
    }

    // 根据 MoodleDef 调用 MoodleRegistry 注册
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
            // 使用加载阶段缓存的自定义精灵图
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