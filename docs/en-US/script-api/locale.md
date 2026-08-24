***English*** | [简体中文](../../zh-CN/script-api/locale.md)

# LocaleApi — Localization

LocaleApi enables multi-language support in scripts. Access via `Log.Locale`.

## Basic Usage

```js
// Get localized text (returns "[key]" if key doesn't exist)
var text = Log.Locale.Get('welcome_message');

// With placeholders
var text2 = Log.Locale.GetFormatted('damage_report', 25, 'Left Leg');
// Matches text: "Took {0} damage to {1}"

// Text from another mod
var text3 = Log.Locale.GetFrom('other_mod', 'greeting');

// Check if key exists
if (Log.Locale.HasKey('error_msg')) {
    Log.Info(Log.Locale.Get('error_msg'));
}
```

| Method                                  | Description                              |
|-----------------------------------------|------------------------------------------|
| `Get(key)`                              | Get text, returns `[key]` if missing     |
| `GetFormatted(key, ...args)`            | Get formatted text, `{0}` `{1}` replaced |
| `GetFrom(modId, key)`                   | Read another mod's text                  |
| `GetFormattedFrom(modId, key, ...args)` | Read another mod's formatted text        |
| `HasKey(key)`                           | Key exists?                              |

## Key Auto-Expansion

Your key gets the mod id automatically inserted. If your mod id is `my_mod`:

```js
Log.Locale.Get('hello')
// → actually looks up log.my_mod.hello
// If already under option.log.item etc., the prefix is preserved
```

In short: use short keys. `Locale.Get('greeting')` is equivalent to searching `log.my_mod.greeting`.

## Combined with Logging

LogApi includes `*F` methods for one-step localization + output:

```js
Log.InfoF('welcome', 'PlayerName');    // GetFormatted + Info
Log.ErrorF('critical_error', 42);      // GetFormatted + Error
```

## Configuring Locale Files

Create `Lang/EN.json` in your mod directory:

```json
{
  "log": {
    "welcome": "Welcome to this mod!",
    "damage_report": "Took {0} damage to {1}"
  }
}
```

Bark loads the matching language text automatically.

Loaded locale entries are also forwarded through `BetterLocale.SetDefault` and, on `BetterLocale.Flush()`, exported to
`BepInEx/config/CUCoreLib/Locales/{modId}/` so each script mod's localization is kept in its own namespace subdirectory
for sharing.
