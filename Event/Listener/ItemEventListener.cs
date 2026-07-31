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

// 物品动作事件监听器：通过 Harmony 补丁拦截游戏物品相关方法，
// 触发 ItemUseEvent / ItemEquipEvent / ItemUnequipEvent / ItemLimbUseEvent / ItemAttackEvent。
// 新增被动状态、耐久/容量/电量条件触发器的轮询检测。
public static class ItemEventListener
{
    // 轮询间隔（秒）
    private const float PollInterval = 0.5f;

    // instanceId → itemId 映射，用于卸下时传递物品 ID
    private static readonly Dictionary<int, string> KnownWearableIds = new();
    private static readonly Dictionary<int, float> LimbConditionTracker = new();

    // 条件触发器上次值缓存：(itemId, triggerIndex) → lastValue
    private static readonly Dictionary<string, float> TriggerLastValues = new();

    private static Coroutine? _useCoroutine;
    private static Coroutine? _equipCoroutine;
    private static Coroutine? _limbCoroutine;
    private static Coroutine? _attackCoroutine;
    private static Coroutine? _passiveCoroutine;
    private static Coroutine? _durabilityCoroutine;
    private static Coroutine? _capacityCoroutine;
    private static Coroutine? _chargeCoroutine;
    private static MonoBehaviour? _runner;

    private static int _lastHandSlot = -1;
    private static bool _lastHandOccupied;
    private static string? _lastHandItemId;

    // 攻击检测：追踪手部物品 condition
    private static float _lastHandCondition = 1f;
    private static int _lastAttackFrame;

    // 穿戴攻击：追踪被穿戴物品的 condition
    private static readonly Dictionary<string, float> LastWearCondition = new();

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;

        TryPatchItemUse();
        TryPatchItemUseInHand();
        TryPatchItemAttack();

        _useCoroutine ??= runner.StartCoroutine(PollItemUse());
        _equipCoroutine ??= runner.StartCoroutine(PollEquipChange());
        _limbCoroutine ??= runner.StartCoroutine(PollLimbUse());
        _attackCoroutine ??= runner.StartCoroutine(PollItemAttack());
        _passiveCoroutine ??= runner.StartCoroutine(PollPassiveStates());
        _durabilityCoroutine ??= runner.StartCoroutine(PollDurability());
        _capacityCoroutine ??= runner.StartCoroutine(PollCapacity());
        _chargeCoroutine ??= runner.StartCoroutine(PollCharge());
    }

    internal static void Stop()
    {
        if (_runner == null) return;

        if (_useCoroutine != null) { _runner.StopCoroutine(_useCoroutine); _useCoroutine = null; }
        if (_equipCoroutine != null) { _runner.StopCoroutine(_equipCoroutine); _equipCoroutine = null; }
        if (_limbCoroutine != null) { _runner.StopCoroutine(_limbCoroutine); _limbCoroutine = null; }
        if (_attackCoroutine != null) { _runner.StopCoroutine(_attackCoroutine); _attackCoroutine = null; }
        if (_passiveCoroutine != null) { _runner.StopCoroutine(_passiveCoroutine); _passiveCoroutine = null; }
        if (_durabilityCoroutine != null) { _runner.StopCoroutine(_durabilityCoroutine); _durabilityCoroutine = null; }
        if (_capacityCoroutine != null) { _runner.StopCoroutine(_capacityCoroutine); _capacityCoroutine = null; }
        if (_chargeCoroutine != null) { _runner.StopCoroutine(_chargeCoroutine); _chargeCoroutine = null; }

        KnownWearableIds.Clear();
        LimbConditionTracker.Clear();
        TriggerLastValues.Clear();
        LastWearCondition.Clear();
        _runner = null;
    }

    // ============================================================
    // Harmony 补丁
    // ============================================================

    private static void TryPatchItemUse()
    {
        PatchMethod(typeof(Body), "UseItem", nameof(OnItemUseFromInventory),
            "Bark.ItemUseEventListener");
    }

    private static void TryPatchItemUseInHand()
    {
        PatchMethod(typeof(Body), "UseItemInHand", nameof(OnItemUseInHand),
            "Bark.ItemHandUseEventListener");
    }

    private static void PatchMethod(Type type, string methodName, string callbackName, string harmonyId)
    {
        var method = AccessTools.Method(type, methodName);
        if (method == null) return;

        try
        {
            var harmony = new Harmony(harmonyId);
            harmony.Patch(method, new HarmonyMethod(typeof(ItemEventListener), callbackName));
            LogUtil.Info("item_event.patch_use_ok", $"{type.Name}.{methodName}");
        }
        catch
        {
            // ignored
        }
    }

    // 背包中使用物品（Body.UseItem(Item item)）
    private static bool OnItemUseFromInventory(Item item)
    {
        if (item == null || string.IsNullOrEmpty(item.id)) return true;
        if (!IsPlayerItem(item)) return true;

        if (!HasUseBackpackScript(item.id)) return true;
        EventUtil.Trigger(new ItemUseEvent { ItemId = item.id, Item = item });
        return false;
    }

    // 手持物品使用（Body.UseItemInHand()）
    private static bool OnItemUseInHand(Body __instance)
    {
        if (__instance == null) return true;
        var item = __instance.GetItem(__instance.handSlot);
        if (item == null || string.IsNullOrEmpty(item.id)) return true;
        if (!IsPlayerItem(item)) return true;

        if (!HasUseHandScript(item.id)) return true;
        EventUtil.Trigger(new ItemHandUseEvent { ItemId = item.id, Item = item });
        return false;
    }

    // 检查物品是否有 use 背包脚本
    private static bool HasUseBackpackScript(string itemId)
    {
        var entry = ItemScriptRegistry.GetEntry(itemId);
        return entry != null && entry.GetUseScriptsForBackpack().Count > 0;
    }

    // 检查物品是否有 use 手持脚本
    private static bool HasUseHandScript(string itemId)
    {
        var entry = ItemScriptRegistry.GetEntry(itemId);
        return entry != null && entry.GetUseScriptsForHand().Count > 0;
    }

    // ============================================================
    // 轮询：被动状态检测（in_hand / not_in_hand / in_backpack）
    // ============================================================

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
            var currentHandItemId = handItem != null && !string.IsNullOrEmpty(handItem.id) ? handItem.id : null;

            // in_hand / not_in_hand 状态变化
            if (_lastHandItemId != currentHandItemId)
            {
                // 旧物品：not_in_hand
                if (!string.IsNullOrEmpty(_lastHandItemId))
                {
                    var oldEntry = ItemScriptRegistry.GetEntry(_lastHandItemId);
                    if (oldEntry?.NotInHand is { Count: > 0 })
                        ExecutePassiveScripts(oldEntry, _lastHandItemId, "not_in_hand", oldEntry.NotInHand);
                }

                // 新物品：in_hand
                if (!string.IsNullOrEmpty(currentHandItemId))
                {
                    var newEntry = ItemScriptRegistry.GetEntry(currentHandItemId);
                    if (newEntry?.InHand is { Count: > 0 })
                        ExecutePassiveScripts(newEntry, currentHandItemId, "in_hand", newEntry.InHand);
                }

                _lastHandItemId = currentHandItemId;
            }

            _lastHandSlot = handSlot;
            _lastHandOccupied = handItem != null;
        }
    }

    private static void ExecutePassiveScripts(ItemScriptEntry entry, string itemId, string action,
        List<string> scripts)
    {
        foreach (var relativePath in scripts.Where(p => !string.IsNullOrEmpty(p)))
            ScriptUtil.Execute(entry.ModId, relativePath, itemId, null, action);
    }

    // ============================================================
    // 轮询：手部物品使用检测
    // ============================================================

    private static IEnumerator PollItemUse()
    {
        while (_useCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            var body = BodyUtil.Body;
            if (!body) continue;

            var handSlot = body.handSlot;
            var hasItem = body.HoldingItem(handSlot);

            _lastHandSlot = handSlot;
            _lastHandOccupied = hasItem;
        }
    }

    // ============================================================
    // 轮询：装备变化检测
    // ============================================================

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
            if (item == null || string.IsNullOrEmpty(item.id)) continue;
            KnownWearableIds[item.GetInstanceID()] = item.id;
        }
    }

    private static void PollWearableChange()
    {
        var body = BodyUtil.Body;
        if (!body) return;

        var wearables = body.GetAllWearables();
        if (wearables == null) return;

        var currentIds = new Dictionary<int, string>();
        foreach (var item in wearables.OfType<Item>())
        {
            if (item == null || string.IsNullOrEmpty(item.id)) continue;
            currentIds[item.GetInstanceID()] = item.id;
        }

        // 检测新装备
        foreach (var kv in currentIds)
        {
            if (!KnownWearableIds.ContainsKey(kv.Key))
            {
                EventUtil.Trigger(new ItemEquipEvent { ItemId = kv.Value, Item = wearables.OfType<Item>()
                    .FirstOrDefault(i => i.GetInstanceID() == kv.Key) });
            }
        }

        // 检测卸下装备
        var toRemove = KnownWearableIds.Where(kv => !currentIds.ContainsKey(kv.Key)).ToList();
        foreach (var kv in toRemove)
        {
            EventUtil.Trigger(new ItemUnequipEvent { ItemId = kv.Value, Item = null });
            KnownWearableIds.Remove(kv.Key);
        }

        // 合并新增
        foreach (var kv in currentIds)
        {
            if (!KnownWearableIds.ContainsKey(kv.Key))
                KnownWearableIds[kv.Key] = kv.Value;
        }

        // 穿戴攻击检测：检查穿戴物品 condition 下降
        foreach (var kv in currentIds)
        {
            var item = wearables.OfType<Item>().FirstOrDefault(i => i.GetInstanceID() == kv.Key);
            if (item == null) continue;
            var currentCondition = item.condition;
            if (LastWearCondition.TryGetValue(kv.Value, out var lastCond)
                && lastCond - currentCondition > 0.01f)
            {
                EventUtil.Trigger(new ItemWearDamageEvent
                {
                    ItemId = kv.Value,
                    Item = item,
                    DamageAmount = lastCond - currentCondition
                });
            }
            LastWearCondition[kv.Value] = currentCondition;
        }
    }

    // ============================================================
    // 轮询：肢体使用物品检测
    // ============================================================

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

    // ============================================================
    // 攻击检测
    // ============================================================

    private static void TryPatchItemAttack()
    {
        PatchMethod(typeof(Body), "Attack", nameof(OnItemAttackHarmony),
            "Bark.ItemAttackEventListener");
    }

    private static void OnItemAttackHarmony(object __instance)
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

    // ============================================================
    // 轮询：耐久条件触发器
    // ============================================================

    private static IEnumerator PollDurability()
    {
        yield return new WaitForSeconds(2f);

        while (_durabilityCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            var body = BodyUtil.Body;
            if (!body) continue;

            // 遍历所有已注册 durabiltiy 触发器的物品
            foreach (var entry in GetTrackedEntriesWithTrigger(e => e.Durability.Count > 0))
            {
                var item = FindItemOnBody(body, entry.Key);
                if (item == null) continue;

                var currentValue = item.condition / 100f; // 转换为 0~1
                CheckAndFireTriggers(entry.Key, item, entry.Value.Durability, currentValue,
                    (itemId, it, op, threshold, cv) => EventUtil.Trigger(new ItemDurabilityEvent
                    {
                        ItemId = itemId, Item = it,
                        Operator = op, ThresholdValue = threshold, CurrentValue = cv
                    }), "d");
            }
        }
    }

    // ============================================================
    // 轮询：容器容量条件触发器
    // ============================================================

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
                if (item == null) continue;

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

    // ============================================================
    // 轮询：电池电量条件触发器
    // ============================================================

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
                if (item == null) continue;

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

    // ============================================================
    // 条件触发器通用检测逻辑
    // ============================================================

    // 获取已注册且有指定类型触发器的物品条目
    private static IEnumerable<KeyValuePair<string, ItemScriptEntry>> GetTrackedEntriesWithTrigger(
        Func<ItemScriptEntry, bool> hasTrigger)
    {
        // 简化：遍历 KnownWearableIds 中的物品
        foreach (var (_, itemId) in KnownWearableIds)
        {
            var entry = ItemScriptRegistry.GetEntry(itemId);
            if (entry != null && hasTrigger(entry))
                yield return new KeyValuePair<string, ItemScriptEntry>(itemId, entry);
        }
    }

    // 在 body 上查找物品（先查装备再查 inventory）
    private static Item? FindItemOnBody(Body body, string itemId)
    {
        var wearables = body.GetAllWearables();
        if (wearables != null)
        {
            foreach (var item in wearables)
            {
                if (item != null && item.id == itemId)
                    return item;
            }
        }
        return null;
    }

    // 检查条件触发器并触发（边沿检测）
    private static void CheckAndFireTriggers(string itemId, Item? item, List<ConditionTriggerDef> triggers,
        float currentValue, Action<string, Item?, string, float, float> fireEvent,
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

            // 只在上一次不满足、当前满足时触发（边沿触发）
            if (triggeredNow && !triggeredBefore)
                fireEvent(itemId, item, trigger.Operator, trigger.Value, currentValue);
        }
    }

    // 比较 currentValue 与 threshold 是否满足 operator
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

    // ============================================================
    // 辅助
    // ============================================================

    private static bool IsPlayerItem(Item item)
    {
        return item != null && item.transform != null
                            && BodyUtil.Body != null
                            && item.transform.IsChildOf(BodyUtil.Body.transform);
    }
}
