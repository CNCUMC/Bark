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
[BepInDependency("net.cucorelib", "1.0.4")]
[BepInDependency("KrokoshaCasualtiesMP", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.jimmyking.whatmodisthisfrom", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "org.cncumc.bark";
    public const string Name = "Bark";
    public const string Version = "2.4.0";
    public const string NameSpace = "bark";
    internal new static ManualLogSource Logger = null!;
    internal static ScriptModLoader? _scriptModLoader;
    internal static readonly string BarkCachePath = Paths.CachePath + "Bark/";

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

        // 音频管理器初始化，供所有物品系统加载自定义音效
        AudioManager.Initialize(this);

        new LangGenerator().Initialize(Logger);

        // BetterOptions.Bool("bark", "test", Setting.SettingCategory.Game, false);
        BetterLocale.Flush();
        _harmony.PatchAll();
        WmitfPatch.Apply(_harmony);

        DeployPuertsNativeFiles();

        // 扫描注解驱动的事件：
        // 1. C# 模组的 [EventBusSubscriber]（方法参数为 BarkEvent 子类即自动注册）
        // 2. 脚本模组的 [ScriptEvent]（标记哪些事件需要桥接到 Lua/JS）
        EventRegistry.ScanAndRegister();
        ScriptEventScanner.Scan();

        // 注册所有带 [ScriptMethod] 的 Tool 类型到 ApiRegistry
        // 脚本引擎加载时会自动从 ApiRegistry 平铺注入到全局作用域
        RegisterScriptApis();

        // 注册内置物品模板（如 gun 模板），供后续模组的物品 JSON 引用
        InitializeBuiltinTemplates();

        // 以 Bark 自身作为模组加载 JSON 物品：扫描与本程序集同目录的 mod.json + Item/ 子目录。
        // 这样 Bark 自带的 C# 物品（如 BepInEx/plugins/Bark/Item/*.json）也会注册进 ItemRegistry。
        // LoadOwnItems();

        // 安装枪械运行时补丁，覆盖 GunScript 原生装弹/卸弹/开火逻辑，
        // 用模板标签匹配替换硬编码的弹药类型枚举
        GunRuntimeManager.Apply();

        LoadScriptMods();

        // 多人游戏脚本模组同步（KrokMP 未安装时零开销）
        NetworkModSync.Initialize(ScriptModsPath);

        // 主机 sr 重载触发的增量文件同步（客户端上报 hash -> 主机对比 -> 推送差异文件）
        ScriptFileSync.Initialize(ScriptModsPath);

        // 脚本模组加载完后，将 Lang 本地化刷新到 CCL 的 locale 文件，确保选项标签/描述在游戏 UI 中可见
        BetterLocale.Flush();

        ModCommand.RegisterCommands();

        // 监听主菜单加载完成后触发事件
        MainMenuEventListener.Listen(this);
        // 监听世界生成完成后触发事件
        WorldEventListener.Listen(this);
        PlayerEventListener.Listen(this);
        BodyEventListener.Listen(this);
        LimbEventListener.Listen(this);
        // 监听 Moodle 获取/遍历/消失，触发 Moodle 脚本事件
        MoodleEventListener.Listen(this);
        // 监听物品使用/装备/对肢体使用，触发物品脚本
        ItemEventListener.Listen(this);
        // 注册物品脚本运行器监听物品事件
        ItemScriptRunner.Listen();
        // 注册 Moodle 脚本运行器监听 Moodle 事件
        MoodleScriptRunner.Listen();
        // 监听物块放置/破坏/受击，触发物块脚本
        TileEventListener.Listen(this);
        // 注册物块脚本运行器监听物块事件
        TileScriptRunner.Listen();
        // 监听枪械操作（开火/拉栓/保险/装弹/卸弹/卡壳），触发枪械事件
        GunEventListener.Listen(this);
        // 监听小游戏（AED 除颤/包扎），触发小游戏事件
        MinigameEventListener.Listen();
        // 监听世界物品/实体（电池/自动泵/捕兽夹/建筑等），触发对应事件
        WorldEntityEventListener.Listen();
        // 监听水晶效果/水晶敌人，触发水晶事件
        CrystalEventListener.Listen();
        // 监听环境（洞穴蜘蛛/可攀爬物/电线圈/尸体），触发环境事件
        EnvironmentEventListener.Listen();
        // 监听世界对象（可损坏物/板条箱/钻探舱/长老/PDA/间歇泉/暗幕/捕抓植物/抓钩），触发对应事件
        WorldObjectEventListener.Listen();
        // 监听系统（精神抹除/辐射线/存档/技能/商人/炮塔/世界重生/电锯/声波炮），触发对应事件
        SystemEventListener.Listen();
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

    // 注册所有带 [ScriptApi] 的类到 ApiRegistry
    // ApiRegistry 为每个类型生成 AutoApi 代理，脚本引擎按 camelCase 类名直接注入全局
    private static void RegisterScriptApis()
    {
        ApiRegistry.ScanAndRegister();
    }

    // 注册内置物品模板，供模组物品 JSON 引用
    // 模板在 Plugin 初始化时注册，之后 ItemLoader 解析物品时可引用
    // gun → mag → ammo → casing 四层模板通过 ammo_type / mag_type / casing_type 标签建立关联
    private static void InitializeBuiltinTemplates()
    {
        new GunTemplate().Register();
        new MagTemplate().Register();
        new AmmunitionTemplate().Register();
        new CasingTemplate().Register();
        new FoodTemplate().Register();
        new ClothingTemplate().Register();
        new PlushTemplate().Register();
        // 安装玩偶吱吱音效补丁：配置了 squeak_sound 的玩偶用 Bark Audio 播放自定义音效
        PlushTemplate.ApplySqueakHook();
    }
}