***English*** | [简体中文](../../zh-CN/script-api/skills.md)

# Skills System

Skill operates on three skills: Strength, Resilience, Intelligence.

## Skill Identifiers

Specify skills by string, case-insensitive. Supports short and full names:

| Value            | Meaning                         |
|------------------|---------------------------------|
| `"str"`          | Strength                        |
| `"strength"`     | Strength                        |
| `"res"`          | Resilience                      |
| `"resilience"`   | Resilience                      |
| `"int"`          | Intelligence                    |
| `"intelligence"` | Intelligence                    |
| Any other value  | Intelligence (default fallback) |

## Methods

```js
// Read level (integer)
var strLevel = Skill.GetLevel("str");

// Read current XP
var exp = Skill.GetExperience("res");

// Read progress toward next level 0~1
var progress = Skill.GetProgress("int");

// Add XP
Skill.AddExperience("str", 500);

// Set level (resets XP to that level's starting value)
Skill.SetLevel("res", 10);

// XP required for next level
var needed = Skill.GetExperienceForNextLevel("int");
```

> ⚠️ `SetLevel` resets XP to zero for the set level. Going from level 5 to 10 loses all progress between those levels.

## XP Multiplier

Get the mod id via `ScriptInfo` and use the options system to control global XP multiplier. `XpMultiplier` is a C#
property; scripts use OptionsApi equivalently.

```js
// Read the mod's own multiplier config
var multiplier = OptionsApi.GetFloat("xp_multiplier");
Skill.AddExperience("int", 100 * multiplier);
```

## Full Example

A double XP mod:

```js
function onLoad() {
    Log.Info("Double XP enabled");
}

function onWorldGenerated() {
    // Just a demo of read/write — real logic should be event-driven
    setInterval(function () {
        for (var i = 0; i < 3; i++) {
            var skill = ["str", "res", "int"][i];
            var currentExp = Skill.GetExperience(skill);
        }
    }, 1000);
}
```
