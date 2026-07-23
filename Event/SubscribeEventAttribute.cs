using System;

namespace Bark.Event;

// 标记一个方法为事件处理器
// 方法必须是 public static，参数为具体的 BarkEvent 子类
[AttributeUsage(AttributeTargets.Method)]
public class SubscribeEventAttribute : Attribute
{
}