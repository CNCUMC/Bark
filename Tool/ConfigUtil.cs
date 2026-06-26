using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bark.BetterCCL;
using BepInEx.Configuration;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class ConfigUtil
{
    public static bool HasConfig(string config, Dictionary<string, ConfigEntryBase> registry)
    {
        return registry.ContainsKey(config);
    }

    public static ConfigEntryBase? GetConfig(string config, Dictionary<string, ConfigEntryBase> registry)
    {
        if (registry.TryGetValue(config, out var entry)) return entry;
        Error("config.get_config.not_exist_config");
        return null;
    }

    public static object? GetConfigValue(string config, Dictionary<string, ConfigEntryBase> registry)
    {
        if (registry.TryGetValue(config, out var entry)) return entry.BoxedValue;
        Error("config.get_config.not_exist_config");
        return null;
    }

    public static string? GetConfigKey<T>(ConfigEntry<T> configEntry, Dictionary<string, ConfigEntryBase> registry)
    {
        var entry = registry.FirstOrDefault(x => x.Value == configEntry).Key;
        if (!string.IsNullOrEmpty(entry)) return entry;
        Error("config.get_config.not_exist_key");
        return null;
    }

    private static void Error(string key)
    {
        LogUtil.Error(Locale(key), Plugin.Logger);
    }

    private static string Locale(string key)
    {
        return BetterLocale.GetOther("log." + key);
    }
}