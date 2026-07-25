[English](../../en-US/script-api/world.md) | ***简体中文***

# WorldUtil — 世界编辑

WorldUtil 提供方块放置、区域填充和物品生成。只有 5 个方法，但够你改造世界了。

> ⚠️ 所有方法必须在 `onWorldGenerated` 之后调用，主菜单阶段世界不存在。

## 方块操作

```js
// 放置单个方块
WorldUtil.PlaceBlock(50, 30, 18);   // 18 号是 Marble 方块

// 区域填充
WorldUtil.FillBlocks(0, 0, 10, 5, 3);  // 从 (0,0) 到 (10,5) 填满 3 号方块
```

| 方法                                              | 说明                                          |
|---------------------------------------------------|-----------------------------------------------|
| `PlaceBlock(x, y, blockId)`                       | 在指定坐标放一个方块                          |
| `FillBlocks(startX, startY, endX, endY, blockId)` | 矩形区域批量填充，坐标会自动 clamp 到世界边界 |

## 物品生成

```js
// 生成物品到地上
WorldUtil.PlaceItem(100, 50, 'medkit');
WorldUtil.PlaceItem(102, 50, 'ammo_rifle');
```

## 世界尺寸

```js
var w = WorldUtil.GetWidth();   // 世界宽度（块）
var h = WorldUtil.GetHeight();  // 世界高度（块）
```

## 完整示例

画一圈墙把自己围起来：

```js
function onWorldGenerated() {
    var pos = PlayerUtil.GetPosition();
    var cx = Math.floor(pos.x);
    var cy = Math.floor(pos.y);
    var r = 5;

    // 画四条边
    WorldUtil.FillBlocks(cx - r, cy - r, cx + r, cy - r, 18);  // 下边
    WorldUtil.FillBlocks(cx - r, cy + r, cx + r, cy + r, 18);  // 上边
    WorldUtil.FillBlocks(cx - r, cy - r, cx - r, cy + r, 18);  // 左边
    WorldUtil.FillBlocks(cx + r, cy - r, cx + r, cy + r, 18);  // 右边

    // 门口放个医疗包
    WorldUtil.PlaceItem(cx, cy - r + 1, 'medkit');

    Log.Info('城墙已建好');
}
```
