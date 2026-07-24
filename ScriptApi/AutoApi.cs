using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Bark.ScriptApi;

// 运行时代理生成器：扫描指定类型中标记了 [ScriptMethod] 的静态方法，
// 动态生成包含对应实例方法的代理类型，供 PuerTS 直接绑定。
// 自动为带默认参数的方法生成去尾部可选参数的重载，解决 Lua 不支持 C# 默认参数的问题。
public static class AutoApi
{
    // (类型组合签名) → 生成的代理类型
    private static readonly Dictionary<string, Type> s_proxyCache = new(StringComparer.Ordinal);

    // 扫描 utilityTypes 中所有 [ScriptMethod] 方法，生成代理实例
    public static object CreateProxy(params Type[] utilityTypes)
    {
        if (utilityTypes is null) throw new ArgumentNullException(nameof(utilityTypes));
        if (utilityTypes.Length == 0)
            throw new ArgumentException("At least one utility type is required.", nameof(utilityTypes));

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
        foreach (var method in utilityType.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = method.GetCustomAttribute<ScriptMethodAttribute>();
            if (attr == null) continue;

            // Name 显式指定则用指定值，否则保持 PascalCase
            var name = attr.Name ?? method.Name;
            var parameters = method.GetParameters();
            var paramTypes = new Type[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
                paramTypes[i] = parameters[i].ParameterType;

            // 签名 key，防止重复定义
            var sigKey = name + "::" + string.Join(",", paramTypes.Select(t => t.FullName));
            if (!registered.Add(sigKey)) continue;

            // 生成完整签名代理
            EmitProxyMethod(typeBuilder, name, method, paramTypes, parameters.Length);

            // 尾部可选参数 → 自动生成省略它们的重载
            GenerateOptionalOverloads(typeBuilder, name, method, parameters, paramTypes, registered);
        }

        return typeBuilder.CreateType()!;
    }

    // 为每个尾部可选参数组合生成重载：Alert(string,bool,float=0) → 额外生成 Alert(string,bool)
    private static void GenerateOptionalOverloads(
        TypeBuilder typeBuilder, string name, MethodInfo method,
        ParameterInfo[] parameters, Type[] paramTypes, HashSet<string> registered)
    {
        // 从右往左找连续的可选参数起始位置
        var firstOptional = parameters.Length;
        for (var i = parameters.Length - 1; i >= 0; i--)
            if (parameters[i].HasDefaultValue)
                firstOptional = i;
            else
                break;

        // 无可选参数或全部参数都是可选的第一个位置（已生成完整版），跳过
        if (firstOptional >= parameters.Length || (firstOptional == 0 && parameters.Length == 1))
            return;

        // 生成 (firstOptional) ~ (parameters.Length - 1) 个参数的重载
        for (var paramCount = firstOptional; paramCount < parameters.Length; paramCount++)
        {
            var overloadTypes = paramTypes.Take(paramCount).ToArray();
            var sigKey = name + "::" + string.Join(",", overloadTypes.Select(t => t.FullName));
            if (!registered.Add(sigKey)) continue;

            EmitProxyMethod(typeBuilder, name, method, overloadTypes, paramCount,
                parameters, paramCount);
        }
    }

    // 发射代理方法，optionalStart 之后的参数用默认值填充
    private static void EmitProxyMethod(
        TypeBuilder typeBuilder, string name, MethodInfo method,
        Type[] paramTypes, int overloadParamCount,
        ParameterInfo[]? originalParams = null, int optionalStart = 0)
    {
        var methodBuilder = typeBuilder.DefineMethod(
            name,
            MethodAttributes.Public,
            method.ReturnType,
            paramTypes);

        var il = methodBuilder.GetILGenerator();

        // 实例方法：arg0 = this，参数从 arg1 开始
        for (var i = 0; i < overloadParamCount; i++)
            il.Emit(OpCodes.Ldarg, i + 1);

        // 省略的可选参数 → 填入默认值
        if (originalParams != null)
            for (var i = optionalStart; i < originalParams.Length; i++)
                EmitDefaultValue(il, originalParams[i]);

        il.Emit(OpCodes.Call, method);
        il.Emit(OpCodes.Ret);
    }

    // 发射参数的默认值到 IL 栈上
    private static void EmitDefaultValue(ILGenerator il, ParameterInfo param)
    {
        var type = param.ParameterType;
        var defaultValue = param.DefaultValue;

        if (defaultValue == null || defaultValue is DBNull)
        {
            EmitTypeDefault(il, type);
            return;
        }

        // 常见值类型直接发射常量
        if (type == typeof(int))
            il.Emit(OpCodes.Ldc_I4, (int)defaultValue);
        else if (type == typeof(float))
            il.Emit(OpCodes.Ldc_R4, (float)defaultValue);
        else if (type == typeof(double))
            il.Emit(OpCodes.Ldc_R8, (double)defaultValue);
        else if (type == typeof(bool))
            il.Emit((bool)defaultValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        else if (type == typeof(string))
            il.Emit(OpCodes.Ldstr, (string)defaultValue);
        else if (type == typeof(long))
            il.Emit(OpCodes.Ldc_I8, (long)defaultValue);
        else if (type == typeof(byte) || type == typeof(sbyte) ||
                 type == typeof(short) || type == typeof(ushort))
            il.Emit(OpCodes.Ldc_I4, Convert.ToInt32(defaultValue));
        else if (type.IsEnum)
            il.Emit(OpCodes.Ldc_I4, Convert.ToInt32(defaultValue));
        else
            EmitTypeDefault(il, type);
    }

    private static void EmitTypeDefault(ILGenerator il, Type type)
    {
        if (type.IsValueType)
        {
            var local = il.DeclareLocal(type);
            il.Emit(OpCodes.Ldloca_S, local);
            il.Emit(OpCodes.Initobj, type);
            il.Emit(OpCodes.Ldloc_S, local);
        }
        else
        {
            il.Emit(OpCodes.Ldnull);
        }
    }
}