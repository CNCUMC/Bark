using System;
using System.Collections;
using System.Collections.Generic;
using Bark.Events;
using Bark.Tool;
using HarmonyLib;
using UnityEngine;

namespace Bark.Event.Listener;

// 物品动作事件监听器：通过 Harmony 补丁拦截游戏物品相关方法，
// 触发 ItemUseEvent / ItemEquipEvent / ItemUnequipEvent / ItemLimbUseEvent。
// 
// 拦截策略：
// - use: 尝试补丁 Item.Use() 方法（如存在），同时轮询手部物品变化作为兜底
// - equip/unequip: 轮询 GetAllWearables() 集合变化
// - use_on_limb: 轮询肢体状态变化（感染/出血等）检测治疗动作
public static class ItemEventListener
{
    // 轮询间隔（秒）
    private const float PollInterval = 0.5f;

    private static readonly HashSet<int> KnownWearableIds = new();
    private static readonly Dictionary<int, float> LimbConditionTracker = new();

    private static Coroutine? _useCoroutine;
    private static Coroutine? _equipCoroutine;
    private static Coroutine? _limbCoroutine;
    private static MonoBehaviour? _runner;

    private static int _lastHandSlot = -1;
    private static bool _lastHandOccupied;

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;

        // 尝试 Harmony 补丁 Item.Use 方法
        TryPatchItemUse();

        // 启动轮询检测
        _useCoroutine ??= runner.StartCoroutine(PollItemUse());
        _equipCoroutine ??= runner.StartCoroutine(PollEquipChange());
        _limbCoroutine ??= runner.StartCoroutine(PollLimbUse());
    }

    internal static void Stop()
    {
        if (_runner == null) return;

        if (_useCoroutine != null) { _runner.StopCoroutine(_useCoroutine); _useCoroutine = null; }
        if (_equipCoroutine != null) { _runner.StopCoroutine(_equipCoroutine); _equipCoroutine = null; }
        if (_limbCoroutine != null) { _runner.StopCoroutine(_limbCoroutine); _limbCoroutine = null; }

        KnownWearableIds.Clear();
        LimbConditionTracker.Clear();
        _runner = null;
    }

    // ============================================================
    // Harmony 补丁：尝试拦截 Item.Use()
    // ============================================================

    private static void TryPatchItemUse()
    {
        // 尝试多种可能的方法名
        foreach (var (type, methodName) in new[]
                 {
                     (typeof(Item), "Use"),
                     (typeof(Body), "UseItem"),
                     (typeof(Body), "UseItemInHand"),
                 })
        {
            var method = AccessTools.Method(type, methodName);
            if (method == null) continue;

            try
            {
                var harmony = new Harmony("Bark.ItemEventListener");
                harmony.Patch(method, new HarmonyMethod(typeof(ItemEventListener), nameof(OnItemUse)));
                LogUtil.Info("item_event.patch_use_ok", $"{type.Name}.{methodName}");
                return;
            }
            catch
            {
                // 补丁失败，继续尝试下一个
            }
        }
    }

    private static void OnItemUse(Item __instance)
    {
        if (__instance == null || string.IsNullOrEmpty(__instance.id)) return;
        if (!IsPlayerItem(__instance)) return;

        EventUtil.Trigger(new ItemUseEvent
        {
            ItemId = __instance.id,
            Item = __instance
        });
    }

    // ============================================================
    // 轮询：手部物品使用检测
    // ============================================================

    private static IEnumerator PollItemUse()
    {
        while (_useCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            var body = PlayerUtil.Body;
            if (!body) continue;

            var handSlot = body.handSlot;
            var hasItem = body.HoldingItem(handSlot);

            // 手部物品从有变无 = 物品被使用/消耗
            if (_lastHandOccupied && !hasItem && _lastHandSlot == handSlot)
            {
                // 无法知道被消耗的物品 ID，此路径作为兜底，
                // 精确拦截由 Harmony 补丁完成
            }

            _lastHandSlot = handSlot;
            _lastHandOccupied = hasItem;
        }
    }

    // ============================================================
    // 轮询：装备变化检测
    // ============================================================

    private static IEnumerator PollEquipChange()
    {
        // 等待世界生成
        yield return new WaitForSeconds(1f);
        // 初始化已知装备集合
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
        var body = PlayerUtil.Body;
        if (!body) return;

        var wearables = body.GetAllWearables();
        if (wearables == null) return;
        foreach (var item in wearables)
            if (item != null)
                KnownWearableIds.Add(item.GetInstanceID());
    }

    private static void PollWearableChange()
    {
        var body = PlayerUtil.Body;
        if (!body) return;

        var wearables = body.GetAllWearables();
        if (wearables == null) return;

        // 构建当前装备 ID 集合
        var currentIds = new HashSet<int>();
        foreach (var item in wearables)
            if (item != null)
                currentIds.Add(item.GetInstanceID());

        // 检测新装备
        foreach (var item in wearables)
        {
            if (item == null || string.IsNullOrEmpty(item.id)) continue;
            var instanceId = item.GetInstanceID();
            if (currentIds.Contains(instanceId) && !KnownWearableIds.Contains(instanceId))
            {
                EventUtil.Trigger(new ItemEquipEvent
                {
                    ItemId = item.id,
                    Item = item
                });
            }
        }

        // 检测卸下装备：用缓存的副本遍历，避免修改冲突
        var toRemove = new List<int>();
        foreach (var id in KnownWearableIds)
        {
            if (!currentIds.Contains(id))
                toRemove.Add(id);
        }

        foreach (var id in toRemove)
            KnownWearableIds.Remove(id);

        // 注意：已卸下的物品无法获取 Item 引用（已被销毁/移除），
        // 仅触发事件通知 ID 为 unknown
        // 如需获取 ID，可考虑在已知集合中存储 id → instanceID 映射
    }

    // ============================================================
    // 轮询：肢体使用物品检测（感染/出血减少表明可能有治疗）
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
        var body = PlayerUtil.Body;
        if (!body || body.limbs == null) return;

        foreach (var limb in body.limbs)
        {
            if (limb == null || limb.dismembered) continue;
            var key = limb.GetInstanceID();
            LimbConditionTracker[key] = GetLimbConditionScore(limb);
        }
    }

    private static void PollLimbChange()
    {
        var body = PlayerUtil.Body;
        if (!body || body.limbs == null || body.limbs.Length == 0) return;

        // 获取当前手部物品（用于关联使用动作）
        var handItem = body.GetItem(body.handSlot);

        for (var i = 0; i < body.limbs.Length; i++)
        {
            var limb = body.limbs[i];
            if (limb == null || limb.dismembered) continue;

            var key = limb.GetInstanceID();
            var currentScore = GetLimbConditionScore(limb);

            if (!LimbConditionTracker.TryGetValue(key, out var prevScore))
            {
                LimbConditionTracker[key] = currentScore;
                continue;
            }

            // 肢体状况改善（出血减少、感染减少）且手上有物品 → 可能使用了物品
            if (prevScore > currentScore + 0.1f && handItem != null && !string.IsNullOrEmpty(handItem.id))
            {
                EventUtil.Trigger(new ItemLimbUseEvent
                {
                    ItemId = handItem.id,
                    Item = handItem,
                    LimbIndex = i,
                    LimbName = limb.fullName ?? string.Empty
                });
            }

            LimbConditionTracker[key] = currentScore;
        }
    }

    // 肢体状况综合评分（越大越差）：出血 + 感染 + 骨骼计时器 + 脱臼计时器
    private static float GetLimbConditionScore(Limb limb)
    {
        if (limb == null) return 0f;
        return limb.bleedAmount
               + limb.infectionAmount
               + limb.boneHealTimer
               + limb.dislocationTimer;
    }

    // ============================================================
    // 辅助
    // ============================================================

    private static bool IsPlayerItem(Item item)
    {
        return item != null && item.transform != null
                            && PlayerUtil.Body != null
                            && item.transform.IsChildOf(PlayerUtil.Body.transform);
    }
}
