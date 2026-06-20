using System;
using System.Diagnostics.CodeAnalysis;
using CUCoreLib.Registries;

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
                LocaleRegistry.Get("other", "tool.console.null_or_empty", "Command cannot be null or empty"), nameof(key));

        if (Instance == null)
            throw new InvalidOperationException(
                LocaleRegistry.Get("other", "tool.console.not_initialized", "ConsoleScript not initialized"));

        ConsoleScript.SearchExact(key).action(key.Split());
    }
}
