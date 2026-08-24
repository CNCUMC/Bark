using Bark.ScriptApi;
using CUCoreLib.Helpers;
using UnityEngine;

namespace Bark.Tool;

// 持久化存储：封装 CUCoreUtils 的 PlayerPrefs 读写，供脚本保存自己的设置 / 数据。
[ScriptApi]
public static class StorageUtil
{
    // 读取布尔值，不存在时返回 defaultValue
    [ScriptMethod]
    public static bool GetBool(string key, bool defaultValue = false)
    {
        if (string.IsNullOrEmpty(key)) return defaultValue;
        return CUCoreUtils.GetBool(key, defaultValue);
    }

    // 写入布尔值
    [ScriptMethod]
    public static void SetBool(string key, bool value)
    {
        if (string.IsNullOrEmpty(key)) return;
        CUCoreUtils.SetBool(key, value);
    }

    // 读取浮点值，不存在时返回 defaultValue
    [ScriptMethod]
    public static float GetFloat(string key, float defaultValue = 0f)
    {
        if (string.IsNullOrEmpty(key)) return defaultValue;
        return CUCoreUtils.GetFloat(key, defaultValue);
    }

    // 写入浮点值
    [ScriptMethod]
    public static void SetFloat(string key, float value)
    {
        if (string.IsNullOrEmpty(key)) return;
        CUCoreUtils.SetFloat(key, value);
    }

    // 读取整数，不存在时返回 defaultValue
    [ScriptMethod]
    public static int GetInt(string key, int defaultValue = 0)
    {
        if (string.IsNullOrEmpty(key)) return defaultValue;
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    // 写入整数
    [ScriptMethod]
    public static void SetInt(string key, int value)
    {
        if (string.IsNullOrEmpty(key)) return;
        PlayerPrefs.SetInt(key, value);
    }

    // 读取字符串，不存在时返回 defaultValue
    [ScriptMethod]
    public static string GetString(string key, string defaultValue = "")
    {
        if (string.IsNullOrEmpty(key)) return defaultValue;
        return CUCoreUtils.GetString(key, defaultValue);
    }

    // 写入字符串
    [ScriptMethod]
    public static void SetString(string key, string value)
    {
        if (string.IsNullOrEmpty(key)) return;
        CUCoreUtils.SetString(key, value);
    }
}
