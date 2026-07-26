[English](../en-US/script-events.md) | ***简体中文***

# 脚本事件钩子

Bark 内置的事件钩子。当游戏中发生对应事件时，Bark 自动调用你脚本里的同名函数，并传入一个包含事件数据的 `event` 对象。

## 怎么用

定义全局函数，名字跟钩子一致即可。Bark 会自动传入 `event` 参数。

```js
function onPlayerJumpStart(event) {
    Log.Info('玩家起跳');
}
```

不需要事件数据时可以省略参数——JavaScript 和 Lua 自动忽略多余参数：

```js
function onPlayerJumpStart() {
    Log.Info('玩家起跳');  // 同样 OK
}
```

就这么简单。不需要注册、不需要引入任何东西。

## 钩子一览

### 玩家事件

| 钩子函数            | 触发时机   | event 字段 |
|---------------------|------------|------------|
| `onPlayerJumpStart` | 按下跳跃键 | —          |
| `onPlayerJumpOver`  | 起跳后落地 | —          |
| `onPlayerDeath`     | 玩家死亡   | —          |

```js
function onPlayerDeath(event) {
    Log.Warning('玩家死了');
    // 死亡时自动存档之类的逻辑
}
```

### 肢体事件

所有肢体相关事件，6 个钩子覆盖骨折、脱臼、感染、截肢等状态变化。

| 钩子函数             | 触发时机 | event 字段 |
|----------------------|----------|------------|
| `onLimbBroken`       | 骨骼断裂 | —          |
| `onLimbMended`       | 骨骼治愈 | —          |
| `onLimbDislocated`   | 关节脱臼 | —          |
| `onLimbUnDislocated` | 脱臼复位 | —          |
| `onLimbDismembered`  | 肢体截断 | —          |
| `onLimbInfected`     | 伤口感染 | —          |

> ℹ️ 肢体钩子不携带专属事件字段。如果想知道具体哪个肢体受了伤，在钩子里用 `LimbUtil` 遍历检查。比如
> `LimbUtil.IsBroken(0)` 检查第 0 号肢体是否骨折。

```js
function onLimbBroken(event) {
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

背包使用、手持使用、装备、脱卸、对肢体使用、攻击都会触发全局钩子。

所有物品事件都携带一个 `event` 对象，包含以下字段：

| 字段            | 类型     | 说明                    |
|-----------------|----------|-------------------------|
| `event.ItemId`  | `string` | 物品 ID（如 `"arrow"`） |
| `event.Item`    | `Item`   | C# Item 实例            |

`onItemLimbUse` 额外携带：

| 字段               | 类型     | 说明                 |
|--------------------|----------|----------------------|
| `event.LimbIndex`  | `int`    | 目标肢体索引，-1 未知 |
| `event.LimbName`   | `string` | 目标肢体名称          |

| 钩子函数         | 触发时机               |
|------------------|------------------------|
| `onItemUse`      | 玩家从背包中使用某物品 |
| `onItemHandUse`  | 玩家使用手中持有的物品 |
| `onItemEquip`    | 物品被穿戴上           |
| `onItemUnequip`  | 物品被卸下             |
| `onItemLimbUse`  | 物品被用在某个肢体上   |
| `onItemAttack`   | 手持物品进行近战攻击   |

```js
function onItemUse(event) {
    Log.Info('使用了: ' + event.ItemId);
    // event.Item 提供 C# Item 实例供高级操作
}

function onItemLimbUse(event) {
    Log.Info(event.ItemId + ' 被用在肢体 ' + event.LimbName + ' 上');
}

function onItemAttack(event) {
    Log.Info('用 ' + event.ItemId + ' 攻击');
}
```

### Moodle 事件

与自定义 Moodle 系统配套的生命周期事件。

所有 Moodle 事件都携带一个 `event` 对象，包含以下字段（视事件类型不同）：

| 字段                  | 类型       | 说明                     | 适用事件           |
|-----------------------|------------|--------------------------|--------------------|
| `event.MoodleKey`     | `string`   | Moodle 唯一标识          | `onMoodleGet`、`onMoodleLose` |
| `event.MoodleName`    | `string`   | Moodle 显示名称          | `onMoodleGet`、`onMoodleLose` |
| `event.Intensity`     | `int`      | Moodle 强度              | `onMoodleGet`      |
| `event.Critical`      | `bool`     | 是否严重                 | `onMoodleGet`      |
| `event.HoldSeconds`   | `float`    | 持续时间（秒）           | `onMoodleGet`      |
| `event.ActiveKeys`    | `string[]` | 当前所有活跃 Moodle 的 key 列表 | `onMoodleIterate`  |

| 钩子函数          | 触发时机                  |
|-------------------|---------------------------|
| `onMoodleGet`     | Moodle 被应用到玩家身上   |
| `onMoodleIterate` | 轮询（每 0.5 秒触发一次） |
| `onMoodleLose`    | Moodle 到期或被移除       |

```js
function onMoodleGet(event) {
    Log.Info('获得状态: ' + event.MoodleKey);

    if (event.Critical) {
        PlayerUtil.Alert('严重状态: ' + event.MoodleName, true);
    }
}

function onMoodleIterate(event) {
    // 每 0.5 秒触发，event.ActiveKeys 包含所有活跃状态
    Log.Debug('活跃状态: ' + event.ActiveKeys.join(', '));
}

function onMoodleLose(event) {
    Log.Info('状态消失: ' + event.MoodleKey);
}
```

### 世界 / 菜单事件

| 钩子函数           | 触发时机                           | event 字段 |
|--------------------|------------------------------------|------------|
| `onMainMenuLoaded` | 进入主菜单                         | —          |
| `onWorldGenerated` | 世界生成完毕，可以安全访问世界数据 | —          |

```js
function onWorldGenerated(event) {
    Log.Info('世界已就绪，模组初始化完成');
    // 在这里做需要世界数据的操作
}

function onMainMenuLoaded(event) {
    Log.Info('回到了主菜单');
}
```

> ⚠️ `onWorldGenerated` 是第一个可以安全访问 WorldUtil 的时机。在此之前（包括 `onLoad`）世界还没生成，调用 WorldUtil
> 会报错。

### 命令事件

玩家在控制台输入脚本模组注册的自定义命令时触发。命令通过 `Command/*.json` 定义，详见 [脚本命令](script-mod/command.md)。

| 字段                 | 类型       | 说明                                         |
|----------------------|------------|----------------------------------------------|
| `event.CommandName`  | `string`   | 触发的命令名称（不含参数）                   |
| `event.Args`         | `string[]` | 完整输入列表（`args[0]` 为命令名，`args[1..]` 为用户参数） |

| 钩子函数    | 触发时机                 |
|-------------|--------------------------|
| `onCommand` | 玩家输入已注册的脚本命令 |

```js
function onCommand(event) {
    Log.Info('收到命令: ' + event.CommandName);
    Log.Info('参数: ' + event.Args.join(', '));
}
```

## 物品脚本

除了全局钩子，你还可以通过 JSON 为特定物品绑定脚本。当该物品触发某个动作（使用、攻击、装备等）时，Bark 执行这些脚本并
调用其中的 `main()` 函数，传入参数。

详见 [自定义物品](script-mod/item.md) 了解如何配置。脚本侧写法如下：

```js
// arrow.js — 在 arrow.json 的 "attack" 下注册
function main(itemId, item, action) {
    // itemId: "arrow"
    // item:    C# Item 实例
    // action:  "attack" / "use" / "equip" / "unequip" / "use_in_hand" / "use_on_limb"
    itemUtil.Destroy(itemId);
    PlayerUtil.Alert('箭无虚发！', true);
}
```

`main` 函数接受 0 到 3 个参数——JavaScript 和 Lua 自动忽略多余参数。常见写法：

```js
function main(itemId)               { /* 只需 ID */ }
function main(itemId, item)         { /* 需要物品对象 */ }
function main(itemId, item, action) { /* 完整上下文 */ }
```

向后兼容：旧式的顶层 `__barkItemId` 全局变量仍然可用，但推荐使用 `main()` 函数。

## 完整示例

一个记录所有受伤事件的脚本模组：

```js
// 在 onLoad 里初始化统计
var injuredCount = 0;
var brokenCount = 0;

function onLoad() {
    Log.Info('伤势追踪模组已加载');
}

function onLimbBroken(event) {
    brokenCount++;
    injuredCount++;
    Log.Warning('骨折！共 ' + brokenCount + ' 次骨折，' + injuredCount + ' 次受伤');
    PlayerUtil.Alert('又断了根骨头……', true);
}

function onLimbInfected(event) {
    injuredCount++;
    Log.Warning('感染！共 ' + injuredCount + ' 次受伤');
}

function onPlayerDeath(event) {
    Log.Warning('玩家死亡。本次统计：骨折 ' + brokenCount + ' 次，总受伤 ' + injuredCount + ' 次');
    injuredCount = 0;
    brokenCount = 0;
}
```

## 注意事项

- 钩子函数名必须完全一致，大小写敏感
- 钩子接收一个 `event` 对象——物品事件提供 `event.ItemId` 和 `event.Item`
- 钩子里的代码要尽量快，不要阻塞。耗时操作用 `setInterval` / `setTimeout` 异步处理
- 钩子触发频率不定 —— `onLimbBroken` 可能连续触发多次（多肢体同时骨折），要做好幂等处理
- 同名钩子可以被多个脚本模组定义，互不干扰
