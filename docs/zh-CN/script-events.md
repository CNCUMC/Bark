[English](../en-US/script-events.md) | ***简体中文***

# 脚本事件钩子

Bark 内置了 15 个事件钩子。当游戏中发生对应事件时， Bark 自动调用你脚本里的同名函数。

## 怎么用

定义全局函数，名字跟钩子一致即可。函数不需要参数， Bark 负责在合适的时机调用。

```js
function onPlayerJumpStart() {
    Log.Info('玩家起跳');
}
```

就这么简单。不需要注册、不需要引入任何东西。

## 钩子一览

### 玩家事件

| 钩子函数            | 触发时机   |
|---------------------|------------|
| `onPlayerJumpStart` | 按下跳跃键 |
| `onPlayerJumpOver`  | 起跳后落地 |
| `onPlayerDeath`     | 玩家死亡   |

```js
function onPlayerDeath() {
    Log.Warning('玩家死了');
    // 死亡时自动存档之类的逻辑
}
```

### 肢体事件

所有肢体相关事件，6 个钩子覆盖骨折、脱臼、感染、截肢等状态变化。

| 钩子函数             | 触发时机 |
|----------------------|----------|
| `onLimbBroken`       | 骨骼断裂 |
| `onLimbMended`       | 骨骼治愈 |
| `onLimbDislocated`   | 关节脱臼 |
| `onLimbUnDislocated` | 脱臼复位 |
| `onLimbDismembered`  | 肢体截断 |
| `onLimbInfected`     | 伤口感染 |

> ℹ️ 肢体钩子不传参。如果想知道具体哪个肢体受了伤，在钩子里用 `LimbUtil` 遍历检查。比如 `LimbUtil.IsBroken(0)` 检查第 0
> 号肢体是否骨折。

```js
function onLimbBroken() {
    // 遍历所有肢体，找出刚断的
    var count = LimbUtil.GetLimbCount();
    var brokenList = [];
    for (var i = 0; i < count; i++) {
        if (LimbUtil.IsBroken(i)) {
            brokenList.push(i);
        }
    }
    Log.Info('骨折的肢体索引: ' + brokenList.join(', '));
}
```

### 物品事件

物品使用、装备、脱卸、对肢体使用都会触发全局钩子。

| 钩子函数         | 触发时机               |
|------------------|------------------------|
| `onItemUse`      | 玩家使用某物品         |
| `onItemEquip`    | 物品被穿戴上           |
| `onItemUnequip`  | 物品被卸下             |
| `onItemLimbUse`  | 物品被用在某个肢体上   |

> ℹ️ 物品钩子不传参。如果需要知道具体是哪个物品，可以在钩子里用 `InventoryUtil` 查询当前装备/手持物品。

```js
function onItemUse() {
    Log.Info('使用了某个物品');
}

function onItemLimbUse() {
    Log.Info('物品被用在肢体上');
}
```

### 世界 / 菜单事件

| 钩子函数           | 触发时机                           |
|--------------------|------------------------------------|
| `onMainMenuLoaded` | 进入主菜单                         |
| `onWorldGenerated` | 世界生成完毕，可以安全访问世界数据 |

```js
function onWorldGenerated() {
    Log.Info('世界已就绪，模组初始化完成');
    // 在这里做需要世界数据的操作
}

function onMainMenuLoaded() {
    Log.Info('回到了主菜单');
}
```

> ⚠️ `onWorldGenerated` 是第一个可以安全访问 WorldUtil 的时机。在此之前（包括 `onLoad`）世界还没生成，调用 WorldUtil 会报错。

## 完整示例

一个记录所有受伤事件的模组：

```js
// 在 onLoad 里初始化统计
var injuredCount = 0;
var brokenCount = 0;

function onLoad() {
    Log.Info('伤势追踪模组已加载');
}

function onLimbBroken() {
    brokenCount++;
    injuredCount++;
    Log.Warning('骨折！共 ' + brokenCount + ' 次骨折，' + injuredCount + ' 次受伤');
    PlayerUtil.Alert('又断了根骨头……', true);
}

function onLimbInfected() {
    injuredCount++;
    Log.Warning('感染！共 ' + injuredCount + ' 次受伤');
}

function onPlayerDeath() {
    Log.Warning('玩家死亡。本次统计：骨折 ' + brokenCount + ' 次，总受伤 ' + injuredCount + ' 次');
    injuredCount = 0;
    brokenCount = 0;
}
```

## 注意事项

- 钩子函数名必须完全一致，大小写敏感
- 钩子不能带参数， Bark 回调时不传参
- 钩子里的代码要尽量快，不要阻塞。耗时操作用 `setInterval` / `setTimeout` 异步处理
- 钩子触发频率不定 —— `onLimbBroken` 可能连续触发多次（多肢体同时骨折），要做好幂等处理
- 同名钩子可以被多个模组定义，互不干扰
