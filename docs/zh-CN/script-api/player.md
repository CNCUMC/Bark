[English](../../en-US/script-api/player.md) | ***简体中文***

# Player — 玩家操作

Player 提供传送、拾取物品和提示弹窗。方法少，直接给示例。

## 传送

```js
// 传送到指定坐标
Player.Teleport(100, 200);

// 获取当前位置
var pos = Player.GetPosition();
Log.Info('当前位置: ' + pos.x + ', ' + pos.y);
```

## 拾取物品

```js
// 把物品放入第 0 号格子
Player.PickUpItem('backpack', 0);

// 强制放入已有物品的格子（第三个参数 force）
Player.PickUpItem('rifle', 1, true);

// 自动放入一个物品 不限位置
Player.AutoPickUpItem('rifle');

// 自动放入一个物品 给两个
Player.AutoPickUpItem('rifle', 2);
```

> ℹ️ 背包只有 8 个格子（`Player.MaxInventorySlots`），slot 从 0 到 7。

`PickUpItem` 的 `force` 参数默认为 `false`，可以省略。详见 [命名规则](../script-mod.md#api-命名规则) 中关于可选参数的说明。

## 提示弹窗

```js
// 普通提示
Player.Alert('找到钥匙了', false);

// 重要提示（红色）
Player.Alert('你中毒了！', true);

// 延时提示（第三个参数 delay，单位秒，可省略）
Player.Alert('3 秒后显示', false, 3);
```

`delay` 默认为 `0`（立即显示），可省略。

## 说话

让玩家说出对话气泡（或通过电子设备如手表）。

```js
// 玩家说一句台词
Player.Talk('我得找点吃的。');

// 通过通用电子设备说话
Player.TalkElectronic('正在搜索信号...');

// 通过指定电子物品说话
Player.TalkElectronic('正在分析样本...', 'watch');
```

| 方法                                  | 说明                                     |
|---------------------------------------|------------------------------------------|
| `Talk(dialogue)`                      | 玩家说出对话气泡                         |
| `TalkElectronic(dialogue)`            | 通过通用电子发声代理说话                 |
| `TalkElectronic(dialogue, itemId)`    | 通过指定电子物品说话                     |

> `TalkElectronic` 的 `itemId` 参数可省略，遵循与其他物品 ID 相同的模组 id 前缀补全规则。

## 完整示例

一键回家 + 装备满配：

```js
function onLoad() {
    // 绑定按键（如果有输入系统的话）
    Log.Info('Player 示例模组已加载');
}

function onWorldGenerated() {
    // 传送回世界原点
    Player.Teleport(0, 0);

    // 凑齐一套装备
    Player.PickUpItem('backpack', 0);
    Player.PickUpItem('rifle', 1, true);
    Player.PickUpItem('ammo_rifle', 2);
    Player.PickUpItem('medkit', 3);

    Player.Alert('装备已就绪', false);
}
```
