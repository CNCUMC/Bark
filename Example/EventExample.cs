namespace Bark.Example;

// C# 模组事件订阅示例（使用 EventBusSubscriber + SubscribeEvent 属性模式）
// [EventBusSubscriber(Plugin.Guid)]
// public class EventExample
// {
//     [SubscribeEvent]
//     public static void OnPlayerJumpStart(PlayerEvents.JumpStartEvent eve)
//     {
//         // 处理跳跃起跳事件
//     }
//
//     [SubscribeEvent]
//     public static void OnPlayerJumpOver(PlayerEvents.JumpOverEvent eve)
//     {
//         // 处理跳跃落地事件
//     }
// }