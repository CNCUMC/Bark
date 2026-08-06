***English*** | [简体中文](../../zh-CN/script-mod/moodle.md)

# Custom Moodles

Define custom status effects (Moodles) like bleeding, infection, poison, etc. via JSON. Place JSON files in your mod's
`Moodle/` directory and sprite images in `Assets/Moodle/`.

The Moodle system is built on the game's status queue mechanism — each applied Moodle auto-expires after `hold_seconds`,
and disappears automatically, triggering the `onMoodleLose` event.

## Directory Layout

```
ScriptMod/Mods/
  MyMod/
    mod.json
    main.js
    Moodle/
      bleeding.json          ← custom Moodle
      poison.json
    Assets/Moodle/
      bleeding.png           ← custom sprite (optional)
      poison.png
```

## JSON Format

```json
{
  "intensity": 2,
  "name": "Severe Bleeding",
  "description": "Losing blood rapidly. Stop the bleeding now!",
  "critical": true,
  "important": true,
  "key": "severe_bleeding",
  "hold_seconds": 30,
  "icon_id": "bleeding",
  "script": {
    "get": ["bleeding_get.js"],
    "iterate": ["bleeding_tick.js"],
    "lose": ["bleeding_lose.js"]
  }
}
```

### Fields

| Field          | Type   | Default  | Description                                                                                                    |
|----------------|--------|----------|----------------------------------------------------------------------------------------------------------------|
| `intensity`    | int    | `1`      | Intensity level, affects icon display size and priority                                                        |
| `name`         | string | required | Display name, supports localization (matches `moodle.{key}.name` in locale)                                    |
| `description`  | string | `""`     | Tooltip text, supports localization (matches `moodle.{key}.description` in locale)                             |
| `critical`     | bool   | `false`  | Whether it's a critical condition, affects UI warning intensity                                                |
| `chipped_only` | bool   | `false`  | Consumable-only display mode                                                                                   |
| `important`    | bool   | `true`   | Show in main area (true) or sidebar (false)                                                                    |
| `key`          | string | auto     | Unique Moodle ID. Auto-generated from filename if omitted                                                      |
| `hold_seconds` | float  | `0.75`   | Duration in seconds, auto-expires afterwards                                                                   |
| `icon_id`      | string | null     | Built-in game icon ID, e.g. `"bleeding"`, `"hunger"`                                                           |
| `icon_asset`   | string | null     | Custom sprite path, e.g. `"Assets/Moodle/bleeding.png"`. Mutually exclusive with `icon_id` (icon_id preferred) |
| `sprite_scale` | float  | `0.5`    | Custom sprite scale. Larger = bigger sprite.1 = 16 PPU baseline                                                |
| `animated`     | bool   | `false`  | Use animated Moodle.Ignores `icon_id` / `icon_asset` when enabled                                              |
| `animation_id` | string | null     | Animation ID (only when `animated = true`)                                                                     |
| `script`       | object | null     | Script trigger definitions (see below)                                                                         |

## Icon Source Priority (highest first)

The icon is resolved in this order, stopping at the first match:

| Priority | Source                                | Description                                        |
|----------|---------------------------------------|----------------------------------------------------|
| 1        | `animated` + `animation_id`           | Animated moodle                                    |
| 2        | `icon_id`                             | Built-in game icon                                 |
| 3        | `icon_asset`                          | Custom sprite path                                 |
| 4        | Auto-lookup `Assets/Moodle/{key}.png` | Same auto-lookup as items (`Assets/Item/{id}.png`) |

If all fail, the Moodle is **skipped** with a warning log. This means you don't even need to specify any icon field —
just place a `.png` file with the same name as the key under `Assets/Moodle/`.

### Sprite Scaling

When using custom sprites (via `icon_asset` or auto-lookup), `sprite_scale` controls the sprite's rendering scale.
Baseline is 1 (16 PPU). Smaller values make the sprite larger:

```json
{
  "name": "Infection",
  "icon_asset": "infection.png",
  "sprite_scale": 0.5
}
```

Sprites load with Point filtering (no blur), suitable for pixel art assets.

## Moodle Key Generation

Key generation priority:

1. If `key` is specified in JSON, use it directly (lowercased)
2. Otherwise derive from filename (strip `.json`, convert to snake_case)

Example: file `Severe Bleeding.json` → auto-generated key `severe_bleeding`.

> ℹ️ When two mods define the same key, the last loaded wins. The key is the global unique reference — use it when
> calling `Moodle.ApplyMoodle()` from scripts.

## Moodle Scripts

The `script` field binds script files to Moodle's three lifecycle phases. When a phase triggers, Bark executes each
script in order.

### Lifecycle Phases

| Key       | Trigger                                    | Corresponding Event Hook |
|-----------|--------------------------------------------|--------------------------|
| `get`     | Moodle is applied to the player            | `onMoodleGet`            |
| `iterate` | While Moodle is active (polled every 0.5s) | `onMoodleIterate`        |
| `lose`    | Moodle expires or is removed               | `onMoodleLose`           |

### Script Paths

Paths are relative to the mod directory:

```json
{
  "script": {
    "get": ["Scripts/poison_get.js"],
    "iterate": ["Scripts/poison_tick.js"],
    "lose": ["Scripts/poison_lose.js"]
  }
}
```

Each phase's script list executes in order. Multiple scripts per phase are supported.

## Full Example

A poison Moodle using auto-lookup for the icon:

**`Moodle/poison.json`**:

```json
{
  "intensity": 3,
  "name": "Poisoned",
  "description": "Toxins are spreading through your body, causing continuous damage.",
  "critical": true,
  "hold_seconds": 60,
  "script": {
    "get": ["poison_get.js"],
    "iterate": ["poison_tick.js"]
  }
}
```

**`poison_get.js`** (at mod root):

```js
function main(moodleKey) {
    Log.Info('Player poisoned! key = ' + moodleKey);
    Player.Alert('You are poisoned! Find an antidote', true);
}
```

**`poison_tick.js`**:

```js
function main(moodleKey) {
    // Deal 2 damage each tick
    var hp = Body.GetBloodVolume();
    Body.SetBloodVolume(hp - 2);
}
```

**`Assets/Moodle/poison.png`**: Place under `Assets/Moodle/`, Bark auto-locates and loads it.

## Using Moodles in Scripts

Via the `Moodle` global variable:

```js
// Apply a Moodle (uses JSON-defined default duration)
Moodle.ApplyMoodle('poison');

// Apply with custom duration (5 seconds)
Moodle.ApplyMoodle('bleeding', 5);

// Force remove
Moodle.RemoveMoodle('poison');

// Query
if (Moodle.HasMoodle('poison')) {
    Log.Info('Player still poisoned');
}

// Get all active Moodles
var actives = Moodle.GetActiveMoodles();
Log.Info('Active moodle count: ' + Moodle.GetMoodleCount());

// Query properties
var intensity = Moodle.GetIntensity('poison');
var isCritical = Moodle.IsCritical('poison');
```

Full API reference at [Moodle](moodle.md).

## Notes

- `script reload` / `sr` reloads all Moodle definitions
- Same key overwrites: later-loaded mods override earlier ones
- JSON fields use `snake_case` naming
- Moodle script `main` functions receive one argument: `moodleKey` (string), the Moodle's unique identifier
- Polling interval is fixed at 0.5s — avoid heavy operations in `iterate` scripts
