[English](../../../en-US/script-mod/item-template/gun.md) | ***简体中文***

← [返回模板总览](index.md)

# 枪械模板

`"type": "gun"` — 可开火的枪械物品。模板预设了 `wolfsrife` 预制体（原生游戏猎枪的物品逻辑），然后通过 Harmony 补丁覆盖其行为。

## 参数

```json
{
  "template": {
    "type": "gun",
    "gun": true,
    "ammo_type": "7_62x51mm",
    "damage": 42,
    "fire_sound": "Assets/Audio/ak47_shot.wav",
    "rack_sound": "Audio/ak47_rack.wav",
    "unrack_sound": "",
    "mag_type": "ar15_mag",
    "capacity": 15,
    "is_direct_loading": false,
    "full_auto": true,
    "fire_interval": 0.08,
    "reload_time": 2.5,
    "recoil_force": 1.2,
    "recoil_variance": 0.15,
    "recoil_recovery": 0.8,
    "muzzle_velocity": 350.0,
    "durability_per_shot": 0.05,
    "durability_per_reload": 0.05,
    "barrel_offset": {
      "x": 0.5,
      "y": 0.0
    }
  }
}
```

| 参数                    | 类型   | 默认值                 | 说明                                                                                                                                          |
|-------------------------|--------|------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------|
| `gun`                   | bool   | `true`                 | 内部标记，**不要删除**                                                                                                                        |
| `ammo_type`             | string | `"7_62x51mm"`          | 接受的弹药口径，如 `"9mm"` `"12_gauge"`                                                                                                       |
| `damage`                | float  | `42`                   | 单发伤害                                                                                                                                      |
| `fire_sound`            | string | `""`                   | 开火音效路径，相对于模组根目录，如 `"Assets/Audio/ak47_shot.wav"`。纯文件名自动补全 `Assets/Audio/` 前缀。为空则按 `ammo_type` 自动选默认音效 |
| `rack_sound`            | string | `""`                   | 拉膛音效路径，为空用游戏默认 `"gunrack"`                                                                                                      |
| `unrack_sound`          | string | `""`                   | 回膛音效路径。为空但 `rack_sound` 不为空时，复用上膛音效                                                                                      |
| `mag_type`              | string | `"rifle_mag"`          | 匹配的弹匣类型标签                                                                                                                            |
| `capacity`              | int    | `15`                   | 直装枪械的管容量（霰弹枪等无弹匣枪械）。有弹匣时此值被弹匣容量覆盖                                                                            |
| `is_direct_loading`     | bool   | `false`                | `true` = 直接装填（泵动霰弹枪等无弹匣枪械）                                                                                                   |
| `full_auto`             | bool   | `true`                 | `true` = 按住连发，`false` = 单发                                                                                                             |
| `fire_interval`         | float  | `0.08`                 | 连发间隔（秒），越小射速越快                                                                                                                  |
| `reload_time`           | float  | `2.5`                  | 换弹时间（秒）                                                                                                                                |
| `recoil_force`          | float  | `1.2`                  | 后坐力大小                                                                                                                                    |
| `recoil_variance`       | float  | `0.15`                 | 后坐力随机偏移量                                                                                                                              |
| `recoil_recovery`       | float  | `0.8`                  | 后坐力恢复速度（越大回正越快）                                                                                                                |
| `muzzle_velocity`       | float  | `350`                  | 枪口初速，影响子弹飞行速度                                                                                                                    |
| `durability_per_shot`   | float  | `0.05`                 | 每次开火消耗的耐久百分比                                                                                                                      |
| `durability_per_reload` | float  | `0.05`                 | 每次换弹消耗的耐久百分比                                                                                                                      |
| `barrel_offset`         | object | `{ "x": 0.5, "y": 0 }` | 枪口位置偏移                                                                                                                                  |

## 弹药匹配逻辑

```json
// 枪械 ammo_type = "7_62x51mm"
// 弹药 ammo_type = "7_62x51mm"
//   → 匹配成功，可以装弹 ✅

// 枪械 ammo_type = "9mm"
// 弹药 ammo_type = "7_62x51mm"
//   → 不匹配，无法装弹 ❌
```

## 弹匣匹配逻辑

枪械通过 `mag_type` + `ammo_type` 双重匹配弹匣：

```json
// 枪械: mag_type = "ak_mag"
// 弹匣: mag_type = "ak_mag" ❌ ammo_type = "7_62x51mm"
//   → mag_type 匹配 ✅ ammo_type 匹配 ✅ → 可用

// 枪械: mag_type = "ar15_mag", ammo_type = "5_56x45mm"
// 弹匣: mag_type = "ar15_mag", ammo_type = "9mm"
//   → mag_type 匹配 ✅ ammo_type 不匹配 ❌ → 不可用
```
