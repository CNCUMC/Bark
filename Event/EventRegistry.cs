using System;
using System.Collections.Generic;
using System.Reflection;
using Bark.Tool;
using UnityEngine;

namespace Bark.Event;

public static class EventRegistry
{
    private static readonly Dictionary<Type, List<EventHandler>> Handlers = new();

    public static IReadOnlyDictionary<Type, List<EventHandler>> AllHandlers => Handlers;

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
                    var subscriberAttr =
                        type.GetCustomAttribute<EventBusSubscriberAttribute>();
                    if (subscriberAttr == null) continue;

                    var typeName = type.FullName ?? type.Name;

                    foreach (var method in type.GetMethods(BindingFlags.Public
                        | BindingFlags.Static))
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length != 1
                            || !typeof(BarkEvent).IsAssignableFrom(parameters[0]
                                .ParameterType))
                        {
                            LogUtil.Warning("event.invalid_handler", typeName,
                                method.Name);
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
            }

        LogUtil.Info("event.scanned", subscribedMethods.ToString());
    }

    public static void Trigger(BarkEvent evt)
    {
        var eventType = evt.GetType();

        evt.Time = Time.time;

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

    public static void Register(Type eventType, Action<BarkEvent> callback,
        string guid = "bark.script")
    {
        if (!Handlers.ContainsKey(eventType))
            Handlers[eventType] = [];

        Handlers[eventType].Add(new EventHandler(guid, callback));
    }

    public static void Unregister(Type eventType, string guid)
    {
        if (!Handlers.TryGetValue(eventType, out var handler)) return;
        handler.RemoveAll(h => h.Guid == guid);
    }

    public static void UnregisterAll(string guid)
    {
        foreach (var handlers in Handlers.Values)
            handlers.RemoveAll(h => h.Guid == guid);
    }
}

public class EventHandler
{
    public EventHandler(string guid, Type classType, MethodInfo method)
    {
        Guid = guid;
        ClassType = classType;
        Method = method;
        Callback = null;
    }

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

    public void Invoke(BarkEvent evt)
    {
        if (Callback != null)
            Callback(evt);
        else
            Method.Invoke(null, [evt]);
    }
}
