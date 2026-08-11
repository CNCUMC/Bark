[简体中文](../../../zh-CN/script-mod/item-template/plush.md) | ***English***

← [Back to Templates](index.md)

# Plush Template

`"type": "plush"` — a squeaky toy plush. The template presets the `plushie` prefab and all of its default properties
(weight, value, recognition, tags, etc.), and plays a squeak when used. **Sound uses Bark's Audio properties**: you can
set a custom squeak sound via `squeak_sound`; otherwise the game default is used.

## Quick Start

```json
{
  "template": {
    "type": "plush"
  }
}
```

That single line gives you a full `plushie` equivalent.

## Plush-Specific Parameters (inside `template`)

```json
{
  "template": {
    "type": "plush",
    "plush": true,
    "squeak_sound": "Assets/Audio/plush_squeak.wav"
  }
}
```

| Parameter      | Type   | Default | Description                                                                                                                                       |
|----------------|--------|---------|---------------------------------------------------------------------------------------------------------------------------------------------------|
| `plush`        | bool   | `true`  | Internal marker, **do not remove**                                                                                                                |
| `squeak_sound` | string | `""`    | Custom squeak sound (**Bark Audio property**). Relative to the mod root; bare filenames auto-prepend `Assets/Audio/`. Empty uses the game default |

When `squeak_sound` is set, Bark loads and plays the custom sound via `AudioManager` (`.wav`/`.mp3`/`.aif`, same as other
sound conventions), replacing the game default `PlushScript` squeak.

## Overridable Top-Level Fields

The plush template presets these `ItemInfo` fields, which you can override at the item JSON top level:

```json
{
  "category": "utility",
  "weight": 0.15,
  "value": 5,
  "recognition": 6,
  "tags": "belttool",
  "destroy_at_zero_condition": true,
  "sprite": {
    "slot_rotation": 0
  }
}
```

| Field                       | Type   | Default    | Description                       |
|-----------------------------|--------|------------|-----------------------------------|
| `category`                  | string | `utility`  | Category                          |
| `weight`                    | float  | `0.15`     | Weight                            |
| `value`                     | int    | `5`        | Value                             |
| `recognition`               | int    | `6`        | Recognition level                 |
| `tags`                      | string | `belttool` | Tags, comma-separated             |
| `destroy_at_zero_condition` | bool   | `true`     | Destroy when condition hits zero  |
| `sprite.slot_rotation`      | float  | `0`        | Inventory slot rotation (degrees) |

## Examples

### Simplest Plush

```json
{
  "full_name": "Brown Bear Plush",
  "description": "A soft brown bear. Squeeze it and it squeaks.",
  "category": "Toys",
  "template": { "type": "plush" }
}
```

Uses the game default squeak sound.

### Custom Squeak Sound

```json
{
  "full_name": "Rubber Duck",
  "description": "A rubber duck that quacks when squeezed.",
  "category": "Toys",
  "weight": 0.1,
  "template": {
    "type": "plush",
    "squeak_sound": "duck_squeak.wav"
  }
}
```

Put `duck_squeak.wav` in the mod's `Assets/Audio/`. Bark plays it via `AudioManager` instead of the default squeak.

## Script Integration

The squeak is handled automatically by the template — no script required. For extra logic on use (buffs, other events),
listen to the global event [`onPlushSqueak`](../../script-events.md) or define a `use` script.
