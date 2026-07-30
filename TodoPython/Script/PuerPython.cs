// using System;
// using System.IO;
// using System.Text;
// using Bark.Event;
// using Bark.Items;
// using Bark.ScriptApi;
// using Bark.Tool;
// using Puerts;
//
// namespace Bark.Script;
//
// // PuerTS Python 引擎包装器，管理脚本模组的生命周期。
// // 不依赖 Unity GameObject，避免场景切换时被意外销毁。
// // Python 后端由 Puerts.Python.Complete NuGet 包提供（pythonnet 嵌入式运行时），
// // 并非系统安装的 CPython。部分标准库模块（如 inspect）可能不可用。
// // Python 侧通过 PuerTS 扩展的 import 语法访问 C# 类型，或通过 puerts.load_type() 动态加载。
// public class PuerPython : ScriptEngine
// {
//     private bool _isLoaded;
//     private ScriptEnv? _scriptEnv;
//
//     // 加载并执行 Python 脚本，返回是否成功
//     public override bool Load(ScriptManifest manifest)
//     {
//         base.Load(manifest);
//
//         try
//         {
//             // 创建 Python 引擎实例
//             _scriptEnv = new ScriptEnv(new BackendPython());
//
//             // 注入 API 到全局作用域（无 bark. 前缀）
//             InjectBarkApi();
//
//             // 执行入口脚本
//             var script = File.ReadAllText(Manifest.EntryFile);
//             _scriptEnv.Eval(WrapExec(script));
//
//             _isLoaded = true;
//
//             // 调用 onLoad 生命周期钩子
//             CallLifecycleHook("onLoad");
//         }
//         catch (Exception ex)
//         {
//             LogUtil.Warning("script_engine.py_load_failed", Manifest.Id, ex.ToString());
//             Dispose();
//             return false;
//         }
//
//         return true;
//     }
//
//     private void InjectBarkApi()
//     {
//         if (_scriptEnv == null) return;
//
//         var id = EscapeString(Manifest.Id);
//         var version = EscapeString(Manifest.Version);
//         var scriptName = EscapeString(Manifest.Name);
//
//         var sb = new StringBuilder();
//
//         // AutoApi 生成的代理：通过 puerts.load_type 动态加载 ApiRegistry 获取代理
//         sb.AppendLine("_api_registry = puerts.load_type('Bark.ScriptApi.ApiRegistry')");
//         foreach (var (name, _) in ApiRegistry.Proxies)
//             sb.AppendLine($"{name} = _api_registry.GetProxy('{name}')");
//
//         // 特殊 API：Log / Locale / ScriptInfo
//         sb.AppendLine(
//             $"_log_api = puerts.load_type('Bark.ScriptApi.LogApi')('{scriptName}', '{id}')");
//         sb.AppendLine("Log = _log_api");
//         sb.AppendLine("Locale = _log_api.Locale");
//         sb.AppendLine(
//             $"ScriptInfo = {{'Id': '{id}', 'Version': '{version}', 'Name': '{scriptName}'}}");
//
//         _scriptEnv.Eval(WrapExec(sb.ToString()));
//     }
//
//     // 调用生命周期钩子
//     private void CallLifecycleHook(string hookName)
//     {
//         if (_scriptEnv == null) return;
//
//         try
//         {
//             _scriptEnv.Eval(WrapExec(
//                 $"_hook = globals().get('{hookName}')\nif _hook and callable(_hook):\n    _hook()"));
//         }
//         catch (Exception ex)
//         {
//             LogUtil.Warning("script_mod_loader.hook_failed", Manifest.Id, hookName, ex.Message);
//         }
//     }
//
//     // 激活模组（调用 onEnable）
//     public override void Enable()
//     {
//         if (!_isLoaded) return;
//         CallLifecycleHook("onEnable");
//     }
//
//     // 停用模组（调用 onDisable）
//     public override void Disable()
//     {
//         if (!_isLoaded) return;
//         CallLifecycleHook("onDisable");
//     }
//
//     // 卸载模组（调用 onUnload）
//     public override void Unload()
//     {
//         if (!_isLoaded) return;
//         CallLifecycleHook("onUnload");
//         Dispose();
//     }
//
//     // 向脚本侧发送事件通知：调用全局钩子函数（如 onPlayerJumpStart），
//     // 传入事件数据供脚本侧 onItemUse(event) 等访问 event.ItemId / event.Item
//     public override void CallTriggerEvent(string eventName, BarkEvent? eventData = null)
//     {
//         if (_scriptEnv == null) return;
//
//         try
//         {
//             // 注入事件数据，供脚本侧通过 __barkEvent 或传参访问
//             if (eventData != null)
//             {
//                 EventScriptContext.CurrentEvent = eventData;
//                 _scriptEnv.Eval(WrapExec(
//                     "__barkEvent = puerts.load_type('Bark.Script.EventScriptContext').CurrentEvent"));
//             }
//             else
//             {
//                 _scriptEnv.Eval(WrapExec("__barkEvent = None"));
//             }
//
//             // 先尝试带参调用，TypeError 表示函数不接受参数则回退无参调用。
//             // Puerts.Python.Complete 内嵌运行时不一定有 inspect 模块，故用 try/except 探测。
//             _scriptEnv.Eval(WrapExec(
//                 $"_hook = globals().get('{eventName}')\nif _hook and callable(_hook):\n    try:\n        _hook(__barkEvent)\n    except TypeError:\n        _hook()"));
//         }
//         catch (Exception ex)
//         {
//             LogUtil.Warning("script_mod_loader.hook_failed", Manifest.Id, eventName, ex.Message);
//         }
//         finally
//         {
//             EventScriptContext.CurrentEvent = null;
//         }
//     }
//
//     // 执行单个物品脚本文件，执行前注入上下文全局变量，
//     // 脚本可定义 def main(itemId, item, action): 接收参数
//     public override void ExecuteItemFile(string filePath, string? itemId, Item? item = null, string? action = null)
//     {
//         if (_scriptEnv == null || !File.Exists(filePath)) return;
//
//         // 暂存上下文供 Python 侧通过 CS.Bark.Items.ItemScriptContext 访问
//         ItemScriptContext.CurrentItem = item;
//         ItemScriptContext.CurrentAction = action;
//
//         try
//         {
//             // 注入上下文全局变量
//             var escapedId = itemId != null ? EscapeString(itemId) : "None";
//             _scriptEnv.Eval(WrapExec($"__barkItemId = '{escapedId}'"));
//             _scriptEnv.Eval(WrapExec(
//                 "__barkItem = puerts.load_type('Bark.Items.ItemScriptContext').CurrentItem"));
//             _scriptEnv.Eval(WrapExec(
//                 "__barkAction = puerts.load_type('Bark.Items.ItemScriptContext').CurrentAction"));
//
//             // 执行脚本文件（注册 main 函数等定义）
//             var script = File.ReadAllText(filePath);
//             _scriptEnv.Eval(WrapExec(script));
//
//             // 调用 main(itemId, item, action) — Python 自动忽略多余参数
//             _scriptEnv.Eval(WrapExec(
//                 "_hook = globals().get('main')\nif _hook and callable(_hook):\n    _hook(__barkItemId, __barkItem, __barkAction)"));
//         }
//         catch (Exception ex)
//         {
//             LogUtil.Warning("script_engine.py_exec_file_failed", Manifest.Id, filePath, ex.Message);
//         }
//         finally
//         {
//             ItemScriptContext.CurrentItem = null;
//             ItemScriptContext.CurrentAction = null;
//         }
//     }
//
//     // 执行单个物块脚本文件，注入 tileId / tileContext / action 到脚本全局，
//     // 脚本可定义 def main(tileId, context, action):
//     public override void ExecuteTileFile(string filePath, string? tileId, Tile.TileScriptContext? context = null,
//         string? action = null)
//     {
//         if (_scriptEnv == null || !File.Exists(filePath)) return;
//
//         // 暂存上下文供 Python 侧通过 CS.Bark.Tile.TileScriptContext 访问
//         Tile.TileScriptContext.CurrentContext = context;
//         Tile.TileScriptContext.CurrentAction = action;
//
//         try
//         {
//             var escapedId = tileId != null ? EscapeString(tileId) : "None";
//             _scriptEnv.Eval(WrapExec($"__barkTileId = '{escapedId}'"));
//             _scriptEnv.Eval(WrapExec(
//                 "__barkTileContext = puerts.load_type('Bark.Tile.TileScriptContext').CurrentContext"));
//             _scriptEnv.Eval(WrapExec(
//                 "__barkAction = puerts.load_type('Bark.Tile.TileScriptContext').CurrentAction"));
//
//             var script = File.ReadAllText(filePath);
//             _scriptEnv.Eval(WrapExec(script));
//
//             _scriptEnv.Eval(WrapExec(
//                 "_hook = globals().get('main')\nif _hook and callable(_hook):\n    _hook(__barkTileId, __barkTileContext, __barkAction)"));
//         }
//         catch (Exception ex)
//         {
//             LogUtil.Warning("script_engine.py_exec_file_failed", Manifest.Id, filePath, ex.Message);
//         }
//         finally
//         {
//             Tile.TileScriptContext.CurrentContext = null;
//             Tile.TileScriptContext.CurrentAction = null;
//         }
//     }
//
//     // 每帧调用脚本侧的 onUpdate() 函数（静默，不记录错误日志避免刷屏）
//     public override void CallUpdate()
//     {
//         if (_scriptEnv == null || !_isLoaded) return;
//
//         try
//         {
//             _scriptEnv.Eval(WrapExec(
//                 "_hook = globals().get('onUpdate')\nif _hook and callable(_hook):\n    _hook()"));
//         }
//         catch
//         {
//             // 静默跳过：onUpdate 无需日志，避免每帧刷屏
//         }
//     }
//
//     // 释放引擎资源
//     public override void Dispose()
//     {
//         if (_scriptEnv != null)
//         {
//             try
//             {
//                 _scriptEnv.Dispose();
//             }
//             catch (Exception ex)
//             {
//                 LogUtil.Warning("script_engine.py_dispose_error", Manifest.Id, ex.Message);
//             }
//
//             _scriptEnv = null;
//         }
//
//         _isLoaded = false;
//     }
//
//     // 转义字符串中的特殊字符（用于 PuerTS Eval 注入到 Python 字符串字面量）
//     private static string EscapeString(string value)
//     {
//         return value
//             .Replace("\\", @"\\")
//             .Replace("'", @"\'")
//             .Replace("\n", @"\n")
//             .Replace("\r", @"\r");
//     }
//
//     // 将多行 Python 代码包装在 exec('''...''') 中，供 ScriptEnv.Eval 执行。
//     // Puerts.Python.Complete 内嵌运行时需 exec() 执行多条语句，eval() 仅支持单表达式。
//     private static string WrapExec(string pythonCode)
//     {
//         return $"exec('''{pythonCode}''')";
//     }
// }

