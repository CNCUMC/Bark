using System;
using Bark.Tool;
using Puerts;
using UnityEngine;

namespace Bark.ScriptMod;

// PuerTS Python 引擎包装器，管理脚本模组的生命周期
public class PuerPython : MonoBehaviour
{
    private ScriptEnv? _scriptEnv;
    private ModManifest _manifest = null!;
    private bool _isLoaded;

    // 加载并执行 Python 模组
    public void Load(ModManifest manifest)
    {
        _manifest = manifest;

        try
        {
            // 创建 Python 引擎实例
            _scriptEnv = new ScriptEnv(new BackendV8());

            // 注入 bark.* API
            InjectBarkApi();

            // 执行入口脚本
            var script = System.IO.File.ReadAllText(manifest.EntryFile);
            _scriptEnv.Eval(script);

            _isLoaded = true;

            // 调用 onLoad 生命周期钩子
            CallLifecycleHook("on_load");

            LogUtil.Info("scriptmod.python_loaded", manifest.Id);
        }
        catch (Exception ex)
        {
            LogUtil.Warning("scriptmod.python_load_failed", manifest.Id, ex.Message);
            Cleanup();
        }
    }

    // 注入 bark.* API 到 Python 环境
    private void InjectBarkApi()
    {
        if (_scriptEnv == null) return;

        // 注入 bark.log
        _scriptEnv.Eval("""
            class BarkLogger:
                @staticmethod
                def log(msg):
                    print(f'[Bark] {msg}')
                @staticmethod
                def log_warn(msg):
                    print(f'[Bark WARN] {msg}')
                @staticmethod
                def log_error(msg):
                    print(f'[Bark ERROR] {msg}')

            class BarkMod:
                def __init__(self):
                    self.id = ''
                    self.version = ''
                    self.name = ''

            bark = type('bark', (), {
                'log': BarkLogger.log,
                'logWarn': BarkLogger.log_warn,
                'logError': BarkLogger.log_error,
                'mod': BarkMod()
            })()
        """);

        // 设置模组元数据
        _scriptEnv.Eval($"bark.mod.id = '{EscapeJs(_manifest.Id)}'");
        _scriptEnv.Eval($"bark.mod.version = '{EscapeJs(_manifest.Version)}'");
        _scriptEnv.Eval($"bark.mod.name = '{EscapeJs(_manifest.Name)}'");
    }

    // 调用生命周期钩子
    private void CallLifecycleHook(string hookName)
    {
        if (_scriptEnv == null) return;

        try
        {
            _scriptEnv.Eval($$"""
                if callable({{hookName}}):
                    {{hookName}}()
            """);
        }
        catch (Exception ex)
        {
            LogUtil.Warning("scriptmod.hook_failed", _manifest.Id, hookName, ex.Message);
        }
    }

    // 激活模组（调用 on_enable）
    public void Enable()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("on_enable");
    }

    // 停用模组（调用 on_disable）
    public void Disable()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("on_disable");
    }

    // 卸载模组（调用 on_unload）
    public void Unload()
    {
        if (!_isLoaded) return;
        CallLifecycleHook("on_unload");
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

    // 转义 JS 字符串中的特殊字符
    private static string EscapeJs(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }
}
