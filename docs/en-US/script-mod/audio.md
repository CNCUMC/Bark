***English*** | [简体中文](../zh-CN/script-mod/audio.md)

# Custom Audio

Bark uses `AudioManager` to manage custom audio loading. Audio files live under the plugin directory and are auto-cached by `AssetLoader` — no need to manually reload.

## Directory Layout

```
ScriptMod/Mods/MyMod/
  Assets/
    Audio/                  ← custom audio files go here
      ak47_shot.wav
      ak47_rack.wav
      shotgun_blast.mp3
      reload_click.wav
```

> 📁 Paths are relative to the **script root directory** (not the plugin directory). The `Assets/Audio/` directory is not auto-created — create it manually and place your files there.

## Supported Formats

| Format               | Supported        |
|----------------------|------------------|
| `.wav`               | ✅               |
| `.mp3` `.mp1` `.mp2` | ✅               |
| `.aif` `.aiff`       | ✅               |
| `.cue`               | ✅               |
| `.ogg`               | ❌ Not supported |

## Using in Templates

Gun template's `fire_sound`, `rack_sound`, `unrack_sound` take relative paths directly:

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

Paths are relative to the mod root directory and must include the file extension.

**Bare filename auto-completion**: if the path contains no `/` or `\`, it is automatically prefixed with `Assets/Audio/`.  
E.g. `"fire_sound": "ak47_shot.wav"` is equivalent to `"Assets/Audio/ak47_shot.wav"`.

### Audio Fallback Rules

| Field          | Behavior when empty                                                                      |
|----------------|------------------------------------------------------------------------------------------|
| `fire_sound`   | Auto-selects default game SFX based on `ammo_type` (pistol/rifle/shotgun)                |
| `rack_sound`   | Uses game default `"gunrack"` sound                                                      |
| `unrack_sound` | Uses its own value if set; **falls back to rack sound** if empty but `rack_sound` is set |

### Default Sound Mapping

| ammo_type                  | Default fire sound                               |
|----------------------------|--------------------------------------------------|
| Shotgun / 12gauge variants | `sounds/shotgunshot`                             |
| Rifle variants             | `sounds/rifleshot` (falls back to `shotgunshot`) |
| Others                     | `sounds/pistolshot`                              |

## Performance

`AssetLoader.LoadAudioFromPluginFolder` **auto-caches by full file path**:

- Same audio file referenced multiple times loads only once
- After hot reload, cached `AudioClip` objects are not re-loaded
- No manual cache API calls needed

## Manual Loading in Code

If you need to load audio manually from plugins or scripts, use `AudioManager`:

```csharp
// C# side
var clip = AudioManager.LoadCustomAudio("Audio/my_sound.wav");
if (clip != null)
    audioSource.clip = clip;
```

```js
// Script side — AudioManager is exposed to scripts
var clip = AudioManager.LoadCustomAudio("Audio/my_sound.wav");
CUCoreUtils.PlaySoundAt(clip, 0.7, 0, transform.position, 1.0);
```

Returns `null` when the file is missing or format unsupported — callers should null-check and fall back.
