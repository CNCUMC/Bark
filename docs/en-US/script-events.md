***English*** | [简体中文](../zh-CN/script-events.md)

# Script Event Hooks

Bark provides 11 built-in event hooks. When the corresponding event occurs in-game, Bark automatically calls the
matching global function in your script.

## How to Use

Define a global function with the exact hook name. No parameters needed — Bark handles the call timing.

```js
function onPlayerJumpStart() {
    Log.Info('Player jumped');
}
```

That's it. No registration, no imports required.

## Hook Reference

### Player Events

| Hook Function       | Trigger           |
|---------------------|-------------------|
| `onPlayerJumpStart` | Jump key pressed  |
| `onPlayerJumpOver`  | Landed after jump |
| `onPlayerDeath`     | Player died       |

```js
function onPlayerDeath() {
    Log.Warning('Player died');
    // auto-save on death, etc.
}
```

### Limb Events

Six hooks cover all limb status changes: fracture, dislocation, infection, dismemberment.

| Hook Function        | Trigger          |
|----------------------|------------------|
| `onLimbBroken`       | Bone fractured   |
| `onLimbMended`       | Bone healed      |
| `onLimbDislocated`   | Joint dislocated |
| `onLimbUnDislocated` | Joint relocated  |
| `onLimbDismembered`  | Limb severed     |
| `onLimbInfected`     | Wound infected   |

> ℹ️ Limb hooks carry no parameter. To find which limb was affected, iterate with `LimbUtil` inside the hook. e.g.,
> `LimbUtil.IsBroken(0)` checks if limb #0 is broken.

```js
function onLimbBroken() {
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

### World / Menu Events

| Hook Function      | Trigger                                         |
|--------------------|-------------------------------------------------|
| `onMainMenuLoaded` | Entered main menu                               |
| `onWorldGenerated` | World finished generating, safe to access world |

```js
function onWorldGenerated() {
    Log.Info('World ready, mod initialized');
    // Do world-dependent operations here
}

function onMainMenuLoaded() {
    Log.Info('Returned to main menu');
}
```

> ⚠️ `onWorldGenerated` is the first moment you can safely call `WorldUtil`. Before this (including `onLoad`), the world
> doesn't exist and calling WorldUtil will error.

## Full Example

A mod that tracks all injury events:

```js
// Initialize counters in onLoad
var injuredCount = 0;
var brokenCount = 0;

function onLoad() {
    Log.Info('Injury tracker mod loaded');
}

function onLimbBroken() {
    brokenCount++;
    injuredCount++;
    Log.Warning('Fracture! Total fractures: ' + brokenCount + ', injuries: ' + injuredCount);
    PlayerUtil.Alert('Another bone broken...', true);
}

function onLimbInfected() {
    injuredCount++;
    Log.Warning('Infection! Total injuries: ' + injuredCount);
}

function onPlayerDeath() {
    Log.Warning('Player died. Session stats: fractures ' + brokenCount + ', injuries ' + injuredCount);
    injuredCount = 0;
    brokenCount = 0;
}
```

## Notes

- Hook names are case-sensitive and must match exactly
- Hooks take no parameters — Bark passes nothing
- Keep hook code fast — don't block. Use `setInterval` / `setTimeout` for heavy work
- Hook frequency varies — `onLimbBroken` may fire multiple times (multiple limbs breaking at once), so make your
  handlers idempotent
- Multiple mods can define the same hook — they don't interfere
