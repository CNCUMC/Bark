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
using BepInEx;
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

        // scd = script detail 的别名，索引 0 直接补全模组 ID（按类型独立，不与内容 ID 混用）
        ConsoleCommandRegistry.Register(
            "scd",
            LocaleCommand("help.detail"),
            args => PrintDetail(args, 1),
            BuildScriptDetailAutofill()
        );
    }

    // 构造 script 命令的自动补全：
    //   索引 0 = 子命令词（help/reload/list/spawn/tile/moodle/detail）
    //   索引 1 = 候选列表，由 SyncScriptAutofill 按当前子命令（args[1]）动态切换，
    //            使 script spawn 只补物品、script tile 只补物块、script moodle 只补状态、
    //            script detail 只补模组 ID，而不是把所有 ID 混在一起。
    // 注：CCL 的 argAutofill 按参数位置索引补全、本身无法根据子命令切换候选，
    //     因此由 ConsoleAutofillPatch 在补全前实时调用 SyncScriptAutofill 改写索引 1。
    private static Dictionary<int, List<string>> BuildScriptAutofill()
    {
        return new Dictionary<int, List<string>>
        {
            { 0, ["help", "reload", "list", "spawn", "tile", "moodle", "detail"] },
            { 1, [] }
        };
    }

    // 构造 scd（script detail 别名）命令的自动补全：索引 0 = 已加载的模组 ID
    private static Dictionary<int, List<string>> BuildScriptDetailAutofill()
    {
        return new Dictionary<int, List<string>>
        {
            { 0, ScriptModLoader.ListMods().Select(m => m.Id).ToList() }
        };
    }

    // 按子命令切换 script 命令的索引 1 候选列表（供 ConsoleAutofillPatch 在补全前调用）。
    // sub 为用户输入的子命令词；未知子命令回退为空列表（这些子命令无第二参数）。
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

    // 刷新 script/basp/bast/basm/scd 的补全名单（重载物品后调用）
    private static void RefreshSpawnAutofill()
    {
        RefreshCommandAutofill("script", BuildScriptAutofill());
        RefreshCommandAutofill("basp", BuildBarkSpawnAutofill());
        RefreshCommandAutofill("bast", BuildBarkTileAutofill());
        RefreshCommandAutofill("basm", BuildBarkMoodleAutofill());
        RefreshCommandAutofill("scd", BuildScriptDetailAutofill());
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
            LogUtil.Info("script_mod_loader.no_mods", Plugin.Logger);
            return;
        }

        MessageCommand("list.header", mods.Count);
        foreach (var mod in mods)
            MessageCommand("list.item", mod.Name, mod.Version, GetLanguageLabel(mod.Language), mod.Id);
    }

    // 查询单个脚本模组注册的内容统计：物品 / 物块 / 配方 / 状态数量，以及脚本语言。
    // 数据取自各 Loader 的 Loaded* 字典（按 modId 过滤），与脚本 reload 实时一致。
    // skip = 需跳过的命令前缀 token 数（script detail = 2，scd = 1）。
    private static void PrintDetail(string[] args, int skip)
    {
        if (args.Length <= skip)
        {
            LogUtil.Info("script.detail.usage", Plugin.Logger);
            return;
        }

        var modId = args[skip];
        var mod = ScriptModLoader.ListMods().FirstOrDefault(m => m.Id == modId);
        if (mod is null)
        {
            LogUtil.Info("script.detail.not_found", modId);
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

        MessageCommand("detail.header", mod.Name, mod.Version, GetLanguageLabel(mod.Language));

        // 元数据（mod.json 字段）
        var authorText = mod.Author.Count == 0
            ? LocaleCommand("detail.none")
            : string.Join(", ", mod.Author.Select(kv => $"{kv.Key}: {kv.Value}"));
        MessageCommand("detail.author", authorText);
        MessageCommand("detail.description", string.IsNullOrEmpty(mod.Description)
            ? LocaleCommand("detail.none")
            : mod.Description);
        MessageCommand("detail.bark_version", string.IsNullOrEmpty(mod.BarkVersion)
            ? LocaleCommand("detail.none")
            : mod.BarkVersion);
        MessageCommand("detail.game_version", string.IsNullOrEmpty(mod.GameVersion)
            ? LocaleCommand("detail.none")
            : mod.GameVersion);
        MessageCommand("detail.repository", mod.Repository ?? LocaleCommand("detail.none"));

        // 依赖列表
        MessageCommand("detail.dependencies");
        if (mod.Dependencies.Count == 0)
            MessageCommand("detail.none");
        else
            foreach (var dep in mod.Dependencies)
                MessageCommand("detail.dep_item", dep.Id, dep.Version);

        // 注册内容统计
        MessageCommand("detail.items", itemCount);
        MessageCommand("detail.tiles", tileCount);
        MessageCommand("detail.recipes", recipeCount);
        MessageCommand("detail.moodles", moodleCount);
    }

    // 把脚本语言枚举转为列表显示的本地化标签（None 显示为 Data）
    private static string GetLanguageLabel(ScriptLanguage language)
    {
        var suffix = language switch
        {
            ScriptLanguage.None => "none",
            ScriptLanguage.JavaScript => "javascript",
            ScriptLanguage.Lua => "lua",
            _ => "none"
        };
        return BetterLocale.GetCommand($"{Plugin.NameSpace}.script.list.language.{suffix}");
    }

    private static void ReloadScripts()
    {
        Plugin._scriptModLoader?.ReloadAll();
        RefreshSpawnAutofill();
        MessageCommand("reload.completed");

        // 主机重载后，触发增量文件同步：把修改过的模组文件推给所有已连接客户端
        ScriptFileSync.TriggerSync();
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

        // 跳过前缀后剩余参数原样转发给 cuspawn。
        // 注意：不能用 CUCoreUtils.ConsoleRunCommand，它底层走 RunCommandString 会把 '_' 替换成空格，
        // 而物品 ID 形如 modid.itemid 含下划线，替换后无法被 CCL 正确识别。
        // 改为反射调用 ConsoleScript.ExecuteCommand（直接 Split，不替换下划线）。
        var cuspawnArgs = string.Join(" ", args, skip, args.Length - skip);
        RunCommandRaw($"cuspawn {cuspawnArgs}");
    }

    // 通过反射调用 ConsoleScript.ExecuteCommand（public 实例方法），
    // 直接按空格拆分参数执行，不会像 RunCommandString 那样把 '_' 替换成空格，
    // 从而保证 bark.itemid 这类含下划线的内容 ID 能被 CCL 正确解析。
    private static void RunCommandRaw(string command)
    {
        var instance = ConsoleScript.instance;
        if (instance == null) return;
        CUCoreUtils.InvokeMethod(instance, "ExecuteCommand", command);
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
        var command = string.IsNullOrEmpty(settileArgs)
            ? $"settile {index}"
            : $"settile {index} {settileArgs}";
        RunCommandRaw(command);
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

    // 让 script 命令的自动补全按子命令类型切换候选，而不是把全部 ID 混在一起。
    //
    // 背景：CCL 的 Command.argAutofill 是按参数位置索引（int）补全的，
    //      补全时只读取 command.argAutofill[key] 这个固定列表，无法根据 args[1] 的子命令值动态路由。
    //      因此 script spawn / script tile / script moodle / script detail 共用同一个索引 1 列表时，
    //      候选会混成一团（写 script tile 时也会补全物品/状态/脚本 ID）。
    //
    // 关键路径（来自 CCL ConsoleScript 反编译）：
    //   Update() 每帧调用 HandleDescriptionText(args)，候选下拉列表由 HandleDescriptionText 读取
    //   command.argAutofill[key] 展示；TryFinishCommandPart 仅在按 Tab 时调用（负责插入补全文本）。
    //   所以必须在本帧、在 HandleDescriptionText 读取之前改写 argAutofill[1]，候选下拉才能正确切换。
    //
    // 方案：Harmony patch ConsoleScript.Update 的开头，根据当前输入框文本中的子命令（args[1]）
    //      实时改写 script 命令的 argAutofill[1]，使补全候选与子命令类型严格对应。
    // 这也顺带解决了 Bark 内容 ID 含下划线的问题——补全候选来自 Bark 自有列表，不再经过
    // RunCommandString 的 '_'→' ' 替换。
    [HarmonyPatch(typeof(ConsoleScript))]
    [HarmonyPatch("Update")]
    private static class ConsoleAutofillPatch
    {
        // 缓存私有字段访问，避免每帧重复反射
        private static readonly FieldInfo? InputField = typeof(ConsoleScript)
            .GetField("input", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Prefix(ConsoleScript __instance)
        {
            if (InputField is null)
                return;

            // input 是 ConsoleScript 的私有 TMP_InputField 字段，反射读取当前输入文本
            var inputField = InputField.GetValue(__instance);
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
            if (args.Length < 2 || !string.Equals(args[0], "script", StringComparison.OrdinalIgnoreCase))
                return;

            SyncScriptAutofill(args[1]);
        }
    }
}