using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Bark.ScriptApi;

// 运行时代理生成器：扫描指定类型中标记了 [ScriptMethod] 的静态方法，
// 动态生成包含对应实例方法的代理类型，供 PuerTS 直接绑定
public static class AutoApi
{
    // (类型组合签名) → 生成的代理类型
    private static readonly Dictionary<string, Type> s_proxyCache = new(StringComparer.Ordinal);

    // 扫描 utilityTypes 中所有 [ScriptMethod] 方法，生成代理实例
    public static object CreateProxy(params Type[] utilityTypes)
    {
        if (utilityTypes is null) throw new ArgumentNullException(nameof(utilityTypes));
        if (utilityTypes.Length == 0)
            throw new ArgumentException("至少提供一个 utility 类型", nameof(utilityTypes));

        var key = string.Join("|", utilityTypes.Select(t => t.FullName));
        if (s_proxyCache.TryGetValue(key, out var proxyType)) return Activator.CreateInstance(proxyType)!;
        proxyType = BuildProxyType(utilityTypes);
        s_proxyCache[key] = proxyType;

        return Activator.CreateInstance(proxyType)!;
    }

    private static Type BuildProxyType(Type[] utilityTypes)
    {
        var assemblyName = new AssemblyName($"AutoApi_{Guid.NewGuid():N}");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("ProxyModule");
        var typeBuilder = module.DefineType("ScriptApiProxy", TypeAttributes.Public | TypeAttributes.Class);

        // (方法名, 参数类型签名) 去重，避免同签名方法冲突
        var registered = new HashSet<string>(StringComparer.Ordinal);

        foreach (var utilityType in utilityTypes)
        {
            foreach (var method in utilityType.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<ScriptMethodAttribute>();
                if (attr == null) continue;

                // Name 显式指定则用指定值，否则自动转 camelCase
                var name = attr.Name ?? ToCamelCase(method.Name);
                var parameters = method.GetParameters();
                var paramTypes = new Type[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                    paramTypes[i] = parameters[i].ParameterType;

                // 签名 key，防止重复定义
                var sigKey = name + "::" + string.Join(",", paramTypes.Select(t => t.FullName));
                if (!registered.Add(sigKey)) continue;

                var methodBuilder = typeBuilder.DefineMethod(
                    name,
                    MethodAttributes.Public,
                    method.ReturnType,
                    paramTypes);

                var il = methodBuilder.GetILGenerator();
                // 实例方法：arg0 = this，参数从 arg1 开始
                for (var i = 0; i < parameters.Length; i++)
                    il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Call, method);
                il.Emit(OpCodes.Ret);
            }
        }

        return typeBuilder.CreateType()!;
    }

    // PascalCase → camelCase（"IsAlive" → "isAlive", "GetHP" → "getHP"）
    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        // 全大写缩略词保持原样（如 "HP" → "hp", 但 "GetHP" → "getHP"）
        if (name.Length == 1) return name.ToLowerInvariant();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
