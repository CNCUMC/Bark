***English*** | [简体中文](../../zh-CN/script-api/storage.md)

# Storage — Persisted Values

Storage reads and writes small values that persist across sessions using the game's `PlayerPrefs`. Useful for saving
your mod's own settings or progress.

## Reading / Writing

```js
// Booleans
Storage.SetBool('my_mod.feature_enabled', true);
var enabled = Storage.GetBool('my_mod.feature_enabled', false);

// Numbers
Storage.SetInt('my_mod.kills', 10);
Storage.SetFloat('my_mod.volume', 0.8);
var kills = Storage.GetInt('my_mod.kills', 0);

// Strings
Storage.SetString('my_mod.player_note', 'hello');
var note = Storage.GetString('my_mod.player_note', '');
```

| Method                               | Returns  | Description                              |
|--------------------------------------|----------|------------------------------------------|
| `GetBool(key, defaultValue?)`        | `bool`   | Read a boolean, fallback to default      |
| `SetBool(key, value)`                | —        | Write a boolean                          |
| `GetInt(key, defaultValue?)`         | `int`    | Read an integer, fallback to default     |
| `SetInt(key, value)`                 | —        | Write an integer                         |
| `GetFloat(key, defaultValue?)`       | `float`  | Read a float, fallback to default        |
| `SetFloat(key, value)`               | —        | Write a float                            |
| `GetString(key, defaultValue?)`      | `string` | Read a string, fallback to default       |
| `SetString(key, value)`              | —        | Write a string                           |

> Default values are returned when the key has never been written. Use namespaced keys (e.g. `my_mod.*`) to avoid
> collisions with other mods.
