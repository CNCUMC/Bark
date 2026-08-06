using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bark.BetterCCL;
using Bark.Items;
using Bark.Moodle;
using Bark.Script;
using Bark.Tile;
using Bark.Tool;
using BepInEx;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;

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
    }

    // 构造 script 命令的自动补全：
    //   索引 0 = 子命令词（help/reload/list/spawn/tile/moodle）
    //   索引 1 = Bark 注册的全部内容 ID（供 spawn/tile/moodle 子命令补全）
    private static Dictionary<int, List<string>> BuildScriptAutofill()
    {
        return new Dictionary<int, List<string>>
        {
            { 0, ["help", "reload", "list", "spawn", "tile", "moodle"] },
            { 1, GetRegisteredSpawnIds() }
        };
    }

    // 构造 basp 命令的自动补全：索引 0 = Bark 注册的全部内容 ID（同 CCL cuspawn）
    private static Dictionary<int, List<string>> BuildBarkSpawnAutofill()
    {
        return new Dictionary<int, List<string>>
        {
            { 0, [.. ScriptModLoader.Items] }
        };
    }

    // 构造 bast 命令的自动补全：索引 0 = Bark 注册的物块 ID
    private static Dictionary<int, List<string>> BuildBarkTileAutofill()
    {
        return new Dictionary<int, List<string>>
        {
            { 0, [.. ScriptModLoader.Tiles] }
        };
    }

    // 构造 basm 命令的自动补全：索引 0 = Bark 注册的 Moodle key
    private static Dictionary<int, List<string>> BuildBarkMoodleAutofill()
    {
        return new Dictionary<int, List<string>>
        {
            { 0, [.. ScriptModLoader.Moodles] }
        };
    }

    // 收集 Bark 注册的内容 ID（物品/物块/配方/Moodle），用于 spawn 子命令的自动补全。
    // 统一取自 ScriptModLoader.GetRegisteredContentIds()，只包含 Bark 添加的内容，
    // 不会混入 CCL 原版或其他模组的物品。
    private static List<string> GetRegisteredSpawnIds()
    {
        return [.. ScriptModLoader.GetRegisteredContentIds()];
    }

    // 刷新 script/basp/bast/basm 的补全名单（重载物品后调用）
    private static void RefreshSpawnAutofill()
    {
        RefreshCommandAutofill("script", BuildScriptAutofill());
        RefreshCommandAutofill("basp", BuildBarkSpawnAutofill());
        RefreshCommandAutofill("bast", BuildBarkTileAutofill());
        RefreshCommandAutofill("basm", BuildBarkMoodleAutofill());
    }

    private static void RefreshCommandAutofill(string name, Dictionary<int, List<string>> autofill)
    {
        var command = ConsoleScript.SearchExact(name);
        if (command?.argAutofill == null) return;
        foreach (var (index, list) in autofill)
            command.argAutofill[index] = list;
    }

    private static void ExportLocaleDebugFile()
    {
        var path = Path.Combine(Paths.CachePath, "catfcabl.txt");
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
            ("moodle", LocaleCommand("help.moodle"))
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
            LogUtil.Info("script_mod_loader.no_mods", Plugin.Logger);
            return;
        }

        MessageCommand("list.header", mods.Count);
        foreach (var mod in mods)
            MessageCommand("list.item", mod.Name, mod.Version, mod.Language, mod.Id);
    }

    private static void ReloadScripts()
    {
        Plugin._scriptModLoader?.ReloadAll();
        RefreshSpawnAutofill();
        MessageCommand("reload.completed");
    }

    // 生成 Bark 添加的内容：转发为 CCL 的 cuspawn 命令字符串，
    // 由 RunCommandString 按 [id] [position] [condition] [count] 顺序解析。
    // 物品 ID 形如 modid.itemid，正是 CCL 注册表中可识别的 ID。
    // skip = 需跳过的命令前缀 token 数（basp=1，script spawn=2）。
    private static void BarkSpawn(string[] args, int skip)
    {
        if (args.Length <= skip)
        {
            LogUtil.Info("script.spawn.usage", Plugin.Logger);
            return;
        }

        // 跳过前缀后剩余参数原样转发给 cuspawn
        var cuspawnArgs = string.Join(" ", args, skip, args.Length - skip);
        CUCoreUtils.ConsoleRunCommand(ConsoleScript.instance, $"cuspawn {cuspawnArgs}");
    }

    // basp 命令入口：args[0]="basp"，直接接物品参数
    private static void BarkSpawn(string[] args)
    {
        BarkSpawn(args, 1);
    }

    // 放置 Bark 注册的物块：先把字符串物块 ID 转为 CCL 的 tile 索引，
    // 再转发为 CCL 的 settile 命令字符串（由 RunCommandString 按 [tileIndex] [position] 解析）。
    // 字符串→索引的转换复用 TileUtil.ResolveIndex（查找 TileRegistry.RegisteredDefinitionIds）。
    // skip = 需跳过的命令前缀 token 数（bast=1，script tile=2）。
    private static void BarkPlaceTile(string[] args, int skip)
    {
        if (args.Length <= skip)
        {
            LogUtil.Info("script.tile.usage", Plugin.Logger);
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
            // TileUtil.ResolveIndex 找不到时会记警告，这里直接返回，不转发
            return;
        }

        // 剩余参数（[position]）原样转发给 settile
        var settileArgs = args.Length > skip + 1
            ? string.Join(" ", args, skip + 1, args.Length - skip - 1)
            : string.Empty;
        var command = string.IsNullOrEmpty(settileArgs) ? $"settile {index}" : $"settile {index} {settileArgs}";
        CUCoreUtils.ConsoleRunCommand(ConsoleScript.instance, command);
    }

    // bast 命令入口：args[0]="bast"，直接接物块参数
    private static void BarkPlaceTile(string[] args)
    {
        BarkPlaceTile(args, 1);
    }

    // 应用 Bark 注册的 Moodle：CCL 无对应命令，直接调用 MoodleUtil.ApplyMoodle。
    // 参数 [moodleKey] [holdSeconds]，holdSeconds 可选（缺省用 JSON 定义的持续时间）。
    // skip = 需跳过的命令前缀 token 数（basm=1，script moodle=2）。
    private static void BarkApplyMoodle(string[] args, int skip)
    {
        if (args.Length <= skip)
        {
            LogUtil.Info("script.moodle.usage", Plugin.Logger);
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

    // basm 命令入口：args[0]="basm"，直接接 moodle 参数
    private static void BarkApplyMoodle(string[] args)
    {
        BarkApplyMoodle(args, 1);
    }

    private static void MessageCommand(string key, params object[] args)
    {
        LogUtil.Message($"script.{key}", args);
    }

    private static string LocaleCommand(string key, params object[] args)
    {
        return BetterLocale.GetCommand($"{Plugin.NameSpace}.script.{key}", args);
    }
}