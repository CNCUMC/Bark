using System;
using System.IO;
using System.Text;
using Bark.ScriptApi;
using Bark.Tool;
using Puerts;

namespace Bark.Script;

// PuerTS JavaScript 引擎包装器，管理脚本模组的生命周期
// 不依赖 Unity GameObject，避免场景切换时被意外销毁
public class PuerJavaScript : ScriptEngine
{
    private bool _isLoaded;
    private ScriptEnv? _scriptEnv;

    // 加载并执行 JS 模组，返回是否成功
    public override bool Load(ScriptManifest manifest)
    {
        base.Load(manifest);

        try
        {
            // 创建 V8 引擎实例
            _scriptEnv = new ScriptEnv(new BackendV8());

            // 注入 API 到全局作用域（无 bark. 前缀）
            InjectBarkApi();

            // 执行入口脚本
            var script = File.ReadAllText(Manifest.EntryFile);
            _scriptEnv.Eval(script);

            _isLoaded = true;

            // 调用 onLoad 生命周期钩子
            CallLifecycleHook("onLoad");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"[Bark] JS Load FAILED | id={Manifest.Id} | {ex}");
            Dispose();
            return false;
        }

        return true;
    }

    private void InjectBarkApi()
    {
        if (_scriptEnv == null) return;

        var id = EscapeString(Manifest.Id);
        var version = EscapeString(Manifest.Version);
        var scriptName = EscapeString(Manifest.Name);
        var logsDir = EscapeString(LogsDir);

        var sb = new StringBuilder();

        // AutoApi 生成的代理
        foreach (var (name, _) in ApiRegistry.Proxies)
        {
            sb.AppendLine($"var {name} = CS.Bark.ScriptApi.ApiRegistry.GetProxy('{name}');");
        }

        // 特殊 API
        sb.AppendLine($"var logApi = new CS.Bark.ScriptApi.LogApi('{scriptName}', '{logsDir}', '{id}');");
        sb.AppendLine("var log = logApi;");
        sb.AppendLine("var locale = logApi.Locale;");
        sb.AppendLine($"var scriptInfo = {{ Id: '{id}', Version: '{version}', Name: '{scriptName}' }};");

        _scriptEnv.Eval(sb.ToString());
    }

    // 调用生命周期钩子
    private void CallLifecycleHook(string hookName)
    {
        if (_scriptEnv == null) return;

        try
        {
            _scriptEnv.Eval($"if (typeof {hookName} === 'function') {{ {hookName}(); }}");
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_mod_loader.hook_failed", Manifest.Id, hookName, ex.Message);
        }
    }

    // 激活模组（调用 onEnable）
    public override void Enable()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("onEnable");
    }

    // 停用模组（调用 onDisable）
    public override void Disable()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("onDisable");
    }

    // 卸载模组（调用 onUnload）
    public override void Unload()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("onUnload");
        Dispose();
    }

    // 向脚本侧发送事件通知：调用全局钩子函数（如 onPlayerJumpStart）
    public override void CallTriggerEvent(string eventName)
    {
        if (_scriptEnv == null) return;

        try
        {
            _scriptEnv.Eval($"if (typeof {eventName} === 'function') {{ {eventName}(); }}");
        }
        catch
        {
            // ignored
        }
    }

    // 释放引擎资源
    public override void Dispose()
    {
        if (_scriptEnv != null)
        {
            try
            {
                _scriptEnv.Dispose();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogWarning($"[Bark] JS Dispose error | {ex.Message}");
            }

            _scriptEnv = null;
        }

        _isLoaded = false;
    }

    // 转义字符串中的特殊字符（用于 PuerTS Eval 注入）
    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", @"\\")
            .Replace("'", @"\'")
            .Replace("\n", @"\n")
            .Replace("\r", @"\r");
    }
}