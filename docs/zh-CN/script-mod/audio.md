[English](../../en-US/script-mod/audio.md) | ***简体中文***

# 自定义音效

Bark 通过 `AudioManager` 统一管理自定义音效加载。音效文件放在插件目录下，由 `AssetLoader` 自动缓存，无需重复加载。

## 目录结构

```
ScriptMod/Mods/MyMod/
  Assets/
    Audio/                  ← 自定义音效放这里
      ak47_shot.wav
      ak47_rack.wav
      shotgun_blast.mp3
      reload_click.wav
```

> 📁 路径相对于**脚本根目录**（而非插件目录）。`Assets/Audio/` 不会自动创建——请手动建立并存好文件。

## 支持格式

| 格式                 | 支持      |
|----------------------|-----------|
| `.wav`               | ✅        |
| `.mp3` `.mp1` `.mp2` | ✅        |
| `.aif` `.aiff`       | ✅        |
| `.cue`               | ✅        |
| `.ogg`               | ❌ 不支持 |

## 用在模板中

枪械模板的 `fire_sound`、`rack_sound`、`unrack_sound` 直接写相对路径：

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

路径相对于模组根目录，格式须带扩展名。

**纯文件名自动补全**：如果不含 `/` 或 `\`，自动补全为 `Assets/Audio/filename`。  
例如 `"fire_sound": "ak47_shot.wav"` 等价于 `"Assets/Audio/ak47_shot.wav"`。

### 音效回退规则

| 字段           | 为空时的行为                                               |
|----------------|------------------------------------------------------------|
| `fire_sound`   | 按 `ammo_type` 自动选择默认游戏音效（手枪/步枪/霰弹枪）    |
| `rack_sound`   | 使用游戏默认 `"gunrack"` 音效                              |
| `unrack_sound` | 优先取自身值，为空但 `rack_sound` 不为空时**复用上膛音效** |

### 默认音效对照

| ammo_type            | 默认开火音效                             |
|----------------------|------------------------------------------|
| Shotgun / 12gauge 类 | `sounds/shotgunshot`                     |
| Rifle / 步枪类       | `sounds/rifleshot`（回退 `shotgunshot`） |
| 其他                 | `sounds/pistolshot`                      |

## 性能

`AssetLoader.LoadAudioFromPluginFolder` 按**完整文件路径自动缓存**：

- 同一音效文件多次引用只加载一次
- 热重载后已缓存的 `AudioClip` 不会被重复加载
- 不需要手动调用缓存 API

## 代码中手动加载

如果你在插件或脚本中需要手动加载音效，通过 `AudioManager`：

```csharp
// C# 侧
var clip = AudioManager.LoadCustomAudio("Audio/my_sound.wav");
if (clip != null)
    audioSource.clip = clip;
```

```js
// 脚本侧 — AudioManager 已暴露给脚本
var clip = AudioManager.LoadCustomAudio("Audio/my_sound.wav");
CUCoreUtils.PlaySoundAt(clip, 0.7, 0, transform.position, 1.0);
```

返回 `null` 表示文件不存在或格式不支持，调用方应做空值检查并回退。
