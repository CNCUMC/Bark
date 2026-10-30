***English*** | [简体中文](../../zh-CN/script-api/world.md)

# World — World Editing

World provides block placement, area fill, item spawning, tile queries, and explosions. Only a few methods, but enough
to reshape the world.

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

## Explosions

`Explode` takes **world coordinates** (same as `PlaceItem`, not tile coordinates) and detonates immediately.
Defaults match the in-game dynamite: `range` 20, `velocity` 20, `structuralDamage` 1250, with zero body
damage — so by default it only destroys terrain and throws bodies around with the shockwave, exactly like
vanilla dynamite.

```js
// Default explosion (dynamite-sized)
World.Explode(pos.x, pos.y);

// Bigger blast, slower shockwave
World.Explode(pos.x, pos.y, 40, 10);

// Lethal to bodies: skin/muscle damage + shrapnel
World.Explode(pos.x, pos.y, 12, 25, 800, 60, 60, 0.8);

// Only cracks blocks, no terrain destruction
World.Explode(pos.x, pos.y, 15, 5, 100);
```

| Parameter                          | Default | Description                                     |
|------------------------------------|---------|-------------------------------------------------|
| `range`                            | `20`    | Blast radius in world units                     |
| `velocity`                         | `20`    | Shockwave push speed on bodies                  |
| `structuralDamage`                 | `1250`  | Damage dealt to tiles (`0` leaves terrain intact) |
| `skinDamage`                       | `0`     | Skin damage dealt to bodies in range            |
| `muscleDamage`                     | `0`     | Muscle damage dealt to bodies in range          |
| `shrapnelChance`                   | `0`     | Chance (0-1) of spawning shrapnel               |
| `sound`                            | `null`  | Sound name, omit for the game default           |

> Must be called on the main thread (i.e. from a script callback), and only after `onWorldGenerated`.

### Remote-detonated mine example

```js
function onTileExist(e) {
    if (e.tileId !== 'my_mine') return;
    var pos = Player.GetPosition();
    var dx = pos.x - (e.posX + 0.5);
    var dy = pos.y - (e.posY + 0.5);
    if (dx * dx + dy * dy < 4) {          // player stepped within 2 units
        World.Explode(e.posX + 0.5, e.posY + 0.5, 10, 25, 600, 50, 50, 0.5);
        World.PlaceTile(e.posX, e.posY, 'air');
    }
}
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
