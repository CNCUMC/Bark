using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Bark.Base;
using BepInEx.Logging;

namespace Bark.Tool;

// CCL 兼容的语言生成器管理器
// 注册 ModLangGenBase 实例，在插件启动时通过 CCL LocaleRegistry 注册本地化条目
// 玩家运行 createLocale 命令即可收集所有条目到 EN.json
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class LocaleGenerator
{
    private static readonly List<ModLangGenBase> Generators = [];

    public static void SetLogger(ManualLogSource logger)
    {
    }

    public static void Register(ModLangGenBase generator, ManualLogSource logger)
    {
        if (generator == null)
            throw new ArgumentNullException(nameof(generator));

        if (Generators.Contains(generator))
            return;

        generator.Initialize(logger, Assembly.GetCallingAssembly());
        Generators.Add(generator);
    }
}