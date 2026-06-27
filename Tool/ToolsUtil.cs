using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Bark.BetterCCL;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class ToolsUtil
{
    public static void CheckArgumentCount(string[] args, int desired)
    {
        if (args == null) throw new ArgumentNullException(nameof(args));
        if (args.Length <= desired) throw new Exception(Locale("utils.check_argument_count", desired, args.Length - 1));
    }

    public static float ParseFloat(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException(Locale("utils.string.null_or_empty"), nameof(text));
        return float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture,
            out var result)
            ? result
            : throw new FormatException(Locale("utils.parse.float_invalid", text));
    }

    public static int ParseInt(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException(Locale("utils.string.null_or_empty"), nameof(text));
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : throw new FormatException(Locale("utils.parse.int_invalid", text));
    }

    public static bool TryParseFloat(string text, out float result)
    {
        result = 0;
        return !string.IsNullOrWhiteSpace(text) && float.TryParse(text,
            NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.InvariantCulture, out result);
    }

    private static string Locale(string key, params object[] args)
    {
        return BetterLocale.GetLog(key, args);
    }
}