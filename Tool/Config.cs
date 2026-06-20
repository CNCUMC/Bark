using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BepInEx.Configuration;
using CUCoreLib.Registries;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class Config
{
    public static ConfigEntry<T> Register<T>(ConfigFile configFile, string section, string key, T defaultValue,
        Func<string, string> getLocale, Dictionary<string, ConfigEntryBase> registry)
    {
        var entry = configFile.Bind(section, key, defaultValue,
            getLocale($"tool.config.{key.ToLower()}.description"));
        registry[key] = entry;
        return entry;
    }

    public static bool HasConfig(string config, Dictionary<string, ConfigEntryBase> registry)
    {
        return registry.ContainsKey(config);
    }

    public static ConfigEntryBase GetConfig(string config, Dictionary<string, ConfigEntryBase> registry)
    {
        var hasConfig = registry.TryGetValue(config, out var entry);
        if (hasConfig) return entry;

        Error("tool.config.get_config.not_exist_config", config);
        return null;
    }

    public static object GetConfigValue(string config, Dictionary<string, ConfigEntryBase> registry)
    {
        if (registry.TryGetValue(config, out var entry)) return entry.BoxedValue;

        Error("tool.config.get_config.not_exist_config", config);
        return null;
    }

    public static string GetConfigKey<T>(ConfigEntry<T> configEntry, Dictionary<string, ConfigEntryBase> registry)
    {
        var entry = registry.FirstOrDefault(x => x.Value == configEntry)
            .Key;

        if (!string.IsNullOrEmpty(entry))
            return entry;

        Error("tool.config.get_config.not_exist_key", configEntry, entry);
        return null;
    }

    private static void Error(string key, params object[] args)
    {
        var text = LocaleRegistry.Get("other", key, key);
        var message = args.Length > 0 ? string.Format(text, args) : text;
        Log.Error(message, Plugin.Logger);
    }
}
