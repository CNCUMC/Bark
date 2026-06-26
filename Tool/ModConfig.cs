using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bark.BetterCCL;
using BepInEx.Configuration;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class ModConfig
{
    public static bool HasConfig(string config, Dictionary<string, ConfigEntryBase> registry)
    {
        return registry.ContainsKey(config);
    }

    public static ConfigEntryBase? GetConfig(string config, Dictionary<string, ConfigEntryBase> registry)
    {
        var hasConfig = registry.TryGetValue(config, out var entry);
        if (hasConfig) return entry;

        Error("get_config.not_exist_config", config);
        return null;
    }

    public static object? GetConfigValue(string config, Dictionary<string, ConfigEntryBase> registry)
    {
        if (registry.TryGetValue(config, out var entry)) return entry.BoxedValue;

        Error("get_config.not_exist_config", config);
        return null;
    }

    public static string? GetConfigKey<T>(ConfigEntry<T> configEntry, Dictionary<string, ConfigEntryBase> registry)
    {
        var entry = registry.FirstOrDefault(x => x.Value == configEntry)
            .Key;

        if (!string.IsNullOrEmpty(entry))
            return entry;

        Error("get_config.not_exist_key", configEntry, entry);
        return null;
    }

    private static void Error(string key, params object[] args)
    {
        Log.Error(Locale(key, args), Plugin.Logger);
    }

    private static string Locale(string key, params object[] args)
    {
        return BetterLocale.Other("log." + key, args);
    }
}