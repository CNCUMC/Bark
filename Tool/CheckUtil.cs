using System;
using System.Globalization;
using Bark.BetterCCL;
using BepInEx.Logging;
using CUCoreLib.Helpers;

namespace Bark.Tool;

public static class CheckUtil
{
    public static void CheckWorld(ManualLogSource? logger = null)
    {
        if (CUCoreUtils.IsInWorld()) return;
        if (logger != null) throw Fail("check.check_for_world", logger);
    }

    public static void CheckBody(ManualLogSource? logger = null)
    {
        CheckWorld();
        if (CUCoreUtils.TryGetBody(out _)) return;
        if (logger != null) throw Fail("check.body_null", logger);
    }

    public static void CheckConsole(ManualLogSource? logger = null)
    {
        if (ConsoleScript.instance != null) return;
        if (logger != null) throw Fail("check.console_not_initialized", logger);
    }

    public static void CheckArgumentCount(string[] args, int minCount, ManualLogSource? logger = null)
    {
        if (args == null) throw new ArgumentNullException(nameof(args));
        if (args.Length >= minCount) return;
        if (logger != null)
            throw Fail("check.check_argument_count", logger, minCount, args.Length);
    }

    public static void CheckNotNullOrEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(BetterLocale.GetLog("check.string.null_or_empty"), paramName);
    }

    public static float CheckParseFloat(string parse, ManualLogSource? logger = null)
    {
        if (string.IsNullOrWhiteSpace(parse))
        {
            if (logger != null)
                throw Fail("check.string.null_or_empty", logger);
            return 0f;
        }

        if (float.TryParse(parse, NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out var result))
            return result;

        if (logger != null)
            throw Fail("check.parse.float_invalid", logger, parse);
        return 0f;
    }

    public static int CheckParseInt(string parse, ManualLogSource? logger = null)
    {
        if (string.IsNullOrWhiteSpace(parse))
        {
            if (logger != null)
                throw Fail("check.string.null_or_empty", logger);
            return 0;
        }

        if (int.TryParse(parse, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;

        if (logger != null)
            throw Fail("check.parse.int_invalid", logger, parse);
        return 0;
    }

    private static Exception Fail(string key, ManualLogSource logger, params object[] args)
    {
        var msg = BetterLocale.GetLog(key, args);
        LogUtil.Error(msg, logger);
        return new InvalidOperationException(msg);
    }
}