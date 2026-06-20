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
    private const string LocaleKeyPre = "tool_config_";

    public static ConfigEntry<T> Register<T>(ConfigFile configFile, string section, string key, T defaultValue,
        Func<string, string> getLocale, Dictionary<string, ConfigEntryBase> registry)
    {
        var entry = configFile.Bind(section, key, defaultValue,
            getLocale($"config_{key}_description"));
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

        Error("getconfig_notexistconfig", config);
        return null;
    }

    public static object GetConfigValue(string config, Dictionary<string, ConfigEntryBase> registry)
    {
        if (registry.TryGetValue(config, out var entry)) return entry.BoxedValue;

        Error("getconfig_notexistconfig", config);
        return null;
    }

    public static string GetConfigKey<T>(ConfigEntry<T> configEntry, Dictionary<string, ConfigEntryBase> registry)
    {
        var entry = registry.FirstOrDefault(x => x.Value == configEntry)
            .Key;

        if (!string.IsNullOrEmpty(entry))
            return entry;

        Error("getconfig_notexistkey", configEntry, entry);
        return null;
    }

    private static void Error(string key, params object[] args)
    {
        Log.Error(Locale(key, args), Plugin.Logger);
    }

    private static string Locale(string key, params object[] args)
    {
        var fullKey = $"{LocaleKeyPre}{key}";
        var text = LocaleRegistry.Get("other", fullKey, fullKey);
        return args.Length > 0 ? string.Format(text, args) : text;
    }
}
