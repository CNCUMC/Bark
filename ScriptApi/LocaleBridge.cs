using System;

namespace Bark.ScriptApi;

internal static class LocaleBridge
{
    public static Func<string, string>? Getter;
    public static Func<string, object[], string>? FormattedGetter;
    public static Func<string, bool>? KeyChecker;

    public static void Reset()
    {
        Getter = null;
        FormattedGetter = null;
        KeyChecker = null;
    }
}