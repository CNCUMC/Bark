namespace Bark.ScriptAPI;

// 脚本模组主 API 入口（bark.* 全局对象）
// 通过 loadType('Bark.ScriptAPI.BarkScriptAPI') 在脚本中实例化
public class ScriptApi(string id, string version, string name)
{
    public LogApi Log { get; } = new(name);
    public ScriptInfo ScriptInfo { get; } = new() { Id = id, Version = version, Name = name };
}
