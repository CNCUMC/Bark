using System.Linq;
using Bark.Tool;

namespace Bark.ScriptApi;

public class InventorApi
{
    public int GetHandSlot()
    {
        return InventoryUtil.GetHandSlot();
    }

    public string? GetHandItemId()
    {
        return InventoryUtil.GetItemIdInHand();
    }

    public bool IsHandEmpty()
    {
        return InventoryUtil.IsHandEmpty();
    }

    public bool HasItemInHand(string id)
    {
        return InventoryUtil.HasItemInHand(id);
    }

    public bool HasItemInHandByTag(string tag)
    {
        return InventoryUtil.HasItemInHandByTag(tag);
    }

    public bool HasItemInHandByCategory(string category)
    {
        return InventoryUtil.HasItemInHandByCategory(category);
    }

    public int GetSlotCount()
    {
        return InventoryUtil.GetSlotCount();
    }

    public bool IsSlotOccupied(int slot)
    {
        return InventoryUtil.IsSlotOccupied(slot);
    }

    public bool IsSlotEmpty(int slot)
    {
        return InventoryUtil.IsSlotEmpty(slot);
    }

    public string? GetItemId(int slot)
    {
        return InventoryUtil.GetItemId(slot);
    }

    public int FindFirstEmptySlot()
    {
        return InventoryUtil.FindFirstEmptySlot() ?? -1;
    }
    
    public bool HasItem(string id)
    {
        return InventoryUtil.HasItem(id);
    }

    public bool HasItemByTag(string tag)
    {
        return InventoryUtil.HasItemByTag(tag);
    }

    public bool HasItemByCategory(string category)
    {
        return InventoryUtil.HasItemByCategory(category);
    }

    public bool HasAnyItem(params string[] ids)
    {
        return InventoryUtil.HasAnyItem(ids);
    }

    public int CountItem(string id)
    {
        return InventoryUtil.CountItem(id);
    }

    public string[] GetAllItemIds()
    {
        return [.. InventoryUtil.GetAllItemIds()];
    }

    public string[] GetItemIdsByTag(string tag)
    {
        return [.. InventoryUtil.GetItemsByTag(tag).Select(i => i.id)];
    }

    public string[] GetItemIdsByCategory(string category)
    {
        return [.. InventoryUtil.GetItemsByCategory(category).Select(i => i.id)];
    }
    
    public bool HasWearableItem()
    {
        return InventoryUtil.HasWearableItem();
    }

    public string[] GetWearableItemIds()
    {
        return [.. InventoryUtil.GetWearables().Select(i => i.id)];
    }

    public string[] GetWearableIdsByTag(string tag)
    {
        return [.. InventoryUtil.GetWearablesByTag(tag).Select(i => i.id)];
    }

    public string[] GetWearableIdsByCategory(string category)
    {
        return [.. InventoryUtil.GetWearablesByCategory(category).Select(i => i.id)];
    }

    public bool HasItemThorough(string id)
    {
        return InventoryUtil.HasItemThorough(id);
    }

    public string[] GetAllItemIdsAll()
    {
        return [.. InventoryUtil.GetAllItemsAll().Select(i => i.id)];
    }
}