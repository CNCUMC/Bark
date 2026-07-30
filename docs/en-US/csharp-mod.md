***English*** | [简体中文](../zh-CN/csharp-mod.md)

# C# Mod Development

If you prefer C# over scripting, Bark provides a complete event system and tool APIs so you can write native C# code to
extend the game.

## Creating a Project

Use CNCUMC's [Moss Template](https://github.com/CNCUMC/Moss-Template) as a starting point — it comes pre-configured with
BepInEx + CCL references.

Write your initialization logic in `Plugin.Awake()`.

## Subscribing to Events

Annotate your class with `[EventBusSubscriber]`, then write `public static` methods that accept a `BarkEvent` subclass
parameter. Bark auto-scans and registers these at startup — no manual `+=` needed.

```csharp
using Bark.Event;
using Bark.Events;

[EventBusSubscriber("org.cncumc.bark")]
public static class MyEventHandlers
{
    public static void OnPlayerJump(PlayerJumpStartEvent evt)
    {
        Logger.LogInfo($"Player jumped, time: {evt.Time}");
    }

    public static void OnPlayerDeath(PlayerDeathEvent evt)
    {
        Logger.LogWarning("Player died");
    }
}
```

- Annotate the class with `[EventBusSubscriber("your GUID")]`
- Methods must be `public static` and accept a `BarkEvent` subclass parameter
- Method name doesn't matter — Bark matches by parameter type
- One class can handle multiple event types, one method per event

Full event type list at [C# Events Reference](csharp-events.md).

## Calling Tool APIs

All static classes under `Tool/` are callable directly. C# methods map 1:1 to script APIs — refer to the script-side
docs for details:

| Class                        | Docs                                         |
|------------------------------|----------------------------------------------|
| `BodyUtil`                   | [Body System](script-api/body-system.md)     |
| `PlayerUtil`                 | [Player](script-api/player.md)               |
| `LimbUtil`                   | [Limbs](script-api/limbs.md)                 |
| `InventoryUtil` / `ItemUtil` | [Inventory & Items](script-api/inventory.md) |
| `SkillUtil`                  | [Skills](script-api/skills.md)               |
| `WorldUtil`                  | [World Editing](script-api/world.md)         |
| `LogUtil`                    | [Logging](script-api/log.md)                 |
| `OptionsApi`                 | [Options](script-api/options.md)             |
| `Locale`                     | [Localization](script-api/locale.md)         |

```csharp
using Bark.Tool;

// Read/write player vitals
var hunger = BodyUtil.GetHunger();
BodyUtil.SetHunger(hunger + 10);

// Manipulate limbs
LimbUtil.Break(0);          // break limb #0
LimbUtil.Mend(0);           // heal limb #0

// Manipulate the world
WorldUtil.PlaceBlock("marble", 10, 5);

// Player actions
PlayerUtil.Teleport(100, 200);
PlayerUtil.Alert("You've got mail", true);
```

All methods are null-safe: if game state isn't ready (e.g., world hasn't spawned), methods silently return or log a
warning instead of crashing the game.

## Writing Harmony Patches

Bark itself is a Harmony mod — `Plugin.Awake()` already calls `_harmony.PatchAll()`, so any `[HarmonyPatch]` in your
assembly will be applied automatically.

```csharp
using HarmonyLib;

[HarmonyPatch(typeof(Body), nameof(Body.Jump))]
public static class JumpPatch
{
    // Prefix: fires before Body.Jump() executes
    public static void Prefix(Body __instance)
    {
        Logger.LogInfo($"Character {__instance.name} about to jump");
    }

    // Postfix: fires after Body.Jump() executes
    public static void Postfix(Body __instance)
    {
        Logger.LogInfo($"Character {__instance.name} jumped");
    }
}
```

> 💡 Harmony Patch + `EventUtil.Trigger()` is the recommended pattern — fire events in your patch and let other mods
> respond via the event system, rather than cramming all logic into one patch.

## Registering as a Script API (Advanced)

To expose your C# utility methods to JS/Lua scripts, you have two options.

### Using [ScriptApi] Auto-Registration (Recommended)

For pure `public static` utility classes, just annotate with `[ScriptApi]` and Bark auto-scans and registers at
startup — no `ApiRegistry.Register()` call needed.

```csharp
using Bark.ScriptApi;

[ScriptApi]
public static class MyMathTool
{
    [ScriptMethod]
    public static int Double(int value) => value * 2;

    [ScriptMethod]
    public static string Greet(string name) => $"Hello, {name}!";
}
```

Scripts access it via a camelCase global: `myMathTool.Double(5)`.

- The variable name defaults to the class name minus any `Util` suffix (e.g. `BodyUtil` → `Body`). Override with
  `[ScriptApi(Name = "MyApi")]`.
- Nothing to do in `Awake()` — Bark's `ApiRegistry.ScanAndRegister()` finds all `[ScriptApi]` classes automatically.

### Manual Registration (Compatibility)

If you need to control registration timing or do extra initialization, `ApiRegistry.Register()` still works:

```csharp
using Bark.ScriptApi;

public static class MyTool
{
    [ScriptMethod]
    public static int Double(int value) => value * 2;

    [ScriptMethod]
    public static string Greet(string name) => $"Hello, {name}!";
}

// In Awake():
ApiRegistry.Register(typeof(MyTool));
```

After registration, scripts access it via a camelCase global variable: `myTool.Double(5)`.

Optional parameters generate overloads automatically (same behavior as Bark's built-in APIs):

```csharp
[ScriptMethod]
public static void Alert(string text, bool important = false, float delay = 0f)
{
    // Script can call myTool.Alert("hello"), myTool.Alert("hello", true), myTool.Alert("hello", true, 0.5)
}
```

## Triggering Custom Events

```csharp
using Bark.Tool;

// Trigger a Bark event — all subscribers receive it
EventUtil.Trigger<PlayerJumpStartEvent>();

// Or with an instance
var evt = new PlayerJumpStartEvent { Body = someBody, Camera = someCamera };
EventUtil.Trigger(evt);
```

Detailed API at [EventUtil](csharp-api/event-util.md).

## Version Checking

```csharp
using Bark.Tool;

// Call at the end of Awake() — outputs to BepInEx console if an update is available
UpdateUtil.Check("YourGitHubUsername/Repo", "ModName", "CurrentVersion", Logger);
```

Detailed API at [UpdateUtil](csharp-api/update.md).
