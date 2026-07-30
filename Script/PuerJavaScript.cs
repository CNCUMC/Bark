using System;
using System.IO;
using System.Text;
using Bark.Event;
using Bark.Items;
using Bark.ScriptApi;
using Bark.Tile;
using Bark.Tool;
using Puerts;

namespace Bark.Script;

// PuerTS JavaScript 引擎包装器，管理脚本模组的生命周期
// 不依赖 Unity GameObject，避免场景切换时被意外销毁
public class PuerJavaScript : ScriptEngine
{
    private bool _isLoaded;
    private ScriptEnv? _scriptEnv;

    // 加载并执行 JS 脚本，返回是否成功
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
            LogUtil.Warning("script_engine.js_load_failed", Manifest.Id, ex.ToString());
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

        var sb = new StringBuilder();

        // AutoApi 生成的代理
        foreach (var (name, _) in ApiRegistry.Proxies)
            sb.AppendLine($"var {name} = CS.Bark.ScriptApi.ApiRegistry.GetProxy('{name}');");

        // 特殊 API
        sb.AppendLine($"var logApi = new CS.Bark.ScriptApi.LogApi('{scriptName}', '{id}');");
        sb.AppendLine("var Log = logApi;");
        sb.AppendLine("var Locale = logApi.Locale;");
        sb.AppendLine($"var ScriptInfo = {{ Id: '{id}', Version: '{version}', Name: '{scriptName}' }};");

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

    // 向脚本侧发送事件通知：调用全局钩子函数（如 onPlayerJumpStart），
    // 传入事件数据供脚本侧 onItemUse(event) 等访问 event.ItemId / event.Item
    public override void CallTriggerEvent(string eventName, BarkEvent? eventData = null)
    {
        if (_scriptEnv == null) return;

        try
        {
            // 注入事件数据，供脚本侧通过 __barkEvent 或传参访问
            if (eventData != null)
            {
                EventScriptContext.CurrentEvent = eventData;
                _scriptEnv.Eval("var __barkEvent = CS.Bark.Script.EventScriptContext.CurrentEvent;");
            }
            else
            {
                _scriptEnv.Eval("var __barkEvent = null;");
            }

            _scriptEnv.Eval(
                $"if (typeof {eventName} === 'function') {{ {eventName}(__barkEvent); }}");
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_mod_loader.hook_failed", Manifest.Id, eventName, ex.Message);
        }
        finally
        {
            EventScriptContext.CurrentEvent = null;
        }
    }

    // 执行单个物品脚本文件，执行前注入上下文全局变量，
    // 脚本可定义 function main(itemId, item, action) 接收参数
    public override void ExecuteItemFile(string filePath, string? itemId, Item? item = null, string? action = null)
    {
        if (_scriptEnv == null || !File.Exists(filePath)) return;

        // 暂存上下文供 JS 侧通过 CS.Bark.Items.ItemScriptContext 访问
        ItemScriptContext.CurrentItem = item;
        ItemScriptContext.CurrentAction = action;

        try
        {
            // 注入上下文全局变量（兼容旧 __barkItemId 写法）
            var escapedId = itemId != null ? EscapeString(itemId) : "null";
            _scriptEnv.Eval($"var __barkItemId = '{escapedId}';");
            _scriptEnv.Eval("var __barkItem = CS.Bark.Items.ItemScriptContext.CurrentItem;");
            _scriptEnv.Eval("var __barkAction = CS.Bark.Items.ItemScriptContext.CurrentAction;");

            // 执行脚本文件（注册 main 函数等定义）
            var script = File.ReadAllText(filePath);
            _scriptEnv.Eval(script);

            // 调用 main(itemId, item, action) — JS 自动忽略多余/缺失参数
            _scriptEnv.Eval(
                "if (typeof main === 'function') { main(__barkItemId, __barkItem, __barkAction); }");
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_engine.js_exec_file_failed", Manifest.Id, filePath, ex.Message);
        }
        finally
        {
            ItemScriptContext.CurrentItem = null;
            ItemScriptContext.CurrentAction = null;
        }
    }

    // 执行单个物块脚本文件，注入 tileId / tileContext / action 到脚本全局
    public override void ExecuteTileFile(string filePath, string? tileId, TileScriptContext? context = null,
        string? action = null)
    {
        if (_scriptEnv == null || !File.Exists(filePath)) return;

        // 暂存上下文供 JS 侧通过 CS.Bark.Tile.TileScriptContext 访问
        TileScriptContext.CurrentContext = context;
        TileScriptContext.CurrentAction = action;

        try
        {
            var escapedId = tileId != null ? EscapeString(tileId) : "null";
            _scriptEnv.Eval($"var __barkTileId = '{escapedId}';");
            _scriptEnv.Eval("var __barkTileContext = CS.Bark.Tile.TileScriptContext.CurrentContext;");
            _scriptEnv.Eval("var __barkAction = CS.Bark.Tile.TileScriptContext.CurrentAction;");

            var script = File.ReadAllText(filePath);
            _scriptEnv.Eval(script);

            _scriptEnv.Eval(
                "if (typeof main === 'function') { main(__barkTileId, __barkTileContext, __barkAction); }");
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_engine.js_exec_file_failed", Manifest.Id, filePath, ex.Message);
        }
        finally
        {
            TileScriptContext.CurrentContext = null;
            TileScriptContext.CurrentAction = null;
        }
    }

    // 每帧调用脚本侧的 onUpdate() 函数（静默，不记录错误日志避免刷屏）
    public override void CallUpdate()
    {
        if (_scriptEnv == null || !_isLoaded) return;

        try
        {
            _scriptEnv.Eval("if (typeof onUpdate === 'function') { onUpdate(); }");
        }
        catch
        {
            // 静默跳过：onUpdate 无需日志，避免每帧刷屏
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
                LogUtil.Warning("script_engine.js_dispose_error", Manifest.Id, ex.Message);
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