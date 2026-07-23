using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bark.BetterCCL;
using Bark.Tool;
using BepInEx;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;

namespace Bark.Example;

public static class ModCommand
{
    public static void RegisterCommands()
    {
        ConsoleCommandRegistry.Register(
            "catfcabl",
            BetterLocale.GetCommand("catfcabl"),
            _ => ExportLocaleDebugFile()
        );

        ConsoleCommandRegistry.Register(
            "rs",
            LocaleCommand("reload"),
            _ => Plugin._scriptModLoader?.ReloadAll()
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

    public static void ExportLocaleDebugFile()
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
        LogUtil.Message($"catfcabl.txt: {path}", Plugin.Logger);
        LogUtil.Message($"Register Count: {BetterLocale.LocaleKeys.Count}", Plugin.Logger);
        LogUtil.Message($"Call Count: {BetterLocale.LocaleGetKeys.Count}", Plugin.Logger);
    }

    private static void ExecuteScriptCommand(string[] args)
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

    public static void PrintHelp()
    {
        var helpItems = new List<(string key, string value)>
        {
            ("help", LocaleCommand("help.help")),
            ("reload", LocaleCommand("help.reload")),
            ("list", LocaleCommand("help.list"))
        };

        var header = LocaleCommand("help.header");
        LogUtil.PrintKeyValueList(header, helpItems, Plugin.Logger);
    }

    private static void PrintList()
    {
        if (Plugin._scriptModLoader == null) return;

        var mods = Plugin._scriptModLoader.ListMods();
        if (mods.Count == 0)
        {
            LogUtil.Info("script_mod_loader.no_mods", Plugin.Logger);
            return;
        }

        LogUtil.Message(LocaleCommand("list.header", mods.Count), Plugin.Logger);
        foreach (var mod in mods)
            LogUtil.Message(LocaleCommand("list.item", mod.Name, mod.Version, mod.Language, mod.Id), Plugin.Logger);
    }

    public static string LocaleCommand(string key, params object[] args)
    {
        return BetterLocale.GetCommand($"{Plugin.NameSpace}.script.{key}", args);
    }
}