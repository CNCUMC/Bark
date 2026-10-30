using System;
using System.IO;
using Bark.Moodle;
using Bark.Recipe;
using Bark.Script;
using Bark.Tile;
using Bark.Tool;
using BepInEx;

namespace Bark.Items;

public static class ModContentApi
{
    private const string ManifestFileName = "mod.json";

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

    public static LoadResult LoadFromPlugins(string modName)
    {
        if (modName is null)
            throw new ArgumentNullException(nameof(modName));

        var dir = Path.Combine(Paths.PluginPath, modName);
        var manifestPath = Path.Combine(dir, ManifestFileName);
        return LoadFromManifest(manifestPath);
    }

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

    public static void Unload(string modId)
    {
        if (modId is null)
            throw new ArgumentNullException(nameof(modId));

        ItemLoader.UnregisterOwner(modId);
        TileLoader.UnregisterOwner(modId);
        RecipeLoader.UnregisterOwner(modId);
        MoodleLoader.UnregisterOwner(modId);
    }

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

    public class LoadResult
    {
        public string ModId { get; set; } = string.Empty;
        public int Items { get; set; }
        public int Tiles { get; set; }
        public int Recipes { get; set; }
        public int Moodles { get; set; }
        public int Total => Items + Tiles + Recipes + Moodles;
    }
}