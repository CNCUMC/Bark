using System;
using System.Diagnostics.CodeAnalysis;
using CUCoreLib.Registries;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class GameConsole
{
    private const string LocaleKeyPre = "log.console.";
    public static readonly ConsoleScript Instance = ConsoleScript.instance;

    public static void RunCommand(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException(
                LocaleRegistry.Get("other", $"{LocaleKeyPre}null_or_empty", "Command cannot be null or empty"), nameof(key));

        if (Instance == null)
            throw new InvalidOperationException(
                LocaleRegistry.Get("other", $"{LocaleKeyPre}notinitialized", "ConsoleScript not initialized"));

        ConsoleScript.SearchExact(key).action(key.Split());
    }
}
