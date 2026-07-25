[English](../../en-US/script-api/locale.md) | ***简体中文***

# LocaleApi — 多语言

LocaleApi 让脚本支持多语言，根据游戏当前语言显示不同文本。通过 `Log.Locale` 访问。

## 基本用法

```js
// 取本地化文本（key 不存在则返回 "[key]"）
var text = Log.Locale.Get('welcome_message');

// 带占位符
var text2 = Log.Locale.GetFormatted('damage_report', 25, '左腿');
// 对应文本 "你受到了 {0} 点伤害，部位: {1}"

// 来自其他模组的文本
var text3 = Log.Locale.GetFrom('other.mod.id', 'greeting');

// 检查 key 是否存在
if (Log.Locale.HasKey('error_msg')) {
    Log.Info(Log.Locale.Get('error_msg'));
}
```

| 方法                                    | 说明                           |
|-----------------------------------------|--------------------------------|
| `Get(key)`                              | 取文本，不存在返回 `[key]`     |
| `GetFormatted(key, ...args)`            | 取格式化文本，`{0}` `{1}` 替换 |
| `GetFrom(modId, key)`                   | 读其他模组的文本               |
| `GetFormattedFrom(modId, key, ...args)` | 读其他模组的格式化文本         |
| `HasKey(key)`                           | key 是否存在                   |

## key 的自动展开

你传的 key 会自动加上模组 id 前缀。比如你的模组 id 是 `my_mod`：

```js
Log.Locale.Get('hello')
// → 实际查找 publish.my_mod.hello
// 如果已经在 option.log.item 等分层前缀下，就保留
```

简单说：直接写短 key 就行，`Locale.Get('greeting')` 等价于查 `log.my_mod.greeting`。

## 配合日志用

LogApi 内置了 `*F` 方法，一步完成本地化 + 输出：

```js
Log.InfoF('welcome', '玩家名');   // GetFormatted + Info
Log.ErrorF('critical_error', 42);  // GetFormatted + Error
```

## 配置 locale 文件

在你的模组目录下的 `Lang` 放 `zh-CN.json`：

```json
{
  "log": {
    "welcome": "欢迎使用本模组！",
    "damage_report": "受到 {0} 点伤害，部位: {1}"
  }
}
```

Bark 会自动加载对应语言的文本。
