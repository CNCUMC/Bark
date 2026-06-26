using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using Bark.Tool;
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
    // language → category → key → value
    private static readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> Defaults = new();

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
    
    public static bool HasKeyLog(string key)
    {
        return HasKey("log", key);
    }
    
    public static bool HasKeyCommand(string key)
    {
        return HasKey("command", key);
    }
    
    public static bool HasKeyOption(string key)
    {
        return HasKey("option", key);
    }
    
    public static bool HasKeyLiquid(string key)
    {
        return HasKey("liquid", key);
    }
    
    public static bool HasKeyTitle(string key)
    {
        return HasKey("title", key);
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
    
    public static string GetLog(string key, params object[]? args)
    {
        return Get("log", key, args);
    }

    public static string GetCommand(string key, params object[]? args)
    {
        return Get("command", key, args);
    }

    public static string GetOption(string key, params object[]? args)
    {
        return Get("option", key, args);
    }
    
    public static string GetLiquid(string key, params object[]? args)
    {
        return Get("liquid", key, args);
    }

    public static string GetTitle(string key, params object[]? args)
    {
        return Get("title", key, args);
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

    public static void SetDefault(string language, string category, string key, string value)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (!Defaults.TryGetValue(language, out var langDict))
            Defaults[language] = langDict = new Dictionary<string, Dictionary<string, string>>();
        if (!langDict.TryGetValue(category, out var catDict))
            langDict[category] = catDict = new Dictionary<string, string>();
        catDict[key] = value;
    }

    private static string? GetDefault(string language, string key)
    {
        if (!Defaults.TryGetValue(language, out var langDict)) return null;
        foreach (var catDict in langDict.Values)
            if (catDict.TryGetValue(key, out var value)) return value;
        return null;
    }

    public static void Flush()
    {
        var outputDirectory = Path.Combine(Paths.ConfigPath, "CUCoreLib", "Locales");
        // CleanCclOther(outputDirectory);

        foreach (var langKvp in Defaults)
        {
            var language = langKvp.Key;
            foreach (var catKvp in langKvp.Value)
            {
                var category = catKvp.Key;
                foreach (var keyKvp in catKvp.Value)
                {
                    var key = keyKvp.Key;
                    var value = keyKvp.Value;

                    try
                    {
                        var filePath = Path.Combine(outputDirectory, $"{language}.json");
                        Directory.CreateDirectory(outputDirectory);

                        JObject root;
                        if (File.Exists(filePath))
                            try { root = JObject.Parse(File.ReadAllText(filePath)); }
                            catch { root = new JObject(); }
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
                        LogUtil.Warning($"[BetterLocale] Failed to write '{language}.json': {ex.Message}", Plugin.Logger);
                    }
                }
            }
        }
    }

    // // 清理 CCL 自动生成的 other.gameset* 条目（Bark 已管理在 option 分类中）
    // private static void CleanCclOther(string outputDirectory)
    // {
    //     try
    //     {
    //         foreach (var file in Directory.GetFiles(outputDirectory, "*.json"))
    //         {
    //             JObject root;
    //             try { root = JObject.Parse(File.ReadAllText(file)); }
    //             catch { continue; }
    //
    //             if (root["other"] is not JObject other) continue;
    //             var keysToRemove = new List<string>();
    //             foreach (var prop in other.Properties())
    //                 if (prop.Name.StartsWith("gameset"))
    //                     keysToRemove.Add(prop.Name);
    //
    //             if (keysToRemove.Count == 0) continue;
    //             foreach (var key in keysToRemove) other.Remove(key);
    //             File.WriteAllText(file, JsonConvert.SerializeObject(root, Formatting.Indented) + Environment.NewLine);
    //         }
    //     }
    //     catch { /* best effort */ }
    // }
}