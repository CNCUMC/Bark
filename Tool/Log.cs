using System;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;
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
}