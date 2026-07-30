using System;
using JetBrains.Annotations;

namespace Bark.ScriptApi;

// 标记一个静态类为脚本 API，ScanAndRegister() 自动注册到 ApiRegistry
// Name 为脚本侧全局变量名（默认取类名去掉 "Util" 后缀，如 BodyUtil → "Body"）
// [MeansImplicitUse] 告知 IDE 被标记的类通过反射使用，不报"未使用"警告
[MeansImplicitUse(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.WithMembers)]
[AttributeUsage(AttributeTargets.Class)]
public class ScriptApiAttribute : Attribute
{
    // 脚本侧全局变量名，null 则自动推导（去 "Util" 后缀）
    public string? Name { get; set; }
}
