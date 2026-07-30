namespace Bark.Tile;

// 物块脚本执行上下文，传入脚本 main(tileId, context, action)。
// context 包含物块索引和世界坐标。
public class TileScriptContext(int tileIndex, int posX, int posY)
{
    // 物块的世界横坐标（格子单位）
    public int PosX = posX;

    // 物块的世界纵坐标（格子单位）
    public int PosY = posY;

    // 物块索引（与 CustomTileDefinition 绑定的 int 索引）
    public int TileIndex = tileIndex;

    // 当前执行的上下文引用（脚本引擎在执行前设置，执行后清除）
    public static TileScriptContext? CurrentContext { get; internal set; }

    // 当前触发动作名（如 "on_place"）
    public static string? CurrentAction { get; internal set; }
}