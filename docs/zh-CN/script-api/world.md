[English](../../en-US/script-api/world.md) | ***简体中文***

# World — 世界编辑

World 提供方块放置、区域填充、物品生成和瓦片查询。方法不多，但够你改造世界了。

> ⚠️ 所有方法必须在 `onWorldGenerated` 之后调用，主菜单阶段世界不存在。

## 方块操作

```js
// 放置单个方块
World.PlaceTile(50, 30, 18);   // 18 号是 Marble 方块

// 区域填充
World.FillTiles(0, 0, 10, 5, 3);  // 从 (0,0) 到 (10,5) 填满 3 号方块
```

| 方法                                             | 说明                                          |
|--------------------------------------------------|-----------------------------------------------|
| `PlaceTile(x, y, blockId)`                       | 在指定坐标放一个方块                          |
| `FillTiles(startX, startY, endX, endY, blockId)` | 矩形区域批量填充，坐标会自动 clamp 到世界边界 |

## 瓦片查询

`TileAt` 接收**瓦片坐标**（整数格），返回物块 ID 字符串——空地返回 `"air"`，否则返回原版或自定义物块 ID
（如 `"marble"`、`"my_custom_wall"`）。越界坐标同样视为 `"air"`。

`ToTilePosition` 把世界坐标转成瓦片坐标，`ToRelativePosition` 取坐标点在其所属瓦片内的偏移比例（每轴 0-1）。

```js
// 这个格子上是什么物块？
var tile = World.TileAt(10, 5);
if (tile === 'air') {
    Log.Info('空地');
}

// 世界坐标 → 瓦片坐标
var pos = Player.GetPosition();
var tilePos = World.ToTilePosition(pos.x, pos.y);
Log.Info('站在瓦片 ' + tilePos.x + ', ' + tilePos.y);

// 这个点落在瓦片内的什么位置？（0-1）
var rel = World.ToRelativePosition(pos.x, pos.y);
Log.Info('瓦片内偏移：' + rel.x + ', ' + rel.y);
```

| 方法                        | 说明                                          |
|-----------------------------|-----------------------------------------------|
| `TileAt(tileX, tileY)`      | 格子上的物块 ID，空地或越界返回 `"air"`        |
| `ToTilePosition(x, y)`      | 世界坐标 → 瓦片坐标（`Vector2Int`）            |
| `ToRelativePosition(x, y)`  | 世界坐标 → 瓦片内偏移比例（`Vector2`，0-1）    |

### 撞墙检测示例

冲刺前先探测目标格子，遇到墙就停下，不穿模：

```js
function tryDash(dirX, dirY, distance) {
    var pos = Player.GetPosition();
    var from = World.ToTilePosition(pos.x, pos.y);
    var to = World.ToTilePosition(pos.x + dirX * distance, pos.y + dirY * distance);

    // 沿直线逐格探测，遇到第一个非空物块就停
    var steps = Math.max(Math.abs(to.x - from.x), Math.abs(to.y - from.y));
    for (var i = 1; i <= steps; i++) {
        var cx = from.x + (to.x - from.x) * i / steps;
        var cy = from.y + (to.y - from.y) * i / steps;
        var tile = World.TileAt(Math.round(cx), Math.round(cy));
        if (tile !== 'air') {
            Log.Info('被 ' + tile + ' 挡住');
            return false;
        }
    }

    Player.Teleport(to.x + 0.5, to.y + 0.5);
    return true;
}
```

## 物品生成

```js
// 生成物品到地上
World.PlaceItem(100, 50, 'medkit');
World.PlaceItem(102, 50, 'ammo_rifle');
```

## 世界尺寸

```js
var w = World.GetWidth();   // 世界宽度（块）
var h = World.GetHeight();  // 世界高度（块）
```

## 完整示例

画一圈墙把自己围起来：

```js
function onWorldGenerated() {
    var pos = Player.GetPosition();
    var cx = Math.floor(pos.x);
    var cy = Math.floor(pos.y);
    var r = 5;

    // 画四条边
    World.FillTiles(cx - r, cy - r, cx + r, cy - r, 18);  // 下边
    World.FillTiles(cx - r, cy + r, cx + r, cy + r, 18);  // 上边
    World.FillTiles(cx - r, cy - r, cx - r, cy + r, 18);  // 左边
    World.FillTiles(cx + r, cy - r, cx + r, cy + r, 18);  // 右边

    // 门口放个医疗包
    World.PlaceItem(cx, cy - r + 1, 'medkit');

    Log.Info('城墙已建好');
}
```
