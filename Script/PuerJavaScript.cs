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

        // 使用 JS Proxy 包装 C# bark 对象，拦截 events.on 存入原生 JS 数组
        // 避免 Puerts C# 对象无法挂载新属性、无法可靠覆盖方法的问题
        var jsCode = """

                     var _bark = new CS.Bark.ScriptApi.ScriptApi('__ID__', '__VERSION__', '__NAME__');
                     var __barkEventCallbacks = {};
                     var bark = new Proxy(_bark, {
                         get: function(target, prop) {
                             if (prop === 'events') {
                                 return {
                                     on: function(name, cb) {
                                         if (!__barkEventCallbacks[name]) __barkEventCallbacks[name] = [];
                                         __barkEventCallbacks[name].push(cb);
                                     }
                                 };
                             }
                             return target[prop];
                         }
                     });
                     function __barkTriggerEvent(name) {
                         var cbs = __barkEventCallbacks[name];
                         if (!cbs || cbs.length === 0) return;
                         for (var i = 0; i < cbs.length; i++) {
                             try { cbs[i](); } catch(e) { console.log('[Bark] event cb error: ' + name + ' ' + e); }
                         }
                     }

                     """.Replace("__ID__", id).Replace("__VERSION__", version).Replace("__NAME__", scriptName);

        _scriptEnv.Eval(jsCode);
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

    // 世界生成完成钩子
    public override void CallWorldGenerated()
    {
        if (_scriptEnv == null)
        {
            Plugin.Logger.LogWarning($"[Bark] JS CallWorldGenerated SKIP | _scriptEnv is null | id={_manifest.Id}");
            return;
        }

        try
        {
            _scriptEnv.Eval("""

                                            if (typeof onWorldGenerated === 'function') onWorldGenerated();
                                            if (typeof __barkTriggerEvent === 'function') __barkTriggerEvent('world_generated');
                                        
                            """);
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"[Bark] JS CallWorldGenerated FAILED | id={_manifest.Id} | {ex.Message}");
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