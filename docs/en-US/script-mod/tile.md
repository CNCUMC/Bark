***English*** | [简体中文](../../zh-CN/script-mod/tile.md)

# Custom Tiles

Create custom tiles (ground/wall blocks) by placing JSON files in your mod's `Tile/` directory. Bark auto-scans and
registers them with CUCoreLib's `TileRegistry` at mod load time.

## Directory Layout

```
ScriptMod/Mods/
  MyMod/
    mod.json
    main.js
    Tile/
      marble.json           ← custom tile
      mahogany.json
    Assets/Tile/
      marble.png            ← tile sprite ({id}.png)
      mahogany.png
```

## Tile Index

The tile index is the registration parameter for `TileRegistry.Register(index, def)` — it is **not** a
`CustomTileDefinition` field. Declare indices centrally in `mod.json`'s `tiles` section, not in individual tile JSONs.

```json
{
  "id": "my_mod",
  "name": "My Mod",
  "version": "1.0.0",
  "tiles": {
    "marble": 50,
    "mahogany": 51
  }
}
```

| Field   | Type                 | Required | Notes                                                                   |
|---------|----------------------|----------|-------------------------------------------------------------------------|
| `tiles` | `object<string,int>` | No       | Tile index mapping. Key = filename (without `.json`), value = int >= 36 |

> ⚠️ Indices are written to save files. Pick them carefully and never change them. Indices must not conflict across
> mods.

## JSON Format

`Tile/{name}.json` fields map 1:1 to `CustomTileDefinition`:

```json
{
  "name": "Marble",
  "color": "#FFDDCC",
  "collider_type": "Grid",
  "health": 200,
  "hit_sound": "stone",
  "step_sound": "Gravel",
  "sleep_quality": "Good",
  "no_variation": false,
  "metallic": false,
  "toxicity": 0,
  "slippery": false,
  "spawn_amount": 0.5,
  "spawn_layers": [
    2,
    4
  ],
  "generation_style": [
    "Vein",
    "Outskirt"
  ],
  "drops": [
    {
      "id": "marble",
      "chance": 1.0,
      "condition_min": 0.8,
      "condition_max": 1.0
    }
  ],
  "custom_data": {
    "my_key": "my_value"
  },
  "script": {
    "on_place": [
      "scripts/marble_place.js"
    ],
    "on_exist": [
      "scripts/marble_exist.js"
    ],
    "on_damaging": [
      "scripts/marble_damage.js"
    ],
    "on_destroyed": [
      "scripts/marble_destroy.js"
    ]
  },
  "sprite_import_scale": 8.0
}
```

## Field Reference

### Basics

| Field           | Type   | Default      | Notes                                                |
|-----------------|--------|--------------|------------------------------------------------------|
| `name`          | string | `""`         | Display name (registered as `other.ID` locale entry) |
| `color`         | string | null (white) | Sprite tint, RGBA hex (`#FF0000` / `#FF0000FF`)      |
| `collider_type` | string | `"Grid"`     | Collider type: `Grid` / `Sprite` / `None`            |

### Properties

| Field           | Type   | Default     | Notes                                                              |
|-----------------|--------|-------------|--------------------------------------------------------------------|
| `health`        | float  | `100`       | HP (damage required to break)                                      |
| `hit_sound`     | string | `"stone"`   | Hit sound ID (`stone`, `metal`, `rock`, etc.)                      |
| `step_sound`    | string | `"Gravel"`  | Footstep sound ID (`Gravel`, `Rock`, etc.)                         |
| `sleep_quality` | string | null (none) | Sleep quality: `Excellent` / `Good` / `Mediocre` / `Bad` / `Awful` |
| `no_variation`  | bool   | `false`     | Disable vanilla visual variation (flip, etc.)                      |
| `metallic`      | bool   | `false`     | Enable metal damage behavior                                       |
| `toxicity`      | float  | `0`         | Radiation toxicity                                                 |
| `slippery`      | bool   | `false`     | Enable slippery surface                                            |

### Generation

| Field              | Type     | Default        | Notes                                                                             |
|--------------------|----------|----------------|-----------------------------------------------------------------------------------|
| `spawn_amount`     | float    | `0`            | Spawn multiplier. `0` disables, `1` = copper rate                                 |
| `spawn_layers`     | int[]    | null (all)     | Allowed game layers (1-based), e.g. `[2, 4, 5]`                                   |
| `generation_style` | string[] | null (default) | Shape style: `Vein` / `HeavyVeins` / `Singular` / `Stripe` / `Inner` / `Outskirt` |

### Drops

| Field                   | Type   | Notes                                        |
|-------------------------|--------|----------------------------------------------|
| `drops[].id`            | string | Dropped item ID                              |
| `drops[].chance`        | float  | Drop chance 0~1 (default `1`)                |
| `drops[].condition_min` | float  | Min durability of dropped item               |
| `drops[].condition_max` | float  | Max durability of dropped item (default `1`) |

### Extras

| Field                 | Type                         | Default | Notes                                                     |
|-----------------------|------------------------------|---------|-----------------------------------------------------------|
| `custom_data`         | `Dictionary<string, object>` | null    | Arbitrary data, read via `TileRegistry.TryGetCustomData`  |
| `script`              | object                       | null    | Tile script definition, see [Tile Scripts](#tile-scripts) |
| `sprite_import_scale` | float                        | `8.0`   | Sprite import scale                                       |

> 📝 Tile ID comes from the JSON filename (without extension), e.g. `marble.json` → ID is `marble`, consistent with item
> registration.

## Sprite Assets

| File pattern | Purpose     |
|--------------|-------------|
| `{id}.png`   | Tile sprite |

Sprites go in `Assets/Tile/`. **A sprite is required** — registration fails and skips if the sprite is missing.

## Tile Scripts

Each tile can define script files for four trigger actions via the `script` field, working identically
to [item scripts](item.md#item-scripts).

### Script Actions

| Action Key     | Trigger                      | Notes                                         |
|----------------|------------------------------|-----------------------------------------------|
| `on_place`     | When tile is placed          | Intercepted via `WorldGeneration.SetBlock`    |
| `on_exist`     | While tile exists (periodic) | Scans within radius 10 of the player every 1s |
| `on_damaging`  | When tile takes damage       | Intercepted via damage method patch           |
| `on_destroyed` | When tile is fully destroyed | Intercepted via `SetBlock` (index change)     |

Each action value is an array of script file paths, relative to the mod directory. Scripts execute in order.

### Script Function Signature

Scripts must export a `main` function receiving three parameters:

```js
// JS example
function main(tileId, context, action) {
    // tileId: tile ID (e.g. "marble")
    // context: { tileIndex, posX, posY } — tile index and world position
    // action: trigger action name (e.g. "on_place")
    console.log(tileId + " at (" + context.posX + ", " + context.posY + ") " + action);
}
```

```lua
-- Lua example
function main(tileId, context, action)
    -- context: CS.Bark.Tile.TileScriptContext object
    print(tileId .. " at (" .. context.PosX .. ", " .. context.PosY .. ") " .. action)
end
```

You can also expose data to scripts via `custom_data`:

- **JS scripts**: read `custom_data` via `CS.CUCoreLib.Registries.TileRegistry.TryGetCustomData(tileIndex)`

## Notes

- Indices must be declared in `mod.json`'s `tiles` section, and must be >= 36 (0~35 reserved for vanilla)
- If two mods register the same `tile_index`, the last loaded wins
- JSON fields use `snake_case`
- `script reload` / `rs` reloads tile definitions — no restart needed during development
