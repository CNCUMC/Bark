using System;
using System.Collections;
using System.IO;
using Bark.BetterCCL;
using Bark.Event;
using Bark.Example;
using Bark.Script;
using Bark.Tool;
using BepInEx;
using BepInEx.Logging;
using CUCoreLib.Helpers;
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
    internal static ScriptModLoader? _scriptModLoader;

    private static bool WorldGeneratedOver;

    public readonly string ScriptModsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptMod");
    private readonly Harmony _harmony = new(Guid);

    public void Awake()
    {
        Logger = base.Logger;

        new LangGenerator().Initialize(Logger);

        BetterOptions.Bool("bark", "test", Setting.SettingCategory.Game, false);
        BetterLocale.Flush();
        _harmony.PatchAll();

        DeployPuertsNativeFiles();
        // 先扫描 C# 事件处理器，再加载脚本模组（脚本模组会注册额外的处理器）
        EventRegistry.ScanAndRegister();
        LoadScriptMods();

        ModCommand.RegisterCommands();

        UpdateUtil.Check("CNCUMC/Bark", Name, Version, Logger);

        // 使用 CCL AwaitWorldGeneration 协程等待世界完全生成后再触发事件
        StartCoroutine(WaitForWorldGeneration());
    }

    private static IEnumerator WaitForWorldGeneration()
    {
        yield return CUCoreUtils.AwaitWorldGeneration();
        TriggerWorldGeneratedEvent();
    }

    private static void DeployPuertsNativeFiles()
    {
        var barkDir = Path.GetDirectoryName(typeof(Plugin).Assembly.Location) ?? string.Empty;
        var gameRoot = Path.GetDirectoryName(barkDir) ?? string.Empty;
        gameRoot = Path.GetDirectoryName(Path.GetDirectoryName(gameRoot)) ?? gameRoot;

        CopyNativeDlls(barkDir, gameRoot);
        CopyPuertsRuntime(barkDir, gameRoot);
    }

    // Papi* 和 PuertsCore 是原生 C++ 库，需要复制到游戏根目录
    // Puerts.* 是托管 .NET 程序集，由 BepInEx 从 plugins 目录加载
    private static void CopyNativeDlls(string sourceDir, string destDir)
    {
        foreach (var dll in new[]
                 {
                     "PuertsCore.dll",
                     "PapiV8.dll",
                     "PapiLua.dll"
                     // "PapiPython.dll"
                 })
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
            File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);

        foreach (var dir in Directory.GetDirectories(source))
            CopyDirectory(dir, Path.Combine(dest, Path.GetFileName(dir)));
    }

    private void LoadScriptMods()
    {
        _scriptModLoader = new ScriptModLoader(ScriptModsPath);
        _scriptModLoader.LoadAll();
    }

    private static void TriggerWorldGeneratedEvent()
    {
        if (WorldGeneratedOver) return;
        Events.Trigger(new WorldEvents.GeneratedEvent());
        WorldGeneratedOver = true;
    }
}