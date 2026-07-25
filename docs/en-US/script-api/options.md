***English*** | [简体中文](../../zh-CN/script-api/options.md)

# OptionsApi — Reading Config

OptionsApi lets scripts read configuration options registered by other mods. **Read-only** — cannot write.

```js
// Read another mod's config
var enabled = OptionsApi.GetBool('some.mod.id', 'myOption');
var volume = OptionsApi.GetFloat('some.mod.id', 'volume');
var count = OptionsApi.GetInt('some.mod.id', 'maxItems');
var selection = OptionsApi.GetDropdown('some.mod.id', 'quality');  // selected index
var key = OptionsApi.GetKeybind('some.mod.id', 'hotkey');          // key name
```

| Method                    | Returns  | Description      |
|---------------------------|----------|------------------|
| `GetBool(modId, key)`     | `bool`   | Read bool toggle |
| `GetInt(modId, key)`      | `int`    | Read integer     |
| `GetFloat(modId, key)`    | `float`  | Read float       |
| `GetDropdown(modId, key)` | `int`    | Dropdown index   |
| `GetKeybind(modId, key)`  | `string` | Key binding name |

> ℹ️ Writing config (`Set*`) is not exposed to scripts. Scripts can only read, not write.

## Example: Adapting to Another Mod's Settings

```js
function onLoad() {
    // Read a difficulty mod's damage multiplier
    var damageMult = OptionsApi.GetFloat('difficulty.mod', 'damageMultiplier');
    if (damageMult > 0) {
        Log.Info('Damage multiplier: ' + damageMult);
    }
}
```
