***English*** | [简体中文](../../zh-CN/script-api/moodle.md)

# Moodle — Custom Status Management

Moodle is used to apply, remove, and query custom Moodle statuses on the player. All Moodle definitions come from JSON
files in your mod's `Moodle/` directory.

## Methods Overview

| Method                           | Description                 |
|----------------------------------|-----------------------------|
| `ApplyMoodle(key, holdSeconds?)` | Apply a defined Moodle      |
| `RemoveMoodle(key)`              | Remove a specific Moodle    |
| `HasMoodle(key)`                 | Check if a Moodle is active |
| `GetActiveMoodles()`             | Get all active Moodle keys  |
| `GetMoodleCount()`               | Get active Moodle count     |
| `GetIntensity(key)`              | Get intensity               |
| `GetName(key)`                   | Get name                    |
| `GetDescription(key)`            | Get description             |
| `GetHoldSeconds(key)`            | Get default duration        |
| `IsCritical(key)`                | Whether it's critical       |
| `IsChippedOnly(key)`             | Whether it's chipped-only   |
| `IsImportant(key)`               | Whether it's important      |

## Applying Moodles

```js
// Use default duration (from JSON hold_seconds)
Moodle.ApplyMoodle('bleeding');

// Override duration: auto-expires after 5 seconds
Moodle.ApplyMoodle('bleeding', 5);

// If the key is not found in loaded definitions, logs a warning, no exception thrown
Moodle.ApplyMoodle('non_existent');  // safe, nothing happens
```

`holdSeconds` is optional. Pass 0 or a negative value to use the JSON-defined default duration.

## Removing Moodles

```js
// Mark as expired, triggers lose event and removes on next poll cycle
var removed = Moodle.RemoveMoodle('bleeding');
if (removed) {
    Log.Info('Bleeding status removed');
}
```

Returns `true` if the Moodle existed and was marked for removal, `false` if the key doesn't exist or is already gone.

## Querying Moodles

```js
// Check if active
if (Moodle.HasMoodle('poison')) {
    Log.Warning('Player is poisoned');
}

// Get all active Moodles
var keys = Moodle.GetActiveMoodles();
for (var i = 0; i < keys.length; i++) {
    Log.Info('Active: ' + keys[i]);
}

// Count
var count = Moodle.GetMoodleCount();
Log.Info('Currently ' + count + ' status effects');
```

## Reading Moodle Properties

Read properties from loaded JSON definitions (not runtime state):

```js
var name = Moodle.GetName('bleeding');           // Display name
var desc = Moodle.GetDescription('bleeding');    // Description
var intensity = Moodle.GetIntensity('bleeding'); // Intensity level
var duration = Moodle.GetHoldSeconds('bleeding');// Default duration
var isCrit = Moodle.IsCritical('bleeding');      // Whether critical
var isImportant = Moodle.IsImportant('bleeding');// Whether important
```

If the key doesn't match any loaded definition, property methods return safe defaults (`0`, `false`, empty string).

## Full Example

A script that automatically detects and notifies about critical statuses:

```js
function onMoodleGet(event) {
    // event.MoodleKey, event.MoodleName, event.Intensity, event.Critical, event.HoldSeconds
    if (event.Critical) {
        Player.Alert('Critical status gained: ' + event.MoodleName, true);
    }
}

function onWorldGenerated() {
    // Check all critical statuses every 2 seconds
    setInterval(function() {
        var keys = Moodle.GetActiveMoodles();
        for (var i = 0; i < keys.length; i++) {
            if (Moodle.IsCritical(keys[i])) {
                Log.Warning('Critical status active: ' + Moodle.GetName(keys[i]));
            }
        }
    }, 2000);
}
```

## Notes

- `ApplyMoodle` requires the Moodle definition to be loaded from JSON, otherwise it silently does nothing (warning log)
- Re-applying a Moodle with the same key refreshes its expiration timer (restarts countdown)
- `RemoveMoodle` doesn't remove instantly — it marks for removal on the next poll cycle (up to 0.5s delay)
- Property query methods read static values from the JSON definition, not runtime state
- The global variable is `Moodle`, callable directly in both JS and Lua
