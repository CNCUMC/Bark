***English*** | [简体中文](../zh-CN/script-mod.md)

# Script Development

Bark supports writing mods in JavaScript or Lua. This guide uses JavaScript for examples. Lua users see
the [Lua Notes](#lua-notes) section.

## Creating a Script

Create your own folder under `ScriptMod/Mods/` with a `mod.json` and an entry script:

```
ScriptMod/Mods/
  MyMod/
    mod.json
    main.js        ← Entry file: main.js / main.mjs / main.lua
```

Language is detected from the entry file extension — `main.js` = JS mod, `main.lua` = Lua mod.

### mod.json

```json
{
  "id": "my_mod",
  "name": "My Mod",
  "version": "1.0.0",
  "author": {
    "code": "YourName"
  },
  "description": "Mod description",
  "bark_version": ">=0.1.0",
  "game_version": ">=0.9.0",
  "dependencies": []
}
```

| Field          | Type   | Required | Description                                                  |
|----------------|--------|----------|--------------------------------------------------------------|
| `id`           | string | Yes      | Unique ID, use snake_case, don't conflict with others        |
| `name`         | string | Yes      | Display name                                                 |
| `version`      | string | Yes      | Semver, e.g. `"1.0.0"`                                       |
| `author`       | object | No       | Contributors, key = role (code, art, etc.), value = name     |
| `description`  | string | No       | Mod description                                              |
| `bark_version` | string | No       | Required Bark version (semver range)                         |
| `game_version` | string | No       | Compatible game version (semver range)                       |
| `dependencies` | array  | No       | Dependent mods, format `[{"id": "some_mod", "version": "1.0.0"}]` |
| `tiles`        | object | No       | Tile index mapping, e.g. `{"marble": 50}`, see [Custom Tiles](script-mod/tile.md) |

### Entry File

```js
function onLoad() {
    Log.Info("My mod loaded!");
}

function onPlayerJumpStart() {
    Log.Info("Player jumped!");
}
```

Save, start the game. If you see `[YourModName] My mod loaded!` in the BepInEx console, it's working.

## Lifecycle Hooks

Bark calls these functions in your script at specific times. **None are required** — define only what you need.

| Hook          | When               | Note                                                     |
|---------------|--------------------|----------------------------------------------------------|
| `onLoad()`    | Script loaded      | Called at main menu. Do **not** access World/Player here |
| `onEnable()`  | Script activated   | —                                                        |
| `onDisable()` | Script deactivated | —                                                        |
| `onUnload()`  | Script unloaded    | —                                                        |
| `onUpdate()`  | Every frame        | ***Don't do heavy work here***                           |

```js
function onLoad() {
    Log.info("Loaded");
}

function onEnable() {
    // Called when entering the game world. Safe to access Player here.
}
```

## Global Variables

Bark injects tool classes as global variables with PascalCase names matching the C# class names. Use them directly.

| Variable        | What It Does                                                                        | API Docs                                     |
|-----------------|-------------------------------------------------------------------------------------|----------------------------------------------|
| `BodyUtil`      | Body vitals (blood, hunger, temperature, consciousness...)                          | [Body System](script-api/body-system.md)     |
| `PlayerUtil`    | Player actions (teleport, pickup, alerts)                                           | [Player](script-api/player.md)               |
| `LimbUtil`      | Limb operations (fractures, dislocations, infections...)                            | [Limbs](script-api/limbs.md)                 |
| `InventoryUtil` | Inventory queries                                                                   | [Inventory & Items](script-api/inventory.md) |
| `ItemUtil`      | Item search, durability, repair                                                     | [Inventory & Items](script-api/inventory.md) |
| `MoodleUtil`    | Status effect apply, remove, query                                                  | [Custom Status](script-api/moodle.md)        |
| `SkillUtil`     | Skill XP/levels                                                                     | [Skills](script-api/skills.md)               |
| `WorldUtil`     | World editing (place blocks, items)                                                 | [World Editing](script-api/world.md)         |
| `OptionsApi`    | Read mod config options                                                             | [Options](script-api/options.md)             |
| `Log`           | Logging: `Log.info()` / `Log.warning()` / `Log.error()`                             | [Logging](script-api/log.md)                 |
| `Locale`        | Localized text: `Locale.Get("key")`                                                 | [Localization](script-api/locale.md)         |
| `ScriptInfo`    | Current script metadata: `ScriptInfo.Id` / `ScriptInfo.Name` / `ScriptInfo.Version` | —                                            |

## Naming Conventions

All tool methods follow consistent prefixes. Once you know the prefix, you know what the method does. **No need to
memorize — let IDE autocomplete do the work.**

| Prefix                                             | Meaning       | Example                                        |
|----------------------------------------------------|---------------|------------------------------------------------|
| `Get*`                                             | Read a value  | `BodyUtil.GetHunger()` → hunger level          |
| `Set*`                                             | Set a value   | `BodyUtil.SetHunger(50)`                       |
| `Is*`                                              | Is it...?     | `BodyUtil.IsAlive()` → alive?                  |
| `Has*`                                             | Has...?       | `InventoryUtil.HasItem("axe")`                 |
| `Can*`                                             | Can do...?    | `BodyUtil.CanTakeNap()`                        |
| `Add*`                                             | Increment     | `SkillUtil.AddXP(100)`                         |
| `Remove*`                                          | Remove        | `BodyUtil.RemovePainkillers()`                 |
| `Place*`                                           | Place         | `WorldUtil.PlaceBlock("marble", 10, 5)`        |
| `Fill*`                                            | Fill area     | `WorldUtil.FillBlocks(0, 0, 10, 10, "marble")` |
| `Kill` / `Resurrect` / `Break` / `Mend` / `Repair` | Obvious verbs | `LimbUtil.Break(0)`                            |

**Get/Set pairs**: Everything with a Get usually has a matching Set — same name, just one more parameter.

```js
var hunger = BodyUtil.GetHunger();   // read
BodyUtil.SetHunger(hunger + 10);     // set
```

**Optional parameters**: C# methods with default values can omit them in scripts.

```js
// Alert(text, important, delay) — delay defaults to 0
PlayerUtil.Alert("You're injured");            // normal
PlayerUtil.Alert("Warning!", true);            // important
PlayerUtil.Alert("Evacuate now", true, 0.5);   // all specified
```

**Enums**: C# enums use integers in scripts.

```js
PlayerUtil.Teleport(100, 200);   // teleport to coordinates
```

## Event Hooks

Define a global function matching the hook name, and Bark calls it when the event fires. The function receives an
`event` object with event data (such as `event.ItemId` and `event.Item` for item hooks). You can omit the parameter
if you don't need it.

Full hook list at [Script Event Hooks](script-events.md). A few examples:

```js
function onPlayerJumpStart(event) {
    Log.info("Jump started!");
}

function onLimbBroken(event) {
    Log.info("Bone broken!");
}

function onItemUse(event) {
    Log.info("Used item: " + event.ItemId);
    // event.Item is the C# Item instance
}

function onWorldGenerated(event) {
    Log.info("World generated, time to cause trouble");
}
```

## Item Scripts

You can attach scripts to custom items, triggered by specific actions (use, attack, equip, etc.). Define a `main`
function that receives `(itemId, item, action)`:

```js
// arrow.js — registered in arrow.json under "attack"
function main(itemId, item, action) {
    // itemId: the item's ID string
    // item:    the C# Item instance (can access .condition, etc.)
    // action:  "attack" | "use" | "equip" | "unequip" | "use_in_hand" | "use_on_limb"
    itemUtil.Destroy(itemId);
    PlayerUtil.Alert("Bullseye!", true);
}
```

`main` accepts 0 to 3 parameters — JavaScript and Lua ignore extras:

```js
function main(itemId)           { /* only need the ID */ }
function main(itemId, item, action) { /* full context */ }
```

See [Custom Items](script-mod/item.md) for the JSON configuration format.

## Custom Tiles

Define custom tiles (ground/wall blocks) via JSON in the `Tile/` directory, with sprites in `Assets/Tile/`. Bark auto-scans and registers them with `TileRegistry`.

See [Custom Tiles](script-mod/tile.md).

## Custom Moodles

Define custom status effects (bleeding, poison, infection, etc.) via JSON in the `Moodle/` directory, then apply and query them with `MoodleUtil`. Three lifecycle phases: get (obtained), iterate (polling), lose (expired).

See [Custom Moodles](script-mod/moodle.md) for full documentation.

## Full Example

A working mod: auto-heal and injury alerts:

```js
// main.js

function onLoad() {
    Log.info("Auto-heal mod loaded");
}

function onWorldGenerated() {
    // Refill health every 5 seconds
    setInterval(function () {
        var hp = BodyUtil.GetBloodVolume();
        if (hp < 100) {
            BodyUtil.SetBloodVolume(hp + 5);
        }
    }, 5000);
}

function onLimbBroken() {
    PlayerUtil.Alert("Bone broken! Treat it now", true);
}
```

## Console Commands

Bark registers several in-game console commands useful for development.

| Command         | Alias | What It Does            |
|-----------------|-------|-------------------------|
| `script help`   | —     | Show command help       |
| `script reload` | `rs`  | Reload all script mods  |
| `script list`   | —     | List loaded script mods |

Usage: press `` ` `` in-game to open the console, type the command and press Enter.

```text
> script list
Script Mod List (3):
  Hello World-JavaScript v1.0.0 [JavaScript] (hello_world_js)
  My Mod v1.0.0 [Lua] (my_mod)

> rs
All script mods reloaded
```

> 💡 `script reload` is the most-used command. Modify scripts, hit `rs` — no need to restart the game.

### Registering Custom Commands

Define your own console commands via JSON in the `Command/` directory. When entered, they trigger the `onCommand` event in your script.

See [Script Commands](script-mod/command.md).

## Lua Notes

Lua users only need to note these differences. Everything else is the same.

**Entry file is always `main.lua`** — just put it in your mod folder.

**Method calls use `:` instead of `.`**:

```lua
-- JS: BodyUtil.GetHunger()
-- Lua: use colon
local hunger = BodyUtil:GetHunger()
```

**Function definitions**:

```lua
function onLoad()
    Log:info("Loaded")
end

function onPlayerJumpStart()
    Log:info("Jump started!")
end
```
