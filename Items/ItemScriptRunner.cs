using System;
using System.Collections.Generic;
using System.Linq;
using Bark.Events;
using Bark.Tool;

namespace Bark.Items;

public static class ItemScriptRunner
{
    private const string Guid = Plugin.Guid + ".items";

    public static void Listen()
    {
        EventUtil.On<ItemUseEvent>(OnItemUse, Guid);
        EventUtil.On<ItemHandUseEvent>(OnItemHandUse, Guid);
        EventUtil.On<ItemEquipEvent>(OnItemEquip, Guid);
        EventUtil.On<ItemUnequipEvent>(OnItemUnequip, Guid);
        EventUtil.On<ItemLimbUseEvent>(OnItemLimbUse, Guid);
        EventUtil.On<ItemAttackEvent>(OnItemAttack, Guid);
        EventUtil.On<ItemWearAttackEvent>(OnItemWearAttack, Guid);
        EventUtil.On<ItemWearDamageEvent>(OnItemWearDamage, Guid);
        EventUtil.On<ItemDurabilityEvent>(OnItemDurability, Guid);
        EventUtil.On<ItemCapacityEvent>(OnItemCapacity, Guid);
        EventUtil.On<ItemChargeEvent>(OnItemCharge, Guid);
        EventUtil.On<ItemHasEvent>(OnItemHas, Guid);
        EventUtil.On<ItemWearingEvent>(OnItemWearing, Guid);
    }

    public static void Stop()
    {
        EventUtil.UnregisterAll(Guid);
    }

    private static void OnItemUse(ItemUseEvent evt)
    {
        ExecuteUseScripts(evt.ItemId, evt.Item, "use", e => e.GetUseScriptsForBackpack());
    }

    private static void OnItemHandUse(ItemHandUseEvent evt)
    {
        ExecuteUseScripts(evt.ItemId, evt.Item, "use_in_hand",
            e => e.GetUseScriptsForHand());
    }

    private static void OnItemEquip(ItemEquipEvent evt)
    {
        ExecuteScripts(evt.ItemId, evt.Item, "equip", e => e.WearEquip);
    }

    private static void OnItemUnequip(ItemUnequipEvent evt)
    {
        ExecuteScripts(evt.ItemId, evt.Item, "unequip", e => e.WearUnequip);
    }

    private static void OnItemLimbUse(ItemLimbUseEvent evt)
    {
        if (string.IsNullOrEmpty(evt.ItemId))
            return;

        var entry = ItemScriptRegistry.GetEntry(evt.ItemId);
        if (entry is null)
            return;

        ExecuteList(entry, evt.ItemId, evt.Item, "use_on_limb", entry.UseOnLimb);

        var limbScripts = entry.GetUseScriptsForLimb(evt.LimbName);
        ExecuteList(entry, evt.ItemId, evt.Item, "use_limb_slot", limbScripts);
    }

    private static void OnItemAttack(ItemAttackEvent evt)
    {
        ExecuteScripts(evt.ItemId, evt.Item, "attack", e => e.Attack);
    }

    private static void OnItemWearAttack(ItemWearAttackEvent evt)
    {
        ExecuteScripts(evt.ItemId, evt.Item, "wear_attack", e => e.WearAttack);
    }

    private static void OnItemWearDamage(ItemWearDamageEvent evt)
    {
        ExecuteScripts(evt.ItemId, evt.Item, "wear_damage", e => e.WearDamage);
    }

    private static void OnItemDurability(ItemDurabilityEvent evt)
    {
        ItemScriptContext.CurrentTriggerOperator = evt.Operator;
        ItemScriptContext.CurrentTriggerValue = evt.CurrentValue;
        ItemScriptContext.CurrentTriggerThreshold = evt.ThresholdValue;
        try
        {
            ExecuteScripts(evt.ItemId, evt.Item, "durability", e =>
                e.Durability.SelectMany(t => t.Script).ToList());
        }
        finally
        {
            ItemScriptContext.ClearTriggerContext();
        }
    }

    private static void OnItemCapacity(ItemCapacityEvent evt)
    {
        ItemScriptContext.CurrentTriggerOperator = evt.Operator;
        ItemScriptContext.CurrentTriggerValue = evt.CurrentValue;
        ItemScriptContext.CurrentTriggerThreshold = evt.ThresholdValue;
        try
        {
            ExecuteScripts(evt.ItemId, evt.Item, "capacity", e =>
                e.CapacityTrigger.SelectMany(t => t.Script).ToList());
        }
        finally
        {
            ItemScriptContext.ClearTriggerContext();
        }
    }

    private static void OnItemCharge(ItemChargeEvent evt)
    {
        ItemScriptContext.CurrentTriggerOperator = evt.Operator;
        ItemScriptContext.CurrentTriggerValue = evt.CurrentValue;
        ItemScriptContext.CurrentTriggerThreshold = evt.ThresholdValue;
        try
        {
            ExecuteScripts(evt.ItemId, evt.Item, "charge", e =>
                e.ChargeTrigger.SelectMany(t => t.Script).ToList());
        }
        finally
        {
            ItemScriptContext.ClearTriggerContext();
        }
    }

    private static void OnItemHas(ItemHasEvent evt)
    {
        ExecuteScripts(evt.ItemId, null, "has", e => e.Has);
    }

    private static void OnItemWearing(ItemWearingEvent evt)
    {
        ExecuteScripts(evt.ItemId, evt.Item, "wearing", e => e.WearWearing);
    }

    private static void ExecuteUseScripts(string itemId, Item? item, string action,
        Func<ItemScriptEntry, List<string>> getScriptList)
    {
        if (string.IsNullOrEmpty(itemId))
            return;

        var entry = ItemScriptRegistry.GetEntry(itemId);
        if (entry is null)
            return;

        var scripts = getScriptList(entry);
        ExecuteList(entry, itemId, item, action, scripts);
    }

    private static void ExecuteScripts(string itemId, Item? item, string action,
        Func<ItemScriptEntry, List<string>> getScriptList)
    {
        if (string.IsNullOrEmpty(itemId))
            return;

        var entry = ItemScriptRegistry.GetEntry(itemId);
        if (entry is null)
            return;

        var scripts = getScriptList(entry);
        ExecuteList(entry, itemId, item, action, scripts);
    }

    private static void ExecuteList(ItemScriptEntry entry, string itemId, Item? item,
        string action,
        List<string> scripts)
    {
        if (scripts.Count == 0)
            return;

        foreach (var relativePath in scripts.Where(relativePath =>
            !string.IsNullOrEmpty(relativePath)))
            ScriptUtil.Execute(entry.ModId, relativePath, itemId, item, action);
    }
}
