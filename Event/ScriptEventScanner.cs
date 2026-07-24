using System;
using System.Collections.Generic;
using System.Reflection;
using Bark.Script;
using Bark.Tool;

namespace Bark.Event;

// 在启动时扫描所有 [ScriptEvent] 标记的事件类型并缓存
// 脚本模组加载时自动注册为桥接回调
public static class ScriptEventScanner
{
    // 事件类型 → HookName
    private static readonly Dictionary<Type, string> s_scriptEvents = new();
    private static bool s_scanned;

    // 扫描所有程序集中标注了 [ScriptEvent] 的 BarkEvent 子类
    public static void Scan()
    {
        if (s_scanned) return;

        var count = 0;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            // 快速过滤：只扫描 Bark 相关程序集
            var assemblyName = assembly.GetName().Name;
            if (assemblyName == null || !assemblyName.StartsWith("Bark", StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attr = type.GetCustomAttribute<ScriptEventAttribute>();
                    if (attr == null) continue;

                    if (!typeof(BarkEvent).IsAssignableFrom(type))
                    {
                        LogUtil.Warning("script_event.not_bark_event", type.FullName ?? type.Name);
                        continue;
                    }

                    s_scriptEvents[type] = attr.HookName;
                    count++;
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // 跳过无法加载类型的程序集
            }
        }

        s_scanned = true;
        LogUtil.Info("script_event.scanned", count.ToString());
    }

    // 为指定模组注册所有已扫描到的脚本事件
    public static void RegisterForMod(ScriptManifest manifest)
    {
        if (manifest is null) throw new ArgumentNullException(nameof(manifest));

        var engine = manifest.Engine;
        if (engine == null)
        {
            LogUtil.Warning("event.script_handler_no_engine", manifest.Id);
            return;
        }

        foreach (var (eventType, hookName) in s_scriptEvents)
        {
            var hook = hookName; // 闭包捕获
            EventRegistry.Register(eventType, _ => engine.CallTriggerEvent(hook), manifest.Id);
        }
    }
}