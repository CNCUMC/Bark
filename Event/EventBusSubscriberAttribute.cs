using System;
using JetBrains.Annotations;

namespace Bark.Event;

[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
[AttributeUsage(AttributeTargets.Class)]
public class EventBusSubscriberAttribute(string guid) : Attribute
{
    public string Guid { get; } = guid;
}
