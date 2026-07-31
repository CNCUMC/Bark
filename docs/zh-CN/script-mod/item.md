[English](../../en-US/script-mod/item.md) | ***简体中文***

# 自定义物品

通过 JSON 定义自定义物品、液体容器和纯液体。JSON 文件放在脚本模组的 `Item/` 目录下，精灵图片放在 `Assets/Item/` 下。

## 目录结构

```
ScriptMod/Mods/
  MyMod/
    mod.json
    main.js
    Item/
      bandage123.json          ← 自定义物品
      arrow.json            ← 带脚本的自定义物品
      potion.json           ← 液体容器
      water.json            ← 纯液体
    Assets/Item/
      bandage123.png           ← 物品精灵图 (itemId.png)
      arrow.png
      potion.png
      potion_fill.png       ← 液体填充遮罩 (itemId_fill.png)
```

## 物品 JSON 格式

物品类型由 JSON 字段自动检测：

| 包含字段               | 类型     |
|------------------------|----------|
| `capacity`             | 液体容器 |
| `color`（无 `weight`） | 纯液体   |
| 其他                   | 普通物品 |

### 通用字段

```json
{
  "full_name": "绷带",
  "description": "止血并治疗轻伤。",
  "category": "Medical",
  "weight": 0.2,
  "value": 15,
  "tags": "medical,bandage",
  "sprite": {
    "scale": 1.0
  },
  "spawn": {
    "drop_pool": ["Medical"],
    "frequency": 5
  },
  "script": {
    "use": [
      "bandage123_use.js"
    ]
  }
}
```

| 字段              | 类型     | 默认值       | 说明                           |
|-------------------|----------|--------------|--------------------------------|
| `full_name`       | string   | `""`         | 显示名称                       |
| `description`     | string   | `""`         | 悬浮提示                       |
| `category`        | string   | `""`         | 背包分类                       |
| `weight`          | float    | `0`          | 重量（千克）                   |
| `value`           | int      | `0`          | 货币价值                       |
| `tags`            | string   | `""`         | 逗号分隔的标签                 |
| `sprite`          | object   | —            | 精灵图相关配置（见下方）       |
| `origin_prefab`   | string   | `"geofruit"` | 回退用的预制体（用于精灵尺寸） |
| `spawn`           | object   | —            | 世界生成/掉落配置（见下方）    |
| `script`          | object   | null         | 动作 → 脚本映射（见下文）      |
| `custom_data`     | object   | null         | 自定义数据，供脚本读取         |

> 📝 物品 ID 就是文件名（不含 `.json` 扩展名），如 `bandage123.json` → ID 为 `"bandage123"`。 **不是** JSON 里的字段。

### 可穿戴字段

让物品可穿戴，添加 `wearable` 对象：

```json
{
  "wearable": {
    "slot_id": "back",
    "desired_limb": "Head",
    "can_be_held": true,
    "armor": 0.3,
    "isolation": 0.1,
    "hit_durability_loss_multiplier": 1.0,
    "sorting_order": 0,
    "visual_offset": 5,
    "sprite_offset_x": 0,
    "sprite_offset_y": 0,
    "multi": {
      "FootF": {
        "sprite_offset_x": 2,
        "sprite_offset_y": 1
      }
    }
  }
}
```

| 字段                                      | 类型                    | 默认值  | 说明                                                |
|-------------------------------------------|-------------------------|---------|-----------------------------------------------------|
| `wearable.slot_id`                        | string                  | `""`    | 装备槽位标识（**必填**，如 `"Head"`, `"back"` 等）  |
| `wearable.desired_limb`                   | string                  | `""`    | 穿戴贴图目标肢体（**必填**，见下方有效值列表）      |
| `wearable.can_be_held`                    | bool                    | `false` | 装备后是否仍可手持                                  |
| `wearable.armor`                          | float                   | `0`     | 护甲值（0–1）                                       |
| `wearable.isolation`                      | float                   | `0`     | 保暖值（0–1）                                       |
| `wearable.hit_durability_loss_multiplier` | float                   | `0`     | 受击耐久损失倍率                                    |
| `wearable.sorting_order`                  | int?                    | null    | 装备贴图渲染排序                                    |
| `wearable.visual_offset`                  | int                     | `5`     | 视觉层级偏移                                        |
| `wearable.sprite_offset_x`                | float                   | `0`     | 装备贴图水平偏移                                    |
| `wearable.sprite_offset_y`                | float                   | `0`     | 装备贴图垂直偏移                                    |
| `wearable.multi`                          | object（肢体名 → 偏移） | null    | 额外肢体贴图及偏移（使用 `{itemId}_mw_{肢体}.png`） |

> ⚠️ **有效肢体名**：`wearable.desired_limb` 必须设为游戏已知的 15 个肢体之一（**必填**）：
> `Head`、`UpTorso`、`DownTorso`、`UpArmF`、`DownArmF`、`HandF`、`UpArmB`、`DownArmB`、`HandB`、`ThighF`、`CrusF`、`FootF`、`ThighB`、`CrusB`、`FootB`。
> **`slot_id` 和 `desired_limb` 是两个独立的概念**：`slot_id` 是装备槽标识（如 `"Head"`, `"back"`），`desired_limb` 是身体肢体名。两者不能混用。
> **`slot_id` 为空** 或 **`desired_limb` 为空**时，物品的可穿戴属性会被禁用，防止 CUCoreLib 内部 NRE 崩溃。
> 可使用 `Limb.IsValidLimbName()` 在运行时校验（参见 [Limb API](../script-api/limbs.md)）。

### 容器字段

让物品成为容器（背包、挎包等）：

```json
{
  "container": {
    "max_weight": 10,
    "max_weight_per_item": 5,
    "items_visible": true
  }
}
```

### 电池字段

让物品使用电池：

```json
{
  "battery": {
    "preset": "medium",
    "spawn_with_battery": true
  }
}
```

可选预设：`"small"`（50 电量）、`"medium"`（100）、`"large"`（300）。不填 `preset` 时可自定义
`max_allowed_charge` 和 `start_charge`。

## 液体容器

添加 `capacity` 即可让物品盛装液体：

```json
{
  "full_name": "水瓶",
  "category": "容器",
  "weight": 0.3,
  "capacity": 1000,
  "default_liquid": {
    "water": 500
  }
}
```

| 字段             | 类型                    | 说明                        |
|------------------|-------------------------|-----------------------------|
| `capacity`       | float                   | 最大容量（毫升）            |
| `auto_fill`      | bool                    | 生成时自动灌装（默认 true） |
| `default_liquid` | object（液体ID → 毫升） | 初始内容物                  |

## 纯液体

用 `color` 定义，不写 `weight`：

```json
{
  "color": "#4488FF",
  "description": "清澈的水。",
  "value_per_liter": 1,
  "health_usable": true
}
```

| 字段              | 类型   | 说明                   |
|-------------------|--------|------------------------|
| `color`           | string | 十六进制颜色 (#RRGGBB) |
| `value_per_liter` | float  | 每 1000ml 的价值       |
| `health_usable`   | bool   | 是否可用于治疗         |
| `injectable`      | bool   | 是否可注射             |

## 物品脚本

`script` 字段将脚本文件绑定到物品动作上。当动作触发时，Bark 依次执行每个脚本并调用其中的 `main(itemId, item, action)`
函数。

### 支持的动作

| 键            | 触发时机             |
|---------------|----------------------|
| `use`         | 从背包中使用         |
| `use_in_hand` | 手持时使用           |
| `equip`       | 装备（穿上）         |
| `unequip`     | 脱卸（取下）         |
| `use_on_limb` | 对某个肢体使用       |
| `attack`      | 用此物品进行近战攻击 |

### 脚本路径

`script` 数组中的路径相对于 **脚本目录**而非 JSON 所在位置。例如 `"bandage123_use.js"` 指 `ModDir/bandage123_use.js`。
你可以用子目录组织脚本：

```json
{
  "script": {
    "use": [
      "Scripts/bandage123_use.js"
    ],
    "equip": [
      "Scripts/bandage123_equip.js"
    ]
  }
}
```

### 脚本函数签名

```js
// bandage123_use.js
function main(itemId, item, action) {
    // itemId: "bandage123"
    // item:    C# Item 实例
    // action:  "use"

    Player.Alert("已使用绷带", true);
}
```

`main` 函数接收三个参数：

| 参数     | 类型   | 说明                                    |
|----------|--------|-----------------------------------------|
| `itemId` | string | 物品 ID                                 |
| `item`   | Item   | C# Item 实例（不可用时为 null）         |
| `action` | string | 动作：`"use"`、`"attack"`、`"equip"` 等 |

只接受部分参数也可以——JavaScript 和 Lua 自动忽略多余参数：

```js
function main(itemId) { /* 只取 ID */
}

function main(itemId, item, action) { /* 完整上下文 */
}
```

### 完整示例

攻击时销毁自身的一支自定义箭：

**`Item/arrow.json`**：

```json
{
  "full_name": "树皮箭",
  "category": "武器",
  "weight": 0.05,
  "value": 3,
  "script": {
    "attack": [
      "arrow.js"
    ]
  }
}
```

**`arrow.js`**（放在脚本模组根目录）：

```js
function main(itemId, item, action) {
    Item.Destroy(itemId);
    Player.Alert("箭无虚发！", true);
}
```

### 与全局钩子的关系

物品脚本和[全局事件钩子](../script-events.md)各自独立工作：

- **物品脚本**（`main`）：只在此物品的特定动作时触发。用于按物品定制的逻辑。
- **全局钩子**（`onItemUse`、`onItemAttack` 等）：任何物品触发都会调用。用于脚本模组级别的统计追踪。

两者可以共存——一次箭的普通攻击会同时触发 `arrow.js` 中的 `main()` 和 脚本模组主脚本中的 `onItemAttack(event)`。

## 精灵图资源

| 文件模式                 | 用途             |
|--------------------------|------------------|
| `{itemId}.png`           | 背包/世界精灵图  |
| `{itemId}_worn.png`      | 装备后贴图       |
| `{itemId}_mw_{肢体}.png` | 额外肢体装备贴图 |
| `{itemId}_fill.png`      | 液体填充遮罩     |

所有精灵图放在 `Assets/Item/` 下。如果 `{itemId}.png` 不存在，Bark 会回退到 `origin_prefab` *（也就是默认的地生果）* 精灵图。

> 💡 **装备贴图回退**：如果可穿戴物品未提供 `{itemId}_worn.png`，Bark 会自动使用物品主贴图作为
> 装备后的贴图（`{itemId}.png`）。仅当两者都不存在时才会阻止装备并发出警告。因此大多数物品
> 可以完全省略 `_worn.png`，装备后直接复用背包精灵图。

## 注意事项

- 两个脚本模组同时定义 `"bandage123"` 时，后加载的会覆盖先加载的
- JSON 字段使用 `snake_case` 命名（所有单词全部小写，单词与单词之间使用下划线 `_` 链接）
- 如果物品只需要脚本而不需要自定义贴图，可以完全省略在 `Assets/Item/` 目录下放置贴图
- 开发时无需重启游戏，指令 `script reload`/`rs` 会重载物品定义
