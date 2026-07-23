using System;
using Bark.ScriptApi;
using UnityEngine;

namespace Bark.Tool;

public static class WorldUtil
{
    public static WorldGeneration World => WorldGeneration.world;

    [ScriptMethod]
    public static int GetWidth()
    {
        return (int)World.width;
    }

    [ScriptMethod]
    public static int GetHeight()
    {
        return (int)World.height;
    }

    [ScriptMethod]
    public static void PlaceBlock(int x, int y, ushort block)
    {
        PlaceBlock(new Vector2(x, y), block);
    }

    public static void PlaceBlock(Vector2 pos, ushort block)
    {
        CheckUtil.CheckWorld(Plugin.Logger);
        try
        {
            World.SetBlock(World.WorldToBlockPos(pos), block);
        }
        catch (Exception ex)
        {
            LogUtil.Error("world.place_block", ex);
        }
    }

    [ScriptMethod]
    public static void FillBlocks(int startX, int startY, int endX, int endY, ushort block)
    {
        CheckUtil.CheckWorld(Plugin.Logger);
        var w = World;
        var csx = Mathf.Clamp(startX, 0, (int)w.width - 2);
        var csy = Mathf.Clamp(startY, 0, (int)w.height - 2);
        var cex = Mathf.Clamp(endX, 0, (int)w.width - 2);
        var cey = Mathf.Clamp(endY, 0, (int)w.height - 2);
        for (var x = csx; x <= cex; x++)
        for (var y = csy; y <= cey; y++)
            w.SetBlockNoUpdate(new Vector2Int(x, y), block);
        for (var cx = csx / WorldGeneration.CHUNKSIZE; cx <= cex / WorldGeneration.CHUNKSIZE; cx++)
        for (var cy = csy / WorldGeneration.CHUNKSIZE; cy <= cey / WorldGeneration.CHUNKSIZE; cy++)
            w.UpdateChunk(new Vector2Int(cx, cy));
    }

    [ScriptMethod]
    public static void PlaceItem(int x, int y, string item)
    {
        PlaceItem(new Vector2(x, y), item);
    }

    public static void PlaceItem(Vector2 pos, string item)
    {
        CheckUtil.CheckWorld(Plugin.Logger);
        CheckUtil.CheckNotNullOrEmpty(item, nameof(item));
        try
        {
            Utils.Create(item, pos, 0f);
        }
        catch (Exception ex)
        {
            LogUtil.Error("world.place_item", ex);
        }
    }
}