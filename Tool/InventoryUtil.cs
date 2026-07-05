using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bark.BetterCCL;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class InventoryUtil
{
    public static bool IsSlotOccupied(int slot)
    {
        return PlayerUtil.Body.HoldingItem(slot);
    }

    public static bool IsSlotEmpty(int slot)
    {
        return !IsSlotOccupied(slot);
    }

    public static Item GetItem(int slot)
    {
        return PlayerUtil.Body.GetItem(slot);
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
        return PlayerUtil.Body.HoldingItem(id);
    }

    public static bool HasItemThorough(string id)
    {
        LogUtil.CheckNotNullOrEmpty(id, nameof(id));
        return PlayerUtil.Body.FindByIdThorough(id, out _);
    }

    public static bool HasAnyItem(params string[] ids)
    {
        return ids is { Length: > 0 } && PlayerUtil.Body.Let(body => ids.Any(body.HoldingItem));
    }

    public static bool HasItem(Predicate<ItemInfo> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        var body = PlayerUtil.Body;
        for (var i = 0; i < body.slots.Length; i++)
        {
            var info = body.GetItem(i)?.Stats;
            if (info != null && predicate(info)) return true;
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
        var body = PlayerUtil.Body;
        var c = 0;
        for (var i = 0; i < body.slots.Length; i++)
            if (body.GetItem(i)?.id == id)
                c++;
        return c;
    }

    public static List<Item> GetAllItems()
    {
        return PlayerUtil.Body.GetAllItems();
    }

    public static List<Item> GetAllItemsThorough()
    {
        return PlayerUtil.Body.GetAllItemsThorough();
    }

    public static List<ItemInfo> GetAllItemInfos()
    {
        return GetAllItems().Select(i => i.Stats).Where(i => i != null).ToList();
    }

    public static List<ItemInfo> GetAllItemInfosThorough()
    {
        return GetAllItemsThorough().Select(i => i.Stats)
            .Where(i => i != null).ToList();
    }

    public static List<string> GetAllItemIds()
    {
        return GetAllItems().Select(i => i.id).ToList();
    }

    public static List<Item> GetWearables()
    {
        return PlayerUtil.Body.GetAllWearables();
    }

    public static List<ItemInfo> GetWearableInfos()
    {
        return GetWearables().Select(i => i.Stats).Where(i => i != null).ToList();
    }

    public static int? FindFirstEmptySlot()
    {
        return PlayerUtil.Body.FirstEmptySlot();
    }

    public static bool FindById(string id, out Item? item)
    {
        item = null;
        return !string.IsNullOrWhiteSpace(id) && PlayerUtil.Body.FindByIdSurface(id, out item);
    }

    public static bool FindByIdThorough(string id, out Item? item)
    {
        item = null;
        return !string.IsNullOrWhiteSpace(id) && PlayerUtil.Body.FindByIdThorough(id, out item);
    }

    public static int GetHandSlot()
    {
        return PlayerUtil.Body.handSlot;
    }

    public static Item GetItemInHand()
    {
        return PlayerUtil.Body.GetItem(PlayerUtil.Body.handSlot);
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
        return PlayerUtil.Body.slots.Length;
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