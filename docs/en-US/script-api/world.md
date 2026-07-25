***English*** | [简体中文](../../zh-CN/script-api/world.md)

# WorldUtil — World Editing

WorldUtil provides block placement, area fill, and item spawning. Only 5 methods, but enough to reshape the world.

> ⚠️ All methods must be called after `onWorldGenerated`. The world doesn't exist during the main menu phase.

## Block Operations

```js
// Place a single block
WorldUtil.PlaceBlock(50, 30, 18);   // 18 = Marble block

// Area fill
WorldUtil.FillBlocks(0, 0, 10, 5, 3);  // fill (0,0) to (10,5) with block #3
```

| Method                                            | Description                                           |
|---------------------------------------------------|-------------------------------------------------------|
| `PlaceBlock(x, y, blockId)`                       | Place one block at coordinates                        |
| `FillBlocks(startX, startY, endX, endY, blockId)` | Batch-fill a rectangle, coords auto-clamped to bounds |

## Item Spawning

```js
// Spawn items on the ground
WorldUtil.PlaceItem(100, 50, 'medkit');
WorldUtil.PlaceItem(102, 50, 'ammo_rifle');
```

## World Size

```js
var w = WorldUtil.GetWidth();   // world width (blocks)
var h = WorldUtil.GetHeight();  // world height (blocks)
```

## Full Example

Build a wall around the player:

```js
function onWorldGenerated() {
    var pos = PlayerUtil.GetPosition();
    var cx = Math.floor(pos.x);
    var cy = Math.floor(pos.y);
    var r = 5;

    // Draw four sides
    WorldUtil.FillBlocks(cx - r, cy - r, cx + r, cy - r, 18);  // bottom
    WorldUtil.FillBlocks(cx - r, cy + r, cx + r, cy + r, 18);  // top
    WorldUtil.FillBlocks(cx - r, cy - r, cx - r, cy + r, 18);  // left
    WorldUtil.FillBlocks(cx + r, cy - r, cx + r, cy + r, 18);  // right

    // Drop a medkit at the entrance
    WorldUtil.PlaceItem(cx, cy - r + 1, 'medkit');

    Log.Info('Wall built');
}
```
