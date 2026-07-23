using System;
using System.Collections.Generic;

namespace Bark.ScriptApi;

// 统一 API 注册表：管理所有 [ScriptMethod] 标记的 Tool 类型，
// 通过 AutoApi 为每个类型生成代理实例，按 camelCase 类名暴露给脚本引擎
public static class ApiRegistry
{
    // (camelCase 类名) → (代理实例)
    private static readonly Dictionary<string, object> s_proxies = new(StringComparer.Ordinal);

    // 只读视图，供脚本引擎遍历注入
    public static IReadOnlyDictionary<string, object> Proxies => s_proxies;

    // 注册一个 utility 类型：BodyUtil → 生成 bodyUtil 代理
    public static void Register(Type utilityType)
    {
        if (utilityType is null) throw new ArgumentNullException(nameof(utilityType));
        var name = ClassNameToCamelCase(utilityType.Name);
        s_proxies[name] = AutoApi.CreateProxy(utilityType);
    }

    // 按 camelCase 类名获取代理实例（供 PuerTS Lua/JS 侧调用）
    public static object GetProxy(string className)
    {
        if (className is null) throw new ArgumentNullException(nameof(className));
        return s_proxies.TryGetValue(className, out var proxy)
            ? proxy
            : throw new KeyNotFoundException($"Type '{className}' not found in ApiRegistry.");
    }

    // 清除所有已注册的代理
    public static void Clear()
    {
        s_proxies.Clear();
    }

    // "BodyUtil" → "bodyUtil", "PlayerUtil" → "playerUtil"
    private static string ClassNameToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
