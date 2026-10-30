using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Bark.Commands;
using Bark.Items;
using Bark.Moodle;
using Bark.Recipe;
using Bark.Save;
using Bark.Tile;
using Bark.Tool;

namespace Bark.Script;

public class ScriptModLoader(string modsPath) : IDisposable
{
    private const string DisabledSuffix = ".dis";

    private static readonly string ZipCacheDir =
        Path.Combine(Plugin.BarkCachePath, "ScriptMods");

    private static readonly Dictionary<string, ScriptLanguage> ExtensionMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { ".js", ScriptLanguage.JavaScript },
            { ".mjs", ScriptLanguage.JavaScript },
            { ".lua", ScriptLanguage.Lua }
        };

    private static readonly Regex SnakeCaseRegex =
        new("^[a-z][a-z0-9]*(_[a-z0-9]+)*$", RegexOptions.Compiled);

    private static readonly Dictionary<string, ScriptManifest> _loadedMods = new();

    public static IReadOnlyDictionary<string, ScriptManifest> LoadedScriptMods =>
        _loadedMods;

    public static IReadOnlyList<ScriptManifest> LoadedJavaScriptMods =>
        _loadedMods.Values.Where(m => m.Language == ScriptLanguage.JavaScript).ToList()
            .AsReadOnly();

    public static IReadOnlyList<ScriptManifest> LoadedLuaMods =>
        _loadedMods.Values.Where(m => m.Language == ScriptLanguage.Lua).ToList()
            .AsReadOnly();

    public static IReadOnlyList<string> Items =>
        ItemLoader.LoadedItems.Values.SelectMany(list => list).Select(e => e.Id).ToList()
            .AsReadOnly();

    public static IReadOnlyList<string> Tiles =>
        TileLoader.LoadedTiles.Values.SelectMany(list => list).Select(e => e.TileId)
            .ToList().AsReadOnly();

    public static IReadOnlyList<string> Recipes =>
        RecipeLoader.LoadedRecipes.Values.SelectMany(list => list).Select(e => e.Id)
            .ToList().AsReadOnly();

    public static IReadOnlyList<string> Moodles =>
        MoodleLoader.LoadedMoodles.Values.SelectMany(list => list).Select(e => e.Key)
            .ToList().AsReadOnly();

    public void Dispose()
    {
        UnloadAll();
    }

    private static bool IsDisabledName(string? name)
    {
        return name != null
            && name.EndsWith(DisabledSuffix, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEnabledDirectory(string dir)
    {
        return !IsDisabledName(Path.GetFileName(dir));
    }

    public static List<string> GetRegisteredContentIds()
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in Items) ids.Add(id);
        foreach (var id in Tiles) ids.Add(id);
        foreach (var id in Recipes) ids.Add(id);
        foreach (var id in Moodles) ids.Add(id);
        return [.. ids];
    }

    public void LoadAll()
    {
        ScriptLocaleManager.Initialize();

        var modsDir = Path.Combine(modsPath, "Mods");
        var configsDir = Path.Combine(modsPath, "Configs");

        if (!Directory.Exists(modsPath))
        {
            Directory.CreateDirectory(modsPath);
            LogUtil.Info("script_mod_loader.dir_created", modsPath);
        }

        Directory.CreateDirectory(modsDir);
        Directory.CreateDirectory(configsDir);

        ExtractZipMods(modsDir);

        var modDirectories = Directory.GetDirectories(modsDir).Where(IsEnabledDirectory)
            .ToList();

        if (Directory.Exists(ZipCacheDir))
            modDirectories.AddRange(Directory.GetDirectories(ZipCacheDir)
                .Where(IsEnabledDirectory));

        if (modDirectories.Count == 0)
        {
            LogUtil.Info("script_mod_loader.no_mods");
            return;
        }

        var manifests = modDirectories.Select(LoadManifest).OfType<ScriptManifest>()
            .ToList();

        manifests = DeduplicateMods(manifests);

        var sorted = TopologicalSort(manifests);

        foreach (var manifest in sorted)
            ScriptLocaleManager.LoadModLocale(manifest.Directory, manifest.Id);

        foreach (var manifest in sorted)
            OptionsUtil.RegisterFromMod(manifest, manifest.Directory, configsDir);

        foreach (var manifest in sorted)
            ItemLoader.RegisterFromMod(manifest);

        foreach (var manifest in sorted)
            TileLoader.RegisterFromMod(manifest);

        foreach (var manifest in sorted)
            RecipeLoader.RegisterFromMod(manifest);

        foreach (var manifest in sorted)
            MoodleLoader.RegisterFromMod(manifest);

        foreach (var manifest in sorted)
            CommandLoader.RegisterFromMod(manifest);

        foreach (var manifest in sorted)
        {
            LoadMod(manifest);
            ItemLoader.RegisterScripts(manifest);
            TileLoader.RegisterScripts(manifest);
            MoodleLoader.RegisterScripts(manifest);
            CommandLoader.RegisterScripts(manifest);
        }
    }

    private static void ExtractZipMods(string modsDir)
    {
        var zipFiles = Directory.GetFiles(modsDir, "*.zip");

        if (zipFiles.Length == 0)
        {
            if (Directory.Exists(ZipCacheDir))
                Directory.Delete(ZipCacheDir, true);
            return;
        }

        Directory.CreateDirectory(ZipCacheDir);
        var validCachePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var zipPath in zipFiles)
        {
            var modName = Path.GetFileNameWithoutExtension(zipPath);

            if (IsDisabledName(modName))
            {
                LogUtil.Info("script_mod_loader.disabled_mod", modName);
                continue;
            }

            var targetDir = Path.Combine(ZipCacheDir, modName);
            validCachePaths.Add(targetDir);

            if (Directory.Exists(targetDir)
                && File.Exists(Path.Combine(targetDir, "mod.json")))
                continue;

            try
            {
                if (Directory.Exists(targetDir))
                    Directory.Delete(targetDir, true);

                ZipFile.ExtractToDirectory(zipPath, targetDir);
                LogUtil.Info("script_mod_loader.zip_extracted", modName);
            }
            catch (Exception ex)
            {
                LogUtil.Warning("script_mod_loader.zip_extract_failed", zipPath,
                    ex.Message);
            }
        }

        if (!Directory.Exists(ZipCacheDir)) return;
        foreach (var dir in Directory.GetDirectories(ZipCacheDir))
        {
            if (validCachePaths.Contains(dir))
                continue;

            Directory.Delete(dir, true);
            LogUtil.Info("script_mod_loader.cache_cleaned", Path.GetFileName(dir));
        }
    }

    private static List<ScriptManifest> DeduplicateMods(List<ScriptManifest> manifests)
    {
        return
        [
            .. manifests
                .GroupBy(m => m.Id)
                .Select(g =>
                {
                    var list = g.ToList();
                    var preferred =
                        list.FirstOrDefault(m => !IsFromZipCache(m.Directory));
                    return preferred ?? list.First();
                })
        ];
    }

    private static bool IsFromZipCache(string dir)
    {
        return dir.StartsWith(ZipCacheDir, StringComparison.OrdinalIgnoreCase);
    }

    private static ScriptManifest? LoadManifest(string modDir)
    {
        var manifestPath = Path.Combine(modDir, "mod.json");
        if (!File.Exists(manifestPath))
        {
            LogUtil.Warning("script_mod_loader.skip_no_manifest", modDir);
            return null;
        }

        try
        {
            var json = File.ReadAllText(manifestPath);
            var manifest = JsonUtil.Deserialize<ScriptManifest>(json);
            if (manifest == null)
            {
                LogUtil.Warning("script_mod_loader.parse_failed", manifestPath);
                return null;
            }

            if (string.IsNullOrWhiteSpace(manifest.Id))
            {
                LogUtil.Warning("script_mod_loader.missing_id", manifestPath);
                return null;
            }

            if (!IsSnakeCase(manifest.Id))
            {
                LogUtil.Error("script_mod_loader.id_not_snake_case", manifest.Id,
                    manifestPath);
                return null;
            }

            if (string.IsNullOrWhiteSpace(manifest.Version))
            {
                LogUtil.Warning("script_mod_loader.missing_version", manifestPath);
                return null;
            }

            manifest.Directory = modDir;

            var entryFile = FindEntryFile(modDir);
            if (entryFile == null)
            {
                manifest.Language = ScriptLanguage.None;
                LogUtil.Info("script_mod_loader.data_only_mod", manifest.Id, modDir);
            }
            else
            {
                manifest.EntryFile = entryFile;
                manifest.Language = GetLanguage(entryFile);
            }

            return manifest;
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_mod_loader.manifest_read_error", manifestPath,
                ex.Message);
            return null;
        }
    }

    private static string? FindEntryFile(string modDir)
    {
        return ExtensionMap.Keys.Select(ext => Path.Combine(modDir, $"main{ext}"))
            .FirstOrDefault(File.Exists);
    }

    private static ScriptLanguage GetLanguage(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        return ExtensionMap.GetValueOrDefault(ext, ScriptLanguage.JavaScript);
    }

    private static bool IsSnakeCase(string id)
    {
        return SnakeCaseRegex.IsMatch(id);
    }

    private static void LoadMod(ScriptManifest manifest)
    {
        if (_loadedMods.ContainsKey(manifest.Id))
        {
            LogUtil.Warning("script_mod_loader.duplicate_id", manifest.Id);
            return;
        }

        try
        {
            ScriptEngine? engine;
            switch (manifest.Language)
            {
                case ScriptLanguage.None:
                    _loadedMods[manifest.Id] = manifest;
                    LogUtil.Message("script_mod_loader.data_mod_loaded", manifest.Name,
                        manifest.Version);
                    return;
                case ScriptLanguage.JavaScript:
                    engine = LoadJavaScriptMod(manifest);
                    break;
                case ScriptLanguage.Lua:
                    engine = LoadLuaMod(manifest);
                    break;
                default:
                    LogUtil.Warning("script_mod_loader.unsupported_language",
                        manifest.Language, manifest.Id);
                    return;
            }

            if (engine == null) return;

            manifest.Engine = engine;
            _loadedMods[manifest.Id] = manifest;

            ScriptUtil.Register(manifest.Id, engine, manifest.Directory);

            RegisterScriptEventHandlers(manifest);

            switch (engine)
            {
                case PuerJavaScript js: js.Enable(); break;
                case PuerLua lua: lua.Enable(); break;
            }
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_mod_loader.load_failed", manifest.Id, ex.Message);
        }
    }

    private static void RegisterScriptEventHandlers(ScriptManifest manifest)
    {
        ScriptEventScanner.RegisterForMod(manifest);
    }

    private static PuerJavaScript? LoadJavaScriptMod(ScriptManifest manifest)
    {
        LogUtil.Message("script_mod_loader.mod_loading", "JavaScript", manifest.Name,
            manifest.Version);
        var engine = new PuerJavaScript();
        return engine.Load(manifest)
            ? engine
            : null;
    }

    private static PuerLua? LoadLuaMod(ScriptManifest manifest)
    {
        LogUtil.Message("script_mod_loader.mod_loading", "Lua", manifest.Name,
            manifest.Version);
        var engine = new PuerLua();
        return engine.Load(manifest)
            ? engine
            : null;
    }

    private static List<ScriptManifest> TopologicalSort(List<ScriptManifest> manifests)
    {
        var manifestMap = manifests.ToDictionary(m => m.Id);
        var inDegree = manifests.ToDictionary(m => m.Id, _ => 0);
        var dependents = manifests.ToDictionary(m => m.Id, _ => new List<string>());

        foreach (var manifest in manifests)
            foreach (var dep in manifest.Dependencies.Where(dep =>
                manifestMap.ContainsKey(dep.Id)))
            {
                inDegree[manifest.Id]++;
                dependents[dep.Id].Add(manifest.Id);
            }

        var resolved = new List<ScriptManifest>();
        var queue =
            new Queue<string>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));

        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            resolved.Add(manifestMap[id]);

            foreach (var dependent in dependents[id])
            {
                inDegree[dependent]--;
                if (inDegree[dependent] == 0)
                    queue.Enqueue(dependent);
            }
        }

        if (resolved.Count >= manifests.Count) return resolved;
        {
            var unresolved =
                manifests.Where(m => resolved.All(r => r.Id != m.Id)).ToList();
            foreach (var mod in unresolved)
                LogUtil.Warning("script_mod_loader.circular_dependency", mod.Id);
        }

        return resolved;
    }

    public void ReloadAll()
    {
        UnloadAll();
        LoadAll();
    }

    public static void DisableAllScripts()
    {
        UnloadAll();
    }

    private static void UnloadAll()
    {
        foreach (var manifest in _loadedMods.Values)
            try
            {
                switch (manifest.Engine)
                {
                    case PuerJavaScript js:
                        js.Disable();
                        js.Unload();
                        break;
                    case PuerLua lua:
                        lua.Disable();
                        lua.Unload();
                        break;
                }

                manifest.Engine?.Dispose();
            }
            catch (Exception ex)
            {
                LogUtil.Warning("script_mod_loader.reload_unload_failed", manifest.Id,
                    ex.Message);
            }

        foreach (var manifest in _loadedMods.Values)
            ScriptUtil.Unregister(manifest.Id);

        _loadedMods.Clear();
        SaveLoader.Clear();
        TileLoader.LoadedTiles.Clear();
    }

    public ScriptManifest? GetMod(string modId)
    {
        return _loadedMods.GetValueOrDefault(modId);
    }

    public bool IsLoaded(string modId)
    {
        return _loadedMods.ContainsKey(modId);
    }

    public static IReadOnlyList<ScriptManifest> ListMods()
    {
        return _loadedMods.Values.ToList().AsReadOnly();
    }

    public void UpdateAll()
    {
        foreach (var manifest in _loadedMods.Values)
            try
            {
                manifest.Engine?.CallUpdate();
            }
            catch
            {
                // ignored
            }
    }
}
