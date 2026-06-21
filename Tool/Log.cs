using System;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;
using CUCoreLib.Registries;
using HarmonyLib;
using UnityEngine;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[HarmonyPatch(typeof(ConsoleScript))]
public static class Log
{
    public const int MaxLogCount = 100;

    private static readonly ConsoleScript ConsoleScript = GameConsole.Instance;

    private static void EnsureConsoleInitialized()
    {
        if (ConsoleScript == null)
            throw new InvalidOperationException(
                "ConsoleScript not initialized. Make sure the game has started.");
    }

    public static void LogToConsole(string text)
    {
        if (ConsoleScript == null)
            return;

        ConsoleScript.logs.Add(
            $"[<alpha=#55>{TimeSpan.FromSeconds(Time.realtimeSinceStartup):mm\\:ss}<alpha=#FF>] {text}");
        if (ConsoleScript.logs.Count > MaxLogCount)
            ConsoleScript.logs.RemoveAt(0);
        if (!ConsoleScript.active)
            return;
        UpdateLogScreen(ConsoleScript);
    }

    public static void UpdateLogScreen(ConsoleScript consoleScript)
    {
        if (consoleScript?.logText == null) return;
        consoleScript.logText.text = string.Join("\n", consoleScript.logs);
    }

    public static void NewLine()
    {
        LogToConsole("");
    }

    public static void Divider(char divider = '-', int length = 27)
    {
        var message = new string(divider, length);
        LogToConsole(message);
    }

    public static void Info(string text, ManualLogSource logger)
    {
        LogToConsole(text);
        logger.LogInfo(text);
    }

    public static void Error(string text, ManualLogSource logger)
    {
        LogToConsole($"[ERROR] {text}");
        logger.LogError(text);
    }

    public static void Warning(string text, ManualLogSource logger)
    {
        LogToConsole($"[WARNING] {text}");
        logger.LogWarning(text);
    }

    public static void Alert(string text, ManualLogSource logger, bool important, float delay = 0f)
    {
        Info(text, logger);
        Player.Alert(text, important, delay);
    }

    // ==================== 通用检查方法 ====================

    /// <summary>
    /// 检查世界是否已加载，否则抛出异常并记录错误。
    /// </summary>
    public static void CheckWorld(ManualLogSource logger)
    {
        if (PlayerCamera.main == null)
        {
            var msg = Locale("tool.world.check_for_world");
            Error(msg, logger);
            throw new InvalidOperationException(msg);
        }
    }

    /// <summary>
    /// 检查玩家身体是否为空，否则抛出异常并记录错误。
    /// </summary>
    public static void CheckPlayerBody(ManualLogSource logger)
    {
        if (PlayerCamera.main?.body == null)
        {
            var msg = Locale("tool.player.body_null");
            Error(msg, logger);
            throw new InvalidOperationException(msg);
        }
    }

    /// <summary>
    /// 验证参数数量，不足时抛出异常并记录错误。
    /// </summary>
    public static void CheckArgumentCount(string[] args, int minCount, ManualLogSource logger)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        if (args.Length <= minCount)
        {
            var msg = Locale("tool.utils.check_argument_count", minCount, args.Length - 1);
            Error(msg, logger);
            throw new Exception(msg);
        }
    }

    /// <summary>
    /// 验证字符串不为空或空白，否则抛出异常并记录错误。
    /// </summary>
    public static void CheckNotNullOrEmpty(string value, string paramName, ManualLogSource logger)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            var msg = Locale("tool.utils.string.null_or_empty");
            Error(msg, logger);
            throw new ArgumentException(msg, paramName);
        }
    }

    /// <summary>
    /// 尝试解析浮点数，失败时记录错误。
    /// </summary>
    public static bool TryParseFloat(string s, out float result, ManualLogSource logger)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(s))
        {
            Error(Locale("tool.utils.string.null_or_empty"), logger);
            return false;
        }

        if (float.TryParse(s, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands,
                System.Globalization.CultureInfo.InvariantCulture, out result))
            return true;

        Error(Locale("tool.utils.parse.float_invalid", s), logger);
        return false;
    }

    /// <summary>
    /// 尝试解析整数，失败时记录错误。
    /// </summary>
    public static bool TryParseInt(string s, out int result, ManualLogSource logger)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(s))
        {
            Error(Locale("tool.utils.string.null_or_empty"), logger);
            return false;
        }

        if (int.TryParse(s, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out result))
            return true;

        Error(Locale("tool.utils.parse.int_invalid", s), logger);
        return false;
    }

    // ==================== Locale 辅助 ====================

    private static string Locale(string key, params object[] args)
    {
        var text = LocaleRegistry.Get("other", key, key);
        return args.Length > 0 ? string.Format(text, args) : text;
    }
}
