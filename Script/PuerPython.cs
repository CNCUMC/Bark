// FUCK PYTHON
// FUCK PYTHON
// FUCK PYTHON
// FUCK PYTHON
// FUCK PYTHON
// FUCK PYTHON
// FUCK PYTHON
// FUCK PYTHON
// FUCK PYTHON
// FUCK PYTHON
// FUCK PYTHON
// FUCK PYTHON

// using System;
// using System.IO;
// using Bark.Tool;
// using Puerts;
// using UnityEngine;
//
// namespace Bark.Script;
//
// // PuerTS Python 引擎包装器，管理脚本模组的生命周期
// public class PuerPython : MonoBehaviour
// {
//     private ScriptEnv? _scriptEnv;
//     private ScriptManifest _manifest = null!;
//     private bool _isLoaded;
//
//     private static readonly string[] PythonDllNames =
//     {
//         "python314.dll", "python313.dll", "python312.dll",
//         "python311.dll", "python310.dll", "python3.dll"
//     };
//
//     // 检测 Python 运行时是否可用（避免原生崩溃）
//     private static bool IsPythonAvailable()
//     {
//         // 1. 检查 PYTHONHOME 目录中是否存在 pythonXX.dll
//         var pythonHome = Environment.GetEnvironmentVariable("PYTHONHOME");
//         if (!string.IsNullOrEmpty(pythonHome) && Directory.Exists(pythonHome))
//         {
//             foreach (var dll in PythonDllNames)
//                 if (File.Exists(Path.Combine(pythonHome, dll)))
//                     return true;
//         }
//
//         // 2. 检查游戏根目录
//         var gameRoot = Path.GetDirectoryName(Path.GetDirectoryName(
//             Path.GetDirectoryName(typeof(PuerPython).Assembly.Location) ?? "")) ?? "";
//         foreach (var dll in PythonDllNames)
//             if (File.Exists(Path.Combine(gameRoot, dll)))
//                 return true;
//
//         // 3. 检查系统目录
//         var systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
//         foreach (var dll in PythonDllNames)
//             if (File.Exists(Path.Combine(systemDir, dll)))
//                 return true;
//
//         return false;
//     }
//
//     // 加载并执行 Python 模组，返回是否成功
//     public bool Load(ScriptManifest manifest)
//     {
//         _manifest = manifest;
//
//         if (!IsPythonAvailable())
//         {
//             LogUtil.Warning("script_mod_loader.python_not_available", manifest.Id);
//             return false;
//         }
//
//         try
//         {
//             // 创建 Python 引擎实例
//             _scriptEnv = new ScriptEnv(new BackendPython());
//
//             // 注入 bark.* API
//             InjectBarkApi();
//
//             // 执行入口脚本
//             var script = System.IO.File.ReadAllText(manifest.EntryFile);
//             _scriptEnv.Eval(script);
//
//             _isLoaded = true;
//
//             // 调用 onLoad 生命周期钩子
//             CallLifecycleHook("on_load");
//         }
//         catch (Exception ex)
//         {
//             LogUtil.Warning("script_mod_loader.load_failed", manifest.Id, ex.Message);
//             Cleanup();
//             return false;
//         }
//
//         return true;
//     }
//
//     // 注入 bark.* API 到 Python 环境
//     private void InjectBarkApi()
//     {
//         if (_scriptEnv == null) return;
//
//         // Python 使用 import 语法导入 C# 类，无 new 关键字
//         var id = EscapeString(_manifest.Id);
//         var version = EscapeString(_manifest.Version);
//         var scriptName = EscapeString(_manifest.Name);
//         _scriptEnv.Eval($"""
//                              exec('import Bark.ScriptApi.ScriptApi as ScriptApi')
//                              bark = ScriptApi('{id}', '{version}', '{scriptName}')
//                          """);
//     }
//
//     // 调用生命周期钩子
//     private void CallLifecycleHook(string hookName)
//     {
//         if (_scriptEnv == null) return;
//
//         try
//         {
//             _scriptEnv.Eval($"""
//                                  if callable({hookName}):
//                                      {hookName}()
//                              """);
//         }
//         catch (Exception ex)
//         {
//             LogUtil.Warning("script_mod_loader.hook_failed", _manifest.Id, hookName, ex.Message);
//         }
//     }
//
//     // 激活模组（调用 on_enable）
//     public void Enable()
//     {
//         if (!_isLoaded) return;
//         CallLifecycleHook("on_enable");
//     }
//
//     // 停用模组（调用 on_disable）
//     public void Disable()
//     {
//         if (!_isLoaded) return;
//         CallLifecycleHook("on_disable");
//     }
//
//     // 卸载模组（调用 on_unload）
//     public void Unload()
//     {
//         if (!_isLoaded) return;
//         CallLifecycleHook("on_unload");
//         Cleanup();
//     }
//
//     // 清理资源
//     private void Cleanup()
//     {
//         if (_scriptEnv != null)
//         {
//             try
//             {
//                 _scriptEnv.Dispose();
//             }
//             catch
//             {
//                 /* 静默忽略清理异常 */
//             }
//
//             _scriptEnv = null;
//         }
//
//         _isLoaded = false;
//     }
//
//     private void OnDestroy()
//     {
//         Cleanup();
//     }
//
//     // 转义字符串中的特殊字符（用于 PuerTS Eval 注入）
//     private static string EscapeString(string value)
//     {
//         return value
//             .Replace("\\", @"\\")
//             .Replace("'", @"\'")
//             .Replace("\n", @"\n")
//             .Replace("\r", @"\r");
//     }
// }

