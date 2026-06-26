using System;
using System.Diagnostics.CodeAnalysis;
using Bark.BetterCCL;
using BepInEx.Logging;
using UnityEngine;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class WorldUtil
{
    private static readonly ManualLogSource Logger = Plugin.Logger;

    public static void PlaceBlock(int x, int y, ushort block) => PlaceBlock(new Vector2(x, y), block);
    public static void PlaceBlock(Vector2 pos, ushort block) { LogUtil.CheckWorld(Logger); try { GameInstances.World!.SetBlock(GameInstances.World.WorldToBlockPos(pos), block); } catch (Exception ex) { Err("world.place_block", ex); } }

    public static void FillBlocks(int sx, int sy, int ex, int ey, ushort block)
    {
        LogUtil.CheckWorld(Logger); var w = GameInstances.World!;
        var csx = Mathf.Clamp(sx, 0, (int)w.width - 2); var csy = Mathf.Clamp(sy, 0, (int)w.height - 2);
        var cex = Mathf.Clamp(ex, 0, (int)w.width - 2); var cey = Mathf.Clamp(ey, 0, (int)w.height - 2);
        for (var x = csx; x <= cex; x++) for (var y = csy; y <= cey; y++) w.SetBlockNoUpdate(new Vector2Int(x, y), block);
        for (var cx = csx / WorldGeneration.CHUNKSIZE; cx <= cex / WorldGeneration.CHUNKSIZE; cx++)
        for (var cy = csy / WorldGeneration.CHUNKSIZE; cy <= cey / WorldGeneration.CHUNKSIZE; cy++) w.UpdateChunk(new Vector2Int(cx, cy));
    }

    public static void PlaceItem(int x, int y, string item) => PlaceItem(new Vector2(x, y), item);
    public static void PlaceItem(Vector2 pos, string item)
    {
        LogUtil.CheckWorld(Logger); if (string.IsNullOrWhiteSpace(item)) throw new ArgumentException(Loc("world.place_item.null_or_empty"), nameof(item));
        try { Utils.Create(item, pos, 0f); } catch (Exception ex) { Err("world.place_item", ex); }
    }

    public static void CheckForWorld() { if (!GameInstances.IsInGame) throw new InvalidOperationException(Loc("world.check_for_world")); }

    private static void Err(string key, params object[] args) => LogUtil.Error(Loc(key, args), Logger);
    private static string Loc(string key, params object[] args) => BetterLocale.Other("log." + key, args);
}
