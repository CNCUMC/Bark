using System;
using JetBrains.Annotations;

namespace Bark.ScriptApi;

[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
[AttributeUsage(AttributeTargets.Class)]
public class ScriptApiAttribute : Attribute
{
    public string? Name { get; set; }
}
