***English*** | [简体中文](../zh-CN/script-mod.md)

# Script Development

Bark supports writing mods in JavaScript or Lua. This guide uses JavaScript for examples. Lua users see
the [Lua Notes](#lua-notes) section.

## Creating a Script

Create your own folder under `ScriptMod/Mods/` with a `mod.json`. An entry script is **optional**:

```
ScriptMod/Mods/
  MyMod/
    mod.json
    main.js        ← Entry file: main.js / main.mjs / main.lua (optional)
```

Language is detected from the entry file extension — `main.js` = JS mod, `main.lua` = Lua mod.

### Data-only Mods

A mod without any entry script (`main.js` / `main.mjs` / `main.lua`) is treated as a **data-only mod**.
It is still loaded and can provide JSON content (items, tiles, recipes, moodles, commands) via the
`Assets/` folders, but no script engine is started and no lifecycle hooks run.

```
ScriptMod/Mods/
  MyDataMod/
    mod.json
    Assets/
      Item/
        my_item.json
      Recipe/
        my_recipe.json
```

You can also combine both: a mod with an entry script may additionally define JSON content.

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

| Field          | Type   | Required | Description                                                                       |
|----------------|--------|----------|-----------------------------------------------------------------------------------|
| `id`           | string | Yes      | Unique ID, use snake_case, don't conflict with others                             |
| `name`         | string | Yes      | Display name                                                                      |
| `version`      | string | Yes      | Semver, e.g. `"1.0.0"`                                                            |
| `author`       | object | No       | Contributors, key = role (code, art, etc.), value = name                          |
| `description`  | string | No       | Mod description                                                                   |
| `bark_version` | string | No       | Required Bark version (semver range)                                              |
| `game_version` | string | No       | Compatible game version (semver range)                                            |
| `dependencies` | array  | No       | Dependent mods, format `[{"id": "some_mod", "version": "1.0.0"}]`                 |

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

Bark injects tool classes as global variables with PascalCase names matching the C# class names (minus the `Util`
suffix). Use them directly.

| Variable     | What It Does                                                                        | API Docs                                     |
|--------------|-------------------------------------------------------------------------------------|----------------------------------------------|
| `Body`       | Body vitals (blood, hunger, temperature, consciousness...)                          | [Body](script-api/body.md)                   |
| `Player`     | Player actions (teleport, pickup, alerts)                                           | [Player](script-api/player.md)               |
| `Limb`       | Limb operations (fractures, dislocations, infections...)                            | [Limbs](script-api/limbs.md)                 |
| `Inventory`  | Inventory queries                                                                   | [Inventory & Items](script-api/inventory.md) |
| `Item`       | Item search, durability, repair                                                     | [Inventory & Items](script-api/inventory.md) |
| `Moodle`     | Status effect apply, remove, query                                                  | [Custom Status](script-api/moodle.md)        |
| `Skill`      | Skill XP/levels                                                                     | [Skills](script-api/skills.md)               |
| `World`      | World editing (place blocks, items)                                                 | [World Editing](script-api/world.md)         |
| `OptionsApi` | Read mod config options                                                             | [Options](script-api/options.md)             |
| `Log`        | Logging: `Log.info()` / `Log.warning()` / `Log.error()`                             | [Logging](script-api/log.md)                 |
| `Locale`     | Localized text: `Locale.Get("key")`                                                 | [Localization](script-api/locale.md)         |
| `ScriptInfo` | Current script metadata: `ScriptInfo.Id` / `ScriptInfo.Name` / `ScriptInfo.Version` | —                                            |

## Naming Conventions

All tool methods follow consistent prefixes. Once you know the prefix, you know what the method does. **No need to
memorize — let IDE autocomplete do the work.**

| Prefix                                             | Meaning       | Example                                    |
|----------------------------------------------------|---------------|--------------------------------------------|
| `Get*`                                             | Read a value  | `Body.GetHunger()` → hunger level          |
| `Set*`                                             | Set a value   | `Body.SetHunger(50)`                       |
| `Is*`                                              | Is it...?     | `Body.IsAlive()` → alive?                  |
| `Has*`                                             | Has...?       | `Inventory.HasItem("axe")`                 |
| `Can*`                                             | Can do...?    | `Body.CanTakeNap()`                        |
| `Add*`                                             | Increment     | `Skill.AddXP(100)`                         |
| `Remove*`                                          | Remove        | `Body.RemovePainkillers()`                 |
| `Place*`                                           | Place         | `World.PlaceBlock("marble", 10, 5)`        |
| `Fill*`                                            | Fill area     | `World.FillBlocks(0, 0, 10, 10, "marble")` |
| `Kill` / `Resurrect` / `Break` / `Mend` / `Repair` | Obvious verbs | `Limb.Break(0)`                            |

**Get/Set pairs**: Everything with a Get usually has a matching Set — same name, just one more parameter.

```js
var hunger = Body.GetHunger();   // read
Body.SetHunger(hunger + 10);     // set
```

**Optional parameters**: C# methods with default values can omit them in scripts.

```js
// Alert(text, important, delay) — delay defaults to 0
Player.Alert("You're injured");            // normal
Player.Alert("Warning!", true);            // important
Player.Alert("Evacuate now", true, 0.5);   // all specified
```

**Enums**: C# enums use integers in scripts.

```js
Player.Teleport(100, 200);   // teleport to coordinates
```

## Event Hooks

Define a global function matching the hook name, and Bark calls it when the event fires. The function receives an
`event` object with event data (such as `event.ItemId` and `event.Item` for item hooks). You can omit the parameter if
you don't need it.

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
    Item.Destroy(itemId);
    Player.Alert("Bullseye!", true);
}
```

`main` accepts 0 to 3 parameters — JavaScript and Lua ignore extras:

```js
function main(itemId)           { /* only need the ID */ }
function main(itemId, item, action) { /* full context */ }
```

See [Custom Items](script-mod/item.md) for the JSON configuration format.

## Custom Tiles

Define custom tiles (ground/wall blocks) via JSON in the `Tile/` directory, with sprites in `Assets/Tile/`. Bark
auto-scans and registers them with `TileRegistry`.

See [Custom Tiles](script-mod/tile.md).

## Custom Moodles

Define custom status effects (bleeding, poison, infection, etc.) via JSON in the `Moodle/` directory, then apply and
query them with `Moodle`. Three lifecycle phases: get (obtained), iterate (polling), lose (expired).

See [Custom Moodles](script-mod/moodle.md) for full documentation.

## Custom Item Templates

Templates are preset groups of item properties. Guns, magazines, ammo, casings, and more come with built-in templates — reference them via the `"template"` field to dramatically simplify item JSON. You can also register your own templates in the `item-template/` directory.

See [Item Templates](script-mod/item-template/index.md).

## Custom Audio

Place custom audio files under your mod's `Assets/Audio/`. Supported formats include `.wav`, `.mp3`, `.aif` and more. Gun templates accept `fire_sound` / `rack_sound` / `unrack_sound` as paths relative to the mod root (bare filenames auto-prepend `Assets/Audio/`); `AudioManager` handles loading and caching automatically.

See [Custom Audio](script-mod/audio.md).

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
        var hp = Body.GetBloodVolume();
        if (hp < 100) {
            Body.SetBloodVolume(hp + 5);
        }
    }, 5000);
}

function onLimbBroken() {
    Player.Alert("Bone broken! Treat it now", true);
}
```

## Console Commands

Bark registers several in-game console commands useful for development.

| Command         | Alias  | What It Does                                       |
|-----------------|--------|----------------------------------------------------|
| `script help`   | —      | Show command help                                  |
| `script reload` | `sr`   | Reload all script mods                             |
| `script list`   | —      | List loaded script mods                            |
| `script spawn`  | `basp` | Spawn a Bark item by its registered ID             |
| `script tile`   | `bast` | Place a Bark tile by its registered ID             |
| `script moodle` | `basm` | Apply a Bark moodle by its key                     |
| `script detail` | `scd`  | Show a mod's metadata and registered-content stats |

Usage: press `` ` `` in-game to open the console, type the command and press Enter.

```text
> script list
Script Mod List (3):
  Hello World-JavaScript v1.0.0 [JavaScript] (hello_world_js)
  My Mod v1.0.0 [Lua] (my_mod)

> sr
All script mods reloaded
```

> 💡 `script reload` is the most-used command. Modify scripts, hit `sr` — no need to restart the game.

### Spawning / Placing Bark Content

These commands create content that was registered by Bark (items, tiles, moodles from any script mod or C# mod).
They only accept **Bark-registered IDs** — vanilla CCL items/tiles are still spawned via the game's `cuspawn` / `settile`.

The content ID is the full registered name, formatted as `modid.entryname` (e.g. `hello_world_js.ak47`).

Press `Tab` to auto-complete, and the candidates are **filtered by subcommand type**: `script spawn` only suggests
items, `script tile` only tiles, `script moodle` only moodle keys, and `script detail` / `scd` only mod IDs
(IDs are never mixed together). The completion list refreshes after `script reload`.

**Spawn an item** — forwards to CCL `cuspawn`, arguments in order: `[id] [position] [condition] [count]`.

```text
> basp hello_world_js.ak47
> script spawn hello_world_js.improved_headlamp 100 1
```

**Place a tile** — the Bark string tile ID is converted to a CCL tile index, then forwarded to CCL `settile`
(`[tileIndex] [position]`).

```text
> bast hello_world_js.marble
> script tile hello_world_js.marble 12,34
```

**Apply a moodle** — applies a registered moodle to the player. CCL has no equivalent command, so Bark calls
`MoodleUtil.ApplyMoodle` directly. Arguments: `[moodleKey] [holdSeconds]` (`holdSeconds` optional, defaults to the
JSON-defined duration).

```text
> basm hello_world_js.bleeding
> script moodle hello_world_js.bleeding 30
```

**Inspect a mod's registered content** — use `script detail <id>` (alias `scd <id>`) to query a loaded mod's
`mod.json` metadata and how many items / tiles / recipes / moodles it actually registered. Output includes: author,
description, required Bark version, game version, repository, dependency list, and per-type content counts.

```text
> scd hello_world_js
Content of 'hello_world_js' v1.0.0 [JavaScript]:
  Author: author: Jimmy
  Description: A hello-world demo mod
  Requires Bark: >=2.3.0
  Game version: >=1.0.0
  Repository: https://github.com/user/hello_world
  Dependencies:
    - bark_utils (>=1.0.0)
  Items: 3
  Tiles: 2
  Recipes: 1
  Moodles: 0
```

### Registering Custom Commands

Define your own console commands via JSON in the `Command/` directory. When entered, they trigger the `onCommand` event
in your script.

See [Script Commands](script-mod/command.md).

## Distributing as a Zip

To distribute your mod to players, zip the entire mod folder and drop it into `ScriptMod/Mods/`:

```
ScriptMod/Mods/
  MyMod.zip          ← packaged mod
```

Bark automatically extracts `.zip` files to the BepInEx cache directory (`Paths.CachePath`) on startup — no manual
unzipping required.

| Behavior           | Detail                                                                                              |
|--------------------|-----------------------------------------------------------------------------------------------------|
| First load         | Extracts once, skips on subsequent runs (unless zip is replaced)                                    |
| Directory priority | If both `Mods/MyMod/` and `MyMod.zip` exist with the same id, **the directory wins** (dev-friendly) |
| Deleting the zip   | Cache is auto-cleaned on next startup, no orphaned files                                            |
| Hot reload         | Not supported for zip mods — zips are distribution artifacts. To edit, extract to a directory       |

**Dev workflow**: develop as a loose directory, package as `.zip` for release. When both coexist, the directory
overrides the zip so your changes take effect immediately.

> 💡 Zip is for distribution only, not development. Use the directory format for hot reloading and editing.

## Lua Notes

Lua users only need to note these differences. Everything else is the same.

**Entry file is always `main.lua`** — just put it in your mod folder.

**Method calls use `:` instead of `.`**:

```lua
-- JS: Body.GetHunger()
-- Lua: use colon
local hunger = Body:GetHunger()
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

## Python Notes

PuerTS supports Python, but the Python runtime is relatively large (~22 MB) and has been put on hold. It will be
considered in a future major release if there is user demand. The related implementation has been archived in the source
repository's `TodoPython/` directory.
