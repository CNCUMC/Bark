using System;
using System.Collections.Generic;
using System.Linq;
using Bark.Events;
using Bark.Tool;

namespace Bark.Tile;

public static class TileScriptRunner
{
    private const string Guid = Plugin.Guid + ".tiles";

    public static void Listen()
    {
        EventUtil.On<TilePlaceEvent>(OnTilePlace, Guid);
        EventUtil.On<TileExistEvent>(OnTileExist, Guid);
        EventUtil.On<TileDamagingEvent>(OnTileDamaging, Guid);
        EventUtil.On<TileDestroyedEvent>(OnTileDestroyed, Guid);
    }

    public static void Stop()
    {
        EventUtil.UnregisterAll(Guid);
    }

    private static void OnTilePlace(TilePlaceEvent evt)
    {
        ExecuteScripts(evt.TileId, evt.TileIndex, evt.PosX, evt.PosY,
            "on_place", e => e.OnPlace);
    }

    private static void OnTileExist(TileExistEvent evt)
    {
        ExecuteScripts(evt.TileId, evt.TileIndex, evt.PosX, evt.PosY,
            "on_exist", e => e.OnExist);
    }

    private static void OnTileDamaging(TileDamagingEvent evt)
    {
        ExecuteScripts(evt.TileId, evt.TileIndex, evt.PosX, evt.PosY,
            "on_damaging", e => e.OnDamaging);
    }

    private static void OnTileDestroyed(TileDestroyedEvent evt)
    {
        ExecuteScripts(evt.TileId, evt.TileIndex, evt.PosX, evt.PosY,
            "on_destroyed", e => e.OnDestroyed);
    }

    private static void ExecuteScripts(string tileId, int tileIndex, int posX, int posY,
        string action, Func<TileScriptEntry, List<string>> getScriptList)
    {
        if (string.IsNullOrEmpty(tileId))
            return;

        var entry = TileScriptRegistry.GetEntry(tileId);
        if (entry is null)
            return;

        var scripts = getScriptList(entry);
        if (scripts.Count == 0)
            return;

        var context = new TileScriptContext(tileIndex, posX, posY);

        foreach (var relativePath in scripts.Where(relativePath =>
            !string.IsNullOrEmpty(relativePath)))
            ScriptUtil.ExecuteTile(entry.ModId, relativePath, tileId, context, action);
    }
}
