using Bark.Example.Lang;
using Bark.Tool;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Bark;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency("net.cucorelib")]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "org.cucnmc.bark";
    public const string Name = "Bark";
    public const string Version = "1.0.0";
    internal new static ManualLogSource Logger;
    private readonly Harmony _harmony = new(Guid);

    public void Awake()
    {
        Logger = base.Logger;

        LocaleGenerator.SetLogger(Logger);
        LocaleGenerator.Register(new EnLangGenerator(), Logger);
        LocaleGenerator.Register(new ZhCnLangGenerator(), Logger);
        LocaleGenerator.Register(new ZhTwLangGenerator(), Logger);
        LocaleGenerator.GenerateAll();

        _harmony.PatchAll();

        Logger.LogInfo("Bark!");
    }
}