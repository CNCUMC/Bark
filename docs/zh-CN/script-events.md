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

### 身体（Body）事件

玩家生命体征、意识、行为动作、睡眠与特殊状态的变化事件。所有身体事件均携带 `event.Body`（C# Body 实例）与 `event.Camera`
（PlayerCamera）。

#### 生命体征临界

| 钩子函数                  | 触发时机                            | 附加字段                       |
|---------------------------|-------------------------------------|--------------------------------|
| `onBodyCardiacArrest`     | 心脏骤停 / 恢复心跳（heartRate<20） | `event.IsCardiacArrest` (bool) |
| `onBodyFibrillationStart` | 心室颤动开始                        | —                              |
| `onBodyFibrillationEnd`   | 心室颤动结束                        | —                              |
| `onBodyBreathChange`      | 呼吸停止 / 恢复                     | `event.IsBreathing` (bool)     |

```js
function onBodyCardiacArrest(event) {
    if (event.IsCardiacArrest) {
        Log.Warning('心脏骤停！快进行心肺复苏！');
    } else {
        Log.Info('心跳恢复了');
    }
}
```

#### 意识状态

| 钩子函数                    | 触发时机            | 附加字段                    |
|-----------------------------|---------------------|-----------------------------|
| `onBodyConsciousnessChange` | 昏迷 / 苏醒         | `event.IsConscious` (bool)  |
| `onBodyBrainDying`          | 进入 / 离开濒死状态 | `event.IsBrainDying` (bool) |

```js
function onBodyConsciousnessChange(event) {
    if (event.IsConscious) {
        Log.Info('玩家醒来了');
    } else {
        Log.Warning('玩家昏迷了');
    }
}
```

#### 行为动作

| 钩子函数              | 触发时机         | 附加字段                     |
|-----------------------|------------------|------------------------------|
| `onBodyClimbStart`    | 开始攀爬         | —                            |
| `onBodyClimbEnd`      | 停止攀爬         | —                            |
| `onBodyExerciseStart` | 开始锻炼         | —                            |
| `onBodyExerciseEnd`   | 停止锻炼         | —                            |
| `onBodySwitchHands`   | 交换左右手物品   | —                            |
| `onBodySwitchDir`     | 切换朝向（转身） | `event.IsRight` (bool)       |
| `onBodyCrouchChange`  | 开始 / 停止下蹲  | `event.IsCrouching` (bool)   |
| `onBodyPickUp`        | 拾起物品         | `event.ItemId`、`event.Slot` |
| `onBodyDrop`          | 丢弃物品         | `event.ItemId`               |

```js
function onBodyPickUp(event) {
    Log.Info('捡起了 ' + event.ItemId + '（槽位 ' + event.Slot + '）');
}

function onBodySwitchHands(event) {
    Log.Info('切换了手持物品');
}
```

#### 睡眠 / 特殊状态

| 钩子函数            | 触发时机           | 附加字段                    |
|---------------------|--------------------|-----------------------------|
| `onBodySleepChange` | 入睡 / 醒来        | `event.IsSleeping` (bool)   |
| `onBodyLastStand`   | 成功触发"最后坚持" | —                           |
| `onBodyDisfigure`   | 玩家被毁容         | —                           |
| `onBodyRemoveEye`   | 玩家失去眼睛       | `event.BothEyesGone` (bool) |

```js
function onBodySleepChange(event) {
    if (event.IsSleeping) {
        Log.Info('玩家睡着了');
    } else {
        Log.Info('玩家醒了');
    }
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

> ℹ️ 肢体钩子不携带专属事件字段。如果想知道具体哪个肢体受了伤，在钩子里用 `Limb` 遍历检查。比如
> `Limb.IsBroken(0)` 检查第 0 号肢体是否骨折。

```js
function onLimbBroken(event) {
    // 遍历所有肢体，找出刚断的
    var count = Limb.GetLimbCount();
    var brokenList = [];
    for (var i = 0; i < count; i++) {
        if (Limb.IsBroken(i)) {
            brokenList.push(i);
        }
    }
    Log.Info('骨折的肢体索引: ' + brokenList.join(', '));
}
```

### 物品事件

背包使用、手持使用、装备、脱卸、对肢体使用、攻击都会触发全局钩子。

所有物品事件都携带一个 `event` 对象，包含以下字段：

| 字段           | 类型     | 说明                    |
|----------------|----------|-------------------------|
| `event.ItemId` | `string` | 物品 ID（如 `"arrow"`） |
| `event.Item`   | `Item`   | C# Item 实例            |

`onItemLimbUse` 额外携带：

| 字段              | 类型     | 说明                  |
|-------------------|----------|-----------------------|
| `event.LimbIndex` | `int`    | 目标肢体索引，-1 未知 |
| `event.LimbName`  | `string` | 目标肢体名称          |

| 钩子函数        | 触发时机               |
|-----------------|------------------------|
| `onItemUse`     | 玩家从背包中使用某物品 |
| `onItemHandUse` | 玩家使用手中持有的物品 |
| `onItemEquip`   | 物品被穿戴上           |
| `onItemUnequip` | 物品被卸下             |
| `onItemLimbUse` | 物品被用在某个肢体上   |
| `onItemAttack`  | 手持物品进行近战攻击   |

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

### 状态事件

与自定义状态系统配套的生命周期事件。

所有状态事件都携带一个 `event` 对象，包含以下字段（视事件类型不同）：

| 字段                | 类型       | 说明                            | 适用事件                      |
|---------------------|------------|---------------------------------|-------------------------------|
| `event.MoodleKey`   | `string`   | Moodle 唯一标识                 | `onMoodleGet`、`onMoodleLose` |
| `event.MoodleName`  | `string`   | Moodle 显示名称                 | `onMoodleGet`、`onMoodleLose` |
| `event.Intensity`   | `int`      | Moodle 强度                     | `onMoodleGet`                 |
| `event.Critical`    | `bool`     | 是否严重                        | `onMoodleGet`                 |
| `event.HoldSeconds` | `float`    | 持续时间（秒）                  | `onMoodleGet`                 |
| `event.ActiveKeys`  | `string[]` | 当前所有活跃 Moodle 的 key 列表 | `onMoodleIterate`             |

| 钩子函数          | 触发时机                  |
|-------------------|---------------------------|
| `onMoodleGet`     | Moodle 被应用到玩家身上   |
| `onMoodleIterate` | 轮询（每 0.5 秒触发一次） |
| `onMoodleLose`    | Moodle 到期或被移除       |

```js
function onMoodleGet(event) {
    Log.Info('获得状态: ' + event.MoodleKey);

    if (event.Critical) {
        Player.Alert('严重状态: ' + event.MoodleName, true);
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

### 枪械事件

6 个钩子覆盖枪械操作全流程：开火、拉栓、保险、装弹、卸弹、卡壳。所有枪械事件均携带 `event.GunItem` 字段，返回开火的枪械 Item 对象。

| 字段                   | 类型     | 说明                                                 | 适用事件            |
|------------------------|----------|------------------------------------------------------|---------------------|
| `event.GunItem`        | `Item`   | 操作的枪械 C# Item 实例                              | 全部                |
| `event.Suicide`        | `bool`   | 是否为自杀射击（枪口对准自己）                       | `onGunFire`         |
| `event.Racked`         | `bool`   | 拉栓后的状态（true = 已拉栓/空仓挂机，false = 复位） | `onGunRack`         |
| `event.Safe`           | `bool`   | 保险状态（true = 已开启保险，false = 关闭保险）      | `onGunSafetyToggle` |
| `event.AmmoItemId`     | `string` | 装填的弹药或弹匣 ID                                  | `onGunLoadAmmo`     |
| `event.Rounds`         | `int`    | 装填的弹药数量                                       | `onGunLoadAmmo`     |
| `event.RoundsUnloaded` | `int`    | 卸下的弹药数量                                       | `onGunUnload`       |

| 钩子函数            | 触发时机                                 |
|---------------------|------------------------------------------|
| `onGunFire`         | 枪械开火（Fire() 被调用）                |
| `onGunRack`         | 拉枪栓 / 枪栓复位（TryRack() 被调用）    |
| `onGunSafetyToggle` | 保险切换（ToggleSafety() 被调用）        |
| `onGunLoadAmmo`     | 装弹（LoadMag() 成功装填后触发）         |
| `onGunUnload`       | 卸弹（UnloadMag() 成功卸下弹匣时触发）   |
| `onGunJam`          | 卡壳（拉栓未抛壳或复位未上膛，轮询检测） |

```js
function onGunFire(event) {
    var itemId = event.GunItem.id;
    if (event.Suicide) {
        Log.Warning('玩家用 ' + itemId + ' 自杀！');
        Player.Alert('一切归于沉寂……', true);
        return;
    }
    Log.Info('开火: ' + itemId);
}

function onGunLoadAmmo(event) {
    Log.Info(event.GunItem.id + ' 装填了 ' + event.Rounds + ' 发 ' + event.AmmoItemId);
}

function onGunRack(event) {
    var action = event.Racked ? '拉栓' : '复位';
    Log.Debug(event.GunItem.id + ' ' + action);
}

function onGunSafetyToggle(event) {
    var state = event.Safe ? '开启保险' : '关闭保险';
    Log.Debug(event.GunItem.id + ' ' + state);
}

function onGunUnload(event) {
    Log.Info(event.GunItem.id + ' 卸下弹匣，共 ' + event.RoundsUnloaded + ' 发');
}

function onGunJam(event) {
    Log.Warning(event.GunItem.id + ' 卡壳了！');
    Player.Alert('枪卡壳了！快处理！', true);
}
```

> ℹ️ `onGunJam` 通过每 0.2 秒轮询 `GunScript` 状态检测卡壳，而非 Harmony 补丁。检测逻辑：拉栓后弹膛未排空 →
> 卡壳；枪栓复位后弹匣有弹但仍未上膛 → 卡壳。

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

> ⚠️ `onWorldGenerated` 是第一个可以安全访问 World 的时机。在此之前（包括 `onLoad`）世界还没生成，调用 World
> 会报错。

### 小游戏事件

玩家进行除颤 / 包扎小游戏时触发。

| 钩子函数                       | 触发时机                  | event 字段                            |
|--------------------------------|---------------------------|---------------------------------------|
| `onAEDMinigameStart`           | AED 除颤小游戏开始        | `event.Limb`、`event.LimbIndex`       |
| `onAEDMinigameDefibrillate`    | AED 除颤成功（放电）      | `event.Limb`、`event.WasFibrillating` |
| `onAEDMinigameFail`            | AED 分析失败              | `event.Limb`                          |
| `onBandageMinigameStart`       | 包扎小游戏开始            | `event.Limb`、`event.BandageAngle`    |
| `onBandageMinigameWrap`        | 包扎完成一圈缠绕          | `event.Limb`                          |
| `onDislocationMinigameStart`   | 脱臼复位小游戏开始        | `event.Limb`、`event.HasWrench`       |
| `onDislocationMinigameSuccess` | 肢体复位成功              | `event.Limb`                          |
| `onHandCrankMinigameStart`     | 手摇曲柄小游戏开始        | —                                     |
| `onHandCrankMinigameCharge`    | 转动曲柄给设备充电        | `event.Angle`                         |
| `onHandCrankMinigameEnd`       | 耐力耗尽小游戏结束        | —                                     |
| `onKeypadMinigameStart`        | 键盘密码小游戏开始        | `event.ToDestroy`                     |
| `onKeypadMinigameSuccess`      | 密码正确、目标建筑摧毁    | `event.ToDestroy`                     |
| `onLockpingMinigameStart`      | 撬锁小游戏开始            | `event.ToDestroy`、`event.HasPick`    |
| `onLockpingMinigameSuccess`    | 撬锁成功                  | `event.ToDestroy`                     |
| `onLockpingMinigameStuck`      | 撬锁卡住（损坏工具/手指） | `event.ToDestroy`                     |
| `onManualDefibMinigameStart`   | 手动除颤小游戏开始        | `event.Limb`、`event.OnTorso`         |
| `onManualDefibMinigameShock`   | 手动除颤放电              | `event.Limb`、`event.Charge`          |
| `onManualDefibMinigameEnd`     | 电池耗尽小游戏结束        | `event.Limb`                          |
| `onShrapnelMinigameStart`      | 取弹片小游戏开始          | `event.Limb`、`event.HasTweezers`     |
| `onShrapnelMinigameSuccess`    | 所有弹片取出              | `event.Limb`                          |
| `onShrapnelMinigameFail`       | 夹碎弹片伤口加深          | `event.Limb`                          |
| `onSyringeMinigameStart`       | 注射小游戏开始            | `event.Limb`                          |
| `onSyringeMinigameInject`      | 注射器推入药液            | `event.Limb`                          |
| `onSyringeMinigameFail`        | 注射扎偏（扎碎弹片）      | `event.Limb`                          |
| `onAmputationMinigameStart`    | 截肢小游戏开始            | `event.Limb`                          |
| `onAmputationMinigameSuccess`  | 肢体被切断                | `event.Limb`                          |

```js
function onAEDMinigameDefibrillate(event) {
    if (event.WasFibrillating) {
        Log.Info('除颤成功，心室颤动已停止！');
    }
}
```

### 世界物品与实体事件

玩家操作或世界中的物品/实体状态变化时触发。

| 钩子函数               | 触发时机               | event 字段                                           |
|------------------------|------------------------|------------------------------------------------------|
| `onBatteryLoad`        | 给设备装入电池         | `event.Device`、`event.Battery`、`event.BatteryType` |
| `onBatteryUnload`      | 从设备卸下电池         | `event.Device`、`event.BatteryType`                  |
| `onAutoPumpActive`     | 自动泵开始运作（补压） | `event.Item`                                         |
| `onAutoPumpInactive`   | 自动泵停止运作         | `event.Item`                                         |
| `onBatteryRecharge`    | 电池放入充电器         | `event.Charger`                                      |
| `onBearTrapTrigger`    | 捕兽夹夹住肢体         | `event.Trap`、`event.Limb`                           |
| `onBearTrapRelease`    | 捕兽夹松开             | `event.Trap`                                         |
| `onBioTerminalUse`     | 使用生物终端           | `event.Terminal`、`event.Success`                    |
| `onGroundBlood`        | 流血粒子落地形成血迹   | `event.Position`、`event.Vomit`                      |
| `onBlockDamaged`       | 方块受损 / 被破坏      | `event.Pos`、`event.Damage`、`event.Destroyed`       |
| `onBlueprintCreate`    | 蓝图生成并分配配方     | `event.Blueprint`、`event.RecipeIndex`               |
| `onBoughtItemExpire`   | 已购物品到期被移除     | `event.Item`                                         |
| `onBounceShroomBounce` | 踩到弹跳蘑菇被弹起     | `event.Mushroom`                                     |
| `onBuildingDestroy`    | 建筑实体被完全破坏     | `event.Building`、`event.BuildingId`                 |

```js
function onBearTrapTrigger(event) {
    Log.Warning('捕兽夹夹住了肢体！');
    Player.Alert('疼！', true);
}
```

### 水晶事件

水晶效果被触碰 / 攻击，以及水晶敌人的攻击 / 死亡。

| 钩子函数               | 触发时机                  | event 字段                          |
|------------------------|---------------------------|-------------------------------------|
| `onCrystalTouch`       | 玩家 / 物品触碰到水晶效果 | `event.EffectType`、`event.Crystal` |
| `onCrystalHit`         | 玩家攻击水晶效果          | `event.EffectType`、`event.Crystal` |
| `onCrystalEnemyAttack` | 水晶敌人对玩家突刺攻击    | `event.Enemy`                       |
| `onCrystalEnemyDeath`  | 水晶敌人被击杀            | `event.Enemy`                       |

```js
function onCrystalTouch(event) {
    Log.Info('触碰到水晶效果: ' + event.EffectType);
}
```

### 环境事件

洞穴蜘蛛、可攀爬物、电线圈、尸体等环境对象的状态变化。

| 钩子函数              | 触发时机           | event 字段                             |
|-----------------------|--------------------|----------------------------------------|
| `onCaveTickSpawn`     | 洞穴蜘蛛生成器触发 | `event.Position`                       |
| `onClimbableRegister` | 可攀爬物被注册     | `event.Climbable`、`event.TotalLength` |
| `onCoilShock`         | 电线圈对肢体放电   | `event.Coil`、`event.Limb`             |
| `onCorpseSeen`        | 玩家首次看到尸体   | `event.Corpse`、`event.AnimalCorpse`   |
| `onCorpseDestroy`     | 玩家破坏尸体       | `event.Corpse`                         |

```js
function onCoilShock(event) {
    Log.Warning('被电线圈电到了！');
}
```

### 世界对象事件

可损坏物、伤害板条箱、钻探舱、脊背兽长老、PDA、间歇泉、全局暗幕、捕抓植物、抓钩等对象的状态变化。

| 钩子函数                  | 触发时机                     | event 字段                         |
|---------------------------|------------------------------|------------------------------------|
| `onDamageableDamaged`     | 可损坏物受击                 | `event.Damageable`、`event.Damage` |
| `onDamagingCrateHit`      | 伤害板条箱发生碰撞           | `event.Crate`、`event.Type`        |
| `onDrillPodRepair`        | 钻探舱被维修包修复           | `event.Pod`                        |
| `onDrillPodUse`           | 钻探舱激活重建世界（传送）   | `event.Pod`                        |
| `onThornbackNear`         | 脊背兽长老靠近玩家           | `event.Thornback`                  |
| `onThornbackStage`        | 长老进入下一阶段（狂暴）     | `event.Thornback`、`event.Stage`   |
| `onThornbackDeath`        | 长老被击杀                   | `event.Thornback`                  |
| `onPdaUse`                | 使用 PDA 阅读笔记            | `event.Pda`、`event.FirstRead`     |
| `onGeyserRumble`          | 间歇泉开始轰鸣               | `event.Geyser`                     |
| `onGeyserActivate`        | 间歇泉喷发                   | `event.Geyser`                     |
| `onGlobalDark`            | 全局暗幕开始变暗             | `event.Darkening`                  |
| `onGrabberPlantGrab`      | 捕抓植物抓住玩家肢体         | `event.Plant`                      |
| `onGrapplingHookFire`     | 抓钩发射                     | `event.Hook`                       |
| `onGrapplingHookHit`      | 抓钩勾住表面                 | `event.Hook`                       |
| `onGrapplingHookReturn`   | 抓钩收回                     | `event.Hook`                       |
| `onItemDestroy`           | 物品耐久归零被销毁           | `event.ItemId`、`event.Item`       |
| `onJumpPadBounce`         | 踩上跳跃平台被弹起           | `event.Pad`                        |
| `onLifepodButtonPress`    | 按下救生舱按钮               | `event.Type`                       |
| `onLifepodShowerActivate` | 救生舱淋浴激活               | `event.Shower`                     |
| `onMedStationHeal`        | 进入医疗站开始治疗           | `event.Station`                    |
| `onMineTrigger`           | 地雷被触发                   | `event.Mine`                       |
| `onObserverLastStand`     | 成功"最后坚持"（观察者拉近） | `event.Observer`                   |
| `onObserverGunSuicide`    | 用枪自杀（观察者拉近）       | `event.Observer`                   |
| `onOpenableUse`           | 打开可开启物（门/箱）        | `event.Openable`、`event.Mode`     |
| `onPlushSqueak`           | 毛绒玩具被挤压吱吱叫         | `event.Plush`                      |
| `onPreRunStart`           | 开始新游戏                   | —                                  |
| `onPreRunLoad`            | 读取存档继续游戏             | —                                  |
| `onPreRunTutorial`        | 开始教程                     | —                                  |
| `onOpiateOverdose`        | 阿片类水平过高（中毒）       | —                                  |
| `onSelfDestruct`          | 触发自毁序列                 | —                                  |
| `onWoundViewToggle`       | 打开/关闭伤口面板            | `event.Open`                       |
| `onCraftPanelToggle`      | 打开/关闭制作面板            | `event.Open`                       |
| `onAmmoUnload`            | 从弹匣卸下一发子弹           | `event.Magazine`                   |
| `onAmmoLoad`              | 向弹匣装入一发子弹           | `event.Magazine`                   |
| `onAltHoverToggle`        | 按住/切换 Alt 显示物品标签   | `event.Active`                     |

```js
function onThornbackStage(event) {
    Log.Warning('脊背兽长老进入了第 ' + event.Stage + ' 阶段！');
}

function onPdaUse(event) {
    if (event.FirstRead) {
        Log.Info('首次阅读 PDA 笔记，获得经验');
    }
}
```

### 系统事件

精神抹除、辐射线、存档、技能升级、商人、炮塔、世界重生、电锯、声波炮等系统级事件。

| 钩子函数             | 触发时机               | event 字段                                            |
|----------------------|------------------------|-------------------------------------------------------|
| `onMindwipe`         | 触发精神抹除           | —                                                     |
| `onRadiationStart`   | 辐射线开始逼近         | —                                                     |
| `onGameSave`         | 保存游戏               | —                                                     |
| `onSkillLevelUp`     | 属性升级               | `event.Stat`、`event.OldLevel`、`event.NewLevel`      |
| `onTraderMeet`       | 与商人开始对话         | `event.Trader`、`event.Character`、`event.Reputation` |
| `onTraderHaggle`     | 与商人讲价             | `event.Trader`、`event.Reputation`                    |
| `onTraderDeath`      | 商人被击杀             | `event.Trader`                                        |
| `onTurretShoot`      | 炮塔开火               | `event.Turret`                                        |
| `onTurretExplode`    | 炮塔被摧毁爆炸         | `event.Turret`                                        |
| `onWorldRegenerate`  | 世界重生（进入下一层） | `event.Twice`                                         |
| `onSawbladeHit`      | 电锯锯到肢体           | `event.Sawblade`                                      |
| `onSoundCannonShoot` | 声波炮发射             | `event.Cannon`                                        |

```js
function onSkillLevelUp(event) {
    var statName = ['力量', '耐力', '智力'][event.Stat];
    Log.Info(statName + ' 升级到 ' + event.NewLevel);
}

function onWorldRegenerate(event) {
    Log.Info(event.Twice ? '连续跨越两层！' : '进入下一层');
}
```

### 命令事件

玩家在控制台输入脚本模组注册的自定义命令时触发。命令通过 `Command/*.json` 定义，详见 [脚本命令](script-mod/command.md)。

| 字段                | 类型       | 说明                                                       |
|---------------------|------------|------------------------------------------------------------|
| `event.CommandName` | `string`   | 触发的命令名称（不含参数）                                 |
| `event.Args`        | `string[]` | 完整输入列表（`args[0]` 为命令名，`args[1..]` 为用户参数） |

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

除了全局钩子，你还可以通过 JSON 为特定物品绑定脚本。当该物品触发某个动作（使用、攻击、装备等）时，Bark 执行这些脚本并 调用其中的
`main()` 函数，传入参数。

详见 [自定义物品](script-mod/item.md) 了解如何配置。脚本侧写法如下：

```js
// arrow.js — 在 arrow.json 的 "attack" 下注册
function main(itemId, item, action) {
    // itemId: "arrow"
    // item:    C# Item 实例
    // action:  "attack" / "use" / "equip" / "unequip" / "use_in_hand" / "use_on_limb"
    Item.Destroy(itemId);
    Player.Alert('箭无虚发！', true);
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
    Player.Alert('又断了根骨头……', true);
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
