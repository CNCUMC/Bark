using System;
using System.IO;
using Bark.Tool;
using Puerts;

namespace Bark.Script;

// PuerTS JavaScript 引擎包装器，管理脚本模组的生命周期
// 不依赖 Unity GameObject，避免场景切换时被意外销毁
public class PuerJavaScript : ScriptEngine
{
    private bool _isLoaded;
    private ScriptManifest _manifest = null!;
    private ScriptEnv? _scriptEnv;

    // 加载并执行 JS 模组，返回是否成功
    public override bool Load(ScriptManifest manifest)
    {
        _manifest = manifest;

        try
        {
            // 创建 V8 引擎实例
            _scriptEnv = new ScriptEnv(new BackendV8());

            // 注入 bark.* API
            InjectBarkApi();

            // 执行入口脚本
            var script = File.ReadAllText(manifest.EntryFile);
            _scriptEnv.Eval(script);

            _isLoaded = true;

            // 调用 onLoad 生命周期钩子
            CallLifecycleHook("onLoad");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"[Bark] JS Load FAILED | id={manifest.Id} | {ex}");
            Dispose();
            return false;
        }

        return true;
    }

    // 注入 bark.* API 到 JS 环境
    private void InjectBarkApi()
    {
        if (_scriptEnv == null) return;

        var id = EscapeString(_manifest.Id);
        var version = EscapeString(_manifest.Version);
        var scriptName = EscapeString(_manifest.Name);

        _scriptEnv.Eval($"var bark = new CS.Bark.ScriptApi.ScriptApi('{id}', '{version}', '{scriptName}');");
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
            LogUtil.Warning("script_mod_loader.hook_failed", _manifest.Id, hookName, ex.Message);
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

    // 向脚本侧发送事件通知：调用全局生命周期函数（如 onPlayerJumpStart）
    public override void CallTriggerEvent(string eventName)
    {
        if (_scriptEnv == null) return;

        var hookName = EventToHookName(eventName);

        try
        {
            _scriptEnv.Eval($"if (typeof {hookName} === 'function') {{ {hookName}(); }}");
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