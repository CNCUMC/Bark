using System;
using System.Diagnostics.CodeAnalysis;
using Bark.Tool.BetterCCL;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class GameConsole
{
    public static readonly ConsoleScript Instance = ConsoleScript.instance;

    public static void RunCommand(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException(
                Locale("null_or_empty"),
                nameof(key));

        if (Instance == null)
            throw new InvalidOperationException(
                Locale("not_initialized"));

        ConsoleScript.SearchExact(key).action(key.Split());
    }

    private static string Locale(string key, params object[] args)
    {
        return BetterLocale.Other("log.console." + key, args);
    }
}