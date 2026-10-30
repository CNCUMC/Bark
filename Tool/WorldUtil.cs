using System;
using Bark.Constant;
using Bark.ScriptApi;
using CUCoreLib.Registries;
using UnityEngine;
using Object = UnityEngine.Object;

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
    public static string TileAt(int x, int y)
    {
        if (World == null || x < 0 || y < 0 || x >= GetWidth() || y >= GetHeight())
            return Tiles.Air.LocaleKey;

        var index = World.GetBlock(new Vector2Int(x, y));
        return TileUtil.TryResolveId(index, out var tileId) && tileId != null
            ? tileId
            : Tiles.Air.LocaleKey;
    }

    [ScriptMethod]
    public static Vector2Int ToTilePosition(float x, float y)
    {
        return World == null ? default : World.WorldToBlockPos(new Vector2(x, y));
    }

    [ScriptMethod]
    public static Vector2 ToRelativePosition(float x, float y)
    {
        if (World == null) return Vector2.zero;

        var tile = ToTilePosition(x, y);
        return new Vector2(x - tile.x, y - tile.y);
    }

    [ScriptMethod]
    public static void PlaceTile(int x, int y, ushort block)
    {
        PlaceTile(new Vector2(x, y), block);
    }

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
        for (var cx = csx / WorldGeneration.CHUNKSIZE;
            cx <= cex / WorldGeneration.CHUNKSIZE;
            cx++)
            for (var cy = csy / WorldGeneration.CHUNKSIZE;
                cy <= cey / WorldGeneration.CHUNKSIZE;
                cy++)
                World.UpdateChunk(new Vector2Int(cx, cy));
    }

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
    public static void FillBlocks(int startX, int startY, int endX, int endY,
        string block)
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

        var prefab = Resources.Load(item) as GameObject;
        if (prefab == null)
        {
            LogUtil.Warning("world.item_not_found", item);
            return;
        }

        try
        {
            var go = Object.Instantiate(prefab, pos, Quaternion.Euler(0f, 0f, rot));
            if (go == null)
                LogUtil.Warning("world.place_item_failed", item);
        }
        catch (Exception ex)
        {
            LogUtil.Error("world.place_item", pos, item, ex.Message);
        }
    }

    [ScriptMethod]
    public static void Explode(float x, float y, float range = 20f, float velocity = 20f,
        float structuralDamage = 1250f, float skinDamage = 0f, float muscleDamage = 0f,
        float shrapnelChance = 0f, string? sound = null)
    {
        CheckUtil.CheckWorld(Plugin.Logger);

        var pos = new Vector2(x, y);
        try
        {
            WorldGeneration.CreateExplosion(new ExplosionParams
            {
                position = pos,
                range = range,
                velocity = velocity,
                structuralDamage = structuralDamage,
                skinDamage = new RangeF(skinDamage, skinDamage),
                muscleDamage = new RangeF(muscleDamage, muscleDamage),
                shrapnelChance = shrapnelChance,
                sound = sound ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            LogUtil.Error("world.explode", pos, ex.Message);
        }
    }
}
