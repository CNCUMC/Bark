[English](../../en-US/script-api/world.md) | ***简体中文***

# World — 世界编辑

World 提供方块放置、区域填充和物品生成。只有 5 个方法，但够你改造世界了。

> ⚠️ 所有方法必须在 `onWorldGenerated` 之后调用，主菜单阶段世界不存在。

## 方块操作

```js
// 放置单个方块
World.PlaceTile(50, 30, 18);   // 18 号是 Marble 方块

// 区域填充
World.FillTiles(0, 0, 10, 5, 3);  // 从 (0,0) 到 (10,5) 填满 3 号方块
```

| 方法                                              | 说明                                          |
|---------------------------------------------------|-----------------------------------------------|
| `PlaceTile(x, y, blockId)`                       | 在指定坐标放一个方块                          |
| `FillTiles(startX, startY, endX, endY, blockId)` | 矩形区域批量填充，坐标会自动 clamp 到世界边界 |

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
