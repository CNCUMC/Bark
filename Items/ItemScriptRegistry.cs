using System;
using System.Collections.Generic;
using System.Linq;
using Bark.Script;

namespace Bark.Items;

// 物品脚本映射存储：itemId → (ScriptEngine, 脚本文件路径列表按分类分组)
// 供 ItemScriptRunner 在物品事件触发时查找并执行脚本。
public static class ItemScriptRegistry
{
    // itemId → 脚本映射记录
    private static readonly Dictionary<string, ItemScriptEntry> Entries = new();

    // 注册一个物品的脚本映射。itemId 为物品 ID，def 为完整 ItemDef（收集所有脚本来源），
    // engine 为模组的 ScriptEngine（每个模组一个），modId 为模组 ID，modDir 为模组目录。
    public static void Register(string itemId, ItemDef def, ScriptEngine engine, string modId,
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

    // 注销指定物品的脚本映射（热重载时调用）
    public static void Unregister(string itemId)
    {
        Entries.Remove(itemId);
    }

    // 获取指定物品的脚本映射，未注册时返回 null
    public static ItemScriptEntry? GetEntry(string itemId)
    {
        return Entries.GetValueOrDefault(itemId);
    }

    // 判断 ItemDef 是否没有任何脚本需要注册
    private static bool IsEmpty(ItemDef def)
    {
        var s = def.Script;
        var hasPassive = s != null && (
            s.Attack.Count > 0 ||
            s.UseOnLimb.Count > 0 ||
            s.InBackpack.Count > 0 ||
            s.InHand.Count > 0 ||
            s.NotInHand.Count > 0 ||
            s.Durability.Count > 0);

        var hasUse = def.Use is { Count: > 0 } useList &&
            useList.Any(e => e.Script.Count > 0);

        var w = def.Wearable;
        var hasWearable = w != null && (
            w.Equip.Count > 0 ||
            w.Unequip.Count > 0 ||
            w.Attack.Count > 0 ||
            w.Damage.Count > 0);

        var hasContainer = def.Container?.CapacityTrigger is { Count: > 0 } ct &&
            ct.Any(t => t.Script.Count > 0);

        var hasBattery = def.Battery?.ChargeTrigger is { Count: > 0 } bt &&
            bt.Any(t => t.Script.Count > 0);

        return !hasPassive && !hasUse && !hasWearable && !hasContainer && !hasBattery;
    }
}

// 单个物品的脚本映射记录：包含引擎引用、模组 ID、脚本文件路径列表（按分类分组）
public class ItemScriptEntry(ScriptEngine engine, ItemDef def, string modId, string modDir)
{
    public readonly string ModDir = modDir;
    public readonly string ModId = modId;
    public ScriptEngine Engine = engine;

    // ---- script（被动检测） ----
    public readonly List<string> Attack = def.Script?.Attack ?? [];
    public readonly List<string> UseOnLimb = def.Script?.UseOnLimb ?? [];
    public readonly List<string> InBackpack = def.Script?.InBackpack ?? [];
    public readonly List<string> InHand = def.Script?.InHand ?? [];
    public readonly List<string> NotInHand = def.Script?.NotInHand ?? [];
    public readonly List<ConditionTriggerDef> Durability = def.Script?.Durability ?? [];

    // ---- use（主动使用，数组形式） ----
    public readonly List<UseEntryDef> UseEntries = def.Use ?? [];

    // ---- wearable（穿戴脚本） ----
    public readonly List<string> WearEquip = def.Wearable?.Equip ?? [];
    public readonly List<string> WearUnequip = def.Wearable?.Unequip ?? [];
    public readonly List<string> WearAttack = def.Wearable?.Attack ?? [];
    public readonly List<string> WearDamage = def.Wearable?.Damage ?? [];

    // ---- 条件触发器 ----
    public readonly List<ConditionTriggerDef> CapacityTrigger = def.Container?.CapacityTrigger ?? [];
    public readonly List<ConditionTriggerDef> ChargeTrigger = def.Battery?.ChargeTrigger ?? [];

    // 获取匹配背包使用的脚本（排除 hand-only 和 limb 条目）
    public List<string> GetUseScriptsForBackpack()
    {
        var result = new List<string>();
        foreach (var entry in UseEntries)
        {
            if (entry.LimbSlot is { Count: > 0 }) continue;
            if (IsHandOnlyEntry(entry)) continue;
            result.AddRange(entry.Script);
        }
        return result;
    }

    // 获取匹配手持使用的脚本（hand-only 或 all-slots 条目，排除 limb）
    public List<string> GetUseScriptsForHand()
    {
        var result = new List<string>();
        foreach (var entry in UseEntries)
        {
            if (entry.LimbSlot is { Count: > 0 }) continue;
            if (IsHandOnlyEntry(entry) || IsAllSlotsEntry(entry))
                result.AddRange(entry.Script);
        }
        return result;
    }

    // 获取匹配肢体使用的脚本
    public List<string> GetUseScriptsForLimb(string limbName)
    {
        var result = new List<string>();
        foreach (var entry in UseEntries)
        {
            if (entry.LimbSlot is not { Count: > 0 }) continue;
            if (entry.LimbSlot.Count == 0 ||
                entry.LimbSlot.Contains(limbName, StringComparer.OrdinalIgnoreCase))
                result.AddRange(entry.Script);
        }
        return result;
    }

    private static bool IsHandOnlyEntry(UseEntryDef entry)
    {
        return entry.Slot is { Count: > 0 } list &&
            list.Any(s => s is string str && string.Equals(str, "hand", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsAllSlotsEntry(UseEntryDef entry)
    {
        return entry.Slot is null or { Count: 0 };
    }
}
