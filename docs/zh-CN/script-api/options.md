[English](../../en-US/script-api/options.md) | ***简体中文***

# OptionsApi — 读配置项

OptionsApi 让脚本读取其他脚本模组注册的配置选项。 **只读**，不能写入。

```js
// 读取其他脚本模组的配置
var enabled = OptionsApi.GetBool('some_mod', 'myOption');
var volume = OptionsApi.GetFloat('some_mod', 'volume');
var count = OptionsApi.GetInt('some_mod', 'maxItems');
var selection = OptionsApi.GetDropdown('some_mod', 'quality');  // 返回选中项索引
var key = OptionsApi.GetKeybind('some_mod', 'hotkey');          // 返回按键名
```

| 方法                      | 返回类型 | 说明           |
|---------------------------|----------|----------------|
| `GetBool(modId, key)`     | `bool`   | 读取布尔开关   |
| `GetInt(modId, key)`      | `int`    | 读取整数       |
| `GetFloat(modId, key)`    | `float`  | 读取浮点数     |
| `GetDropdown(modId, key)` | `int`    | 下拉框选中索引 |
| `GetKeybind(modId, key)`  | `string` | 快捷键绑定     |

> ℹ️ 写配置项（`Set*`）不暴露给脚本。脚本只能读不能写。

## 示例：适配其他脚本模组的设置

```js
function onLoad() {
    // 读取某个脚本模组的伤害倍率
    var damageMult = OptionsApi.GetFloat('difficulty_mod', 'damageMultiplier');
    if (damageMult > 0) {
        Log.Info('伤害倍率: ' + damageMult);
    }
}
```
