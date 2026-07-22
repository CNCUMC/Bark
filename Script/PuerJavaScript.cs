using System;
using Puerts;
using UnityEngine;

namespace Bark.Script;

// PuerTS JavaScript 引擎包装器，管理脚本模组的生命周期
public class PuerJavaScript : MonoBehaviour
{
    private ScriptEnv? _scriptEnv;
    private ScriptManifest _manifest = null!;
    private bool _isLoaded;

    // 加载并执行 JS 模组，返回是否成功
    public bool Load(ScriptManifest manifest)
    {
        _manifest = manifest;

        try
        {
            // 创建 V8 引擎实例
            _scriptEnv = new ScriptEnv(new BackendV8());

            // 注入 bark.* API
            InjectBarkApi();

            // 执行入口脚本
            var script = System.IO.File.ReadAllText(manifest.EntryFile);
            _scriptEnv.Eval(script);

            _isLoaded = true;

            // 调用 onLoad 生命周期钩子
            CallLifecycleHook("onLoad");
        }
        catch (Exception ex)
        {
            Cleanup();
            return false;
        }
        return true;
    }

    // 注入 bark.* API 到 JS 环境
    private void InjectBarkApi()
    {
        if (_scriptEnv == null) return;

        // 注入 bark.* API（日志输出到控制台）
        _scriptEnv.Eval("""
            var bark = {
                log: function(msg) { console.log('[Bark] ' + String(msg)); },
                logWarn: function(msg) { console.warn('[Bark] ' + String(msg)); },
                logError: function(msg) { console.error('[Bark] ' + String(msg)); },
                mod: {
                    id: '',
                    version: '',
                    name: ''
                }
            };
        """);

        // 设置模组元数据
        _scriptEnv.Eval($"bark.mod.id = '{EscapeString(_manifest.Id)}'");
        _scriptEnv.Eval($"bark.mod.version = '{EscapeString(_manifest.Version)}'");
        _scriptEnv.Eval($"bark.mod.name = '{EscapeString(_manifest.Name)}'");
    }

    // 调用生命周期钩子
    private void CallLifecycleHook(string hookName)
    {
        if (_scriptEnv == null) return;

        try
        {
            _scriptEnv.Eval($$"""
                if (typeof {{hookName}} === 'function') {
                    {{hookName}}();
                }
            """);
        }
        catch (Exception ex)
        {
            // 静默忽略钩子执行异常
        }
    }

    // 激活模组（调用 onEnable）
    public void Enable()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("onEnable");
    }

    // 停用模组（调用 onDisable）
    public void Disable()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("onDisable");
    }

    // 卸载模组（调用 onUnload）
    public void Unload()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("onUnload");
        Cleanup();
    }

    // 清理资源
    private void Cleanup()
    {
        if (_scriptEnv != null)
        {
            try { _scriptEnv.Dispose(); }
            catch { /* 静默忽略清理异常 */ }
            _scriptEnv = null;
        }
        _isLoaded = false;
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    // 转义字符串中的特殊字符（用于 PuerTS Eval 注入）
    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }
}
