using System;
using System.Diagnostics.CodeAnalysis;
using Bark.BetterCCL;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class ConsoleUtil
{
    public static ConsoleScript? Instance => GameInstances.Console;

    public static void RunCommand(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException(Locale("null_or_empty"), nameof(key));
        if (GameInstances.Console == null)
            throw new InvalidOperationException(Locale("not_initialized"));
        ConsoleScript.SearchExact(key).action(key.Split());
    }

    private static string Locale(string key, params object[] args) => BetterLocale.Other("log.console." + key, args);
}
