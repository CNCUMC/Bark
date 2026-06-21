using System;
using CUCoreLib.Data;
using CUCoreLib.Registries;
using UnityEngine;

namespace Bark.Tool.BetterCCL;

// CCL 设置注册封装
// 提供 Float/Int/Bool/Dropdown/Keybind 五种设置类型
// ID 格式: 命名空间.分类.键 (例 "bark.game.test")
// 分类自动从 Setting.SettingCategory 转换为小写
// 标签: BetterLocale.Other("{ns}.{category}.{key}")
// 描述: BetterLocale.Other("{ns}.{category}.{key}dsc")
public static class BetterOptions
{
    private static string Cat(Setting.SettingCategory category) =>
        category.ToString().ToLowerInvariant();

    private static string MakeId(string ns, Setting.SettingCategory category, string key) =>
        $"{ns}.{Cat(category)}.{key}";

    private static string MakeLabel(string ns, Setting.SettingCategory category, string key) =>
        BetterLocale.Other($"{ns}.{Cat(category)}.{key}");

    private static string MakeDescription(string ns, Setting.SettingCategory category, string key) =>
        BetterLocale.Other($"{ns}.{Cat(category)}.{key}dsc");

    // 注册浮点数设置 (滑块)
    public static void Float(
        string ns, string key, Setting.SettingCategory category,
        float defaultValue, float min, float max,
        Action<float>? apply = null,
        Func<float, string>? formatValue = null)
    {
        ModOptionsRegistry.Register(ModOptionDefinition.Float(
            MakeId(ns, category, key),
            MakeLabel(ns, category, key),
            MakeDescription(ns, category, key),
            category, defaultValue, min, max, apply, formatValue));
    }

    // 注册整数设置
    public static void Int(
        string ns, string key, Setting.SettingCategory category,
        int defaultValue, int min, int max,
        Action<int>? apply = null)
    {
        ModOptionsRegistry.Register(ModOptionDefinition.Int(
            MakeId(ns, category, key),
            MakeLabel(ns, category, key),
            MakeDescription(ns, category, key),
            category, defaultValue, min, max, apply));
    }

    // 注册布尔开关 (Toggle)
    public static void Bool(
        string ns, string key, Setting.SettingCategory category,
        bool defaultValue,
        Action<bool>? apply = null)
    {
        ModOptionsRegistry.Register(ModOptionDefinition.Bool(
            MakeId(ns, category, key),
            MakeLabel(ns, category, key),
            MakeDescription(ns, category, key),
            category, defaultValue, apply));
    }

    // 注册下拉选择
    public static void Dropdown(
        string ns, string key, Setting.SettingCategory category,
        int defaultValue, ModDropdownChoice[] choices,
        Action<int>? apply = null)
    {
        ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
            MakeId(ns, category, key),
            MakeLabel(ns, category, key),
            MakeDescription(ns, category, key),
            category, defaultValue, choices, apply));
    }

    // 注册按键绑定
    public static void Keybind(
        string ns, string key, Setting.SettingCategory category,
        KeyCode defaultValue,
        Action<KeyCode>? apply = null)
    {
        ModOptionsRegistry.Register(ModOptionDefinition.Keybind(
            MakeId(ns, category, key),
            MakeLabel(ns, category, key),
            MakeDescription(ns, category, key),
            category, defaultValue, apply));
    }
}
