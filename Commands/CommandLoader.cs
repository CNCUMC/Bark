using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bark.Events;
using Bark.Script;
using Bark.Tool;
using CUCoreLib.Registries;
using Newtonsoft.Json;

namespace Bark.Commands;

// 脚本命令加载器：递归扫描 ModDir/Command/**/*.json，注册到 ConsoleCommandRegistry，
// 输入命令时通过 EventUtil.Trigger 分发到事件总线，所有脚本引擎均可接收 onCommand。
// 命令名由文件名决定（不拼接子目录路径）。
public static class CommandLoader
{
    // 所有已注册的命令追踪（供调试/重载用）
    public static readonly Dictionary<string, CommandEntry> RegisteredCommands = new();

    // 暂存的命令定义：modId → 命令名 → (manifest, def)，待引擎就绪后注册
    private static readonly Dictionary<string, Dictionary<string, (ScriptManifest manifest, CommandDef def)>>
        PendingCommands = new();

    // 从模组目录扫描并暂存命令定义（引擎尚未就绪，暂不注册到 ConsoleCommandRegistry）
    public static void RegisterFromMod(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        var commandsDir = Path.Combine(manifest.Directory, "Command");
        if (!Directory.Exists(commandsDir))
            return;

        var jsonFiles = Directory.GetFiles(commandsDir, "*.json", SearchOption.AllDirectories);
        if (jsonFiles.Length == 0)
            return;

        var modCommands = new Dictionary<string, (ScriptManifest, CommandDef)>();
        var loadedCount = 0;

        foreach (var jsonFile in jsonFiles)
        {
            var commandName = Path.GetFileNameWithoutExtension(jsonFile);

            if (commandName.Contains(' '))
            {
                LogUtil.Warning("command.name_has_spaces", jsonFile, commandName);
                continue;
            }

            CommandDef? def;
            try
            {
                def = JsonUtil.ReadFile<CommandDef>(jsonFile);
            }
            catch (Exception ex)
            {
                LogUtil.Error("command.parse_failed", jsonFile, manifest.Id, ex.Message);
                continue;
            }

            if (def is null)
            {
                LogUtil.Warning("command.parse_failed", jsonFile, manifest.Id, "(null)");
                continue;
            }

            modCommands[commandName] = (manifest, def);
            loadedCount++;
        }

        if (modCommands.Count <= 0) return;
        PendingCommands[manifest.Id] = modCommands;
        LogUtil.Info("command.pending_count", manifest.Id, loadedCount);
    }

    // 引擎就绪后，将暂存的命令注册到 ConsoleCommandRegistry，通过事件系统分发给所有脚本
    public static void RegisterScripts(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        if (!PendingCommands.Remove(manifest.Id, out var modCommands) || modCommands.Count == 0)
            return;

        var registeredCount = 0;

        foreach (var (name, (_, def)) in modCommands)
        {
            // 构建参数自动完成
            var argAutofill = BuildArgAutofill(def);
            var argDescriptions = BuildArgDescriptions(def);

            ConsoleCommandRegistry.Register(
                name,
                def.Description ?? string.Empty,
                args =>
                {
                    EventUtil.Trigger(new CommandEvent
                    {
                        CommandName = name,
                        Args = [.. args]
                    });
                },
                argAutofill,
                argDescriptions);

            RegisteredCommands[name] = new CommandEntry(name, def.Description ?? string.Empty);
            registeredCount++;
        }

        if (registeredCount > 0)
            LogUtil.Info("command.scripts_registered", manifest.Id, registeredCount);
    }

    // 构建参数自动完成字典
    private static Dictionary<int, List<string>> BuildArgAutofill(CommandDef def)
    {
        var result = new Dictionary<int, List<string>>();
        if (def.Args is not { Length: > 0 })
            return result;

        for (var i = 0; i < def.Args.Length; i++)
        {
            var suggestions = def.Args[i].Suggestions;
            if (suggestions is { Length: > 0 })
                result[i + 1] = [.. suggestions]; // args[0] 为命令名本身
        }

        return result;
    }

    // 构建参数描述数组
    private static (string shortDesc, string longDesc)[] BuildArgDescriptions(CommandDef def)
    {
        if (def.Args is not { Length: > 0 })
            return [];

        return
        [
            .. def.Args
                .Select(a => (a.Name, a.Description ?? string.Empty))
        ];
    }
}

// 命令 JSON 定义（ModDir/Command/my_command.json）
public class CommandDef
{
    // 命令名由文件名决定（如 greet.json → 命令名 "greet"），无需 JSON 中声明

    // 帮助描述
    [JsonProperty("description")] public string? Description { get; set; }

    // 参数定义
    [JsonProperty("args")] public ArgDef[]? Args { get; set; }
}

// 命令参数定义
public class ArgDef
{
    // 参数名称（简短描述）
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;

    // 参数详细描述
    [JsonProperty("description")] public string? Description { get; set; }

    // 自动完成候选值
    [JsonProperty("suggestions")] public string[]? Suggestions { get; set; }
}

// 已注册命令的记录项
public class CommandEntry(string name, string description)
{
    // 命令描述
    public string Description = description;

    // 命令名称
    public string Name = name;
}