***English*** | [简体中文](../../../zh-CN/script-mod/item-template/food.md)

← [Back to Templates Overview](index.md)

# Food Template

`"type": "food"` — Edible food items. The template presets the `geofruit` prefab with all default geofruit properties
(decay, weight, qualities, etc.), and calls `body.Eat()`, `body.Drink()`, and other native methods on consumption.

## Quick Reference

```json
{
  "template": {
    "type": "food"
  }
}
```

Just this gives you a complete geofruit equivalent. Override any field to customize.

## Food-Specific Parameters (inside template)

These control the eating effects, located inside the `template` object:

```json
{
  "template": {
    "type": "food",
    "food": true,
    "nutrition": 3.5,
    "weight_offset": 0.1,
    "hydration": 5.0,
    "happiness": 0.5,
    "condition_loss": 0.5,
    "eat_sound": "eatCrunch",
    "eat_good_voice": true
  }
}
```

| Parameter        | Type   | Default       | Description                                                   |
|------------------|--------|---------------|---------------------------------------------------------------|
| `food`           | bool   | `true`        | Internal marker — **do not remove**                           |
| `nutrition`      | float  | `3.5`         | Hunger restored, passed to `body.Eat(hunger, weight)`         |
| `weight_offset`  | float  | `0.1`         | Weight gain, passed to `body.Eat(hunger, weight)`             |
| `hydration`      | float  | `5.0`         | Thirst restored, passed to `body.Drink(thirst)`               |
| `happiness`      | float  | `0.5`         | Happiness gain: `body.happiness +=`                           |
| `condition_loss` | float  | `0.5`         | Durability consumed per use: `item.condition -=`              |
| `eat_sound`      | string | `"eatCrunch"` | Sound effect name for `Sound.Play()`. Empty string = no sound |
| `eat_good_voice` | bool   | `true`        | Whether to trigger `body.talker.EatGood()` voice line         |

## Overridable General Fields (top-level)

The food template presets these `ItemInfo` fields. Override them at the top level of your item JSON:

```json
{
  "weight": 0.75,
  "value": 1,
  "ignore_depression": false,
  "recognition": 3,
  "tags": "cangetwet",
  "destroy_at_zero_condition": true,
  "scale_weight_with_condition": true,
  "decay": {
    "info": 0,
    "minutes": 12.0
  },
  "sprite": {
    "slot_rotation": 45
  },
  "qualities": [
    { "type": "produce" }
  ]
}
```

| Field                         | Type   | Default              | Description                                                       |
|-------------------------------|--------|----------------------|-------------------------------------------------------------------|
| `weight`                      | float  | `0.75`               | Item weight                                                       |
| `value`                       | int    | `1`                  | Item value                                                        |
| `ignore_depression`           | bool   | `false`              | `true` = can be eaten while depressed (comfort food)              |
| `recognition`                 | int    | `3`                  | Recognition level                                                 |
| `tags`                        | string | `cangetwet`          | Comma-separated tags                                              |
| `destroy_at_zero_condition`   | bool   | `true`               | `true` = destroy item when condition reaches 0                    |
| `scale_weight_with_condition` | bool   | `true`               | `true` = weight scales proportionally with condition              |
| `decay.info`                  | int    | `0`                  | Decay type flags: `1` = NoDecayWithoutContainerItem (canned food) |
| `decay.minutes`               | float  | `12.0`               | Decay time in minutes                                             |
| `sprite.slot_rotation`        | float  | `45`                 | Inventory slot rotation in degrees                                |
| `qualities`                   | array  | `[{type:"produce"}]` | Crafting quality list                                             |

## Examples

### Simplest Food

```json
// Item/my_apple.json
{
  "full_name": "Apple",
  "description": "A juicy red apple.",
  "category": "Food",
  "template": { "type": "food" }
}
```

Fully identical to geofruit.

### Feast

```json
{
  "full_name": "Steak Dinner",
  "description": "A perfectly grilled steak with mashed potatoes.",
  "category": "Food",
  "weight": 0.5,
  "value": 8,
  "template": {
    "type": "food",
    "nutrition": 15,
    "hydration": 2,
    "happiness": 3.0,
    "condition_loss": 1.0
  }
}
```

### Canned Food (no decay)

```json
{
  "full_name": "Canned Beans",
  "description": "A sealed can of beans. Won't spoil until opened.",
  "category": "Food",
  "weight": 1.0,
  "value": 3,
  "decay": {
    "info": 1,
    "minutes": 0
  },
  "template": {
    "type": "food",
    "nutrition": 8,
    "hydration": 4
  }
}
```

`decay.info = 1` (NoDecayWithoutContainerItem) means the can only rots once spilled out of a container.

### Comfort Food

```json
{
  "full_name": "Comfort Candy",
  "description": "So sweet it makes even the darkest days brighter.",
  "category": "Food",
  "weight": 0.1,
  "value": 2,
  "ignore_depression": true,
  "template": {
    "type": "food",
    "nutrition": 1,
    "happiness": 5.0,
    "condition_loss": 0.25
  }
}
```

## Script Integration

The entire eating flow is handled by C# `useAction` — no script required. To add extra logic (buffs, VFX, etc.) before
or after eating, define `use` scripts in your item JSON:

```json
{
  "use": [
    {
      "script": ["onUse", "Args", "Enum.Use"],
      "condition": "normal"
    }
  ]
}
```

Note: defining a `use` array will override the auto Eat/Drink effects — you'll need to call them manually from script.
If you only want additional side effects, prefer `onUse` hooks (currently, `use` scripts and template useAction are
mutually exclusive; this may be merged in a future version).

## Comparison with geofruit

| Property                     | geofruit  | Food Template Default |
|------------------------------|-----------|-----------------------|
| Prefab                       | geofruit  | geofruit              |
| Inventory rotation           | 45°       | 45°                   |
| Usable                       | ✓        | ✓                    |
| Decay time (min)             | 12        | 12                    |
| Destroy at zero condition    | ✓        | ✓                    |
| Weight                       | 0.75      | 0.75                  |
| Weight scales with condition | ✓        | ✓                    |
| `body.Eat(hunger)`           | 3.5       | 3.5                   |
| `body.Eat(weight)`           | 0.1       | 0.1                   |
| `body.Drink(thirst)`         | 5         | 5                     |
| `body.happiness`             | +0.5      | +0.5                  |
| `item.condition`             | -0.5      | -0.5                  |
| Eating sound                 | eatCrunch | eatCrunch             |
| "Good food" voice            | ✓        | ✓                    |
| Crafting quality             | produce   | produce               |
