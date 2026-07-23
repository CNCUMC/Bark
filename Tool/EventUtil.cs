using System;
using Bark.Event;

namespace Bark.Tool;

// 静态事件 API，C# 模组和脚本模组共用
public static class EventUtil
{
    // 触发事件
    public static void Trigger<T>() where T : Event.BarkEvent, new()
    {
        EventRegistry.Trigger(new T());
    }

    public static void Trigger(Event.BarkEvent evt)
    {
        EventRegistry.Trigger(evt);
    }

    // 注册事件处理器
    public static void On<T>(Action<T> callback, string guid) where T : Event.BarkEvent
    {
        EventRegistry.Register(typeof(T), evt => callback((T)evt), guid);
    }

    // 注销某个模组的所有处理器
    public static void UnregisterAll(string guid)
    {
        EventRegistry.UnregisterAll(guid);
    }
}