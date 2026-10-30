using System.Collections.Generic;
using System.IO;
using Bark.Script;
using Bark.ScriptApi;
using Bark.Tile;

namespace Bark.Tool;

[ScriptApi]
public static class ScriptUtil
{
    private static readonly Dictionary<string, (ScriptEngine Engine, string ModDir)>
        ModEngines = new();

    internal static void Register(string modId, ScriptEngine engine, string modDir)
    {
        ModEngines[modId] = (engine, modDir);
    }

    internal static void Unregister(string modId)
    {
        ModEngines.Remove(modId);
    }

    internal static bool TryGetEngine(string modId, out ScriptEngine engine,
        out string modDir)
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

    internal static void ExecuteTile(string modId, string fileName,
        string tileId, TileScriptContext context, string action)
    {
        if (string.IsNullOrEmpty(modId)) return;
        if (string.IsNullOrEmpty(fileName)) return;

        if (!ModEngines.TryGetValue(modId, out var entry)) return;

        var fullPath = Path.Combine(entry.ModDir, fileName);
        if (!File.Exists(fullPath)) return;

        entry.Engine.ExecuteTileFile(fullPath, tileId, context, action);
    }
}
