[中文指南](README_ZH.md)

## Bark

[GitHub](https://github.com/CNCUMC/Bark) | [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib)

_A mod utility library for [Casualties Unknown](https://store.steampowered.com/app/3624440/Casualties_Unknown_Demo/), built on top of [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib)._

_Evolved from [Moss Lib](https://github.com/Explosive-Hydra/Moss-Lib)._

---

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Localization](#localization)
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

---

## Overview

**Bark** is a BepInEx plugin utility library for **Casualties Unknown**, extending [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib) (CCL) with additional tools and constants for mod development. It includes:

| Module | Description |
|--------|-------------|
| [`Log`](Tool/Log.cs) | Advanced in-game console logging utilities |
| [`GameConsole`](Tool/Console.cs) | Wrapper to execute game console commands programmatically |
| [`World`](Tool/World.cs) | World manipulation: place blocks, items, and background tiles |
| [`Player`](Tool/Player.cs) | Player manipulation: teleport, alerts, inventory management |
| [`Key`](Tool/Key.cs) | Input handling: key binding checks, mouse click waiting, world-space coordinates |
| [`Multiplayer`](Tool/Multiplayer.cs) | Multiplayer support with reflection-based KrokoshaCasualtiesMP integration |
| [`Config`](Tool/Config.cs) | BepInEx configuration entry helpers |
| [`RichText`](Tool/RichText.cs) | Unity rich text formatting: color, alpha, bold, italic, size |
| [`Tools`](Tool/Tools.cs) | Argument validation, float/int parsing utilities |
| [`Blocks`](Constant/Blocks.cs) | Strongly-typed block definitions with properties |
| [`Items`](Constant/Items.cs) | Strongly-typed item definitions with properties |
| [`Backgrounds`](Constant/Backgrounds.cs) | Background ID string constants |
| [`Keys`](Constant/Keys.cs) | Strongly-typed key action constants |
| [`Slots`](Constant/Slots.cs) | Inventory slot definitions |

---

## Installation

1. Install [BepInEx 5.x](https://github.com/BepInEx/BepInEx) for Casualties Unknown.
2. Install [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib) — place `CUCoreLib.dll` into `BepInEx/plugins/CUCoreLib/`.
3. Download the latest `Bark.dll` from the [Releases](https://github.com/CNCUMC/Bark/releases) page.
4. Place `Bark.dll` into your `BepInEx/plugins/` folder.
5. (Optional) For multiplayer features, install **KrokoshaCasualtiesMP** as a soft dependency.

> **For mod developers:** Reference `Bark.dll` in your project, and add `[BepInDependency("org.cucnmc.bark")]` to your plugin class.

---

## Quick Start

### 1. Add Dependencies

```csharp
[BepInPlugin(Guid, Name, Version)]
[BepInDependency("net.cucorelib")]  // CCL is required
[BepInDependency("org.cucnmc.bark")] // Bark extends CCL
public class MyPlugin : BaseUnityPlugin
{
    // ...
}
```

### 2. Localization (via CCL)

Bark uses [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib)'s [`LocaleRegistry`](https://github.com/jimmyking9999999/CUCoreLib) for all localization. No separate locale setup is needed.

```csharp
using CUCoreLib.Registries;

// In your code — just use LocaleRegistry.Get
string text = LocaleRegistry.Get("other", "mykey", "Fallback English text");

// With string.Format arguments
string msg = LocaleRegistry.Get("other", "mykey.arg", "Hello {0}!");
Console.WriteLine(string.Format(msg, "World"));
```

After your mod registers content, run the `createLocale` command in-game to generate the baseline `EN.json` file:

```
createLocale
```

This writes to `BepInEx/config/CUCoreLib/Locales/EN.json`. Translators can then create language-specific files (e.g. `CN-zh.json`).

### 3. Register a Custom Command

```csharp
[HarmonyPatch(typeof(ConsoleScript))]
public class MyCommand
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

## Localization

All localization in Bark is handled by [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib)'s [`LocaleRegistry`](https://github.com/jimmyking9999999/CUCoreLib).

### How It Works

1. Use `LocaleRegistry.Get("category", "key", "fallback")` in code
2. Run the `createLocale` console command to generate `EN.json`
3. Hand the file to translators, who produce language files (e.g. `CN-zh.json`)
4. CCL automatically loads the correct locale file at runtime

### Categories

| Category | Usage |
|----------|-------|
| `item` | Item names and descriptions |
| `building` | Building names and descriptions |
| `moodle` | Status effect names |
| `other` | UI text, alerts, console commands, misc strings |

### Example

```csharp
// Simple lookup
string tooltip = LocaleRegistry.Get("other", "bark.tooltip.heat", "Hot enough to warp a glove.");

// With format args
string msg = LocaleRegistry.Get("other", "bark.teleport.success", "Teleported: {0} to {1}");
Console.WriteLine(string.Format(msg, "Player1", "10,20"));
```

---

## Tools Reference

### Log

`Bark.Tool.Log` — Enhanced in-game console logging.

| Method | Description |
|--------|-------------|
| `Log.Info(text, logger)` | Log info to both game console and BepInEx |
| `Log.Error(text, logger)` | Log error with `[ERROR]` prefix |
| `Log.Warning(text, logger)` | Log warning with `[WARNING]` prefix |
| `Log.LogToConsole(text)` | Write directly to game console |
| `Log.Alert(text, logger, important, delay?)` | Show alert on screen + log |

### GameConsole

`Bark.Tool.GameConsole` — Programmatic console command execution.

| Method | Description |
|--------|-------------|
| `GameConsole.RunCommand(key)` | Execute a registered console command by name |

### World

`Bark.Tool.World` — World manipulation utilities.

| Method | Description |
|--------|-------------|
| `World.PlaceBlock(x, y, blockId)` | Place a block at world position |
| `World.FillBlocks(startX, startY, endX, endY, blockId)` | Fill a rectangular area with blocks |
| `World.PlaceItem(x, y, itemId)` | Spawn an item at world position |
| `World.PlaceBackground(pos, backgroundId)` | Place a background tile |
| `World.CheckForWorld()` | Throw if no world is loaded |
| `World.ClearCache()` | Clear cached sprites and materials |

### Player

`Bark.Tool.Player` — Player manipulation.

| Method | Description |
|--------|-------------|
| `Player.Tp(x, y)` | Teleport player to position |
| `Player.Alert(text, important, delay?)` | Show alert message on screen |
| `Player.PickItem(itemId, slot, force?)` | Add item to inventory slot |

### Key

`Bark.Tool.Key` — Input handling.

| Method | Description |
|--------|-------------|
| `Key.MouseWorldPosition()` | Get mouse position in world coordinates |
| `Key.LeftClickPosition()` | Get position on left click (or zero) |
| `Key.WaitForLeftClick()` | Coroutine: wait for left click |
| `Key.WaitForRightClick()` | Coroutine: wait for right click |

### Multiplayer

`Bark.Tool.Multiplayer` — Multiplayer support via reflection.

| Property | Description |
|----------|-------------|
| `Multiplayer.IsNetworkRunning` | Check if multiplayer session is active |
| `Multiplayer.IsClient` | Check if running as client |
| `Multiplayer.Tp(playerName, x, y)` | Teleport player by name (supports `@a` for all) |

### Config

`Bark.Tool.Config` — BepInEx configuration helpers.

| Method | Description |
|--------|-------------|
| `Config.Register(...)` | Register a config entry with locale description |

### RichText

`Bark.Tool.RichText` — Unity rich text formatting.

| Method | Description |
|--------|-------------|
| `RichText.Color(text, color)` | Wrap text in color tags |
| `RichText.Hex(text, hex)` | Wrap text in hex color |
| `RichText.Bold(text)` | Wrap text in bold tags |
| `RichText.Size(text, size)` | Wrap text in size tags |
| `RichText.Alpha(text, alpha)` | Set text alpha transparency |

### Tools

`Bark.Tool.Tools` — Utility functions.

| Method | Description |
|--------|-------------|
| `Tools.CheckArgumentCount(args, min)` | Validate minimum argument count |
| `Tools.ParseFloat(s)` | Parse float with error handling |
| `Tools.ParseInt(s)` | Parse int with error handling |

---

## Constants Reference

### Blocks

`Bark.Constant.Blocks` — Strongly-typed block definitions.

```csharp
ushort blockId = Blocks.SteelTile;        // implicit conversion to ushort
float health = Blocks.Diamond.Health;     // block health
bool metallic = Blocks.SteelTile.IsMetallic;
Blocks block = Blocks.FromId(6);          // lookup by ID
```

### Items

`Bark.Constant.Items` — Strongly-typed item definitions.

```csharp
string itemId = Items.Medkit;             // implicit conversion to string
float weight = Items.Medkit.Weight;
string category = Items.Medkit.Category;  // "container"
Items item = Items.FromId("medkit");      // lookup by ID
```

### Backgrounds

`Bark.Constant.Backgrounds` — Background ID constants.

```csharp
string bgId = Backgrounds.Rock;           // "rockBackground"
Backgrounds bg = Backgrounds.FromId("rockBackground");
```

### Keys

`Bark.Constant.Keys` — Key binding constants.

```csharp
KeyCode jumpKey = Keys.Jump;
KeyCode attackKey = Keys.Attack;
```
