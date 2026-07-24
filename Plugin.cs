using System;
using System.IO;
using Bark.BetterCCL;
using Bark.Event;
using Bark.Event.Listener;
using Bark.Example;
using Bark.Script;
using Bark.ScriptApi;
using Bark.Tool;
using BepInEx;
using BepInEx.Logging;
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

    public readonly string ScriptModsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptMod");
    private readonly Harmony _harmony = new(Guid);

    public void Awake()
    {
        AwakeInternal();
    }

    public void Update()
    {
        _scriptModLoader?.UpdateAll();
    }

    public void OnDestroy()
    {
        PlayerEventListener.Stop();
        LimbEventListener.Stop();
        _scriptModLoader?.Dispose();
    }

    private void AwakeInternal()
    {
        Logger = base.Logger;

        new LangGenerator().Initialize(Logger);

        BetterOptions.Bool("bark", "test", Setting.SettingCategory.Game, false);
        BetterLocale.Flush();
        _harmony.PatchAll();

        DeployPuertsNativeFiles();
        // 扫描注解驱动的事件：
        // 1. C# 模组的 [EventBusSubscriber]（方法参数为 BarkEvent 子类即自动注册）
        // 2. 脚本模组的 [ScriptEvent]（标记哪些事件需要桥接到 Lua/JS）
        EventRegistry.ScanAndRegister();
        ScriptEventScanner.Scan();

        // 注册所有带 [ScriptMethod] 的 Tool 类型到 ApiRegistry
        // 脚本引擎加载时会自动从 ApiRegistry 平铺注入到全局作用域
        RegisterScriptApis();

        LoadScriptMods();

        // 脚本模组加载完后，将 Lang 本地化刷新到 CCL 的 locale 文件，确保选项标签/描述在游戏 UI 中可见
        BetterLocale.Flush();

        ModCommand.RegisterCommands();

        UpdateUtil.Check("CNCUMC/Bark", Name, Version, Logger);

        // 监听主菜单加载完成后触发事件
        MainMenuEventListener.Listen(this);
        // 监听世界生成完成后触发事件
        WorldEventListener.Listen(this);
        PlayerEventListener.Listen(this);
        LimbEventListener.Listen(this);
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

    // 注册所有带 [ScriptMethod] 的 Tool 类型到 ApiRegistry
    // ApiRegistry 为每个类型生成 AutoApi 代理，脚本引擎按 camelCase 类名直接注入全局
    private static void RegisterScriptApis()
    {
        ApiRegistry.Register(typeof(BodyUtil));
        ApiRegistry.Register(typeof(PlayerUtil));
        ApiRegistry.Register(typeof(InventoryUtil));
        ApiRegistry.Register(typeof(ItemUtil));
        ApiRegistry.Register(typeof(LimbUtil));
        ApiRegistry.Register(typeof(SkillUtil));
        ApiRegistry.Register(typeof(WorldUtil));
        ApiRegistry.Register(typeof(OptionsApi));
    }
}