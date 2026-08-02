[English](../../../en-US/script-mod/item-template/clothing.md) | ***简体中文***

← [返回模板总览](index.md)

# 衣服模板

`"type": "clothing"` — 可穿戴的服装物品。模板预设 `geofruit` 预制体及合理的默认值（utility 分类、不腐烂、0 护甲/隔离等），通过原版 `wearable` 系统实现装备与卸下。

> 📘 衣服模板负责注册和预设默认值。容器（`container`）和电池（`battery`）等扩展功能使用原版 `ItemDef` 对应字段。非原版的运行时行为（头灯、攀爬、潜水等）由脚本系统实现。

## 参数速览

```json
{
  "template": {
    "type": "clothing"
  }
}
```

仅此一行即可获得完整的可穿戴基础服装。`wearable.slot_id` 和 `wearable.desired_limb` **必须由用户填写**，否则装备系统不会激活。

## 模板策略

衣服模板在 `template` 内部提供以下原版 `ItemDef` 嵌套字段的预设值，可在物品 JSON 顶层直接覆盖：

### 穿戴

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `wearable.slot_id` | string | `""` | **必填**。装备槽位 ID（如 `UpTorso`、`Head`、`HandRight` 等 15 个肢体之一） |
| `wearable.desired_limb` | string | `""` | **必填**。穿戴贴图的目标肢体 |
| `wearable.armor` | float | `0.0` | 护甲值 |
| `wearable.isolation` | float | `0.0` | 保暖/隔热值 |
| `wearable.visual_offset` | int | `5` | 穿戴视觉层级偏移 |
| `wearable.can_be_held` | bool | `false` | 装备后是否仍可手持物品 |
| `wearable.hit_durability_loss_multiplier` | float | `0.0` | 受击时耐久损耗倍率 |
| `wearable.sprite_offset_x` | float | `0.0` | 穿戴贴图 X 偏移 |
| `wearable.sprite_offset_y` | float | `0.0` | 穿戴贴图 Y 偏移 |

> ⚠️ `slot_id` 和 `desired_limb` 均为空时整个 `wearable` 被忽略（不可穿戴）。

### 容器（背包）

`container.max_weight > 0` 时自动激活容器功能。在所有衣服模板物品上可用。

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `container.max_weight` | float | `0.0` | 容器最大承重（kg） |
| `container.max_weight_per_item` | float | `0.0` | 单个物品最大重量限制（0=不限） |
| `container.encumbrance_mult` | float | `1.0` | 负重倍率（背包内物品的重量系数） |
| `container.items_visible` | bool | `false` | 物品是否在外观上可见 |
| `container.tag_restriction` | string[] | `[]` | 限制只能放入特定标签的物品 |

### 电池

`battery.max_allowed_charge > 0` 时自动激活电池系统。

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `battery.battery_type` | string | `""` | 兼容的电池类型 ID |
| `battery.max_allowed_charge` | float | `0.0` | 最大电池容量 |
| `battery.start_charge` | float | `0.0` | 初始电量 |
| `battery.spawn_with_battery` | bool | `true` | 生成时是否预装电池 |
| `battery.weight_reduction` | bool | `false` | 电池供电状态下减重 |
| `battery.explode_at_zero` | bool | `false` | 电量归零时爆炸 |
| `battery.preset` | string | `""` | 电池预设 |

### 通用字段

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `category` | string | `utility` | 物品分类 |
| `origin_prefab` | string | `geofruit` | 基础预制体 |
| `weight` | float | `1.0` | 物品重量（kg） |
| `value` | int | `5` | 物品价值 |
| `destroy_at_zero_condition` | bool | `false` | `true` = 耐久归零销毁 |
| `scale_weight_with_condition` | bool | `false` | `true` = 重量随耐久缩放 |
| `recognition` | int | `0` | 识别等级 |
| `tags` | string | `""` | 标签，逗号分隔 |
| `decay.info` | int | `0` | 腐烂类型标志 |
| `decay.minutes` | float | `0.0` | 腐烂时间（分钟），`0`=永不腐烂 |
| `sprite.slot_rotation` | float | `0.0` | 物品栏格子旋转角度 |

## 使用示例

### 最简单的衣服

```json
// Item/my_shirt.json
{
  "full_name": "T恤",
  "description": "一件普通的棉T恤。",
  "wearable": {
    "slot_id": "UpTorso",
    "desired_limb": "UpTorso"
  },
  "template": { "type": "clothing" }
}
```

### 有护甲的头盔

```json
// Item/helmet.json
{
  "full_name": "战术头盔",
  "description": "标准军用头盔，提供良好的头部防护。",
  "weight": 1.5,
  "value": 50,
  "wearable": {
    "slot_id": "Head",
    "desired_limb": "Head",
    "armor": 10.0
  },
  "template": { "type": "clothing" }
}
```

### 背包

```json
// Item/backpack.json
{
  "full_name": "登山背包",
  "description": "坚固的登山背包，能装不少东西。",
  "weight": 1.5,
  "value": 30,
  "wearable": {
    "slot_id": "UpTorso",
    "desired_limb": "UpTorso"
  },
  "container": {
    "max_weight": 25.0,
    "max_weight_per_item": 5.0,
    "encumbrance_mult": 0.8,
    "items_visible": true
  },
  "template": { "type": "clothing" }
}
```

### 电池供电的装备

```json
// Item/night_vision_goggles.json
{
  "full_name": "夜视仪",
  "description": "电池供电的夜视镜，让你在黑暗中看清一切。",
  "weight": 0.8,
  "value": 120,
  "wearable": {
    "slot_id": "Head",
    "desired_limb": "Head"
  },
  "battery": {
    "max_allowed_charge": 100.0,
    "start_charge": 100.0,
    "spawn_with_battery": true
  },
  "template": { "type": "clothing" }
}
```

### 全身防护装甲

```json
// Item/body_armor.json
{
  "full_name": "重型防弹衣",
  "description": "全身防护装甲，大幅减少受到的伤害。",
  "weight": 8.0,
  "value": 200,
  "wearable": {
    "slot_id": "UpTorso",
    "desired_limb": "UpTorso",
    "armor": 25.0,
    "isolation": 5.0,
    "hit_durability_loss_multiplier": 0.3
  },
  "container": {
    "max_weight": 10.0,
    "encumbrance_mult": 1.2
  },
  "template": { "type": "clothing" }
}
```

## 脚本集成

### 查询 API

| 方法 | 返回 | 说明 |
|------|------|------|
| `ClothingTemplate.IsClothing(itemId)` | bool | 是否为衣服模板物品 |
| `ClothingTemplate.GetClothingData(itemId)` | ClothingData \| null | 获取衣服注册记录 |

### JS 示例

```javascript
// 装备时检查是否为衣服
global.onItemEquip = function(event) {
    if (clothingTemplate.IsClothing(event.itemId)) {
        log.info("Equipped clothing: " + event.itemId);
        // 可通过游戏物品系统读取 wearable.armor、container.max_weight 等原版数据
    }
};
```
