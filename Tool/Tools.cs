using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CUCoreLib.Registries;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class Tools
{
    private const string LocaleKeyPre = "tool_utils_";

    public static void CheckArgumentCount(string[] args, int desired)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        if (args.Length <= desired)
            throw new Exception(Locale("checkargumentcount", desired, args.Length - 1));
    }

    public static float ParseFloat(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException(Locale("string_nullorempty"), nameof(s));

        return !float.TryParse(
            s, NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.InvariantCulture, out var result)
            ? throw new FormatException(Locale("parse_float_invalid", s))
            : result;
    }

    public static int ParseInt(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException(Locale("string_nullorempty"), nameof(s));

        return !int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? throw new FormatException(Locale("parse_int_invalid", s))
            : result;
    }

    public static bool TryParseFloat(string s, out float result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(s))
            return false;

        return float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.InvariantCulture, out result);
    }

    private static string Locale(string key, params object[] args)
    {
        var fullKey = $"{LocaleKeyPre}{key}";
        var text = LocaleRegistry.Get("other", fullKey, fullKey);
        return args.Length > 0 ? string.Format(text, args) : text;
    }
}
