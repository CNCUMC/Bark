***English*** | [简体中文](../../zh-CN/script-api/input.md)

# Input — Mouse & Keys

Input provides mouse position and friendly key-name helpers for building UI hints and input handling.

## Mouse Position

Get the mouse's current position in world coordinates.

```js
// Current mouse position in world space
var pos = Input.GetMousePosition();
Log.Info('Mouse at: ' + pos.x + ', ' + pos.y);
```

| Method               | Returns   | Description                              |
|----------------------|-----------|------------------------------------------|
| `GetMousePosition()` | `Vector2` | Mouse position in world coordinates      |

## Friendly Key Names

Convert a `KeyCode` into a human-readable name, useful for displaying keybind hints in settings UI.

```js
// KeyCode.Mouse0 → "Left Click"
var name = Input.GetFriendlyKeyName(KeyCode.Mouse0);
Log.Info('Bound to: ' + name);
```

| Method                        | Returns | Description                                   |
|-------------------------------|---------|-----------------------------------------------|
| `GetFriendlyKeyName(KeyCode)` | `string`| Readable name, e.g. `Mouse0` → `Left Click`    |
