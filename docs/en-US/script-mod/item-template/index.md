***English*** | [简体中文](../../../zh-CN/script-mod/item-template/index.md)

# Item Templates

Templates are preset groups of item properties. By referencing a template via the `"template"` field, you can dramatically simplify item JSON — the template provides defaults, and you only override what needs changing.

## Why Templates

Say you want to define an AK-47. The base game has no "assault rifle" item type. With templates, just write:

```json
{
  "full_name": "AK-47",
  "category": "Weapons",
  "template": { "type": "gun", "ammo_type": "7_62x51mm", "damage": 45, "mag_type": "ak_mag" }
}
```

The template automatically fills prefab, weight, durability, fire interval, recoil, and a dozen other defaults — zero configuration needed. Hand-writing the equivalent takes dozens of lines.

**Templates are not magic**: the merged result is identical to a pure hand-written JSON. Override any field by simply writing it.

## Directory Layout

Template JSON files go in your script mod's `item-template/` directory:

```
ScriptMod/Mods/
  MyMod/
    mod.json
    main.js
    Item/
      ak47.json
      ak_mag.json
      762_ammo.json
    item-template/
      templates.json        ← custom templates (optional)
```

The `item-template/` directory is optional. Omit it to use only Bark's built-in templates.

## Using Templates in Item JSON

Specify the template type and parameters in the item JSON's `template` field:

```json
// Item/ak47.json — a gun
{
  "full_name": "AK-47",
  "category": "Weapons",
  "template": {
    "type": "gun",
    "ammo_type": "7_62x51mm",
    "damage": 45,
    "mag_type": "ak_mag",
    "fire_sound": "Assets/Audio/ak47_shot.wav"
  }
}
```

| template field | Description |
|----------------|-------------|
| `type` | **Required**. Template type: `"gun"` `"mag"` `"ammo"` `"casing"` `"food"` |
| `fire_sound` etc. | Type parameters, fill as needed; unset fields use template defaults |

> ⚠️ `type` is the only required field. Not sure what parameters are available? Read on for each template type.

## Template Types

Bark ships four built-in template types:

| Type  | Purpose         | type value | Detailed Docs     |
|-------|-----------------|------------|-------------------|
| Gun   | Fireable weapon | `"gun"`    | [Gun Template](gun.md) |
| Magazine | Ammo container  | `"mag"`    | [Magazine Template](mag.md) |
| Ammo  | Bullets         | `"ammo"`   | [Ammo Template](ammo.md) |
| Casing | Post-fire drops | `"casing"` | [Casing Template](casing.md) |
| Food  | Edible items    | `"food"`   | [Food Template](food.md) |

## Custom Templates

Beyond Bark's built-in templates, you can register your own in `item-template/templates.json`:

**`item-template/templates.json`**:
```json
{
  "my_melee": {
    "template": {
      "type": "custom",
      "melee": true
    },
    "origin_prefab": "axe",
    "category": "Weapons",
    "weight": 1.5,
    "value": 50,
    "tags": "melee,weapon"
  }
}
```

Then reference it in an item:
```json
{
  "full_name": "Custom Machete",
  "template": { "type": "my_melee", "damage": 60 }
}
```

Registration methods:

| Method      | Description                                                                       |
|-------------|-----------------------------------------------------------------------------------|
| JSON file   | Define in `item-template/templates.json`: key = template name, value = default JSON |
| Script-side | `TemplateLoader.Register("name", jsonObj)` or `TemplateLoader.RegisterFromJson("name", jsonString)` |

## Hot Reload

After `script reload` / `rs`:

- All item JSONs are re-parsed, templates re-merged
- Existing gun instances in the game world auto-refresh **audio clips** and **barrel offsets** (hot-reloadable properties)
- Magazine, ammo, and casing registries auto-update

No game restart needed.
