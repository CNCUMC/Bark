[English](../../en-US/script-api/storage.md) | ***简体中文***

# Storage — 持久化存储

Storage 通过游戏的 `PlayerPrefs` 读写跨会话保留的小数据，适合保存脚本模组自己的设置或进度。

## 读写

```js
// 布尔值
Storage.SetBool('my_mod.feature_enabled', true);
var enabled = Storage.GetBool('my_mod.feature_enabled', false);

// 数字
Storage.SetInt('my_mod.kills', 10);
Storage.SetFloat('my_mod.volume', 0.8);
var kills = Storage.GetInt('my_mod.kills', 0);

// 字符串
Storage.SetString('my_mod.player_note', 'hello');
var note = Storage.GetString('my_mod.player_note', '');
```

| 方法                               | 返回      | 说明                             |
|------------------------------------|-----------|----------------------------------|
| `GetBool(key, defaultValue?)`      | `bool`    | 读取布尔值，未写入时返回默认值   |
| `SetBool(key, value)`              | —         | 写入布尔值                       |
| `GetInt(key, defaultValue?)`       | `int`     | 读取整数，未写入时返回默认值     |
| `SetInt(key, value)`               | —         | 写入整数                         |
| `GetFloat(key, defaultValue?)`     | `float`   | 读取浮点，未写入时返回默认值     |
| `SetFloat(key, value)`             | —         | 写入浮点                         |
| `GetString(key, defaultValue?)`    | `string`  | 读取字符串，未写入时返回默认值   |
| `SetString(key, value)`            | —         | 写入字符串                       |

> 当 key 从未写入时返回默认值。请使用带命名空间的 key（如 `my_mod.*`）避免与其他模组冲突。
