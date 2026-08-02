[English](../../../en-US/script-mod/item-template/index.md) | ***简体中文***

# 物品模板

模板是一组预设的物品属性。通过 `"template"` 字段引用模板，可以大幅简化物品 JSON——模板自动提供默认值，你只需覆盖需要改动的地方。

## 为什么用模板

假设你定义一把 AK-47，原始游戏没有"自动步枪"这种物品。用模板只需写：

```json
{
  "full_name": "AK-47",
  "template": {
    "type": "gun",
    "ammo_type": "7_62x51mm",
    "damage": 45,
    "mag_type": "ak_mag"
  }
}
```

模板自动填好的默认值包括预制体、重量、耐久、连发间隔、后坐力等十几项——你一行都不用写。相比之下纯手写要写几十行。

**模板不是黑魔法**：模板合并后的结果和纯手写 JSON 完全等价。你想覆盖哪个字段直接写即可。

## 目录结构

模板 JSON 放在脚本模组的 `item-template/` 目录下：

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
      templates.json        ← 自定义模板（可选）
```

`item-template/` 目录是可选的。不写就只用 Bark 内置模板。

## 物品 JSON 中用模板

在物品 JSON 的 `template` 字段指定类型和参数：

```json
// Item/ak47.json — 枪械
{
  "full_name": "AK-47",
  "category": "武器",
  "template": {
    "type": "gun",
    "ammo_type": "7_62x51mm",
    "damage": 45,
    "mag_type": "ak_mag",
    "fire_sound": "Assets/Audio/ak47_shot.wav"
  }
}
```

| template 字段   | 说明                                                      |
|-----------------|-----------------------------------------------------------|
| `type`          | **必填**。模板类型名：`"gun"` `"mag"` `"ammo"` `"casing"` `"food"` `"clothing"` |
| `fire_sound` 等 | 类型参数，按需填写；不写的字段取模板默认值                |

> ⚠️ `type` 是唯一必填字段。不知道有哪些参数？往下看各模板的类型文档。

## 模板类型

Bark 内置四种模板类型：

| 类型 | 用途         | type 值    | 详细文档              |
|------|--------------|------------|-----------------------|
| 枪械 | 可开火的武器 | `"gun"`    | [枪械模板](gun.md)    |
| 弹匣 | 装弹容器     | `"mag"`    | [弹匣模板](mag.md)    |
| 弹药 | 子弹         | `"ammo"`   | [弹药模板](ammo.md)   |
| 弹壳 | 射击后掉落物 | `"casing"` | [弹壳模板](casing.md) |
| 食物 | 可食用的物品 | `"food"`   | [食物模板](food.md)   |
| 衣服 | 可穿戴的服装 | `"clothing"`   | [衣服模板](clothing.md) |

## 自定义模板

除了 Bark 内置模板，你可以在 `item-template/templates.json` 中注册自己的模板：

**`item-template/templates.json`**：

```json
{
  "my_melee": {
    "template": {
      "type": "custom",
      "melee": true
    },
    "origin_prefab": "axe",
    "category": "武器",
    "weight": 1.5,
    "value": 50,
    "tags": "melee,weapon"
  }
}
```

然后在物品中引用：

```json
{
  "full_name": "定制砍刀",
  "template": {
    "type": "my_melee",
    "damage": 60
  }
}
```

自定义模板的注册方式：

| 方式      | 说明                                                                                                |
|-----------|-----------------------------------------------------------------------------------------------------|
| JSON 文件 | `item-template/templates.json` 中定义，key 为模板名，value 为模板默认 JSON                          |
| 脚本端    | `TemplateLoader.Register("name", jsonObj)` 或 `TemplateLoader.RegisterFromJson("name", jsonString)` |

## 热重载

使用 `script reload` / `rs` 重载模组后：

- 所有物品 JSON 重新解析，模板重新合并
- 已存在于游戏世界中的枪械实例会自动刷新 **音效**和 **枪口偏移**等可热更属性
- 弹匣/弹药/弹壳注册表自动更新

无需重启游戏。
