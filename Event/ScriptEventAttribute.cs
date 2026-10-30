using System;
using JetBrains.Annotations;

namespace Bark.Event;

[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class)]
public class ScriptEventAttribute(string hookName) : Attribute
{
    public string HookName { get; } = hookName;
}
