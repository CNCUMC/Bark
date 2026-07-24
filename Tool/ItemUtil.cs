using Bark.ScriptApi;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Tool;

public static class ItemUtil
{
    public static void SetCondition(Item? item, float condition)
    {
        item?.SetCondition(Mathf.Clamp01(condition));
    }

    public static void Repair(Item? item)
    {
        SetCondition(item, 1f);
    }

    public static void SetFavourited(Item? item, bool favourited)
    {
        if (item != null) item.favourited = favourited;
    }

    public static void Destroy(Item? item)
    {
        if (item == null) return;
        if (item.ParentContainer() != null) item.transform.SetParent(null, true);
        Object.Destroy(item.gameObject);
    }

    [ScriptMethod]
    public static void SetCondition(string itemId, float condition)
    {
        if (InventoryUtil.FindById(itemId, out var item))
            SetCondition(item, condition);
    }

    [ScriptMethod]
    public static void Repair(string itemId)
    {
        if (InventoryUtil.FindById(itemId, out var item))
            Repair(item);
    }

    [ScriptMethod]
    public static void SetFavourited(string itemId, bool favourited)
    {
        if (InventoryUtil.FindById(itemId, out var item))
            SetFavourited(item, favourited);
    }

    [ScriptMethod]
    public static void Destroy(string itemId)
    {
        if (InventoryUtil.FindById(itemId, out var item))
            Destroy(item);
    }
}