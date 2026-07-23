using System;

namespace Bark.ScriptApi;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ScriptMethodAttribute : Attribute
{
    public string? Name { get; set; }
}
