using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class ItemUtil
{
    public static List<Item> FindNearby(Vector2 center, float radius, bool includeContained = false)
    {
        var result = new List<Item>(); if (radius <= 0f) return result;
        var sqr = radius * radius;
        foreach (var item in Object.FindObjectsOfType<Item>())
        {
            if (item == null) continue;
            if (!includeContained && item.ParentContainer() != null) continue;
            if (((Vector2)item.transform.position - center).sqrMagnitude <= sqr) result.Add(item);
        }
        return result;
    }

    public static Item? FindClosest(Vector2 center, float maxRadius = float.MaxValue, bool includeContained = false)
    {
        Item? best = null; var bestSqr = maxRadius * maxRadius;
        foreach (var item in Object.FindObjectsOfType<Item>())
        {
            if (item == null) continue;
            if (!includeContained && item.ParentContainer() != null) continue;
            var d = ((Vector2)item.transform.position - center).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = item; }
        }
        return best;
    }

    public static void SetCondition(Item? item, float condition) => item?.SetCondition(Mathf.Clamp01(condition));
    public static void Repair(Item? item) => SetCondition(item, 1f);
    public static void SetFavourited(Item? item, bool f) { if (item != null) item.favourited = f; }
    public static void Destroy(Item? item)
    {
        if (item == null) return;
        if (item.ParentContainer() != null) item.transform.SetParent(null, true);
        Object.Destroy(item.gameObject);
    }

    public static ItemInfo? GetInfo(string? id) => string.IsNullOrEmpty(id) || Item.GlobalItems == null ? null : Item.GlobalItems.TryGetValue(id, out var info) ? info : null;
    public static bool IsKnownId(string? id) => GetInfo(id) != null;
    public static IEnumerable<string> GetAllIds() => Item.GlobalItems != null ? new List<string>(Item.GlobalItems.Keys) : Enumerable.Empty<string>();
}
