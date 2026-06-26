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
    private static readonly ManualLogSource Logger = Plugin.Logger;

    private static Body GetBody() { WorldUtil.CheckForWorld(); return GameInstances.Body ?? throw new InvalidOperationException(Loc("inventory.body_null")); }

    public static bool IsSlotOccupied(int slot) => GetBody().HoldingItem(slot);
    public static bool IsSlotEmpty(int slot) => !IsSlotOccupied(slot);
    public static Item GetItem(int slot) => GetBody().GetItem(slot);
    public static ItemInfo? GetItemInfo(int slot) => GetItem(slot)?.Stats;
    public static string? GetItemId(int slot) => GetItem(slot)?.id;

    public static bool HasItem(string id) { if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(Loc("inventory.id.null_or_empty"), nameof(id)); return GetBody().HoldingItem(id); }
    public static bool HasItemThorough(string id) { if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(Loc("inventory.id.null_or_empty"), nameof(id)); return GetBody().FindByIdThorough(id, out _); }
    public static bool HasAnyItem(params string[] ids) => ids is { Length: > 0 } && GetBody().Let(b => ids.Any(b.HoldingItem));
    public static bool HasItem(Predicate<ItemInfo> p) { if (p == null) throw new ArgumentNullException(nameof(p)); var b = GetBody(); for (var i = 0; i < b.slots.Length; i++) { var info = b.GetItem(i)?.Stats; if (info != null && p(info)) return true; } return false; }
    public static bool HasItemByTag(string tag) => !string.IsNullOrWhiteSpace(tag) && HasItem(info => info.HasTag(tag));
    public static bool HasItemByCategory(string cat) => !string.IsNullOrWhiteSpace(cat) && HasItem(info => info.category == cat);
    public static bool HasWearableItem() => HasItem(info => info.wearable);
    public static int CountItem(string id) { if (string.IsNullOrWhiteSpace(id)) return 0; var b = GetBody(); var c = 0; for (var i = 0; i < b.slots.Length; i++) if (b.GetItem(i)?.id == id) c++; return c; }
    public static List<Item> GetAllItems() => GetBody().GetAllItems();
    public static List<string> GetAllItemIds() => GetAllItems().Select(item => item.id).ToList();
    public static List<Item> GetAllItemsThorough() => GetBody().GetAllItemsThorough();
    public static List<Item> GetWearables() => GetBody().GetAllWearables();
    public static int? FindFirstEmptySlot() => GetBody().FirstEmptySlot();
    public static bool FindById(string id, out Item? item) { item = null; return !string.IsNullOrWhiteSpace(id) && GetBody().FindByIdSurface(id, out item); }
    public static bool FindByIdThorough(string id, out Item? item) { item = null; return !string.IsNullOrWhiteSpace(id) && GetBody().FindByIdThorough(id, out item); }
    public static int GetHandSlot() => GetBody().handSlot;
    public static Item GetItemInHand() => GetBody().GetItem(GetBody().handSlot);
    public static ItemInfo? GetItemInfoInHand() => GetItemInHand()?.Stats;
    public static string? GetItemIdInHand() => GetItemInHand()?.id;
    public static int GetSlotCount() => GetBody().slots.Length;
    public static string GetItemIdsString() { var ids = GetAllItemIds(); return ids.Count > 0 ? string.Join(", ", ids) : Loc("inventory.empty"); }

    private static string Loc(string key, params object[] args) => BetterLocale.Other("log." + key, args);
}

internal static class BodyExt { public static TResult Let<TResult>(this Body b, Func<Body, TResult> f) => f(b); }
