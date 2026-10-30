using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bark.BetterCCL;
using Bark.Items;
using Bark.Moodle;
using Bark.Recipe;
using Bark.Script;
using Bark.Tile;
using Bark.Tool;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using HarmonyLib;
using TMPro;

namespace Bark;

public static class ModCommand
{
    internal static void RegisterCommands()
    {
        ConsoleCommandRegistry.Register(
            "catfcabl",
            BetterLocale.GetCommand("catfcabl"),
            _ => ExportLocaleDebugFile()
        );

        ConsoleCommandRegistry.Register(
            "sr",
            LocaleCommand("reload"),
            _ => ReloadScripts()
        );

        ConsoleCommandRegistry.Register(
            "script",
            LocaleCommand("description"),
            ExecuteScriptCommand,
            BuildScriptAutofill()
        );

        ConsoleCommandRegistry.Register(
            "basp",
            LocaleCommand("help.spawn"),
            BarkSpawn,
            BuildBarkSpawnAutofill()
        );

        ConsoleCommandRegistry.Register(
            "bast",
            LocaleCommand("help.tile"),
            BarkPlaceTile,
            BuildBarkTileAutofill()
        );

        ConsoleCommandRegistry.Register(
            "basm",
            LocaleCommand("help.moodle"),
            BarkApplyMoodle,
            BuildBarkMoodleAutofill()
        );

        ConsoleCommandRegistry.Register(
            "scd",
            LocaleCommand("help.detail"),
            args => PrintDetail(args, 1),
            BuildScriptDetailAutofill()
        );
    }

    private static Dictionary<int, List<string>> BuildScriptAutofill()
    {
        return new Dictionary<int, List<string>>
        {
            { 0, ["help", "reload", "list", "spawn", "tile", "moodle", "detail"] },
            { 1, [] }
        };
    }

    private static Dictionary<int, List<string>> BuildScriptDetailAutofill()
    {
        return new Dictionary<int, List<string>>
        {
            { 0, [.. ScriptModLoader.ListMods().Select(m => m.Id)] }
        };
    }

    public static void SyncScriptAutofill(string sub)
    {
        var command = ConsoleScript.SearchExact("script");

        command?.argAutofill?[1] = sub switch
        {
            "spawn" => [.. ScriptModLoader.Items],
            "tile" => [.. ScriptModLoader.Tiles],
            "moodle" => [.. ScriptModLoader.Moodles],
            "detail" => [.. ScriptModLoader.ListMods().Select(m => m.Id)],
            _ => []
        };
    }

    private static Dictionary<int, List<string>> BuildBarkSpawnAutofill()
    {
        return new Dictionary<int, List<string>>
        {
            { 0, [.. ScriptModLoader.Items] }
        };
    }

    private static Dictionary<int, List<string>> BuildBarkTileAutofill()
    {
        return new Dictionary<int, List<string>>
        {
            { 0, [.. ScriptModLoader.Tiles] }
        };
    }

    private static Dictionary<int, List<string>> BuildBarkMoodleAutofill()
    {
        return new Dictionary<int, List<string>>
        {
            { 0, [.. ScriptModLoader.Moodles] }
        };
    }

    private static void RefreshSpawnAutofill()
    {
        RefreshCommandAutofill("script", BuildScriptAutofill());
        RefreshCommandAutofill("basp", BuildBarkSpawnAutofill());
        RefreshCommandAutofill("bast", BuildBarkTileAutofill());
        RefreshCommandAutofill("basm", BuildBarkMoodleAutofill());
        RefreshCommandAutofill("scd", BuildScriptDetailAutofill());
    }

    private static void RefreshCommandAutofill(string name,
        Dictionary<int, List<string>> autofill)
    {
        var command = ConsoleScript.SearchExact(name);
        if (command?.argAutofill == null) return;
        foreach (var (index, list) in autofill)
            command.argAutofill[index] = list;
    }

    private static void ExportLocaleDebugFile()
    {
        var path = Path.Combine(Plugin.BarkCachePath, "catfcabl.txt");
        var lines = new List<string> { $"Register ({BetterLocale.LocaleKeys.Count}):" };

        lines.AddRange(BetterLocale.LocaleKeys
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}: {x.Value}"));
        lines.Add("");
        lines.Add($"Call ({BetterLocale.LocaleGetKeys.Count}):");
        lines.AddRange(BetterLocale.LocaleGetKeys
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}: {x.Value}"));

        File.WriteAllLines(path, lines);
        LogUtil.Message($"catfcabl.txt: {path}", Plugin.Logger);
        LogUtil.Message($"Register Count: {BetterLocale.LocaleKeys.Count}",
            Plugin.Logger);
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
                CUCoreUtils.ConsoleRunCommand(ConsoleScript.instance, "sr");
                break;
            case "list":
                PrintList();
                break;
            case "spawn":
                BarkSpawn(args, 2);
                break;
            case "tile":
                BarkPlaceTile(args, 2);
                break;
            case "moodle":
                BarkApplyMoodle(args, 2);
                break;
            case "detail":
                PrintDetail(args, 2);
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
            ("reload", LocaleCommand("help.reload")),
            ("list", LocaleCommand("help.list")),
            ("spawn", LocaleCommand("help.spawn")),
            ("tile", LocaleCommand("help.tile")),
            ("moodle", LocaleCommand("help.moodle")),
            ("detail", LocaleCommand("help.detail"))
        };

        var header = LocaleCommand("help.header");
        LogUtil.PrintKeyValueList(header, helpItems, Plugin.Logger);
    }

    private static void PrintList()
    {
        if (Plugin._scriptModLoader == null) return;

        var mods = ScriptModLoader.ListMods();
        if (mods.Count == 0)
        {
            LogUtil.Info("script_mod_loader.no_mods");
            return;
        }

        InfoCommand("list.header", mods.Count);
        foreach (var mod in mods)
            InfoCommand("list.item", mod.Name, mod.Version,
                GetLanguageLabel(mod.Language), mod.Id);
    }

    private static void PrintDetail(string[] args, int skip)
    {
        if (args.Length <= skip)
        {
            InfoCommand("detail.usage");
            return;
        }

        var modId = args[skip];
        var mod = ScriptModLoader.ListMods().FirstOrDefault(m => m.Id == modId);
        if (mod is null)
        {
            InfoCommand("detail.not_found", modId);
            return;
        }

        var itemCount = ItemLoader.LoadedItems.TryGetValue(modId, out var items)
            ? items.Count
            : 0;
        var tileCount = TileLoader.LoadedTiles.TryGetValue(modId, out var tiles)
            ? tiles.Count
            : 0;
        var recipeCount = RecipeLoader.LoadedRecipes.TryGetValue(modId, out var recipes)
            ? recipes.Count
            : 0;
        var moodleCount = MoodleLoader.LoadedMoodles.TryGetValue(modId, out var moodles)
            ? moodles.Count
            : 0;

        InfoCommand("detail.header", mod.Name, mod.Version,
            GetLanguageLabel(mod.Language));

        var authorText = mod.Author.Count == 0
            ? LocaleCommand("detail.none")
            : string.Join(", ", mod.Author.Select(kv => $"{kv.Key}: {kv.Value}"));
        InfoCommand("detail.author", authorText);
        InfoCommand("detail.description", string.IsNullOrEmpty(mod.Description)
            ? LocaleCommand("detail.none")
            : mod.Description);
        InfoCommand("detail.bark_version", string.IsNullOrEmpty(mod.BarkVersion)
            ? LocaleCommand("detail.none")
            : mod.BarkVersion);
        InfoCommand("detail.game_version", string.IsNullOrEmpty(mod.GameVersion)
            ? LocaleCommand("detail.none")
            : mod.GameVersion);
        InfoCommand("detail.repository", mod.Repository ?? LocaleCommand("detail.none"));

        InfoCommand("detail.dependencies");
        if (mod.Dependencies.Count == 0)
            InfoCommand("detail.none");
        else
            foreach (var dep in mod.Dependencies)
                InfoCommand("detail.dep_item", dep.Id, dep.Version);

        InfoCommand("detail.items", itemCount);
        InfoCommand("detail.tiles", tileCount);
        InfoCommand("detail.recipes", recipeCount);
        InfoCommand("detail.moodles", moodleCount);
    }

    private static string GetLanguageLabel(ScriptLanguage language)
    {
        var suffix = language switch
        {
            ScriptLanguage.None => "none",
            ScriptLanguage.JavaScript => "javascript",
            ScriptLanguage.Lua => "lua",
            _ => "none"
        };
        return BetterLocale.GetCommand(
            $"{Plugin.NameSpace}.script.list.language.{suffix}");
    }

    private static void ReloadScripts()
    {
        Plugin._scriptModLoader?.ReloadAll();
        RefreshSpawnAutofill();
        InfoCommand("reload.completed");

        ScriptFileSync.TriggerSync();
    }

    private static void BarkSpawn(string[] args, int skip)
    {
        if (args.Length <= skip)
        {
            InfoCommand("spawn.usage");
            return;
        }

        var cuspawnArgs = string.Join(" ", args, skip, args.Length - skip);
        RunCommandRaw($"cuspawn {cuspawnArgs}");
    }

    private static void RunCommandRaw(string command)
    {
        var instance = ConsoleScript.instance;
        if (instance == null) return;
        CUCoreUtils.InvokeMethod(instance, "ExecuteCommand", command);
    }

    private static void BarkSpawn(string[] args)
    {
        BarkSpawn(args, 1);
    }

    private static void BarkPlaceTile(string[] args, int skip)
    {
        if (args.Length <= skip)
        {
            InfoCommand("tile.usage");
            return;
        }

        var tileId = args[skip];
        ushort index;
        try
        {
            index = TileUtil.ResolveIndex(tileId);
        }
        catch (Exception)
        {
            return;
        }

        var settileArgs = args.Length > skip + 1
            ? string.Join(" ", args, skip + 1, args.Length - skip - 1)
            : string.Empty;
        var command = string.IsNullOrEmpty(settileArgs)
            ? $"settile {index}"
            : $"settile {index} {settileArgs}";
        RunCommandRaw(command);
    }

    private static void BarkPlaceTile(string[] args)
    {
        BarkPlaceTile(args, 1);
    }

    private static void BarkApplyMoodle(string[] args, int skip)
    {
        if (args.Length <= skip)
        {
            InfoCommand("moodle.usage");
            return;
        }

        var moodleKey = args[skip];
        var holdSeconds = 0f;
        if (args.Length > skip + 1 && !float.TryParse(args[skip + 1], out holdSeconds))
        {
            LogUtil.Warning("script.moodle.invalid_hold", args[skip + 1]);
            return;
        }

        MoodleUtil.ApplyMoodle(moodleKey, holdSeconds);
    }

    private static void BarkApplyMoodle(string[] args)
    {
        BarkApplyMoodle(args, 1);
    }

    private static void InfoCommand(string key, params object[] args)
    {
        LogUtil.Info($"script.{key}", args);
    }

    private static string LocaleCommand(string key, params object[] args)
    {
        return BetterLocale.GetCommand($"{Plugin.NameSpace}.script.{key}", args);
    }


    private static readonly FieldInfo? InputField = typeof(ConsoleScript)
        .GetField("input", BindingFlags.Instance | BindingFlags.NonPublic);

    [HarmonyPatch(typeof(ConsoleScript), "Update")]
    public static void ConsoleScriptUpdatePrefix(ConsoleScript __instance)
    {
        var inputField = InputField?.GetValue(__instance);
        if (inputField is null)
            return;

        var text = inputField switch
        {
            TMP_InputField tmp => tmp.text,
            _ => inputField.GetType().GetProperty("text")?.GetValue(inputField) as string
        };
        if (string.IsNullOrEmpty(text))
            return;

        var args = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (args.Length < 2
            || !string.Equals(args[0], "script", StringComparison.OrdinalIgnoreCase))
            return;

        SyncScriptAutofill(args[1]);
    }
}
