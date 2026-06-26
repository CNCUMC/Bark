using System;
using CUCoreLib.Data;
using CUCoreLib.Registries;
using UnityEngine;

namespace Bark.BetterCCL;

// CCL 设置注册封装
// ID: {ns}.{category}.{key} (例如 "bark.game.test")
// 标签 locale: gameset.{id}
// 描述 locale: gameset.{id}dsc
public static class BetterOptions
{
    private static string Cat(Setting.SettingCategory category)
    {
        return category.ToString().ToLowerInvariant();
    }

    private static string Id(string ns, Setting.SettingCategory category, string key)
    {
        return $"{ns}.{Cat(category)}.{key}";
    }

    private static string Label(string ns, Setting.SettingCategory category, string key)
    {
        return BetterLocale.GetOther($"gameset.{Id(ns, category, key)}");
    }

    private static string Description(string ns, Setting.SettingCategory category, string key)
    {
        return BetterLocale.GetOther($"gameset.{Id(ns, category, key)}dsc");
    }

    public static void Float(
        string ns, string key, Setting.SettingCategory category,
        float defaultValue, float min, float max,
        Action<float>? apply = null,
        Func<float, string>? formatValue = null)
    {
        ModOptionsRegistry.Register(ModOptionDefinition.Float(
            Id(ns, category, key), Label(ns, category, key), Description(ns, category, key),
            category, defaultValue, min, max, apply, formatValue));
    }

    public static void Int(
        string ns, string key, Setting.SettingCategory category,
        int defaultValue, int min, int max,
        Action<int>? apply = null)
    {
        ModOptionsRegistry.Register(ModOptionDefinition.Int(
            Id(ns, category, key), Label(ns, category, key), Description(ns, category, key),
            category, defaultValue, min, max, apply));
    }

    public static void Bool(
        string ns, string key, Setting.SettingCategory category,
        bool defaultValue,
        Action<bool>? apply = null)
    {
        ModOptionsRegistry.Register(ModOptionDefinition.Bool(
            Id(ns, category, key), Label(ns, category, key), Description(ns, category, key),
            category, defaultValue, apply));
    }

    public static void Dropdown(
        string ns, string key, Setting.SettingCategory category,
        int defaultValue, ModDropdownChoice[] choices,
        Action<int>? apply = null)
    {
        ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
            Id(ns, category, key), Label(ns, category, key), Description(ns, category, key),
            category, defaultValue, choices, apply));
    }

    public static void Keybind(
        string ns, string key, Setting.SettingCategory category,
        KeyCode defaultValue,
        Action<KeyCode>? apply = null)
    {
        ModOptionsRegistry.Register(ModOptionDefinition.Keybind(
            Id(ns, category, key), Label(ns, category, key), Description(ns, category, key),
            category, defaultValue, apply));
    }
}