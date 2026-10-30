using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onTilePlace")]
public class TilePlaceEvent : BarkEvent
{
    public string TileId { get; set; } = string.Empty;
    public int TileIndex { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
}

[ScriptEvent("onTileExist")]
public class TileExistEvent : BarkEvent
{
    public string TileId { get; set; } = string.Empty;
    public int TileIndex { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
}

[ScriptEvent("onTileDamaging")]
public class TileDamagingEvent : BarkEvent
{
    public string TileId { get; set; } = string.Empty;
    public int TileIndex { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
}

[ScriptEvent("onTileDestroyed")]
public class TileDestroyedEvent : BarkEvent
{
    public string TileId { get; set; } = string.Empty;
    public int TileIndex { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
}
