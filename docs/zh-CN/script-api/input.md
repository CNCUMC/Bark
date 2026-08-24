[English](../../en-US/script-api/input.md) | ***简体中文***

# Input — 鼠标与按键

Input 提供鼠标位置和按键友好名称，用于构建 UI 提示和输入处理。

## 鼠标位置

获取鼠标在世界坐标中的当前位置。

```js
// 鼠标世界坐标
var pos = Input.GetMousePosition();
Log.Info('鼠标位置: ' + pos.x + ', ' + pos.y);
```

| 方法                  | 返回      | 说明                 |
|-----------------------|-----------|----------------------|
| `GetMousePosition()`  | `Vector2` | 鼠标世界坐标         |

## 按键友好名称

将 `KeyCode` 转换为人类可读名称，用于在设置 UI 中显示按键绑定提示。

```js
// KeyCode.Mouse0 → "Left Click"
var name = Input.GetFriendlyKeyName(KeyCode.Mouse0);
Log.Info('绑定按键: ' + name);
```

| 方法                               | 返回      | 说明                                 |
|------------------------------------|-----------|--------------------------------------|
| `GetFriendlyKeyName(KeyCode)`      | `string`  | 可读名称，如 `Mouse0` → `Left Click` |
