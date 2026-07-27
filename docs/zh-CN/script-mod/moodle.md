[English](../../en-US/script-mod/moodle.md) | ***简体中文***

# 自定义 Moodle

通过 JSON 定义自定义角色状态（Moodle），如流血、感染、中毒等。JSON 文件放在脚本模组的 `Moodle/` 目录下，精灵图片放在 `Assets/Moodle/` 下。

Moodle 系统基于游戏内建的状态队列机制——每次应用 Moodle 都会根据 `hold_seconds` 自动设定到期时间，到期后自动消失并触发 `onMoodleLose` 事件。

## 目录结构

```
ScriptMod/Mods/
  MyMod/
    mod.json
    main.js
    Moodle/
      bleeding.json          ← 自定义 Moodle
      poison.json
    Assets/Moodle/
      bleeding.png           ← 自定义精灵图 (可选)
      poison.png
```

## JSON 格式

```json
{
  "intensity": 2,
  "name": "大出血",
  "description": "伤口正在大量失血，必须立即止血！",
  "critical": true,
  "important": true,
  "key": "severe_bleeding",
  "hold_seconds": 30,
  "icon_id": "bleeding",
  "script": {
    "get": ["bleeding_get.js"],
    "iterate": ["bleeding_tick.js"],
    "lose": ["bleeding_lose.js"]
  }
}
```

### 字段说明

| 字段            | 类型     | 默认值  | 说明                                                                  |
|-----------------|----------|---------|-----------------------------------------------------------------------|
| `intensity`     | int      | `1`     | 强度等级，影响图标显示大小和优先级                                    |
| `name`          | string   | 必填    | 显示名称，需本地化（对应 locale 中 `moodle.{key}.name`）              |
| `description`   | string   | `""`    | 描述文本，需本地化（对应 locale 中 `moodle.{key}.description`）       |
| `critical`      | bool     | `false` | 是否为严重状态，影响 UI 警告强度                                      |
| `chipped_only`  | bool     | `false` | 仅消耗品显示模式                                                      |
| `important`     | bool     | `true`  | 重要状态显示在主区域（true），否则显示在侧边栏（false）               |
| `key`           | string   | 自动生成 | Moodle 唯一标识。不填则用文件名 snake_case 化自动生成                 |
| `hold_seconds`  | float    | `0.75`  | 持续时间（秒），到期后自动消失                                        |
| `icon_id`       | string   | null    | 游戏内置图标 ID，如 `"bleeding"`、`"hunger"`                          |
| `icon_asset`    | string   | null    | 自定义精灵图路径，如 `"Assets/Moodle/bleeding.png"`。与 `icon_id` 互斥，优先用 `icon_id` |
| `sprite_scale`  | float    | `0.5`   | 自定义精灵缩放倍数。数值越大精灵越大，1 = 16 PPU 基准                 |
| `animated`      | bool     | `false` | 使用动画 Moodle。启用后 `icon_id` / `icon_asset` 均被忽略             |
| `animation_id`  | string   | null    | 动画 ID（仅 `animated = true` 时有效）                                |
| `script`        | object   | null    | 脚本触发定义（见下文）                                                |

## 图标来源（优先级从高到低）

Moodle 图标按以下优先级查找，找到即停止：

| 优先级 | 来源                                       | 说明                                      |
|--------|--------------------------------------------|-------------------------------------------|
| 1      | `animated` + `animation_id`                | 动画 Moodle                               |
| 2      | `icon_id`                                  | 游戏内置图标                              |
| 3      | `icon_asset`                               | 自定义精灵图路径                          |
| 4      | 自动查找 `Assets/Moodle/{key}.png`         | 与物品自动查找 `Assets/Item/{id}.png` 一致 |

如果以上全部失败，该 Moodle 将**跳过注册**并输出 warning 日志。这意味着你甚至可以不写任何图标字段——只要在 `Assets/Moodle/` 下放一张与 key 同名的 `.png` 图片即可。

### 精灵缩放

当使用自定义精灵图（`icon_asset` 或自动查找）时，`sprite_scale` 控制精灵的渲染缩放。基准值为 1（16 PPU），缩小数值使精灵变大：

```json
{
  "name": "感染",
  "icon_asset": "infection.png",
  "sprite_scale": 0.5
}
```

精灵加载时使用 Point 过滤（无模糊），适合像素风格素材。

## Moodle Key 生成规则

key 的生成优先级：

1. 如果 JSON 中指定了 `key` 字段，直接使用（转小写）
2. 否则取文件名（不含 `.json`），转为 snake_case

例如：文件 `Severe Bleeding.json` → key 自动为 `severe_bleeding`。

> ℹ️ 两个模组定义相同 key 的 Moodle 时，后加载的覆盖先加载的。key 用于全局唯一引用，脚本中调用 `Moodle.ApplyMoodle()` 时传入的就是这个 key。

## Moodle 脚本

`script` 字段将脚本文件绑定到 Moodle 的三个生命周期阶段。当阶段触发时，Bark 依次执行每个脚本。

### 生命周期阶段

| 键        | 触发时机                 | 对应事件钩子      |
|-----------|--------------------------|-------------------|
| `get`     | Moodle 被应用到玩家身上  | `onMoodleGet`     |
| `iterate` | Moodle 存在期间轮询（每 0.5 秒） | `onMoodleIterate` |
| `lose`    | Moodle 到期或被移除时    | `onMoodleLose`    |

### 脚本路径

路径相对于脚本模组目录：

```json
{
  "script": {
    "get": ["Scripts/poison_get.js"],
    "iterate": ["Scripts/poison_tick.js"],
    "lose": ["Scripts/poison_lose.js"]
  }
}
```

每个阶段的脚本列表按顺序执行，可指定多个脚本。

## 完整示例

一个中毒 Moodle，使用自动查找图标：

**`Moodle/poison.json`**：

```json
{
  "intensity": 3,
  "name": "中毒",
  "description": "毒素正在蔓延全身，持续造成伤害。",
  "critical": true,
  "hold_seconds": 60,
  "script": {
    "get": ["poison_get.js"],
    "iterate": ["poison_tick.js"]
  }
}
```

**`poison_get.js`**（放在模组根目录）：

```js
function main(moodleKey) {
    Log.Info('玩家中毒了！key = ' + moodleKey);
    Player.Alert('你中毒了！快找解药', true);
}
```

**`poison_tick.js`**：

```js
function main(moodleKey) {
    // 每次轮询造成 2 点伤害
    var hp = Body.GetBloodVolume();
    Body.SetBloodVolume(hp - 2);
}
```

**`Assets/Moodle/poison.png`**：放在 `Assets/Moodle/` 下，Bark 自动查找加载。

## 在脚本中操作 Moodle

通过 `Moodle` 全局变量：

```js
// 应用 Moodle（使用 JSON 中定义的默认持续时间）
Moodle.ApplyMoodle('poison');

// 应用 Moodle 并覆盖持续时间（5 秒后消失）
Moodle.ApplyMoodle('bleeding', 5);

// 强制移除
Moodle.RemoveMoodle('poison');

// 查询
if (Moodle.HasMoodle('poison')) {
    Log.Info('玩家仍在中度');
}

// 获取所有活跃的 Moodle
var actives = Moodle.GetActiveMoodles();
Log.Info('活跃状态数: ' + Moodle.GetMoodleCount());

// 查询属性
var intensity = Moodle.GetIntensity('poison');
var isCritical = Moodle.IsCritical('poison');
```

完整 API 见 [Moodle](moodle.md)。

## 注意事项

- 热重载（`script reload` / `rs`）会重新加载所有 Moodle 定义
- 同 key 覆盖：后加载的模组定义覆盖先加载的
- JSON 字段使用 `snake_case` 命名
- Moodle 脚本的 `main` 函数接收一个参数：`moodleKey`（string），Moodle 的唯一标识
- 轮询间隔固定 0.5 秒，不宜在 `iterate` 脚本中做过于频繁的操作
