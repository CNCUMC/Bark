# PlayerUtil — 玩家操作

PlayerUtil 提供传送、拾取物品和提示弹窗。方法少，直接给示例。

## 传送

```js
// 传送到指定坐标
PlayerUtil.Teleport(100, 200);

// 获取当前位置
var pos = PlayerUtil.GetPosition();
Log.Info('当前位置: ' + pos.x + ', ' + pos.y);
```

## 拾取物品

```js
// 把物品放入第 0 号格子
PlayerUtil.PickUpItem('backpack', 0);

// 强制放入已有物品的格子（第三个参数 force）
PlayerUtil.PickUpItem('rifle', 1, true);
```

> ℹ️ 背包只有 8 个格子（`PlayerUtil.MaxInventorySlots`），slot 从 0 到 7。

`PickUpItem` 的 `force` 参数默认为 `false`，可以省略。详见 [命名规则](../script-mod.md#api-命名规则) 中关于可选参数的说明。

## 提示弹窗

```js
// 普通提示
PlayerUtil.Alert('找到钥匙了', false);

// 重要提示（红色）
PlayerUtil.Alert('你中毒了！', true);

// 延时提示（第三个参数 delay，单位秒，可省略）
PlayerUtil.Alert('3 秒后显示', false, 3);
```

`delay` 默认为 `0`（立即显示），可省略。

## 完整示例

一键回家 + 装备满配：

```js
function onLoad() {
    // 绑定按键（如果有输入系统的话）
    Log.Info('PlayerUtil 示例模组已加载');
}

function onWorldGenerated() {
    // 传送回世界原点
    PlayerUtil.Teleport(0, 0);

    // 凑齐一套装备
    PlayerUtil.PickUpItem('backpack', 0);
    PlayerUtil.PickUpItem('rifle', 1, true);
    PlayerUtil.PickUpItem('ammo_rifle', 2);
    PlayerUtil.PickUpItem('medkit', 3);

    PlayerUtil.Alert('装备已就绪', false);
}
```
