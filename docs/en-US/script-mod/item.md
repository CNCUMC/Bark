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

| Fields present        | Type             |
|-----------------------|------------------|
| `capacity`            | Liquid container |
| `color` (no `weight`) | Pure liquid      |
| Otherwise             | Normal item      |

### Common Fields

```json
{
  "full_name": "Bandage",
  "description": "Stops bleeding and heals minor wounds.",
  "category": "Medical",
  "weight": 0.2,
  "value": 15,
  "tags": "medical,bandage",
  "sprite": {
    "scale": 1.0
  },
  "spawn": {
    "drop_pool": ["Medical"],
    "frequency": 5
  },
  "script": {
    "use": [
      "bandage123_use.js"
    ]
  }
}
```

| Field           | Type   | Default      | Notes                                 |
|-----------------|--------|--------------|---------------------------------------|
| `full_name`     | string | `""`         | Display name                          |
| `description`   | string | `""`         | Tooltip text                          |
| `category`      | string | `""`         | Inventory category                    |
| `weight`        | float  | `0`          | Weight in kg                          |
| `value`         | int    | `0`          | Monetary value                        |
| `tags`          | string | `""`         | Comma-separated tags                  |
| `sprite`        | object | —            | Sprite-related config (see below)     |
| `origin_prefab` | string | `"geofruit"` | Fallback prefab for sprite size       |
| `spawn`         | object | —            | World spawn / loot config (see below) |
| `script`        | object | null         | Action → script mapping (see below)   |
| `custom_data`   | object | null         | Arbitrary data for scripts to read    |

> 📝 Item ID is the filename without `.json` (e.g. `bandage123.json` → ID `"bandage123"`). It is NOT a JSON field.

### Wearable Fields

To make an item wearable, add a `wearable` object:

```json
{
  "wearable": {
    "slot_id": "back",
    "desired_limb": "Head",
    "can_be_held": true,
    "armor": 0.3,
    "isolation": 0.1,
    "hit_durability_loss_multiplier": 1.0,
    "sorting_order": 0,
    "visual_offset": 5,
    "sprite_offset_x": 0,
    "sprite_offset_y": 0,
    "multi": {
      "FootF": {
        "sprite_offset_x": 2,
        "sprite_offset_y": 1
      }
    }
  }
}
```

| Field                                     | Type                        | Default | Notes                                                             |
|-------------------------------------------|-----------------------------|---------|-------------------------------------------------------------------|
| `wearable.slot_id`                        | string                      | `""`    | Equipment slot identifier (**required**, e.g. `"Head"`, `"back"`) |
| `wearable.desired_limb`                   | string                      | `""`    | Target limb for worn sprite (**required**, see valid list below)  |
| `wearable.can_be_held`                    | bool                        | `false` | Can also be held in hand when worn                                |
| `wearable.armor`                          | float                       | `0`     | Armor protection (0–1)                                            |
| `wearable.isolation`                      | float                       | `0`     | Thermal isolation (0–1)                                           |
| `wearable.hit_durability_loss_multiplier` | float                       | `0`     | Durability loss on hit multiplier                                 |
| `wearable.sorting_order`                  | int?                        | null    | Render sorting order for equipped sprite                          |
| `wearable.visual_offset`                  | int                         | `5`     | Visual layer offset                                               |
| `wearable.sprite_offset_x`                | float                       | `0`     | Horizontal offset for worn sprite                                 |
| `wearable.sprite_offset_y`                | float                       | `0`     | Vertical offset for worn sprite                                   |
| `wearable.multi`                          | object (limb name → offset) | null    | Extra limb sprites with offsets (uses `{itemId}_mw_{limb}.png`)   |

> ⚠️ **Valid limb names**: `wearable.desired_limb` must be one of the 15 game limbs (**required**):
> `Head`, `UpTorso`, `DownTorso`, `UpArmF`, `DownArmF`, `HandF`, `UpArmB`, `DownArmB`, `HandB`, `ThighF`, `CrusF`, `FootF`, `ThighB`, `CrusB`, `FootB`.
> **`slot_id` and `desired_limb` are two independent concepts**: `slot_id` is an equipment slot identifier (e.g. `"back"`), while `desired_limb` is a body limb name. They cannot be used interchangeably.
> **If `slot_id` is empty** or **`desired_limb` is empty**, the wearable feature is disabled to prevent CUCoreLib internal NRE crashes.
> Use `Limb.IsValidLimbName()` to validate names at runtime (see [Limb API](../script-api/limbs.md)).

### Container Fields

To make an item a container (backpack, pouch, etc.):

```json
{
  "container": {
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
  "battery": {
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

| Field            | Type             | Notes                             |
|------------------|------------------|-----------------------------------|
| `capacity`       | float            | Max liquid volume in ml           |
| `auto_fill`      | bool             | Auto-fill on spawn (default true) |
| `default_liquid` | object (id → ml) | Starting contents                 |

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

| Field             | Type   | Notes                   |
|-------------------|--------|-------------------------|
| `color`           | string | Hex color (#RRGGBB)     |
| `value_per_liter` | float  | Value per 1000ml        |
| `health_usable`   | bool   | Can be used for healing |
| `injectable`      | bool   | Can be injected         |

## Item Scripts

Item scripts are split into three layers:

- **`script`**: Passive state detection + condition triggers
- **`use`**: Active use (mutually exclusive with `wearable`)
- **`wearable`**: Equipment-related scripts

When an action fires, Bark runs each script and calls its `main(itemId, item, action)` function.

### script (Passive + Triggers)

| Key           | Type                  | Trigger                                       |
|---------------|-----------------------|-----------------------------------------------|
| `attack`      | string[]              | Melee attack while holding this item          |
| `use_on_limb` | string[]              | Used on a specific limb                       |
| `in_backpack` | string[]              | Item is in player's backpack (polled)         |
| `in_hand`     | string[]              | Item is picked up (taken in hand)             |
| `not_in_hand` | string[]              | Item is dropped (removed from hand)           |
| `durability`  | ConditionTriggerDef[] | Condition crosses a threshold (see below)     |

### use (Top-Level, Active)

`use` is an array of entries, each specifying the use origin and scripts. `use` is mutually exclusive with `wearable` — an item is either wearable or usable, not both.

```json
{
  "full_name": "Medkit",
  "category": "Medical",
  "weight": 0.3,
  "use": [
    { "slot": [0, 1, 2, 3],     "script": ["medkit_use.js"] },
    { "slot": ["hand"],          "script": ["medkit_hand.js"] },
    { "limb_slot": ["Head","HandF"], "script": ["medkit_limb.js"] }
  ]
}
```

| Key         | Type     | Notes                                                    |
|-------------|----------|----------------------------------------------------------|
| `slot`      | object[] | Inventory slot indices (int), `"hand"`=held, null/[]=all |
| `limb_slot` | string[] | Limb names, null/[]=all                                  |
| `script`    | string[] | Script file paths                                        |

### Scripts Inside wearable

| Key       | Type     | Trigger                              |
|-----------|----------|--------------------------------------|
| `equip`   | string[] | Equipped (put on)                    |
| `unequip` | string[] | Unequipped (taken off)               |
| `attack`  | string[] | Melee attack while wearing this item |
| `damage`  | string[] | Wearable item took damage            |

```json
{
  "wearable": {
    "slot_id": "Head",
    "desired_limb": "Head",
    "equip": ["helmet_equip.js"],
    "unequip": ["helmet_unequip.js"],
    "attack": ["helmet_attack.js"],
    "damage": ["helmet_damage.js"]
  }
}
```

### Condition Triggers (ConditionTriggerDef)

Reused by `durability`, `capacity_trigger`, and `charge_trigger`. Each entry:

```json
{
  "operator": "<=",
  "value": 0.3,
  "script": ["low_durability.js"]
}
```

| Key        | Type     | Notes                                              |
|------------|----------|----------------------------------------------------|
| `operator` | string   | Comparison: `"<"`/`"<="`/`"=="`/`">="`/`">"`     |
| `value`    | float    | Threshold (0.0–1.0 percentage)                     |
| `script`   | string[] | Script file paths                                  |

Edge-triggered: fires only once when the condition transitions from unsatisfied to satisfied, avoiding repeated calls.

### Container Capacity Trigger

```json
{
  "container": {
    "max_weight": 10,
    "capacity_trigger": [
      { "operator": ">=", "value": 0.8, "script": ["near_full.js"] }
    ]
  }
}
```

### Battery Charge Trigger

```json
{
  "battery": {
    "preset": "medium",
    "charge_trigger": [
      { "operator": "<=", "value": 0.1, "script": ["low_battery.js"] }
    ]
  }
}
```

### Script Path

Script paths in the `script` array are relative to the **mod directory**, not the JSON location. For example,
`"bandage123_use.js"` means `ModDir/bandage123_use.js`. You can organize scripts in subdirectories:

```json
{
  "script": {
    "use_on_limb": [
      "Scripts/bandage123_limb.js"
    ],
    "attack": [
      "Scripts/bandage123_attack.js"
    ]
  },
  "wearable": {
    "slot_id": "Head",
    "desired_limb": "Head",
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

    Player.Alert("Applied bandage", true);
}
```

The `main` function receives three base arguments, plus three additional arguments in condition trigger contexts:

| Parameter        | Type   | Description                                           |
|------------------|--------|-------------------------------------------------------|
| `itemId`         | string | The item's ID                                         |
| `item`           | Item   | C# Item instance (null if unavailable)                |
| `action`         | string | Action: `"use"`, `"attack"`, `"equip"`, etc.          |
| `currentValue`   | float  | **[Condition trigger]** Current percentage (0.0~1.0)  |
| `thresholdValue` | float  | **[Condition trigger]** Trigger threshold (0.0~1.0)   |
| `operator`       | string | **[Condition trigger]** Operator (`"<"` `"<="` `"=="` `">="` `">"`) |

The last three arguments are only passed during `durability`, `capacity_trigger`, and `charge_trigger` callbacks; they are `null` otherwise.

You can accept any subset — JavaScript and Lua ignore extra arguments:

```js
function main(itemId) { /* just the ID */
}

function main(itemId, item, action) { /* full context */
}

// Condition trigger example: durability drops below 30%
function main(itemId, item, action, currentValue, thresholdValue, operator) {
    Player.Alert(`Item durability ${currentValue} ${operator} ${thresholdValue}, triggered!`, true);
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
    Item.Destroy(itemId);
    Player.Alert("Bullseye!", true);
}
```

### Interaction with Global Hooks

Item scripts and [global event hooks](../script-events.md) work independently:

- **Item scripts** (`main`): only fire for this specific item's action. Use for per-item logic.
- **Global hooks** (`onItemUse`, `onItemAttack`, etc.): fire for any item. Use for mod-wide tracking.

Both can coexist — a single attack with the arrow triggers both `main()` in `arrow.js` AND `onItemAttack(event)`
in your mod's main script.

## Sprite Assets

| File pattern             | Purpose                          |
|--------------------------|----------------------------------|
| `{itemId}.png`           | Inventory / world sprite         |
| `{itemId}_worn.png`      | Worn (equipped) sprite           |
| `{itemId}_mw_{limb}.png` | Multi-worn sprite for extra limb |
| `{itemId}_fill.png`      | Liquid fill mask                 |

All sprites go in `Assets/Item/`. If `{itemId}.png` is missing, Bark falls back to `origin_prefab` *(i.e. the default
geofruit)* sprite.

> 💡 **Worn sprite fallback**: If `{itemId}_worn.png` is not provided for a wearable item, Bark automatically falls
> back to the main item texture (`{itemId}.png`) as the worn sprite. Only if both are missing is equipping blocked
> with a warning. This means you can skip `_worn.png` entirely for most items — the inventory sprite will be
> reused when equipped.

## Notes

- If two mods both define `"bandage123"`, the last loaded wins
- JSON fields use `snake_case` naming (words are all lowercase, connected by underscores `_`)
- If an item only needs a script (no custom sprite), you can omit `Assets/Item/` entirely
- `script reload`/`rs` reloads item definitions — no restart needed during development
