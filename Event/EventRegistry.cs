using System;
using System.Collections.Generic;
using System.Reflection;
using Bark.Tool;
using UnityEngine;

namespace Bark.Event;

// 事件注册表：扫描程序集、注册处理器、触发事件
public static class EventRegistry
{
    // 事件类型 → 处理器列表
    private static readonly Dictionary<Type, List<EventHandler>> Handlers = new();

    // 已注册的处理器（用于调试）
    public static IReadOnlyDictionary<Type, List<EventHandler>> AllHandlers => Handlers;

    // 扫描所有已加载程序集，注册标注了 [EventBusSubscriber] 的类中参数为 BarkEvent 子类的静态方法
    public static void ScanAndRegister()
    {
        Handlers.Clear();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var subscribedMethods = 0;

        foreach (var assembly in assemblies)
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    // 查找 [EventBusSubscriber] 类
                    var subscriberAttr = type.GetCustomAttribute<EventBusSubscriberAttribute>();
                    if (subscriberAttr == null) continue;

                    var typeName = type.FullName ?? type.Name;

                    // 查找 public static 方法，参数为 BarkEvent 子类即自动视为事件处理器
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        // 验证方法签名：必须有一个参数，且参数类型是 BarkEvent 的子类
                        var parameters = method.GetParameters();
                        if (parameters.Length != 1 ||
                            !typeof(BarkEvent).IsAssignableFrom(parameters[0].ParameterType))
                        {
                            LogUtil.Warning("event.invalid_handler", typeName, method.Name);
                            continue;
                        }

                        var eventType = parameters[0].ParameterType;
                        var handler = new EventHandler(subscriberAttr.Guid, type, method);

                        if (!Handlers.ContainsKey(eventType))
                            Handlers[eventType] = [];

                        Handlers[eventType].Add(handler);
                        subscribedMethods++;
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // 跳过无法加载的程序集
            }

        LogUtil.Info("event.scanned", subscribedMethods.ToString());
    }

    // 触发事件
    public static void Trigger(BarkEvent evt)
    {
        var eventType = evt.GetType();

        // 设置触发时间
        evt.Time = Time.time;

        // 查找匹配的处理器（包括父类事件的处理器）
        var currentType = eventType;
        while (currentType != null && currentType != typeof(object))
        {
            if (Handlers.TryGetValue(currentType, out var handlers))
                foreach (var handler in handlers)
                    try
                    {
                        handler.Invoke(evt);
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Warning("event.handler_failed",
                            handler.ClassFullName, handler.Method.Name,
                            ex.InnerException?.Message ?? ex.Message);
                    }

            currentType = currentType.BaseType;
        }
    }

    // 手动注册处理器（用于脚本模组或动态注册）
    public static void Register(Type eventType, Action<BarkEvent> callback, string guid = "bark.script")
    {
        if (!Handlers.ContainsKey(eventType))
            Handlers[eventType] = [];

        Handlers[eventType].Add(new EventHandler(guid, callback));
    }

    // 注销处理器
    public static void Unregister(Type eventType, string guid)
    {
        if (!Handlers.TryGetValue(eventType, out var handler)) return;
        handler.RemoveAll(h => h.Guid == guid);
    }

    // 注销某个模组的所有处理器
    public static void UnregisterAll(string guid)
    {
        foreach (var handlers in Handlers.Values)
            handlers.RemoveAll(h => h.Guid == guid);
    }
}

// 事件处理器信息
public class EventHandler
{
    // 反射注册（C# 模组）
    public EventHandler(string guid, Type classType, MethodInfo method)
    {
        Guid = guid;
        ClassType = classType;
        Method = method;
        Callback = null;
    }

    // 委托注册（脚本模组或动态注册）
    public EventHandler(string guid, Action<BarkEvent> callback)
    {
        Guid = guid;
        ClassType = null!;
        Method = null!;
        Callback = callback;
    }

    public string Guid { get; }
    public Type ClassType { get; }
    public MethodInfo Method { get; }
    public Action<BarkEvent>? Callback { get; }

    public string ClassFullName => ClassType.FullName ?? "delegate";

    // 执行处理器
    public void Invoke(BarkEvent evt)
    {
        if (Callback != null)
            Callback(evt);
        else
            Method.Invoke(null, [evt]);
    }
}