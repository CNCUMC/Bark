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
        LogUtil.CheckConsole(Plugin.Logger);
        LogUtil.CheckNotNullOrEmpty(key, nameof(key));
        ConsoleScript.SearchExact(key).action(key.Split());
    }

    private static string Locale(string key, params object[] args)
    {
        return BetterLocale.Other("log.console." + key, args);
    }
}