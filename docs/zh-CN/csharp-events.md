[English](../en-US/csharp-events.md) | ***简体中文***

# C# 事件系统

Bark 的事件系统让 C# 模组之间解耦通信，同时桥接到脚本引擎。它基于 Attribute 扫描，零手动注册。

## 订阅事件

给你的类加上 `[EventBusSubscriber]`，然后写 `public static` 方法，参数为你想监听的事件类型。

```csharp
using Bark.Event;
using Bark.Events;

[EventBusSubscriber(Plugin.Guid)]  // 用你插件的 GUID
public static class MyEventHandlers
{
    // 方法签名：public static void 方法名(BarkEvent子类 参数)
    public static void OnPlayerJump(PlayerJumpStartEvent evt)
    {
        LogUtil.Info($"玩家起跳！时间: {evt.Time}");
    }

    public static void OnPlayerDeath(PlayerDeathEvent evt)
    {
        LogUtil.Warning("玩家死了");
    }
}
```

启动时 Bark 自动扫描所有程序集，找到 `[EventBusSubscriber]` 类中的匹配方法并注册。你什么都不用做。

> ℹ️ 方法名随意， Bark 只看参数类型。但建议以 `On` 开头保持可读。

## 触发事件

三种方式：

```csharp
using Bark.Event;
using Bark.Events;
using Bark.Tool;

// 方式 1：构造实例触发
var evt = new LimbBrokenEvent { LimbIndex = 2, LimbName = "左腿" };
EventUtil.Trigger(evt);

// 方式 2：泛型触发（无参构造）
EventUtil.Trigger<MainMenuLoadedEvent>();

// 方式 3：不用 EventUtil，直接调 EventRegistry
EventRegistry.Trigger(new WorldReadyEvent { World = WorldGeneration.world });
```

`EventUtil.Trigger<T>()` 适用于不需要设置属性的事件。`EventUtil.Trigger(evt)` 适用于需要传数据的事件。

## 脚本侧监听 C# 事件

通过 `EventUtil.On<T>()` 注册，配合 `UnregisterAll` 清理：

```csharp
// 在模组 Awake 里手动注册
EventUtil.On<PlayerDeathEvent>(evt =>
{
    LogUtil.Warning($"玩家死亡，时间: {evt.Time}");
}, Plugin.Guid);

// 卸载时清理
EventUtil.UnregisterAll(Plugin.Guid);
```

这和 `[EventBusSubscriber]` 的区别是：`On<T>` 是手动注册，适合动态场景；`[EventBusSubscriber]` 是自动扫描，适合静态处理器。

## 事件类型一览

### 玩家事件

| C# 类型                | 属性                         | 触发描述   |
|------------------------|------------------------------|------------|
| `PlayerJumpStartEvent` | `Body body`, `Camera camera` | 按下跳跃键 |
| `PlayerJumpOverEvent`  | `Body body`, `Camera camera` | 起跳后落地 |
| `PlayerDeathEvent`     | `Body body`, `Camera camera` | 玩家死亡   |

### 身体（Body）事件

所有身体事件均携带 `Body body` 与 `Camera camera`。

#### 生命体征临界 / 意识

| C# 类型                        | 附加属性               | 触发描述            |
|--------------------------------|------------------------|---------------------|
| `BodyCardiacArrestEvent`       | `bool IsCardiacArrest` | 心脏骤停 / 恢复心跳 |
| `BodyFibrillationStartEvent`   | —                      | 心室颤动开始        |
| `BodyFibrillationEndEvent`     | —                      | 心室颤动结束        |
| `BodyBreathChangeEvent`        | `bool IsBreathing`     | 呼吸停止 / 恢复     |
| `BodyConsciousnessChangeEvent` | `bool IsConscious`     | 昏迷 / 苏醒         |
| `BodyBrainDyingEvent`          | `bool IsBrainDying`    | 进入 / 离开濒死     |

#### 行为动作 / 睡眠 / 特殊状态

| C# 类型                  | 附加属性                    | 触发描述        |
|--------------------------|-----------------------------|-----------------|
| `BodyClimbStartEvent`    | —                           | 开始攀爬        |
| `BodyClimbEndEvent`      | —                           | 停止攀爬        |
| `BodyExerciseStartEvent` | —                           | 开始锻炼        |
| `BodyExerciseEndEvent`   | —                           | 停止锻炼        |
| `BodySwitchHandsEvent`   | —                           | 交换左右手物品  |
| `BodySwitchDirEvent`     | `bool IsRight`              | 切换朝向        |
| `BodyCrouchChangeEvent`  | `bool IsCrouching`          | 开始 / 停止下蹲 |
| `BodyPickUpEvent`        | `string ItemId`, `int Slot` | 拾起物品        |
| `BodyDropEvent`          | `string ItemId`             | 丢弃物品        |
| `BodySleepChangeEvent`   | `bool IsSleeping`           | 入睡 / 醒来     |
| `BodyLastStandEvent`     | —                           | 最后坚持成功    |
| `BodyDisfigureEvent`     | —                           | 玩家被毁容      |
| `BodyRemoveEyeEvent`     | `bool BothEyesGone`         | 玩家失去眼睛    |

### 肢体事件

| C# 类型                 | 属性                               | 触发描述 |
|-------------------------|------------------------------------|----------|
| `LimbBrokenEvent`       | `int LimbIndex`, `string LimbName` | 骨骼断裂 |
| `LimbMendedEvent`       | `int LimbIndex`, `string LimbName` | 骨骼治愈 |
| `LimbDislocatedEvent`   | `int LimbIndex`, `string LimbName` | 关节脱臼 |
| `LimbUnDislocatedEvent` | `int LimbIndex`, `string LimbName` | 脱臼复位 |
| `LimbDismemberedEvent`  | `int LimbIndex`, `string LimbName` | 肢体截断 |
| `LimbInfectedEvent`     | `int LimbIndex`, `string LimbName` | 伤口感染 |

### Moodle 事件

| C# 类型              | 属性                                                                                           | 触发描述          |
|----------------------|------------------------------------------------------------------------------------------------|-------------------|
| `MoodleGetEvent`     | `string MoodleKey`, `string MoodleName`, `int Intensity`, `bool Critical`, `float HoldSeconds` | Moodle 应用到玩家 |
| `MoodleIterateEvent` | `string[] ActiveKeys`                                                                          | 轮询（每 0.5 秒） |
| `MoodleLoseEvent`    | `string MoodleKey`, `string MoodleName`                                                        | Moodle 到期或移除 |

### 世界 / 菜单事件

| C# 类型               | 属性                    | 触发描述     |
|-----------------------|-------------------------|--------------|
| `MainMenuLoadedEvent` | 无（仅继承 `Time`）     | 进入主菜单   |
| `WorldReadyEvent`     | `WorldGeneration World` | 世界生成完毕 |

## 自定义事件

定义自己的事件类型，脚本也能监听。

### 1. 定义事件类

```csharp
using Bark.Event;

namespace MyMod.Events;

// 继承 BarkEvent，加 [ScriptEvent] 就能让脚本侧也收到
[ScriptEvent("onMyCustomEvent")]  // 不加这个就只能 C# 用
public class MyCustomEvent : BarkEvent
{
    public string Message { get; set; } = string.Empty;
    public int Value { get; set; }
}
```

### 2. 触发

```csharp
// C# 侧触发
EventUtil.Trigger(new MyCustomEvent
{
    Message = "Hello from C#",
    Value = 42
});
```

### 3. 脚本侧接收

加了 `[ScriptEvent("onMyCustomEvent")]` 后，脚本里定义同名函数就行：

```js
function onMyCustomEvent() {
    Log.Info('收到 C# 发来的自定义事件');
}
```

> ℹ️ 脚本钩子函数不带参数，这是由 `ScriptEventScanner` 的设计决定的。如需传数据，让脚本在钩子里调 API 查询。

## API 参考

| 方法                                                           | 说明                     |
|----------------------------------------------------------------|--------------------------|
| `EventUtil.Trigger(BarkEvent evt)`                             | 触发有数据的事件         |
| `EventUtil.Trigger<T>()`                                       | 触发无参构造事件         |
| `EventUtil.On<T>(Action<T>, string guid)`                      | 手动注册处理器           |
| `EventUtil.UnregisterAll(string guid)`                         | 清理某个模组的所有处理器 |
| `EventRegistry.Register(Type, Action<BarkEvent>, string guid)` | 底层注册（脚本引擎在用） |
| `EventRegistry.Unregister(Type, string guid)`                  | 底层注销                 |

## 注意事项

- `[EventBusSubscriber]` 的 GUID 应该用你自己的插件 GUID，不要用 `Plugin.Guid` 去订阅别人的模组
- 处理器方法里抛异常会被 `EventRegistry` 捕获并记录日志，不会影响其他处理器
- 事件沿继承链向上冒泡：订阅 `BarkEvent` 基类会收到所有事件
- 确保在 `Unload` 时调用 `EventUtil.UnregisterAll` 清理手动注册的处理器
