using Bark.Tool;

namespace Bark.ScriptAPI;

// 脚本模组日志 API（通过 bark.log 访问）
public class LogApi(string modName)
{
    // 输出普通日志
    public void Info(string msg)
    {
        LogUtil.Info(Log(modName, msg), Plugin.Logger);
    }
    
    // 输出信息日志
    public void Message(string msg)
    {
        LogUtil.Message(Log(modName, msg), Plugin.Logger);
    }

    // 输出警告日志
    public void Warn(string msg)
    {
        LogUtil.Warning(Log(modName, msg), Plugin.Logger);
    }

    // 输出错误日志
    public void Error(string msg)
    {
        LogUtil.Error(Log(modName, msg), Plugin.Logger);
    }

    private static string Log(string modName, string msg)
    {
        return $"[{modName}] {msg}";
    }
}
