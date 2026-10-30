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

public static class CommandLoader
{
    public static readonly Dictionary<string, CommandEntry> RegisteredCommands = new();

    private static readonly Dictionary<string, Dictionary<string, (ScriptManifest manifest, CommandDef def)>>
        PendingCommands = new();

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

    public static void RegisterScripts(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        if (!PendingCommands.Remove(manifest.Id, out var modCommands) || modCommands.Count == 0)
            return;

        var registeredCount = 0;

        foreach (var (name, (_, def)) in modCommands)
        {
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

    private static Dictionary<int, List<string>> BuildArgAutofill(CommandDef def)
    {
        var result = new Dictionary<int, List<string>>();
        if (def.Args is not { Length: > 0 })
            return result;

        for (var i = 0; i < def.Args.Length; i++)
        {
            var suggestions = def.Args[i].Suggestions;
            if (suggestions is { Length: > 0 })
                result[i + 1] = [.. suggestions];
        }

        return result;
    }

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

public class CommandDef
{
    [JsonProperty("description")] public string? Description { get; set; }
    [JsonProperty("args")] public ArgDef[]? Args { get; set; }
}

public class ArgDef
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("description")] public string? Description { get; set; }
    [JsonProperty("suggestions")] public string[]? Suggestions { get; set; }
}

public class CommandEntry(string name, string description)
{
    public string Description = description;
    public string Name = name;
}