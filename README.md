![Logo](Logo.png)

[中文指南](README_ZH.md)

# Bark

[GitHub](https://github.com/CNCUMC/Bark) | [NexusMods](https://www.nexusmods.com/scavprototype/mods/362) | [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib)

_A mod utility library for [Casualties Unknown](https://store.steampowered.com/app/4576490/), built on top
of [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib)._

_Evolved from [Moss Lib](https://github.com/Explosive-Hydra/Moss-Lib)._

---

## Table of Contents

- [Overview](#overview)
- [Documentation](#documentation)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Localization](#localization)
- [Setting Options (BetterOptions)](#setting-options-betteroptions)
- [Update Checking (UpdateUtil)](#update-checking-updateutil)
- [Tools Reference](#tools-reference)
- [Constants Reference](#constants-reference)
- [License](#license)

---

## Overview

**Bark** is a BepInEx plugin utility library for **Casualties Unknown**,
extending [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib) (CCL) with enhanced localization, settings, and
game utility tools.

| Module                                        | Description                                                              |
|-----------------------------------------------|--------------------------------------------------------------------------|
| [`BetterLocale`](BetterCCL/BetterLocale.cs)   | Localization system built on CCL's `LocaleRegistry`                      |
| [`BetterOptions`](BetterCCL/BetterOptions.cs) | CCL settings registration wrapper (Float/Int/Bool/Dropdown/Keybind)      |
| [`ModLangGenBase`](Base/ModLangGenBase.cs)    | Language generator base class                                            |
| [`UpdateUtil`](Tool/UpdateUtil.cs)            | GitHub-based mod update checker                                          |
| [`PlayerUtil`](Tool/PlayerUtil.cs)            | Player: status/vitals/movement/drugs/inventory/recovery/alert/thresholds |
| [`SkillUtil`](Tool/SkillUtil.cs)              | Skill level/XP manipulation                                              |
| [`LimbUtil`](Tool/LimbUtil.cs)                | Limb operations: healing, damage, status checks                          |
| [`WorldUtil`](Tool/WorldUtil.cs)              | World manipulation: blocks, items                                        |
| [`InventoryUtil`](Tool/InventoryUtil.cs)      | Inventory operations                                                     |
| [`ItemUtil`](Tool/ItemUtil.cs)                | Item utilities: FindNearby, Repair, SetCondition                         |
| [`InputUtil`](Tool/InputUtil.cs)              | Input handling: mouse position, click waiting                            |
| [`LogUtil`](Tool/LogUtil.cs)                  | Console logging + validation helpers                                     |
| [`TextUtil`](Tool/TextUtil.cs)                | Rich text formatting: color, alpha, bold, italic, size                   |
| [`ToolsUtil`](Tool/ToolsUtil.cs)              | Argument validation, float/int parsing                                   |
| [`Blocks`](Constant/Blocks.cs)                | Strongly-typed block definitions                                         |
| [`Items`](Constant/Items.cs)                  | Strongly-typed item definitions                                          |
| [`Backgrounds`](Constant/Backgrounds.cs)      | Background ID string constants                                           |
| [`Keys`](Constant/Keys.cs)                    | Key action constants                                                     |
| [`Slots`](Constant/Slots.cs)                  | Inventory slot definitions                                               |

---

## Documentation

Full documentation at [`docs/en-US/`](docs/en-US) (and [简体中文](docs/zh-CN)):

- [Getting Started](docs/en-US/getting-started.md) — Installation, environment, choosing your path
- [Script Development](docs/en-US/script-mod.md) — JS/Lua mods, lifecycle hooks, console commands
  > **Note:** Puerts supports Python, but the Python runtime (~22 MB) currently bundled
  > with the package is too large and has been put on hold. Python support will be
  > considered in a future major release if there is user demand. See `TodoPython/`
  > for archived implementation.
- [C# Mod Development](docs/en-US/csharp-mod.md) — Event subscription, Harmony patches, API registration
- [Configuration & Localization](docs/en-US/configuration.md) — Options registration, multi-language
- [Script Event Hooks](docs/en-US/script-events.md) — All listenable event hooks
- [C# Event System](docs/en-US/csharp-events.md) — Event subscription / trigger / custom events
- [Script API Reference](docs/en-US/script-api) — BodyUtil, PlayerUtil, LimbUtil, etc.
- [C# API Reference](docs/en-US/csharp-api) — EventUtil, UpdateUtil

---

## Installation

1. Install [BepInEx 5.x](https://github.com/BepInEx/BepInEx) for Casualties Unknown.
2. Install [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib) ≥ 1.0.2 — place `CUCoreLib.dll` into
   `BepInEx/plugins/CUCoreLib/`.
3. Install [Bark](https://www.nexusmods.com/scavprototype/mods/362) Extract it and place it in the `BepInEx/plugins/`
   folder.

> **For mod developers:** Reference `Bark.dll` in your project, and add `[BepInDependency("org.cucnmc.bark")]` to your
> plugin class.

---

## Quick Start

### 1. Add Dependencies

```csharp
[BepInPlugin(Guid, Name, Version)]
[BepInDependency("net.cucorelib")]     // CCL is required
[BepInDependency("org.cucnmc.bark")]   // Bark extends CCL
public class MyPlugin : BaseUnityPlugin
{
    // ...
}
```

### 2. Localization

Bark provides `BetterLocale` on top of CCL's `LocaleRegistry`:

```csharp
using Bark.BetterCCL;

// Get localized text (CCL → Bark defaults → key fallback)
string text = BetterLocale.GetOther("bark.feature.enabled");

// Define fallback translations via language generators:
// EnLangGenerator.cs
Other("feature.enabled", "Enable Feature");

// ZhCnLangGenerator.cs
Other("eature.enabled", "启用功能");
```

See [Example/Lang/](Example/LangGenerator.cs) for sample generators.

### 3. Register a Setting

```csharp
using Bark.BetterCCL;

BetterOptions.Bool("bark", "feature_enabled", Setting.SettingCategory.Game, true);

// With custom category tab
BetterOptions.Bool("bark", "advanced_mode", "Bark", false);
```

### 4. Check for Updates

```csharp
using Bark.Tool;

// Call in Awake() — async, results output to logger + game console
UpdateUtil.Check("YourName/YourRepo", "YourMod", "1.0.0", Logger);
```

---

## Localization

### Generators (`ModLangGenBase`)

```csharp
public class EnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "EN";
    protected override void BuildLocaleData()
    {
        Other("bark.tooltip.heat", "Hot enough to warp.");
        Option("bark.game.test", "Test Mode", "Turns on the test mode");
    }
}
```

| Method                              | Category                   |
|-------------------------------------|----------------------------|
| `Item(key, value, description)`     | `item`                     |
| `Building(key, value, description)` | `build`                    |
| `Moodle(key, value, description)`   | `moodle`                   |
| `Other(key, value)`                 | `other`                    |
| `Option(key, label, description)`   | `option` (settings labels) |
| `Log(key, value)`                   | `log`                      |
| `Command(key, value, description)`  | `command`                  |
| `Liquid(key, value, description)`   | `liquid`                   |
| `Title(key, value, description)`    | `title`                    |

### BetterLocale API

#### Get (retrieve localized text)

| Method                    | Category  |
|---------------------------|-----------|
| `GetItem(key, args?)`     | `item`    |
| `GetBuilding(key, args?)` | `build`   |
| `GetMoodle(key, args?)`   | `moodle`  |
| `GetOther(key, args?)`    | `other`   |
| `GetLog(key, args?)`      | `log`     |
| `GetCommand(key, args?)`  | `command` |
| `GetOption(key, args?)`   | `option`  |
| `GetLiquid(key, args?)`   | `liquid`  |
| `GetTitle(key, args?)`    | `title`   |

> **Note:** `args` replace `{0}`, `{1}`, etc. in the resolved locale value.
> For example, `BetterLocale.GetLog("update.available", "Bark", "1.0", "2.0")` returns
> `"Bark update available! 1.0 -> 2.0"`.

#### Has (check if translation exists)

| Method                  | Category     |
|-------------------------|--------------|
| `HasKey(category, key)` | Any category |
| `HasKeyItem(key)`       | `item`       |
| `HasKeyBuilding(key)`   | `build`      |
| `HasKeyMoodle(key)`     | `moodle`     |
| `HasKeyOther(key)`      | `other`      |
| `HasKeyLog(key)`        | `log`        |
| `HasKeyCommand(key)`    | `command`    |
| `HasKeyOption(key)`     | `option`     |
| `HasKeyLiquid(key)`     | `liquid`     |
| `HasKeyTitle(key)`      | `title`      |

#### Other

| Method                            | Description                                |
|-----------------------------------|--------------------------------------------|
| `SetDefault(lang, cat, key, val)` | Register fallback value                    |
| `Flush()`                         | Write all defaults to CCL locale directory |
| `ToRichText(md)`                  | Convert **Markdown** to Unity Rich Text    |
| `StripMarkdown(md)`               | Strip markdown to plain text               |

---

## Setting Options (BetterOptions)

```csharp
BetterOptions.Bool("ns", "key", Setting.SettingCategory.Game, true);
BetterOptions.Int("ns", "level", Setting.SettingCategory.Game, 5, 1, 10);
BetterOptions.Float("ns", "volume", Setting.SettingCategory.Audio, 0.8f, 0f, 1f);
BetterOptions.Dropdown("ns", "mode", Setting.SettingCategory.Game, 0, choices);
BetterOptions.Keybind("ns", "hotkey", Setting.SettingCategory.Input, KeyCode.F5);

// Custom category tab
BetterOptions.Bool("ns", "key", "My Mod Tab", false);
```

---

## Update Checking (UpdateUtil)

```csharp
using Bark.Tool;

// Async check via GitHub Releases API — localizable messages
UpdateUtil.Check("CNCUMC/Bark", "MyMod", "1.0.0", Logger);
```

| Parameter        | Description                                       |
|------------------|---------------------------------------------------|
| `githubRepo`     | GitHub repo path, e.g. `"CNCUMC/Bark"`            |
| `modName`        | Display name used in log/console messages         |
| `currentVersion` | Current version, supports `"1.0.0"` or `"v1.0.0"` |
| `logger`         | Mod's BepInEx `ManualLogSource`                   |

Results are output to both the BepInEx log and the game console. Messages are localized via `BetterLocale`
(`update.no_repo`, `update.failed`, `update.no_version`, `update.available`, `update.uptodate`).

---

## Tools Reference

> Full API docs at [Script API Reference](docs/en-US/script-api) and [C# API Reference](docs/en-US/csharp-api).
> Overview below.

| Class           | Description                          | Detailed Docs                                           |
|-----------------|--------------------------------------|---------------------------------------------------------|
| `LogUtil`       | Logging + validation helpers         | [Log](docs/en-US/script-api/log.md)                     |
| `PlayerUtil`    | Player operations                    | [Player](docs/en-US/script-api/player.md)               |
| `BodyUtil`      | Body vitals system                   | [Body System](docs/en-US/script-api/body-system.md)     |
| `LimbUtil`      | Limb operations                      | [Limbs](docs/en-US/script-api/limbs.md)                 |
| `WorldUtil`     | World editing                        | [World](docs/en-US/script-api/world.md)                 |
| `SkillUtil`     | Skill level/XP                       | [Skills](docs/en-US/script-api/skills.md)               |
| `InventoryUtil` | Inventory queries                    | [Inventory & Items](docs/en-US/script-api/inventory.md) |
| `ItemUtil`      | Item search, repair, durability      | [Inventory & Items](docs/en-US/script-api/inventory.md) |
| `InputUtil`     | Input handling                       | —                                                       |
| `TextUtil`      | Rich text formatting                 | —                                                       |
| `ToolsUtil`     | Argument validation, float/int parse | —                                                       |
| `EventUtil`     | Event trigger / manual registration  | [EventUtil](docs/en-US/csharp-api/event-util.md)        |
| `UpdateUtil`    | GitHub release version check         | [UpdateUtil](docs/en-US/csharp-api/update.md)           |

---

## Constants Reference

### Blocks

```csharp
ushort blockId = Blocks.SteelTile;  // implicit conversion
Blocks block = Blocks.FromId(6);
```

### Items

```csharp
string itemId = Items.Medkit;       // implicit conversion
Items item = Items.FromId("medkit");
```

### Backgrounds / Keys / Slots

```csharp
string bgId = Backgrounds.Rock;
KeyCode key = Keys.Jump;
int slotId = Slots.MainHand;
```

---

## License

[LGPL v3](LICENSE.md)
