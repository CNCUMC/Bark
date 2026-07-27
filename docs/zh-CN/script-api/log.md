[English](../../en-US/script-api/log.md) | ***简体中文***

# LogApi — 日志输出

LogApi 是脚本里输出日志的唯一方式，注入为全局变量 `Log`。输出同时显示在 BepInEx 控制台和写入脚本模组日志文件。

## 日志级别

```js
Log.Debug('调试信息');       // Debug 级别
Log.Info('普通信息');        // Info 级别
Log.Warning('警告');         // Warning 级别
Log.Error('错误！');         // Error 级别
Log.Message('纯消息');       // Message 级别
```

所有级别语法一致，只需要一个参数：字符串。

## 本地化日志

`*F` 后缀的方法先做本地化再输出：

```js
Log.InfoF('welcome', '玩家');     // = Log.Info(Log.Locale.GetFormatted('welcome', '玩家'))
Log.ErrorF('error_code', 500);     // = Log.Error(Log.Locale.GetFormatted('error_code', 500))
Log.WarningF('low_hp', 15);       // = Log.Warning(Log.Locale.GetFormatted('low_hp', 15))
Log.DebugF('loaded', 'MyMod');    // = Log.Debug(Log.Locale.GetFormatted('loaded', 'MyMod'))
```

## 格式化输出

```js
Log.NewLine();             // 空行
Log.Divider();             // 默认分隔线 "---------------------------"
Log.Divider('=', 40);     // 自定义分隔线
```

## Log.Locale

`Log.Locale` 提供本地化 API，详见 [LocaleApi](locale.md)。

```js
var text = Log.Locale.Get('greeting');
```
