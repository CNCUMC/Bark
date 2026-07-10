using System;
using System.Collections.Generic;
using System.Linq;
using Bark.BetterCCL;

namespace Bark.Tool;

public static class InventoryUtil
{
    public static bool IsSlotOccupied(int slot) => PlayerUtil.Body.HoldingItem(slot);
    public static bool IsSlotEmpty(int slot) => !IsSlotOccupied(slot);
    public static Item GetItem(int slot) => PlayerUtil.Body.GetItem(slot);
    public static ItemInfo? GetItemInfo(int slot) => GetItem(slot).Stats;
    public static string? GetItemId(int slot) => GetItem(slot).id;

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

    public static bool HasItemByTag(string tag) => !string.IsNullOrWhiteSpace(tag) && HasItem(info => info.HasTag(tag));
    public static bool HasItemByCategory(string cat) =>
        !string.IsNullOrWhiteSpace(cat) && HasItem(info => info.category == cat);
    public static bool HasWearableItem() => HasItem(info => info.wearable);

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

    public static List<Item> GetAllItems() => PlayerUtil.Body.GetAllItems();
    public static List<Item> GetAllItemsThorough() => PlayerUtil.Body.GetAllItemsThorough();
    public static List<ItemInfo> GetAllItemInfos() => GetAllItems().Select(i => i.Stats).Where(i => i != null).ToList();

    public static List<ItemInfo> GetAllItemInfosThorough()
    {
        return GetAllItemsThorough().Select(i => i.Stats)
            .Where(i => i != null).ToList();
    }

    public static List<string> GetAllItemIds() => GetAllItems().Select(i => i.id).ToList();
    public static List<Item> GetWearables() => PlayerUtil.Body.GetAllWearables();

    public static List<ItemInfo> GetWearableInfos()
    {
        return GetWearables().Select(i => i.Stats).Where(i => i != null).ToList();
    }

    public static int? FindFirstEmptySlot() => PlayerUtil.Body.FirstEmptySlot();

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

    public static int GetHandSlot() => PlayerUtil.Body.handSlot;
    public static Item GetItemInHand() => PlayerUtil.Body.GetItem(PlayerUtil.Body.handSlot);
    public static ItemInfo? GetItemInfoInHand() => GetItemInHand().Stats;
    public static string? GetItemIdInHand() => GetItemInHand().id;
    public static int GetSlotCount() => PlayerUtil.Body.slots.Length;

    public static string GetItemIdsString()
    {
        var ids = GetAllItemIds();
        return ids.Count > 0 ? string.Join(", ", ids) : LocaleLog("inventory.empty");
    }

    private static string LocaleLog(string key, params object[] args) => BetterLocale.GetLog(key, args);
}

internal static class BodyExt
{
    public static TResult Let<TResult>(this Body b, Func<Body, TResult> f)
    {
        return f(b);
    }
}