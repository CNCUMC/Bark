using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Bark.Script;
using Bark.Tool;
using Newtonsoft.Json;
using UnityEngine;

namespace Bark.Moodle;

public class MoodleEntry(string key, string fileName)
{
    public string FileName = fileName;
    public string Key = key;
}

public static class MoodleLoader
{
    public static readonly Dictionary<string, List<MoodleEntry>> LoadedMoodles = new();
    internal static readonly Dictionary<string, MoodleDef> LoadedMoodleDefs = new();
    internal static readonly Dictionary<string, Sprite> LoadedMoodleSprites = new();

    private static readonly Dictionary<string,
            Dictionary<string, (MoodleScriptDef def, string modDir)>>
        PendingScripts =
            new();

    private static readonly Regex SnakeCaseSanitizer =
        new("[^a-z0-9_]", RegexOptions.Compiled);

    public static void RegisterFromMod(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        RegisterFromDirectory(manifest.Id, manifest.Directory);
    }

    public static int RegisterFromDirectory(string modId, string modDir,
        bool allowPendingScripts = true)
    {
        if (modId is null)
            throw new ArgumentNullException(nameof(modId));
        if (modDir is null)
            throw new ArgumentNullException(nameof(modDir));

        // 热重载时清除旧记录
        LoadedMoodles.Remove(modId);
        PendingScripts.Remove(modId);

        var moodleDir = Path.Combine(modDir, "Moodle");
        if (!Directory.Exists(moodleDir))
            return 0;

        var jsonFiles =
            Directory.GetFiles(moodleDir, "*.json", SearchOption.AllDirectories);
        if (jsonFiles.Length == 0)
            return 0;

        var assetsMoodleDir = Path.Combine(modDir, "Assets", "Moodle");

        var loadedList = new List<MoodleEntry>();
        var loadedCount = 0;

        foreach (var jsonFile in jsonFiles)
            try
            {
                var entry = LoadAndRegister(jsonFile, assetsMoodleDir, modId, modDir);
                if (entry == null) continue;
                loadedCount++;
                loadedList.Add(entry);
            }
            catch (Exception ex)
            {
                LogUtil.Error("moodle.load_error", jsonFile, modId, ex.Message);
            }

        LoadedMoodles[modId] = loadedList;

        if (PendingScripts.TryGetValue(modId, out var pending) && pending.Count > 0)
        {
            if (allowPendingScripts)
            {
                LogUtil.Info("moodle.scripts_pending", modId, pending.Count);
            }
            else
            {
                LogUtil.Warning("moodle.csharp.scripts_ignored", modId, pending.Count);
                PendingScripts.Remove(modId);
            }
        }

        if (loadedCount > 0)
            LogUtil.Message("moodle.loaded_count", modId, loadedCount);

        return loadedCount;
    }

    public static void UnregisterOwner(string modId)
    {
        if (modId is null)
            throw new ArgumentNullException(nameof(modId));
        LoadedMoodles.Remove(modId);
        PendingScripts.Remove(modId);
    }

    public static void RegisterScripts(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));
        if (manifest.Engine is null)
            return;

        if (!PendingScripts.TryGetValue(manifest.Id, out var moodleScripts)
            || moodleScripts.Count == 0)
            return;

        foreach (var (moodleKey, (scriptDef, modDir)) in moodleScripts)
            MoodleScriptRegistry.Register(moodleKey, scriptDef, manifest.Engine,
                manifest.Id, modDir);

        var count = moodleScripts.Count;
        PendingScripts.Remove(manifest.Id);
        LogUtil.Info("moodle.scripts_registered", manifest.Id, count);
    }

    private static MoodleEntry? LoadAndRegister(string jsonFile, string assetsDir,
        string modId, string modDir)
    {
        string json;
        try
        {
            json = File.ReadAllText(jsonFile);
        }
        catch (Exception ex)
        {
            LogUtil.Warning("moodle.read_failed", jsonFile, ex.Message);
            return null;
        }

        MoodleDef? def;
        try
        {
            def = JsonConvert.DeserializeObject<MoodleDef>(json);
        }
        catch (JsonException ex)
        {
            LogUtil.Warning("moodle.invalid_json", jsonFile, ex.Message);
            return null;
        }

        if (def is null)
            return null;

        if (string.IsNullOrWhiteSpace(def.Name))
        {
            LogUtil.Warning("moodle.missing_name", jsonFile);
            return null;
        }

        var key = BuildMoodleKey(def, jsonFile);

        if (def.Animated && !string.IsNullOrWhiteSpace(def.AnimationId))
        {
        }
        else if (!string.IsNullOrWhiteSpace(def.IconId))
        {
        }
        else if (!string.IsNullOrWhiteSpace(def.IconAsset))
        {
            // 自定义精灵图：Assets/Moodle/{icon_asset}
            var spritePath = Path.Combine(assetsDir, def.IconAsset);
            if (!Path.IsPathRooted(def.IconAsset))
                spritePath = Path.Combine(assetsDir, def.IconAsset);

            var sprite = ItemUtil.LoadSprite(spritePath, def.SpriteScale);
            if (sprite != null)
                LoadedMoodleSprites[key] = sprite;
            else
                LogUtil.Warning("moodle.sprite_load_failed", spritePath, key);
        }
        else
        {
            var autoSpritePath = Path.Combine(assetsDir, key + ".png");
            var autoSprite = ItemUtil.LoadSprite(autoSpritePath, def.SpriteScale);
            if (autoSprite != null)
            {
                LoadedMoodleSprites[key] = autoSprite;
            }
            else
            {
                LogUtil.Warning("moodle.no_icon", jsonFile);
                return null;
            }
        }

        StashScript(key, def.Script, modId, modDir);

        LoadedMoodleDefs[key] = def;

        LogUtil.Info("moodle.registered", key, modId);

        var fileName = Path.GetFileName(jsonFile);
        return new MoodleEntry(key, fileName);
    }

    private static void StashScript(string moodleKey, MoodleScriptDef? scriptDef,
        string modId, string modDir)
    {
        if (scriptDef is null) return;
        var isEmpty = scriptDef.Get.Count == 0
            && scriptDef.Iterate.Count == 0
            && scriptDef.Lose.Count == 0;
        if (isEmpty) return;

        if (!PendingScripts.TryGetValue(modId, out var moodleScripts))
        {
            moodleScripts = new Dictionary<string, (MoodleScriptDef, string)>();
            PendingScripts[modId] = moodleScripts;
        }

        moodleScripts[moodleKey] = (scriptDef, modDir);
    }

    private static string BuildMoodleKey(MoodleDef def, string jsonFile)
    {
        if (!string.IsNullOrWhiteSpace(def.Key))
            return def.Key.Trim().ToLowerInvariant();

        var baseName = Path.GetFileNameWithoutExtension(jsonFile);
        var sanitized = SnakeCaseSanitizer.Replace(baseName, "_");
        sanitized = Regex.Replace(sanitized, "_+", "_");
        sanitized = sanitized.Trim('_').ToLowerInvariant();
        return sanitized;
    }
}
