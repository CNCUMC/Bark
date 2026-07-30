***English*** | [简体中文](../zh-CN/configuration.md)

# Configuration & Localization

Bark provides two configuration systems: `BetterOptions` + `LangGenerator` for C# mods, and `config.json` +
`locale.json` for script mods.

## C# Mods

### Registering Options

Use `BetterOptions` static methods to register options. They appear in the game's settings UI for players to tweak.

```csharp
using Bark.BetterCCL;
using CUCoreLib.Data;
using UnityEngine;

// Bool toggle
BetterOptions.Bool("my_mod", "enable_auto_heal", Setting.SettingCategory.Game,
    true,  // default
    val => Log.Info($"Auto-heal: {(val ? "ON" : "OFF")}"));

// Int slider
BetterOptions.Int("my_mod", "heal_amount", Setting.SettingCategory.Game,
    10, 0, 100);

// Float slider
BetterOptions.Float("my_mod", "speed_multiplier", Setting.SettingCategory.Game,
    1.0f, 0.1f, 5.0f);

// Dropdown
BetterOptions.Dropdown("my_mod", "difficulty", Setting.SettingCategory.Game,
    0,  // default selected index
    new[]
    {
        new ModDropdownChoice("Easy", "Easy"),
        new ModDropdownChoice("Normal", "Normal"),
        new ModDropdownChoice("Hard", "Hard"),
    });

// Keybind
BetterOptions.Keybind("my_mod", "toggle_key", Setting.SettingCategory.Input,
    KeyCode.F5);
```

Full signatures:

```
Bool(ns, key, category, defaultValue, apply?)
Int(ns, key, category, defaultValue, min, max, apply?)
Float(ns, key, category, defaultValue, min, max, apply?, formatValue?)
Dropdown(ns, key, category, defaultValue, choices[], apply?)
Keybind(ns, key, category, defaultValue, apply?)
```

**Category**: `Setting.SettingCategory` includes `Game` / `Audio` / `Input` / `Video`, corresponding to the game
settings tabs. Pass a `string` to create a custom tab.

**apply callback**: Fires when the player changes the option. Useful for hot-reloading.

> ℹ️ Option labels and descriptions need locale text registered via `LangGenerator` (see next section).

### Registering Localization Text

Extend `ModLangGenBase` (single language) or `ModLangGenMultiBase` (multi-language). Override `BuildLocaleData()` and
call helpers to write text for each language.

**Single-language**:

```csharp
using Bark.Base;

internal class MyLangGen : ModLangGenBase
{
    protected override string LanguageCode => "EN";
    protected override string NameSpace => "my_mod";

    protected override void BuildLocaleData()
    {
        Option("enable_auto_heal", "Auto Heal", "Enable periodic health regen");
        Option("heal_amount", "Heal Amount", "HP restored per tick");
        Option("speed_multiplier", "Speed Multiplier", "Movement speed multiplier");

        Log("loaded", "Mod loaded");
        Command("help", "Show help");

        Item("special_sword", "Flame Sword", "A sword imbued with fire damage");

        Other("greeting", "Hello, world");
    }
}
```

Then initialize in `Awake()`:

```csharp
new MyLangGen().Initialize(Logger);
BetterLocale.Flush();
```

**Multi-language** (recommended):

```csharp
internal class MyLangGen : ModLangGenMultiBase
{
    protected override string NameSpace => "my_mod";
    protected override IEnumerable<string> LanguageCodes =>
        ["EN", "zh-CN", "zh-TW", "ru-RU"];

    protected override void BuildLocaleData()
    {
        // Option(labelEN, descEN, labelZH, descZH, labelTW, descTW, labelRU, descRU)
        Option("enable_auto_heal",
            "Auto Heal",        "Enable periodic health regen",
            "自动回血",          "开启后每秒回复一定血量",
            "自動回血",          "開啟後每秒回復一定血量",
            "Автохил",          "Включить периодическое восстановление здоровья");

        // Log(EN, ZH, TW, RU)
        Log("loaded",
            "Mod loaded",
            "模组已加载",
            "模組已載入",
            "Мод загружен");
    }
}
```

`ModLangGenMultiBase` helpers:

| Method                     | Description                         |
|----------------------------|-------------------------------------|
| `Other(key, values...)`    | General text                        |
| `Log(key, values...)`      | Log / console output text           |
| `Command(key, values...)`  | Console command descriptions        |
| `Item(key, values...)`     | Item name + description (pairs)     |
| `Building(key, values...)` | Building name + description (pairs) |
| `Moodle(key, values...)`   | Moodle name + description (pairs)   |
| `Option(key, values...)`   | Option label + description (pairs)  |
| `Liquid(key, values...)`   | Liquid name + description (pairs)   |
| `Title(key, values...)`    | Title name + description (pairs)    |

`values` parameters follow the `LanguageCodes` order. Pair methods (`Item`, `Option`, etc.) use 2 parameters per
language: label + description.

### Flush Localization to File

```csharp
BetterLocale.Flush();
```

Writes all `SetDefault` texts to `BepInEx/config/CUCoreLib/Locales/{language}.json`. **Option labels/descriptions won't
appear in the game UI until flushed.**

### Localization Lookup Priority

```
CCL registered text → Bark fallback defaults → English fallback → raw key
```

If CCL has a translation, use it. Otherwise use Bark defaults. If the current language is missing, fall back to English.
If nothing matches, show the raw key.

## Script Mods

### Registering Config Options

Option **definitions** live in the mod's `Config/options.json` (shipped with the mod, read-only). User **saved values**
are written to `ScriptMod/Configs/{modId}.json` as a simple key-value map.

**`ScriptMod/Mods/MyMod/Config/options.json`** (option definitions, shipped with the mod):

```json
{
  "_options": {
    "enable_auto_heal": {
      "type": "bool",
      "default": true,
      "category": "game"
    },
    "heal_amount": {
      "type": "int",
      "default": 10,
      "min": 1,
      "max": 100,
      "category": "game"
    },
    "speed_multiplier": {
      "type": "float",
      "default": 1.0,
      "min": 0.1,
      "max": 5.0
    },
    "mode": {
      "type": "dropdown",
      "default": 0,
      "choices": [
        "Easy",
        "Normal",
        "Hard"
      ],
      "category": "game"
    },
    "hotkey": {
      "type": "keybind",
      "default": "F5",
      "category": "input"
    }
  }
}
```

**`ScriptMod/Configs/MyMod.json`** (user saved values, auto-generated/updated by the game):

```json
{
  "enable_auto_heal": false,
  "heal_amount": 42,
  "speed_multiplier": 1.5,
  "mode": 1,
  "hotkey": "F6"
}
```

On load, Bark merges both layers: user saved values override the definition defaults before registering with the game
settings UI.

| Field      | Required | Description                                                           |
|------------|----------|-----------------------------------------------------------------------|
| `type`     | Yes      | `bool` / `int` / `float` / `dropdown` / `keybind`                     |
| `default`  | Yes      | Default value                                                         |
| `category` | No       | `game` / `audio` / `input` / `video` — custom strings create new tabs |
| `min`      | No       | int/float minimum                                                     |
| `max`      | No       | int/float maximum                                                     |
| `choices`  | No       | dropdown option list                                                  |

If `category` is omitted, the mod id is used as the tab name.

> ℹ️ Option labels and descriptions require locale files (see below).

### Localization

Create a `Lang/` folder in your mod directory with `EN.json` (or other language codes):

```
ScriptMod/Mods/MyMod/
  mod.json
  main.js
  Lang/
    EN.json
    zh-CN.json
```

**`Lang/EN.json`**:

```json
{
  "option": {
    "my_mod.enable_auto_heal": "Auto Heal",
    "my_mod.enable_auto_healdsc": "Enable periodic health regen",
    "my_mod.heal_amount": "Heal Amount",
    "my_mod.heal_amountdsc": "HP restored per tick"
  },
  "log": {
    "welcome": "Welcome to this mod!",
    "damage_report": "Took {0} damage, location: {1}"
  }
}
```

Option locale key format: `{modId}.{category}.{optionKey}`, append `dsc` for description.

Bark automatically loads locale data into CCL on load, so the options menu shows the correct text.

### Reading Config in Scripts

```js
var enabled = OptionsApi.GetBool(ScriptInfo.Id, "enable_auto_heal");
var amount = OptionsApi.GetInt(ScriptInfo.Id, "heal_amount");
var speed = OptionsApi.GetFloat(ScriptInfo.Id, "speed_multiplier");
var mode = OptionsApi.GetDropdown(ScriptInfo.Id, "mode");
```

See [OptionsApi](script-api/options.md) for details.

### Using Localization in Scripts

```js
var msg = Locale.Get("welcome");                     // auto-inserts mod id prefix
var msg2 = Locale.GetFormatted("damage_report", 25);  // {0} → 25
```

See [LocaleApi](script-api/locale.md) for details.
