***English*** | [简体中文](../../zh-CN/script-mod/item.md)

# Custom Items

Define custom items, liquid containers, and pure liquids via JSON. Place the JSON files in your mod's `Item/`
directory and sprite images in `Assets/Item/`.

## Directory Layout

```
ScriptMod/Mods/
  MyMod/
    mod.json
    main.js
    Item/
      bandage123.json          ← custom item
      arrow.json            ← custom item with scripts
      potion.json           ← liquid container
      water.json            ← pure liquid
    Assets/Item/
      bandage123.png           ← item sprite (itemId.png)
      arrow.png
      potion.png
      potion_fill.png       ← liquid fill mask (itemId_fill.png)
```

## Item JSON Format

Item type is auto-detected from the JSON fields:

| Fields present          | Type          |
|-------------------------|---------------|
| `capacity`              | Liquid container |
| `color` (no `weight`)  | Pure liquid   |
| Otherwise               | Normal item   |

### Common Fields

```json
{
  "full_name": "Bandage",
  "description": "Stops bleeding and heals minor wounds.",
  "category": "Medical",
  "weight": 0.2,
  "value": 15,
  "tags": "medical,bandage",
  "sprite_scale": 1.0,
  "drop_pool": [
    "Medical"
  ],
  "spawn_frequency": 5,
  "script": {
    "use": [
      "bandage123_use.js"
    ]
  }
}
```

| Field            | Type      | Default           | Notes                                        |
|------------------|-----------|-------------------|----------------------------------------------|
| `full_name`      | string    | `""`              | Display name                                 |
| `description`    | string    | `""`              | Tooltip text                                 |
| `category`       | string    | `""`              | Inventory category                           |
| `weight`         | float     | `0`               | Weight in kg                                 |
| `value`          | int       | `0`               | Monetary value                               |
| `tags`           | string    | `""`              | Comma-separated tags                         |
| `sprite_scale`   | float     | `0`               | Sprite render scale                          |
| `origin_prefab`  | string    | `"geofruit"`      | Fallback prefab for sprite size              |
| `drop_pool`      | string[]  | null              | Loot table pools                             |
| `spawn_frequency`| int       | `0`               | World spawn weight                           |
| `script`         | object    | null              | Action → script mapping (see below)          |
| `custom_data`    | object    | null              | Arbitrary data for scripts to read           |

> 📝 Item ID is the filename without `.json` (e.g. `bandage123.json` → ID `"bandage123"`). It is NOT a JSON field.

### Wearable Fields

To make an item wearable, add:

```json
{
  "wearable": true,
  "wearable_can_be_held": true,
  "wear_slot_id": "head",
  "wearable_armor": 0.3,
  "wearable_isolation": 0.1
}
```

### Container Fields

To make an item a container (backpack, pouch, etc.):

```json
{
  "container_data": {
    "max_weight": 10,
    "max_weight_per_item": 5,
    "items_visible": true
  }
}
```

### Battery Fields

To make an item battery-powered:

```json
{
  "battery_data": {
    "preset": "medium",
    "spawn_with_battery": true
  }
}
```

Valid presets: `"small"` (50 charge), `"medium"` (100), `"large"` (300). Omit `preset` to define custom
`max_allowed_charge` and `start_charge`.

## Liquid Container

Add `capacity` to make an item hold liquids:

```json
{
  "full_name": "Water Bottle",
  "category": "Container",
  "weight": 0.3,
  "capacity": 1000,
  "default_liquid": {
    "water": 500
  }
}
```

| Field           | Type                   | Notes                                |
|-----------------|------------------------|--------------------------------------|
| `capacity`      | float                  | Max liquid volume in ml              |
| `auto_fill`     | bool                   | Auto-fill on spawn (default true)    |
| `default_liquid`| object (id → ml)       | Starting contents                    |

## Pure Liquid

Define with `color` and omit `weight`:

```json
{
  "color": "#4488FF",
  "description": "Fresh water.",
  "value_per_liter": 1,
  "health_usable": true
}
```

| Field              | Type   | Notes                    |
|--------------------|--------|--------------------------|
| `color`            | string | Hex color (#RRGGBB)      |
| `value_per_liter`  | float  | Value per 1000ml         |
| `health_usable`    | bool   | Can be used for healing  |
| `injectable`       | bool   | Can be injected          |

## Item Scripts

The `script` field binds script files to item actions. When the action fires, Bark runs each script and calls its
`main(itemId, item, action)` function.

### Supported Actions

| Key            | Trigger                           |
|----------------|-----------------------------------|
| `use`          | Used from inventory               |
| `use_in_hand`  | Used while held in hand           |
| `equip`        | Equipped (put on)                 |
| `unequip`      | Unequipped (taken off)            |
| `use_on_limb`  | Used on a specific limb           |
| `attack`       | Melee attack with this item       |

### Script Path

Script paths in the `script` array are relative to the **mod directory**, not the JSON location. For example,
`"bandage123_use.js"` means `ModDir/bandage123_use.js`. You can organize scripts in subdirectories:

```json
{
  "script": {
    "use": [
      "Scripts/bandage123_use.js"
    ],
    "equip": [
      "Scripts/bandage123_equip.js"
    ]
  }
}
```

### Script Function Signature

```js
// bandage123_use.js
function main(itemId, item, action) {
    // itemId: "bandage123"
    // item:    C# Item instance
    // action:  "use"

    PlayerUtil.Alert("Applied bandage", true);
}
```

The `main` function receives three arguments:

| Parameter  | Type     | Description                                 |
|------------|----------|---------------------------------------------|
| `itemId`   | string   | The item's ID                               |
| `item`     | Item     | C# Item instance (null if unavailable)      |
| `action`   | string   | Action: `"use"`, `"attack"`, `"equip"`, etc.|

You can accept any subset — JavaScript and Lua ignore extra arguments:

```js
function main(itemId) { /* just the ID */
}

function main(itemId, item, action) { /* full context */
}
```

### Complete Example

A custom arrow that destroys itself on attack:

**`Item/arrow.json`**:

```json
{
  "full_name": "Bark Arrow",
  "category": "Weapon",
  "weight": 0.05,
  "value": 3,
  "script": {
    "attack": [
      "arrow.js"
    ]
  }
}
```

**`arrow.js`** (at mod root):

```js
function main(itemId, item, action) {
    itemUtil.Destroy(itemId);
    PlayerUtil.Alert("Bullseye!", true);
}
```

### Interaction with Global Hooks

Item scripts and [global event hooks](../script-events.md) work independently:

- **Item scripts** (`main`): only fire for this specific item's action. Use for per-item logic.
- **Global hooks** (`onItemUse`, `onItemAttack`, etc.): fire for any item. Use for mod-wide tracking.

Both can coexist — a single attack with the arrow triggers both `main()` in `arrow.js` AND `onItemAttack(event)`
in your mod's main script.

## Sprite Assets

| File pattern                | Purpose                          |
|-----------------------------|----------------------------------|
| `{itemId}.png`              | Inventory / world sprite         |
| `{itemId}_worn.png`         | Worn (equipped) sprite           |
| `{itemId}_mw_{limb}.png`    | Multi-worn sprite for extra limb |
| `{itemId}_fill.png`         | Liquid fill mask                 |

All sprites go in `Assets/Item/`. If `{itemId}.png` is missing, Bark falls back to `origin_prefab` *(i.e. the default geofruit)* sprite.

## Notes

- If two mods both define `"bandage123"`, the last loaded wins
- JSON fields use `snake_case` naming (words are all lowercase, connected by underscores `_`)
- If an item only needs a script (no custom sprite), you can omit `Assets/Item/` entirely
- `script reload`/`rs` reloads item definitions — no restart needed during development
