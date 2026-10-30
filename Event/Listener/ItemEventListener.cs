using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bark.Events;
using Bark.Items;
using Bark.Tool;
using HarmonyLib;
using UnityEngine;

namespace Bark.Event.Listener;

public static class ItemEventListener
{
    private const float PollInterval = 0.5f;

    private static readonly Dictionary<int, string> KnownWearableIds = new();
    private static readonly Dictionary<int, float> LimbConditionTracker = new();

    private static readonly Dictionary<string, float> TriggerLastValues = new();

    private static Coroutine? _equipCoroutine;
    private static Coroutine? _limbCoroutine;
    private static Coroutine? _attackCoroutine;
    private static Coroutine? _passiveCoroutine;
    private static Coroutine? _durabilityCoroutine;
    private static Coroutine? _capacityCoroutine;
    private static Coroutine? _chargeCoroutine;
    private static Coroutine? _hasCoroutine;
    private static Coroutine? _wearingCoroutine;
    private static MonoBehaviour? _runner;

    private static string? _lastHandItemId;

    private static float _lastHandCondition = 1f;
    private static int _lastAttackFrame;

    private static readonly Dictionary<string, float> LastWearCondition = new();

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;

        _equipCoroutine ??= runner.StartCoroutine(PollEquipChange());
        _limbCoroutine ??= runner.StartCoroutine(PollLimbUse());
        _attackCoroutine ??= runner.StartCoroutine(PollItemAttack());
        _passiveCoroutine ??= runner.StartCoroutine(PollPassiveStates());
        _durabilityCoroutine ??= runner.StartCoroutine(PollDurability());
        _capacityCoroutine ??= runner.StartCoroutine(PollCapacity());
        _chargeCoroutine ??= runner.StartCoroutine(PollCharge());
        _hasCoroutine ??= runner.StartCoroutine(PollHasItems());
        _wearingCoroutine ??= runner.StartCoroutine(PollWearPassive());
    }

    internal static void Stop()
    {
        if (_runner == null) return;

        StopCoroutine(ref _equipCoroutine);
        StopCoroutine(ref _limbCoroutine);
        StopCoroutine(ref _attackCoroutine);
        StopCoroutine(ref _passiveCoroutine);
        StopCoroutine(ref _durabilityCoroutine);
        StopCoroutine(ref _capacityCoroutine);
        StopCoroutine(ref _chargeCoroutine);
        StopCoroutine(ref _hasCoroutine);
        StopCoroutine(ref _wearingCoroutine);

        KnownWearableIds.Clear();
        LimbConditionTracker.Clear();
        TriggerLastValues.Clear();
        LastWearCondition.Clear();
        _runner = null;
    }

    private static void StopCoroutine(ref Coroutine? coroutine)
    {
        if (coroutine == null) return;
        _runner!.StopCoroutine(coroutine);
        coroutine = null;
    }

    private static bool HasUseBackpackScript(string itemId)
    {
        var entry = ItemScriptRegistry.GetEntry(itemId);
        return entry != null && entry.GetUseScriptsForBackpack().Count > 0;
    }

    private static bool HasUseHandScript(string itemId)
    {
        var entry = ItemScriptRegistry.GetEntry(itemId);
        return entry != null && entry.GetUseScriptsForHand().Count > 0;
    }

    private static IEnumerator PollPassiveStates()
    {
        yield return new WaitForSeconds(1f);

        while (_passiveCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            var body = BodyUtil.Body;
            if (!body) continue;

            var handSlot = body.handSlot;
            var handItem = body.GetItem(handSlot);
            var currentHandItemId = handItem && !string.IsNullOrEmpty(handItem.id) ? handItem.id : null;

            if (_lastHandItemId == currentHandItemId) continue;
            if (!string.IsNullOrEmpty(_lastHandItemId))
            {
                var oldEntry = ItemScriptRegistry.GetEntry(_lastHandItemId);
                if (oldEntry?.NotInHand is { Count: > 0 })
                    ExecutePassiveScripts(oldEntry, _lastHandItemId, "not_in_hand", oldEntry.NotInHand);
            }

            if (!string.IsNullOrEmpty(currentHandItemId))
            {
                var newEntry = ItemScriptRegistry.GetEntry(currentHandItemId);
                if (newEntry?.InHand is { Count: > 0 })
                    ExecutePassiveScripts(newEntry, currentHandItemId, "in_hand", newEntry.InHand);
            }

            _lastHandItemId = currentHandItemId;
        }
    }

    private static void ExecutePassiveScripts(ItemScriptEntry entry, string itemId, string action,
        List<string> scripts)
    {
        foreach (var relativePath in scripts.Where(p => !string.IsNullOrEmpty(p)))
            ScriptUtil.Execute(entry.ModId, relativePath, itemId, null, action);
    }

    private static IEnumerator PollEquipChange()
    {
        yield return new WaitForSeconds(1f);
        InitWearableSet();

        while (_equipCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);
            PollWearableChange();
        }
    }

    private static void InitWearableSet()
    {
        KnownWearableIds.Clear();
        var body = BodyUtil.Body;
        if (!body) return;

        var wearables = body.GetAllWearables();
        if (wearables == null) return;
        foreach (var item in wearables.OfType<Item>())
        {
            if (item && (!item || string.IsNullOrEmpty(item.id))) continue;
            KnownWearableIds[item.GetInstanceID()] = item.id;
        }
    }

    private static void PollWearableChange()
    {
        var body = BodyUtil.Body;
        if (!body) return;

        var wearables = body.GetAllWearables();
        if (wearables == null) return;

        var currentIds = new Dictionary<int, (string Id, Item Item)>();
        foreach (var item in wearables.OfType<Item>())
        {
            if (!item || string.IsNullOrEmpty(item.id)) continue;
            currentIds[item.GetInstanceID()] = (item.id, item);
        }

        foreach (var kv in currentIds.Where(kv => !KnownWearableIds.ContainsKey(kv.Key)))
        {
            EventUtil.Trigger(new ItemEquipEvent
            {
                ItemId = kv.Value.Id,
                Item = kv.Value.Item
            });
            KnownWearableIds[kv.Key] = kv.Value.Id;
        }

        var toRemove = KnownWearableIds.Where(kv => !currentIds.ContainsKey(kv.Key)).ToList();
        foreach (var kv in toRemove)
        {
            EventUtil.Trigger(new ItemUnequipEvent { ItemId = kv.Value, Item = null });
            KnownWearableIds.Remove(kv.Key);
        }

        foreach (var kv in currentIds)
        {
            var item = kv.Value.Item;
            var currentCondition = item.condition;
            if (LastWearCondition.TryGetValue(kv.Value.Id, out var lastCond)
                && lastCond - currentCondition > 0.01f)
                EventUtil.Trigger(new ItemWearDamageEvent
                {
                    ItemId = kv.Value.Id,
                    Item = item,
                    DamageAmount = lastCond - currentCondition
                });

            LastWearCondition[kv.Value.Id] = currentCondition;
        }
    }

    private static IEnumerator PollLimbUse()
    {
        yield return new WaitForSeconds(1f);
        InitLimbTracker();

        while (_limbCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);
            PollLimbChange();
        }
    }

    private static void InitLimbTracker()
    {
        LimbConditionTracker.Clear();
        var body = BodyUtil.Body;
        if (!body || body.limbs == null) return;

        foreach (var limb in body.limbs)
        {
            if (!limb || limb.dismembered) continue;
            LimbConditionTracker[limb.GetInstanceID()] = GetLimbConditionScore(limb);
        }
    }

    private static void PollLimbChange()
    {
        var body = BodyUtil.Body;
        if (!body || body.limbs == null || body.limbs.Length == 0) return;

        var handItem = body.GetItem(body.handSlot);

        for (var i = 0; i < body.limbs.Length; i++)
        {
            var limb = body.limbs[i];
            if (!limb || limb.dismembered) continue;

            var key = limb.GetInstanceID();
            var currentScore = GetLimbConditionScore(limb);

            if (!LimbConditionTracker.TryGetValue(key, out var prevScore))
            {
                LimbConditionTracker[key] = currentScore;
                continue;
            }

            if (prevScore > currentScore + 0.1f && handItem && !string.IsNullOrEmpty(handItem.id))
                EventUtil.Trigger(new ItemLimbUseEvent
                {
                    ItemId = handItem.id,
                    Item = handItem,
                    LimbIndex = i,
                    LimbName = limb.fullName ?? string.Empty
                });

            LimbConditionTracker[key] = currentScore;
        }
    }

    private static float GetLimbConditionScore(Limb limb)
    {
        if (!limb) return 0f;
        return limb.bleedAmount
               + limb.infectionAmount
               + limb.boneHealTimer
               + limb.dislocationTimer;
    }

    private static IEnumerator PollItemAttack()
    {
        yield return new WaitForSeconds(1f);

        while (_attackCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            var body = BodyUtil.Body;
            if (!body) continue;

            var handItem = body.GetItem(body.handSlot);
            if (!handItem || string.IsNullOrEmpty(handItem.id))
            {
                _lastHandCondition = 1f;
                continue;
            }

            var currentCondition = handItem.condition;
            if (Time.frameCount != _lastAttackFrame
                && _lastHandCondition - currentCondition > 0.01f)
            {
                _lastAttackFrame = Time.frameCount;
                EventUtil.Trigger(new ItemAttackEvent { ItemId = handItem.id, Item = handItem });
            }

            _lastHandCondition = currentCondition;
        }
    }

    private static IEnumerator PollDurability()
    {
        yield return new WaitForSeconds(2f);

        while (_durabilityCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            var body = BodyUtil.Body;
            if (!body) continue;

            foreach (var entry in GetTrackedEntriesWithTrigger(e => e.Durability.Count > 0))
            {
                var item = FindItemOnBody(body, entry.Key);
                if (item is null) continue;

                var currentValue = item.condition / 100f;

                CheckAndFireTriggers(entry.Key, item, entry.Value.Durability, currentValue,
                    (itemId, it, op, threshold, cv) => EventUtil.Trigger(new ItemDurabilityEvent
                    {
                        ItemId = itemId, Item = it,
                        Operator = op, ThresholdValue = threshold, CurrentValue = cv
                    }));
            }
        }
    }

    private static IEnumerator PollCapacity()
    {
        yield return new WaitForSeconds(2f);

        while (_capacityCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            var body = BodyUtil.Body;
            if (!body) continue;

            foreach (var entry in GetTrackedEntriesWithTrigger(e => e.CapacityTrigger.Count > 0))
            {
                var item = FindItemOnBody(body, entry.Key);
                if (!item) continue;

                var currentWeight = Traverse.Create(item).Property("Stats")
                    .Property("TotalWeight").GetValue<float>();
                var container = Traverse.Create(item).Property("Stats")
                    .Property("Container").GetValue();
                var maxWeight = container != null
                    ? Traverse.Create(container).Property("Capacity").GetValue<float>()
                    : 1f;
                var currentValue = maxWeight > 0f
                    ? Mathf.Clamp01(currentWeight / maxWeight)
                    : 0f;

                CheckAndFireTriggers(entry.Key, item, entry.Value.CapacityTrigger, currentValue,
                    (itemId, it, op, threshold, cv) => EventUtil.Trigger(new ItemCapacityEvent
                    {
                        ItemId = itemId, Item = it,
                        Operator = op, ThresholdValue = threshold, CurrentValue = cv
                    }), "c");
            }
        }
    }

    private static IEnumerator PollCharge()
    {
        yield return new WaitForSeconds(2f);

        while (_chargeCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            var body = BodyUtil.Body;
            if (!body) continue;

            foreach (var entry in GetTrackedEntriesWithTrigger(e => e.ChargeTrigger.Count > 0))
            {
                var item = FindItemOnBody(body, entry.Key);
                if (!item) continue;

                var battery = Traverse.Create(item).Property("Stats")
                    .Property("Battery").GetValue();
                if (battery == null) continue;

                var currentCharge = Traverse.Create(battery).Property("CurrentCharge").GetValue<float>();
                var maxCharge = Traverse.Create(battery).Property("MaxCharge").GetValue<float>();
                var currentValue = maxCharge > 0f
                    ? Mathf.Clamp01(currentCharge / maxCharge)
                    : 0f;

                CheckAndFireTriggers(entry.Key, item, entry.Value.ChargeTrigger, currentValue,
                    (itemId, it, op, threshold, cv) => EventUtil.Trigger(new ItemChargeEvent
                    {
                        ItemId = itemId, Item = it,
                        Operator = op, ThresholdValue = threshold, CurrentValue = cv
                    }), "chr");
            }
        }
    }

    private static IEnumerator PollHasItems()
    {
        yield return new WaitForSeconds(1f);

        while (_hasCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            foreach (var (itemId, entry) in ItemScriptRegistry.AllEntries)
            {
                if (entry.Has.Count == 0) continue;
                if (!InventoryUtil.HasItem(itemId)) continue;

                EventUtil.Trigger(new ItemHasEvent { ItemId = itemId });
            }
        }
    }

    private static IEnumerator PollWearPassive()
    {
        yield return new WaitForSeconds(1f);

        while (_wearingCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            var body = BodyUtil.Body;
            if (!body) continue;

            foreach (var (_, itemId) in KnownWearableIds)
            {
                var entry = ItemScriptRegistry.GetEntry(itemId);
                if (entry is null || entry.WearWearing.Count == 0) continue;

                var item = FindItemOnBody(body, itemId);
                if (!item) continue;

                EventUtil.Trigger(new ItemWearingEvent { ItemId = itemId, Item = item });
            }
        }
    }

    private static IEnumerable<KeyValuePair<string, ItemScriptEntry>> GetTrackedEntriesWithTrigger(
        Func<ItemScriptEntry, bool> hasTrigger)
    {
        foreach (var (_, itemId) in KnownWearableIds)
        {
            var entry = ItemScriptRegistry.GetEntry(itemId);
            if (entry != null && hasTrigger(entry))
                yield return new KeyValuePair<string, ItemScriptEntry>(itemId, entry);
        }
    }

    private static Item? FindItemOnBody(Body body, string itemId)
    {
        var wearables = body.GetAllWearables();
        return wearables?.FirstOrDefault(item => item.id == itemId);
    }

    private static void CheckAndFireTriggers(
        string itemId,
        Item? item,
        List<ConditionTriggerDef> triggers,
        float currentValue,
        Action<string, Item?, string, float, float> fireEvent,
        string triggerType = "d")
    {
        for (var i = 0; i < triggers.Count; i++)
        {
            var trigger = triggers[i];
            if (trigger.Script.Count == 0) continue;

            var key = $"{itemId}_{triggerType}{i}";
            var hasPrevious = TriggerLastValues.TryGetValue(key, out var previousValue);

            var triggeredNow = EvaluateTrigger(trigger.Operator, trigger.Value, currentValue);
            var triggeredBefore = hasPrevious
                                  && EvaluateTrigger(trigger.Operator, trigger.Value, previousValue);

            TriggerLastValues[key] = currentValue;

            if (triggeredNow && !triggeredBefore)
                fireEvent(itemId, item, trigger.Operator, trigger.Value, currentValue);
        }
    }

    private static bool EvaluateTrigger(string op, float threshold, float current)
    {
        return op switch
        {
            "<" => current < threshold,
            "<=" => current <= threshold,
            "==" => Mathf.Approximately(current, threshold),
            ">=" => current >= threshold,
            ">" => current > threshold,
            _ => false
        };
    }

    private static bool IsPlayerItem(Item item)
    {
        return item != null
               && item.transform != null
               && BodyUtil.Body != null
               && item.transform.IsChildOf(BodyUtil.Body.transform);
    }

    [HarmonyPatch(typeof(Body))]
    public static class BodyPatch
    {
        [HarmonyPatch("UseItem", typeof(Item))]
        private static bool UseItemPrefix(Item item)
        {
            if (item == null || string.IsNullOrEmpty(item.id)) return true;
            if (!IsPlayerItem(item)) return true;
            if (!HasUseBackpackScript(item.id)) return true;

            EventUtil.Trigger(new ItemUseEvent { ItemId = item.id, Item = item });
            return false;
        }

        [HarmonyPatch("UseItemInHand")]
        private static bool UseItemInHandPrefix(Body __instance)
        {
            if (__instance == null) return true;

            var item = __instance.GetItem(__instance.handSlot);
            if (item == null || string.IsNullOrEmpty(item.id)) return true;
            if (!IsPlayerItem(item)) return true;
            if (!HasUseHandScript(item.id)) return true;

            EventUtil.Trigger(new ItemHandUseEvent { ItemId = item.id, Item = item });
            return false;
        }

        [HarmonyPatch("Attack")]
        private static void AttackPostfix(object __instance)
        {
            var item = __instance switch
            {
                Item itm => itm,
                Body body => body.GetItem(body.handSlot),
                _ => null
            };

            if (item == null || string.IsNullOrEmpty(item.id)) return;
            if (!IsPlayerItem(item)) return;

            EventUtil.Trigger(new ItemAttackEvent { ItemId = item.id, Item = item });
        }
    }
}