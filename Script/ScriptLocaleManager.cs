using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bark.BetterCCL;
using Bark.Tool;
using Newtonsoft.Json;
using UnityEngine;

namespace Bark.Script;

public static class ScriptLocaleManager
{
    private static readonly Dictionary<string, string> LocaleData = new();

    private static string _currentLang = "EN";
    // (modDir, modId) 配对，用于 Reload
    private static readonly List<(string ModDir, string ModId)> LoadedMods = [];

    public static void Initialize()
    {
        try
        {
            _currentLang = PlayerPrefs.GetString("locale", "EN");
        }
        catch
        {
            _currentLang = "EN";
        }

        LoadedMods.Clear();
        LocaleData.Clear();
    }

    // 加载单个模组的语言文件（使用 manifest 中声明的真实 modId）
    public static void LoadModLocale(string modDir, string modId)
    {
        if (modDir is null) throw new ArgumentNullException(nameof(modDir));
        if (modId is null) throw new ArgumentNullException(nameof(modId));

        LoadedMods.Add((modDir, modId));

        LoadLangFile(modDir, modId, "EN");

        if (_currentLang == "EN") return;
        LoadLangFile(modDir, modId, _currentLang);
    }

    private static void LoadLangFile(string modDir, string modId, string langCode)
    {
        var langDir = Path.Combine(modDir, "Lang");
        var langFile = Path.Combine(langDir, $"{langCode}.json");

        if (!File.Exists(langFile)) return;

        try
        {
            var json = File.ReadAllText(langFile);
            var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
            if (data == null) return;

            foreach (var (category, dictionary) in data)
            {
                if (dictionary == null) continue;

                foreach (var (key, value) in dictionary)
                {
                    // 存入本地 LocaleData，格式为 "{category}.{key}" 便于 Get() 查找
                    var flatKey = $"{category}.{key}";
                    LocaleData[flatKey] = value;

                    // 同步推入 CCL 本地化
                    // key 自带命名空间，如 "hello_world_js.loaded" → ns="hello_world_js", rest="loaded"
                    // 也支持跨模组本地化，如 "quantum.auto_rack" → ns="quantum", rest="auto_rack"
                    // 无点号则回退用当前 modId 做命名空间
                    var dotIndex = key.IndexOf('.');
                    var localeNs = dotIndex > 0 ? key.Substring(0, dotIndex) : modId;
                    var localeKey = dotIndex > 0 ? key.Substring(dotIndex + 1) : key;
                    BetterLocale.SetDefault(langCode, localeNs, category, localeKey, value);
                }
            }
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_locale.load_failed", langFile, ex.Message);
        }
    }

    public static string Get(string key)
    {
        if (LocaleData.TryGetValue(key, out var value))
            return value;

        return $"[{key}]";
    }

    public static string GetFormatted(string key, params object[] args)
    {
        var template = Get(key);
        if (template.StartsWith("[") && template.EndsWith("]"))
            return template;

        try
        {
            return string.Format(template, args);
        }
        catch (FormatException)
        {
            return template;
        }
    }

    public static bool HasKey(string key)
    {
        return LocaleData.ContainsKey(key);
    }

    public static Dictionary<string, string> GetByPrefix(string prefix)
    {
        return LocaleData
            .Where(kvp => kvp.Key.StartsWith(prefix))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static void Reload()
    {
        LocaleData.Clear();
        foreach (var (modDir, modId) in LoadedMods)
            LoadLangFile(modDir, modId, "EN");
        if (_currentLang == "EN") return;
        {
            foreach (var (modDir, modId) in LoadedMods)
                LoadLangFile(modDir, modId, _currentLang);
        }
    }
}