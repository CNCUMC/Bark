using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using BepInEx;
using CUCoreLib.Registries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Bark.BetterCCL;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class BetterLocale
{
    private static readonly Dictionary<string, Dictionary<string, string>> Defaults = new();
    private static readonly HashSet<string> Pending = [];

    public static string Item(string key, params object[]? args)
    {
        return Get("item", key, args);
    }

    public static string Building(string key, params object[]? args)
    {
        return Get("build", key, args);
    }

    public static string Moodle(string key, params object[]? args)
    {
        return Get("moodle", key, args);
    }

    public static string Other(string key, params object[]? args)
    {
        return Get("other", key, args);
    }

    private static string Get(string category, string key, params object[]? args)
    {
        var resolvedKey = Replace(key, args);

        // 1. 尝试从 CCL 获取本地化
        var cclText = LocaleRegistry.Get(category, resolvedKey, resolvedKey);
        if (!string.IsNullOrWhiteSpace(cclText) && cclText != resolvedKey)
            return cclText;

        // 2. CCL 没有 → 从 Fallback 获取当前语言默认值
        var lang = PlayerPrefs.GetString("locale", "EN");
        var fallback = GetDefault(lang, resolvedKey);
        if (fallback != null) return fallback;

        // 3. 当前语言没有 → 回退到英语默认值
        if (lang == "EN") return resolvedKey;
        fallback = GetDefault("EN", resolvedKey);
        return fallback ??
               // 4. 完全找不到 → 返回 key 自身
               resolvedKey;
    }

    private static string Replace(string key, params object[]? args)
    {
        if (args == null || args.Length == 0) return key;
        return Regex.Replace(key, @"\{(\d+)\}", match =>
        {
            var index = int.Parse(match.Groups[1].Value);
            return args[index].ToString();
        });
    }

    // ModLangGenBase.Other() → SetDefault()
    public static void SetDefault(string language, string key, string value)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (!Defaults.TryGetValue(language, out var dict))
            Defaults[language] = dict = new Dictionary<string, string>();
        dict[key] = value;
    }

    // BetterLocale.Get() → 获取默认值并标记待写入
    private static string? GetDefault(string language, string key)
    {
        if (!Defaults.TryGetValue(language, out var dict) || !dict.TryGetValue(key, out var value)) return null;
        Pending.Add($"{language}\0{key}");
        return value;
    }

    // 写入所有 Pending 条目到 CCL 语言目录
    public static void Flush()
    {
        var outputDirectory = Path.Combine(Paths.ConfigPath, "CUCoreLib", "Locales");

        foreach (var entry in Pending)
        {
            var sepIndex = entry.IndexOf('\0');
            if (sepIndex < 0) continue;
            var language = entry.Substring(0, sepIndex);
            var key = entry.Substring(sepIndex + 1);

            if (!Defaults.TryGetValue(language, out var dict) ||
                !dict.TryGetValue(key, out var value))
                continue;

            var dotIndex = key.IndexOf('.');
            var category = dotIndex > 0 ? key.Substring(0, dotIndex) : "other";
            category = category switch { "item" or "building" or "moodle" => category, _ => "other" };

            try
            {
                var filePath = Path.Combine(outputDirectory, $"{language}.json");
                Directory.CreateDirectory(outputDirectory);

                JObject root;
                if (File.Exists(filePath))
                    try
                    {
                        root = JObject.Parse(File.ReadAllText(filePath));
                    }
                    catch
                    {
                        root = new JObject();
                    }
                else root = new JObject();

                if (root[category] is not JObject catObj)
                {
                    catObj = new JObject();
                    root[category] = catObj;
                }

                if (catObj[key] == null)
                {
                    catObj[key] = value;
                    File.WriteAllText(filePath,
                        JsonConvert.SerializeObject(root, Formatting.Indented) + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogWarning($"[BetterLocale] Failed to write '{language}.json': {ex.Message}");
            }
        }

        Pending.Clear();
    }
}