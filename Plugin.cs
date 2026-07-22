using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bark.BetterCCL;
using Bark.Example;
using Bark.ScriptMod;
using Bark.Tool;
using BepInEx;
using BepInEx.Logging;
using CUCoreLib.Registries;
using HarmonyLib;

namespace Bark;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency("net.cucorelib", "1.0.3")]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "org.cncumc.bark";
    public const string Name = "Bark";
    public const string Version = "2.0.0";
    public const string NameSpace = "bark";
    internal new static ManualLogSource Logger = null!;
    private readonly Harmony _harmony = new(Guid);
    
    public readonly string ScriptModsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptMods");

    public void Awake()
    {
        Logger = base.Logger;

        new LangGenerator().Initialize(Logger);

        BetterOptions.Bool("bark", "test", Setting.SettingCategory.Game, false);
        BetterLocale.Flush();
        _harmony.PatchAll();

        // 加载脚本模组
        var scriptModLoader = new ScriptModLoader(ScriptModsPath);
        scriptModLoader.LoadAll();

        ConsoleCommandRegistry.Register(
            "catfcabl",
            BetterLocale.GetCommand("catfcabl"),
            _ =>
            {
                var path = Paths.CachePath + "\\catfcabl.txt";
                var lines = new List<string>();
                lines.AddRange(BetterLocale.LocaleKeys.OrderBy(x => x.Key).Select(x => $"{x.Key}: {x.Value}"));
                lines.Add("");
                lines.AddRange(BetterLocale.LocaleGetKeys.OrderBy(x => x.Key).Select(x => $"{x.Key}: {x.Value}"));
                File.WriteAllLines(path, lines);
                LogUtil.Message($"catfcabl.txt: {path}", Logger);
                LogUtil.Message($"Register Count: {BetterLocale.LocaleKeys.Count}", Logger);
                LogUtil.Message($"Call Count: {BetterLocale.LocaleGetKeys.Count}", Logger);
            }
        );

        UpdateUtil.Check("CNCUMC/Bark", Name, Version, Logger);
    }
}