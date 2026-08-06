[English](../../en-US/script-mod/tile.md) | ***简体中文***

# 自定义物块

在脚本模组目录下创建 `Tile/` 文件夹，放入 JSON 文件即可注册自定义物块（地面/墙壁方块）。Bark 会在脚本模组加载时自动扫描并注册到
CUCoreLib 的 `TileRegistry`。

## 目录结构

```
ScriptMod/Mods/
  MyMod/
    mod.json
    main.js
    Tile/
      marble.json           ← 自定义物块
      mahogany.json
    Assets/Tile/
      marble.png            ← 物块精灵图 ({id}.png)
      mahogany.png
```

## JSON 格式

`Tile/{name}.json` 字段与 `CustomTileDefinition` 一一对应：

```json
{
  "name": "大理石",
  "color": "#FFDDCC",
  "collider_type": "Grid",
  "health": 200,
  "hit_sound": "stone",
  "step_sound": "Gravel",
  "sleep_quality": "Good",
  "no_variation": false,
  "metallic": false,
  "toxicity": 0,
  "slippery": false,
  "spawn_amount": 0.5,
  "spawn_layers": [
    2,
    4
  ],
  "generation_style": [
    "Vein",
    "Outskirt"
  ],
  "drops": [
    {
      "id": "marble",
      "chance": 1.0,
      "condition_min": 0.8,
      "condition_max": 1.0
    }
  ],
  "custom_data": {
    "my_key": "my_value"
  },
  "script": {
    "on_place": [
      "scripts/marble_place.js"
    ],
    "on_exist": [
      "scripts/marble_exist.js"
    ],
    "on_damaging": [
      "scripts/marble_damage.js"
    ],
    "on_destroyed": [
      "scripts/marble_destroy.js"
    ]
  },
  "sprite_import_scale": 8.0
}
```

## 字段参考

### 基础

| 字段            | 类型   | 默认值       | 说明                                                 |
|-----------------|--------|--------------|------------------------------------------------------|
| `name`          | string | `""`         | 显示名称（注册为 `other.ID` 本地化条目）             |
| `color`         | string | null（白色） | 精灵图着色，支持 RGBA hex（`#FF0000` / `#FF0000FF`） |
| `collider_type` | string | `"Grid"`     | 碰撞器类型：`Grid` / `Sprite` / `None`               |

### 属性

| 字段            | 类型   | 默认值     | 说明                                                          |
|-----------------|--------|------------|---------------------------------------------------------------|
| `health`        | float  | `100`      | 生命值（破坏该方块的伤害量）                                  |
| `hit_sound`     | string | `"stone"`  | 打击音效 ID（`stone`、`metal`、`rock` 等）                    |
| `step_sound`    | string | `"Gravel"` | 行走音效 ID（`Gravel`、`Rock` 等）                            |
| `sleep_quality` | string | null（无） | 睡眠质量：`Excellent` / `Good` / `Mediocre` / `Bad` / `Awful` |
| `no_variation`  | bool   | `false`    | 禁用原版视觉随机变化（翻转等）                                |
| `metallic`      | bool   | `false`    | 启用金属伤害行为                                              |
| `toxicity`      | float  | `0`        | 毒性（辐射）值                                                |
| `slippery`      | bool   | `false`    | 启用滑动行为                                                  |

### 生成

| 字段               | 类型     | 默认值       | 说明                                                                               |
|--------------------|----------|--------------|------------------------------------------------------------------------------------|
| `spawn_amount`     | float    | `0`          | 生成数量乘数。`0` 禁用自动生成，`1` 等同铜矿                                       |
| `spawn_layers`     | int[]    | null（全部） | 允许生成的游戏层（1-based），如 `[2, 4, 5]`                                        |
| `generation_style` | string[] | null（默认） | 生成形状样式：`Vein` / `HeavyVeins` / `Singular` / `Stripe` / `Inner` / `Outskirt` |

### 掉落

| 字段                    | 类型   | 说明                         |
|-------------------------|--------|------------------------------|
| `drops[].id`            | string | 掉落物品 ID                  |
| `drops[].chance`        | float  | 掉落几率（0~1，默认 `1`）    |
| `drops[].condition_min` | float  | 掉落物品最低耐久             |
| `drops[].condition_max` | float  | 掉落物品最高耐久（默认 `1`） |

### 扩展

| 字段                  | 类型                         | 默认值 | 说明                                                      |
|-----------------------|------------------------------|--------|-----------------------------------------------------------|
| `custom_data`         | `Dictionary<string, object>` | null   | 自定义元数据，可通过 `TileRegistry.TryGetCustomData` 读取 |
| `script`              | object                       | null   | 物块脚本定义，见[物块脚本](#物块脚本)                     |
| `sprite_import_scale` | float                        | `8.0`  | 精灵图导入放大倍数                                        |

> 📝 物块 ID 取自 JSON 文件名（不含扩展名），如 `marble.json` → ID 为 `marble`，与物品注册规则一致。

## 精灵图资源

| 文件模式   | 用途       |
|------------|------------|
| `{id}.png` | 物块精灵图 |

精灵图放在 `Assets/Tile/` 下。 **物块注册必须提供精灵图**，否则加载失败并跳过。

## 物块脚本

每个物块可以通过 `script` 字段定义四个触发动作的脚本文件列表，工作方式与[物品脚本](item.md#物品脚本)一致。

### 脚本动作

| 动作键         | 触发时机             | 说明                                 |
|----------------|----------------------|--------------------------------------|
| `on_place`     | 物块被放置到世界时   | 通过 `WorldGeneration.SetBlock` 拦截 |
| `on_exist`     | 物块存在时（周期性） | 每秒扫描玩家周围半径 10 格内的物块   |
| `on_damaging`  | 物块受到伤害时       | 通过伤害方法补丁拦截                 |
| `on_destroyed` | 物块被完全破坏时     | 通过 `SetBlock` 拦截（索引变化）     |

每个动作的值是脚本文件路径数组，路径相对于模组目录。多个脚本按顺序执行。

### 脚本函数签名

脚本需导出 `main` 函数，接收三个参数：

```js
// JS 示例
function main(tileId, context, action) {
    // tileId: 物块 ID（如 "marble"）
    // context: { tileIndex, posX, posY } — 物块索引和世界坐标
    // action: 触发动作名（如 "on_place"）
    console.log(tileId + " at (" + context.posX + ", " + context.posY + ") " + action);
}
```

```lua
-- Lua 示例
function main(tileId, context, action)
    -- context: CS.Bark.Tile.TileScriptContext 对象
    print(tileId .. " at (" .. context.PosX .. ", " .. context.PosY .. ") " .. action)
end
```

也可以通过 `custom_data` 向脚本暴露信息：

- **JS 脚本**中通过 `CS.CUCoreLib.Registries.TileRegistry.TryGetCustomData(tileIndex)` 读取 `custom_data`

## 注意事项

- 物块索引由 Bark 自动分配（>= 36），模组无需在 `mod.json` 中声明
- 物块 ID 取自 JSON 文件名（不含扩展名），按字母排序以确定索引分配顺序
- JSON 字段使用 `snake_case`
- 开发时无需重启游戏，`script reload` / `sr` 会重载物块定义
