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
}