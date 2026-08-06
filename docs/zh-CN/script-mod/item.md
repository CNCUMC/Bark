[English](../../en-US/script-mod/item.md) | ***简体中文***

# 自定义物品

> 💡 **使用 `"template"` 可以大幅简化物品 JSON。**  
> 枪械、弹匣、弹药、弹壳等常见类型已有内置模板，一行 `"template": { "type": "gun" }`
> 即可自动填入预制体、重量、耐久等十几项默认值。  
> 详见 **[物品模板文档](./item-template/index.md)**。

---

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
  "use": [
    {
      "slot": [0, 1, 2, 3],
      "script": ["bandage123_use.js"]
    }
  ]
}
```

| 字段                       | 类型     | 默认值       | 说明                             |
|----------------------------|----------|--------------|----------------------------------|
| `full_name`                | string   | `""`         | 显示名称                         |
| `description`              | string   | `""`         | 悬浮提示                         |
| `category`                 | string   | `""`         | 背包分类                         |
| `weight`                   | float    | `0`          | 重量（千克）                     |
| `value`                    | int      | `0`          | 货币价值                         |
| `tags`                     | string   | `""`         | 逗号分隔的标签                   |
| `sprite`                   | object   | —            | 精灵图相关配置（见下方）         |
| `origin_prefab`            | string   | `"geofruit"` | 回退用的预制体（用于精灵尺寸）   |
| `spawn`                    | object   | —            | 世界生成/掉落配置（见下方）      |
| `script`                   | object   | null         | 动作 → 脚本映射（见下文）        |
| `custom_data`              | object   | null         | 自定义数据，供脚本读取           |
| `spawn_components`         | string[] | null         | 生成时附加的组件类型名（见下文） |
| `icon_animation_id`        | string   | null         | 物品栏图标动画 ID                |
| `worn_sprite_animation_id` | string   | null         | 穿戴贴图动画 ID                  |
| `held_sprite_offset`       | object   | null         | 手持精灵偏移 `{ "x", "y" }`      |
| `light`                    | object   | null         | 光源配置（见下文）               |
| `bandage`                  | object   | null         | 绷带配置（见下文）               |
| `syringe`                  | object   | null         | 注射器配置（见下文）             |
| `tool`                     | object   | null         | 工具/近战配置（见下文）          |

> 📝 物品 ID = `{模组ID}.{文件名}`（命名空间格式），如模组 `my_mod` 的 `bandage123.json` → ID `"my_mod.bandage123"`。原版物品（如
> `bandage`）无前缀，直接使用物品名。 **不是** JSON 里的字段。

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

> ⚠️ **有效肢体名**：`wearable.desired_limb` 必须设为游戏已知的 15 个肢体之一（ **必填**）：
> `Head`、`UpTorso`、`DownTorso`、`UpArmF`、`DownArmF`、`HandF`、`UpArmB`、`DownArmB`、`HandB`、`ThighF`、`CrusF`、`FootF`、
> `ThighB`、`CrusB`、`FootB`。
> **`slot_id` 和 `desired_limb` 是两个独立的概念**：`slot_id` 是装备槽标识（如 `"back"`），`desired_limb` 是身体肢体名。两者不能混用。
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

### 光源字段

让物品发光（手电、应急灯等）：

```json
{
  "light": {
    "intensity": 1.2,
    "color": "#FFFFAA",
    "light_type": "Point",
    "x_offset": 0,
    "y_offset": 0,
    "point_light_inner_angle": 360,
    "point_light_inner_radius": 0,
    "point_light_outer_angle": 360,
    "point_light_outer_radius": 8
  }
}
```

| 字段                             | 类型   | 默认值      | 说明                                                                                                                  |
|----------------------------------|--------|-------------|-----------------------------------------------------------------------------------------------------------------------|
| `light.intensity`                | float  | `10`        | 光照强度（过大时散光多次重叠，视觉上像渲染了两遍，建议根据半径调低）                                                  |
| `light.color`                    | string | `"#FFFFFF"` | 十六进制颜色 (#RRGGBB)                                                                                                |
| `light.light_type`               | string | `"Point"`   | 光源类型：`Point`/`Sprite`/`Global` 等                                                                                |
| `light.rotation`                 | float  | `-90`       | 光源旋转角度。CUCoreLib 1.0.3 的 LightProperties 无 Rotation 字段，Bark 在物品生成时直接旋转光源子物体（`Light`）实现 |
| `light.follow_mouse`             | bool   | `false`     | 光源是否跟随鼠标                                                                                                      |
| `light.light_on_zero_condition`  | bool   | `false`     | 物品耐久归零时光源是否仍亮                                                                                            |
| `light.x_offset`                 | float  | `0`         | 光源水平偏移                                                                                                          |
| `light.y_offset`                 | float  | `0`         | 光源垂直偏移                                                                                                          |
| `light.point_light_inner_angle`  | float  | `360`       | 点光源内锥角                                                                                                          |
| `light.point_light_inner_radius` | float  | `0`         | 点光源内半径                                                                                                          |
| `light.point_light_outer_angle`  | float  | `360`       | 点光源外锥角                                                                                                          |
| `light.point_light_outer_radius` | float  | `8`         | 点光源外半径                                                                                                          |

### 绷带字段

让物品具有绷带行为（包扎伤口、减速出血等）：

```json
{
  "bandage": {
    "effectiveness": 8,
    "skin_heal_amount": 8,
    "bandage_slow_amount": 18,
    "pain_reduction": 40,
    "bone_heal_timer_reduction": 5,
    "dislocation_timer_reduction": 5,
    "create_wrap_sprite": true,
    "wrap_sprite_path": "Special/bandageWrap",
    "wrap_sprite_color": "#FFFFFF",
    "minigame_color": "#E6E6E6"
  }
}
```

| 字段                                  | 类型   | 默认值                  | 说明                     |
|---------------------------------------|--------|-------------------------|--------------------------|
| `bandage.effectiveness`               | float  | `8`                     | 治疗效果                 |
| `bandage.skin_heal_amount`            | float  | `8`                     | 皮肤愈合量               |
| `bandage.bandage_slow_amount`         | float  | `18`                    | 减速出血量               |
| `bandage.pain_reduction`              | float  | `40`                    | 减痛量                   |
| `bandage.bone_heal_timer_reduction`   | float  | `5`                     | 骨折愈合加速（秒）       |
| `bandage.dislocation_timer_reduction` | float  | `5`                     | 脱位愈合加速（秒）       |
| `bandage.create_wrap_sprite`          | bool   | `true`                  | 是否生成包扎贴图         |
| `bandage.wrap_sprite_path`            | string | `"Special/bandageWrap"` | 包扎贴图路径             |
| `bandage.wrap_sprite_color`           | string | `"#FFFFFF"`             | 包扎贴图颜色 (#RRGGBB)   |
| `bandage.minigame_color`              | string | `"#E6E6E6"`             | 小游戏界面颜色 (#RRGGBB) |

### 注射器字段

让物品作为注射器（抽取/注射液体）：

```json
{
  "syringe": {
    "capacity": 100,
    "auto_fill": false,
    "amount_per_full_use": 100,
    "use_average_color": true,
    "minigame_color": "#FFFFFF"
  }
}
```

| 字段                          | 类型   | 默认值      | 说明                     |
|-------------------------------|--------|-------------|--------------------------|
| `syringe.capacity`            | float  | `100`       | 最大容量（毫升）         |
| `syringe.auto_fill`           | bool   | `false`     | 生成时自动填充           |
| `syringe.amount_per_full_use` | float  | `100`       | 每次完整使用消耗的量     |
| `syringe.use_average_color`   | bool   | `true`      | 是否使用平均颜色         |
| `syringe.minigame_color`      | string | `"#FFFFFF"` | 小游戏界面颜色 (#RRGGBB) |

### 工具字段

让物品成为近战/工具（可挥舞攻击）：

```json
{
  "tool": {
    "damage": 25,
    "structural_damage": 25,
    "attack_cooldown_multiplier": 0.66,
    "distance": 2.5,
    "knock_back": 270,
    "cooldown": 0.35,
    "attack_animation": "SwingAnim",
    "stamina_use": 0.5,
    "piercing": false,
    "swing_sounds": ["BSSwing1", "BSSwing2"],
    "volume": 0.5,
    "rotate_amount": 15.5,
    "physical_swing": true,
    "do_attack_animation": true,
    "metal_more_damage": false,
    "condition_loss_on_hit": 0.02
  }
}
```

| 字段                              | 类型     | 默认值        | 说明                 |
|-----------------------------------|----------|---------------|----------------------|
| `tool.damage`                     | float    | `25`          | 伤害                 |
| `tool.structural_damage`          | float    | `25`          | 结构伤害             |
| `tool.attack_cooldown_multiplier` | float    | `0.66`        | 攻击冷却倍率         |
| `tool.distance`                   | float    | `2.5`         | 攻击距离             |
| `tool.knock_back`                 | float    | `270`         | 击退力               |
| `tool.cooldown`                   | float    | `0.35`        | 冷却时间             |
| `tool.attack_animation`           | string   | `"SwingAnim"` | 攻击动画名           |
| `tool.stamina_use`                | float    | `0.5`         | 体力消耗             |
| `tool.piercing`                   | bool     | `false`       | 是否穿刺             |
| `tool.swing_sounds`               | string[] | 4 个默认值    | 挥舞音效             |
| `tool.volume`                     | float    | `0.5`         | 音量                 |
| `tool.rotate_amount`              | float    | `15.5`        | 旋转量               |
| `tool.physical_swing`             | bool     | `true`        | 是否物理挥舞         |
| `tool.do_attack_animation`        | bool     | `true`        | 是否播放攻击动画     |
| `tool.metal_more_damage`          | bool     | `false`       | 金属是否造成更多伤害 |
| `tool.condition_loss_on_hit`      | float    | `0.02`        | 命中时耐久损失       |

### 生成组件

生成物品时附加自定义组件（按类型名）：

```json
{
  "spawn_components": ["MyMod.MyComponent", "MyMod.AnotherComponent"]
}
```

| 字段               | 类型     | 说明                                       |
|--------------------|----------|--------------------------------------------|
| `spawn_components` | string[] | 生成物品时附加的组件类型全名（含命名空间） |

> ⚠️ 组件类型必须存在于运行时程序集中，否则会被忽略。

### 图标 / 穿戴动画与手持偏移

```json
{
  "icon_animation_id": "my_icon_anim",
  "worn_sprite_animation_id": "my_worn_anim",
  "held_sprite_offset": { "x": 2, "y": -1 }
}
```

| 字段                       | 类型   | 说明                                      |
|----------------------------|--------|-------------------------------------------|
| `icon_animation_id`        | string | 物品栏图标动画 ID                         |
| `worn_sprite_animation_id` | string | 穿戴贴图动画 ID                           |
| `held_sprite_offset`       | object | 手持精灵偏移 `{ "x": float, "y": float }` |

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

`script` 字段绑定被动检测脚本和条件触发器。`use` 字段（顶层，与 `wearable` 互斥）绑定主动使用脚本。`wearable` 内可绑定装备相关脚本。

### script（被动检测 + 条件触发器）

| 键            | 类型                  | 触发时机                     |
|---------------|-----------------------|------------------------------|
| `attack`      | string[]              | 手持此物品近战攻击           |
| `use_on_limb` | string[]              | 对某个肢体使用               |
| `has`         | string[]              | 物品在玩家背包中（持续轮询） |
| `in_hand`     | string[]              | 物品被拿在手上               |
| `not_in_hand` | string[]              | 物品从手上放下               |
| `durability`  | ConditionTriggerDef[] | 耐久值越过阈值时（见下方）   |

### use（顶层，主动使用）

`use` 是一个数组，每项指定使用来源和脚本。`use` 与 `wearable` 互斥 —— 一个物品要么可穿戴，要么可使用，不能同时。

```json
{
  "full_name": "医疗包",
  "category": "Medical",
  "weight": 0.3,
  "use": [
    { "slot": [0, 1, 2, 3],     "script": ["medkit_use.js"] },
    { "slot": ["hand"],          "script": ["medkit_hand.js"] },
    { "limb_slot": ["Head","HandF"], "script": ["medkit_limb.js"] }
  ]
}
```

| 键          | 类型     | 说明                                                  |
|-------------|----------|-------------------------------------------------------|
| `slot`      | object[] | 背包槽位索引（数字），`"hand"`=手持，`null`/`[]`=全部 |
| `limb_slot` | string[] | 肢体槽位名，`null`/`[]`=全部                          |
| `script`    | string[] | 脚本文件路径数组                                      |

### wearable 内的脚本字段

| 键        | 类型     | 触发时机             |
|-----------|----------|----------------------|
| `equip`   | string[] | 装备（穿上）         |
| `unequip` | string[] | 卸下（脱下）         |
| `attack`  | string[] | 穿着此物品时近战攻击 |
| `damage`  | string[] | 装备受到伤害         |
| `wearing` | string[] | 装备在身上时持续轮询 |

```json
{
  "wearable": {
    "slot_id": "back",
    "desired_limb": "Head",
    "equip": ["helmet_equip.js"],
    "unequip": ["helmet_unequip.js"],
    "attack": ["helmet_attack.js"],
    "damage": ["helmet_damage.js"],
    "wearing": ["helmet_wearing.js"]
  }
}
```

### 条件触发器（ConditionTriggerDef）

复用于 `durability`、`capacity_trigger`、`charge_trigger`，每项含：

```json
{
  "operator": "<=",
  "value": 0.3,
  "script": ["low_durability.js"]
}
```

| 键         | 类型     | 说明                                         |
|------------|----------|----------------------------------------------|
| `operator` | string   | 比较运算符：`"<"`/`"<="`/`"=="`/`">="`/`">"` |
| `value`    | float    | 阈值（0.0~1.0 百分比）                       |
| `script`   | string[] | 脚本文件路径数组                             |

触发器采用边沿检测：仅当条件从"不满足"变为"满足"时触发一次，避免重复执行。

### 容器容量触发器

```json
{
  "container": {
    "max_weight": 10,
    "capacity_trigger": [
      { "operator": ">=", "value": 0.8, "script": ["near_full.js"] }
    ]
  }
}
```

### 电池电量触发器

```json
{
  "battery": {
    "preset": "medium",
    "charge_trigger": [
      { "operator": "<=", "value": 0.1, "script": ["low_battery.js"] }
    ]
  }
}
```

### 脚本路径

`script` 数组中的路径相对于 **脚本目录**而非 JSON 所在位置。例如 `"bandage123_use.js"` 指 `ModDir/bandage123_use.js`。
你可以用子目录组织脚本：

```json
{
  "script": {
    "use_on_limb": [
      "Scripts/bandage123_limb.js"
    ],
    "attack": [
      "Scripts/bandage123_attack.js"
    ]
  },
  "wearable": {
    "slot_id": "Head",
    "desired_limb": "Head",
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
    // itemId: "my_mod.bandage123"
    // item:    C# Item 实例
    // action:  "use"

    Player.Alert("已使用绷带", true);
}
```

`main` 函数接收三个基础参数，条件触发器场景额外接收三个参数：

| 参数             | 类型   | 说明                                                        |
|------------------|--------|-------------------------------------------------------------|
| `itemId`         | string | 物品 ID                                                     |
| `item`           | Item   | C# Item 实例（不可用时为 null）                             |
| `action`         | string | 动作：`"use"`、`"attack"`、`"equip"` 等                     |
| `currentValue`   | float  | **[条件触发器]** 当前百分比值（0.0~1.0）                    |
| `thresholdValue` | float  | **[条件触发器]** 触发器阈值（0.0~1.0）                      |
| `operator`       | string | **[条件触发器]** 运算符（`"<"` `"<="` `"=="` `">="` `">"`） |

后三个参数仅在 `durability`、`capacity_trigger`、`charge_trigger` 触发时传入，其余场景为 `null`。

只接受部分参数也可以——JavaScript 和 Lua 自动忽略多余参数：

```js
function main(itemId) { /* 只取 ID */
}

function main(itemId, item, action) { /* 完整上下文 */
}

// 条件触发器示例：耐久低于 30% 时执行
function main(itemId, item, action, currentValue, thresholdValue, operator) {
    Player.Alert(`物品耐久 ${currentValue} ${operator} ${thresholdValue}，触发！`, true);
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

- 两个脚本模组同时定义同名物品时，后加载的会覆盖先加载的（但命名空间前缀有效防止此类冲突）
- JSON 字段使用 `snake_case` 命名（所有单词全部小写，单词与单词之间使用下划线 `_` 链接）
- 如果物品只需要脚本而不需要自定义贴图，可以完全省略在 `Assets/Item/` 目录下放置贴图
- 开发时无需重启游戏，指令 `script reload`/`sr` 会重载物品定义
