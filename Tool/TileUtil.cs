using System;
using System.Collections.Generic;
using System.Reflection;
using Bark.Constant;
using Bark.ScriptApi;
using CUCoreLib.Registries;

namespace Bark.Tool;

public static class TileUtil
{
    private static readonly Dictionary<string, ushort> s_vanillaLookup =
        BuildVanillaLookup();

    private static readonly FieldInfo? s_registeredIdsField =
        typeof(TileRegistry).GetField(
            "RegisteredDefinitionIds", BindingFlags.NonPublic | BindingFlags.Static);

    [ScriptMethod]
    public static ushort ResolveIndex(string tileId)
    {
        if (TryResolveIndex(tileId, out var index))
            return index;
        LogUtil.Warning("world.tile_not_found", tileId);
        throw new ArgumentException($"Tile '{tileId}' is not registered.",
            nameof(tileId));
    }

    public static bool TryResolveIndex(string tileId, out ushort index)
    {
        if (!string.IsNullOrEmpty(tileId))
        {
            if (s_vanillaLookup.TryGetValue(tileId, out index))
                return true;

            if (s_registeredIdsField?.GetValue(null) is Dictionary<string, ushort>
                    customIds
                && customIds.TryGetValue(tileId, out index))
                return true;
        }

        index = 0;
        return false;
    }

    public static bool TryResolveId(ushort index, out string? tileId)
    {
        if (index < TileRegistry.FirstCustomTileIndex)
        {
            tileId = Tiles.FromId(index)?.LocaleKey;
            return !string.IsNullOrEmpty(tileId);
        }

        if (s_registeredIdsField?.GetValue(null) is Dictionary<string, ushort> customIds)
            foreach (var (id, customIndex) in customIds)
            {
                if (customIndex != index) continue;
                tileId = id;
                return true;
            }

        tileId = null;
        return false;
    }

    private static Dictionary<string, ushort> BuildVanillaLookup()
    {
        var dict = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in typeof(Tiles).GetFields(BindingFlags.Public
            | BindingFlags.Static))
        {
            if (field.FieldType != typeof(Tiles)
                || field.GetValue(null) is not Tiles block)
                continue;
            if (!string.IsNullOrEmpty(block.LocaleKey))
                dict[block.LocaleKey] = block.Id;
        }

        return dict;
    }
}
