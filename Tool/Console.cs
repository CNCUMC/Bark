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
                LocaleRegistry.Get("other", "tool_console_nullorempty", "Command cannot be null or empty"), nameof(key));

        if (Instance == null)
            throw new InvalidOperationException(
                LocaleRegistry.Get("other", "tool_console_notinitialized", "ConsoleScript not initialized"));

        ConsoleScript.SearchExact(key).action(key.Split());
    }
}
