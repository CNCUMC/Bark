***English*** | [简体中文](../../zh-CN/script-api/player.md)

# Player — Player Actions

Player provides teleportation, item pickup, and alert popups. Few methods — straight to examples.

## Teleport

```js
// Teleport to coordinates
Player.Teleport(100, 200);

// Get current position
var pos = Player.GetPosition();
Log.Info('Current position: ' + pos.x + ', ' + pos.y);
```

## Picking Up Items

```js
// Put item into slot 0
Player.PickUpItem('backpack', 0);

// Force into an occupied slot (third param: force)
Player.PickUpItem('rifle', 1, true);

// Auto pick up an item
Player.AutoPickUpItem('rifle');

// Auto pick up 2 items
Player.AutoPickUpItem('rifle', 2);
```

> ℹ️ The backpack has 8 slots (`Player.MaxInventorySlots`), indexed 0 to 7.

`PickUpItem`'s `force` parameter defaults to `false` and can be omitted.
See [naming conventions](../script-mod.md#naming-conventions) for optional parameter details.

## Alert Popups

```js
// Normal alert
Player.Alert('Found a key', false);

// Important alert (red)
Player.Alert('You are poisoned!', true);

// Delayed alert (third param: delay in seconds, optional)
Player.Alert('Shown in 3 seconds', false, 3);
```

`delay` defaults to `0` (immediate) and can be omitted.

## Talking

Make the player speak dialogue bubbles (or via an electronic device like the watch).

```js
// Player says a line
Player.Talk('I should find some food.');

// Speak through an electronic device (generic proxy)
Player.TalkElectronic('Searching for signal...');

// Speak through a specific electronic item
Player.TalkElectronic('Analyzing sample...', 'watch');
```

| Method                              | Description                                   |
|-------------------------------------|-----------------------------------------------|
| `Talk(dialogue)`                    | Player speaks a dialogue bubble               |
| `TalkElectronic(dialogue)`          | Speak via the generic electronic talker proxy |
| `TalkElectronic(dialogue, itemId)`  | Speak via a specific electronic item          |

> The `itemId` parameter of `TalkElectronic` is optional and resolves through the same mod-id prefixing rules as item
> IDs elsewhere.

## Full Example

Teleport home + full loadout:

```js
function onLoad() {
    Log.Info('Player demo mod loaded');
}

function onWorldGenerated() {
    // Teleport back to origin
    Player.Teleport(0, 0);

    // Gear up
    Player.PickUpItem('backpack', 0);
    Player.PickUpItem('rifle', 1, true);
    Player.PickUpItem('ammo_rifle', 2);
    Player.PickUpItem('medkit', 3);

    Player.Alert('Gear ready', false);
}
```
