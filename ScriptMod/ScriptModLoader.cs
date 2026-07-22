using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bark.Tool;
using UnityEngine;

namespace Bark.ScriptMod;

// 脚本模组加载器：扫描 ScriptMods 目录，读取 mod.json，路由到对应 PuerTS 引擎
public class ScriptModLoader(string modsPath)
{
    private readonly Dictionary<string, ModManifest> _loadedMods = new();

    // 所有已加载的模组（只读）
    public IReadOnlyDictionary<string, ModManifest> LoadedMods => _loadedMods;

    // 已加载的 JS 模组
    public IReadOnlyList<ModManifest> LoadedJavaScriptMods =>
        _loadedMods.Values.Where(m => m.Language == ScriptLanguage.JavaScript).ToList().AsReadOnly();

    // 已加载的 Lua 模组
    public IReadOnlyList<ModManifest> LoadedLuaMods =>
        _loadedMods.Values.Where(m => m.Language == ScriptLanguage.Lua).ToList().AsReadOnly();

    // 已加载的 Python 模组
    public IReadOnlyList<ModManifest> LoadedPythonMods =>
        _loadedMods.Values.Where(m => m.Language == ScriptLanguage.Python).ToList().AsReadOnly();

    // 支持的入口文件扩展名 → 语言映射
    private static readonly Dictionary<string, ScriptLanguage> ExtensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".js", ScriptLanguage.JavaScript },
        { ".mjs", ScriptLanguage.JavaScript },
        { ".lua", ScriptLanguage.Lua },
        { ".py", ScriptLanguage.Python }
    };

    // 扫描并加载所有脚本模组
    public void LoadAll()
    {
        // 创建目录结构
        var modsDir = Path.Combine(modsPath, "Mods");
        var logsDir = Path.Combine(modsPath, "Logs");
        var configsDir = Path.Combine(modsPath, "Configs");

        if (!Directory.Exists(modsPath))
        {
            Directory.CreateDirectory(modsPath);
            LogUtil.Info("scriptmod.dir_created", modsPath);
        }
        Directory.CreateDirectory(modsDir);
        Directory.CreateDirectory(logsDir);
        Directory.CreateDirectory(configsDir);

        // 初始化脚本模组日志
        ScriptModLogger.Initialize(logsDir);

        // 1. 扫描 Mods/ 子目录
        var modDirectories = Directory.GetDirectories(modsDir);
        if (modDirectories.Length == 0)
        {
            LogUtil.Info("scriptmod.no_mods");
            return;
        }

        // 2. JSON 加载器：读取所有 mod.json
        var manifests = modDirectories.Select(LoadManifest).OfType<ModManifest>().ToList();

        // 3. 依赖检查 + 拓扑排序
        var sorted = TopologicalSort(manifests);

        // 4. 按顺序加载模组
        foreach (var manifest in sorted)
        {
            LoadMod(manifest);
        }

    }

    // 读取单个模组的 mod.json
    private static ModManifest? LoadManifest(string modDir)
    {
        var manifestPath = Path.Combine(modDir, "mod.json");
        if (!File.Exists(manifestPath))
        {
            LogUtil.Warning("scriptmod.skip_no_manifest", modDir);
            return null;
        }

        try
        {
            var json = File.ReadAllText(manifestPath);
            var manifest = JsonUtil.Deserialize<ModManifest>(json);
            if (manifest == null)
            {
                LogUtil.Warning("scriptmod.parse_failed", manifestPath);
                return null;
            }

            // 验证必填字段
            if (string.IsNullOrWhiteSpace(manifest.Id))
            {
                LogUtil.Warning("scriptmod.missing_id", manifestPath);
                return null;
            }

            if (string.IsNullOrWhiteSpace(manifest.Version))
            {
                LogUtil.Warning("scriptmod.missing_version", manifestPath);
                return null;
            }

            // 设置运行时字段
            manifest.Directory = modDir;

            // 查找入口文件（默认 main.js）
            var entryFile = FindEntryFile(modDir);
            if (entryFile == null)
            {
                LogUtil.Warning("scriptmod.no_entry_file", modDir);
                return null;
            }

            manifest.EntryFile = entryFile;
            manifest.Language = GetLanguage(entryFile);

            return manifest;
        }
        catch (Exception ex)
        {
            LogUtil.Warning("scriptmod.manifest_read_error", manifestPath, ex.Message);
            return null;
        }
    }

    // 查找入口文件（默认 main.js，也支持 main.lua / main.py）
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

    // 加载单个模组（路由到对应 PuerTS 引擎）
    private void LoadMod(ModManifest manifest)
    {
        if (_loadedMods.ContainsKey(manifest.Id))
        {
            LogUtil.Warning("scriptmod.duplicate_id", manifest.Id);
            return;
        }

        try
        {
            var success = false;
            switch (manifest.Language)
            {
                case ScriptLanguage.JavaScript:
                    success = LoadJavaScriptMod(manifest);
                    break;
                case ScriptLanguage.Lua:
                    success = LoadLuaMod(manifest);
                    break;
                case ScriptLanguage.Python:
                    success = LoadPythonMod(manifest);
                    break;
                default:
                    LogUtil.Warning("scriptmod.unsupported_language", manifest.Language, manifest.Id);
                    return;
            }

            if (!success) return;

            _loadedMods[manifest.Id] = manifest;
        }
        catch (Exception ex)
        {
            LogUtil.Warning("scriptmod.load_failed", manifest.Id, ex.Message);
        }
    }

    private static bool LoadJavaScriptMod(ModManifest manifest)
    {
        LogUtil.Message("scriptmod.mod_loading", "JS", manifest.Name);
        var go = new GameObject($"[ScriptMod-JS] {manifest.Id}");
        var engine = go.AddComponent<PuerJavaScript>();
        return engine.Load(manifest);
    }

    private static bool LoadLuaMod(ModManifest manifest)
    {
        LogUtil.Message("scriptmod.mod_loading", "Lua", manifest.Name);
        var go = new GameObject($"[ScriptMod-Lua] {manifest.Id}");
        var engine = go.AddComponent<PuerLua>();
        return engine.Load(manifest);
    }

    private static bool LoadPythonMod(ModManifest manifest)
    {
        LogUtil.Message("scriptmod.mod_loading", "Python", manifest.Name);
        var go = new GameObject($"[ScriptMod-Python] {manifest.Id}");
        var engine = go.AddComponent<PuerPython>();
        return engine.Load(manifest);
    }

    // 拓扑排序：根据依赖关系确定加载顺序
    private static List<ModManifest> TopologicalSort(List<ModManifest> manifests)
    {
        var manifestMap = manifests.ToDictionary(m => m.Id);
        var inDegree = manifests.ToDictionary(m => m.Id, _ => 0);
        var dependents = manifests.ToDictionary(m => m.Id, _ => new List<string>());

        // 构建依赖图
        foreach (var manifest in manifests)
        {
            foreach (var dep in manifest.Dependencies.Where(dep => manifestMap.ContainsKey(dep.Id)))
            {
                inDegree[manifest.Id]++;
                dependents[dep.Id].Add(manifest.Id);
            }
        }

        // 检查循环依赖
        var resolved = new List<ModManifest>();
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
            foreach (var mod in unresolved)
            {
                LogUtil.Warning("scriptmod.circular_dependency", mod.Id);
            }
        }

        return resolved;
    }

    // 获取已加载的模组信息
    public ModManifest? GetMod(string modId)
    {
        return _loadedMods.GetValueOrDefault(modId);
    }

    // 检查模组是否已加载
    public bool IsLoaded(string modId)
    {
        return _loadedMods.ContainsKey(modId);
    }

    // 获取所有已加载模组的列表
    public IReadOnlyList<ModManifest> ListMods()
    {
        return _loadedMods.Values.ToList().AsReadOnly();
    }
}