***English*** | [简体中文](../../../zh-CN/script-mod/item-template/casing.md)

← [Back to Template Overview](index.md)

# Casing Template

`"type": "casing"` — a casing item. Drops after firing, can be collected and reloaded.

## Parameters

```json
{
  "template": {
    "type": "casing",
    "casing": true,
    "casing_type": "7_62x51mm_casing"
  }
}
```

| Parameter     | Type   | Default              | Description                                      |
|---------------|--------|----------------------|--------------------------------------------------|
| `casing`      | bool   | `true`               | Internal marker, **do not remove**               |
| `casing_type` | string | `"7_62x51mm_casing"` | Casing type tag, matched by ammo's `casing_type` |

## Script-Side Queries

```js
CasingTemplate.IsCasing(itemId)        // → bool
CasingTemplate.GetCasingType(itemId)   // → "7_62x51mm_casing"
```
