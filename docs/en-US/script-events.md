***English*** | [简体中文](../zh-CN/script-events.md)

# Script Event Hooks

Bark provides built-in event hooks. When the corresponding event occurs in-game, Bark automatically calls the
matching global function in your script, passing an `event` object with relevant data.

## How to Use

Define a global function with the exact hook name. Bark calls it with an `event` object.

```js
function onPlayerJumpStart(event) {
    Log.Info('Player jumped');
}
```

If you don't need the event data, you can omit the parameter — JavaScript and Lua ignore extra arguments:

```js
function onPlayerJumpStart() {
    Log.Info('Player jumped');  // fine too
}
```

That's it. No registration, no imports required.

## Hook Reference

### Player Events

| Hook Function       | Trigger           | event Fields |
|---------------------|-------------------|--------------|
| `onPlayerJumpStart` | Jump key pressed  | —            |
| `onPlayerJumpOver`  | Landed after jump | —            |
| `onPlayerDeath`     | Player died       | —            |

```js
function onPlayerDeath(event) {
    Log.Warning('Player died');
    // auto-save on death, etc.
}
```

### Limb Events

Six hooks cover all limb status changes: fracture, dislocation, infection, dismemberment.

| Hook Function        | Trigger          | event Fields |
|----------------------|------------------|--------------|
| `onLimbBroken`       | Bone fractured   | —            |
| `onLimbMended`       | Bone healed      | —            |
| `onLimbDislocated`   | Joint dislocated | —            |
| `onLimbUnDislocated` | Joint relocated  | —            |
| `onLimbDismembered`  | Limb severed     | —            |
| `onLimbInfected`     | Wound infected   | —            |

> ℹ️ Limb hooks carry no dedicated event fields. To find which limb was affected, iterate with `LimbUtil` inside the
> hook. e.g., `LimbUtil.IsBroken(0)` checks if limb #0 is broken.

```js
function onLimbBroken(event) {
    // Iterate all limbs to find the broken one
    var count = LimbUtil.GetLimbCount();
    var brokenList = [];
    for (var i = 0; i < count; i++) {
        if (LimbUtil.IsBroken(i)) {
            brokenList.push(i);
        }
    }
    Log.Info('Broken limb indices: ' + brokenList.join(', '));
}
```

### Item Events

Global hooks for item use, hand-use, equip, unequip, limb use, and attack.

All item events pass an `event` object with these fields:

| Field          | Type     | Description                        |
|----------------|----------|------------------------------------|
| `event.ItemId` | `string` | The item ID (e.g. `"arrow"`)       |
| `event.Item`   | `Item`   | The C# Item instance               |

`onItemLimbUse` additionally provides:

| Field              | Type     | Description                   |
|--------------------|----------|-------------------------------|
| `event.LimbIndex`  | `int`    | Target limb index, -1 unknown |
| `event.LimbName`   | `string` | Target limb name              |

| Hook Function    | Trigger                              |
|------------------|--------------------------------------|
| `onItemUse`      | Player used an item from inventory   |
| `onItemHandUse`  | Player used an item held in hand     |
| `onItemEquip`    | Item was equipped                    |
| `onItemUnequip`  | Item was unequipped                  |
| `onItemLimbUse`  | Item was used on a limb              |
| `onItemAttack`   | Melee attack with an item            |

```js
function onItemUse(event) {
    Log.Info('Item used: ' + event.ItemId);
    // event.Item gives you the C# Item instance for advanced access
}

function onItemLimbUse(event) {
    Log.Info(event.ItemId + ' used on limb ' + event.LimbName);
}

function onItemAttack(event) {
    Log.Info('Attacked with ' + event.ItemId);
}
```

### Moodle Events

Lifecycle events for the custom Moodle system.

All Moodle events carry an `event` object with these fields (varies by event type):

| Field                | Type       | Description                                   | Applicable Events            |
|----------------------|------------|-----------------------------------------------|------------------------------|
| `event.MoodleKey`    | `string`   | Unique Moodle identifier                      | `onMoodleGet`, `onMoodleLose` |
| `event.MoodleName`   | `string`   | Moodle display name                           | `onMoodleGet`, `onMoodleLose` |
| `event.Intensity`    | `int`      | Moodle intensity                              | `onMoodleGet`                |
| `event.Critical`     | `bool`     | Whether it's critical                         | `onMoodleGet`                |
| `event.HoldSeconds`  | `float`    | Duration in seconds                           | `onMoodleGet`                |
| `event.ActiveKeys`   | `string[]` | List of all currently active Moodle keys      | `onMoodleIterate`            |

| Hook Function     | Trigger                                |
|-------------------|----------------------------------------|
| `onMoodleGet`     | Moodle is applied to the player        |
| `onMoodleIterate` | Polled (every 0.5 seconds)             |
| `onMoodleLose`    | Moodle expires or is removed           |

```js
function onMoodleGet(event) {
    Log.Info('Moodle gained: ' + event.MoodleKey);

    if (event.Critical) {
        PlayerUtil.Alert('Critical status: ' + event.MoodleName, true);
    }
}

function onMoodleIterate(event) {
    // Fires every 0.5s with all active statuses
    Log.Debug('Active statuses: ' + event.ActiveKeys.join(', '));
}

function onMoodleLose(event) {
    Log.Info('Moodle lost: ' + event.MoodleKey);
}
```

### World / Menu Events

| Hook Function      | Trigger                                         | event Fields |
|--------------------|-------------------------------------------------|--------------|
| `onMainMenuLoaded` | Entered main menu                               | —            |
| `onWorldGenerated` | World finished generating, safe to access world | —            |

```js
function onWorldGenerated(event) {
    Log.Info('World ready, mod initialized');
    // Do world-dependent operations here
}

function onMainMenuLoaded(event) {
    Log.Info('Returned to main menu');
}
```

> ⚠️ `onWorldGenerated` is the first moment you can safely call `WorldUtil`. Before this (including `onLoad`), the world
> doesn't exist and calling WorldUtil will error.

### Command Event

Fires when the player enters a custom command registered by a script mod. Commands are defined via `Command/*.json`. See [Script Commands](script-mod/command.md) for details.

| Field                | Type       | Description                                                |
|----------------------|------------|------------------------------------------------------------|
| `event.CommandName`  | `string`   | Triggered command name (without arguments)                 |
| `event.Args`         | `string[]` | All input tokens (`args[0]` = command name, `args[1..]` = user arguments) |

| Hook Function | Trigger                               |
|---------------|---------------------------------------|
| `onCommand`   | Player entered a registered script command |

```js
function onCommand(event) {
    Log.Info('Command: ' + event.CommandName);
    Log.Info('Args: ' + event.Args.join(', '));
}
```

## Item Scripts

In addition to global hooks, you can attach scripts to specific items via JSON. When that item triggers an action
(use, attack, equip, etc.), Bark executes the script and calls its `main()` function with arguments.

See [Custom Items](script-mod/item.md) for setup. The script side looks like this:

```js
// arrow.js — registered in arrow.json under "attack"
function main(itemId, item, action) {
    // itemId: "arrow"
    // item:    C# Item instance
    // action:  "attack" / "use" / "equip" / "unequip" / "use_in_hand" / "use_on_limb"
    itemUtil.Destroy(itemId);
    PlayerUtil.Alert('Bullseye!', true);
}
```

The `main` function accepts 0 to 3 parameters — JavaScript and Lua auto-ignore extras. Common patterns:

```js
function main(itemId)           { /* only need the ID */ }
function main(itemId, item)     { /* need the item object too */ }
function main(itemId, item, action) { /* full context */ }
```

Backward compatibility: old-style top-level `__barkItemId` global still works, but `main()` is the recommended way.

## Full Example

A mod that tracks all injury events:

```js
// Initialize counters in onLoad
var injuredCount = 0;
var brokenCount = 0;

function onLoad() {
    Log.Info('Injury tracker mod loaded');
}

function onLimbBroken(event) {
    brokenCount++;
    injuredCount++;
    Log.Warning('Fracture! Total fractures: ' + brokenCount + ', injuries: ' + injuredCount);
    PlayerUtil.Alert('Another bone broken...', true);
}

function onLimbInfected(event) {
    injuredCount++;
    Log.Warning('Infection! Total injuries: ' + injuredCount);
}

function onPlayerDeath(event) {
    Log.Warning('Player died. Session stats: fractures ' + brokenCount + ', injuries ' + injuredCount);
    injuredCount = 0;
    brokenCount = 0;
}
```

## Notes

- Hook names are case-sensitive and must match exactly
- Hooks receive an `event` object — item events provide `event.ItemId` and `event.Item`
- Keep hook code fast — don't block. Use `setInterval` / `setTimeout` for heavy work
- Hook frequency varies — `onLimbBroken` may fire multiple times (multiple limbs breaking at once), so make your
  handlers idempotent
- Multiple mods can define the same hook — they don't interfere
