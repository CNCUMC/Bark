using System;
using System.IO;
using Bark.Audio;
using Bark.BetterCCL;
using Bark.Compat.Wmitf;
using Bark.Event;
using Bark.Event.Listener;
using Bark.Items;
using Bark.Items.Runtime.Gun;
using Bark.Items.Templates;
using Bark.Moodle;
using Bark.Script;
using Bark.ScriptApi;
using Bark.Tile;
using Bark.Tool;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Bark;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency("net.cucorelib", "1.0.6")]
[BepInDependency("CasualtiesMP", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.jimmyking.whatmodisthisfrom",
    BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "org.cncumc.bark";
    public const string Name = "Bark";
    public const string Version = "2.5.0";
    public const string NameSpace = "bark";
    internal new static ManualLogSource Logger = null!;
    internal static ScriptModLoader? _scriptModLoader;
    internal static readonly string BarkCachePath = Paths.CachePath + "Bark/";

    public readonly string ScriptModsPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptMod");

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
        BodyEventListener.Stop();
        LimbEventListener.Stop();
        MoodleEventListener.Stop();
        ItemEventListener.Stop();
        ItemScriptRunner.Stop();
        MoodleScriptRunner.Stop();
        TileEventListener.Stop();
        TileScriptRunner.Stop();
        GunEventListener.Stop();
        MinigameEventListener.Stop();
        WorldEntityEventListener.Stop();
        CrystalEventListener.Stop();
        EnvironmentEventListener.Stop();
        WorldObjectEventListener.Stop();
        SystemEventListener.Stop();
        GunRuntimeManager.Unapply();
        GunMagTracker.ClearAll();
        AudioManager.Shutdown();
        _scriptModLoader?.Dispose();
    }

    private void AwakeInternal()
    {
        Logger = base.Logger;

        AudioManager.Initialize(this);

        new LangGenerator().Initialize(Logger);

        BetterLocale.Flush();
        _harmony.PatchAll();
        WmitfPatch.Apply(_harmony);

        DeployPuertsNativeFiles();

        EventRegistry.ScanAndRegister();
        ScriptEventScanner.Scan();

        RegisterScriptApis();
        InitializeBuiltinTemplates();

        GunRuntimeManager.Apply();

        LoadScriptMods();

        NetworkModSync.Initialize(ScriptModsPath);
        ScriptFileSync.Initialize(ScriptModsPath);
        BetterLocale.Flush();

        ModCommand.RegisterCommands();

        MainMenuEventListener.Listen(this);
        WorldEventListener.Listen(this);
        PlayerEventListener.Listen(this);
        BodyEventListener.Listen(this);
        LimbEventListener.Listen(this);
        MoodleEventListener.Listen(this);
        ItemEventListener.Listen(this);
        ItemScriptRunner.Listen();
        MoodleScriptRunner.Listen();
        TileEventListener.Listen(this);
        TileScriptRunner.Listen();
        GunEventListener.Listen(this);
        MinigameEventListener.Listen();
        WorldEntityEventListener.Listen();
        CrystalEventListener.Listen();
        EnvironmentEventListener.Listen();
        WorldObjectEventListener.Listen();
        SystemEventListener.Listen();
    }

    private static void DeployPuertsNativeFiles()
    {
        var barkDir = Path.GetDirectoryName(typeof(Plugin).Assembly.Location)
            ?? string.Empty;
        var gameRoot = Path.GetDirectoryName(barkDir) ?? string.Empty;
        gameRoot = Path.GetDirectoryName(Path.GetDirectoryName(gameRoot)) ?? gameRoot;

        CopyNativeDlls(barkDir, gameRoot);
        CopyPuertsRuntime(barkDir, gameRoot);
    }

    private static void CopyNativeDlls(string sourceDir, string destDir)
    {
        foreach (var dll in new[]
            {
                "PuertsCore.dll",
                "PapiV8.dll",
                "PapiLua.dll"
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

    private static void RegisterScriptApis()
    {
        ApiRegistry.ScanAndRegister();
    }

    private static void InitializeBuiltinTemplates()
    {
        new GunTemplate().Register();
        new MagTemplate().Register();
        new AmmunitionTemplate().Register();
        new CasingTemplate().Register();
        new FoodTemplate().Register();
        new ClothingTemplate().Register();
        new PlushTemplate().Register();
        PlushTemplate.ApplySqueakHook();
    }
}
