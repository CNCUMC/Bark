using System;
using System.Collections.Generic;
using System.Linq;
using Bark.Script;

namespace Bark.Items;

public static class ItemScriptRegistry
{
    private static readonly Dictionary<string, ItemScriptEntry> Entries = new();

    public static IEnumerable<KeyValuePair<string, ItemScriptEntry>> AllEntries =>
        Entries;

    public static void Register(string itemId, ItemDef def, ScriptEngine engine,
        string modId,
        string modDir)
    {
        if (string.IsNullOrEmpty(itemId))
            throw new ArgumentNullException(nameof(itemId));
        if (def is null)
            throw new ArgumentNullException(nameof(def));
        if (engine is null)
            throw new ArgumentNullException(nameof(engine));
        if (string.IsNullOrEmpty(modId))
            throw new ArgumentNullException(nameof(modId));
        if (string.IsNullOrEmpty(modDir))
            throw new ArgumentNullException(nameof(modDir));

        if (IsEmpty(def))
            return;

        Entries[itemId] = new ItemScriptEntry(engine, def, modId, modDir);
    }

    public static void Unregister(string itemId)
    {
        Entries.Remove(itemId);
    }

    public static ItemScriptEntry? GetEntry(string itemId)
    {
        return Entries.GetValueOrDefault(itemId);
    }

    private static bool IsEmpty(ItemDef def)
    {
        var s = def.Script;
        var hasPassive = s != null
            && (
                s.Attack.Count > 0
                || s.UseOnLimb.Count > 0
                || s.Has.Count > 0
                || s.InHand.Count > 0
                || s.NotInHand.Count > 0
                || s.Durability.Count > 0);

        var hasUse = def.Use is { Count: > 0 } useList
            && useList.Any(e => e.Script.Count > 0);

        var w = def.Wearable;
        var hasWearable = w != null
            && (
                w.Equip.Count > 0
                || w.Unequip.Count > 0
                || w.Attack.Count > 0
                || w.Damage.Count > 0
                || w.Wearing.Count > 0);

        var hasContainer = def.Container?.CapacityTrigger is { Count: > 0 } ct
            && ct.Any(t => t.Script.Count > 0);

        var hasBattery = def.Battery?.ChargeTrigger is { Count: > 0 } bt
            && bt.Any(t => t.Script.Count > 0);

        return !hasPassive && !hasUse && !hasWearable && !hasContainer && !hasBattery;
    }
}

public class ItemScriptEntry(
    ScriptEngine engine,
    ItemDef def,
    string modId,
    string modDir)
{
    public readonly List<string> Attack = def.Script?.Attack ?? [];

    public readonly List<ConditionTriggerDef> CapacityTrigger =
        def.Container?.CapacityTrigger ?? [];

    public readonly List<ConditionTriggerDef> ChargeTrigger =
        def.Battery?.ChargeTrigger ?? [];

    public readonly List<ConditionTriggerDef> Durability = def.Script?.Durability ?? [];
    public readonly List<string> Has = def.Script?.Has ?? [];
    public readonly List<string> InHand = def.Script?.InHand ?? [];
    public readonly string ModDir = modDir;
    public readonly string ModId = modId;
    public readonly List<string> NotInHand = def.Script?.NotInHand ?? [];

    public readonly List<UseEntryDef> UseEntries = def.Use ?? [];
    public readonly List<string> UseOnLimb = def.Script?.UseOnLimb ?? [];
    public readonly List<string> WearAttack = def.Wearable?.Attack ?? [];
    public readonly List<string> WearDamage = def.Wearable?.Damage ?? [];

    public readonly List<string> WearEquip = def.Wearable?.Equip ?? [];
    public readonly List<string> WearUnequip = def.Wearable?.Unequip ?? [];
    public readonly List<string> WearWearing = def.Wearable?.Wearing ?? [];
    public ScriptEngine Engine = engine;

    public List<string> GetUseScriptsForBackpack()
    {
        var result = new List<string>();
        foreach (var entry in UseEntries
            .Where(entry => entry.LimbSlot is not { Count: > 0 })
            .Where(entry => !IsHandOnlyEntry(entry)))
            result.AddRange(entry.Script);

        return result;
    }

    public List<string> GetUseScriptsForHand()
    {
        var result = new List<string>();
        foreach (var entry in UseEntries
            .Where(entry => entry.LimbSlot is not { Count: > 0 })
            .Where(entry => IsHandOnlyEntry(entry) || IsAllSlotsEntry(entry)))
            result.AddRange(entry.Script);

        return result;
    }

    public List<string> GetUseScriptsForLimb(string limbName)
    {
        var result = new List<string>();
        foreach (var entry in UseEntries)
        {
            if (entry.LimbSlot is not { Count: > 0 }) continue;
            if (entry.LimbSlot.Count == 0
                || entry.LimbSlot.Contains(limbName, StringComparer.OrdinalIgnoreCase))
                result.AddRange(entry.Script);
        }

        return result;
    }

    private static bool IsHandOnlyEntry(UseEntryDef entry)
    {
        return entry.Slot is { Count: > 0 } list
            && list.Any(s =>
                s is string str
                && string.Equals(str, "hand", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsAllSlotsEntry(UseEntryDef entry)
    {
        return entry.Slot is null or { Count: 0 };
    }
}
