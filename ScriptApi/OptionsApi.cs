using System;
using Bark.Script;

namespace Bark.ScriptApi;

// 脚本配置读取 API：暴露给 JS/Lua 脚本侧的 [ScriptMethod] getter，
// 从 OptionsUtil 的内部缓存中读取当前选项值
public static class OptionsApi
{
    [ScriptMethod]
    public static bool GetBool(string modId, string key)
    {
        if (modId is null) throw new ArgumentNullException(nameof(modId));
        if (key is null) throw new ArgumentNullException(nameof(key));
        return OptionsUtil.TryGetBool(modId, key);
    }

    [ScriptMethod]
    public static int GetInt(string modId, string key)
    {
        if (modId is null) throw new ArgumentNullException(nameof(modId));
        if (key is null) throw new ArgumentNullException(nameof(key));
        return OptionsUtil.TryGetInt(modId, key);
    }

    [ScriptMethod]
    public static float GetFloat(string modId, string key)
    {
        if (modId is null) throw new ArgumentNullException(nameof(modId));
        if (key is null) throw new ArgumentNullException(nameof(key));
        return OptionsUtil.TryGetFloat(modId, key);
    }

    [ScriptMethod]
    public static int GetDropdown(string modId, string key)
    {
        if (modId is null) throw new ArgumentNullException(nameof(modId));
        if (key is null) throw new ArgumentNullException(nameof(key));
        return OptionsUtil.TryGetInt(modId, key);
    }

    [ScriptMethod]
    public static string GetKeybind(string modId, string key)
    {
        if (modId is null) throw new ArgumentNullException(nameof(modId));
        if (key is null) throw new ArgumentNullException(nameof(key));
        return OptionsUtil.TryGetString(modId, key);
    }
}
