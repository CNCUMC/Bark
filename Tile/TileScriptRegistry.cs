using System;
using System.Collections.Generic;
using Bark.Script;

namespace Bark.Tile;

public static class TileScriptRegistry
{
    private static readonly Dictionary<string, TileScriptEntry> Entries = new();

    public static void Register(string tileId, TileScriptDef scriptDef,
        ScriptEngine engine, string modId,
        string modDir)
    {
        if (string.IsNullOrEmpty(tileId))
            throw new ArgumentNullException(nameof(tileId));
        if (scriptDef is null)
            throw new ArgumentNullException(nameof(scriptDef));
        if (engine is null)
            throw new ArgumentNullException(nameof(engine));
        if (string.IsNullOrEmpty(modId))
            throw new ArgumentNullException(nameof(modId));
        if (string.IsNullOrEmpty(modDir))
            throw new ArgumentNullException(nameof(modDir));

        if (IsEmpty(scriptDef))
            return;

        Entries[tileId] = new TileScriptEntry(engine, scriptDef, modId, modDir);
    }

    public static void Unregister(string tileId)
    {
        Entries.Remove(tileId);
    }

    public static TileScriptEntry? GetEntry(string tileId)
    {
        return Entries.GetValueOrDefault(tileId);
    }

    private static bool IsEmpty(TileScriptDef def)
    {
        return def.OnPlace.Count == 0
            && def.OnExist.Count == 0
            && def.OnDamaging.Count == 0
            && def.OnDestroyed.Count == 0;
    }
}

public class TileScriptEntry(
    ScriptEngine engine,
    TileScriptDef scriptDef,
    string modId,
    string modDir)
{
    public readonly ScriptEngine Engine = engine;
    public readonly string ModDir = modDir;
    public readonly string ModId = modId;
    public readonly List<string> OnDamaging = scriptDef.OnDamaging;
    public readonly List<string> OnDestroyed = scriptDef.OnDestroyed;
    public readonly List<string> OnExist = scriptDef.OnExist;
    public readonly List<string> OnPlace = scriptDef.OnPlace;
}
