[English](../../en-US/script-mod/audio.md) | ***简体中文***

# 自定义音效

Bark 提供 **两种音效配置方式**：简单模式（单文件路径）和音效档案模式（JSON 多音效随机池）。

简单模式适合快速原型，档案模式适合需要音效变化、音量/音高定制的正式作品。

## 目录结构

```
ScriptMod/Mods/MyMod/
  Audio/                          ← 音效档案 JSON 放这里
    ak47.json
    shotgun.json
  Assets/
    Audio/                        ← 实际音频文件放这里
      ak47_shot_1.wav
      ak47_shot_2.wav
      ak47_rack.wav
      ak47_trigger.wav
```

> 📁 **JSON 和音频文件分开存放**：`.json` 档案在 `Audio/`，`.wav`/`.mp3` 等音频文件在 `Assets/Audio/`。

## 支持格式

| 格式                 | 支持      |
|----------------------|-----------|
| `.wav`               | ✅        |
| `.mp3` `.mp1` `.mp2` | ✅        |
| `.aif` `.aiff`       | ✅        |
| `.cue`               | ✅        |
| `.ogg`               | ❌ 不支持 |

---

## GunSoundProfile（音效档案）

音效档案用 JSON 定义一把枪 **所有场景**下使用的音效，支持每个场景配置多组音效文件并按权重随机选取。

### SoundEntry 字段

档案中的每个音效条目为一个对象，字段如下：

| 字段     | 类型   | 默认值 | 说明                                         |
|----------|--------|--------|----------------------------------------------|
| `file`   | string | `""`   | 音频文件名，相对于 `Assets/Audio/`           |
| `volume` | float  | `1.0`  | 音量，范围 0.0 ~ 1.0                         |
| `pitch`  | float  | `1.0`  | 音高/播放速度，范围 0.5 ~ 2.0                |
| `weight` | float  | `1.0`  | 随机权重（多个条目中，权重越高越容易被选中） |

### 音效类别

档案支持以下类别，每个类别对应枪械的一个操作场景：

| 类别        | JSON key     | 触发场景                                    |
|-------------|--------------|---------------------------------------------|
| `Fire`      | `fire`       | 扣动扳机，发射弹丸                          |
| `Rack`      | `rack`       | 拉膛（上膛）                                |
| `Unrack`    | `unrack`     | 回膛（退壳）                                |
| `LoadMag`   | `load_mag`   | 装入弹匣                                    |
| `LoadShell` | `load_shell` | 逐发装弹（直装式枪械，一颗颗压入）          |
| `UnloadMag` | `unload_mag` | 卸下弹匣                                    |
| `Trigger`   | `trigger`    | 扣下扳机（击锤/撞针声响，与 Fire 同时触发） |
| `Jam`       | `jam`        | 卡壳（弹药未能正常上膛或抛壳）              |
| `Safety`    | `safety`     | 保险开关切换                                |

每个类别可设为 `null`（不使用档案，回退默认音效）或空数组 `[]`（静音）。若完全不写该字段，等同于回退默认音效。

### 随机选取逻辑

当一个类别有多个 `SoundEntry` 时，按以下方式随机选取 **一个**播放：

- **单条目**：直接播放
- **多条目的权重总和 ≤ 0**：取第一个
- **多条目**：按 `weight` 加权随机，权重越高的越容易被选中
- 选出的条目通过 `AudioSource` 播放，支持独立的 `volume` 和 `pitch`

### 音量/音高优化

如果被选中的条目 `volume` 为 1.0 且 `pitch` 为 1.0（默认值），直接使用游戏的 `Sound.Play` 播放（保留 3D 衰减逻辑）。只有自定义参数时才创建临时
`AudioSource`。

### JSON 完整示例

```json
{
  "fire": [
    { "file": "ak47_shot_1.wav", "volume": 0.9, "pitch": 1.0, "weight": 3 },
    { "file": "ak47_shot_2.wav", "volume": 0.85, "pitch": 0.97, "weight": 2 },
    { "file": "ak47_shot_3.wav", "volume": 0.88, "pitch": 1.03, "weight": 1 }
  ],
  "rack": [
    { "file": "ak47_rack.wav", "volume": 0.6, "pitch": 1.0, "weight": 1 }
  ],
  "unrack": [
    { "file": "ak47_unrack.wav", "volume": 0.55, "pitch": 1.0, "weight": 1 }
  ],
  "load_mag": [
    { "file": "ak47_load_mag.wav", "volume": 0.5, "pitch": 1.1, "weight": 1 }
  ],
  "load_shell": [
    { "file": "ak47_load_shell.wav", "volume": 0.4, "pitch": 1.0, "weight": 1 }
  ],
  "unload_mag": [
    { "file": "ak47_unload_mag.wav", "volume": 0.5, "pitch": 0.95, "weight": 1 }
  ],
  "trigger": [
    { "file": "ak47_trigger.wav", "volume": 0.3, "pitch": 1.0, "weight": 1 }
  ],
  "jam": [
    { "file": "ak47_jam.wav", "volume": 0.7, "pitch": 1.0, "weight": 1 }
  ],
  "safety": [
    { "file": "ak47_safety.wav", "volume": 0.3, "pitch": 0.9, "weight": 1 }
  ]
}
```

---

## 枪械模板中的音效配置

枪械模板 JSON（`items/` 目录下的 `.json`）通过两个字段控制音效：

### 简单模式：`fire_sound` / `rack_sound` / `unrack_sound`

直接指定单个音频文件路径。适合快速原型。

```json
{
  "template": {
    "type": "gun",
    "ammo_type": "7_62x51mm",
    "fire_sound": "Assets/Audio/ak47_shot.wav",
    "rack_sound": "Assets/Audio/ak47_rack.wav",
    "unrack_sound": ""
  }
}
```

- 路径相对于模组根目录， **不含** `/` 或 `\` 时自动补全为 `Assets/Audio/文件名`
- 空字符串表示使用默认游戏音效

**回退规则**：

| 字段           | 为空时的行为                                                                     |
|----------------|----------------------------------------------------------------------------------|
| `fire_sound`   | 按 `ammo_type` 自动选择：手枪→`pistolshot`，步枪→`rifleshot`，霰弹→`shotgunshot` |
| `rack_sound`   | 使用游戏默认 `"gunrack"`                                                         |
| `unrack_sound` | 为空但 `rack_sound` 非空时复用上膛音效；都为空则用默认 `"gununrack"`             |

### 档案模式：`sound`

通过 `sound` 字段引用音效档案（`Audio/` 目录下的 `.json` 文件）：

```json
{
  "template": {
    "type": "gun",
    "ammo_type": "7_62x51mm",
    "sound": "ak47"
  }
}
```

`"sound": "ak47"` 会加载 `Audio/ak47.json` 中的 `GunSoundProfile`。

### 优先级

当同时设置了 `sound` 和简单模式字段时：

- **档案模式始终优先**。一旦 `sound` 加载成功，该类别优先使用档案中的条目
- 档案中某类别为 `null` 或未写时， **退回**简单模式对应的字段
- 简单模式字段也为空时，使用游戏默认音效

以 `Fire` 类别为例的完整回退链：

```
sound.fire (非 null) → fire_sound → ammo_type 默认音效
```

---

## AudioManager API

`AudioManager` 统一管理音效加载，支持两种加载路径：

### 从插件目录加载（通用音效）

```csharp
// C#: 加载 BepInEx/plugins/Bark/ 下的音效
var clip = AudioManager.LoadCustomAudio("Audio/my_sound.wav");
```

### 从模组目录加载（模组专属音效）

```csharp
// C#: 加载 {ModDir}/Assets/Audio/ 下的音效
// relativePath 不含 / 或 \ 时自动补全 Assets/Audio/ 前缀
var clip = AudioManager.LoadModAudio(modDir, "ak47_shot.wav");
```

```js
// 脚本侧 — AudioManager 已暴露给脚本
var clip = AudioManager.LoadModAudio(modInfo.modDir, "my_sound.wav");
if (clip) {
  CUCoreUtils.PlaySoundAt(clip, 0.7, transform.position, 1.0);
}
```

### 自动缓存

两个方法背后的 `AssetLoader` 均按 **完整文件路径自动缓存**：

- 同一文件多次引用只加载一次
- 热重载后已缓存的 `AudioClip` 不会重复加载
- `GunSoundProfile.Load()` 在 JSON 反序列化后自动预加载所有引用的音频文件
- 不需要手动管理缓存

---

## 性能注意

- `GunSoundProfile` 在游戏启动时 **一次性预加载**所有引用的音频文件，运行时不再产生 IO
- 自定义 `volume`/`pitch` 时使用临时 `AudioSource`（播放完毕自动销毁），默认参数下直接用 `Sound.Play` 无额外开销
- 档案 JSON 解析失败会记录错误日志并返回 `null`，不会导致崩溃
