using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Bark.ScriptApi;

public static class AutoApi
{
    private static readonly Dictionary<string, Type> s_proxyCache =
        new(StringComparer.Ordinal);

    public static object CreateProxy(params Type[] utilityTypes)
    {
        if (utilityTypes is null) throw new ArgumentNullException(nameof(utilityTypes));
        if (utilityTypes.Length == 0)
            throw new ArgumentException("At least one utility type is required.",
                nameof(utilityTypes));

        var key = string.Join("|", utilityTypes.Select(t => t.FullName));
        if (s_proxyCache.TryGetValue(key, out var proxyType))
            return Activator.CreateInstance(proxyType)!;
        proxyType = BuildProxyType(utilityTypes);
        s_proxyCache[key] = proxyType;

        return Activator.CreateInstance(proxyType)!;
    }

    private static Type BuildProxyType(Type[] utilityTypes)
    {
        var assemblyName = new AssemblyName($"AutoApi_{Guid.NewGuid():N}");
        var assembly =
            AssemblyBuilder.DefineDynamicAssembly(assemblyName,
                AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("ProxyModule");
        var typeBuilder = module.DefineType("ScriptApiProxy",
            TypeAttributes.Public | TypeAttributes.Class);

        var registered = new HashSet<string>(StringComparer.Ordinal);

        foreach (var utilityType in utilityTypes)
            foreach (var method in utilityType.GetMethods(BindingFlags.Public
                | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<ScriptMethodAttribute>();
                if (attr == null) continue;

                var name = attr.Name ?? method.Name;
                var parameters = method.GetParameters();
                var paramTypes = new Type[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                    paramTypes[i] = parameters[i].ParameterType;

                var sigKey = name
                    + "::"
                    + string.Join(",", paramTypes.Select(t => t.FullName));
                if (!registered.Add(sigKey)) continue;

                EmitProxyMethod(typeBuilder, name, method, paramTypes, parameters.Length);

                GenerateOptionalOverloads(typeBuilder, name, method, parameters,
                    paramTypes, registered);
            }

        return typeBuilder.CreateType()!;
    }

    private static void GenerateOptionalOverloads(
        TypeBuilder typeBuilder,
        string name,
        MethodInfo method,
        ParameterInfo[] parameters,
        Type[] paramTypes,
        HashSet<string> registered)
    {
        var firstOptional = parameters.Length;
        for (var i = parameters.Length - 1; i >= 0; i--)
            if (parameters[i].HasDefaultValue)
                firstOptional = i;
            else
                break;

        if (firstOptional >= parameters.Length
            || (firstOptional == 0 && parameters.Length == 1))
            return;

        for (var paramCount = firstOptional; paramCount < parameters.Length; paramCount++)
        {
            var overloadTypes = paramTypes.Take(paramCount).ToArray();
            var sigKey = name
                + "::"
                + string.Join(",", overloadTypes.Select(t => t.FullName));
            if (!registered.Add(sigKey)) continue;

            EmitProxyMethod(typeBuilder, name, method, overloadTypes, paramCount,
                parameters, paramCount);
        }
    }

    private static void EmitProxyMethod(
        TypeBuilder typeBuilder,
        string name,
        MethodInfo method,
        Type[] paramTypes,
        int overloadParamCount,
        ParameterInfo[]? originalParams = null,
        int optionalStart = 0)
    {
        var methodBuilder = typeBuilder.DefineMethod(
            name,
            MethodAttributes.Public,
            method.ReturnType,
            paramTypes);

        var il = methodBuilder.GetILGenerator();

        for (var i = 0; i < overloadParamCount; i++)
            il.Emit(OpCodes.Ldarg, i + 1);

        if (originalParams != null)
            for (var i = optionalStart; i < originalParams.Length; i++)
                EmitDefaultValue(il, originalParams[i]);

        il.Emit(OpCodes.Call, method);
        il.Emit(OpCodes.Ret);
    }

    private static void EmitDefaultValue(ILGenerator il, ParameterInfo param)
    {
        var type = param.ParameterType;
        var defaultValue = param.DefaultValue;

        if (defaultValue is null or DBNull)
        {
            EmitTypeDefault(il, type);
            return;
        }

        if (type == typeof(int))
            il.Emit(OpCodes.Ldc_I4, (int)defaultValue);
        else if (type == typeof(float))
            il.Emit(OpCodes.Ldc_R4, (float)defaultValue);
        else if (type == typeof(double))
            il.Emit(OpCodes.Ldc_R8, (double)defaultValue);
        else if (type == typeof(bool))
            il.Emit((bool)defaultValue
                ? OpCodes.Ldc_I4_1
                : OpCodes.Ldc_I4_0);
        else if (type == typeof(string))
            il.Emit(OpCodes.Ldstr, (string)defaultValue);
        else if (type == typeof(long))
            il.Emit(OpCodes.Ldc_I8, (long)defaultValue);
        else if (type == typeof(byte)
            || type == typeof(sbyte)
            || type == typeof(short)
            || type == typeof(ushort)
            || type.IsEnum)
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
