using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bark.Tool;
using Newtonsoft.Json;
using UnityEngine;

namespace Bark.Script;

public static class ScriptLocaleManager
{
    private static readonly Dictionary<string, string> LocaleData = new();

    private static string _currentLang = "EN";
    private static readonly List<string> LoadedModDirs = [];

    public static void Initialize(string modsPath)
    {
        var modsDir = Path.Combine(modsPath, "Mods");
        if (!Directory.Exists(modsDir)) return;

        try
        {
            _currentLang = PlayerPrefs.GetString("locale", "EN");
        }
        catch
        {
            _currentLang = "EN";
        }

        LoadedModDirs.Clear();
        LocaleData.Clear();

        var modDirectories = Directory.GetDirectories(modsDir);
        LoadedModDirs.AddRange(modDirectories);

        foreach (var modDir in modDirectories)
            LoadLangFile(modDir, "EN");

        if (_currentLang == "EN") return;
        {
            foreach (var modDir in modDirectories)
                LoadLangFile(modDir, _currentLang);
        }
    }

    private static void LoadLangFile(string modDir, string langCode)
    {
        var langDir = Path.Combine(modDir, "Lang");
        var langFile = Path.Combine(langDir, $"{langCode}.json");

        if (!File.Exists(langFile)) return;

        try
        {
            var json = File.ReadAllText(langFile);
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (data == null) return;

            foreach (var kvp in data)
                LocaleData[kvp.Key] = kvp.Value;
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
        foreach (var modDir in LoadedModDirs)
            LoadLangFile(modDir, "EN");
        if (_currentLang == "EN") return;
        {
            foreach (var modDir in LoadedModDirs)
                LoadLangFile(modDir, _currentLang);
        }
    }
}