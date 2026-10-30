using System;
using Bark.Script;

namespace Bark.ScriptApi;

[ScriptApi]
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
