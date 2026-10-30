using System;
using Bark.Event;

namespace Bark.Tool;

public static class EventUtil
{
    public static void Trigger<T>() where T : BarkEvent, new()
    {
        EventRegistry.Trigger(new T());
    }

    public static void Trigger(BarkEvent evt)
    {
        EventRegistry.Trigger(evt);
    }

    public static void On<T>(Action<T> callback, string guid) where T : BarkEvent
    {
        EventRegistry.Register(typeof(T), evt => callback((T)evt), guid);
    }

    public static void UnregisterAll(string guid)
    {
        EventRegistry.UnregisterAll(guid);
    }
}
