***English*** | [简体中文](../../../zh-CN/script-mod/item-template/gun.md)

← [Back to Template Overview](index.md)

# Gun Template

`"type": "gun"` — a fireable gun item. The template presets the `wolfsrife` prefab (the base game's rifle logic), then
overrides its behavior via Harmony patches.

## Parameters

```json
{
  "template": {
    "type": "gun",
    "gun": true,
    "ammo_type": "7_62x51mm",
    "gun_type": "rifle",
    "damage": 42,
    "fire_sound": "ak47_shot.wav",
    "rack_sound": "ak47_rack.wav",
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
    "barrel_offset": { "x": 0.5, "y": 0.0 }
  }
}
```

| Parameter               | Type   | Default                | Description                                                                                                                                                                                                                  |
|-------------------------|--------|------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `gun`                   | bool   | `true`                 | Internal marker, **do not remove**                                                                                                                                                                                           |
| `ammo_type`             | string | `"7_62x51mm"`          | Accepted ammo caliber, e.g. `"9mm"` `"12_gauge"`                                                                                                                                                                             |
| `gun_type`              | string | `"rifle"`              | Gun category, selects default SFX/sprite/muzzle-particle branch: `"pistol"` / `"rifle"` / `"shotgun"`. Set explicitly by the author — **do not infer from ammo caliber** (caliber ≠ gun type)                                |
| `damage`                | float  | `42`                   | Damage per shot                                                                                                                                                                                                              |
| `fire_sound`            | string | `""`                   | Fire SFX path, relative to mod root directory, e.g. `"Assets/Audio/ak47_shot.wav"`. Bare filename auto-prepends `Assets/Audio/`. Empty = auto-select based on `gun_type`                                                     |
| `rack_sound`            | string | `""`                   | Rack SFX path. Empty = game default `"gunrack"`                                                                                                                                                                              |
| `unrack_sound`          | string | `""`                   | Unrack SFX path. Empty but `rack_sound` set = falls back to rack sound                                                                                                                                                       |
| `mag_type`              | string | `"rifle_mag"`          | Matching magazine type tag                                                                                                                                                                                                   |
| `capacity`              | int    | `15`                   | Used only by magazine-less guns: tube capacity for direct-loading guns (shotguns etc.) and cylinder capacity for revolvers. **Ignored by magazine-fed guns** — capacity comes from the matched mag (`mag_type`)'s `capacity` |
| `is_direct_loading`     | bool   | `false`                | `true` = direct load (pump shotguns with no magazine)                                                                                                                                                                        |
| `full_auto`             | bool   | `true`                 | `true` = hold to fire, `false` = semi-auto                                                                                                                                                                                   |
| `fire_interval`         | float  | `0.08`                 | Full-auto interval (seconds), smaller = faster                                                                                                                                                                               |
| `reload_time`           | float  | `2.5`                  | Reload duration (seconds)                                                                                                                                                                                                    |
| `recoil_force`          | float  | `1.2`                  | Recoil magnitude                                                                                                                                                                                                             |
| `recoil_variance`       | float  | `0.15`                 | Recoil random spread                                                                                                                                                                                                         |
| `recoil_recovery`       | float  | `0.8`                  | Recoil recovery speed (higher = faster return)                                                                                                                                                                               |
| `muzzle_velocity`       | float  | `350`                  | Muzzle velocity, affects bullet travel speed                                                                                                                                                                                 |
| `durability_per_shot`   | float  | `0.05`                 | Durability % consumed per shot                                                                                                                                                                                               |
| `durability_per_reload` | float  | `0.05`                 | Durability % consumed per reload                                                                                                                                                                                             |
| `barrel_offset`         | object | `{ "x": 0.5, "y": 0 }` | Muzzle position offset                                                                                                                                                                                                       |

## Ammo Matching

```json
// Gun ammo_type = "7_62x51mm"
// Ammo ammo_type = "7_62x51mm"
//   → match ✅

// Gun ammo_type = "9mm"
// Ammo ammo_type = "7_62x51mm"
//   → no match ❌
```

## Magazine Matching

Guns match magazines via `mag_type` + `ammo_type` double check:

```json
// Gun: mag_type = "ak_mag", ammo_type = "7_62x51mm"
// Mag: mag_type = "ak_mag" ✅ ammo_type = "7_62x51mm" ✅ → usable

// Gun: mag_type = "ar15_mag", ammo_type = "5_56x45mm"
// Mag: mag_type = "ar15_mag", ammo_type = "9mm"
//   → mag_type match ✅ ammo_type mismatch ❌ → not usable
```
