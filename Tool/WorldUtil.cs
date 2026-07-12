using System;
using Bark.BetterCCL;
using CUCoreLib.Helpers;
using UnityEngine;

namespace Bark.Tool;

public static class WorldUtil
{
    public static WorldGeneration World => WorldGeneration.world;

    public static void PlaceBlock(int x, int y, ushort block)
    {
        PlaceBlock(new Vector2(x, y), block);
    }

    public static void PlaceBlock(Vector2 pos, ushort block)
    {
        LogUtil.CheckWorld();
        try
        {
            World!.SetBlock(World.WorldToBlockPos(pos), block);
        }
        catch (Exception ex)
        {
            Error("world.place_block", ex);
        }
    }

    public static void FillBlocks(int sx, int sy, int ex, int ey, ushort block)
    {
        LogUtil.CheckWorld();
        var w = World!;
        var csx = Mathf.Clamp(sx, 0, (int)w.width - 2);
        var csy = Mathf.Clamp(sy, 0, (int)w.height - 2);
        var cex = Mathf.Clamp(ex, 0, (int)w.width - 2);
        var cey = Mathf.Clamp(ey, 0, (int)w.height - 2);
        for (var x = csx; x <= cex; x++)
        for (var y = csy; y <= cey; y++)
            w.SetBlockNoUpdate(new Vector2Int(x, y), block);
        for (var cx = csx / WorldGeneration.CHUNKSIZE; cx <= cex / WorldGeneration.CHUNKSIZE; cx++)
        for (var cy = csy / WorldGeneration.CHUNKSIZE; cy <= cey / WorldGeneration.CHUNKSIZE; cy++)
            w.UpdateChunk(new Vector2Int(cx, cy));
    }

    public static void PlaceItem(int x, int y, string item)
    {
        PlaceItem(new Vector2(x, y), item);
    }

    public static void PlaceItem(Vector2 pos, string item)
    {
        LogUtil.CheckWorld();
        LogUtil.CheckNotNullOrEmpty(item, nameof(item));
        try
        {
            Utils.Create(item, pos, 0f);
        }
        catch (Exception ex)
        {
            Error("world.place_item", ex);
        }
    }

    public static void CheckForWorld()
    {
        if (!CUCoreUtils.IsInWorld()) throw new InvalidOperationException(LocaleLog("world.check_for_world"));
    }

    private static void Error(string key, params object[] args)
    {
        LogUtil.Error(LocaleLog(key, args), Plugin.Logger);
    }

    private static string LocaleLog(string key, params object[] args)
    {
        return BetterLocale.GetLog(key, args);
    }
}