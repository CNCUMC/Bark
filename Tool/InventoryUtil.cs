using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bark.BetterCCL;
using BepInEx.Logging;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class InventoryUtil
{
    private static Body GetBody()
    {
        LogUtil.CheckBody();
        return GameInstances.Body!;
    }

    public static bool IsSlotOccupied(int slot)
    {
        return GetBody().HoldingItem(slot);
    }

    public static bool IsSlotEmpty(int slot)
    {
        return !IsSlotOccupied(slot);
    }

    public static Item GetItem(int slot)
    {
        return GetBody().GetItem(slot);
    }

    public static ItemInfo? GetItemInfo(int slot)
    {
        return GetItem(slot).Stats;
    }

    public static string? GetItemId(int slot)
    {
        return GetItem(slot).id;
    }

    public static bool HasItem(string id)
    {
        LogUtil.CheckNotNullOrEmpty(id, nameof(id));
        return GetBody().HoldingItem(id);
    }

    public static bool HasItemThorough(string id)
    {
        LogUtil.CheckNotNullOrEmpty(id, nameof(id));
        return GetBody().FindByIdThorough(id, out _);
    }

    public static bool HasAnyItem(params string[] ids)
    {
        return ids is { Length: > 0 } && GetBody().Let(b => ids.Any(b.HoldingItem));
    }

    public static bool HasItem(Predicate<ItemInfo> p)
    {
        if (p == null) throw new ArgumentNullException(nameof(p));
        var b = GetBody();
        for (var i = 0; i < b.slots.Length; i++)
        {
            var info = b.GetItem(i)?.Stats;
            if (info != null && p(info)) return true;
        }

        return false;
    }

    public static bool HasItemByTag(string tag)
    {
        return !string.IsNullOrWhiteSpace(tag) && HasItem(info => info.HasTag(tag));
    }

    public static bool HasItemByCategory(string cat)
    {
        return !string.IsNullOrWhiteSpace(cat) && HasItem(info => info.category == cat);
    }

    public static bool HasWearableItem()
    {
        return HasItem(info => info.wearable);
    }

    public static int CountItem(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return 0;
        var b = GetBody();
        var c = 0;
        for (var i = 0; i < b.slots.Length; i++)
            if (b.GetItem(i)?.id == id)
                c++;
        return c;
    }

    public static List<Item> GetAllItems() => GetBody().GetAllItems();
    public static List<Item> GetAllItemsThorough() => GetBody().GetAllItemsThorough();

    public static List<ItemInfo> GetAllItemInfos() =>
        GetAllItems().Select(i => i.Stats).Where(i => i != null).ToList();

    public static List<ItemInfo> GetAllItemInfosThorough() => GetAllItemsThorough().Select(i => i.Stats)
        .Where(i => i != null).ToList();

    public static List<string> GetAllItemIds() => GetAllItems().Select(i => i.id).ToList();
    public static List<Item> GetWearables() => GetBody().GetAllWearables();

    public static List<ItemInfo> GetWearableInfos() =>
        GetWearables().Select(i => i.Stats).Where(i => i != null).ToList();

    public static int? FindFirstEmptySlot()
    {
        return GetBody().FirstEmptySlot();
    }

    public static bool FindById(string id, out Item? item)
    {
        item = null;
        return !string.IsNullOrWhiteSpace(id) && GetBody().FindByIdSurface(id, out item);
    }

    public static bool FindByIdThorough(string id, out Item? item)
    {
        item = null;
        return !string.IsNullOrWhiteSpace(id) && GetBody().FindByIdThorough(id, out item);
    }

    public static int GetHandSlot()
    {
        return GetBody().handSlot;
    }

    public static Item GetItemInHand()
    {
        return GetBody().GetItem(GetBody().handSlot);
    }

    public static ItemInfo? GetItemInfoInHand()
    {
        return GetItemInHand().Stats;
    }

    public static string? GetItemIdInHand()
    {
        return GetItemInHand().id;
    }

    public static int GetSlotCount()
    {
        return GetBody().slots.Length;
    }

    public static string GetItemIdsString()
    {
        var ids = GetAllItemIds();
        return ids.Count > 0 ? string.Join(", ", ids) : Locale("inventory.empty");
    }

    private static string Locale(string key, params object[] args)
    {
        return BetterLocale.GetLog(key, args);
    }
}

internal static class BodyExt
{
    public static TResult Let<TResult>(this Body b, Func<Body, TResult> f)
    {
        return f(b);
    }
}