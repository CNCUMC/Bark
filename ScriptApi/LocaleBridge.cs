using System;

namespace Bark.ScriptApi;

// 内部桥接：由 Script/ScriptLocaleManager 在初始化时注入委托，
// 消除 ScriptApi → Script 的直接静态依赖
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
