namespace Bark.Example;

// C# 模组事件订阅示例：类标记 [EventBusSubscriber]，方法参数为 BarkEvent 子类即可自动注册
// [EventBusSubscriber(Plugin.Guid)]
// public class EventExample
// {
//     public static void OnPlayerJumpStart(PlayerEvents.JumpStartEvent eve)
//     {
//         // 处理跳跃起跳事件
//     }
//
//     public static void OnPlayerJumpOver(PlayerEvents.JumpOverEvent eve)
//     {
//         // 处理跳跃落地事件
//     }
// }