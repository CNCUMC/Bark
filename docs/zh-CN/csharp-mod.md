# C# 模组开发

如果你更喜欢用 C# 而不是脚本语言， Bark 提供了一套完整的事件系统和工具 API，让你可以直接写 C# 代码扩展游戏功能。

## 创建项目

推荐用 CNCUMC 的 [Moss Template](https://github.com/CNCUMC/Moss-Template) 作为模板，它已经配好了 BepInEx + CCL 的引用，开箱即用。

在 `Plugin.Awake()` 里写你的初始化逻辑。

## 订阅事件

用 `[EventBusSubscriber]` 标记你的类，再写 `public static` 方法，参数是 `BarkEvent` 子类。Bark 启动时自动扫描并注册，不需要手动 `+=`。

```csharp
using Bark.Event;
using Bark.Events;

[EventBusSubscriber("org.cncumc.bark")]
public static class MyEventHandlers
{
    public static void OnPlayerJump(PlayerJumpStartEvent evt)
    {
        Logger.LogInfo($"玩家起跳，时间: {evt.Time}");
    }

    public static void OnPlayerDeath(PlayerDeathEvent evt)
    {
        Logger.LogWarning("玩家死了");
    }
}
```

- 类上打 `[EventBusSubscriber("你的 GUID")]`
- 方法必须是 `public static`，接受一个 `BarkEvent` 子类参数
- 方法名随意， Bark 依据参数类型匹配
- 一个类可以写多个处理方法，一个处理一个事件类型

完整事件类型列表见 [C# 事件一览](csharp-events.md)。

## 调用工具 API

`Tool/` 下的所有静态类都可以直接调用：

```csharp
using Bark.Tool;

// 读写玩家生理数据
var hunger = BodyUtil.GetHunger();
BodyUtil.SetHunger(hunger + 10);

// 操作肢体
LimbUtil.Break(0);          // 折断第 0 号肢体
LimbUtil.Mend(0);           // 治疗第 0 号肢体

// 操作世界
WorldUtil.PlaceBlock("marble", 10, 5);

// 操作玩家
PlayerUtil.Teleport(100, 200);
PlayerUtil.Alert("你有一封新邮件", true);
```

所有方法空值安全：如果游戏状态不满足（比如世界没生成），方法会静默返回或记录警告日志，不会抛异常让游戏崩掉。

## 写 Harmony Patch

Bark 本身就是一个 Harmony 模组，`Plugin.Awake()` 里已经执行了 `_harmony.PatchAll()`，所以你的程序集里所有 `[HarmonyPatch]` 都会自动生效。

```csharp
using HarmonyLib;

[HarmonyPatch(typeof(Body), nameof(Body.Jump))]
public static class JumpPatch
{
    // 前缀：在 Body.Jump() 执行前触发
    public static void Prefix(Body __instance)
    {
        Logger.LogInfo($"角色 {__instance.name} 即将跳跃");
    }

    // 后缀：在 Body.Jump() 执行后触发
    public static void Postfix(Body __instance)
    {
        Logger.LogInfo($"角色 {__instance.name} 跳跃完成");
    }
}
```

> 💡 Harmony Patch 配合 `EventUtil.Trigger()` 使用是推荐模式——在 Patch 里发事件，让其他模组通过事件系统响应，而不是都在同一个 Patch 里塞逻辑。

## 注册为脚本 API（高级）

如果你的 C# 模组想把自己的工具方法暴露给 JS/Lua 脚本调用，用 `[ScriptMethod]` 标记方法，然后在 `Awake()` 里注册：

```csharp
using Bark.ScriptApi;

public static class MyTool
{
    [ScriptMethod]
    public static int Double(int value) => value * 2;

    [ScriptMethod]
    public static string Greet(string name) => $"你好, {name}!";
}

// 在 Awake() 里注册
ApiRegistry.Register(typeof(MyTool));
```

注册后，脚本侧可以通过小驼峰命名的全局变量访问：`myTool.Double(5)`。

可选参数会自动生成重载（和 Bark 内置 API 行为一致）：

```csharp
[ScriptMethod]
public static void Alert(string text, bool important = false, float delay = 0f)
{
    // 脚本可以写 myTool.Alert("hello")、myTool.Alert("hello", true)、myTool.Alert("hello", true, 0.5)
}
```

## 触发自定义事件

```csharp
using Bark.Tool;

// 触发一个 Bark 事件，所有订阅者都会收到
EventUtil.Trigger<PlayerJumpStartEvent>();

// 或者用实例
var evt = new PlayerJumpStartEvent { Body = someBody, Camera = someCamera };
EventUtil.Trigger(evt);
```

详细 API 见 [EventUtil](csharp-api/event-util.md)。

## 版本检查

```csharp
using Bark.Tool;

// 在 Awake() 末尾调用，BepInEx 控制台会输出是否有新版本
UpdateUtil.Check("你的GitHub用户名/仓库名", "模组名", "当前版本", Logger);
```

详细 API 见 [UpdateUtil](csharp-api/update.md)。
