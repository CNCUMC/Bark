using System;
using System.Collections.Generic;
using System.Reflection;
using Bark.Event;
using Bark.Tool;

namespace Bark.Script;

public static class ScriptEventScanner
{
    private static readonly Dictionary<Type, string> s_scriptEvents = new();
    private static bool s_scanned;

    public static void Scan()
    {
        if (s_scanned) return;

        var count = 0;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var assemblyName = assembly.GetName().Name;
            if (assemblyName == null
                || !assemblyName.StartsWith("Bark", StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attr = type.GetCustomAttribute<ScriptEventAttribute>();
                    if (attr == null) continue;

                    if (!typeof(BarkEvent).IsAssignableFrom(type))
                    {
                        LogUtil.Warning("script_event.not_bark_event",
                            type.FullName ?? type.Name);
                        continue;
                    }

                    s_scriptEvents[type] = attr.HookName;
                    count++;
                }
            }
            catch (ReflectionTypeLoadException)
            {
            }
        }

        s_scanned = true;
        LogUtil.Info("script_event.scanned", count.ToString());
    }

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
            var hook = hookName;
            EventRegistry.Register(eventType, evt => engine.CallTriggerEvent(hook, evt),
                manifest.Id);
        }
    }
}
