using Bark.Event;

namespace Bark.Events;

// 物块放置事件：自定义物块被放置到世界中时触发
[ScriptEvent("onTilePlace")]
public class TilePlaceEvent : BarkEvent
{
    // 物块 ID（文件名不含扩展名，如 "marble"）
    public string TileId { get; set; } = string.Empty;

    // 物块索引
    public int TileIndex { get; set; }

    // 世界横坐标（格子单位）
    public int PosX { get; set; }

    // 世界纵坐标（格子单位）
    public int PosY { get; set; }
}

// 物块存在事件：世界中存在自定义物块时周期性触发（每帧/定时）
[ScriptEvent("onTileExist")]
public class TileExistEvent : BarkEvent
{
    public string TileId { get; set; } = string.Empty;
    public int TileIndex { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
}

// 物块受击事件：自定义物块受到伤害时触发
[ScriptEvent("onTileDamaging")]
public class TileDamagingEvent : BarkEvent
{
    public string TileId { get; set; } = string.Empty;
    public int TileIndex { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
}

// 物块破坏事件：自定义物块被完全破坏时触发
[ScriptEvent("onTileDestroyed")]
public class TileDestroyedEvent : BarkEvent
{
    public string TileId { get; set; } = string.Empty;
    public int TileIndex { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
}