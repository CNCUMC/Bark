using System;
using System.Collections.Generic;
using System.Reflection;
using Bark.Tool;

namespace Bark.ScriptApi;

// 统一 API 注册表：管理所有 [ScriptMethod] 标记的 Tool 类型，
// 通过 AutoApi 为每个类型生成代理实例，以 PascalCase 类名（去 Util 后缀）暴露给脚本引擎
public static class ApiRegistry
{
    // PascalCase API 名 → 代理实例
    private static readonly Dictionary<string, object> s_proxies = new(StringComparer.Ordinal);

    // 只读视图，供脚本引擎遍历注入
    public static IReadOnlyDictionary<string, object> Proxies => s_proxies;

    // 扫描所有已加载程序集，注册标注了 [ScriptApi] 的静态类
    public static void ScanAndRegister()
    {
        s_proxies.Clear();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var registeredCount = 0;

        foreach (var assembly in assemblies)
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attr = type.GetCustomAttribute<ScriptApiAttribute>();
                    if (attr == null) continue;

                    var name = attr.Name ?? DeriveApiName(type.Name);
                    s_proxies[name] = AutoApi.CreateProxy(type);
                    registeredCount++;
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // 跳过无法加载的程序集
            }

        LogUtil.Info("api.scanned", registeredCount.ToString());
    }

    // 注册一个 utility 类型：自动去除 "Util" 后缀作为 registry key（BodyUtil → Body）
    public static void Register(Type utilityType)
    {
        if (utilityType is null) throw new ArgumentNullException(nameof(utilityType));
        var name = DeriveApiName(utilityType.Name);
        s_proxies[name] = AutoApi.CreateProxy(utilityType);
    }

    // 按 API 名获取代理实例（供 PuerTS Lua/JS 侧调用）
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

    // 从类名推导脚本侧 API 名称：默认去掉 "Util" 后缀
    private static string DeriveApiName(string typeName)
    {
        return typeName.EndsWith("Util", StringComparison.Ordinal)
            ? typeName[..^4]
            : typeName;
    }
}