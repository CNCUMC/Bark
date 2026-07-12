using Bark.BetterCCL;
using Bark.Example.Lang;
using Bark.Tool;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Bark;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency("net.cucorelib", "1.0.2")]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "org.cncumc.bark";
    public const string Name = "Bark";
    public const string Version = "1.0.3";
    internal new static ManualLogSource Logger = null!;
    private readonly Harmony _harmony = new(Guid);

    public void Awake()
    {
        Logger = base.Logger;

        new EnLangGenerator().Initialize(Logger);
        new ZhCnLangGenerator().Initialize(Logger);
        new ZhTwLangGenerator().Initialize(Logger);

        BetterOptions.Bool("bark", "test", Setting.SettingCategory.Game, false);
        BetterLocale.Flush();
        _harmony.PatchAll();

        UpdateUtil.Check("CNCUMC/Bark", Name, Version, Logger);
    }
}