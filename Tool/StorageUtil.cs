using Bark.ScriptApi;
using CUCoreLib.Helpers;
using UnityEngine;

namespace Bark.Tool;

[ScriptApi]
public static class StorageUtil
{
    [ScriptMethod]
    public static bool GetBool(string key, bool defaultValue = false)
    {
        if (string.IsNullOrEmpty(key)) return defaultValue;
        return CUCoreUtils.GetBool(key, defaultValue);
    }

    [ScriptMethod]
    public static void SetBool(string key, bool value)
    {
        if (string.IsNullOrEmpty(key)) return;
        CUCoreUtils.SetBool(key, value);
    }

    [ScriptMethod]
    public static float GetFloat(string key, float defaultValue = 0f)
    {
        if (string.IsNullOrEmpty(key)) return defaultValue;
        return CUCoreUtils.GetFloat(key, defaultValue);
    }

    [ScriptMethod]
    public static void SetFloat(string key, float value)
    {
        if (string.IsNullOrEmpty(key)) return;
        CUCoreUtils.SetFloat(key, value);
    }

    [ScriptMethod]
    public static int GetInt(string key, int defaultValue = 0)
    {
        if (string.IsNullOrEmpty(key)) return defaultValue;
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    [ScriptMethod]
    public static void SetInt(string key, int value)
    {
        if (string.IsNullOrEmpty(key)) return;
        PlayerPrefs.SetInt(key, value);
    }

    [ScriptMethod]
    public static string GetString(string key, string defaultValue = "")
    {
        if (string.IsNullOrEmpty(key)) return defaultValue;
        return CUCoreUtils.GetString(key, defaultValue);
    }

    [ScriptMethod]
    public static void SetString(string key, string value)
    {
        if (string.IsNullOrEmpty(key)) return;
        CUCoreUtils.SetString(key, value);
    }
}
