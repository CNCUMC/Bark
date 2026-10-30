***English*** | [简体中文](../../zh-CN/script-api/world.md)

# World — World Editing

World provides block placement, area fill, item spawning, and tile queries. Only a few methods, but enough to reshape
the world.

> ⚠️ All methods must be called after `onWorldGenerated`. The world doesn't exist during the main menu phase.

## Block Operations

```js
// Place a single block
World.PlaceTile(50, 30, 18);   // 18 = Marble block

// Area fill
World.FillTiles(0, 0, 10, 5, 3);  // fill (0,0) to (10,5) with block #3
```

| Method                                           | Description                                           |
|--------------------------------------------------|-------------------------------------------------------|
| `PlaceTile(x, y, blockId)`                       | Place one block at coordinates                        |
| `FillTiles(startX, startY, endX, endY, blockId)` | Batch-fill a rectangle, coords auto-clamped to bounds |

## Tile Queries

`TileAt` takes **tile coordinates** (integers) and returns the tile ID string — `"air"` for empty space, or the
vanilla/custom tile ID (e.g. `"marble"`, `"my_custom_wall"`). Coordinates outside the world count as `"air"`.

Use `ToTilePosition` to convert a world position into tile coordinates, and `ToRelativePosition` to get the offset
ratio inside the tile (each axis 0-1).

```js
// What tile is at this grid position?
var tile = World.TileAt(10, 5);
if (tile === 'air') {
    Log.Info('empty space');
}

// World position → tile coordinates
var pos = Player.GetPosition();
var tilePos = World.ToTilePosition(pos.x, pos.y);
Log.Info('standing in tile ' + tilePos.x + ', ' + tilePos.y);

// Where inside the tile is this position? (0-1)
var rel = World.ToRelativePosition(pos.x, pos.y);
Log.Info('offset inside tile: ' + rel.x + ', ' + rel.y);
```

| Method                        | Description                                                          |
|-------------------------------|----------------------------------------------------------------------|
| `TileAt(tileX, tileY)`        | Tile ID at grid position, `"air"` if empty or out of bounds          |
| `ToTilePosition(x, y)`        | World position → tile coordinates (`Vector2Int`)                     |
| `ToRelativePosition(x, y)`    | World position → offset ratio inside its tile (`Vector2`, 0-1)       |

### Wall check example

Stop a dash from phasing through walls — probe the destination tile before moving:

```js
function tryDash(dirX, dirY, distance) {
    var pos = Player.GetPosition();
    var from = World.ToTilePosition(pos.x, pos.y);
    var to = World.ToTilePosition(pos.x + dirX * distance, pos.y + dirY * distance);

    // Walk the line and stop at the first non-air tile
    var steps = Math.max(Math.abs(to.x - from.x), Math.abs(to.y - from.y));
    for (var i = 1; i <= steps; i++) {
        var cx = from.x + (to.x - from.x) * i / steps;
        var cy = from.y + (to.y - from.y) * i / steps;
        var tile = World.TileAt(Math.round(cx), Math.round(cy));
        if (tile !== 'air') {
            Log.Info('blocked by ' + tile);
            return false;
        }
    }

    Player.Teleport(to.x + 0.5, to.y + 0.5);
    return true;
}
```

## Item Spawning

```js
// Spawn items on the ground
World.PlaceItem(100, 50, 'medkit');
World.PlaceItem(102, 50, 'ammo_rifle');
```

## World Size

```js
var w = World.GetWidth();   // world width (blocks)
var h = World.GetHeight();  // world height (blocks)
```

## Full Example

Build a wall around the player:

```js
function onWorldGenerated() {
    var pos = Player.GetPosition();
    var cx = Math.floor(pos.x);
    var cy = Math.floor(pos.y);
    var r = 5;

    // Draw four sides
    World.FillTiles(cx - r, cy - r, cx + r, cy - r, 18);  // bottom
    World.FillTiles(cx - r, cy + r, cx + r, cy + r, 18);  // top
    World.FillTiles(cx - r, cy - r, cx - r, cy + r, 18);  // left
    World.FillTiles(cx + r, cy - r, cx + r, cy + r, 18);  // right

    // Drop a medkit at the entrance
    World.PlaceItem(cx, cy - r + 1, 'medkit');

    Log.Info('Wall built');
}
```
