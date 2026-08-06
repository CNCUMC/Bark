using System;
using System.IO;
using BepInEx;
using Bark.Script;
using Bark.Tool;

namespace Bark.Items;

// C# 端物品加载 API。
// 让普通 BepInEx 模组（无脚本引擎）也能通过 JSON 文件注册自定义物品，
// 复用与脚本模组完全相同的 ItemLoader 解析 / 模板合并 / 资产加载 / 注册流程。
//
// 模组通过 mod.json 声明自己的 id（命名空间前缀），物品 ID = {id}.{文件名}。
// 目录约定（与脚本模组一致）：
//   {modRoot}/mod.json            — 模组清单，至少含 "id"
//   {modRoot}/Item/*.json         — 物品 JSON（文件名即物品本地名）
//   {modRoot}/Assets/Item/*.png   — 物品贴图等资产
//
// 最简用法（mod.json 与 DLL 同目录）：
//   ItemLoaderApi.LoadFromPluginDirectory(GetType().Assembly.Location);
//
// 或更明确地指定 mod.json 路径：
//   ItemLoaderApi.LoadFromManifest(Path.Combine(modDir, "mod.json"));
public static class ItemLoaderApi
{
    // mod.json 文件名（与脚本模组一致）
    private const string ManifestFileName = "mod.json";

    // 从 mod.json 加载物品。
    // modJsonPath - mod.json 的完整路径，其所在目录即模组根目录
    // 返回：成功加载的物品数量；文件缺失 / 解析失败 / 无 id 时返回 0。
    public static int LoadFromManifest(string modJsonPath)
    {
        if (modJsonPath is null)
            throw new ArgumentNullException(nameof(modJsonPath));

        if (!File.Exists(modJsonPath))
        {
            LogUtil.Warning("items.csharp.manifest_missing", modJsonPath);
            return 0;
        }

        ScriptManifest? manifest = ReadManifest(modJsonPath);
        if (manifest is null)
            return 0;

        manifest.Directory = Path.GetDirectoryName(modJsonPath) ?? string.Empty;
        return ItemLoader.RegisterFromDirectory(manifest.Id, manifest.Directory, allowPendingScripts: false);
    }

    // 在插件 DLL 所在目录自动查找 mod.json 并加载。
    // assemblyLocation - 通常是 typeof(你的Plugin).Assembly.Location
    // 返回：成功加载的物品数量。
    public static int LoadFromPluginDirectory(string assemblyLocation)
    {
        if (string.IsNullOrWhiteSpace(assemblyLocation))
            throw new ArgumentException("assemblyLocation must not be empty", nameof(assemblyLocation));

        var dir = Path.GetDirectoryName(assemblyLocation);
        if (string.IsNullOrEmpty(dir))
        {
            LogUtil.Warning("items.csharp.assembly_dir_missing", assemblyLocation);
            return 0;
        }

        var manifestPath = Path.Combine(dir, ManifestFileName);
        return LoadFromManifest(manifestPath);
    }

    // 从 BepInEx 插件根目录下的 {modName} 子目录加载（自动查找该目录的 mod.json）。
    // modName - 插件目录名，拼接为 Path.Combine(Paths.PluginPath, modName)
    public static int LoadFromPlugins(string modName)
    {
        if (modName is null)
            throw new ArgumentNullException(nameof(modName));

        var pluginPath = Paths.PluginPath;
        var dir = Path.Combine(pluginPath, modName);
        var manifestPath = Path.Combine(dir, ManifestFileName);
        return LoadFromManifest(manifestPath);
    }

    // 直接指定 modId 与模组根目录加载（不依赖 mod.json）。
    // 适用于不需要元数据、或在测试中显式控制的场景。
    public static int Load(string modId, string modDir)
    {
        if (modId is null)
            throw new ArgumentNullException(nameof(modId));
        if (modDir is null)
            throw new ArgumentNullException(nameof(modDir));

        if (string.IsNullOrWhiteSpace(modId))
            throw new ArgumentException("modId must not be empty", nameof(modId));

        return ItemLoader.RegisterFromDirectory(modId, modDir, allowPendingScripts: false);
    }

    // 清理指定模组此前注册的物品（供热重载 / 卸载时调用）。
    public static void Unload(string modId)
    {
        if (modId is null)
            throw new ArgumentNullException(nameof(modId));
        ItemLoader.UnregisterOwner(modId);
    }

    // 读取并校验 mod.json，缺失 id 时返回 null。
    private static ScriptManifest? ReadManifest(string modJsonPath)
    {
        try
        {
            var json = File.ReadAllText(modJsonPath);
            var manifest = JsonUtil.Deserialize<ScriptManifest>(json);
            if (manifest is null)
            {
                LogUtil.Warning("items.csharp.manifest_parse_failed", modJsonPath);
                return null;
            }

            if (string.IsNullOrWhiteSpace(manifest.Id))
            {
                LogUtil.Error("items.csharp.manifest_no_id", modJsonPath);
                return null;
            }

            return manifest;
        }
        catch (Exception ex)
        {
            LogUtil.Warning("items.csharp.manifest_read_error", modJsonPath, ex.Message);
            return null;
        }
    }
}
