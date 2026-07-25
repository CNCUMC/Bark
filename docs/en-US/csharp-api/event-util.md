***English*** | [简体中文](../../zh-CN/csharp-api/event-util.md)

# EventUtil

EventUtil is the C#-side event dispatch and subscription tool. All Bark mods (including Bark itself) use it to trigger
events and register listeners.

> ℹ️ This is a C#-only API. Scripts use [event hooks](../script-events.md) to receive events.

## Triggering Events

Two modes: generic trigger (parameterless events) or instance trigger (events with data).

```csharp
using Bark.Tool;
using Bark.Events;

// Parameterless: generic trigger
EventUtil.Trigger<PlayerJumpStartEvent>();

// With data: construct an instance first
var evt = new LimbBrokenEvent
{
    LimbIndex = 0,
    LimbName = "Left Foot"
};
EventUtil.Trigger(evt);
```

`Trigger(new T())` internally sets `Time` to the current game time.

## Registering Listeners

`On<T>` registers a delegate callback. Pass your mod GUID for later unregistration.

```csharp
using Bark.Tool;
using Bark.Events;

// In Awake():
EventUtil.On<PlayerDeathEvent>(evt =>
{
    Logger.LogWarning($"Player died, time: {evt.Time}");
    // your mod logic
}, "org.example.my_mod");
```

You can register multiple different event type listeners under the same GUID.

## Unregistering

Clean up all listeners on mod unload to prevent memory leaks:

```csharp
// In OnDestroy():
EventUtil.UnregisterAll("org.example.my_mod");
```

## Custom Events

You can define your own `BarkEvent` subclasses and dispatch them with `EventUtil.Trigger`.

```csharp
using Bark.Event;  // BarkEvent base class

// Define
public class MyCustomEvent : BarkEvent
{
    public string Message;
    public int Value;
}

// Dispatch
EventUtil.Trigger(new MyCustomEvent { Message = "hello", Value = 42 });

// Listen from another mod
EventUtil.On<MyCustomEvent>(evt =>
{
    Logger.LogInfo($"Got custom event: {evt.Message}, {evt.Value}");
}, "org.example.another_mod");
```

If you want script mods to also receive your custom event, add `[ScriptEvent("hookName")]`.
See [Event System](../csharp-events.md) for details.
