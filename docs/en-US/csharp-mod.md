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
| `BodyUtil`                   | [Body System](script-api/body.md)            |
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

## Loading JSON Content

Besides registering content in code, your C# mod can define items, tiles, recipes and moodles in JSON files just like
script mods — Bark parses and registers them into the corresponding CUCoreLib registries. This lets you describe content
as pure data and reuse Bark's item template system (gun / mag / ammo / casing / clothing / food) without hand-writing
registration code.

- For config options, use [BetterCCL's BetterOptions](script-api/options.md) directly — not JSON.
- For console commands, use CCL's `ConsoleCommandRegistry` directly — not JSON.

### Directory Layout

Place content JSON and sprites under your plugin directory, following the same convention as script mods:

```
BepInEx/
  plugins/
    your_mod/                  <- mod root (the directory your DLL lives in)
      your_mod.dll
      mod.json                 <- mod manifest (at least an id)
      Item/                    <- item JSON
        my_rifle.json
      Tile/                    <- tile JSON
        marble.json
      Recipe/                  <- recipe JSON
        ripping.json
      Moodle/                  <- moodle JSON
        thirsty.json
      Assets/
        Item/                  <- item sprites
        Tile/                  <- tile sprites
        Moodle/                <- moodle icons
```

- `Item/*.json`: one file per item. The item ID is `{modId}.{fileName}`.
- `Tile/*.json`: one file per tile. The tile ID is the file name (no modId prefix).
- `Recipe/*.json`: one file per recipe.
- `Moodle/*.json`: one file per moodle.
- `Assets/{Item|Tile|Moodle}/*.png`: sprites, named by the same rules as script mods.

### mod.json

Place a `mod.json` in your mod root (the directory your DLL lives in). It must at least contain an `id` field — this is
the namespace prefix for item IDs, and is also used to track all content registered by the mod (for hot-reload cleanup).
Use snake_case for `id` (e.g. `your_mod`). Other fields (`name`, `version`, ...) are optional metadata — the C# side
only reads `id`.

```json
{
  "id": "your_mod",
  "name": "My C# Mod",
  "version": "1.0.0"
}
```

### Usage

Call `ModContentApi` from `Plugin.Awake()`. The mod id comes from `mod.json`, so you don't hard-code it in code.

```csharp
using Bark.Items;

public class MyPlugin : BaseUnityPlugin
{
    public void Awake()
    {
        // Option 1 (recommended): auto-find mod.json in the DLL directory, loads Item/Tile/Recipe/Moodle
        var result = ModContentApi.LoadFromPluginDirectory(GetType().Assembly.Location);
        Logger.LogInfo($"Loaded {result.Items} item(s) / {result.Tiles} tile(s) / {result.Recipes} recipe(s) / {result.Moodles} moodle(s)");

        // Option 2: pass the mod.json path explicitly
        ModContentApi.LoadFromManifest(Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, "mod.json"));

        // Option 3: locate mod.json from BepInEx/plugins/{modName}
        ModContentApi.LoadFromPlugins("your_mod");
    }
}
```

If you don't want a `mod.json`, you can pass `modId` and the root directory directly (low-level API):

```csharp
ModContentApi.Load("your_mod", Path.GetDirectoryName(GetType().Assembly.Location)!);
```

### Hot Reload / Unload

To support hot reload or mod unload, call `Unload` to clear all previously registered content, then load again:

```csharp
ModContentApi.Unload("your_mod");
ModContentApi.LoadFromPluginDirectory(GetType().Assembly.Location);
```

### Notes

- The JSON format and template usage for each content type are **identical** to script mods — see the corresponding
  docs:
  [items](script-mod/item.md) / [item templates](script-mod/item-template/index.md) / [tiles](script-mod/tile.md) /
  [recipes](script-mod/recipe.md) / [moodles](script-mod/moodle.md).
- Do **not** include a `script` field in your JSON — C# mods have no script engine, so item/tile/moodle script bindings
  are ignored with a warning. Implement item behavior in C# via `[EventBusSubscriber]` + `[HarmonyPatch]` instead.
- Sprites are loaded from `Assets/Item/`, falling back to the `origin_prefab`'s vanilla sprite when missing.
