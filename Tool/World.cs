using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CUCoreLib.Registries;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class World
{
    private static GameObject _cachedBgTemplate;

    private static readonly Dictionary<string, Sprite> SpriteCache =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> MissingSpriteWarnings =
        new(StringComparer.OrdinalIgnoreCase);

    private static Material _cachedBgMaterial;

    public static void PlaceBlock(int x, int y, ushort block)
    {
        PlaceBlock(new Vector2(x, y), block);
    }

    public static void PlaceBlock(Vector2 vector2, ushort block)
    {
        CheckForWorld();
        try
        {
            WorldGeneration.world.SetBlock(WorldGeneration.world.WorldToBlockPos(vector2), block);
        }
        catch (Exception ex)
        {
            Error("tool.world.place_block", vector2, block, ex);
        }
    }

    public static void FillBlocks(int startX, int startY, int endX, int endY, ushort block)
    {
        CheckForWorld();

        try
        {
            var world = WorldGeneration.world;
            var clampedStartX = Mathf.Clamp(startX, 0, (int)world.width - 2);
            var clampedStartY = Mathf.Clamp(startY, 0, (int)world.height - 2);
            var clampedEndX = Mathf.Clamp(endX, 0, (int)world.width - 2);
            var clampedEndY = Mathf.Clamp(endY, 0, (int)world.height - 2);

            for (var x = clampedStartX; x <= clampedEndX; x++)
            for (var y = clampedStartY; y <= clampedEndY; y++)
                world.SetBlockNoUpdate(new Vector2Int(x, y), block);

            var chunkStartX = clampedStartX / WorldGeneration.CHUNKSIZE;
            var chunkStartY = clampedStartY / WorldGeneration.CHUNKSIZE;
            var chunkEndX = clampedEndX / WorldGeneration.CHUNKSIZE;
            var chunkEndY = clampedEndY / WorldGeneration.CHUNKSIZE;

            for (var cx = chunkStartX; cx <= chunkEndX; cx++)
            for (var cy = chunkStartY; cy <= chunkEndY; cy++)
                world.UpdateChunk(new Vector2Int(cx, cy));
        }
        catch (Exception ex)
        {
            Error("tool.world.place_block", startX, startY, endX, endY, block, ex);
        }
    }

    public static void FillBlocks(int startX, int startY, ushort[,] blocks)
    {
        if (blocks == null)
            throw new ArgumentNullException(nameof(blocks));

        CheckForWorld();

        try
        {
            var world = WorldGeneration.world;
            var width = blocks.GetLength(0);
            var height = blocks.GetLength(1);
            var maxX = Mathf.Min(startX + width, (int)world.width - 2);
            var maxY = Mathf.Min(startY + height, (int)world.height - 2);

            for (var x = 0; x < width && startX + x <= maxX; x++)
            for (var y = 0; y < height && startY + y <= maxY; y++)
                world.SetBlockNoUpdate(new Vector2Int(startX + x, startY + y), blocks[x, y]);

            var chunkStartX = Mathf.Max(startX, 0) / WorldGeneration.CHUNKSIZE;
            var chunkStartY = Mathf.Max(startY, 0) / WorldGeneration.CHUNKSIZE;
            var chunkEndX = maxX / WorldGeneration.CHUNKSIZE;
            var chunkEndY = maxY / WorldGeneration.CHUNKSIZE;

            for (var cx = chunkStartX; cx <= chunkEndX; cx++)
            for (var cy = chunkStartY; cy <= chunkEndY; cy++)
                world.UpdateChunk(new Vector2Int(cx, cy));
        }
        catch (Exception ex)
        {
            Error("tool.world.place_block", startX, startY, blocks, ex);
        }
    }

    public static void PlaceItem(int x, int y, string item)
    {
        PlaceItem(new Vector2(x, y), item);
    }

    public static void PlaceItem(Vector2 vector2, string item)
    {
        CheckForWorld();

        if (string.IsNullOrWhiteSpace(item))
            throw new ArgumentException(Locale("tool.world.place_item.null_or_empty"), nameof(item));

        try
        {
            Utils.Create(item, vector2, 0.0f);
        }
        catch (Exception ex)
        {
            Error("tool.world.place_item", vector2, item, ex);
        }
    }

    public static void PlaceBackground(Vector2 pos, string backgroundId)
    {
        var x = (int)pos.x;
        var y = (int)pos.y;
        PlaceBackground(new Vector2Int(x, y), backgroundId);
    }

    public static void PlaceBackground(Vector2Int pos, string backgroundId)
    {
        if (WorldGeneration.world == null)
            return;

        if (!TryGetSprite(backgroundId, out var sprite))
            return;

        var template = GetBgTemplate();
        if (template == null)
            return;

        var worldPos2 = WorldGeneration.world.BlockToWorldPos(pos);
        var go = Object.Instantiate(template, worldPos2, Quaternion.identity);
        go.name = $"BgTile_{pos.x}_{pos.y}";
        go.SetActive(true);

        var parent = WorldGeneration.world.worldGrid?.transform;
        if (parent == null)
        {
            var chunk = WorldGeneration.world.GetClosestChunk(pos);
            if (chunk != null)
                parent = chunk.transform;
        }

        if (parent != null)
            go.transform.SetParent(parent, true);

        var mf = go.GetComponent<MeshFilter>();
        mf.mesh = CreateTileMesh(pos);

        var mr = go.GetComponent<MeshRenderer>();
        mr.material = GetOrCreateBgMaterial(sprite);
        mr.sortingOrder = -5000;
    }

    private static Material GetOrCreateBgMaterial(Sprite sprite)
    {
        if (_cachedBgMaterial == null)
            _cachedBgMaterial = new Material(WorldGeneration.world.defaultMat)
            {
                mainTextureScale = Vector2.one,
                mainTextureOffset = Vector2.one,
                color = Color.gray
            };

        _cachedBgMaterial.mainTexture = sprite.texture;
        return _cachedBgMaterial;
    }

    public static Mesh CreateTileMesh(Vector2Int pos)
    {
        const int tileCount = 4;
        var u = pos.x % tileCount;
        var v = pos.y % tileCount;
        if (u < 0) u += tileCount;
        if (v < 0) v += tileCount;

        var step = 1f / tileCount;
        var u0 = u * step;
        var u1 = (u + 1) * step;
        var v0 = v * step;
        var v1 = (v + 1) * step;

        var mesh = new Mesh
        {
            vertices =
            [
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0)
            ],
            uv =
            [
                new Vector2(u0, v0),
                new Vector2(u1, v0),
                new Vector2(u0, v1),
                new Vector2(u1, v1)
            ],
            triangles = [0, 2, 1, 2, 3, 1]
        };
        mesh.RecalculateNormals();
        return mesh;
    }

    private static GameObject GetBgTemplate()
    {
        if (_cachedBgTemplate != null)
            return _cachedBgTemplate;

        _cachedBgTemplate = new GameObject("World_BgTemplate");
        _cachedBgTemplate.AddComponent<MeshFilter>();
        _cachedBgTemplate.AddComponent<MeshRenderer>();
        _cachedBgTemplate.SetActive(false);
        Object.DontDestroyOnLoad(_cachedBgTemplate);
        return _cachedBgTemplate;
    }

    private static bool TryGetSprite(string backgroundId, out Sprite sprite)
    {
        if (SpriteCache.TryGetValue(backgroundId, out sprite) && sprite != null)
            return true;

        sprite = Resources.Load<Sprite>(backgroundId);
        if (sprite == null)
        {
            if (MissingSpriteWarnings.Add(backgroundId))
                Warning("tool.world.try_get_sprite", backgroundId);
            return false;
        }

        SpriteCache[backgroundId] = sprite;
        return true;
    }

    public static void CheckForWorld()
    {
        if (PlayerCamera.main == null)
            throw new InvalidOperationException(Locale("tool.world.check_for_world"));
    }

    public static void ClearCache()
    {
        SpriteCache.Clear();
        MissingSpriteWarnings.Clear();

        if (_cachedBgTemplate != null)
        {
            Object.Destroy(_cachedBgTemplate);
            _cachedBgTemplate = null;
        }

        if (_cachedBgMaterial != null)
        {
            Object.Destroy(_cachedBgMaterial);
            _cachedBgMaterial = null;
        }
    }

    private static void Warning(string key, params object[] args)
    {
        var message = Locale(key, args);
        Log.Warning(message, Plugin.Logger);
    }

    private static void Error(string key, params object[] args)
    {
        var message = Locale(key, args);
        Log.Error(message, Plugin.Logger);
    }

    private static string Locale(string key, params object[] args)
    {
        var text = LocaleRegistry.Get("other", key, key);
        return args.Length > 0 ? string.Format(text, args) : text;
    }
}