using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Bark.BetterCCL;
using BepInEx.Logging;
using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[HarmonyPatch(typeof(ConsoleScript))]
public static class LogUtil
{
    public const int MaxLogCount = 100;

    public static void LogToConsole(string text)
    {
        var cs = GameInstances.Console;
        if (cs == null) return;
        cs.logs.Add($"[<alpha=#55>{TimeSpan.FromSeconds(Time.realtimeSinceStartup):mm\\:ss}<alpha=#FF>] {text}");
        if (cs.logs.Count > MaxLogCount) cs.logs.RemoveAt(0);
        if (!cs.active) return;
        if (cs.logText != null) cs.logText.text = string.Join("\n", cs.logs);
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
    
    public static void CheckWorld(ManualLogSource? logger = null)
    {
        if (!CUCoreUtils.IsInWorld()) throw Fail("world.check_for_world", logger);
    }

    public static void CheckBody(ManualLogSource? logger = null)
    {
        CheckWorld(logger);
        if (GameInstances.Body == null) throw Fail("player.body_null", logger);
    }

    public static void CheckConsole(ManualLogSource? logger = null)
    {
        if (GameInstances.Console == null) throw Fail("console.not_initialized", logger);
    }

    public static void CheckArgumentCount(string[] args, int minCount, ManualLogSource? logger = null)
    {
        if (args == null) throw new ArgumentNullException(nameof(args));
        if (args.Length <= minCount) throw Fail("utils.check_argument_count", logger, minCount, args.Length - 1);
    }

    public static void CheckNotNullOrEmpty(string value, string paramName, ManualLogSource? logger = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(Locale("utils.string.null_or_empty"), paramName);
    }

    public static float CheckParseFloat(string s, ManualLogSource? logger = null)
    {
        if (string.IsNullOrWhiteSpace(s)) throw Fail("utils.string.null_or_empty", logger);
        return float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture,
            out var r)
            ? r
            : throw Fail("utils.parse.float_invalid", logger, s);
    }

    public static int CheckParseInt(string s, ManualLogSource? logger = null)
    {
        if (string.IsNullOrWhiteSpace(s)) throw Fail("utils.string.null_or_empty", logger);
        return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)
            ? r
            : throw Fail("utils.parse.int_invalid", logger, s);
    }

    private static Exception Fail(string key, ManualLogSource? logger, params object[] args)
    {
        var msg = Locale(key, args);
        Error(msg, logger);
        return new InvalidOperationException(msg);
    }

    public static bool TryParseFloat(string s, out float result, ManualLogSource logger)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(s)) return false;
        if (float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture,
                out result)) return true;
        Error(Locale("utils.parse.float_invalid", s), logger);
        return false;
    }

    public static bool TryParseInt(string s, out int result, ManualLogSource logger)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(s)) return false;
        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out result)) return true;
        Error(Locale("utils.parse.int_invalid", s), logger);
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

    public static void PrintNumberedList(string header, IList<string> items, ManualLogSource logger, int startIndex = 1)
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

    public static void PrintGroupedList(string header, IEnumerable<(string groupName, IList<string> items)> groups,
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

    public static void PrintEmpty(string message, ManualLogSource logger)
    {
        Info(message, logger);
    }

    private static string Locale(string key, params object[] args)
    {
        return BetterLocale.GetLog(key, args);
    }
}