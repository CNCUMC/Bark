![alt text](Covor.png)

[中文指南](README_ZH.md)

# Moss Lib

[GitHub](https://github.com/Black-Moss/Moss-Lib) / [Nexus Mods](https://www.nexusmods.com/scavprototype/mods/8)

_Just a simple library for [Black_Moss](https://github.com/Black-Moss). :)_

---

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Localization System](#localization-system)
- [Command System](#command-system)
- [Language Generator](#language-generator)
- [Tools Reference](#tools-reference)
    - [Log](#log)
    - [GameConsole](#gameconsole)
    - [World](#world)
    - [Player](#player)
    - [Key](#key)
    - [Multiplayer](#multiplayer)
    - [Config](#config)
    - [RichText](#richtext)
    - [Tools (Utils)](#tools-utils)
- [Constants Reference](#constants-reference)
    - [Blocks](#blocks)
    - [Items](#items)
    - [Backgrounds](#backgrounds)
    - [Keys](#keys-1)
- [UI Toolkit (WIP)](#ui-toolkit-wip)

---

## Overview

**Moss Lib** is a BepInEx plugin library for **Casualties Unknown** (and its demo), providing a set of reusable base
classes and utility tools to simplify mod development. It includes:

| Module                                     | Description                                                                      |
|--------------------------------------------|----------------------------------------------------------------------------------|
| [`ModLocaleBase`](Base/ModLocaleBase.cs)   | Multi-language localization system with JSON-based language files                |
| [`ModCommandBase`](Base/ModCommandBase.cs) | Base class for registering custom in-game console commands                       |
| [`ModLangGenBase`](Base/ModLangGenBase.cs) | Language file generator that produces JSON locale files from code                |
| [`Log`](Tool/Log.cs)                       | Advanced in-game console logging utilities                                       |
| [`GameConsole`](Tool/Console.cs)           | Wrapper to execute game console commands programmatically                        |
| [`World`](Tool/World.cs)                   | World manipulation: place blocks, items, and background tiles                    |
| [`Player`](Tool/Player.cs)                 | Player manipulation: teleport, alerts, inventory management                      |
| [`Key`](Tool/Key.cs)                       | Input handling: key binding checks, mouse click waiting, world-space coordinates |
| [`Multiplayer`](Tool/Multiplayer.cs)       | Multiplayer support with reflection-based KrokoshaCasualtiesMP integration       |
| [`Config`](Tool/Config.cs)                 | BepInEx configuration entry toggling helpers                                     |
| [`RichText`](Tool/RichText.cs)             | Unity rich text formatting: color, alpha, bold, italic, size                     |
| [`Tools`](Tool/Tools.cs)                   | Argument validation, float/int parsing utilities                                 |
| [`Blocks`](Constant/Blocks.cs)             | Strongly-typed block definitions with properties                                 |
| [`Items`](Constant/Items.cs)               | Strongly-typed item definitions with properties                                  |
| [`Backgrounds`](Constant/Backgrounds.cs)   | Background ID string constants                                                   |
| [`Keys`](Constant/Keys.cs)                 | Strongly-typed key action constants                                              |

---

## Installation

1. Install [BepInEx 5.x](https://github.com/BepInEx/BepInEx) for Casualties Unknown.
2. Download the latest [`MossLib.dll`](https://github.com/Black-Moss/Moss-Lib/releases) from the Releases page
   (or [Nexus Mods](https://www.nexusmods.com/scavprototype/mods/8)).
3. Place `MossLib.dll` into your `BepInEx/plugins/` folder.
4. (Optional) For multiplayer features, install **KrokoshaCasualtiesMP** as a soft dependency.

> **For mod developers:** Add a reference to `MossLib.dll` in your project, and add
`BepInDependency("blackmoss.mosslib")` to your plugin class.

---

## Quick Start

### 1. Add BepInEx Dependency

```csharp
[BepInPlugin(Guid, Name, Version)]
[BepInDependency("blackmoss.mosslib")]  // Add this line
public class MyPlugin : BaseUnityPlugin
{
    // ...
}
```

### 2. Set Up Localization

```csharp
// Create a locale class (singleton wrapper)
public class MyLocale : ModLocaleBase
{
    private static MyLocale _instance;

    public static void Initialize(ManualLogSource logger)
    {
        _instance = new MyLocale();
        _instance.Initialize(logger, Assembly.GetExecutingAssembly());
    }

    public static string Get(string key) => _instance?.GetString(key) ?? $"[{key}]";
    public static string GetFormat(string key, params object[] args) => _instance?.GetStringFormatted(key, args) ?? $"[{key}]";
}
```

### 3. Register a Custom Command

```csharp
[HarmonyPatch(typeof(ConsoleScript))]
public class MyCommand : ModCommandBase
{
    [HarmonyPatch("RegisterAllCommands")]
    [HarmonyPostfix]
    public static void RegisterCustomCommands(ConsoleScript __instance)
    {
        ConsoleScript.Commands.Add(new Command(
            "mycommand",
            "Description of my command",
            _ => Log.Info("Command executed!", Plugin.Logger),
            null
        ));
    }
}
```

---

## Localization System

The localization system loads JSON language files from the `Lang/` folder inside your plugin directory.

### File Structure

```
BepInEx/plugins/YourPlugin/
├── Lang/
│   ├── EN.json        # English (fallback)
│   ├── zh-CN.json     # Simplified Chinese
│   └── zh-TW.json     # Traditional Chinese
└── YourPlugin.dll
```

### JSON Format

Language files use nested JSON keys with dot notation for organization:

```json
{
  "welcome": "Welcome!",
  "command": {
    "mycommand": {
      "description": "My custom command",
      "text": "Hello {0}!"
    }
  },
  "tool": {
    "player": {
      "bodynull": "Player body is null"
    }
  }
}
```

### API Methods

| Method                                                      | Description                                                              |
|-------------------------------------------------------------|--------------------------------------------------------------------------|
| [`GetString(key)`](Base/ModLocaleBase.cs)                   | Get localized string by key; falls back to English, then returns `[key]` |
| [`GetStringFormatted(key, args...)`](Base/ModLocaleBase.cs) | Get localized string and apply `string.Format()` with arguments          |
| [`GetStringArray(key)`](Base/ModLocaleBase.cs)              | Get a localized string array (JSON array)                                |
| [`GetStringDictionary(key)`](Base/ModLocaleBase.cs)         | Get a localized dictionary (JSON object)                                 |
| [`HasKey(key)`](Base/ModLocaleBase.cs)                      | Check if a translation key exists                                        |

### Example

See [`Example/ModLocale.cs`](Example/ModLocale.cs) for a complete singleton implementation.

```csharp
// Initialize in plugin Awake()
MyLocale.Initialize(Logger);

// Usage
string welcome = MyLocale.Get("welcome");                          // "Welcome!"
string message = MyLocale.GetFormat("command.mycommand.text", "World"); // "Hello World!"
```

---

## Command System

The command system allows you to register custom console commands via Harmony patching.

### Base Class: [`ModCommandBase`](Base/ModCommandBase.cs)

| Member                                                                          | Description                                                                     |
|---------------------------------------------------------------------------------|---------------------------------------------------------------------------------|
| [`Initialize(logger, assembly, harmony?)`](Base/ModCommandBase.cs)              | Initialize with logger and plugin assembly; optionally provide Harmony instance |
| [`LogToConsole(text)`](Base/ModCommandBase.cs)                                  | Log text to the in-game console                                                 |
| [`ApplicationLogCallback(condition, stackTrace, type)`](Base/ModCommandBase.cs) | Callback for Unity log messages with color-coded console output                 |
| [`Logger`](Base/ModCommandBase.cs)                                              | Protected `ManualLogSource` property for subclasses                             |

### Example

```csharp
[HarmonyPatch(typeof(ConsoleScript))]
public class MyCommand : ModCommandBase
{
    [HarmonyPatch("RegisterAllCommands")]
    [HarmonyPostfix]
    public static void RegisterCustomCommands(ConsoleScript __instance)
    {
        ConsoleScript.Commands.Add(new Command(
            "hello",
            "Says hello",
            args => {
                string name = args.Length > 1 ? args[1] : "World";
                LogToConsole($"Hello, {name}!");
            },
            null
        ));
    }
}
```

See [`Example/ModCommand.cs`](Example/ModCommand.cs) for a complete example.

---

## Language Generator

The language generator auto-creates JSON language files from C# code, eliminating the need to manually write and
maintain locale JSON files.

### Base Class: [`ModLangGenBase`](Base/ModLangGenBase.cs)

| Member                                                 | Description                                                                             |
|--------------------------------------------------------|-----------------------------------------------------------------------------------------|
| [`LanguageCode`](Base/ModLangGenBase.cs)               | Abstract property; return the language code (e.g., `"EN"`, `"zh-CN"`)                   |
| [`BuildLocaleData()`](Base/ModLangGenBase.cs)          | Abstract method; call `Add()` to register translations                                  |
| [`Add(key, value)`](Base/ModLangGenBase.cs)            | Add a translation entry                                                                 |
| [`Generate(outputDirectory?)`](Base/ModLangGenBase.cs) | Generate the JSON file; merges with existing files (doesn't overwrite existing entries) |
| [`Count`](Base/ModLangGenBase.cs)                      | Number of registered translation entries                                                |

### Creating a Generator

```csharp
public class EnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "EN";

    protected override void BuildLocaleData()
    {
        Add("welcome", "Welcome!");
        Add("command.hello.description", "Says hello");
        Add("command.hello.text", "Hello, {0}!");
    }
}
```

### Registration & Generation

Use [`LocaleGenerator`](Tool/LocaleGenerator.cs) to register and generate all language files:

| Method                                                                      | Description                                |
|-----------------------------------------------------------------------------|--------------------------------------------|
| [`SetLogger(logger)`](Tool/LocaleGenerator.cs)                              | Set the logger                             |
| [`Register(generator, logger)`](Tool/LocaleGenerator.cs)                    | Register a language generator              |
| [`GenerateAll(outputDirectory?)`](Tool/LocaleGenerator.cs)                  | Generate all registered language files     |
| [`GenerateSingle(languageCode, outputDirectory?)`](Tool/LocaleGenerator.cs) | Generate a single language file            |
| [`PrintInfo()`](Tool/LocaleGenerator.cs)                                    | Print info about all registered generators |

```csharp
// In plugin Awake()
LocaleGenerator.SetLogger(Logger);
LocaleGenerator.Register(new EnLangGenerator(), Logger);
LocaleGenerator.Register(new ZhCnLangGenerator(), Logger);
LocaleGenerator.GenerateAll(); // Creates EN.json, zh-CN.json in Lang/ folder
```

> **Note:** The generator **merges** with existing JSON files — it only adds **new** entries, preserving any user
> modifications to existing translations.

---

## Tools Reference

### Log

[`Log`](Tool/Log.cs) — Enhanced in-game console logging with BepInEx integration.

| Method                                                  | Description                                           |
|---------------------------------------------------------|-------------------------------------------------------|
| [`LogToConsole(text)`](Tool/Log.cs)                     | Write a message to the in-game console with timestamp |
| [`Info(text, logger)`](Tool/Log.cs)                     | Log to both in-game console and BepInEx log           |
| [`Debug(text, logger)`](Tool/Log.cs)                    | Log debug message with `[DEBUG]` prefix               |
| [`Error(text, logger)`](Tool/Log.cs)                    | Log error message with `[ERROR]` prefix               |
| [`Warning(text, logger)`](Tool/Log.cs)                  | Log warning message with `[WARNING]` prefix           |
| [`Alert(text, logger, important, delay?)`](Tool/Log.cs) | Log and show a player alert (screen notification)     |
| [`NewLine()`](Tool/Log.cs)                              | Insert a blank line in the console                    |
| [`Divider(char?, length?)`](Tool/Log.cs)                | Insert a divider line                                 |

### GameConsole

[`GameConsole`](Tool/Console.cs) — Execute game console commands programmatically.

```csharp
// Execute any in-game console command
GameConsole.RunCommand("some_command arg1 arg2");
```

### World

[`World`](Tool/World.cs) — World manipulation tools.

| Method                                                | Description                                  |
|-------------------------------------------------------|----------------------------------------------|
| [`PlaceBlock(x, y, block)`](Tool/World.cs)            | Place a block at tile coordinates            |
| [`PlaceBlock(vector2, block)`](Tool/World.cs)         | Place a block at a Vector2 position          |
| [`PlaceItem(x, y, item)`](Tool/World.cs)              | Spawn an item at tile coordinates            |
| [`PlaceItem(vector2, item)`](Tool/World.cs)           | Spawn an item at a Vector2 position          |
| [`PlaceBackground(pos, backgroundId)`](Tool/World.cs) | Place a background tile (accepts Vector2)    |
| [`PlaceBackground(pos, backgroundId)`](Tool/World.cs) | Place a background tile (accepts Vector2Int) |
| [`CreateTileMesh(pos)`](Tool/World.cs)                | Create a tiled mesh for background rendering |
| [`CheckForWorld()`](Tool/World.cs)                    | Throw if no world is loaded                  |
| [`ClearCache()`](Tool/World.cs)                       | Clear sprite/mesh/material caches            |

### Player

[`Player`](Tool/Player.cs) — Player manipulation tools.

| Method                                            | Description                                                 |
|---------------------------------------------------|-------------------------------------------------------------|
| [`Alert(text, important)`](Tool/Player.cs)        | Show a screen alert to the player                           |
| [`Alert(text, important, delay)`](Tool/Player.cs) | Show a delayed screen alert                                 |
| [`Tp(vector2)`](Tool/Player.cs)                   | Teleport the player (works in singleplayer and multiplayer) |
| [`Tp(x, y)`](Tool/Player.cs)                      | Teleport to float coordinates                               |
| [`PickItem(item, slot, force?)`](Tool/Player.cs)  | Give an item to a specific inventory slot                   |

```csharp
// Examples
Player.Alert("Watch out!", true);           // Important alert
Player.Tp(100.5f, 200.3f);                  // Teleport
Player.PickItem("rifle", 0, true);          // Give rifle to slot 0
```

### Key

[`Key`](Tool/Key.cs) — Input handling utilities.

| Method                                | Description                                                |
|---------------------------------------|------------------------------------------------------------|
| [`HasKey(action)`](Tool/Key.cs)       | Check if a key binding action name exists                  |
| [`IsKey(action)`](Tool/Key.cs)        | Check if a key is currently held down                      |
| [`IsKeyDown(action)`](Tool/Key.cs)    | Check if a key was pressed this frame                      |
| [`IsKeyUp(action)`](Tool/Key.cs)      | Check if a key was released this frame                     |
| [`MouseWorldPosition()`](Tool/Key.cs) | Get the mouse position in world coordinates                |
| [`LeftClickPosition()`](Tool/Key.cs)  | Get left click world position (only valid on click frame)  |
| [`RightClickPosition()`](Tool/Key.cs) | Get right click world position (only valid on click frame) |

**Coroutine-based waiting methods** — These wait until the player clicks, then return the world position:

| Method                                              | Description                                                    |
|-----------------------------------------------------|----------------------------------------------------------------|
| [`WaitForLeftClick(Action<Vector2>)`](Tool/Key.cs)  | Coroutine: wait for left click, invoke callback with position  |
| [`WaitForRightClick(Action<Vector2>)`](Tool/Key.cs) | Coroutine: wait for right click, invoke callback with position |
| [`WaitForLeftClick()`](Tool/Key.cs)                 | Coroutine: wait for left click, returns `WaitForClickResult`   |
| [`WaitForRightClick()`](Tool/Key.cs)                | Coroutine: wait for right click, returns `WaitForClickResult`  |

**`WaitForClickResult`** — Inherits from
[`CustomYieldInstruction`](https://docs.unity3d.com/ScriptReference/CustomYieldInstruction.html),
supports `yield return` waiting; use `.Result` to get the click position in world coordinates after waiting completes.

**Key action constants** are defined in [`Key.InputAction`](Tool/Key.cs):

| Constant                 | Value            | Description              |
|--------------------------|------------------|--------------------------|
| `InputAction.LeftClick`  | `"attack"`       | Left mouse click action  |
| `InputAction.RightClick` | `"iteminteract"` | Right mouse click action |

```csharp
// Check if left click is pressed this frame
if (Key.IsKeyDown(Key.InputAction.LeftClick))
{
    Vector2 pos = Key.MouseWorldPosition();
    // Do something...
}

// ── Coroutine: wait for player to click ──
IEnumerator MyCoroutine()
{
    // Pattern 1: Callback-based
    yield return Key.WaitForLeftClick(pos => {
        Player.Tp(pos);
    });

    // Pattern 2: Result-based (cleaner)
    var waiter = Key.WaitForRightClick();
    yield return waiter;
    Player.Tp(waiter.Result);
}
```

For a full list of key action names, see [`Constant/Keys`](Constant/Keys.cs).

### Multiplayer

[`Multiplayer`](Tool/Multiplayer.cs) — Multiplayer integration via KrokoshaCasualtiesMP.

| Member                                           | Description                                             |
|--------------------------------------------------|---------------------------------------------------------|
| [`IsNetworkRunning`](Tool/Multiplayer.cs)        | Check if the multiplayer network is active              |
| [`IsClient`](Tool/Multiplayer.cs)                | Check if this instance is a client                      |
| [`Tp(vector2)`](Tool/Multiplayer.cs)             | Teleport local player in multiplayer                    |
| [`Tp(x, y)`](Tool/Multiplayer.cs)                | Teleport to float coordinates                           |
| [`Tp(playerName, vector2)`](Tool/Multiplayer.cs) | Teleport a specific player (use `"@a"` for all players) |
| [`Tp(playerName, x, y)`](Tool/Multiplayer.cs)    | Teleport a player to float coordinates                  |
| [`GetPlayerName(player)`](Tool/Multiplayer.cs)   | Get the name of a player object                         |

```csharp
// Teleport all players to a location
if (Multiplayer.IsNetworkRunning)
{
    Multiplayer.Tp("@a", new Vector2(50f, 100f));
}
```

### Config

[`Config`](Tool/Config.cs) — BepInEx configuration helpers.

| Method                                                                     | Description                       |
|----------------------------------------------------------------------------|-----------------------------------|
| [`ChangeConfig(entry, value)`](Tool/Config.cs)                             | Set a config entry value and save |
| [`SwitchType(configEntry, configName)`](Tool/Config.cs)                    | Toggle a boolean config entry     |
| [`SwitchType(configEntry, configName, logger, important)`](Tool/Config.cs) | Toggle with optional player alert |

```csharp
// Toggle a config value
ConfigEntry<bool> mySetting = Config.Bind("General", "MySetting", true, "Description");
Config.SwitchType(mySetting, "My Setting");
```

### RichText

[`RichText`](Tool/RichText.cs) — Unity rich text formatting utilities for in-game console messages.

| Method                                      | Description                                 |
|---------------------------------------------|---------------------------------------------|
| [`Color(text, color)`](Tool/RichText.cs)    | Wrap text in `<color>` tag by name          |
| [`Color(text, color)`](Tool/RichText.cs)    | Wrap text in `<color>` tag by Unity `Color` |
| [`Hex(text, hex)`](Tool/RichText.cs)        | Wrap text in `<color>` tag by hex value     |
| [`Alpha(text, alphaHex)`](Tool/RichText.cs) | Set text alpha by hex value                 |
| [`Alpha(text, alpha)`](Tool/RichText.cs)    | Set text alpha by float (0–1)               |
| [`Alpha(text, alpha)`](Tool/RichText.cs)    | Set text alpha by byte (0–255)              |
| [`Alpha(text, alpha)`](Tool/RichText.cs)    | Set text alpha by int (0–255)               |
| [`Bold(text)`](Tool/RichText.cs)            | Wrap text in `<b>` tag                      |
| [`Italic(text)`](Tool/RichText.cs)          | Wrap text in `<i>` tag                      |
| [`Size(text, size)`](Tool/RichText.cs)      | Wrap text in `<size>` tag                   |

**Shorthand color methods:**

| Method                              | Color     |
|-------------------------------------|-----------|
| [`Blue(text)`](Tool/RichText.cs)    | `blue`    |
| [`Red(text)`](Tool/RichText.cs)     | `red`     |
| [`Green(text)`](Tool/RichText.cs)   | `green`   |
| [`Yellow(text)`](Tool/RichText.cs)  | `yellow`  |
| [`White(text)`](Tool/RichText.cs)   | `white`   |
| [`Black(text)`](Tool/RichText.cs)   | `black`   |
| [`Cyan(text)`](Tool/RichText.cs)    | `cyan`    |
| [`Magenta(text)`](Tool/RichText.cs) | `magenta` |
| [`Gray(text)`](Tool/RichText.cs)    | `gray`    |
| [`Orange(text)`](Tool/RichText.cs)  | `orange`  |
| [`Purple(text)`](Tool/RichText.cs)  | `purple`  |
| [`Pink(text)`](Tool/RichText.cs)    | `pink`    |
| [`Brown(text)`](Tool/RichText.cs)   | `brown`   |

```csharp
// Format colorful console messages
string msg = RichText.Color("Warning: ", "red") + RichText.Bold("Low health!");
Log.Info(msg, Plugin.Logger);

// With hex color
string fancy = RichText.Hex("Hello", "#FF8800") + " " + RichText.Italic("World");
```

### Tools (Utils)

[`Tools`](Tool/Tools.cs) — Utility methods for argument parsing and validation.

| Method                                               | Description                                             |
|------------------------------------------------------|---------------------------------------------------------|
| [`CheckArgumentCount(args, desired)`](Tool/Tools.cs) | Ensure args array has at least `desired + 1` elements   |
| [`ParseFloat(s)`](Tool/Tools.cs)                     | Parse a float with invariant culture; throws on failure |
| [`ParseInt(s)`](Tool/Tools.cs)                       | Parse an int with invariant culture; throws on failure  |
| [`TryParseFloat(s, out result)`](Tool/Tools.cs)      | Try-parse a float without exception                     |

```csharp
// In a command handler
Tools.CheckArgumentCount(args, 2);          // Requires at least 2 args
float x = Tools.ParseFloat(args[1]);        // Parse float argument
int count = Tools.ParseInt(args[2]);        // Parse int argument
```

---

## Constants Reference

The [`Constant/`](Constant) namespace provides strongly-typed constants for game objects, making it easier and safer to
reference blocks, items, backgrounds, and key bindings without memorizing string IDs.

### Blocks

[`Blocks`](Constant/Blocks.cs) — Strongly-typed block definitions. Implicitly convertible to `ushort`.

| Property       | Type     | Description                               |
|----------------|----------|-------------------------------------------|
| `Id`           | `ushort` | Block ID                                  |
| `LocaleKey`    | `string` | Localization key                          |
| `Health`       | `float`  | Block hit points                          |
| `HitSound`     | `string` | Sound played when hit                     |
| `StepSound`    | `string` | Sound played when walking on it           |
| `SleepQuality` | `string` | Sleep quality rating                      |
| `IsMetallic`   | `bool`   | Whether the block is metallic             |
| `NoVariation`  | `bool`   | Whether the block has no visual variation |
| `Toxicity`     | `float`  | Toxicity level                            |
| `IsSlippery`   | `bool`   | Whether the block is slippery             |

Use [`Blocks.FromId(ushort)`](Constant/Blocks.cs) to resolve a block ID to its constant.

```csharp
// Place blocks using strongly-typed constants
World.PlaceBlock(10, 20, Blocks.Granite);
World.PlaceBlock(new Vector2(15, 25), Blocks.SteelTile);

// Implicit conversion to ushort
ushort blockId = Blocks.Copper; // 34
```

### Items

[`Items`](Constant/Items.cs) — Strongly-typed item definitions. Implicitly convertible to `string`.

| Property   | Type     | Description                               |
|------------|----------|-------------------------------------------|
| `Id`       | `string` | Item ID                                   |
| `Category` | `string` | Item category (food, medical, tool, etc.) |
| `Weight`   | `float`  | Item weight                               |
| `Value`    | `int`    | Item value                                |
| `Rec`      | `int`    | Item rarity/recipe tier                   |

Use [`Items.FromId(string)`](Constant/Items.cs) to resolve an item ID to its constant.

```csharp
// Give items using strongly-typed constants
Player.PickItem(Items.Rifle, 0, true);
Player.PickItem(Items.Bandage, 1);

// Implicit conversion to string
string itemId = Items.Rifle; // "rifle"
```

### Backgrounds

[`Backgrounds`](Constant/Backgrounds.cs) — String constants for background tile IDs.

| Constant                                           | Value                   |
|----------------------------------------------------|-------------------------|
| [`Backgrounds.Fungal`](Constant/Backgrounds.cs)    | `"fungalBackground"`    |
| [`Backgrounds.Grass`](Constant/Backgrounds.cs)     | `"grassBackground"`     |
| [`Backgrounds.Ice`](Constant/Backgrounds.cs)       | `"iceBackground"`       |
| [`Backgrounds.Rock`](Constant/Backgrounds.cs)      | `"rockBackground"`      |
| [`Backgrounds.Sand`](Constant/Backgrounds.cs)      | `"sandBackground"`      |
| [`Backgrounds.Soil`](Constant/Backgrounds.cs)      | `"soilBackground"`      |
| [`Backgrounds.Steel`](Constant/Backgrounds.cs)     | `"steelBackground"`     |
| [`Backgrounds.Vents`](Constant/Backgrounds.cs)     | `"ventsBackground"`     |
| [`Backgrounds.Wasteland`](Constant/Backgrounds.cs) | `"wastelandBackground"` |

```csharp
// Place backgrounds using constants
World.PlaceBackground(new Vector2Int(10, 20), Backgrounds.Grass);
World.PlaceBackground(new Vector2(15f, 25f), Backgrounds.Sand);
```

### Keys

[`Keys`](Constant/Keys.cs) — Strongly-typed key action constants. Implicitly convertible to `string`.

| Constant                                   | Action Name         |
|--------------------------------------------|---------------------|
| [`Keys.Jump`](Constant/Keys.cs)            | `"jump"`            |
| [`Keys.Up`](Constant/Keys.cs)              | `"up"`              |
| [`Keys.Left`](Constant/Keys.cs)            | `"left"`            |
| [`Keys.Right`](Constant/Keys.cs)           | `"right"`           |
| [`Keys.Down`](Constant/Keys.cs)            | `"down"`            |
| [`Keys.Attack`](Constant/Keys.cs)          | `"attack"`          |
| [`Keys.ItemInteract`](Constant/Keys.cs)    | `"iteminteract"`    |
| [`Keys.Throw`](Constant/Keys.cs)           | `"throw"`           |
| [`Keys.ToggleInventory`](Constant/Keys.cs) | `"toggleinventory"` |
| [`Keys.Pause`](Constant/Keys.cs)           | `"pause"`           |
| [`Keys.Console`](Constant/Keys.cs)         | `"console"`         |
| [`Keys.Craft`](Constant/Keys.cs)           | `"craft"`           |

Use [`Keys.FromAction(string)`](Constant/Keys.cs) to resolve an action name to its constant.

```csharp
// Check key bindings using constants
if (Key.IsKeyDown(Keys.Attack))
{
    // Player attacked
}

// Implicit conversion to string
string action = Keys.Jump; // "jump"
```

---

## UI Toolkit (WIP)

The [`Tool/UI/`](Tool/UI) directory contains in-development UI creation utilities for building in-game mod interfaces.
Currently **commented out** and under active development.

| File                                | Description                                                               |
|-------------------------------------|---------------------------------------------------------------------------|
| [`UILayout`](Tool/UI/UILayout.cs)   | Canvas management, panel creation, RectTransform helpers                  |
| [`UIWidgets`](Tool/UI/UIWidgets.cs) | Button and text creation with game-style styling and localization support |

These will be enabled in a future release.

---

## Project Structure

```
MossLib/
├── Plugin.cs                    # Main plugin entry point
├── Base/
│   ├── ModCommandBase.cs        # Command registration base class
│   ├── ModLangGenBase.cs        # Language generator base class
│   └── ModLocaleBase.cs         # Localization base class
├── Constant/
│   ├── Backgrounds.cs           # Background ID constants
│   ├── Blocks.cs                # Strongly-typed block definitions
│   ├── Items.cs                 # Strongly-typed item definitions
│   └── Keys.cs                  # Key action name constants
├── Example/
│   ├── ModCommand.cs            # Example command implementation
│   ├── ModLocale.cs             # Example locale implementation
│   └── Lang/
│       ├── EnLangGenerator.cs   # English language generator
│       ├── ZhCnLangGenerator.cs # Simplified Chinese generator
│       └── ZhTwLangGenerator.cs # Traditional Chinese generator
└── Tool/
    ├── Config.cs                # Config toggling utilities
    ├── Console.cs               # Game console wrapper
    ├── Key.cs                   # Input handling (key binds, mouse click waiting)
    ├── LocaleGenerator.cs       # Language file generator manager
    ├── Log.cs                   # Console logging utilities
    ├── Multiplayer.cs           # Multiplayer integration
    ├── Player.cs                # Player manipulation tools
    ├── RichText.cs              # Rich text formatting utilities
    ├── Tools.cs                 # Argument/parsing utilities
    ├── World.cs                 # World manipulation tools
    └── UI/                      # UI Toolkit (WIP, commented out)
        ├── UILayout.cs          # Canvas and panel creation
        └── UIWidgets.cs         # Button and text creation
```
