using System;

namespace Bark.ScriptApi;

// 脚本 API 聚合入口（保留兼容性）
// 当前通过 ApiRegistry + 平铺注入，不再使用此类聚合
// 特殊 API（Log/Locale/ScriptInfo）由脚本引擎直接注入全局作用域
[Obsolete("使用 ApiRegistry 平铺注入，ScriptApi 聚合类已废弃")]
public class ScriptApi(string id, string version, string name)
{
    public LogApi Log { get; } = new(name, AppDomain.CurrentDomain.BaseDirectory, id);
    public LocaleApi Locale { get; } = new(id, name);
    public ScriptInfo ScriptInfo { get; } = new() { Id = id, Version = version, Name = name };
}
