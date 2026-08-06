***English*** | [简体中文](../../../zh-CN/script-mod/item-template/clothing.md)

← [Back to Templates Overview](index.md)

# Clothing Template

`"type": "clothing"` — Wearable clothing items. The template presets the `geofruit` prefab with sensible defaults
(utility category, no decay, 0 armor/isolation, etc.), using the vanilla `wearable` system for equipping and
unequipping.

> 📘 The clothing template handles registration and default presets. Container (`container`) and battery (`battery`)
> features use the corresponding vanilla `ItemDef` fields. Non-vanilla runtime behaviors (headlamp, climbing, diving,
> etc.) are implemented by the script system.

## Quick Reference

```json
{
  "template": {
    "type": "clothing"
  }
}
```

Just this gives you a complete wearable base clothing item. However, `wearable.slot_id` and `wearable.desired_limb`
**must be filled** — if both are empty, the equipment system will not activate.

## Template Defaults

The clothing template provides the following vanilla `ItemDef` nested field presets inside `template`. Override them at
the top level of your item JSON:

### Wearable

| Field                                     | Type   | Default | Description                                                                              |
|-------------------------------------------|--------|---------|------------------------------------------------------------------------------------------|
| `wearable.slot_id`                        | string | `""`    | **Required**. Equipment slot ID (e.g., `UpTorso`, `Head`, `HandRight` — one of 15 limbs) |
| `wearable.desired_limb`                   | string | `""`    | **Required**. Target limb for wear visual                                                |
| `wearable.armor`                          | float  | `0.0`   | Armor value                                                                              |
| `wearable.isolation`                      | float  | `0.0`   | Temperature isolation value                                                              |
| `wearable.visual_offset`                  | int    | `5`     | Wear visual layer offset                                                                 |
| `wearable.can_be_held`                    | bool   | `false` | Whether items can still be held in hands while equipped                                  |
| `wearable.hit_durability_loss_multiplier` | float  | `0.0`   | Durability loss multiplier when hit                                                      |
| `wearable.sprite_offset_x`                | float  | `0.0`   | Wear sprite X offset                                                                     |
| `wearable.sprite_offset_y`                | float  | `0.0`   | Wear sprite Y offset                                                                     |

> ⚠️ The entire `wearable` section is ignored when both `slot_id` and `desired_limb` are empty.

### Container (Backpack)

Container activates when `container.max_weight > 0`. Available on all clothing template items.

| Field                           | Type     | Default | Description                                                  |
|---------------------------------|----------|---------|--------------------------------------------------------------|
| `container.max_weight`          | float    | `0.0`   | Max container carry weight (kg)                              |
| `container.max_weight_per_item` | float    | `0.0`   | Max weight per single item (0 = unlimited)                   |
| `container.encumbrance_mult`    | float    | `1.0`   | Encumbrance multiplier (weight coefficient for items inside) |
| `container.items_visible`       | bool     | `false` | Whether items are visually visible on the model              |
| `container.tag_restriction`     | string[] | `[]`    | Restrict to items with these tags                            |

### Battery

Battery system activates when `battery.max_allowed_charge > 0`.

| Field                        | Type   | Default | Description                        |
|------------------------------|--------|---------|------------------------------------|
| `battery.battery_type`       | string | `""`    | Compatible battery type ID         |
| `battery.max_allowed_charge` | float  | `0.0`   | Max battery capacity               |
| `battery.start_charge`       | float  | `0.0`   | Initial charge                     |
| `battery.spawn_with_battery` | bool   | `true`  | Spawn with a battery pre-installed |
| `battery.weight_reduction`   | bool   | `false` | Reduce weight when powered         |
| `battery.explode_at_zero`    | bool   | `false` | Explode when charge reaches zero   |
| `battery.preset`             | string | `""`    | Battery preset                     |

### General Fields

| Field                         | Type   | Default    | Description                               |
|-------------------------------|--------|------------|-------------------------------------------|
| `category`                    | string | `utility`  | Item category                             |
| `origin_prefab`               | string | `geofruit` | Base prefab                               |
| `weight`                      | float  | `1.0`      | Item weight (kg)                          |
| `value`                       | int    | `5`        | Item value                                |
| `destroy_at_zero_condition`   | bool   | `false`    | `true` = destroy when condition reaches 0 |
| `scale_weight_with_condition` | bool   | `false`    | `true` = weight scales with condition     |
| `recognition`                 | int    | `0`        | Recognition level                         |
| `tags`                        | string | `""`       | Comma-separated tags                      |
| `decay.info`                  | int    | `0`        | Decay type flags                          |
| `decay.minutes`               | float  | `0.0`      | Decay time in minutes, `0` = never decays |
| `sprite.slot_rotation`        | float  | `0.0`      | Inventory slot rotation in degrees        |

## Examples

### Simplest Clothing

```json
// Item/my_shirt.json
{
  "full_name": "T-Shirt",
  "description": "A plain cotton t-shirt.",
  "wearable": {
    "slot_id": "UpTorso",
    "desired_limb": "UpTorso"
  },
  "template": { "type": "clothing" }
}
```

### Armored Helmet

```json
// Item/helmet.json
{
  "full_name": "Tactical Helmet",
  "description": "A standard military helmet providing good head protection.",
  "weight": 1.5,
  "value": 50,
  "wearable": {
    "slot_id": "Head",
    "desired_limb": "Head",
    "armor": 10.0
  },
  "template": { "type": "clothing" }
}
```

### Backpack

```json
// Item/backpack.json
{
  "full_name": "Hiking Backpack",
  "description": "A sturdy hiking backpack that holds plenty of gear.",
  "weight": 1.5,
  "value": 30,
  "wearable": {
    "slot_id": "UpTorso",
    "desired_limb": "UpTorso"
  },
  "container": {
    "max_weight": 25.0,
    "max_weight_per_item": 5.0,
    "encumbrance_mult": 0.8,
    "items_visible": true
  },
  "template": { "type": "clothing" }
}
```

### Battery-Powered Gear

```json
// Item/night_vision_goggles.json
{
  "full_name": "Night Vision Goggles",
  "description": "Battery-powered goggles that let you see in the dark.",
  "weight": 0.8,
  "value": 120,
  "wearable": {
    "slot_id": "Head",
    "desired_limb": "Head"
  },
  "battery": {
    "max_allowed_charge": 100.0,
    "start_charge": 100.0,
    "spawn_with_battery": true
  },
  "template": { "type": "clothing" }
}
```

### Full Body Armor

```json
// Item/body_armor.json
{
  "full_name": "Heavy Body Armor",
  "description": "Full-body protective armor that greatly reduces incoming damage.",
  "weight": 8.0,
  "value": 200,
  "wearable": {
    "slot_id": "UpTorso",
    "desired_limb": "UpTorso",
    "armor": 25.0,
    "isolation": 5.0,
    "hit_durability_loss_multiplier": 0.3
  },
  "container": {
    "max_weight": 10.0,
    "encumbrance_mult": 1.2
  },
  "template": { "type": "clothing" }
}
```

## Script Integration

### Query API

| Method                                     | Returns              | Description                                   |
|--------------------------------------------|----------------------|-----------------------------------------------|
| `ClothingTemplate.IsClothing(itemId)`      | bool                 | Whether this item is a clothing template item |
| `ClothingTemplate.GetClothingData(itemId)` | ClothingData \| null | Get clothing registration record              |

### JS Example

```javascript
// Check if equipped item is clothing
global.onItemEquip = function(event) {
    if (clothingTemplate.IsClothing(event.itemId)) {
        log.info("Equipped clothing: " + event.itemId);
        // Read vanilla data like wearable.armor, container.max_weight via the game's item system
    }
};
```
