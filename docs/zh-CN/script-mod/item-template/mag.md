[English](../../../en-US/script-mod/item-template/mag.md) | ***简体中文***

← [返回模板总览](index.md)

# 弹匣模板

`"type": "mag"` — 弹匣物品。预设 `riflemagazine` 预制体（AmmoScript + 容器属性）。

## 参数

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

| 参数         | 类型   | 默认值            | 说明                         |
|--------------|--------|-------------------|------------------------------|
| `mag`        | bool   | `true`            | 内部标记，**不要删除**       |
| `mag_type`   | string | `"rifle_mag"`     | 弹匣类型标签，枪械通过它匹配 |
| `ammo_type`  | string | `"7_62x51mm"`     | 接受的弹药口径               |
| `capacity`   | int    | `15`              | 弹匣容量（可装多少发）       |
| `max_weight` | float  | `capacity × 0.03` | 容器最大负重                 |

## 脚本端查询

弹匣物品在加载时自动注册到 `MagTemplate` 注册表，脚本端可直接查询：

```js
MagTemplate.IsMag(itemId)          // → bool
MagTemplate.GetMagData(itemId)     // → { MagType, AmmoType, Capacity, MaxWeight }
MagTemplate.GetMagType(itemId)     // → "ak_mag"
MagTemplate.GetAmmoType(itemId)    // → "7_62x51mm"
MagTemplate.GetCapacity(itemId)    // → 30
```
