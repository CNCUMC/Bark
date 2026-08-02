[English](../../../en-US/script-mod/item-template/casing.md) | ***简体中文***

← [返回模板总览](index.md)

# 弹壳模板

`"type": "casing"` — 弹壳物品。射击后掉落，可回收复装。

## 参数

```json
{
  "template": {
    "type": "casing",
    "casing": true,
    "casing_type": "7_62x51mm_casing"
  }
}
```

| 参数          | 类型   | 默认值               | 说明                         |
|---------------|--------|----------------------|------------------------------|
| `casing`      | bool   | `true`               | 内部标记，**不要删除**       |
| `casing_type` | string | `"7_62x51mm_casing"` | 弹壳类型标签，弹药通过它匹配 |

## 脚本端查询

```js
CasingTemplate.IsCasing(itemId)        // → bool
CasingTemplate.GetCasingType(itemId)   // → "7_62x51mm_casing"
```
