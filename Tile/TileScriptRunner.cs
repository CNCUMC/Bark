using System.Collections.Generic;
using System.Linq;
using Bark.Event;
using Bark.Events;
using Bark.Tool;

namespace Bark.Tile;

// 物块脚本运行器：监听物块事件，通过 ScriptUtil 触发对应的脚本文件。
// 在 Plugin.Awake() 中调用 Listen() 注册事件处理器。
public static class TileScriptRunner
{
    private const string Guid = Plugin.Guid + ".tiles";

    // 注册事件处理器（应在所有模组加载完成后调用）
    public static void Listen()
    {
        EventUtil.On<TilePlaceEvent>(OnTilePlace, Guid);
        EventUtil.On<TileExistEvent>(OnTileExist, Guid);
        EventUtil.On<TileDamagingEvent>(OnTileDamaging, Guid);
        EventUtil.On<TileDestroyedEvent>(OnTileDestroyed, Guid);
    }

    // 停止监听（卸载时调用）
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

    // 从 TileScriptRegistry 查找物块脚本，通过 ScriptUtil 按顺序执行
    private static void ExecuteScripts(string tileId, int tileIndex, int posX, int posY,
        string action, System.Func<TileScriptEntry, List<string>> getScriptList)
    {
        if (string.IsNullOrEmpty(tileId))
            return;

        var entry = TileScriptRegistry.GetEntry(tileId);
        if (entry is null)
            return;

        var scripts = getScriptList(entry);
        if (scripts.Count == 0)
            return;

        // 构建上下文
        var context = new TileScriptContext(tileIndex, posX, posY);

        foreach (var relativePath in scripts.Where(relativePath => !string.IsNullOrEmpty(relativePath)))
        {
            ScriptUtil.ExecuteTile(entry.ModId, relativePath, tileId, context, action);
        }
    }
}
