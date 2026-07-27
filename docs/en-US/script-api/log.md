***English*** | [简体中文](../../zh-CN/script-api/log.md)

# LogApi — Logging

LogApi is the sole logging mechanism for scripts, injected as the global variable `Log`. Output goes to both the BepInEx
console and the mod log file.

## Log Levels

```js
Log.Debug('Debug info');        // Debug level
Log.Info('Normal info');        // Info level
Log.Warning('Warning');         // Warning level
Log.Error('Error!');            // Error level
Log.Message('Plain message');   // Message level
```

All levels use the same syntax: one string parameter.

## Localized Logging

`*F` suffix methods localize before output:

```js
Log.InfoF('welcome', 'Player');        // = Log.Info(Log.Locale.GetFormatted('welcome', 'Player'))
Log.ErrorF('error_code', 500);         // = Log.Error(Log.Locale.GetFormatted('error_code', 500))
Log.WarningF('low_hp', 15);            // = Log.Warning(Log.Locale.GetFormatted('low_hp', 15))
Log.DebugF('loaded', 'MyMod');         // = Log.Debug(Log.Locale.GetFormatted('loaded', 'MyMod'))
```

## Formatting Helpers

```js
Log.NewLine();             // blank line
Log.Divider();             // default divider "---------------------------"
Log.Divider('=', 40);     // custom divider
```

## Log.Locale

`Log.Locale` provides the localization API. See [LocaleApi](locale.md) for details.

```js
var text = Log.Locale.Get('greeting');
```
