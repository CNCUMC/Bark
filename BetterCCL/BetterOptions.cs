using System;
using CUCoreLib.Data;
using CUCoreLib.Registries;
using UnityEngine;

namespace Bark.BetterCCL;

// CCL 设置注册封装
public static class BetterOptions
{
    private static string CategoryString(Setting.SettingCategory category) =>
        category.ToString().ToLowerInvariant();

    private static string MakeId(string ns, Setting.SettingCategory category, string key) =>
        $"{ns}.{CategoryString(category)}.{key}";

    private static string MakeId(string ns, string customCategory, string key) =>
        $"{ns}.{customCategory.ToLowerInvariant()}.{key}";

    private static string LabelText(string ns, Setting.SettingCategory category, string key) =>
        MakeId(ns, category, key);

    private static string LabelText(string ns, string customCategory, string key) =>
        MakeId(ns, customCategory, key);

    private static string DescriptionText(string ns, Setting.SettingCategory category, string key) =>
        $"{MakeId(ns, category, key)}dsc";

    private static string DescriptionText(string ns, string customCategory, string key) =>
        $"{MakeId(ns, customCategory, key)}dsc";

    // ==================== Setting.SettingCategory ====================

    public static void Float(string ns, string key, Setting.SettingCategory category,
        float defaultValue, float min, float max,
        Action<float>? apply = null, Func<float, string>? formatValue = null) =>
        ModOptionsRegistry.Register(ModOptionDefinition.Float(
            MakeId(ns, category, key), LabelText(ns, category, key), DescriptionText(ns, category, key),
            category, defaultValue, min, max, apply, formatValue));

    public static void Int(string ns, string key, Setting.SettingCategory category,
        int defaultValue, int min, int max, Action<int>? apply = null) =>
        ModOptionsRegistry.Register(ModOptionDefinition.Int(
            MakeId(ns, category, key), LabelText(ns, category, key), DescriptionText(ns, category, key),
            category, defaultValue, min, max, apply));

    public static void Bool(string ns, string key, Setting.SettingCategory category,
        bool defaultValue, Action<bool>? apply = null) =>
        ModOptionsRegistry.Register(ModOptionDefinition.Bool(
            MakeId(ns, category, key), LabelText(ns, category, key), DescriptionText(ns, category, key),
            category, defaultValue, apply));

    public static void Dropdown(string ns, string key, Setting.SettingCategory category,
        int defaultValue, ModDropdownChoice[] choices, Action<int>? apply = null) =>
        ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
            MakeId(ns, category, key), LabelText(ns, category, key), DescriptionText(ns, category, key),
            category, defaultValue, choices, apply));

    public static void Keybind(string ns, string key, Setting.SettingCategory category,
        KeyCode defaultValue, Action<KeyCode>? apply = null) =>
        ModOptionsRegistry.Register(ModOptionDefinition.Keybind(
            MakeId(ns, category, key), LabelText(ns, category, key), DescriptionText(ns, category, key),
            category, defaultValue, apply));

    // ==================== Custom category (string) ====================

    public static void Float(string ns, string key, string customCategory,
        float defaultValue, float min, float max,
        Action<float>? apply = null, Func<float, string>? formatValue = null) =>
        ModOptionsRegistry.Register(ModOptionDefinition.Float(
            MakeId(ns, customCategory, key), LabelText(ns, customCategory, key),
            DescriptionText(ns, customCategory, key),
            customCategory, defaultValue, min, max, apply, formatValue));

    public static void Int(string ns, string key, string customCategory,
        int defaultValue, int min, int max, Action<int>? apply = null) =>
        ModOptionsRegistry.Register(ModOptionDefinition.Int(
            MakeId(ns, customCategory, key), LabelText(ns, customCategory, key),
            DescriptionText(ns, customCategory, key),
            customCategory, defaultValue, min, max, apply));

    public static void Bool(string ns, string key, string customCategory,
        bool defaultValue, Action<bool>? apply = null) =>
        ModOptionsRegistry.Register(ModOptionDefinition.Bool(
            MakeId(ns, customCategory, key), LabelText(ns, customCategory, key),
            DescriptionText(ns, customCategory, key),
            customCategory, defaultValue, apply));

    public static void Dropdown(string ns, string key, string customCategory,
        int defaultValue, ModDropdownChoice[] choices, Action<int>? apply = null) =>
        ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
            MakeId(ns, customCategory, key), LabelText(ns, customCategory, key),
            DescriptionText(ns, customCategory, key),
            customCategory, defaultValue, choices, apply));

    public static void Keybind(string ns, string key, string customCategory,
        KeyCode defaultValue, Action<KeyCode>? apply = null) =>
        ModOptionsRegistry.Register(ModOptionDefinition.Keybind(
            MakeId(ns, customCategory, key), LabelText(ns, customCategory, key),
            DescriptionText(ns, customCategory, key),
            customCategory, defaultValue, apply));
}