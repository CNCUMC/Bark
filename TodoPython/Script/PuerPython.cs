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
// public class PuerPython : ScriptEngine
// {
//     private bool _isLoaded;
//     private ScriptEnv? _scriptEnv;
//
//     public override bool Load(ScriptManifest manifest)
//     {
//         base.Load(manifest);
//
//         try
//         {
//             _scriptEnv = new ScriptEnv(new BackendPython());
//
//             InjectBarkApi();
//
//             var script = File.ReadAllText(Manifest.EntryFile);
//             _scriptEnv.Eval(WrapExec(script));
//             _isLoaded = true;
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
//         sb.AppendLine("_api_registry = puerts.load_type('Bark.ScriptApi.ApiRegistry')");
//         foreach (var (name, _) in ApiRegistry.Proxies)
//             sb.AppendLine($"{name} = _api_registry.GetProxy('{name}')");
//
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
//             LogUtil.Warning("script_mod_loader.hook_failed", Manifest.Id, hookName,
//                 ex.Message);
//         }
//     }
//
//     public override void Enable()
//     {
//         if (!_isLoaded) return;
//         CallLifecycleHook("onEnable");
//     }
//
//     public override void Disable()
//     {
//         if (!_isLoaded) return;
//         CallLifecycleHook("onDisable");
//     }
//
//     public override void Unload()
//     {
//         if (!_isLoaded) return;
//         CallLifecycleHook("onUnload");
//         Dispose();
//     }
//
//     public override void CallTriggerEvent(string eventName, BarkEvent? eventData = null)
//     {
//         if (_scriptEnv == null) return;
//
//         try
//         {
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
//             _scriptEnv.Eval(WrapExec(
//                 $"_hook = globals().get('{eventName}')\nif _hook and callable(_hook):\n    try:\n        _hook(__barkEvent)\n    except TypeError:\n        _hook()"));
//         }
//         catch (Exception ex)
//         {
//             LogUtil.Warning("script_mod_loader.hook_failed", Manifest.Id, eventName,
//                 ex.Message);
//         }
//         finally
//         {
//             EventScriptContext.CurrentEvent = null;
//         }
//     }
//
//     public override void ExecuteItemFile(string filePath, string? itemId,
//         Item? item = null, string? action = null)
//     {
//         if (_scriptEnv == null || !File.Exists(filePath)) return;
//
//         ItemScriptContext.CurrentItem = item;
//         ItemScriptContext.CurrentAction = action;
//
//         try
//         {
//             var escapedId = itemId != null
//                 ? EscapeString(itemId)
//                 : "None";
//             _scriptEnv.Eval(WrapExec($"__barkItemId = '{escapedId}'"));
//             _scriptEnv.Eval(WrapExec(
//                 "__barkItem = puerts.load_type('Bark.Items.ItemScriptContext').CurrentItem"));
//             _scriptEnv.Eval(WrapExec(
//                 "__barkAction = puerts.load_type('Bark.Items.ItemScriptContext').CurrentAction"));
//
//             var script = File.ReadAllText(filePath);
//             _scriptEnv.Eval(WrapExec(script));
//
//             _scriptEnv.Eval(WrapExec(
//                 "_hook = globals().get('main')\nif _hook and callable(_hook):\n    _hook(__barkItemId, __barkItem, __barkAction)"));
//         }
//         catch (Exception ex)
//         {
//             LogUtil.Warning("script_engine.py_exec_file_failed", Manifest.Id, filePath,
//                 ex.Message);
//         }
//         finally
//         {
//             ItemScriptContext.CurrentItem = null;
//             ItemScriptContext.CurrentAction = null;
//         }
//     }
//
//     public override void ExecuteTileFile(string filePath, string? tileId,
//         Tile.TileScriptContext? context = null,
//         string? action = null)
//     {
//         if (_scriptEnv == null || !File.Exists(filePath)) return;
//
//         Tile.TileScriptContext.CurrentContext = context;
//         Tile.TileScriptContext.CurrentAction = action;
//
//         try
//         {
//             var escapedId = tileId != null
//                 ? EscapeString(tileId)
//                 : "None";
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
//             LogUtil.Warning("script_engine.py_exec_file_failed", Manifest.Id, filePath,
//                 ex.Message);
//         }
//         finally
//         {
//             Tile.TileScriptContext.CurrentContext = null;
//             Tile.TileScriptContext.CurrentAction = null;
//         }
//     }
//
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
//             // ignored
//         }
//     }
//
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
//                 LogUtil.Warning("script_engine.py_dispose_error", Manifest.Id,
//                     ex.Message);
//             }
//
//             _scriptEnv = null;
//         }
//
//         _isLoaded = false;
//     }
//
//     private static string EscapeString(string value)
//     {
//         return value
//             .Replace("\\", @"\\")
//             .Replace("'", @"\'")
//             .Replace("\n", @"\n")
//             .Replace("\r", @"\r");
//     }
//
//     private static string WrapExec(string pythonCode)
//     {
//         return $"exec('''{pythonCode}''')";
//     }
// }
