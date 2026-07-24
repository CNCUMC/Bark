# EventUtil

EventUtil 是 C# 侧的事件发射和订阅工具，所有 Bark 模组（包括 Bark 自身）都用它来触发事件和注册监听。

> ℹ️ 这是 C# 专用 API。脚本侧用 [事件钩子](../script-events.md) 接收事件。

## 触发事件

两种方式：泛型触发（无参事件）或实例触发（带数据的事件）。

```csharp
using Bark.Tool;
using Bark.Events;

// 无参事件：泛型触发
EventUtil.Trigger<PlayerJumpStartEvent>();

// 带数据的事件：先构造实例
var evt = new LimbBrokenEvent
{
    LimbIndex = 0,
    LimbName = "左脚"
};
EventUtil.Trigger(evt);
```

`Trigger(new T())` 内部会自动设 `Time` 为当前游戏时间。

## 注册监听

`On<T>` 用委托注册回调。需要传入你的模组 GUID 用于后续注销。

```csharp
using Bark.Tool;
using Bark.Events;

// 在 Awake() 里注册
EventUtil.On<PlayerDeathEvent>(evt =>
{
    Logger.LogWarning($"玩家死亡，时间: {evt.Time}");
    // 你的模组逻辑
}, "org.example.my_mod");
```

同一个 GUID 下可以注册多个不同类型事件的监听。

## 注销

模组卸载时清理所有监听，防止内存泄漏：

```csharp
// 在 OnDestroy() 里调用
EventUtil.UnregisterAll("org.example.my_mod");
```

## 自定义事件

你可以定义自己的 `BarkEvent` 子类，然后在代码里用 `EventUtil.Trigger` 发射。

```csharp
using Bark.Event;  // BarkEvent 基类

// 定义
public class MyCustomEvent : BarkEvent
{
    public string Message;
    public int Value;
}

// 发射
EventUtil.Trigger(new MyCustomEvent { Message = "hello", Value = 42 });

// 在另一个模组里监听
EventUtil.On<MyCustomEvent>(evt =>
{
    Logger.LogInfo($"收到自定义事件: {evt.Message}, {evt.Value}");
}, "org.example.another_mod");
```

如果你想让自定义事件也能被脚本模组接收，加上 `[ScriptEvent("hookName")]` 属性。详见 [事件系统](../csharp-events.md)。
