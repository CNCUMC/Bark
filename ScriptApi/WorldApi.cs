using Bark.Tool;

namespace Bark.ScriptApi;

public class WorldApi
{
    public int Width => (int)WorldUtil.World.width;
    public int Height => (int)WorldUtil.World.height;

    public void PlaceBlock(int x, int y, ushort block)
    {
        WorldUtil.PlaceBlock(x, y, block);
    }

    public void FillBlocks(int startX, int startY, int endX, int endY, ushort block)
    {
        WorldUtil.FillBlocks(startX, startY, endX, endY, block);
    }

    public void PlaceItem(int x, int y, string item)
    {
        WorldUtil.PlaceItem(x, y, item);
    }
}