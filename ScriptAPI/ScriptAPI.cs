namespace Bark.ScriptAPI;

// 脚本模组主 API 入口（bark.* 全局对象）
// 通过 loadType('Bark.ScriptAPI.BarkScriptAPI') 在脚本中实例化
public class ScriptAPI(string id, string version, string name)
{
    // 日志 API — bark.log.Message() / bark.log.Warn() / bark.log.Error()
    public LogAPI Log { get; } = new(name);

    // 模组元数据 — bark.mod.Id / bark.mod.Version / bark.mod.Name
    public ScriptInfo ScriptInfo { get; } = new() { Id = id, Version = version, Name = name };
}
