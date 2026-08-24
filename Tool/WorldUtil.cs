using System;
using Bark.ScriptApi;
using CUCoreLib.Registries;
using UnityEngine;

namespace Bark.Tool;

[ScriptApi]
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
    public static void PlaceTile(int x, int y, ushort block)
    {
        PlaceTile(new Vector2(x, y), block);
    }

    // 接受物块 ID 字符串（如 "copper"、"marble"），自动解析为索引
    [ScriptMethod(Name = "PlaceTile")]
    public static void PlaceTile(int x, int y, string block)
    {
        if (string.IsNullOrEmpty(block))
            return;
        if (TileUtil.TryResolveIndex(block, out var index))
            PlaceTile(x, y, index);
        else
            LogUtil.Warning("world.tile_not_found", block);
    }

    public static void PlaceTile(Vector2 pos, ushort block)
    {
        CheckUtil.CheckWorld(Plugin.Logger);
        try
        {
            // 使用 TileRegistry.SetBlock 而非 WorldGeneration.SetBlock：
            // 前者会先把已注册的自定义物块注入 world.tiles[index]，再落块；
            // 直接调用后者对自定义物块（index >= 36）会因 tiles 未注入而失败。
            TileRegistry.SetBlock(World, World.WorldToBlockPos(pos), block);
        }
        catch (Exception ex)
        {
            LogUtil.Error("world.place_tile", pos, block, ex.Message);
        }
    }

    [ScriptMethod]
    public static void FillTiles(int startX, int startY, int endX, int endY, ushort block)
    {
        CheckUtil.CheckWorld(Plugin.Logger);
        var csx = Mathf.Clamp(startX, 0, GetWidth() - 2);
        var csy = Mathf.Clamp(startY, 0, GetHeight() - 2);
        var cex = Mathf.Clamp(endX, 0, GetWidth() - 2);
        var cey = Mathf.Clamp(endY, 0, GetHeight() - 2);
        for (var x = csx; x <= cex; x++)
        for (var y = csy; y <= cey; y++)
            TileRegistry.SetBlockNoUpdate(World, new Vector2Int(x, y), block);
        for (var cx = csx / WorldGeneration.CHUNKSIZE; cx <= cex / WorldGeneration.CHUNKSIZE; cx++)
        for (var cy = csy / WorldGeneration.CHUNKSIZE; cy <= cey / WorldGeneration.CHUNKSIZE; cy++)
            World.UpdateChunk(new Vector2Int(cx, cy));
    }

    // 接受物块 ID 字符串（如 "copper"、"marble"），自动解析为索引
    [ScriptMethod(Name = "FillTiles")]
    public static void FillTiles(int startX, int startY, int endX, int endY, string block)
    {
        if (string.IsNullOrEmpty(block))
            return;
        if (TileUtil.TryResolveIndex(block, out var index))
            FillTiles(startX, startY, endX, endY, index);
        else
            LogUtil.Warning("world.tile_not_found", block);
    }

    [Obsolete("Use PlaceTile()")]
    public static void PlaceBlock(int x, int y, ushort block)
    {
        PlaceTile(x, y, block);
    }

    [Obsolete("Use FillTiles()")]
    public static void FillBlocks(int startX, int startY, int endX, int endY, string block)
    {
        FillTiles(startX, startY, endX, endY, block);
    }

    [ScriptMethod]
    public static void PlaceItem(int x, int y, string item, float rot = 0f)
    {
        PlaceItem(new Vector2(x, y), item, rot);
    }

    public static void PlaceItem(Vector2 pos, string item, float rot = 0f)
    {
        CheckUtil.CheckWorld(Plugin.Logger);
        CheckUtil.CheckNotNullOrEmpty(item, nameof(item));

        // 预检查物品预制体是否存在，避免 Resources.Load(null) / Instantiate(null) 抛异常。
        // 物品 ID 不存在通常是作者拼写错误，用 Warning 明确提示，而非捕获异常。
        var prefab = Resources.Load(item) as GameObject;
        if (prefab == null)
        {
            LogUtil.Warning("world.item_not_found", item);
            return;
        }

        try
        {
            // 直接用加载好的预制体实例化（与 Utils.Create 等价，但只 Resources.Load 一次）
            var go = UnityEngine.Object.Instantiate(prefab, pos, Quaternion.Euler(0f, 0f, rot));
            if (go == null)
                LogUtil.Warning("world.place_item_failed", item);
        }
        catch (Exception ex)
        {
            LogUtil.Error("world.place_item", pos, item, ex.Message);
        }
    }
}