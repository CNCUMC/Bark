using System;
using System.Collections.Generic;
using System.Linq;
using Bark.BetterCCL;

namespace Bark.Tool;

public static class InventoryUtil
{
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

    public static bool IsHandEmpty()
    {
        return IsSlotEmpty(GetHandSlot());
    }

    public static bool IsHandOccupied()
    {
        return IsSlotOccupied(GetHandSlot());
    }

    public static bool HasItemInHand(string id)
    {
        var item = GetItemInHand();
        return item != null && item.id == id;
    }

    public static bool HasItemInHandByTag(string tag)
    {
        var info = GetItemInfoInHand();
        return info != null && info.HasTag(tag);
    }

    public static bool HasItemInHandByCategory(string category)
    {
        var info = GetItemInfoInHand();
        return info != null && info.category == category;
    }

    public static int GetSlotCount()
    {
        return PlayerUtil.Body.slots.Length;
    }

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

    public static int? FindFirstEmptySlot()
    {
        return PlayerUtil.Body.FirstEmptySlot();
    }

    public static bool HasItem(string id)
    {
        CheckUtil.CheckNotNullOrEmpty(id, nameof(id));
        return PlayerUtil.Body.HoldingItem(id);
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

    public static bool HasItemByCategory(string category)
    {
        return !string.IsNullOrWhiteSpace(category) && HasItem(info => info.category == category);
    }

    public static bool HasAnyItem(params string[] ids)
    {
        return ids is { Length: > 0 } && PlayerUtil.Body.Let(body => ids.Any(body.HoldingItem));
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

    public static List<ItemInfo> GetAllItemInfos()
    {
        return GetAllItems().Select(i => i.Stats).Where(i => i != null).ToList();
    }

    public static List<string> GetAllItemIds()
    {
        return GetAllItems().Select(i => i.id).ToList();
    }

    public static List<Item> GetItemsByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : GetAllItems().Where(i => i.Stats != null && i.Stats.HasTag(tag)).ToList();
    }

    public static List<Item> GetItemsByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : GetAllItems().Where(i => i.Stats != null && i.Stats.category == category).ToList();
    }

    public static List<ItemInfo> GetItemInfosByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : GetAllItemInfos().Where(i => i.HasTag(tag)).ToList();
    }

    public static List<ItemInfo> GetItemInfosByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : GetAllItemInfos().Where(i => i.category == category).ToList();
    }

    public static bool FindById(string id, out Item? item)
    {
        item = null;
        return !string.IsNullOrWhiteSpace(id) && PlayerUtil.Body.FindByIdSurface(id, out item);
    }

    public static List<Item> GetWearables()
    {
        return PlayerUtil.Body.GetAllWearables();
    }

    public static List<ItemInfo> GetWearableInfos()
    {
        return GetWearables().Select(i => i.Stats).Where(i => i != null).ToList();
    }

    public static bool HasWearableItem()
    {
        return HasItem(info => info.wearable);
    }

    public static bool HasWearableByTag(string tag)
    {
        return !string.IsNullOrWhiteSpace(tag) && GetWearableInfos().Any(i => i.HasTag(tag));
    }

    public static bool HasWearableByCategory(string category)
    {
        return !string.IsNullOrWhiteSpace(category) && GetWearableInfos().Any(i => i.category == category);
    }

    public static List<Item> GetWearablesByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : GetWearables().Where(i => i.Stats != null && i.Stats.HasTag(tag)).ToList();
    }

    public static List<Item> GetWearablesByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : GetWearables().Where(i => i.Stats != null && i.Stats.category == category).ToList();
    }

    public static List<ItemInfo> GetWearableInfosByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : GetWearableInfos().Where(i => i.HasTag(tag)).ToList();
    }

    public static List<ItemInfo> GetWearableInfosByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : GetWearableInfos().Where(i => i.category == category).ToList();
    }

    public static bool HasItemThorough(string id)
    {
        CheckUtil.CheckNotNullOrEmpty(id, nameof(id));
        return PlayerUtil.Body.FindByIdThorough(id, out _);
    }

    public static bool HasItemThoroughByTag(string tag)
    {
        return !string.IsNullOrWhiteSpace(tag) && GetAllItemInfosThorough().Any(i => i.HasTag(tag));
    }

    public static bool HasItemThoroughByCategory(string category)
    {
        return !string.IsNullOrWhiteSpace(category) && GetAllItemInfosThorough().Any(i => i.category == category);
    }

    public static List<Item> GetAllItemsThorough()
    {
        return PlayerUtil.Body.GetAllItemsThorough();
    }

    public static List<ItemInfo> GetAllItemInfosThorough()
    {
        return GetAllItemsThorough().Select(i => i.Stats).Where(i => i != null).ToList();
    }

    public static List<Item> GetItemsThoroughByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : GetAllItemsThorough().Where(i => i.Stats != null && i.Stats.HasTag(tag)).ToList();
    }

    public static List<Item> GetItemsThoroughByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : GetAllItemsThorough().Where(i => i.Stats != null && i.Stats.category == category).ToList();
    }

    public static List<ItemInfo> GetItemInfosThoroughByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : GetAllItemInfosThorough().Where(i => i.HasTag(tag)).ToList();
    }

    public static List<ItemInfo> GetItemInfosThoroughByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : GetAllItemInfosThorough().Where(i => i.category == category).ToList();
    }

    public static bool FindByIdThorough(string id, out Item? item)
    {
        item = null;
        return !string.IsNullOrWhiteSpace(id) && PlayerUtil.Body.FindByIdThorough(id, out item);
    }

    public static List<Item> GetAllItemsAll()
    {
        var items = new List<Item>();
        var hand = GetItemInHand();
        if (hand != null) items.Add(hand);
        items.AddRange(GetAllItems());
        items.AddRange(GetWearables());
        items.AddRange(GetAllItemsThorough());
        return items.Distinct().ToList();
    }

    public static List<ItemInfo> GetAllItemInfosAll()
    {
        return GetAllItemsAll().Select(i => i.Stats).Where(i => i != null).ToList();
    }

    public static string GetItemIdsString()
    {
        return GetAllItemIds().Count > 0
            ? string.Join(", ", GetAllItemIds())
            : "";
    }
}

internal static class BodyExt
{
    public static TResult Let<TResult>(this Body b, Func<Body, TResult> f)
    {
        return f(b);
    }
}