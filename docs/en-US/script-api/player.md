***English*** | [简体中文](../../zh-CN/script-api/player.md)

# PlayerUtil — Player Actions

PlayerUtil provides teleportation, item pickup, and alert popups. Few methods — straight to examples.

## Teleport

```js
// Teleport to coordinates
PlayerUtil.Teleport(100, 200);

// Get current position
var pos = PlayerUtil.GetPosition();
Log.Info('Current position: ' + pos.x + ', ' + pos.y);
```

## Picking Up Items

```js
// Put item into slot 0
PlayerUtil.PickUpItem('backpack', 0);

// Force into an occupied slot (third param: force)
PlayerUtil.PickUpItem('rifle', 1, true);
```

> ℹ️ The backpack has 8 slots (`PlayerUtil.MaxInventorySlots`), indexed 0 to 7.

`PickUpItem`'s `force` parameter defaults to `false` and can be omitted.
See [naming conventions](../script-mod.md#naming-conventions) for optional parameter details.

## Alert Popups

```js
// Normal alert
PlayerUtil.Alert('Found a key', false);

// Important alert (red)
PlayerUtil.Alert('You are poisoned!', true);

// Delayed alert (third param: delay in seconds, optional)
PlayerUtil.Alert('Shown in 3 seconds', false, 3);
```

`delay` defaults to `0` (immediate) and can be omitted.

## Full Example

Teleport home + full loadout:

```js
function onLoad() {
    Log.Info('PlayerUtil demo mod loaded');
}

function onWorldGenerated() {
    // Teleport back to origin
    PlayerUtil.Teleport(0, 0);

    // Gear up
    PlayerUtil.PickUpItem('backpack', 0);
    PlayerUtil.PickUpItem('rifle', 1, true);
    PlayerUtil.PickUpItem('ammo_rifle', 2);
    PlayerUtil.PickUpItem('medkit', 3);

    PlayerUtil.Alert('Gear ready', false);
}
```
