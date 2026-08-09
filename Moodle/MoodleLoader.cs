using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Bark.Script;
using Bark.Tool;
using Newtonsoft.Json;
using UnityEngine;

namespace Bark.Moodle;

// 已加载 Moodle 的记录项
public class MoodleEntry(string key, string fileName)
{
    // 来源文件名（如 "bleeding.json"）
    public string FileName = fileName;

    // Moodle key
    public string Key = key;
}

// 自定义 Moodle 加载器：递归扫描 ModDir/Moodle/**/*.json，加载精灵图并缓存定义。
// 注册阶段不调用 MoodleRegistry.AddMoodle（那会直接应用状态到玩家），
// 仅在 MoodleUtil.ApplyMoodle 被脚本显式调用时才真正应用状态。
// 脚本映射分两阶段：RegisterFromMod 先注册 Moodle 与暂存脚本定义，
// RegisterScripts 在引擎就绪后写入 MoodleScriptRegistry。
// Moodle key 优先用 JSON 内 key 字段，否则由文件名生成；精灵图从 Assets/Moodle/ 平铺读取。
public static class MoodleLoader
{
    // 模组已加载的 Moodle 列表（modId → moodle 记录）
    public static readonly Dictionary<string, List<MoodleEntry>> LoadedMoodles = new();

    // 已加载的 Moodle 定义：moodleKey → MoodleDef，供 MoodleUtil 查询属性
    internal static readonly Dictionary<string, MoodleDef> LoadedMoodleDefs = new();

    // 已加载的 Moodle 精灵图缓存：moodleKey → Sprite，供 MoodleUtil.ApplyMoodle 复用
    internal static readonly Dictionary<string, Sprite> LoadedMoodleSprites = new();

    // 暂存的脚本映射（modId → moodleKey → (scriptDef, modDir)），待引擎创建后注册
    private static readonly Dictionary<string, Dictionary<string, (MoodleScriptDef def, string modDir)>>
        PendingScripts =
            new();

    // 用于将 Moodle 名称 snake_case 化
    private static readonly Regex SnakeCaseSanitizer = new("[^a-z0-9_]", RegexOptions.Compiled);

    public static void RegisterFromMod(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        RegisterFromDirectory(manifest.Id, manifest.Directory);
    }

    // 从任意模组目录加载所有自定义 Moodle，供脚本模组与 C# 模组共用。
    // modId    - Moodle 所有权标记（通常取 mod.json 的 id）
    // modDir   - 模组根目录，递归扫描 {modDir}/Moodle/**/*.json，资产目录为 {modDir}/Assets/Moodle/
    // allowPendingScripts - 是否允许暂存脚本映射待引擎绑定。脚本模组传 true；C# 模组传 false。
    public static int RegisterFromDirectory(string modId, string modDir, bool allowPendingScripts = true)
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

        var jsonFiles = Directory.GetFiles(moodleDir, "*.json", SearchOption.AllDirectories);
        if (jsonFiles.Length == 0)
            return 0;

        // 资产目录：ModDir/Assets/Moodle/
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

    // 清除指定模组此前注册的所有 Moodle（C# 端热重载 / 卸载时调用）
    public static void UnregisterOwner(string modId)
    {
        if (modId is null)
            throw new ArgumentNullException(nameof(modId));
        LoadedMoodles.Remove(modId);
        PendingScripts.Remove(modId);
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
        var key = BuildMoodleKey(def, jsonFile);

        // 加载精灵图资源并缓存（供运行时 ApplyMoodle 使用），但不调用 MoodleRegistry.AddMoodle。
        // 注册阶段仅存储定义，状态的真正应用由 MoodleUtil.ApplyMoodle 在脚本调用时完成。
        if (def.Animated && !string.IsNullOrWhiteSpace(def.AnimationId))
        {
            // 动画 Moodle：animation_id 由游戏运行时解析，无需预加载
        }
        else if (!string.IsNullOrWhiteSpace(def.IconId))
        {
            // 内置图标 ID：由游戏运行时解析，无需预加载
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
            // 未指定图标来源，自动查找 Assets/Moodle/{key}.png
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
    private static string BuildMoodleKey(MoodleDef def, string jsonFile)
    {
        if (!string.IsNullOrWhiteSpace(def.Key))
            return def.Key.Trim().ToLowerInvariant();

        var baseName = Path.GetFileNameWithoutExtension(jsonFile);
        var sanitized = SnakeCaseSanitizer.Replace(baseName, "_");
        // 合并连续下划线
        sanitized = Regex.Replace(sanitized, "_+", "_");
        sanitized = sanitized.Trim('_').ToLowerInvariant();
        return sanitized;
    }
}