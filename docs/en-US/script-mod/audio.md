***English*** | [简体中文](../zh-CN/script-mod/audio.md)

# Custom Audio

Bark provides **two audio configuration modes**: simple mode (single file path) and sound profile mode (JSON multi-file random pool).

Simple mode is for quick prototyping. Sound profiles are for polished work that needs audio variation and volume/pitch control.

## Directory Layout

```
ScriptMod/Mods/MyMod/
  Audio/                          ← sound profile JSON files go here
    ak47.json
    shotgun.json
  Assets/
    Audio/                        ← actual audio files go here
      ak47_shot_1.wav
      ak47_shot_2.wav
      ak47_rack.wav
      ak47_trigger.wav
```

> 📁 **JSON and audio files are stored separately**: `.json` profiles in `Audio/`, actual `.wav`/`.mp3` files in `Assets/Audio/`.

## Supported Formats

| Format               | Supported        |
|----------------------|------------------|
| `.wav`               | ✅               |
| `.mp3` `.mp1` `.mp2` | ✅               |
| `.aif` `.aiff`       | ✅               |
| `.cue`               | ✅               |
| `.ogg`               | ❌ Not supported |

---

## GunSoundProfile (Sound Profile)

A sound profile is a JSON file that defines sounds for **all scenarios** a gun can encounter. Each scenario can have multiple audio entries randomly selected by weight.

### SoundEntry Fields

Each entry in a scenario list is an object with these fields:

| Field    | Type   | Default | Description                                                    |
|----------|--------|---------|----------------------------------------------------------------|
| `file`   | string | `""`    | Audio filename, relative to `Assets/Audio/`                   |
| `volume` | float  | `1.0`   | Volume, range 0.0–1.0                                          |
| `pitch`  | float  | `1.0`   | Pitch / playback speed, range 0.5–2.0                          |
| `weight` | float  | `1.0`   | Random weight (higher weight = more likely to be selected)     |

### Sound Categories

Each category maps to a specific gun action:

| Category    | JSON key      | Triggered by                                                            |
|-------------|---------------|-------------------------------------------------------------------------|
| `Fire`      | `fire`        | Pulling the trigger, firing projectiles                                 |
| `Rack`      | `rack`        | Racking the slide/bolt (chambering a round)                             |
| `Unrack`    | `unrack`      | Unracking (ejecting a round/casing)                                     |
| `LoadMag`   | `load_mag`    | Inserting a magazine                                                    |
| `LoadShell` | `load_shell`  | Loading individual rounds (direct-feed weapons, one round at a time)    |
| `UnloadMag` | `unload_mag`  | Removing a magazine                                                     |
| `Trigger`   | `trigger`     | Pulling the trigger (firing-pin/hammer sound, simultaneous with fire)   |
| `Jam`       | `jam`         | Jam (round fails to chamber or eject properly)                          |
| `Safety`    | `safety`      | Toggling the safety on/off                                              |

Each category can be `null` (fallback to default sound) or empty array `[]` (silent). If omitted entirely, it falls back to the default sound.

### Random Selection Logic

When a category has multiple `SoundEntry`s, **one** is selected for playback:

- **Single entry**: plays directly
- **Total weight ≤ 0**: picks the first entry
- **Multiple entries**: weighted random by `weight` — higher weight = higher chance
- Selected entry plays via `AudioSource` with its own `volume` and `pitch`

### Volume/Pitch Optimization

If the selected entry has `volume` 1.0 and `pitch` 1.0 (defaults), the game's built-in `Sound.Play` is used directly (preserving 3D falloff). A temporary `AudioSource` is only created when custom values are needed.

### Full JSON Example

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

## Audio in Gun Templates

The gun template JSON (in the `items/` directory) controls audio via two fields:

### Simple Mode: `fire_sound` / `rack_sound` / `unrack_sound`

Specify a single audio file path directly. Good for quick prototypes.

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

- Paths are relative to the mod root directory. Paths without `/` or `\` are auto-prefixed with `Assets/Audio/`.
- Empty string means "use the game default sound".

**Fallback rules**:

| Field          | Behavior when empty                                                                               |
|----------------|----------------------------------------------------------------------------------------------------|
| `fire_sound`   | Auto-selected by `ammo_type`: pistol→`pistolshot`, rifle→`rifleshot`, shotgun→`shotgunshot`       |
| `rack_sound`   | Uses game default `"gunrack"`                                                                     |
| `unrack_sound` | If empty but `rack_sound` is set, reuses the rack sound; otherwise defaults to `"gununrack"`      |

### Profile Mode: `sound`

Use the `sound` field to reference a sound profile (`.json` file in the `Audio/` directory):

```json
{
  "template": {
    "type": "gun",
    "ammo_type": "7_62x51mm",
    "sound": "ak47"
  }
}
```

`"sound": "ak47"` loads the `GunSoundProfile` from `Audio/ak47.json`.

### Priority

When both `sound` and simple-mode fields are set:

- **Profile mode always wins**. Once `sound` loads successfully, that category's profile entries are used first.
- If a category in the profile is `null` or omitted, it **falls back** to the corresponding simple-mode field.
- If the simple-mode field is also empty, the game's default sound is used.

Full fallback chain for the fire category:

```
sound.fire (non-null) → fire_sound → ammo_type default
```

---

## AudioManager API

`AudioManager` provides unified audio loading with two loading paths:

### Load from Plugin Directory (general-purpose audio)

```csharp
// C#: load audio from BepInEx/plugins/Bark/
var clip = AudioManager.LoadCustomAudio("Audio/my_sound.wav");
```

### Load from Mod Directory (mod-specific audio)

```csharp
// C#: load audio from {ModDir}/Assets/Audio/
// relativePath without / or \ is auto-prefixed with Assets/Audio/
var clip = AudioManager.LoadModAudio(modDir, "ak47_shot.wav");
```

```js
// Script side — AudioManager is exposed to scripts
var clip = AudioManager.LoadModAudio(modInfo.modDir, "my_sound.wav");
if (clip) {
  CUCoreUtils.PlaySoundAt(clip, 0.7, transform.position, 1.0);
}
```

### Automatic Caching

Both methods internally use `AssetLoader`, which **caches by full file path**:

- Same file referenced multiple times loads only once
- Cached `AudioClip`s survive hot reload
- `GunSoundProfile.Load()` preloads all referenced audio files right after JSON deserialization
- No manual cache management needed

---

## Performance Notes

- `GunSoundProfile` **preloads all** referenced audio files on load; no runtime IO during gameplay.
- Custom `volume`/`pitch` creates a temporary `AudioSource` (auto-destroyed after playback). Default parameters use `Sound.Play` directly with zero overhead.
- JSON parse failures log an error and return `null` — never crash the game.
