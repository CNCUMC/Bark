using System;
using JetBrains.Annotations;

namespace Bark.Event;

// 标记一个 BarkEvent 子类为脚本可触发事件
// HookName 直接对应 Lua/JS 侧的钩子函数名（如 "onPlayerJumpStart"）
// [MeansImplicitUse] 告诉 Rider/ReSharper：被标记的事件类通过反射使用，不报"未使用"警告
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[AttributeUsage(AttributeTargets.Class)]
public class ScriptEventAttribute(string hookName) : Attribute
{
    // 脚本侧钩子函数名（camelCase）
    public string HookName { get; } = hookName;
}