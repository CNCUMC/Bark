using System;
using System.Globalization;
using Bark.BetterCCL;
using BepInEx.Logging;
using CUCoreLib.Helpers;

namespace Bark.Tool;

public static class CheckUtil
{
    public static void CheckWorld(ManualLogSource logger)
    {
        if (!CUCoreUtils.IsInWorld()) throw Fail("check.check_for_world", logger);
    }

    public static void CheckBody(ManualLogSource logger)
    {
        CheckWorld(logger);
        if (!CUCoreUtils.TryGetBody(out _)) throw Fail("check.body_null", logger);
    }

    public static void CheckConsole(ManualLogSource logger)
    {
        if (ConsoleScript.instance == null) throw Fail("check.console_not_initialized", logger);
    }

    public static void CheckArgumentCount(string[] args, int minCount, ManualLogSource logger)
    {
        if (args == null) throw new ArgumentNullException(nameof(args));
        if (args.Length < minCount)
            throw Fail("check.check_argument_count", logger, minCount, args.Length);
    }

    public static void CheckNotNullOrEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(BetterLocale.GetLog("check.string.null_or_empty"), paramName);
    }

    public static float CheckParseFloat(string parse, ManualLogSource logger)
    {
        if (string.IsNullOrWhiteSpace(parse)) throw Fail("check.string.null_or_empty", logger);
        return float.TryParse(parse, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture,
            out var r)
            ? r
            : throw Fail("check.parse.float_invalid", logger, parse);
    }

    public static int CheckParseInt(string parse, ManualLogSource logger)
    {
        if (string.IsNullOrWhiteSpace(parse)) throw Fail("check.string.null_or_empty", logger);
        return int.TryParse(parse, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)
            ? r
            : throw Fail("check.parse.int_invalid", logger, parse);
    }

    private static Exception Fail(string key, ManualLogSource logger, params object[] args)
    {
        var msg = BetterLocale.GetLog(key, args);
        LogUtil.Error(msg, logger);
        return new InvalidOperationException(msg);
    }
}