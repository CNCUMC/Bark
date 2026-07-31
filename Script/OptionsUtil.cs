using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Bark.Tool;
using CUCoreLib.Data;
using CUCoreLib.Registries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Bark.Script;

// 脚本配置 ↔ CCL 游戏设置桥接器
// 选项定义从 mod.json 同层的 options.json 读取（模组自带，只读），
// 用户保存值写入 ScriptMod/Configs/{modId}.json（简单 key-value 格式）。
// 加载时合并：用户保存值覆盖定义中的默认值。
public static class OptionsUtil
{
    // "{modId}.{key}" → value
    private static readonly ConcurrentDictionary<string, object> s_cache = new();

    // 已成功注册的选项 ID 集合（用于热重载时检测重复）
    private static readonly HashSet<string> s_registeredIds = new(StringComparer.Ordinal);

    // 任意选项注册失败时为 true，供外部检查是否需要冷重启
    public static bool RestartNeeded { get; private set; }

    // ---- 通用冷重启提示（非专属于脚本设置，其他场景也可调用） ----

    public static void ShowRestartRequired(string reasonKey)
    {
        RestartNeeded = true;
        LogUtil.Warning($"options_util.{reasonKey}");
    }

    // ---- 配置 → 设置桥接入口，由 ScriptModLoader.LoadAll() 在引擎创建前调用 ----

    public static void RegisterFromMod(ScriptManifest manifest, string modDir, string configsDir)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));
        if (modDir is null)
            throw new ArgumentNullException(nameof(modDir));
        if (configsDir is null)
            throw new ArgumentNullException(nameof(configsDir));

        // 1. 读取选项定义：{modDir}/options.json
        var defPath = Path.Combine(modDir, "options.json");
        if (!File.Exists(defPath))
            return;

        JObject defJson;
        try
        {
            defJson = JObject.Parse(File.ReadAllText(defPath));
        }
        catch (Exception ex)
        {
            Warn("options_util.def_parse_failed",
                manifest.Id, ex.Message);
            return;
        }

        if (defJson["_options"] is not JObject optionsDef)
            return;

        // 2. 读取用户保存值：ScriptMod/Configs/{modId}.json
        var savePath = Path.Combine(configsDir, $"{manifest.Id}.json");
        var savedValues = LoadSavedValues(savePath, manifest.Id);

        // 3. 遍历选项定义，合并保存值后注册
        var modId = manifest.Id;
        var registeredCount = 0;
        var duplicateCount = 0;

        foreach (var prop in optionsDef.Properties())
        {
            var key = prop.Name;
            if (prop.Value is not JObject optionMeta)
                continue;

            var typeToken = optionMeta["type"];
            if (typeToken is null)
            {
                Warn("options_util.missing_type",
                    modId, key);
                continue;
            }

            var type = (typeToken.Value<string>() ?? string.Empty).ToLowerInvariant();

            // 合并：用户保存值覆盖定义默认值
            var mergedMeta = MergeDefault(optionMeta, key, savedValues);

            if (RegisterSingle(modId, key, type, savePath, mergedMeta, manifest.Name))
                registeredCount++;
            else
                duplicateCount++;
        }

        if (registeredCount > 0)
            Info("options_util.registered_options",
                modId, registeredCount);

        if (duplicateCount > 0)
            ShowRestartRequired("config_changed_restart_required");
    }

    // ---- 内部访问器（供 ScriptApi/OptionsApi 调用） ----

    internal static bool TryGetBool(string modId, string key)
    {
        return GetCachedValue<long>(modId, key) != 0;
    }

    internal static int TryGetInt(string modId, string key)
    {
        return (int)GetCachedValue<long>(modId, key);
    }

    internal static float TryGetFloat(string modId, string key)
    {
        return (float)GetCachedValue<double>(modId, key);
    }

    internal static string TryGetString(string modId, string key)
    {
        return GetCachedString(modId, key) ?? string.Empty;
    }

    // ---- 内部实现 ----

    // 读取用户保存值：{ "key": value } 格式
    private static Dictionary<string, JToken>? LoadSavedValues(string savePath, string modId)
    {
        if (!File.Exists(savePath))
            return null;

        try
        {
            return JsonConvert.DeserializeObject<Dictionary<string, JToken>>(File.ReadAllText(savePath));
        }
        catch (Exception ex)
        {
            Warn("options_util.config_parse_failed",
                modId, ex.Message);
            return null;
        }
    }

    // 合并保存值到定义元数据：若用户有保存值则用它覆盖 meta["default"]
    private static JObject MergeDefault(JObject meta, string key, Dictionary<string, JToken>? savedValues)
    {
        if (savedValues == null || !savedValues.TryGetValue(key, out var savedToken))
            return meta;

        var merged = (JObject)meta.DeepClone();
        merged["default"] = savedToken;
        return merged;
    }

    private static T GetCachedValue<T>(string modId, string key) where T : struct
    {
        var cacheKey = $"{modId}.{key}";
        if (s_cache.TryGetValue(cacheKey, out var value) && value is T typed)
            return typed;
        return default;
    }

    private static string? GetCachedString(string modId, string key)
    {
        var cacheKey = $"{modId}.{key}";
        if (s_cache.TryGetValue(cacheKey, out var value) && value is string typed)
            return typed;
        return null;
    }

    // 注册单个选项，返回 true 表示成功，false 表示重复（CCL 拒绝）
    // modName 仅用于自定义选项卡的显示名称，不影响内部 ID 和本地化 key
    private static bool RegisterSingle(string modId, string key, string type,
        string savePath, JObject optionMeta, string modName)
    {
        // category: JSON 中显式指定的用原值，未指定则用 modId（内部 ID 不变）
        var hasExplicitCategory = optionMeta["category"] != null;
        var categoryStr = hasExplicitCategory
            ? optionMeta["category"]!.Value<string>() ?? modId
            : modId;
        var isStandard = TryMapCategory(categoryStr, out var standardCategory);
        var id = BuildId(modId, categoryStr, key);

        // 选项卡显示名：显式 category 用原值，否则用脚本名称
        var displayCategory = hasExplicitCategory ? categoryStr : modName;

        // CCL 用 HashSet 去重，重复 ID 直接拒绝
        if (!s_registeredIds.Add(id))
            return false;

        // 标签/描述由 mod 自己的 LangGenerator 通过 Option() 注册，这里只用 id 作为 locale key
        var descId = $"{id}dsc";

        // 构建 ModOptionDefinition 并注册到 CCL
        var valueToken = optionMeta["default"];
        switch (type)
        {
            case "bool":
                return RegisterBool(id, descId, isStandard, standardCategory, displayCategory,
                    valueToken, modId, key, savePath);
            case "int":
                return RegisterInt(id, descId, isStandard, standardCategory, displayCategory,
                    valueToken, optionMeta, modId, key, savePath);
            case "float":
                return RegisterFloat(id, descId, isStandard, standardCategory, displayCategory,
                    valueToken, optionMeta, modId, key, savePath);
            case "dropdown":
                return RegisterDropdown(id, descId, isStandard, standardCategory, displayCategory,
                    valueToken, optionMeta, modId, key, savePath);
            case "keybind":
                return RegisterKeybind(id, descId, isStandard, standardCategory, displayCategory,
                    valueToken, modId, key, savePath);
            default:
                Warn("options_util.unknown_type",
                    modId, key, type);
                return false;
        }
    }

    private static bool RegisterBool(string id, string desc,
        bool isStandard, Setting.SettingCategory standardCategory, string displayCategory,
        JToken? valueToken, string modId, string key, string savePath)
    {
        var defaultValue = valueToken?.Value<bool>() ?? false;
        var cacheKey = $"{modId}.{key}";
        s_cache[cacheKey] = defaultValue;

        var registered = isStandard
            ? ModOptionsRegistry.Register(ModOptionDefinition.Bool(id, id, desc,
                standardCategory, defaultValue,
                val => ApplyAndWrite(cacheKey, val, savePath)))
            : ModOptionsRegistry.Register(ModOptionDefinition.Bool(id, id, desc,
                displayCategory, defaultValue,
                val => ApplyAndWrite(cacheKey, val, savePath)));

        return registered;
    }

    private static bool RegisterInt(string id, string desc,
        bool isStandard, Setting.SettingCategory standardCategory, string displayCategory,
        JToken? valueToken, JObject optionMeta, string modId, string key, string savePath)
    {
        var defaultValue = valueToken?.Value<int>() ?? 0;
        var min = optionMeta["min"]?.Value<int>() ?? 0;
        var max = optionMeta["max"]?.Value<int>() ?? 100;
        var cacheKey = $"{modId}.{key}";
        s_cache[cacheKey] = defaultValue;

        var registered = isStandard
            ? ModOptionsRegistry.Register(ModOptionDefinition.Int(id, id, desc,
                standardCategory, defaultValue, min, max,
                val => ApplyAndWrite(cacheKey, val, savePath)))
            : ModOptionsRegistry.Register(ModOptionDefinition.Int(id, id, desc,
                displayCategory, defaultValue, min, max,
                val => ApplyAndWrite(cacheKey, val, savePath)));

        return registered;
    }

    private static bool RegisterFloat(string id, string desc,
        bool isStandard, Setting.SettingCategory standardCategory, string displayCategory,
        JToken? valueToken, JObject optionMeta, string modId, string key, string savePath)
    {
        var defaultValue = valueToken?.Value<float>() ?? 0f;
        var min = optionMeta["min"]?.Value<float>() ?? 0f;
        var max = optionMeta["max"]?.Value<float>() ?? 100f;
        var cacheKey = $"{modId}.{key}";
        s_cache[cacheKey] = defaultValue;

        var registered = isStandard
            ? ModOptionsRegistry.Register(ModOptionDefinition.Float(id, id, desc,
                standardCategory, defaultValue, min, max,
                val => ApplyAndWrite(cacheKey, val, savePath)))
            : ModOptionsRegistry.Register(ModOptionDefinition.Float(id, id, desc,
                displayCategory, defaultValue, min, max,
                val => ApplyAndWrite(cacheKey, val, savePath)));

        return registered;
    }

    private static bool RegisterDropdown(string id, string desc,
        bool isStandard, Setting.SettingCategory standardCategory, string displayCategory,
        JToken? valueToken, JObject optionMeta, string modId, string key, string savePath)
    {
        var choicesToken = optionMeta["choices"];
        if (choicesToken is not JArray choicesArray || choicesArray.Count == 0)
        {
            Warn("options_util.dropdown_no_choices",
                modId, key);
            return false;
        }

        var choices = new ModDropdownChoice[choicesArray.Count];
        for (var i = 0; i < choicesArray.Count; i++)
        {
            var choiceText = choicesArray[i].Value<string>() ?? i.ToString();
            choices[i] = new ModDropdownChoice(choiceText, choiceText);
        }

        var defaultValue = valueToken?.Value<int>() ?? 0;
        if (defaultValue < 0 || defaultValue >= choices.Length)
            defaultValue = 0;

        var cacheKey = $"{modId}.{key}";
        s_cache[cacheKey] = defaultValue;

        var registered = isStandard
            ? ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(id, id, desc,
                standardCategory, defaultValue, choices,
                val => ApplyAndWrite(cacheKey, val, savePath)))
            : ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(id, id, desc,
                displayCategory, defaultValue, choices,
                val => ApplyAndWrite(cacheKey, val, savePath)));

        return registered;
    }

    private static bool RegisterKeybind(string id, string desc,
        bool isStandard, Setting.SettingCategory standardCategory, string displayCategory,
        JToken? valueToken, string modId, string key, string savePath)
    {
        var keyStr = valueToken?.Value<string>() ?? string.Empty;
        var defaultValue = ParseKeyCode(keyStr);
        var cacheKey = $"{modId}.{key}";
        s_cache[cacheKey] = keyStr;

        var registered = isStandard
            ? ModOptionsRegistry.Register(ModOptionDefinition.Keybind(id, id, desc,
                standardCategory, defaultValue,
                val => ApplyAndWrite(cacheKey, KeyCodeToString(val), savePath)))
            : ModOptionsRegistry.Register(ModOptionDefinition.Keybind(id, id, desc,
                displayCategory, defaultValue,
                val => ApplyAndWrite(cacheKey, KeyCodeToString(val), savePath)));

        return registered;
    }

    private static void Warn(string key, params object[] args)
    {
        LogUtil.Warning(key, args);
    }

    private static void Info(string key, params object[] args)
    {
        LogUtil.Info(key, args);
    }

    // apply 回调：更新缓存 + 写回用户配置
    private static void ApplyAndWrite(string cacheKey, object value, string savePath)
    {
        s_cache[cacheKey] = value;
        WriteConfigFile(savePath);
    }

    // 将 s_cache 中以 modId 打头的所有 key 写入 {configsDir}/{modId}.json
    private static void WriteConfigFile(string savePath)
    {
        // 从 savePath 反推 modId（savePath = ".../Configs/{modId}.json"）
        var modId = Path.GetFileNameWithoutExtension(savePath);
        var prefix = $"{modId}.";

        var data = new Dictionary<string, object>();
        foreach (var kv in s_cache)
        {
            if (!kv.Key.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            var key = kv.Key.Substring(prefix.Length);
            data[key] = kv.Value;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
            File.WriteAllText(savePath,
                JsonConvert.SerializeObject(data, Formatting.Indented) + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Warn("options_util.write_config_failed",
                savePath, ex.Message);
        }
    }

    // 生成 CCL 选项 ID（与 BetterOptions.MakeId() 逻辑保持一致）
    private static string BuildId(string ns, string category, string key)
    {
        if (TryMapCategory(category, out _))
            return $"{ns}.{category.ToLowerInvariant()}.{key}";

        return $"{ns}.{category.ToLowerInvariant().Replace(" ", "_")}.{key}";
    }

    // 标准分类映射
    private static bool TryMapCategory(string category, out Setting.SettingCategory result)
    {
        var normalized = category.Trim().ToLowerInvariant();
        switch (normalized)
        {
            case "game":
                result = Setting.SettingCategory.Game;
                return true;
            case "audio":
                result = Setting.SettingCategory.Audio;
                return true;
            case "input":
                result = Setting.SettingCategory.Input;
                return true;
            case "video":
                result = Setting.SettingCategory.Video;
                return true;
            default:
                result = default;
                return false;
        }
    }

    // KeyCode 字符串解析（大小写不敏感）
    private static KeyCode ParseKeyCode(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return KeyCode.None;

        if (Enum.TryParse(s, true, out KeyCode result))
            return result;

        Warn("options_util.keycode_parse_failed", s);
        return KeyCode.None;
    }

    private static string KeyCodeToString(KeyCode k)
    {
        return k.ToString();
    }
}