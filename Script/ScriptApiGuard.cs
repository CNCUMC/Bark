using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bark.ScriptApi;

namespace Bark.Script;

public static class ScriptApiGuard
{
    private static readonly HashSet<string> JsSpecial =
        new(StringComparer.Ordinal) { "Log", "Locale", "ScriptInfo" };

    private static readonly HashSet<string> LuaSpecial =
        new(StringComparer.Ordinal) { "Log", "Locale", "ScriptInfo", "CS" };

    public static IReadOnlyCollection<string> GetReservedNames(ScriptLanguage lang)
    {
        var set = new HashSet<string>(ApiRegistry.Proxies.Keys, StringComparer.Ordinal);
        set.UnionWith(lang == ScriptLanguage.Lua
            ? LuaSpecial
            : JsSpecial);
        return set;
    }

    public static List<(string Name, int Line)> FindReservedOverrides(string source,
        ScriptLanguage lang)
    {
        var reserved = GetReservedNames(lang);
        var result = new List<(string, int)>();

        if (string.IsNullOrEmpty(source))
            return result;

        var lines = source.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimStart();
            if (IsComment(line, lang))
                continue;

            result.AddRange(
                from name in reserved
                where IsOverride(line, name, lang)
                select (name, i + 1));
        }

        return result;
    }

    private static bool IsComment(string trimmedLine, ScriptLanguage lang)
    {
        if (lang == ScriptLanguage.Lua)
            return trimmedLine.StartsWith("--");
        return trimmedLine.StartsWith("//") || trimmedLine.StartsWith("/*");
    }

    private static bool IsOverride(string line, string name, ScriptLanguage lang)
    {
        var escaped = Regex.Escape(name);

        if (Regex.IsMatch(line, $@"\b{escaped}\.\w"))
            return false;

        if (lang == ScriptLanguage.Lua)
        {
            if (Regex.IsMatch(line, $@"\bfunction\s+{escaped}\b"))
                return true;
        }
        else
        {
            if (Regex.IsMatch(line, $@"(?:var|let|const|function)\s+{escaped}\b"))
                return true;
        }

        if (Regex.IsMatch(line, $@"(?:^|[^.\w]){escaped}\s*="))
            return true;

        return false;
    }
}
