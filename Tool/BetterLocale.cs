using System.Text.RegularExpressions;
using CUCoreLib.Registries;

namespace Bark.Tool;

public static class BetterLocale
{
    public static string Item(string key, params object[]? args)
    {
        return LocaleRegistry.Get("item", Replace(key, args), key);
    }

    public static string Building(string key, params object[]? args)
    {
        return LocaleRegistry.Get("build", Replace(key, args), key);
    }

    public static string Moodle(string key, params object[]? args)
    {
        return LocaleRegistry.Get("moodle", Replace(key, args), key);
    }

    public static string Other(string key, params object[]? args)
    {
        return LocaleRegistry.Get("other", Replace(key, args), key);
    }

    private static string Replace(string key, params object[]? args)
    {
        if (args == null || args.Length == 0)
            return key;

        return Regex.Replace(key, @"\{(\d+)\}", match =>
        {
            var index = int.Parse(match.Groups[1].Value);
            return args[index].ToString();
        });
    }
}