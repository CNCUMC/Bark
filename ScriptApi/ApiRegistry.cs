using System;
using System.Collections.Generic;
using System.Reflection;
using Bark.Tool;

namespace Bark.ScriptApi;

public static class ApiRegistry
{
    private static readonly Dictionary<string, object> s_proxies =
        new(StringComparer.Ordinal);

    public static IReadOnlyDictionary<string, object> Proxies => s_proxies;

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
            }

        LogUtil.Info("api.scanned", registeredCount.ToString());
    }

    public static void Register(Type utilityType)
    {
        if (utilityType is null) throw new ArgumentNullException(nameof(utilityType));
        var name = DeriveApiName(utilityType.Name);
        s_proxies[name] = AutoApi.CreateProxy(utilityType);
    }

    public static object GetProxy(string className)
    {
        if (className is null) throw new ArgumentNullException(nameof(className));
        return s_proxies.TryGetValue(className, out var proxy)
            ? proxy
            : throw new KeyNotFoundException(
                $"Type '{className}' not found in ApiRegistry.");
    }

    public static void Clear()
    {
        s_proxies.Clear();
    }

    private static string DeriveApiName(string typeName)
    {
        return typeName.EndsWith("Util", StringComparison.Ordinal)
            ? typeName[..^4]
            : typeName;
    }
}
