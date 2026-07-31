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

// 穿戴攻击事件：穿着物品时进行近战攻击触发
[ScriptEvent("onItemWearAttack")]
public class ItemWearAttackEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
}

// 装备受损事件：穿戴物品受到伤害时触发
[ScriptEvent("onItemWearDamage")]
public class ItemWearDamageEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }

    // 本次受到的伤害量
    public float DamageAmount { get; set; }
}

// 耐久跨阈值事件：物品 condition 越过 durability 触发器阈值时触发
[ScriptEvent("onItemDurability")]
public class ItemDurabilityEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }

    // 触发时的运算符（"<", "<=", "==", ">=", ">"）
    public string Operator { get; set; } = "==";

    // 触发器设置的阈值（0.0~1.0）
    public float ThresholdValue { get; set; }

    // 当前 condition 百分比值（0.0~1.0）
    public float CurrentValue { get; set; }
}

// 容器容量事件：容器 fill 百分比越过 capacity_trigger 阈值时触发
[ScriptEvent("onItemCapacity")]
public class ItemCapacityEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }

    public string Operator { get; set; } = "==";
    public float ThresholdValue { get; set; }
    public float CurrentValue { get; set; }
}

// 电池电量事件：电池 charge 百分比越过 charge_trigger 阈值时触发
[ScriptEvent("onItemCharge")]
public class ItemChargeEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }

    public string Operator { get; set; } = "==";
    public float ThresholdValue { get; set; }
    public float CurrentValue { get; set; }
}