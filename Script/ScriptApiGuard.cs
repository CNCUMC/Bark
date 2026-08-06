using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bark.ScriptApi;

namespace Bark.Script;

// Bark 脚本保留词守护：在入口脚本执行前静态扫描源码，检测脚本是否覆盖了 Bark 注入的全局 API 名称
// （如 playerUtil / bodyUtil / Log / Locale / ScriptInfo 等），避免脚本意外 shadow 掉 API，
// 导致后续 onLoad / 事件回调里调用 API 时静默失效。
//
// 仅做警告、不阻断加载：合法脚本（如 Lua 的 local 屏蔽）不会被误杀，但会在日志中提示潜在风险。
public static class ScriptApiGuard
{
    // JS 注入的特殊全局（var 声明）：Log / Locale / ScriptInfo
    private static readonly HashSet<string> JsSpecial =
        new(StringComparer.Ordinal) { "Log", "Locale", "ScriptInfo" };

    // Lua 注入的特殊全局：Log / Locale / ScriptInfo / CS（PuerTS 基础设施，覆盖会破坏一切）
    private static readonly HashSet<string> LuaSpecial =
        new(StringComparer.Ordinal) { "Log", "Locale", "ScriptInfo", "CS" };

    // 返回某语言下 Bark 的全部保留全局名（AutoApi 代理名 + 特殊 API）
    public static IReadOnlyCollection<string> GetReservedNames(ScriptLanguage lang)
    {
        var set = new HashSet<string>(ApiRegistry.Proxies.Keys, StringComparer.Ordinal);
        set.UnionWith(lang == ScriptLanguage.Lua ? LuaSpecial : JsSpecial);
        return set;
    }

    // 扫描源码，返回覆盖保留词的 (名称, 行号[1-based]) 列表。
    // 跳过注释行，排除成员赋值（name.xxx =）与 Lua 的局部声明（local name =），降低误报。
    public static List<(string Name, int Line)> FindReservedOverrides(string source, ScriptLanguage lang)
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

            foreach (var name in reserved)
            {
                if (IsOverride(line, name, lang))
                    result.Add((name, i + 1));
            }
        }

        return result;
    }

    private static bool IsComment(string trimmedLine, ScriptLanguage lang)
    {
        if (lang == ScriptLanguage.Lua)
            return trimmedLine.StartsWith("--");
        // JS：跳过整行 // 与 /* 开头（不处理跨行块注释，足够覆盖绝大多数情况）
        return trimmedLine.StartsWith("//") || trimmedLine.StartsWith("/*");
    }

    // 判断单行是否覆盖了保留词 name（作为全局标识符被赋值/声明）。
    private static bool IsOverride(string line, string name, ScriptLanguage lang)
    {
        var escaped = Regex.Escape(name);

        // 成员赋值 name.xxx = ... 不算覆盖全局，跳过
        if (Regex.IsMatch(line, $@"\b{escaped}\.\w"))
            return false;

        // 声明形式：
        //   JS: var/let/const/function name
        //   Lua: function name（local name 视为局部屏蔽，不计入覆盖）
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

        // 赋值形式：name = ...（Lua 的 local name = 已被上面的排除 + 此正则不含 local 前缀处理）
        // 这里匹配独立的 name 后跟 =，且前面不是 . 或字母数字（避免匹配 member 或更长标识符）
        if (Regex.IsMatch(line, $@"(?:^|[^.\w]){escaped}\s*="))
            return true;

        return false;
    }
}
