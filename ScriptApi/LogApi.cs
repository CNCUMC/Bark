using Bark.Tool;

namespace Bark.ScriptApi;

// 脚本侧 Log 全局变量：封装 LogUtil，自动添加 [模组名] 前缀
public class LogApi(string modName, string modId)
{
    // 本地化访问器
    public LocaleApi Locale { get; } = new(modId, modName);

    public void NewLine()
    {
        LogUtil.NewLine();
    }

    public void Divider(char divider = '-', int length = 27)
    {
        LogUtil.Divider(divider, length);
    }

    public void Info(string msg)
    {
        LogUtil.Info(Format(msg), Plugin.Logger);
    }

    public void Error(string msg)
    {
        LogUtil.Error(Format(msg), Plugin.Logger);
    }

    public void Warning(string msg)
    {
        LogUtil.Warning(Format(msg), Plugin.Logger);
    }

    public void Debug(string msg)
    {
        LogUtil.Debug(Format(msg), Plugin.Logger);
    }

    public void Message(string msg)
    {
        LogUtil.Message(Format(msg), Plugin.Logger);
    }

    // 便捷方法：日志 + 本地化一步完成
    public void InfoF(string key, params object[] args)
    {
        Info(Locale.GetFormatted(key, args));
    }

    public void ErrorF(string key, params object[] args)
    {
        Error(Locale.GetFormatted(key, args));
    }

    public void WarningF(string key, params object[] args)
    {
        Warning(Locale.GetFormatted(key, args));
    }

    public void DebugF(string key, params object[] args)
    {
        Debug(Locale.GetFormatted(key, args));
    }

    public void MessageF(string key, params object[] args)
    {
        Message(Locale.GetFormatted(key, args));
    }

    private string Format(string msg)
    {
        return $"[{modName}] {msg}";
    }
}