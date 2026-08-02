[English](../../../en-US/script-mod/item-template/food.md) | ***简体中文***

← [返回模板总览](index.md)

# 食物模板

`"type": "food"` — 可食用的食物物品。模板预设 `geofruit` 预制体及 geofruit 的全部默认属性（腐烂、重量、品质等），吃下时调用 `body.Eat()`、`body.Drink()` 等原生方法模拟进食效果。

## 参数速览

```json
{
  "template": {
    "type": "food"
  }
}
```

仅此一行即可获得完整的 geofruit 等价物。覆盖任意字段即可定制。

## 食物专属参数（template 内）

这些参数控制吃下时的效果，位于 `template` 对象内部：

```json
{
  "template": {
    "type": "food",
    "food": true,
    "nutrition": 3.5,
    "weight_offset": 0.1,
    "hydration": 5.0,
    "happiness": 0.5,
    "condition_loss": 0.5,
    "eat_sound": "eatCrunch",
    "eat_good_voice": true
  }
}
```

| 参数              | 类型   | 默认值         | 说明                                              |
|-------------------|--------|----------------|---------------------------------------------------|
| `food`            | bool   | `true`         | 内部标记，**不要删除**                            |
| `nutrition`       | float  | `3.5`          | 饥饿值恢复量，传入 `body.Eat(hunger, weight)`     |
| `weight_offset`   | float  | `0.1`          | 体重增量，传入 `body.Eat(hunger, weight)`         |
| `hydration`       | float  | `5.0`          | 口渴值恢复量，传入 `body.Drink(thirst)`           |
| `happiness`       | float  | `0.5`          | 幸福感增量，`body.happiness +=`                   |
| `condition_loss`  | float  | `0.5`          | 每吃一口消耗的耐久度，`item.condition -=`         |
| `eat_sound`       | string | `"eatCrunch"`  | 咀嚼音效名称，`Sound.Play()` 播放。空字符串则不播放 |
| `eat_good_voice`  | bool   | `true`         | 是否触发 `body.talker.EatGood()` 好吃语音         |

## 可覆盖的通用字段（顶层）

食物模板预设了以下 `ItemInfo` 字段，可在物品 JSON 顶层直接覆盖：

```json
{
  "weight": 0.75,
  "value": 1,
  "ignore_depression": false,
  "recognition": 3,
  "tags": "cangetwet",
  "destroy_at_zero_condition": true,
  "scale_weight_with_condition": true,
  "decay": {
    "info": 0,
    "minutes": 12.0
  },
  "sprite": {
    "slot_rotation": 45
  },
  "qualities": [
    { "type": "produce" }
  ]
}
```

| 字段                        | 类型   | 默认值   | 说明                                                                                     |
|-----------------------------|--------|----------|------------------------------------------------------------------------------------------|
| `weight`                    | float  | `0.75`   | 物品重量                                                                                |
| `value`                     | int    | `1`      | 物品价值                                                                                |
| `ignore_depression`         | bool   | `false`  | `true` = 抑郁时仍可食用（治愈食物）                                                      |
| `recognition`               | int    | `3`      | 识别等级                                                                                |
| `tags`                      | string | `cangetwet` | 标签，逗号分隔                                                                      |
| `destroy_at_zero_condition` | bool   | `true`   | `true` = 耐久归零时销毁物品                                                              |
| `scale_weight_with_condition` | bool | `true`  | `true` = 重量随耐久等比缩放                                                              |
| `decay.info`                | int    | `0`      | 腐烂类型标志：`1` = NoDecayWithoutContainerItem（不放容器不腐烂，罐头适用）               |
| `decay.minutes`             | float  | `12.0`   | 腐烂时间（分钟）                                                                         |
| `sprite.slot_rotation`      | float  | `45`     | 物品栏格子旋转角度（度）                                                                  |
| `qualities`                 | array  | `[{type:"produce"}]` | 制作品质列表                                                                   |

## 使用示例

### 最简单的食物

```json
// Item/my_apple.json
{
  "full_name": "苹果",
  "description": "一个多汁的红苹果。",
  "category": "食物",
  "template": { "type": "food" }
}
```

和其他 geofruit 完全相同。

### 大餐

```json
// Item/steak_dinner.json
{
  "full_name": "牛排大餐",
  "description": "烤得恰到好处的牛排配土豆泥。",
  "category": "食物",
  "weight": 0.5,
  "value": 8,
  "template": {
    "type": "food",
    "nutrition": 15,
    "hydration": 2,
    "happiness": 3.0,
    "condition_loss": 1.0,
    "eat_sound": "eatCrunch",
    "eat_good_voice": true
  }
}
```

### 罐头食品（不腐烂）

```json
// Item/canned_beans.json
{
  "full_name": "豆子罐头",
  "description": "一个密封的豆子罐头，打开前不会坏。",
  "category": "食物",
  "weight": 1.0,
  "value": 3,
  "decay": {
    "info": 1,
    "minutes": 0
  },
  "template": {
    "type": "food",
    "nutrition": 8,
    "hydration": 4
  }
}
```

`decay.info = 1`（NoDecayWithoutContainerItem）使罐头只有被倒出/放在容器外才会腐烂。

### 治愈零食（抑郁时可吃）

```json
// Item/comfort_candy.json
{
  "full_name": "治愈糖果",
  "description": "甜到心里的糖果，再难过的日子吃了也会微笑。",
  "category": "食物",
  "weight": 0.1,
  "value": 2,
  "ignore_depression": true,
  "template": {
    "type": "food",
    "nutrition": 1,
    "happiness": 5.0,
    "condition_loss": 0.25
  }
}
```

## 脚本集成

食物吃下的整个流程由 C# `useAction` 执行，无需脚本即可工作。如需在吃下前后添加额外逻辑（如 Buff、特效），可在物品 JSON 中定义 `use` 脚本：

```json
{
  "use": [
    {
      "script": ["onUse", "Args", "Enum.Use"],
      "condition": "normal"
    }
  ]
}
```

注意：一旦定义了 `use` 数组，食物的自动 Eat/Drink 效果会被脚本覆盖，需在脚本中手动调用。如果只想追加效果而不是替代，推荐用 `onUse` 脚本配合食物自动效果（当前 `use` 和模板 useAction 目前互斥，未来版本可能合并）。

## 和 geofruit 的对比

| 属性                  | geofruit | 食物模板默认    |
|-----------------------|----------|-----------------|
| 预制体                | geofruit | geofruit       |
| 物品栏旋转            | 45°      | 45°            |
| 可食用                | ✓        | ✓              |
| 腐烂时间（分钟）      | 12       | 12             |
| 耐久归零销毁          | ✓        | ✓              |
| 重量                  | 0.75     | 0.75           |
| 重量随耐久缩放        | ✓        | ✓              |
| `body.Eat(hunger)`   | 3.5      | 3.5            |
| `body.Eat(weight)`   | 0.1      | 0.1            |
| `body.Drink(thirst)` | 5        | 5              |
| `body.happiness`     | +0.5     | +0.5           |
| `item.condition`     | -0.5     | -0.5           |
| 吃下音效             | eatCrunch | eatCrunch     |
| 好吃语音             | ✓        | ✓              |
| 制作品质             | produce  | produce        |
