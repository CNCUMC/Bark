***English*** | [简体中文](../zh-CN/csharp-events.md)

# C# Event System

Bark's event system decouples C# mod communication while bridging to script engines. It's attribute-based with zero
manual registration.

## Subscribing to Events

Annotate your class with `[EventBusSubscriber]`, then write `public static` methods accepting the event type you want to
handle.

```csharp
using Bark.Event;
using Bark.Events;

[EventBusSubscriber(Plugin.Guid)]  // use your plugin's GUID
public static class MyEventHandlers
{
    // Signature: public static void MethodName(BarkEventSubclass param)
    public static void OnPlayerJump(PlayerJumpStartEvent evt)
    {
        LogUtil.Info($"Player jumped! Time: {evt.Time}");
    }

    public static void OnPlayerDeath(PlayerDeathEvent evt)
    {
        LogUtil.Warning("Player died");
    }
}
```

At startup, Bark scans all assemblies, finds `[EventBusSubscriber]` classes with matching methods, and registers them.
You do nothing.

> ℹ️ Method names don't matter — Bark matches by parameter type. But prefix with `On` for readability.

## Triggering Events

Three ways:

```csharp
using Bark.Event;
using Bark.Events;
using Bark.Tool;

// Way 1: Construct an instance
var evt = new LimbBrokenEvent { LimbIndex = 2, LimbName = "Left Leg" };
EventUtil.Trigger(evt);

// Way 2: Generic trigger (parameterless constructor)
EventUtil.Trigger<MainMenuLoadedEvent>();

// Way 3: Direct call on EventRegistry (skip EventUtil)
EventRegistry.Trigger(new WorldReadyEvent { World = WorldGeneration.world });
```

`EventUtil.Trigger<T>()` is for events that don't need properties set. `EventUtil.Trigger(evt)` is for events with data.

## Listening to Events from C# (Manual)

Use `EventUtil.On<T>()` for manual registration, paired with `UnregisterAll` for cleanup:

```csharp
// In mod's Awake():
EventUtil.On<PlayerDeathEvent>(evt =>
{
    LogUtil.Warning($"Player died, time: {evt.Time}");
}, Plugin.Guid);

// When unloading:
EventUtil.UnregisterAll(Plugin.Guid);
```

Difference from `[EventBusSubscriber]`: `On<T>` is manual, good for dynamic scenarios; `[EventBusSubscriber]` is
automatic, good for static handlers.

## Event Type Reference

### Player Events

| C# Type                | Properties                   | Trigger Description |
|------------------------|------------------------------|---------------------|
| `PlayerJumpStartEvent` | `Body body`, `Camera camera` | Jump key pressed    |
| `PlayerJumpOverEvent`  | `Body body`, `Camera camera` | Landed after jump   |
| `PlayerDeathEvent`     | `Body body`, `Camera camera` | Player died         |

### Body Events

Every body event carries `Body body` and `Camera camera`.

#### Vitals / Consciousness

| C# Type                        | Extra Properties       | Trigger Description            |
|--------------------------------|------------------------|--------------------------------|
| `BodyCardiacArrestEvent`       | `bool IsCardiacArrest` | Cardiac arrest / restored      |
| `BodyFibrillationStartEvent`   | —                      | Fibrillation started           |
| `BodyFibrillationEndEvent`     | —                      | Fibrillation stopped           |
| `BodyBreathChangeEvent`        | `bool IsBreathing`     | Breathing stopped / restored   |
| `BodyConsciousnessChangeEvent` | `bool IsConscious`     | Unconscious / awake            |
| `BodyBrainDyingEvent`          | `bool IsBrainDying`    | Entering / leaving brain-death |

#### Actions / Sleep / Special States

| C# Type                  | Extra Properties            | Trigger Description         |
|--------------------------|-----------------------------|-----------------------------|
| `BodyClimbStartEvent`    | —                           | Started climbing            |
| `BodyClimbEndEvent`      | —                           | Stopped climbing            |
| `BodyExerciseStartEvent` | —                           | Started exercising          |
| `BodyExerciseEndEvent`   | —                           | Stopped exercising          |
| `BodySwitchHandsEvent`   | —                           | Swapped hand items          |
| `BodySwitchDirEvent`     | `bool IsRight`              | Switched facing             |
| `BodyCrouchChangeEvent`  | `bool IsCrouching`          | Started / stopped crouching |
| `BodyPickUpEvent`        | `string ItemId`, `int Slot` | Picked up an item           |
| `BodyDropEvent`          | `string ItemId`             | Dropped an item             |
| `BodySleepChangeEvent`   | `bool IsSleeping`           | Fell asleep / woke up       |
| `BodyLastStandEvent`     | —                           | Last Stand succeeded        |
| `BodyDisfigureEvent`     | —                           | Player disfigured           |
| `BodyRemoveEyeEvent`     | `bool BothEyesGone`         | Player lost an eye          |

### Limb Events

| C# Type                 | Properties                         | Trigger Description |
|-------------------------|------------------------------------|---------------------|
| `LimbBrokenEvent`       | `int LimbIndex`, `string LimbName` | Bone fractured      |
| `LimbMendedEvent`       | `int LimbIndex`, `string LimbName` | Bone healed         |
| `LimbDislocatedEvent`   | `int LimbIndex`, `string LimbName` | Joint dislocated    |
| `LimbUnDislocatedEvent` | `int LimbIndex`, `string LimbName` | Joint relocated     |
| `LimbDismemberedEvent`  | `int LimbIndex`, `string LimbName` | Limb severed        |
| `LimbInfectedEvent`     | `int LimbIndex`, `string LimbName` | Wound infected      |

### Moodle Events

| C# Type              | Properties                                                                                     | Trigger Description      |
|----------------------|------------------------------------------------------------------------------------------------|--------------------------|
| `MoodleGetEvent`     | `string MoodleKey`, `string MoodleName`, `int Intensity`, `bool Critical`, `float HoldSeconds` | Moodle applied to player |
| `MoodleIterateEvent` | `string[] ActiveKeys`                                                                          | Polled (every 0.5s)      |
| `MoodleLoseEvent`    | `string MoodleKey`, `string MoodleName`                                                        | Moodle expired/removed   |

### World / Menu Events

| C# Type               | Properties                   | Trigger Description     |
|-----------------------|------------------------------|-------------------------|
| `MainMenuLoadedEvent` | None (only inherited `Time`) | Entered main menu       |
| `WorldReadyEvent`     | `WorldGeneration World`      | World finished spawning |

## Custom Events

Define your own event types — scripts can listen to them too.

### 1. Define the Event Class

```csharp
using Bark.Event;

namespace MyMod.Events;

// Inherit BarkEvent, add [ScriptEvent] to bridge to scripts
[ScriptEvent("onMyCustomEvent")]  // without this, only C# can listen
public class MyCustomEvent : BarkEvent
{
    public string Message { get; set; } = string.Empty;
    public int Value { get; set; }
}
```

### 2. Trigger

```csharp
// Trigger from C#
EventUtil.Trigger(new MyCustomEvent
{
    Message = "Hello from C#",
    Value = 42
});
```

### 3. Receive in Scripts

With `[ScriptEvent("onMyCustomEvent")]`, scripts just define a matching function:

```js
function onMyCustomEvent() {
    Log.Info('Received custom event from C#');
}
```

> ℹ️ Script hooks take no parameters — this is by design in `ScriptEventScanner`. To pass data, have the script call an
> API inside the hook.

## API Reference

| Method                                                         | Description                                    |
|----------------------------------------------------------------|------------------------------------------------|
| `EventUtil.Trigger(BarkEvent evt)`                             | Trigger event with data                        |
| `EventUtil.Trigger<T>()`                                       | Trigger event with default ctor                |
| `EventUtil.On<T>(Action<T>, string guid)`                      | Manual handler registration                    |
| `EventUtil.UnregisterAll(string guid)`                         | Clean up all handlers for a mod                |
| `EventRegistry.Register(Type, Action<BarkEvent>, string guid)` | Low-level registration (used by script engine) |
| `EventRegistry.Unregister(Type, string guid)`                  | Low-level unregistration                       |

## Notes

- `[EventBusSubscriber]` GUID should be your own plugin GUID — don't use `Plugin.Guid` to subscribe to someone else's
  mod
- Exceptions in handlers are caught by `EventRegistry` and logged — they won't affect other handlers
- Events bubble up the inheritance chain: subscribing to `BarkEvent` receives all events
- Call `EventUtil.UnregisterAll` in `Unload` to clean up manual registrations
