using Bark.Event;

namespace Bark.Events;

// 物品使用事件：玩家在背包中使用物品时触发（补丁 Body.UseItem）
[ScriptEvent("onItemUse")]
public class ItemUseEvent : BarkEvent
{
    // 物品 ID（如 "arrow"）
    public string ItemId { get; set; } = string.Empty;

    // 物品实例引用（可用于读写 condition 等）
    public Item? Item { get; set; }
}

// 手持物品使用事件：玩家直接使用手中物品时触发（补丁 Body.UseItemInHand）
[ScriptEvent("onItemHandUse")]
public class ItemHandUseEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
}

// 物品装备事件：物品被穿戴上时触发
[ScriptEvent("onItemEquip")]
public class ItemEquipEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
}

// 物品脱卸事件：物品被卸下时触发
[ScriptEvent("onItemUnequip")]
public class ItemUnequipEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
}

// 物品对肢体使用事件：物品被使用在某个肢体上时触发
[ScriptEvent("onItemLimbUse")]
public class ItemLimbUseEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
    // 目标肢体的索引（Body.limbs 数组下标），-1 表示未知
    public int LimbIndex { get; set; } = -1;
    // 目标肢体名称
    public string LimbName { get; set; } = string.Empty;
}

// 物品攻击事件：手持物品进行近战攻击时触发
[ScriptEvent("onItemAttack")]
public class ItemAttackEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
}
