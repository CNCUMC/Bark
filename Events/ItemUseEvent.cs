using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onItemUse")]
public class ItemUseEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;

    public Item? Item { get; set; }
}

[ScriptEvent("onItemHandUse")]
public class ItemHandUseEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
}

[ScriptEvent("onItemEquip")]
public class ItemEquipEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
}

[ScriptEvent("onItemUnequip")]
public class ItemUnequipEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
}

[ScriptEvent("onItemLimbUse")]
public class ItemLimbUseEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
    public int LimbIndex { get; set; } = -1;
    public string LimbName { get; set; } = string.Empty;
}

[ScriptEvent("onItemAttack")]
public class ItemAttackEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
}

[ScriptEvent("onItemWearAttack")]
public class ItemWearAttackEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
}

[ScriptEvent("onItemWearDamage")]
public class ItemWearDamageEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
    public float DamageAmount { get; set; }
}

[ScriptEvent("onItemDurability")]
public class ItemDurabilityEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
    public string Operator { get; set; } = "==";
    public float ThresholdValue { get; set; }
    public float CurrentValue { get; set; }
}

[ScriptEvent("onItemCapacity")]
public class ItemCapacityEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
    public string Operator { get; set; } = "==";
    public float ThresholdValue { get; set; }
    public float CurrentValue { get; set; }
}

public class ItemWearingEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
}

public class ItemHasEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
}

[ScriptEvent("onItemCharge")]
public class ItemChargeEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }

    public string Operator { get; set; } = "==";
    public float ThresholdValue { get; set; }
    public float CurrentValue { get; set; }
}
