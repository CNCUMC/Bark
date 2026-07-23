using System;
using System.Collections.Generic;
using Bark.Event;
using Bark.Tool;

namespace Bark.ScriptApi;

// 脚本侧事件 API（通过 bark.events 访问）
public class EventApi
{
    private readonly Dictionary<string, Type> _eventTypeMap = new();
    private readonly string _modId;

    public EventApi(string modId)
    {
        _modId = modId;
        RegisterEventTypes();
    }

    // 注册事件类型映射（脚本名 → C# 类型）
    private void RegisterEventTypes()
    {
        _eventTypeMap["world_generated"] = typeof(WorldEvents.GeneratedEvent);
        _eventTypeMap["player_jump"] = typeof(PlayerEvents.JumpEvent);
        _eventTypeMap["player_death"] = typeof(PlayerEvents.DeathEvent);
        _eventTypeMap["player_respawn"] = typeof(PlayerEvents.RespawnEvent);
    }

    // 注册事件处理器
    // bark.events.On("world_generated", callback)
    public void On(string eventName, Action callback)
    {
        if (!_eventTypeMap.TryGetValue(eventName, out var eventType))
        {
            LogUtil.Warning("event.unknown_event", eventName);
            return;
        }

        EventRegistry.Register(eventType, _ => callback(), _modId);
    }

    // 注册带事件参数的处理器
    public void On(string eventName, Action<Event.Event> callback)
    {
        if (!_eventTypeMap.TryGetValue(eventName, out var eventType))
        {
            LogUtil.Warning("event.unknown_event", eventName);
            return;
        }

        EventRegistry.Register(eventType, callback, _modId);
    }

    // 注销当前模组的所有事件处理器
    public void UnregisterAll()
    {
        EventRegistry.UnregisterAll(_modId);
    }

    // 触发事件（脚本侧主动触发）
    public void Trigger(string eventName)
    {
        if (!_eventTypeMap.TryGetValue(eventName, out var eventType))
        {
            LogUtil.Warning("event.unknown_event", eventName);
            return;
        }

        var evt = (Event.Event)Activator.CreateInstance(eventType)!;
        EventRegistry.Trigger(evt);
    }
}