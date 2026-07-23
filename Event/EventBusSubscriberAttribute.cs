using System;

namespace Bark.Event;

// 标记一个类为事件订阅者
// 扫描时自动发现标注了此特性的类中参数为 BarkEvent 子类的 public static 方法
[AttributeUsage(AttributeTargets.Class)]
public class EventBusSubscriberAttribute(string guid) : Attribute
{
    // 插件 GUID（用于标识模组来源）
    public string Guid { get; } = guid;
}