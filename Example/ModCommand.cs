using System;
using System.Collections.Generic;
using System.Linq;
using Bark.Tool;
using BepInEx.Logging;
using CUCoreLib.Registries;
using HarmonyLib;
using UnityEngine;

namespace Bark.Example;

[HarmonyPatch(typeof(ConsoleScript))]
public class ModCommand
{
    private static readonly ManualLogSource Logger = Plugin.Logger;

    [HarmonyPatch("RegisterAllCommands")]
    [HarmonyPostfix]
    public static void RegisterCustomCommands(ConsoleScript __instance)
    {
        ConsoleScript.Commands.Add(new Command(
            "testhello",
            Locale("command.test_hello.description"),
            _ => Info("command.test_hello.text", TestHello()),
            null)
        );

        ConsoleScript.Commands.Add(new Command(
            "spawnblock",
            Locale("command.spawn_block.description"),
            args =>
            {
                World.CheckForWorld();
                Tools.CheckArgumentCount(args, 1);

                if (!ushort.TryParse(args[1], out var blockId))
                {
                    Log.Error(Locale("command.spawn_block.invalid_block_id", args[1]), Logger);
                    return;
                }

                World.PlaceBlock(Key.MouseWorldPosition(), blockId);
                Log.Info(Locale("command.spawn_block.success", blockId), Logger);
            },
            new Dictionary<int, List<string>>
            {
                { 0, ["block_id"] }
            },
            ("block_id", Locale("command.spawn_block.block_id")))
        );

        ConsoleScript.Commands.Add(new Command(
            "spawnbackground",
            Locale("command.spawn_background.description"),
            args =>
            {
                World.CheckForWorld();
                Tools.CheckArgumentCount(args, 1);

                var backgroundId = args[1];
                if (string.IsNullOrWhiteSpace(backgroundId))
                {
                    Log.Error(Locale("command.spawn_background.invalid_background_id"), Logger);
                    return;
                }

                World.PlaceBackground(Key.MouseWorldPosition(), backgroundId);
                Log.Info(Locale("command.spawn_background.success", backgroundId), Logger);
            },
            new Dictionary<int, List<string>>
            {
                { 0, ["background_id"] }
            },
            ("background_id", Locale("command.spawn_background.background_id")))
        );

        ConsoleScript.Commands.Add(new Command(
            "listbackground",
            Locale("command.list_background.description"),
            _ =>
            {
                World.CheckForWorld();

                var backgrounds = Resources.LoadAll<Sprite>("");
                var backgroundList = (from bg in backgrounds
                    where bg.name.EndsWith("Background", StringComparison.OrdinalIgnoreCase)
                    select bg.name).ToList();

                if (backgroundList.Count == 0)
                {
                    Log.Info(Locale("command.list_background.none"), Logger);
                    return;
                }

                var message = string.Join("\n", backgroundList);
                Log.Info(Locale("command.list_background.header", backgroundList.Count) + "\n" + message, Logger);
            },
            null)
        );
    }

    private static string TestHello()
    {
        var text = Locale("command.test_hello.description");
        var result = "";
        for (var i = 0; i < text.Length; i++) result += RichText.Size(text[i].ToString(), (i + 3) * 9);

        return result;
    }

    private static string Locale(string key, params object[] args)
    {
        var text = LocaleRegistry.Get("other", key, key);
        return args.Length > 0 ? string.Format(text, args) : text;
    }

    private static void Info(string key, params object[] args)
    {
        var message = Locale(key, args);
        Log.Info(message, Logger);
    }
}
