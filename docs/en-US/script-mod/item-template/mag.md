***English*** | [简体中文](../../../zh-CN/script-mod/item-template/mag.md)

← [Back to Template Overview](index.md)

# Magazine Template

`"type": "mag"` — a magazine item. Presets the `riflemagazine` prefab (AmmoScript + container properties).

## Parameters

```json
{
  "template": {
    "type": "mag",
    "mag": true,
    "mag_type": "ak_mag",
    "ammo_type": "7_62x51mm",
    "capacity": 30,
    "max_weight": 0.5
  }
}
```

| Parameter    | Type   | Default           | Description                         |
|--------------|--------|-------------------|-------------------------------------|
| `mag`        | bool   | `true`            | Internal marker, **do not remove**  |
| `mag_type`   | string | `"rifle_mag"`     | Magazine type tag, matched by guns  |
| `ammo_type`  | string | `"7_62x51mm"`     | Accepted ammo caliber               |
| `capacity`   | int    | `15`              | Magazine capacity (how many rounds) |
| `max_weight` | float  | `capacity × 0.03` | Container max weight limit          |

## Script-Side Queries

Magazine items are auto-registered in the `MagTemplate` registry:

```js
MagTemplate.IsMag(itemId)          // → bool
MagTemplate.GetMagData(itemId)     // → { MagType, AmmoType, Capacity, MaxWeight }
MagTemplate.GetMagType(itemId)     // → "ak_mag"
MagTemplate.GetAmmoType(itemId)    // → "7_62x51mm"
MagTemplate.GetCapacity(itemId)    // → 30
```
