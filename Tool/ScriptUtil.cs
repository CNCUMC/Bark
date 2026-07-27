using System.Collections.Generic;
using System.IO;
using Bark.Script;
using Bark.ScriptApi;

namespace Bark.Tool;

// 通用脚本执行工具：按模组 ID + 文件名触发脚本执行。
// 内部由 ScriptModLoader 在加载模组时注册，ItemScriptRunner 调用。
// 脚本侧可通过 Script.Execute(modId, fileName) 直接使用。
public static class ScriptUtil
{
    // modId → (ScriptEngine, modDir)
    private static readonly Dictionary<string, (ScriptEngine Engine, string ModDir)> ModEngines = new();

    // 注册一个模组的引擎（ScriptModLoader 在 LoadMod 后调用）
    internal static void Register(string modId, ScriptEngine engine, string modDir)
    {
        ModEngines[modId] = (engine, modDir);
    }

    // 注销一个模组的引擎（ScriptModLoader 在 UnloadAll 时调用）
    internal static void Unregister(string modId)
    {
        ModEngines.Remove(modId);
    }

    // 尝试获取指定模组的引擎与目录，供 TileScriptRunner 等内部组件直接访问
    internal static bool TryGetEngine(string modId, out ScriptEngine engine, out string modDir)
    {
        if (ModEngines.TryGetValue(modId, out var entry))
        {
            engine = entry.Engine;
            modDir = entry.ModDir;
            return true;
        }

        engine = null!;
        modDir = string.Empty;
        return false;
    }

    // 执行指定模组的物品脚本文件。fileName 是相对于模组目录的路径（含扩展名），
    // itemId 可选，执行时注入到脚本上下文。item 和 action 由 ItemScriptRunner 内部传入。
    [ScriptMethod]
    public static void Execute(string modId, string fileName, string? itemId = null,
        Item? item = null, string? action = null)
    {
        if (string.IsNullOrEmpty(modId)) return;
        if (string.IsNullOrEmpty(fileName)) return;

        if (!ModEngines.TryGetValue(modId, out var entry)) return;

        var fullPath = Path.Combine(entry.ModDir, fileName);
        if (!File.Exists(fullPath)) return;

        entry.Engine.ExecuteItemFile(fullPath, itemId, item, action);
    }

    // 执行指定模组的物块脚本文件。fileName 是相对于模组目录的路径（含扩展名）。
    // tileId 为物块 ID，context 包含物块索引和世界坐标，action 为触发动作名。
    // TileScriptRunner 内部传入，不接受 null。
    internal static void ExecuteTile(string modId, string fileName,
        string tileId, Tile.TileScriptContext context, string action)
    {
        if (string.IsNullOrEmpty(modId)) return;
        if (string.IsNullOrEmpty(fileName)) return;

        if (!ModEngines.TryGetValue(modId, out var entry)) return;

        var fullPath = Path.Combine(entry.ModDir, fileName);
        if (!File.Exists(fullPath)) return;

        entry.Engine.ExecuteTileFile(fullPath, tileId, context, action);
    }
}
