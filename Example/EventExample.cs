namespace Bark.Example;

// C# 模组事件订阅示例：类标记 [EventBusSubscriber]，方法参数为 BarkEvent 子类即可自动注册
// [EventBusSubscriber(Plugin.Guid)]
// public class EventExample
// {
//     public static void OnPlayerJumpStart(PlayerJumpStartEvent eve)
//     {
//         Plugin.Logger.LogInfo("[EventExample] JumpStart!");
//     }
//
//     public static void OnPlayerJumpOver(PlayerJumpOverEvent eve)
//     {
//         Plugin.Logger.LogInfo("[EventExample] JumpOver!");
//     }
// }