using System.Collections.Generic;
using System.Globalization;
using Bark.BetterCCL;
using BepInEx.Logging;
using CUCoreLib.Helpers;

namespace Bark.Tool;

public static class LogUtil
{
    public static void LogToConsole(string text)
    {
        if (ConsoleScript.instance == null) return;
        CUCoreUtils.ConsoleLog(ConsoleScript.instance, text);
    }

    public static void NewLine()
    {
        LogToConsole("");
    }

    public static void Divider(char divider = '-', int length = 27)
    {
        LogToConsole(new string(divider, length));
    }

    public static void Info(string text, ManualLogSource? logger)
    {
        LogToConsole(text);
        logger?.LogInfo(text);
    }

    public static void Error(string text, ManualLogSource? logger)
    {
        LogToConsole($"[ERROR] {text}");
        logger?.LogError(text);
    }

    public static void Warning(string text, ManualLogSource? logger)
    {
        LogToConsole($"[WARNING] {text}");
        logger?.LogWarning(text);
    }

    public static void Debug(string text, ManualLogSource? logger)
    {
        LogToConsole($"[DEBUG] {text}");
        logger?.LogDebug(text);
    }

    public static void Fatal(string text, ManualLogSource? logger)
    {
        LogToConsole($"[FATAL] {text}");
        logger?.LogFatal(text);
    }

    public static void Message(string text, ManualLogSource? logger)
    {
        LogToConsole($"[MESSAGE] {text}");
        logger?.LogMessage(text);
    }

    public static bool TryParseFloat(string s, out float result, ManualLogSource logger)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(s)) return false;
        if (float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture,
                out result)) return true;
        Error(LocaleLog("utils.parse.float_invalid", s), logger);
        return false;
    }

    public static bool TryParseInt(string s, out int result, ManualLogSource logger)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(s)) return false;
        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out result)) return true;
        Error(LocaleLog("utils.parse.int_invalid", s), logger);
        return false;
    }

    public static void PrintList(string header, IEnumerable<string> items, ManualLogSource logger,
        string itemPrefix = "    ")
    {
        Divider();
        Info(header, logger);
        foreach (var item in items) Info($"{itemPrefix}{item}", logger);
        Divider();
    }

    public static void PrintNumberedList(string header, IList<string> items, ManualLogSource logger,
        int startIndex = 1)
    {
        Divider();
        Info(header, logger);
        for (var i = 0; i < items.Count; i++) Info($"    {startIndex + i}. {items[i]}", logger);
        Divider();
    }

    public static void PrintKeyValueList(string header, IEnumerable<(string key, string value)> entries,
        ManualLogSource logger)
    {
        Divider();
        Info(header, logger);
        foreach (var (k, v) in entries) Info($"    {k}: {v}", logger);
        Divider();
    }

    public static void PrintGroupedList(string header,
        IEnumerable<(string groupName, IList<string> items)> groups,
        ManualLogSource logger, string groupPrefix = "    ", string itemPrefix = "        ")
    {
        Divider();
        Info(header, logger);
        foreach (var (gn, items) in groups)
        {
            Info($"{groupPrefix}{gn}:", logger);
            foreach (var it in items) Info($"{itemPrefix}{it}", logger);
        }

        Divider();
    }

    internal static void Info(string text, params object[] args)
    {
        Info(LocaleLog(text, args), Plugin.Logger);
    }

    internal static void Error(string text, params object[] args)
    {
        Error(LocaleLog(text, args), Plugin.Logger);
    }

    internal static void Warning(string text, params object[] args)
    {
        Warning(LocaleLog(text, args), Plugin.Logger);
    }

    internal static void Debug(string text, params object[] args)
    {
        Debug(LocaleLog(text, args), Plugin.Logger);
    }

    internal static void Fatal(string text, params object[] args)
    {
        Fatal(LocaleLog(text, args), Plugin.Logger);
    }

    internal static void Message(string text, params object[] args)
    {
        Message(LocaleLog(text, args), Plugin.Logger);
    }

    private static string LocaleLog(string key, params object[] args)
    {
        return BetterLocale.GetLog($"{Plugin.NameSpace}.{key}", args);
    }
}