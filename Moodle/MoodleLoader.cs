using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Bark.Script;
using Bark.Tool;
using CUCoreLib.Registries;
using Newtonsoft.Json;
using UnityEngine;

namespace Bark.Moodle;

// 已加载 Moodle 的记录项
public class MoodleEntry(string key, string fileName)
{
    // Moodle key
    public string Key = key;

    // 来源文件名（如 "bleeding.json"）
    public string FileName = fileName;
}

// 自定义 Moodle 加载器：扫描 ModDir/Moodle/*.json，调用 MoodleRegistry 注册。
// MoodleRegistry 使用队列机制（到期自动消失），故无 BeginOwnerRegistration 模式。
// 热重载时直接重新注册即可（同 key 会覆盖旧队列条目）。
// 脚本映射分两阶段：RegisterFromMod 先注册 Moodle 与暂存脚本定义，
// RegisterScripts 在引擎就绪后写入 MoodleScriptRegistry。
public static class MoodleLoader
{
    // 模组已加载的 Moodle 列表（modId → moodle 记录）
    public static readonly Dictionary<string, List<MoodleEntry>> LoadedMoodles = new();

    // 已加载的 Moodle 定义：moodleKey → MoodleDef，供 MoodleUtil 查询属性
    internal static readonly Dictionary<string, MoodleDef> LoadedMoodleDefs = new();

    // 已加载的 Moodle 精灵图缓存：moodleKey → Sprite，供 MoodleUtil.ApplyMoodle 复用
    internal static readonly Dictionary<string, Sprite> LoadedMoodleSprites = new();

    // 暂存的脚本映射（modId → moodleKey → (scriptDef, modDir)），待引擎创建后注册
    private static readonly Dictionary<string, Dictionary<string, (MoodleScriptDef def, string modDir)>> PendingScripts =
        new();

    // 用于将 Moodle 名称 snake_case 化
    private static readonly Regex SnakeCaseSanitizer = new("[^a-z0-9_]", RegexOptions.Compiled);

    public static void RegisterFromMod(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        // 热重载时清除旧记录
        LoadedMoodles.Remove(manifest.Id);
        PendingScripts.Remove(manifest.Id);

        // 清除该模组的所有旧 MoodleDef（key 匹配的条目）
        // —— 用 key 前缀不够准确，这里直接依赖同 key 覆盖即可
        // LoadedMoodleDefs 在下方的 LoadAndRegister 中逐条覆盖

        var moodleDir = Path.Combine(manifest.Directory, "Moodle");
        if (!Directory.Exists(moodleDir))
            return;

        var jsonFiles = Directory.GetFiles(moodleDir, "*.json", SearchOption.TopDirectoryOnly);
        if (jsonFiles.Length == 0)
            return;

        // 资产目录：ModDir/Assets/Moodle/
        var assetsMoodleDir = Path.Combine(manifest.Directory, "Assets", "Moodle");

        var loadedList = new List<MoodleEntry>();
        var loadedCount = 0;

        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                var entry = LoadAndRegister(jsonFile, assetsMoodleDir, manifest.Id, manifest.Directory);
                if (entry == null) continue;
                loadedCount++;
                loadedList.Add(entry);
            }
            catch (Exception ex)
            {
                LogUtil.Error("moodle.load_error", jsonFile, manifest.Id, ex.Message);
            }
        }

        LoadedMoodles[manifest.Id] = loadedList;

        if (loadedCount > 0)
            LogUtil.Message("moodle.loaded_count", manifest.Id, loadedCount);
    }

    // 在引擎就绪后，将暂存的 Moodle 脚本映射写入 MoodleScriptRegistry
    public static void RegisterScripts(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));
        if (manifest.Engine is null)
            return;

        if (!PendingScripts.TryGetValue(manifest.Id, out var moodleScripts) || moodleScripts.Count == 0)
            return;

        foreach (var (moodleKey, (scriptDef, modDir)) in moodleScripts)
            MoodleScriptRegistry.Register(moodleKey, scriptDef, manifest.Engine, manifest.Id, modDir);

        var count = moodleScripts.Count;
        PendingScripts.Remove(manifest.Id);
        LogUtil.Info("moodle.scripts_registered", manifest.Id, count);
    }

    // 加载并注册单个 Moodle JSON，成功时返回记录项，失败返回 null
    private static MoodleEntry? LoadAndRegister(string jsonFile, string assetsDir, string modId, string modDir)
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

        // 生成 key：优先用定义中的 key，否则从文件名 + mod_id 自动生成
        var key = BuildMoodleKey(def, jsonFile, modId);

        // 根据图标来源调用不同的注册方法
        if (def.Animated && !string.IsNullOrWhiteSpace(def.AnimationId))
        {
            MoodleRegistry.AddAnimatedMoodle(
                def.Intensity,
                def.AnimationId,
                def.Name,
                def.Description,
                def.Critical,
                def.ChippedOnly,
                def.Important,
                key,
                def.HoldSeconds);
        }
        else if (!string.IsNullOrWhiteSpace(def.IconId))
        {
            MoodleRegistry.AddMoodle(
                def.Intensity,
                def.IconId,
                def.Name,
                def.Description,
                def.Critical,
                def.ChippedOnly,
                def.Important,
                key,
                def.HoldSeconds);
        }
        else if (!string.IsNullOrWhiteSpace(def.IconAsset))
        {
            // 自定义精灵图：Assets/Moodle/{icon_asset}
            var spritePath = Path.Combine(assetsDir, def.IconAsset);
            // 如果不是绝对路径，从模组 assets 拼接
            if (!Path.IsPathRooted(def.IconAsset))
                spritePath = Path.Combine(assetsDir, def.IconAsset);

            var sprite = ItemUtil.LoadSprite(spritePath, def.SpriteScale);
            if (sprite != null)
            {
                // 缓存精灵图供运行时 ApplyMoodle 复用
                LoadedMoodleSprites[key] = sprite;

                MoodleRegistry.AddMoodle(
                    def.Intensity,
                    sprite,
                    def.Name,
                    def.Description,
                    def.Critical,
                    def.ChippedOnly,
                    def.Important,
                    key,
                    def.HoldSeconds);
            }
            else
            {
                // 精灵加载失败，降级使用 icon_id（如果提供）或跳过
                LogUtil.Warning("moodle.sprite_load_failed", spritePath, key);
            }
        }
        else
        {
            // 未指定图标来源，自动查找 Assets/Moodle/{key}.png
            var autoSpritePath = Path.Combine(assetsDir, key + ".png");
            var autoSprite = ItemUtil.LoadSprite(autoSpritePath, def.SpriteScale);
            if (autoSprite != null)
            {
                // 缓存精灵图供运行时 ApplyMoodle 复用
                LoadedMoodleSprites[key] = autoSprite;

                MoodleRegistry.AddMoodle(
                    def.Intensity,
                    autoSprite,
                    def.Name,
                    def.Description,
                    def.Critical,
                    def.ChippedOnly,
                    def.Important,
                    key,
                    def.HoldSeconds);
            }
            else
            {
                LogUtil.Warning("moodle.no_icon", jsonFile);
                return null;
            }
        }

        // 暂存脚本映射（如有），待引擎就绪后由 RegisterScripts 写入 MoodleScriptRegistry
        StashScript(key, def.Script, modId, modDir);

        // 存储 MoodleDef 供 MoodleUtil 查询属性
        LoadedMoodleDefs[key] = def;

        LogUtil.Info("moodle.registered", key, modId);

        var fileName = Path.GetFileName(jsonFile);
        return new MoodleEntry(key, fileName);
    }

    // 暂存 Moodle 脚本映射，待引擎就绪后注册
    private static void StashScript(string moodleKey, MoodleScriptDef? scriptDef, string modId, string modDir)
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

    // 构建 Moodle key：优先用 def.Key，否则用文件名（去扩展名、snake_case 化）
    private static string BuildMoodleKey(MoodleDef def, string jsonFile, string modId)
    {
        if (!string.IsNullOrWhiteSpace(def.Key))
            return def.Key.Trim().ToLowerInvariant();

        var baseName = Path.GetFileNameWithoutExtension(jsonFile);
        var sanitized = SnakeCaseSanitizer.Replace(baseName, "_");
        // 合并连续下划线
        sanitized = Regex.Replace(sanitized, @"_+", "_");
        sanitized = sanitized.Trim('_').ToLowerInvariant();
        return sanitized;
    }
}