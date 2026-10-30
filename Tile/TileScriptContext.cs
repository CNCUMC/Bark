namespace Bark.Tile;

public class TileScriptContext(int tileIndex, int posX, int posY)
{
    public int PosX = posX;
    public int PosY = posY;
    public int TileIndex = tileIndex;
    public static TileScriptContext? CurrentContext { get; internal set; }
    public static string? CurrentAction { get; internal set; }
}
