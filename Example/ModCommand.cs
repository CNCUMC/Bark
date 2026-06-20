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
            Locale("testhello_description"),
            _ => Info("testhello_text", TestHello()),
            null)
        );

        ConsoleScript.Commands.Add(new Command(
            "spawnblock",
            Locale("spawnblock_description"),
            args =>
            {
                World.CheckForWorld();
                Tools.CheckArgumentCount(args, 1);

                if (!ushort.TryParse(args[1], out var blockId))
                {
                    Log.Error(Locale("spawnblock_invalidblockid", args[1]), Logger);
                    return;
                }

                World.PlaceBlock(Key.MouseWorldPosition(), blockId);
                Log.Info(Locale("spawnblock_success", blockId), Logger);
            },
            new Dictionary<int, List<string>>
            {
                { 0, ["blockid"] }
            },
            ("blockid", Locale("spawnblock_blockid")))
        );

        ConsoleScript.Commands.Add(new Command(
            "spawnbackground",
            Locale("spawnbackground_description"),
            args =>
            {
                World.CheckForWorld();
                Tools.CheckArgumentCount(args, 1);

                var backgroundId = args[1];
                if (string.IsNullOrWhiteSpace(backgroundId))
                {
                    Log.Error(Locale("spawnbackground_invalidbackgroundid"), Logger);
                    return;
                }

                World.PlaceBackground(Key.MouseWorldPosition(), backgroundId);
                Log.Info(Locale("spawnbackground_success", backgroundId), Logger);
            },
            new Dictionary<int, List<string>>
            {
                { 0, ["backgroundid"] }
            },
            ("backgroundid", Locale("spawnbackground_backgroundid")))
        );

        ConsoleScript.Commands.Add(new Command(
            "listbackground",
            Locale("listbackground_description"),
            _ =>
            {
                World.CheckForWorld();

                var backgrounds = Resources.LoadAll<Sprite>("");
                var backgroundList = (from bg in backgrounds
                    where bg.name.EndsWith("Background", StringComparison.OrdinalIgnoreCase)
                    select bg.name).ToList();

                if (backgroundList.Count == 0)
                {
                    Log.Info(Locale("listbackground_none"), Logger);
                    return;
                }

                var message = string.Join("\n", backgroundList);
                Log.Info(Locale("listbackground_header", backgroundList.Count) + "\n" + message, Logger);
            },
            null)
        );
    }

    private static string TestHello()
    {
        var text = Locale("testhello_description");
        var result = "";
        for (var i = 0; i < text.Length; i++) result += RichText.Size(text[i].ToString(), (i + 3) * 9);

        return result;
    }

    private static string Locale(string key, params object[] args)
    {
        var fullKey = $"command_{key}";
        var text = LocaleRegistry.Get("other", fullKey, fullKey);
        return args.Length > 0 ? string.Format(text, args) : text;
    }

    private static void Info(string key, params object[] args)
    {
        var message = Locale(key, args);
        Log.Info(message, Logger);
    }
}
