using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CUCoreLib.Registries;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class Inventory
{
    private static Body GetBody()
    {
        World.CheckForWorld();

        return PlayerCamera.main.body == null
            ? throw new InvalidOperationException(Locale("log.inventory.body_null"))
            : PlayerCamera.main.body;
    }

    public static bool IsSlotOccupied(int slot)
    {
        var body = GetBody();
        return body.HoldingItem(slot);
    }

    public static bool IsSlotEmpty(int slot)
    {
        return !IsSlotOccupied(slot);
    }

    public static Item GetItem(int slot)
    {
        var body = GetBody();
        return body.GetItem(slot);
    }

    public static ItemInfo? GetItemInfo(int slot)
    {
        return GetItem(slot).Stats;
    }

    public static string? GetItemId(int slot)
    {
        var item = GetItem(slot);
        return item.id;
    }

    public static bool HasItem(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(Locale("log.inventory.id.null_or_empty"), nameof(id));

        var body = GetBody();
        return body.HoldingItem(id);
    }

    public static bool HasItemThorough(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(Locale("log.inventory.id.null_or_empty"), nameof(id));

        var body = GetBody();
        return body.FindByIdThorough(id, out _);
    }

    public static bool HasAnyItem(params string[]? ids)
    {
        if (ids == null || ids.Length == 0)
            return false;

        var body = GetBody();
        return ids.Any(body.HoldingItem);
    }

    public static bool HasItem(Predicate<ItemInfo> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var body = GetBody();
        for (var i = 0; i < body.slots.Length; i++)
        {
            var info = body.GetItem(i)?.Stats;
            if (info != null && predicate(info))
                return true;
        }

        return false;
    }

    public static bool HasItemThorough(Predicate<ItemInfo> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var body = GetBody();
        return body.GetAllItemsThorough()
            .Any(item => item.Stats != null && predicate(item.Stats));
    }

    public static bool HasItemByTag(string tag)
    {
        return !string.IsNullOrWhiteSpace(tag)
               && HasItem(info => info.HasTag(tag));
    }

    public static bool HasItemByCategory(string category)
    {
        return !string.IsNullOrWhiteSpace(category)
               && HasItem(info => info.category == category);
    }

    public static bool HasWearableItem()
    {
        return HasItem(info => info.wearable);
    }

    public static int CountItem(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return 0;

        var body = GetBody();
        var count = 0;
        for (var i = 0; i < body.slots.Length; i++)
        {
            var item = body.GetItem(i);
            if (item != null && item.id == id)
                count++;
        }

        return count;
    }

    public static int CountItemThorough(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return 0;

        var body = GetBody();
        return body.GetAllItemsThorough().Count(item => item.id == id);
    }

    public static int CountItem(Predicate<ItemInfo> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var body = GetBody();
        var count = 0;
        for (var i = 0; i < body.slots.Length; i++)
        {
            var info = body.GetItem(i)?.Stats;
            if (info != null && predicate(info))
                count++;
        }

        return count;
    }

    public static int CountItemThorough(Predicate<ItemInfo> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var body = GetBody();
        return body.GetAllItemsThorough()
            .Count(item => item.Stats != null && predicate(item.Stats));
    }

    public static int CountItemByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? 0
            : CountItem(info => info.HasTag(tag));
    }

    public static List<Item> GetAllItems()
    {
        var body = GetBody();
        return body.GetAllItems();
    }

    public static List<string> GetAllItemIds()
    {
        return GetAllItems().Select(item => item.id).ToList();
    }

    public static List<Item> GetAllItemsThorough()
    {
        var body = GetBody();
        return body.GetAllItemsThorough();
    }

    public static List<ItemInfo> GetAllItemInfos()
    {
        return GetAllItems()
            .Select(item => item.Stats)
            .Where(info => info != null)
            .ToList();
    }

    public static List<ItemInfo> GetAllItemInfosThorough()
    {
        return GetAllItemsThorough()
            .Select(item => item.Stats)
            .Where(info => info != null)
            .ToList();
    }

    public static List<Item> GetWearables()
    {
        var body = GetBody();
        return body.GetAllWearables();
    }

    public static List<ItemInfo> GetWearableInfos()
    {
        return GetWearables()
            .Select(item => item.Stats)
            .Where(info => info != null)
            .ToList();
    }

    public static bool HasWearable(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(Locale("log.inventory.id.null_or_empty"), nameof(id));

        var body = GetBody();
        return body.HasWearable(id);
    }

    public static bool HasWearable(Predicate<ItemInfo> predicate)
    {
        return predicate == null
            ? throw new ArgumentNullException(nameof(predicate))
            : GetWearableInfos().Any(info => predicate(info));
    }

    public static int? FindFirstEmptySlot()
    {
        var body = GetBody();
        return body.FirstEmptySlot();
    }

    public static bool FindById(string id, out Item? item)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            item = null;
            return false;
        }

        var body = GetBody();
        return body.FindByIdSurface(id, out item);
    }

    public static bool FindByIdThorough(string id, out Item? item)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            item = null;
            return false;
        }

        var body = GetBody();
        return body.FindByIdThorough(id, out item);
    }

    public static bool FindByTag(string tag, out Item? item)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            item = null;
            return false;
        }

        var body = GetBody();
        return body.FindByTagThorough(tag, out item);
    }

    public static List<Item> FindAll(Predicate<ItemInfo> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var body = GetBody();
        var result = new List<Item>();
        for (var i = 0; i < body.slots.Length; i++)
        {
            var item = body.GetItem(i);
            if (item?.Stats != null && predicate(item.Stats))
                result.Add(item);
        }

        return result;
    }

    public static List<Item> FindAllThorough(Predicate<ItemInfo> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var body = GetBody();
        return body.GetAllItemsThorough()
            .Where(item => item.Stats != null && predicate(item.Stats))
            .ToList();
    }

    public static List<Item> FindAllByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : FindAll(info => info.HasTag(tag));
    }

    public static List<Item> FindAllByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : FindAll(info => info.category == category);
    }

    public static List<Item> FindAllWearableItems()
    {
        return FindAll(info => info.wearable);
    }

    public static int GetHandSlot()
    {
        var body = GetBody();
        return body.handSlot;
    }

    public static Item GetItemInHand()
    {
        var body = GetBody();
        return body.GetItem(body.handSlot);
    }

    public static ItemInfo? GetItemInfoInHand()
    {
        return GetItemInHand().Stats;
    }

    public static string? GetItemIdInHand()
    {
        return GetItemInHand().id;
    }

    public static bool IsItemInHand(Predicate<ItemInfo> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var info = GetItemInfoInHand();
        return info != null && predicate(info);
    }

    public static int GetSlotCount()
    {
        var body = GetBody();
        return body.slots.Length;
    }

    public static int GetOccupiedSlotCount()
    {
        var body = GetBody();
        var count = 0;
        for (var i = 0; i < body.slots.Length; i++)
            if (body.HoldingItem(i))
                count++;

        return count;
    }

    public static string GetItemIdsString()
    {
        var ids = GetAllItemIds();
        return ids.Count > 0 ? string.Join(", ", ids) : Locale("log.inventory.empty");
    }

    private static string Locale(string key, params object[] args)
    {
        var text = LocaleRegistry.Get("other", key, key);
        return args.Length > 0 ? string.Format(text, args) : text;
    }
}