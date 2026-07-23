using System;
using System.IO;
using System.Text;
using Bark.ScriptApi;
using Puerts;

namespace Bark.Script;

// PuerTS Lua 引擎包装器，管理脚本模组的生命周期
// 不依赖 Unity GameObject，避免场景切换时被意外销毁
public class PuerLua : ScriptEngine
{
    private bool _isLoaded;
    private ScriptEnv? _scriptEnv;

    // 加载并执行 Lua 模组，返回是否成功
    public override bool Load(ScriptManifest manifest)
    {
        base.Load(manifest);

        try
        {
            // 创建 Lua 引擎实例
            _scriptEnv = new ScriptEnv(new BackendLua());

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
            Plugin.Logger.LogWarning($"[Bark] Lua Load FAILED | id={Manifest.Id} | {ex}");
            Dispose();
            return false;
        }

        return true;
    }

    // 注入 API 到 Lua 全局作用域（无 bark. 前缀，平铺注册）
    //   bodyUtil = ApiRegistry.GetProxy('bodyUtil')
    //   playerUtil = ApiRegistry.GetProxy('playerUtil')
    //   ...
    //   log = LogApi(id, logsDir, name)
    //   locale = log.Locale
    //   scriptInfo = { Id = ..., Version = ..., Name = ... }
    private void InjectBarkApi()
    {
        if (_scriptEnv == null) return;

        var id = EscapeString(Manifest.Id);
        var version = EscapeString(Manifest.Version);
        var scriptName = EscapeString(Manifest.Name);
        var logsDir = EscapeString(LogsDir);

        var sb = new StringBuilder();
        sb.AppendLine("local CS = require('csharp')");

        // AutoApi 生成的代理：类型名 camelCase 作为全局变量
        foreach (var (name, _) in ApiRegistry.Proxies)
        {
            sb.AppendLine($"{name} = CS.Bark.ScriptApi.ApiRegistry.GetProxy('{name}')");
        }

        // 特殊 API：Log / Locale / ScriptInfo
        sb.AppendLine($"local _logApi = CS.Bark.ScriptApi.LogApi('{scriptName}', '{logsDir}', '{id}')");
        sb.AppendLine("log = _logApi");
        sb.AppendLine("locale = _logApi.Locale");
        sb.AppendLine($"scriptInfo = {{ Id = '{id}', Version = '{version}', Name = '{scriptName}' }}");

        _scriptEnv.Eval(sb.ToString());
    }

    // 调用生命周期钩子
    private void CallLifecycleHook(string hookName)
    {
        if (_scriptEnv == null) return;

        try
        {
            _scriptEnv.Eval($"if type({hookName}) == 'function' then {hookName}() end");
        }
        catch
        {
            // ignored
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
            _scriptEnv.Eval($"if type({eventName}) == 'function' then {eventName}() end");
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
                Plugin.Logger.LogWarning($"[Bark] Lua Dispose error | {ex.Message}");
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