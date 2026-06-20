using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bark.Core;

public abstract class ModLangGenBase
{
    private const string LangDirectory = "Lang";
    private bool _isInitialized;
    private ManualLogSource _log;
    private Assembly _ownerAssembly = Assembly.GetCallingAssembly();
    protected abstract string LanguageCode { get; }
    private Dictionary<string, string> LocaleData { get; } = new();

    public int Count
    {
        get
        {
            EnsureInitialized();
            return LocaleData?.Count ?? 0;
        }
    }

    internal void Initialize(ManualLogSource logger, Assembly pluginAssembly)
    {
        if (_isInitialized)
            return;

        _log = logger;
        _ownerAssembly = pluginAssembly;

        BuildLocaleData();
        _isInitialized = true;
    }

    protected abstract void BuildLocaleData();

    protected void Add(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            _log?.LogWarning($"[{LanguageCode}] Warning: Skipping empty key");
            return;
        }

        if (LocaleData.ContainsKey(key))
            _log?.LogWarning($"[{LanguageCode}] Warning: Key '{key}' already exists and will be overwritten");

        LocaleData[key] = value;
    }

    public void Generate(string outputDirectory = null)
    {
        EnsureInitialized();

        if (LocaleData == null || LocaleData.Count == 0)
        {
            _log?.LogWarning($"[{LanguageCode}] Warning: No localization data to generate");
            return;
        }

        if (string.IsNullOrEmpty(outputDirectory))
        {
            var pluginDirectory = Path.GetDirectoryName(_ownerAssembly.Location);
            outputDirectory = Path.Combine(pluginDirectory, LangDirectory);
        }

        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

        try
        {
            var filePath = Path.Combine(outputDirectory, $"{LanguageCode}.json");

            JObject existingJson = null;
            if (File.Exists(filePath))
                try
                {
                    var existingContent = File.ReadAllText(filePath);
                    existingJson = JObject.Parse(existingContent);
                }
                catch (Exception ex)
                {
                    _log?.LogWarning($"[{LanguageCode}] Failed to parse existing file, will regenerate: {ex.Message}");
                }

            JObject resultJson;
            var newEntries = 0;

            if (existingJson != null)
            {
                var existingKeys = FlattenJson(existingJson);

                foreach (var kvp in LocaleData)
                    if (!existingKeys.ContainsKey(kvp.Key))
                    {
                        SetNestedValue(existingJson, kvp.Key, kvp.Value);
                        newEntries++;
                    }

                resultJson = existingJson;
            }
            else
            {
                resultJson = BuildNestedJson();
                newEntries = LocaleData.Count;
            }

            var jsonContent = JsonConvert.SerializeObject(resultJson, Formatting.Indented);
            File.WriteAllText(filePath, jsonContent + Environment.NewLine);

            if (newEntries > 0)
                _log?.LogInfo($"[{LanguageCode}] Added {newEntries} new entries to: {filePath}");
            else
                _log?.LogInfo($"[{LanguageCode}] All entries already exist, no changes: {filePath}");
        }
        catch (Exception ex)
        {
            _log?.LogError($"[{LanguageCode}] Generation failed: {ex.Message}");
        }
    }

    private JObject BuildNestedJson()
    {
        var root = new JObject();

        foreach (var kvp in LocaleData) SetNestedValue(root, kvp.Key, kvp.Value);

        return root;
    }

    private static void SetNestedValue(JObject root, string path, string value)
    {
        var keys = path.Split('.');
        var current = root;

        for (var i = 0; i < keys.Length - 1; i++)
        {
            var key = keys[i];

            if (!current.ContainsKey(key) || current[key].Type != JTokenType.Object) current[key] = new JObject();

            current = (JObject)current[key];
        }

        current[keys[keys.Length - 1]] = value;
    }

    private static Dictionary<string, string> FlattenJson(JObject obj)
    {
        var result = new Dictionary<string, string>();
        FlattenJsonRecursive(obj, "", result);
        return result;
    }

    private static void FlattenJsonRecursive(JToken token, string prefix, Dictionary<string, string> result)
    {
        switch (token)
        {
            case JObject obj:
            {
                foreach (var prop in obj.Properties())
                {
                    var newPrefix = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                    FlattenJsonRecursive(prop.Value, newPrefix, result);
                }

                break;
            }
            case JArray array:
            {
                for (var i = 0; i < array.Count; i++) FlattenJsonRecursive(array[i], $"{prefix}[{i}]", result);
                break;
            }
            case JValue value:
                result[prefix] = value.ToString();
                break;
        }
    }

    private void EnsureInitialized()
    {
        if (!_isInitialized)
            throw new InvalidOperationException($"{GetType().Name} has not been initialized. Call Initialize() first.");
    }
}