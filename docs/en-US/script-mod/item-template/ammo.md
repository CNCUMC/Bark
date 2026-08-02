***English*** | [简体中文](../../../zh-CN/script-mod/item-template/ammo.md)

← [Back to Template Overview](index.md)

# Ammo Template

`"type": "ammo"` — a bullet item. Presets the `9mmround` prefab (AmmoScript + stackable).

## Parameters

```json
{
  "template": {
    "type": "ammo",
    "ammo": true,
    "ammo_type": "7_62x51mm",
    "casing_type": "7_62x51mm_casing"
  }
}
```

| Parameter     | Type   | Default              | Description                                                                                          |
|---------------|--------|----------------------|------------------------------------------------------------------------------------------------------|
| `ammo`        | bool   | `true`               | Internal marker, **do not remove**                                                                   |
| `ammo_type`   | string | `"7_62x51mm"`        | Ammo caliber tag                                                                                     |
| `casing_type` | string | `"7_62x51mm_casing"` | Casing type spawned after firing. `null` or empty = fully consumed, no casing returned (e.g. shells) |

## Script-Side Queries

```js
AmmunitionTemplate.IsAmmo(itemId)       // → bool
AmmunitionTemplate.GetAmmoType(itemId)  // → "7_62x51mm"
AmmunitionTemplate.GetCasingType(itemId) // → "7_62x51mm_casing" or null
```
