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

    // 检查是否已有本地化文本（CCL 或 Bark Defaults 中有）
    public static bool HasKey(string category, string key)
    {
        var text = LocaleRegistry.Get(category, key, key);
        return !string.IsNullOrWhiteSpace(text) && text != key;
    }
    
    public static bool HasKeyItem(string key)
    {
        return HasKey("item", key);
    }

    public static bool HasKeyBuilding(string key)
    {
        return HasKey("build", key);
    }

    public static bool HasKeyMoodle(string key)
    {
        return HasKey("moodle", key);
    }

    public static bool HasKeyOther(string key)
    {
        return HasKey("other", key);
    }

    public static string GetItem(string key, params object[]? args)
    {
        return Get("item", key, args);
    }

    public static string GetBuilding(string key, params object[]? args)
    {
        return Get("build", key, args);
    }

    public static string GetMoodle(string key, params object[]? args)
    {
        return Get("moodle", key, args);
    }

    public static string GetOther(string key, params object[]? args)
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

    // BetterLocale.Get() → 获取默认值
    private static string? GetDefault(string language, string key)
    {
        return Defaults.TryGetValue(language, out var dict) && dict.TryGetValue(key, out var value) ? value : null;
    }

    // 写入默认值到 CCL 语言目录
    public static void Flush()
    {
        var outputDirectory = Path.Combine(Paths.ConfigPath, "CUCoreLib", "Locales");

        foreach (var langKvp in Defaults)
        {
            var language = langKvp.Key;
            foreach (var keyKvp in langKvp.Value)
            {
                var key = keyKvp.Key;
                var value = keyKvp.Value;

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
                UnityEngine.Debug.LogWarning($"[BetterLocale] Failed to write '{language}.json': {ex.Message}");
            }
            }
        }
    }
}