[English](../../../en-US/script-mod/item-template/ammo.md) | ***简体中文***

← [返回模板总览](index.md)

# 弹药模板

`"type": "ammo"` — 弹药物品。预设 `9mmround` 预制体（AmmoScript + 可堆叠）。

## 参数

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

| 参数          | 类型   | 默认值               | 说明                                                                          |
|---------------|--------|----------------------|-------------------------------------------------------------------------------|
| `ammo`        | bool   | `true`               | 内部标记，**不要删除**                                                        |
| `ammo_type`   | string | `"7_62x51mm"`        | 子弹口径标签                                                                  |
| `casing_type` | string | `"7_62x51mm_casing"` | 射击后产生的弹壳类型标签。`null` 或空字符串表示弹药全消耗不返回弹壳（如炮弹） |

## 脚本端查询

```js
AmmunitionTemplate.IsAmmo(itemId)       // → bool
AmmunitionTemplate.GetAmmoType(itemId)  // → "7_62x51mm"
AmmunitionTemplate.GetCasingType(itemId) // → "7_62x51mm_casing" 或 null
```
