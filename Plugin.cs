using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bark.BetterCCL;
using Bark.Example;
using Bark.Script;
using Bark.Tool;
using BepInEx;
using BepInEx.Logging;
using CUCoreLib.Helpers;
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

    public readonly string ScriptModsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptMod");
    private ScriptModLoader? _scriptModLoader;

    public void Awake()
    {
        Logger = base.Logger;

        new LangGenerator().Initialize(Logger);

        BetterOptions.Bool("bark", "test", Setting.SettingCategory.Game, false);
        BetterLocale.Flush();
        _harmony.PatchAll();

        DeployPuertsNativeFiles();
        LoadScriptMods();
        RegisterCommands();

        UpdateUtil.Check("CNCUMC/Bark", Name, Version, Logger);
    }

    /// <summary>
    /// 自动将 PuerTS 原生库和运行时文件复制到游戏根目录
    /// </summary>
    private void DeployPuertsNativeFiles()
    {
        var barkDir = Path.GetDirectoryName(typeof(Plugin).Assembly.Location) ?? string.Empty;
        var gameRoot = Path.GetDirectoryName(barkDir) ?? string.Empty;
        gameRoot = Path.GetDirectoryName(Path.GetDirectoryName(gameRoot)) ?? gameRoot;

        CopyNativeDlls(barkDir, gameRoot);
        CopyPuertsRuntime(barkDir, gameRoot);
    }

    private static void CopyNativeDlls(string sourceDir, string destDir)
    {
        foreach (var dll in new[] { "PuertsCore.dll", "PapiV8.dll" })
        {
            var source = Path.Combine(sourceDir, dll);
            var dest = Path.Combine(destDir, dll);
            if (!File.Exists(source) || File.Exists(dest)) continue;
            File.Copy(source, dest);
            LogUtil.Info("native_dll_copied", dll);
        }
    }

    private static void CopyPuertsRuntime(string sourceDir, string destDir)
    {
        var puertsSource = Path.Combine(sourceDir, "puerts");
        var puertsDest = Path.Combine(destDir, "puerts");
        if (!Directory.Exists(puertsSource) || Directory.Exists(puertsDest)) return;
        CopyDirectory(puertsSource, puertsDest);
        LogUtil.Info("puerts_runtime_copied");
    }

    private static void CopyDirectory(string source, string dest)
    {
        Directory.CreateDirectory(dest);

        foreach (var file in Directory.GetFiles(source))
            File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), overwrite: true);

        foreach (var dir in Directory.GetDirectories(source))
            CopyDirectory(dir, Path.Combine(dest, Path.GetFileName(dir)));
    }

    private void LoadScriptMods()
    {
        _scriptModLoader = new ScriptModLoader(ScriptModsPath);
        _scriptModLoader.LoadAll();
    }

    private void RegisterCommands()
    {
        ConsoleCommandRegistry.Register(
            "catfcabl",
            BetterLocale.GetCommand("catfcabl"),
            _ => ExportLocaleDebugFile()
        );

        ConsoleCommandRegistry.Register(
            "rs",
            LocaleCommand("reload"),
            _ => _scriptModLoader?.ReloadAll()
        );

        ConsoleCommandRegistry.Register(
            "script",
            LocaleCommand("description"),
            ExecuteScriptCommand,
            new Dictionary<int, List<string>>
            {
                { 0, ["help", "reload", "list"] }
            }
        );
    }

    private static void ExportLocaleDebugFile()
    {
        var path = Path.Combine(Paths.CachePath, "catfcabl.txt");
        var lines = new List<string>();

        lines.AddRange(BetterLocale.LocaleKeys
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}: {x.Value}"));
        lines.Add("");
        lines.AddRange(BetterLocale.LocaleGetKeys
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}: {x.Value}"));

        File.WriteAllLines(path, lines);
        LogUtil.Message($"catfcabl.txt: {path}", Logger);
        LogUtil.Message($"Register Count: {BetterLocale.LocaleKeys.Count}", Logger);
        LogUtil.Message($"Call Count: {BetterLocale.LocaleGetKeys.Count}", Logger);
    }

    private void ExecuteScriptCommand(string[] args)
    {
        if (args.Length == 1)
        {
            PrintHelp();
            return;
        }

        switch (args[1])
        {
            case "help":
                PrintHelp();
                break;
            case "reload":
                CUCoreUtils.ConsoleRunCommand(ConsoleScript.instance, "rs");
                break;
            case "list":
                PrintList();
                break;
            default:
                PrintHelp();
                break;
        }
    }

    private static void PrintHelp()
    {
        var helpItems = new List<(string key, string value)>
        {
            ("help", LocaleCommand("help.help")),
            ("reload", LocaleCommand("help.reload"))
        };

        var header = LocaleCommand("help.header");
        LogUtil.PrintKeyValueList(header, helpItems, Logger);
    }

    private void PrintList()
    {
        if (_scriptModLoader == null) return;

        var mods = _scriptModLoader.ListMods();
        if (mods.Count == 0)
        {
            LogUtil.Info("script_mod_loader.no_mods", Logger);
            return;
        }

        LogUtil.Message(LocaleCommand("list.header", mods.Count), Logger);
        foreach (var mod in mods)
            LogUtil.Message(LocaleCommand("list.item", mod.Name, mod.Version, mod.Language, mod.Id), Logger);
    }

    private static string LocaleCommand(string key, params object[] args)
    {
        return BetterLocale.GetCommand($"{NameSpace}.script.{key}", args);
    }
}