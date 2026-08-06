using System;
using System.IO;
using Bark.Moodle;
using Bark.Recipe;
using Bark.Script;
using Bark.Tile;
using Bark.Tool;
using BepInEx;

namespace Bark.Items;

// C# 端模组内容加载 API。
// 让普通 BepInEx 模组（无脚本引擎）也能像脚本模组一样，通过 mod.json + 子目录 JSON 注册内容：
// 物品(Item)、物块(Tile)、配方(Recipe)、状态(Moodle)。
// 复用与各脚本端加载器完全相同的解析 / 资产加载 / 注册流程，但不绑定脚本引擎
// （JSON 中若有 script 字段，物品/物块/状态会跳过并输出警告）。
// 配置项请用 BetterCCL 的 BetterOptions；命令请用 CCL 的 ConsoleCommandRegistry 直接注册。
//
// 目录约定（与脚本模组一致）：
//   {modRoot}/mod.json            — 模组清单，至少含 "id"
//   {modRoot}/Item/*.json         — 物品
//   {modRoot}/Tile/*.json         — 物块
//   {modRoot}/Recipe/*.json       — 配方
//   {modRoot}/Moodle/*.json       — 状态
//   {modRoot}/Assets/{Item|Tile|Moodle}/*.png — 对应贴图
//
// 最简用法（mod.json 与 DLL 同目录）：
//   ModContentApi.LoadFromPluginDirectory(GetType().Assembly.Location);
public static class ModContentApi
{
    // mod.json 文件名（与脚本模组一致）
    private const string ManifestFileName = "mod.json";

    // 从 mod.json 加载全部支持的模组内容。
    // modJsonPath - mod.json 的完整路径，其所在目录即模组根目录
    // 返回：加载结果（含各内容数量）；文件缺失 / 解析失败 / 无 id 时返回空结果。
    public static LoadResult LoadFromManifest(string modJsonPath)
    {
        if (modJsonPath is null)
            throw new ArgumentNullException(nameof(modJsonPath));

        if (!File.Exists(modJsonPath))
        {
            LogUtil.Warning("mod_content.manifest_missing", modJsonPath);
            return new LoadResult();
        }

        var (modId, modDir) = ReadManifest(modJsonPath);
        if (modId is null || modDir is null)
            return new LoadResult();

        return LoadInternal(modId, modDir);
    }

    // 在插件 DLL 所在目录自动查找 mod.json 并加载全部内容。
    // assemblyLocation - 通常是 typeof(你的Plugin).Assembly.Location
    public static LoadResult LoadFromPluginDirectory(string assemblyLocation)
    {
        if (string.IsNullOrWhiteSpace(assemblyLocation))
            throw new ArgumentException("assemblyLocation must not be empty", nameof(assemblyLocation));

        var dir = Path.GetDirectoryName(assemblyLocation);
        if (string.IsNullOrEmpty(dir))
        {
            LogUtil.Warning("mod_content.assembly_dir_missing", assemblyLocation);
            return new LoadResult();
        }

        var manifestPath = Path.Combine(dir, ManifestFileName);
        return LoadFromManifest(manifestPath);
    }

    // 从 BepInEx 插件根目录下的 {modName} 子目录加载（自动查找该目录的 mod.json）。
    public static LoadResult LoadFromPlugins(string modName)
    {
        if (modName is null)
            throw new ArgumentNullException(nameof(modName));

        var dir = Path.Combine(Paths.PluginPath, modName);
        var manifestPath = Path.Combine(dir, ManifestFileName);
        return LoadFromManifest(manifestPath);
    }

    // 直接指定 modId 与模组根目录加载全部内容（不依赖 mod.json）。
    // 适用于不需要元数据、或在测试中显式控制的场景。
    public static LoadResult Load(string modId, string modDir)
    {
        if (modId is null)
            throw new ArgumentNullException(nameof(modId));
        if (modDir is null)
            throw new ArgumentNullException(nameof(modDir));

        if (string.IsNullOrWhiteSpace(modId))
            throw new ArgumentException("modId must not be empty", nameof(modId));

        return LoadInternal(modId, modDir);
    }

    // 清理指定模组此前注册的全部内容（供热重载 / 卸载时调用）。
    public static void Unload(string modId)
    {
        if (modId is null)
            throw new ArgumentNullException(nameof(modId));

        ItemLoader.UnregisterOwner(modId);
        TileLoader.UnregisterOwner(modId);
        RecipeLoader.UnregisterOwner(modId);
        MoodleLoader.UnregisterOwner(modId);
    }

    // 按与脚本端一致的顺序加载全部内容：
    // 物品 → 物块 → 配方（依赖物品）→ 状态。C# 端跳过脚本引擎绑定（allowPendingScripts=false）。
    private static LoadResult LoadInternal(string modId, string modDir)
    {
        var result = new LoadResult
        {
            ModId = modId,
            Items = ItemLoader.RegisterFromDirectory(modId, modDir, false),
            Tiles = TileLoader.RegisterFromDirectory(modId, modDir, false)
        };

        RecipeLoader.RegisterFromDirectory(modId, modDir);
        result.Recipes = RecipeLoader.LoadedRecipes.TryGetValue(modId, out var recipes) ? recipes.Count : 0;
        MoodleLoader.RegisterFromDirectory(modId, modDir, false);
        result.Moodles = MoodleLoader.LoadedMoodles.TryGetValue(modId, out var moodles) ? moodles.Count : 0;

        if (result.Total > 0)
            LogUtil.Info("mod_content.loaded", modId, result.Items, result.Tiles, result.Recipes, result.Moodles);

        return result;
    }

    // 读取 mod.json 的 id 与所在目录。失败返回 (null, null)。
    private static (string? modId, string? modDir) ReadManifest(string modJsonPath)
    {
        try
        {
            var json = File.ReadAllText(modJsonPath);
            var manifest = JsonUtil.Deserialize<ScriptManifest>(json);
            if (manifest is null)
            {
                LogUtil.Warning("mod_content.manifest_parse_failed", modJsonPath);
                return (null, null);
            }

            if (string.IsNullOrWhiteSpace(manifest.Id))
            {
                LogUtil.Error("mod_content.manifest_no_id", modJsonPath);
                return (null, null);
            }

            var dir = Path.GetDirectoryName(modJsonPath) ?? string.Empty;
            return (manifest.Id, dir);
        }
        catch (Exception ex)
        {
            LogUtil.Warning("mod_content.manifest_read_error", modJsonPath, ex.Message);
            return (null, null);
        }
    }

    // C# 端加载结果统计
    public class LoadResult
    {
        // 模组 id（取自 mod.json）
        public string ModId { get; set; } = string.Empty;

        // 各内容加载数量
        public int Items { get; set; }
        public int Tiles { get; set; }
        public int Recipes { get; set; }
        public int Moodles { get; set; }

        // 内容总数
        public int Total => Items + Tiles + Recipes + Moodles;
    }
}