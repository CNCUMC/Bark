[English](../../en-US/script-api/moodle.md) | ***简体中文***

# Moodle — 自定义状态管理

Moodle 用于给玩家应用、移除和查询自定义 Moodle 状态。所有 Moodle 定义来自脚本模组 `Moodle/` 目录下的 JSON 文件。

## 方法一览

| 方法                                  | 说明                     |
|---------------------------------------|--------------------------|
| `ApplyMoodle(key, holdSeconds?)`      | 应用一个已定义的 Moodle  |
| `RemoveMoodle(key)`                   | 移除指定 Moodle          |
| `HasMoodle(key)`                      | 检查 Moodle 是否活跃     |
| `GetActiveMoodles()`                  | 获取所有活跃 Moodle 的 key 列表 |
| `GetMoodleCount()`                    | 获取活跃 Moodle 数量     |
| `GetIntensity(key)`                   | 获取强度                 |
| `GetName(key)`                        | 获取名称                 |
| `GetDescription(key)`                 | 获取描述                 |
| `GetHoldSeconds(key)`                 | 获取持续时间             |
| `IsCritical(key)`                     | 是否为严重状态           |
| `IsChippedOnly(key)`                  | 是否仅消耗品显示         |
| `IsImportant(key)`                    | 是否重要状态             |

## 应用 Moodle

```js
// 使用默认持续时间（JSON 中定义的 hold_seconds）
Moodle.ApplyMoodle('bleeding');

// 覆盖持续时间：5 秒后自动消失
Moodle.ApplyMoodle('bleeding', 5);

// 如果 key 未在已加载的定义中找到，输出 warning 日志，不会抛异常
Moodle.ApplyMoodle('non_existent');  // 安全，什么都不发生
```

`holdSeconds` 为可选参数。传入 0 或负数时使用 JSON 中定义的默认持续时间。

## 移除 Moodle

```js
// 标记到期，下次轮询时触发 lose 事件并移除
var removed = Moodle.RemoveMoodle('bleeding');
if (removed) {
    Log.Info('流血状态已移除');
}
```

返回 `true` 表示该 Moodle 存在并被标记移除，`false` 表示该 key 不存在或已消失。

## 查询 Moodle

```js
// 检查是否活跃
if (Moodle.HasMoodle('poison')) {
    Log.Warning('玩家中度了');
}

// 获取所有活跃 Moodle
var keys = Moodle.GetActiveMoodles();
for (var i = 0; i < keys.length; i++) {
    Log.Info('活跃: ' + keys[i]);
}

// 数量
var count = Moodle.GetMoodleCount();
Log.Info('当前共 ' + count + ' 个状态效果');
```

## 读取 Moodle 属性

从已加载的 JSON 定义中读取属性（非运行时状态）：

```js
var name = Moodle.GetName('bleeding');           // 显示名称
var desc = Moodle.GetDescription('bleeding');    // 描述
var intensity = Moodle.GetIntensity('bleeding'); // 强度等级
var duration = Moodle.GetHoldSeconds('bleeding');// 默认持续时间
var isCrit = Moodle.IsCritical('bleeding');      // 是否严重
var isImportant = Moodle.IsImportant('bleeding');// 是否重要
```

如果 key 对应的定义不存在，属性方法返回安全的默认值（`0`、`false`、空字符串）。

## 完整示例

一个自动检测并通知严重状态的脚本：

```js
function onMoodleGet(event) {
    // event.MoodleKey, event.MoodleName, event.Intensity, event.Critical, event.HoldSeconds
    if (event.Critical) {
        Player.Alert('你获得了严重状态: ' + event.MoodleName, true);
    }
}

function onWorldGenerated() {
    // 每 2 秒检查所有严重状态
    setInterval(function() {
        var keys = Moodle.GetActiveMoodles();
        for (var i = 0; i < keys.length; i++) {
            if (Moodle.IsCritical(keys[i])) {
                Log.Warning('严重状态活跃: ' + Moodle.GetName(keys[i]));
            }
        }
    }, 2000);
}
```

## 注意事项

- `ApplyMoodle` 需要 Moodle 定义已在 JSON 中加载，否则不会生效（warning 日志）
- 同 key 的 Moodle 重复应用会刷新过期时间（重新计时）
- `RemoveMoodle` 不会立即移除，而是标记为下个轮询周期处理（最多 0.5 秒延迟）
- 属性查询方法读取的是 JSON 定义中的静态值，不是运行时变化的值
- 全局变量名为 `Moodle`，在 JS 和 Lua 中均可直接调用
