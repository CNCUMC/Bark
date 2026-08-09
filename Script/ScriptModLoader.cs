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
using BepInEx;

namespace Bark.Script;

// 脚本模组加载器：扫描 ScriptMods 目录，读取 mod.json，路由到对应 PuerTS 引擎
public class ScriptModLoader(string modsPath) : IDisposable
{
    // zip 模组解压到 BepInEx 缓存目录下的子目录
    private static readonly string ZipCacheDir = Path.Combine(Plugin.BarkCachePath, "ScriptMods");

    // 支持的入口文件扩展名 → 语言映射
    private static readonly Dictionary<string, ScriptLanguage> ExtensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".js", ScriptLanguage.JavaScript },
        { ".mjs", ScriptLanguage.JavaScript },
        { ".lua", ScriptLanguage.Lua }
    };

    // 验证 ID 是否为 snake_case：小写字母开头，字母数字组成，下划线分隔
    private static readonly Regex SnakeCaseRegex = new("^[a-z][a-z0-9]*(_[a-z0-9]+)*$", RegexOptions.Compiled);

    // 禁用后缀：文件夹/压缩包名称（不含扩展名）以此结尾则视为禁用，跳过加载
    private const string DisabledSuffix = ".dis";

    // 判断名称是否带禁用标记（不区分大小写）
    private static bool IsDisabledName(string? name)
    {
        return name != null && name.EndsWith(DisabledSuffix, StringComparison.OrdinalIgnoreCase);
    }

    // 判断目录是否被禁用（取目录名做后缀匹配）
    private static bool IsEnabledDirectory(string dir)
    {
        return !IsDisabledName(Path.GetFileName(dir));
    }

    private static readonly Dictionary<string, ScriptManifest> _loadedMods = new();

    // 所有已加载的模组（只读）
    public static IReadOnlyDictionary<string, ScriptManifest> LoadedScriptMods => _loadedMods;

    // 已加载的 JS 模组
    public static IReadOnlyList<ScriptManifest> LoadedJavaScriptMods =>
        _loadedMods.Values.Where(m => m.Language == ScriptLanguage.JavaScript).ToList().AsReadOnly();

    // 已加载的 Lua 模组
    public static IReadOnlyList<ScriptManifest> LoadedLuaMods =>
        _loadedMods.Values.Where(m => m.Language == ScriptLanguage.Lua).ToList().AsReadOnly();

    // 各 Loader 收集到的 Bark 内容 ID，集中暴露供外部（命令补全、查询等）统一调用。
    // 内容分散记录在各 Loader 的 Loaded* 字典中（各自负责所有权与热重载），
    // 此处仅做聚合视图（属性每次访问实时读取），不重复存储，避免双写不一致。

    // Bark 注册的全部物品 ID（形如 modid.itemid）
    public static IReadOnlyList<string> Items =>
        ItemLoader.LoadedItems.Values.SelectMany(list => list).Select(e => e.Id).ToList().AsReadOnly();

    // Bark 注册的全部物块 ID（形如 modid.tileid）
    public static IReadOnlyList<string> Tiles =>
        TileLoader.LoadedTiles.Values.SelectMany(list => list).Select(e => e.TileId).ToList().AsReadOnly();

    // Bark 注册的全部配方 ID
    public static IReadOnlyList<string> Recipes =>
        RecipeLoader.LoadedRecipes.Values.SelectMany(list => list).Select(e => e.Id).ToList().AsReadOnly();

    // Bark 注册的全部 Moodle key
    public static IReadOnlyList<string> Moodles =>
        MoodleLoader.LoadedMoodles.Values.SelectMany(list => list).Select(e => e.Key).ToList().AsReadOnly();

    // 卸载所有已加载的模组并释放资源
    public void Dispose()
    {
        UnloadAll();
    }

    // 汇总所有通过 Bark 注册的内容 ID（物品/物块/配方/Moodle），去重。
    public static List<string> GetRegisteredContentIds()
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in Items) ids.Add(id);
        foreach (var id in Tiles) ids.Add(id);
        foreach (var id in Recipes) ids.Add(id);
        foreach (var id in Moodles) ids.Add(id);
        return [.. ids];
    }

    // 扫描并加载所有脚本模组
    public void LoadAll()
    {
        // 初始化语言管理器状态
        ScriptLocaleManager.Initialize();

        // 创建目录结构
        var modsDir = Path.Combine(modsPath, "Mods");
        var configsDir = Path.Combine(modsPath, "Configs");

        if (!Directory.Exists(modsPath))
        {
            Directory.CreateDirectory(modsPath);
            LogUtil.Info("script_mod_loader.dir_created", modsPath);
        }

        Directory.CreateDirectory(modsDir);
        Directory.CreateDirectory(configsDir);

        // 1. 解压 zip 模组到 BepInEx 缓存目录
        ExtractZipMods(modsDir);

        // 2. 收集所有模组目录：Mods/*/ + 缓存中的 zip 解压目录（跳过带 .dis 禁用标记的目录）
        var modDirectories = Directory.GetDirectories(modsDir).Where(IsEnabledDirectory).ToList();

        if (Directory.Exists(ZipCacheDir))
            modDirectories.AddRange(Directory.GetDirectories(ZipCacheDir).Where(IsEnabledDirectory));

        if (modDirectories.Count == 0)
        {
            LogUtil.Info("script_mod_loader.no_mods");
            return;
        }

        // 3. JSON 加载器：读取所有 mod.json
        var manifests = modDirectories.Select(LoadManifest).OfType<ScriptManifest>().ToList();

        // 3.5 去重：目录版模组优先于同 ID 的 zip 版（开发模式覆盖）
        manifests = DeduplicateMods(manifests);

        // 4. 依赖检查 + 拓扑排序
        var sorted = TopologicalSort(manifests);

        // 5. 加载各模组的语言文件
        foreach (var manifest in sorted)
            ScriptLocaleManager.LoadModLocale(manifest.Directory, manifest.Id);

        // 6. 注册配置选项到游戏设置系统（必须在引擎创建前完成）
        // 选项定义来自 {modDir}/options.json（与 mod.json 同层），用户保存值写入 Configs/{modId}.json
        foreach (var manifest in sorted)
            OptionsUtil.RegisterFromMod(manifest, manifest.Directory, configsDir);

        // 6.3 加载自定义物品到 CUCoreLib ItemRegistry / LiquidRegistry
        foreach (var manifest in sorted)
            ItemLoader.RegisterFromMod(manifest);

        // 6.35 加载自定义物块到 CUCoreLib TileRegistry
        foreach (var manifest in sorted)
            TileLoader.RegisterFromMod(manifest);

        // 6.4 加载自定义合成表到 CUCoreLib RecipeRegistry（必须在物品注册之后）
        foreach (var manifest in sorted)
            RecipeLoader.RegisterFromMod(manifest);

        // 6.5 加载自定义 Moodle 到 CUCoreLib MoodleRegistry
        foreach (var manifest in sorted)
            MoodleLoader.RegisterFromMod(manifest);

        // 6.6 暂存脚本命令定义，待引擎就绪后注册到 ConsoleCommandRegistry
        foreach (var manifest in sorted)
            CommandLoader.RegisterFromMod(manifest);

        // 7. 按顺序加载模组
        foreach (var manifest in sorted)
        {
            LoadMod(manifest);
            // 引擎就绪后，将暂存的物品脚本映射写入 ItemScriptRegistry
            ItemLoader.RegisterScripts(manifest);
            // 引擎就绪后，将暂存的物块脚本映射写入 TileScriptRegistry
            TileLoader.RegisterScripts(manifest);
            // 引擎就绪后，将暂存的 Moodle 脚本映射写入 MoodleScriptRegistry
            MoodleLoader.RegisterScripts(manifest);
            // 引擎就绪后，将暂存的命令注册到 ConsoleCommandRegistry，指向脚本 onCommand
            CommandLoader.RegisterScripts(manifest);
        }
    }


    // 解压 Mods/*.zip 到 BepInEx 缓存目录（仅首次，已存在则跳过）
    // 清理已删除 zip 对应的孤儿缓存
    private static void ExtractZipMods(string modsDir)
    {
        var zipFiles = Directory.GetFiles(modsDir, "*.zip");

        if (zipFiles.Length == 0)
        {
            // 没有 zip 模组，清理整个缓存目录
            if (Directory.Exists(ZipCacheDir))
                Directory.Delete(ZipCacheDir, true);
            return;
        }

        Directory.CreateDirectory(ZipCacheDir);
        var validCachePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var zipPath in zipFiles)
        {
            var modName = Path.GetFileNameWithoutExtension(zipPath);

            // .dis 后缀：视为禁用，跳过解压（其残留缓存会在孤儿清理中被删除）
            if (IsDisabledName(modName))
            {
                LogUtil.Info("script_mod_loader.disabled_mod", modName);
                continue;
            }

            var targetDir = Path.Combine(ZipCacheDir, modName);
            validCachePaths.Add(targetDir);

            // 已解压过且包含 mod.json，跳过
            if (Directory.Exists(targetDir) && File.Exists(Path.Combine(targetDir, "mod.json")))
                continue;

            try
            {
                // 清理旧解压残留
                if (Directory.Exists(targetDir))
                    Directory.Delete(targetDir, true);

                ZipFile.ExtractToDirectory(zipPath, targetDir);
                LogUtil.Info("script_mod_loader.zip_extracted", modName);
            }
            catch (Exception ex)
            {
                LogUtil.Warning("script_mod_loader.zip_extract_failed", zipPath, ex.Message);
            }
        }

        // 清理孤儿缓存（zip 已被删除）
        if (!Directory.Exists(ZipCacheDir)) return;
        foreach (var dir in Directory.GetDirectories(ZipCacheDir))
        {
            if (validCachePaths.Contains(dir))
                continue;

            Directory.Delete(dir, true);
            LogUtil.Info("script_mod_loader.cache_cleaned", Path.GetFileName(dir));
        }
    }

    // 去重：同 ID 的目录版模组优先于 zip 解压版（开发模式覆盖）
    private static List<ScriptManifest> DeduplicateMods(List<ScriptManifest> manifests)
    {
        return
        [
            .. manifests
                .GroupBy(m => m.Id)
                .Select(g =>
                {
                    var list = g.ToList();
                    // 优先选非 zip 缓存的版本（即用户放 Mods/ 目录的）
                    var preferred = list.FirstOrDefault(m => !IsFromZipCache(m.Directory));
                    return preferred ?? list.First();
                })
        ];
    }

    // 判断目录是否在 zip 缓存路径下
    private static bool IsFromZipCache(string dir)
    {
        return dir.StartsWith(ZipCacheDir, StringComparison.OrdinalIgnoreCase);
    }

    // 读取单个模组的 mod.json
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

            // 验证必填字段
            if (string.IsNullOrWhiteSpace(manifest.Id))
            {
                LogUtil.Warning("script_mod_loader.missing_id", manifestPath);
                return null;
            }

            // ID 必须使用 snake_case
            if (!IsSnakeCase(manifest.Id))
            {
                LogUtil.Error("script_mod_loader.id_not_snake_case", manifest.Id, manifestPath);
                return null;
            }

            if (string.IsNullOrWhiteSpace(manifest.Version))
            {
                LogUtil.Warning("script_mod_loader.missing_version", manifestPath);
                return null;
            }

            // 设置运行时字段
            manifest.Directory = modDir;

            // 查找入口文件：纯数据模组（仅 JSON 内容，无脚本）可缺省入口文件
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
            LogUtil.Warning("script_mod_loader.manifest_read_error", manifestPath, ex.Message);
            return null;
        }
    }

    // 查找入口文件（默认 main.js，也支持 main.lua）
    private static string? FindEntryFile(string modDir)
    {
        return ExtensionMap.Keys.Select(ext => Path.Combine(modDir, $"main{ext}")).FirstOrDefault(File.Exists);
    }

    // 根据文件扩展名获取脚本语言
    private static ScriptLanguage GetLanguage(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        return ExtensionMap.GetValueOrDefault(ext, ScriptLanguage.JavaScript);
    }

    private static bool IsSnakeCase(string id)
    {
        return SnakeCaseRegex.IsMatch(id);
    }

    // 加载单个模组（路由到对应 PuerTS 引擎）
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
                // 纯数据模组：无入口脚本，仅注册 JSON 内容（已在 LoadAll 中完成），不创建脚本引擎
                case ScriptLanguage.None:
                    _loadedMods[manifest.Id] = manifest;
                    LogUtil.Message("script_mod_loader.data_mod_loaded", manifest.Name, manifest.Version);
                    return;
                case ScriptLanguage.JavaScript:
                    engine = LoadJavaScriptMod(manifest);
                    break;
                case ScriptLanguage.Lua:
                    engine = LoadLuaMod(manifest);
                    break;
                default:
                    LogUtil.Warning("script_mod_loader.unsupported_language", manifest.Language, manifest.Id);
                    return;
            }

            if (engine == null) return;

            manifest.Engine = engine;
            _loadedMods[manifest.Id] = manifest;

            // 注册到 ScriptUtil 供手动触发脚本
            ScriptUtil.Register(manifest.Id, engine, manifest.Directory);

            // 注册脚本模组的生命周期钩子为事件处理器
            RegisterScriptEventHandlers(manifest);

            // 加载完成后调用 onEnable
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

    // 将脚本模组的生命周期钩子注册为事件处理器
    // 通过 [ScriptEvent] 注解自动发现所有需要桥接到脚本侧的事件类型
    private static void RegisterScriptEventHandlers(ScriptManifest manifest)
    {
        ScriptEventScanner.RegisterForMod(manifest);
    }

    private static PuerJavaScript? LoadJavaScriptMod(ScriptManifest manifest)
    {
        LogUtil.Message("script_mod_loader.mod_loading", "JavaScript", manifest.Name, manifest.Version);
        var engine = new PuerJavaScript();
        return engine.Load(manifest) ? engine : null;
    }

    private static PuerLua? LoadLuaMod(ScriptManifest manifest)
    {
        LogUtil.Message("script_mod_loader.mod_loading", "Lua", manifest.Name, manifest.Version);
        var engine = new PuerLua();
        return engine.Load(manifest) ? engine : null;
    }

    // 拓扑排序：根据依赖关系确定加载顺序
    private static List<ScriptManifest> TopologicalSort(List<ScriptManifest> manifests)
    {
        var manifestMap = manifests.ToDictionary(m => m.Id);
        var inDegree = manifests.ToDictionary(m => m.Id, _ => 0);
        var dependents = manifests.ToDictionary(m => m.Id, _ => new List<string>());

        // 构建依赖图
        foreach (var manifest in manifests)
        foreach (var dep in manifest.Dependencies.Where(dep => manifestMap.ContainsKey(dep.Id)))
        {
            inDegree[manifest.Id]++;
            dependents[dep.Id].Add(manifest.Id);
        }

        // 检查循环依赖
        var resolved = new List<ScriptManifest>();
        var queue = new Queue<string>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));

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

        // 检测未解析的模组（循环依赖）
        if (resolved.Count >= manifests.Count) return resolved;
        {
            var unresolved = manifests.Where(m => resolved.All(r => r.Id != m.Id)).ToList();
            foreach (var mod in unresolved) LogUtil.Warning("script_mod_loader.circular_dependency", mod.Id);
        }

        return resolved;
    }

    // 重载所有脚本模组：先卸载全部，再重新加载
    public void ReloadAll()
    {
        UnloadAll();
        LoadAll();
    }

    // 禁用本地所有脚本模组：卸载引擎并清空加载记录。
    // 用于客户端加入主机时，改用主机同步（GitHub / 主机 fetch）得到的模组。
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
                LogUtil.Warning("script_mod_loader.reload_unload_failed", manifest.Id, ex.Message);
            }

        // 注销所有 ScriptUtil 注册
        foreach (var manifest in _loadedMods.Values)
            ScriptUtil.Unregister(manifest.Id);

        _loadedMods.Clear();
        // 清理 SaveLoader 的追踪记录
        SaveLoader.Clear();

        // 清理 TileLoader 的追踪记录
        TileLoader.LoadedTiles.Clear();
    }

    // 获取已加载的模组信息
    public ScriptManifest? GetMod(string modId)
    {
        return _loadedMods.GetValueOrDefault(modId);
    }

    // 检查模组是否已加载
    public bool IsLoaded(string modId)
    {
        return _loadedMods.ContainsKey(modId);
    }

    // 获取所有已加载模组的列表
    public static IReadOnlyList<ScriptManifest> ListMods()
    {
        return _loadedMods.Values.ToList().AsReadOnly();
    }

    // 每帧调用所有已加载模组的 onUpdate()（由 Plugin.Update 驱动）
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