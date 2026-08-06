***English*** | [简体中文](../../zh-CN/script-mod/item.md)

# Custom Items

> 💡 **Using `"template"` dramatically simplifies item JSON.**  
> Guns, magazines, ammo, casings, and more come with built-in templates. One line of `"template": { "type": "gun" }`
> auto-fills prefab, weight, durability, and a dozen other defaults.  
> See **[Item Template Documentation](./item-template/index.md)**.

---

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
  "use": [
    {
      "slot": [0, 1, 2, 3],
      "script": ["bandage123_use.js"]
    }
  ]
}
```

| Field                      | Type     | Default      | Notes                                              |
|----------------------------|----------|--------------|----------------------------------------------------|
| `full_name`                | string   | `""`         | Display name                                       |
| `description`              | string   | `""`         | Tooltip text                                       |
| `category`                 | string   | `""`         | Inventory category                                 |
| `weight`                   | float    | `0`          | Weight in kg                                       |
| `value`                    | int      | `0`          | Monetary value                                     |
| `tags`                     | string   | `""`         | Comma-separated tags                               |
| `sprite`                   | object   | —            | Sprite-related config (see below)                  |
| `origin_prefab`            | string   | `"geofruit"` | Fallback prefab for sprite size                    |
| `spawn`                    | object   | —            | World spawn / loot config (see below)              |
| `script`                   | object   | null         | Action → script mapping (see below)                |
| `custom_data`              | object   | null         | Arbitrary data for scripts to read                 |
| `spawn_components`         | string[] | null         | Component type names attached on spawn (see below) |
| `icon_animation_id`        | string   | null         | Inventory icon animation ID                        |
| `worn_sprite_animation_id` | string   | null         | Worn sprite animation ID                           |
| `held_sprite_offset`       | object   | null         | Held sprite offset `{ "x", "y" }`                  |
| `light`                    | object   | null         | Light config (see below)                           |
| `bandage`                  | object   | null         | Bandage config (see below)                         |
| `syringe`                  | object   | null         | Syringe config (see below)                         |
| `tool`                     | object   | null         | Tool/melee config (see below)                      |

> 📝 Item ID = `{modId}.{filename}` (namespaced format), e.g. mod `my_mod` with `bandage123.json` → ID
> `"my_mod.bandage123"`. Vanilla items (e.g. `bandage`) have no prefix. It is NOT a JSON field.

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
> `Head`, `UpTorso`, `DownTorso`, `UpArmF`, `DownArmF`, `HandF`, `UpArmB`, `DownArmB`, `HandB`, `ThighF`, `CrusF`,
> `FootF`, `ThighB`, `CrusB`, `FootB`.
> **`slot_id` and `desired_limb` are two independent concepts**: `slot_id` is an equipment slot identifier (e.g.
> `"back"`), while `desired_limb` is a body limb name. They cannot be used interchangeably.
> **If `slot_id` is empty** or **`desired_limb` is empty**, the wearable feature is disabled to prevent CUCoreLib
> internal NRE crashes.
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

### Light Fields

To make an item emit light (flashlight, emergency light, etc.):

```json
{
  "light": {
    "intensity": 1.2,
    "color": "#FFFFAA",
    "light_type": "Point",
    "x_offset": 0,
    "y_offset": 0,
    "point_light_inner_angle": 360,
    "point_light_inner_radius": 0,
    "point_light_outer_angle": 360,
    "point_light_outer_radius": 8
  }
}
```

| Field                            | Type   | Default     | Description                                                                                                                                                    |
|----------------------------------|--------|-------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `light.intensity`                | float  | `10`        | Light intensity (too high causes overlapping scatter that looks doubled; lower it based on radius)                                                             |
| `light.color`                    | string | `"#FFFFFF"` | Hex color (#RRGGBB)                                                                                                                                            |
| `light.light_type`               | string | `"Point"`   | Light type: `Point`/`Sprite`/`Global` etc.                                                                                                                     |
| `light.rotation`                 | float  | `-90`       | Light rotation angle. CUCoreLib 1.0.3's LightProperties has no Rotation field; Bark rotates the light child object (`Light`) directly when the item is created |
| `light.follow_mouse`             | bool   | `false`     | Whether the light follows the mouse                                                                                                                            |
| `light.light_on_zero_condition`  | bool   | `false`     | Whether the light stays on at zero condition                                                                                                                   |
| `light.x_offset`                 | float  | `0`         | Light horizontal offset                                                                                                                                        |
| `light.y_offset`                 | float  | `0`         | Light vertical offset                                                                                                                                          |
| `light.point_light_inner_angle`  | float  | `360`       | Point light inner cone angle                                                                                                                                   |
| `light.point_light_inner_radius` | float  | `0`         | Point light inner radius                                                                                                                                       |
| `light.point_light_outer_angle`  | float  | `360`       | Point light outer cone angle                                                                                                                                   |
| `light.point_light_outer_radius` | float  | `8`         | Point light outer radius                                                                                                                                       |

### Bandage Fields

To give an item bandage behavior (wound dressing, bleeding slowdown, etc.):

```json
{
  "bandage": {
    "effectiveness": 8,
    "skin_heal_amount": 8,
    "bandage_slow_amount": 18,
    "pain_reduction": 40,
    "bone_heal_timer_reduction": 5,
    "dislocation_timer_reduction": 5,
    "create_wrap_sprite": true,
    "wrap_sprite_path": "Special/bandageWrap",
    "wrap_sprite_color": "#FFFFFF",
    "minigame_color": "#E6E6E6"
  }
}
```

| Field                                 | Type   | Default                 | Description                       |
|---------------------------------------|--------|-------------------------|-----------------------------------|
| `bandage.effectiveness`               | float  | `8`                     | Treatment effectiveness           |
| `bandage.skin_heal_amount`            | float  | `8`                     | Skin heal amount                  |
| `bandage.bandage_slow_amount`         | float  | `18`                    | Bleeding slowdown amount          |
| `bandage.pain_reduction`              | float  | `40`                    | Pain reduction                    |
| `bandage.bone_heal_timer_reduction`   | float  | `5`                     | Fracture heal acceleration (s)    |
| `bandage.dislocation_timer_reduction` | float  | `5`                     | Dislocation heal acceleration (s) |
| `bandage.create_wrap_sprite`          | bool   | `true`                  | Whether to create wrap sprite     |
| `bandage.wrap_sprite_path`            | string | `"Special/bandageWrap"` | Wrap sprite path                  |
| `bandage.wrap_sprite_color`           | string | `"#FFFFFF"`             | Wrap sprite color (#RRGGBB)       |
| `bandage.minigame_color`              | string | `"#E6E6E6"`             | Minigame UI color (#RRGGBB)       |

### Syringe Fields

To make an item a syringe (extract/inject liquids):

```json
{
  "syringe": {
    "capacity": 100,
    "auto_fill": false,
    "amount_per_full_use": 100,
    "use_average_color": true,
    "minigame_color": "#FFFFFF"
  }
}
```

| Field                         | Type   | Default     | Description                  |
|-------------------------------|--------|-------------|------------------------------|
| `syringe.capacity`            | float  | `100`       | Max capacity (ml)            |
| `syringe.auto_fill`           | bool   | `false`     | Auto-fill on spawn           |
| `syringe.amount_per_full_use` | float  | `100`       | Amount consumed per full use |
| `syringe.use_average_color`   | bool   | `true`      | Use average color            |
| `syringe.minigame_color`      | string | `"#FFFFFF"` | Minigame UI color (#RRGGBB)  |

### Tool Fields

To make an item a melee/tool (swingable attack):

```json
{
  "tool": {
    "damage": 25,
    "structural_damage": 25,
    "attack_cooldown_multiplier": 0.66,
    "distance": 2.5,
    "knock_back": 270,
    "cooldown": 0.35,
    "attack_animation": "SwingAnim",
    "stamina_use": 0.5,
    "piercing": false,
    "swing_sounds": ["BSSwing1", "BSSwing2"],
    "volume": 0.5,
    "rotate_amount": 15.5,
    "physical_swing": true,
    "do_attack_animation": true,
    "metal_more_damage": false,
    "condition_loss_on_hit": 0.02
  }
}
```

| Field                             | Type     | Default       | Description                |
|-----------------------------------|----------|---------------|----------------------------|
| `tool.damage`                     | float    | `25`          | Damage                     |
| `tool.structural_damage`          | float    | `25`          | Structural damage          |
| `tool.attack_cooldown_multiplier` | float    | `0.66`        | Attack cooldown multiplier |
| `tool.distance`                   | float    | `2.5`         | Attack distance            |
| `tool.knock_back`                 | float    | `270`         | Knockback force            |
| `tool.cooldown`                   | float    | `0.35`        | Cooldown time              |
| `tool.attack_animation`           | string   | `"SwingAnim"` | Attack animation name      |
| `tool.stamina_use`                | float    | `0.5`         | Stamina use                |
| `tool.piercing`                   | bool     | `false`       | Whether piercing           |
| `tool.swing_sounds`               | string[] | 4 defaults    | Swing sound effects        |
| `tool.volume`                     | float    | `0.5`         | Volume                     |
| `tool.rotate_amount`              | float    | `15.5`        | Rotate amount              |
| `tool.physical_swing`             | bool     | `true`        | Physical swing             |
| `tool.do_attack_animation`        | bool     | `true`        | Play attack animation      |
| `tool.metal_more_damage`          | bool     | `false`       | Metal deals more damage    |
| `tool.condition_loss_on_hit`      | float    | `0.02`        | Durability loss on hit     |

### Spawn Components

Attach custom components (by type name) when the item spawns:

```json
{
  "spawn_components": ["MyMod.MyComponent", "MyMod.AnotherComponent"]
}
```

| Field              | Type     | Description                                                  |
|--------------------|----------|--------------------------------------------------------------|
| `spawn_components` | string[] | Component type full names (with namespace) attached on spawn |

> ⚠️ Component types must exist in the runtime assembly, otherwise they are ignored.

### Icon / Worn Animation and Held Offset

```json
{
  "icon_animation_id": "my_icon_anim",
  "worn_sprite_animation_id": "my_worn_anim",
  "held_sprite_offset": { "x": 2, "y": -1 }
}
```

| Field                      | Type   | Description                                     |
|----------------------------|--------|-------------------------------------------------|
| `icon_animation_id`        | string | Inventory icon animation ID                     |
| `worn_sprite_animation_id` | string | Worn sprite animation ID                        |
| `held_sprite_offset`       | object | Held sprite offset `{ "x": float, "y": float }` |

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

| Key           | Type                  | Trigger                                   |
|---------------|-----------------------|-------------------------------------------|
| `attack`      | string[]              | Melee attack while holding this item      |
| `use_on_limb` | string[]              | Used on a specific limb                   |
| `has`         | string[]              | Item is in player's backpack (polled)     |
| `in_hand`     | string[]              | Item is picked up (taken in hand)         |
| `not_in_hand` | string[]              | Item is dropped (removed from hand)       |
| `durability`  | ConditionTriggerDef[] | Condition crosses a threshold (see below) |

### use (Top-Level, Active)

`use` is an array of entries, each specifying the use origin and scripts. `use` is mutually exclusive with `wearable` —
an item is either wearable or usable, not both.

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
| `wearing` | string[] | Continuously polled while worn       |

```json
{
  "wearable": {
    "slot_id": "back",
    "desired_limb": "Head",
    "equip": ["helmet_equip.js"],
    "unequip": ["helmet_unequip.js"],
    "attack": ["helmet_attack.js"],
    "damage": ["helmet_damage.js"],
    "wearing": ["helmet_wearing.js"]
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

| Key        | Type     | Notes                                        |
|------------|----------|----------------------------------------------|
| `operator` | string   | Comparison: `"<"`/`"<="`/`"=="`/`">="`/`">"` |
| `value`    | float    | Threshold (0.0–1.0 percentage)               |
| `script`   | string[] | Script file paths                            |

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
    // itemId: "my_mod.bandage123"
    // item:    C# Item instance
    // action:  "use"

    Player.Alert("Applied bandage", true);
}
```

The `main` function receives three base arguments, plus three additional arguments in condition trigger contexts:

| Parameter        | Type   | Description                                                         |
|------------------|--------|---------------------------------------------------------------------|
| `itemId`         | string | The item's ID                                                       |
| `item`           | Item   | C# Item instance (null if unavailable)                              |
| `action`         | string | Action: `"use"`, `"attack"`, `"equip"`, etc.                        |
| `currentValue`   | float  | **[Condition trigger]** Current percentage (0.0~1.0)                |
| `thresholdValue` | float  | **[Condition trigger]** Trigger threshold (0.0~1.0)                 |
| `operator`       | string | **[Condition trigger]** Operator (`"<"` `"<="` `"=="` `">="` `">"`) |

The last three arguments are only passed during `durability`, `capacity_trigger`, and `charge_trigger` callbacks; they
are `null` otherwise.

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

- If two mods both define items with the same name, the last loaded wins (but the namespace prefix effectively prevents
  such conflicts)
- JSON fields use `snake_case` naming (words are all lowercase, connected by underscores `_`)
- If an item only needs a script (no custom sprite), you can omit `Assets/Item/` entirely
- `script reload`/`sr` reloads item definitions — no restart needed during development
