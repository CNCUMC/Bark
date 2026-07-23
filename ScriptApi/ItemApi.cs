using Bark.Tool;

namespace Bark.ScriptApi;

public class ItemApi
{
    public void SetCondition(string itemId, float condition)
    {
        if (InventoryUtil.FindById(itemId, out var item))
            ItemUtil.SetCondition(item, condition);
    }

    public void Repair(string itemId)
    {
        if (InventoryUtil.FindById(itemId, out var item))
            ItemUtil.Repair(item);
    }

    public void SetFavourited(string itemId, bool favourited)
    {
        if (InventoryUtil.FindById(itemId, out var item))
            ItemUtil.SetFavourited(item, favourited);
    }

    public void Destroy(string itemId)
    {
        if (InventoryUtil.FindById(itemId, out var item))
            ItemUtil.Destroy(item);
    }
}