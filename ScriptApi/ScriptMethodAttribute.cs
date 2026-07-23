using System;

namespace Bark.ScriptApi;

// 标记静态方法，使其自动暴露为脚本 API（无需手写包装类）
// Name 为空时使用原方法名
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ScriptMethodAttribute : Attribute
{
    // 覆盖 Lua 侧的方法名；null 时使用 C# 方法名
    public string? Name { get; set; }
}
