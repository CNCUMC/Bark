using System;
using JetBrains.Annotations;

namespace Bark.Event;

[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
[AttributeUsage(AttributeTargets.Class)]
public class EventBusSubscriberAttribute(string guid) : Attribute
{
    // 插件 GUID（用于标识模组来源）
    public string Guid { get; } = guid;
}