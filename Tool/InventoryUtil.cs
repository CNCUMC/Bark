using System;
using System.Collections.Generic;
using System.Linq;
using Bark.ScriptApi;

namespace Bark.Tool;

public static class InventoryUtil
{
    // ============================================================
    // 内部辅助 - 统一 Body 获取入口
    // ============================================================

    private static Body? GetBody()
    {
        return PlayerCamera.main?.body;
    }

    // ============================================================
    // 手部物品查询
    // ============================================================

    [ScriptMethod]
    public static int GetHandSlot()
    {
        return GetBody()?.handSlot ?? 0;
    }

    public static Item? GetItemInHand()
    {
        var body = GetBody();
        return body?.GetItem(body.handSlot);
    }

    public static ItemInfo? GetItemInfoInHand()
    {
        return GetItemInHand()?.Stats;
    }

    [ScriptMethod]
    public static string GetItemIdInHand()
    {
        return GetItemInHand()?.id ?? string.Empty;
    }

    [ScriptMethod]
    public static bool IsHandEmpty()
    {
        return IsSlotEmpty(GetHandSlot());
    }

    public static bool IsHandOccupied()
    {
        return IsSlotOccupied(GetHandSlot());
    }

    [ScriptMethod]
    public static bool HasItemInHand(string id)
    {
        var item = GetItemInHand();
        return item != null && item.id == id;
    }

    [ScriptMethod]
    public static bool HasItemInHandByTag(string tag)
    {
        var info = GetItemInfoInHand();
        return info != null && info.HasTag(tag);
    }

    [ScriptMethod]
    public static bool HasItemInHandByCategory(string category)
    {
        var info = GetItemInfoInHand();
        return info != null && info.category == category;
    }

    // ============================================================
    // 背包槽位查询
    // ============================================================

    [ScriptMethod]
    public static int GetSlotCount()
    {
        return GetBody()?.slots?.Length ?? 0;
    }

    [ScriptMethod]
    public static bool IsSlotOccupied(int slot)
    {
        return GetBody()?.HoldingItem(slot) ?? false;
    }

    [ScriptMethod]
    public static bool IsSlotEmpty(int slot)
    {
        return !IsSlotOccupied(slot);
    }

    public static Item? GetItem(int slot)
    {
        return GetBody()?.GetItem(slot);
    }

    public static ItemInfo? GetItemInfo(int slot)
    {
        return GetItem(slot)?.Stats;
    }

    [ScriptMethod]
    public static string GetItemId(int slot)
    {
        return GetItem(slot)?.id ?? string.Empty;
    }

    [ScriptMethod]
    public static int FindFirstEmptySlot()
    {
        return GetBody()?.FirstEmptySlot() ?? -1;
    }

    // ============================================================
    // 物品查询 - By id / tag / category
    // ============================================================

    [ScriptMethod]
    public static bool HasItem(string id)
    {
        CheckUtil.CheckNotNullOrEmpty(id, nameof(id));
        return GetBody()?.HoldingItem(id) ?? false;
    }

    public static bool HasItem(Predicate<ItemInfo> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (GetBody() is not { } body) return false;
        for (var i = 0; i < body.slots.Length; i++)
        {
            var info = body.GetItem(i)?.Stats;
            if (info != null && predicate(info)) return true;
        }

        return false;
    }

    [ScriptMethod]
    public static bool HasItemByTag(string tag)
    {
        return !string.IsNullOrWhiteSpace(tag) && HasItem(info => info.HasTag(tag));
    }

    [ScriptMethod]
    public static bool HasItemByCategory(string category)
    {
        return !string.IsNullOrWhiteSpace(category) && HasItem(info => info.category == category);
    }

    [ScriptMethod]
    public static bool HasAnyItem(string[] ids)
    {
        if (ids is not { Length: > 0 }) return false;
        return GetBody() is { } body && ids.Any(body.HoldingItem);
    }

    [ScriptMethod]
    public static int CountItem(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return 0;
        if (GetBody() is not { } body) return 0;
        var c = 0;
        for (var i = 0; i < body.slots.Length; i++)
            if (body.GetItem(i)?.id == id)
                c++;
        return c;
    }

    // ============================================================
    // 全部物品收集
    // ============================================================

    public static List<Item> GetAllItems()
    {
        return GetBody()?.GetAllItems() ?? [];
    }

    public static List<ItemInfo> GetAllItemInfos()
    {
        return [.. GetAllItems().Select(i => i.Stats).Where(i => i != null)];
    }

    // -- [ScriptMethod] 数组返回包装（PuerTS 对 T[] 的支持优于 List<T>） --

    [ScriptMethod]
    public static string[] GetAllItemIds()
    {
        return [.. GetAllItems().Select(i => i.id)];
    }

    [ScriptMethod]
    public static string[] GetItemIdsByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : [.. GetAllItems().Where(i => i.Stats != null && i.Stats.HasTag(tag)).Select(i => i.id)];
    }

    [ScriptMethod]
    public static string[] GetItemIdsByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : [.. GetAllItems().Where(i => i.Stats != null && i.Stats.category == category).Select(i => i.id)];
    }

    public static List<Item> GetItemsByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : [.. GetAllItems().Where(i => i.Stats != null && i.Stats.HasTag(tag))];
    }

    public static List<Item> GetItemsByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : [.. GetAllItems().Where(i => i.Stats != null && i.Stats.category == category)];
    }

    public static List<ItemInfo> GetItemInfosByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : [.. GetAllItemInfos().Where(i => i.HasTag(tag))];
    }

    public static List<ItemInfo> GetItemInfosByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : [.. GetAllItemInfos().Where(i => i.category == category)];
    }

    public static bool FindById(string id, out Item? item)
    {
        item = null;
        if (string.IsNullOrWhiteSpace(id)) return false;
        return GetBody() is { } body && body.FindByIdSurface(id, out item);
    }

    // ============================================================
    // 穿戴装备查询
    // ============================================================

    public static List<Item> GetWearables()
    {
        return GetBody()?.GetAllWearables() ?? [];
    }

    public static List<ItemInfo> GetWearableInfos()
    {
        return [.. GetWearables().Select(i => i.Stats).Where(i => i != null)];
    }

    [ScriptMethod]
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

    // -- [ScriptMethod] 穿戴装备数组返回包装 --

    [ScriptMethod]
    public static string[] GetWearableItemIds()
    {
        return [.. GetWearables().Select(i => i.id)];
    }

    [ScriptMethod]
    public static string[] GetWearableIdsByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : [.. GetWearables().Where(i => i.Stats != null && i.Stats.HasTag(tag)).Select(i => i.id)];
    }

    [ScriptMethod]
    public static string[] GetWearableIdsByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : [.. GetWearables().Where(i => i.Stats != null && i.Stats.category == category).Select(i => i.id)];
    }

    public static List<Item> GetWearablesByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : [.. GetWearables().Where(i => i.Stats != null && i.Stats.HasTag(tag))];
    }

    public static List<Item> GetWearablesByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : [.. GetWearables().Where(i => i.Stats != null && i.Stats.category == category)];
    }

    public static List<ItemInfo> GetWearableInfosByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : [.. GetWearableInfos().Where(i => i.HasTag(tag))];
    }

    public static List<ItemInfo> GetWearableInfosByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : [.. GetWearableInfos().Where(i => i.category == category)];
    }

    // ============================================================
    // Thorough 查询（深度搜索，包括容器内物品）
    // ============================================================

    [ScriptMethod]
    public static bool HasItemThorough(string id)
    {
        CheckUtil.CheckNotNullOrEmpty(id, nameof(id));
        return GetBody() is { } body && body.FindByIdThorough(id, out _);
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
        return GetBody()?.GetAllItemsThorough() ?? [];
    }

    public static List<ItemInfo> GetAllItemInfosThorough()
    {
        return [.. GetAllItemsThorough().Select(i => i.Stats).Where(i => i != null)];
    }

    public static List<Item> GetItemsThoroughByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : [.. GetAllItemsThorough().Where(i => i.Stats != null && i.Stats.HasTag(tag))];
    }

    public static List<Item> GetItemsThoroughByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : [.. GetAllItemsThorough().Where(i => i.Stats != null && i.Stats.category == category)];
    }

    public static List<ItemInfo> GetItemInfosThoroughByTag(string tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? []
            : [.. GetAllItemInfosThorough().Where(i => i.HasTag(tag))];
    }

    public static List<ItemInfo> GetItemInfosThoroughByCategory(string category)
    {
        return string.IsNullOrWhiteSpace(category)
            ? []
            : [.. GetAllItemInfosThorough().Where(i => i.category == category)];
    }

    public static bool FindByIdThorough(string id, out Item? item)
    {
        item = null;
        if (string.IsNullOrWhiteSpace(id)) return false;
        return GetBody() is { } body && body.FindByIdThorough(id, out item);
    }

    // ============================================================
    // 聚合查询（手部 + 背包 + 装备 + 容器内，去重）
    // ============================================================

    public static List<Item> GetAllItemsAll()
    {
        if (GetBody() is not { } body) return [];
        var items = new List<Item>();
        var hand = GetItemInHand();
        if (hand != null) items.Add(hand);
        items.AddRange(body.GetAllItems());
        items.AddRange(body.GetAllWearables());
        items.AddRange(body.GetAllItemsThorough());
        return [.. items.Distinct()];
    }

    public static List<ItemInfo> GetAllItemInfosAll()
    {
        return [.. GetAllItemsAll().Select(i => i.Stats).Where(i => i != null)];
    }

    [ScriptMethod]
    public static string[] GetAllItemIdsAll()
    {
        return [.. GetAllItemsAll().Select(i => i.id)];
    }

    public static string GetItemIdsString()
    {
        var ids = GetAllItemIds();
        return ids.Length > 0
            ? string.Join(", ", ids)
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