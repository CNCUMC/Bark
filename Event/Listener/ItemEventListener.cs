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
public static class ItemEventListener
{
    // 轮询间隔（秒）
    private const float PollInterval = 0.5f;

    private static readonly HashSet<int> KnownWearableIds = new();
    private static readonly Dictionary<int, float> LimbConditionTracker = new();

    private static Coroutine? _useCoroutine;
    private static Coroutine? _equipCoroutine;
    private static Coroutine? _limbCoroutine;
    private static Coroutine? _attackCoroutine;
    private static MonoBehaviour? _runner;

    private static int _lastHandSlot = -1;
    private static bool _lastHandOccupied;

    // 攻击检测：追踪手部物品 condition
    private static float _lastHandCondition = 1f;
    private static int _lastAttackFrame;

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;

        // 尝试 Harmony 补丁 Item.Use 方法
        TryPatchItemUse();

        // 尝试 Harmony 补丁手持使用
        TryPatchItemUseInHand();

        // 尝试 Harmony 补丁攻击方法
        TryPatchItemAttack();

        // 尝试 Harmony 补丁 WearWearable：防止缺 WornSprite 导致 NRE
        TryPatchWearWearable();

        // 启动轮询检测
        _useCoroutine ??= runner.StartCoroutine(PollItemUse());
        _equipCoroutine ??= runner.StartCoroutine(PollEquipChange());
        _limbCoroutine ??= runner.StartCoroutine(PollLimbUse());
        _attackCoroutine ??= runner.StartCoroutine(PollItemAttack());
    }

    internal static void Stop()
    {
        if (_runner == null) return;

        if (_useCoroutine != null)
        {
            _runner.StopCoroutine(_useCoroutine);
            _useCoroutine = null;
        }

        if (_equipCoroutine != null)
        {
            _runner.StopCoroutine(_equipCoroutine);
            _equipCoroutine = null;
        }

        if (_limbCoroutine != null)
        {
            _runner.StopCoroutine(_limbCoroutine);
            _limbCoroutine = null;
        }

        if (_attackCoroutine != null)
        {
            _runner.StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }

        KnownWearableIds.Clear();
        LimbConditionTracker.Clear();
        _runner = null;
    }

    // ============================================================
    // Harmony 补丁：分别拦截 Body.UseItem（背包使用）和 Body.UseItemInHand（手持使用）
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

    // 背包中使用物品（Body.UseItem(Item item) → item 参数按位置捕获）
    // 返回 false 跳过原始调用，避免自定义物品无消耗品数据导致 NRE
    private static bool OnItemUseFromInventory(Item item)
    {
        if (item == null || string.IsNullOrEmpty(item.id)) return true;
        if (!IsPlayerItem(item)) return true;

        // 有 use 脚本的物品跳过原始 UseItem，仅触发脚本
        if (HasScript(item.id, e => e.Use.Count > 0))
        {
            EventUtil.Trigger(new ItemUseEvent
            {
                ItemId = item.id,
                Item = item
            });
            return false;
        }

        return true;
    }

    // 手持物品使用（Body.UseItemInHand() → 无参数，从 __instance 取手部物品）
    private static bool OnItemUseInHand(Body __instance)
    {
        if (__instance == null) return true;
        var item = __instance.GetItem(__instance.handSlot);
        if (item == null || string.IsNullOrEmpty(item.id)) return true;
        if (!IsPlayerItem(item)) return true;

        if (HasScript(item.id, e => e.UseInHand.Count > 0))
        {
            EventUtil.Trigger(new ItemHandUseEvent
            {
                ItemId = item.id,
                Item = item
            });
            return false;
        }

        return true;
    }

    // 检查物品是否有指定脚本注册
    private static bool HasScript(string itemId, Func<ItemScriptEntry, bool> predicate)
    {
        var entry = ItemScriptRegistry.GetEntry(itemId);
        return entry != null && predicate(entry);
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
        var body = BodyUtil.Body;
        if (!body) return;

        var wearables = body.GetAllWearables();
        if (wearables == null) return;
        foreach (var item in wearables.OfType<Item>())
            KnownWearableIds.Add(item.GetInstanceID());
    }

    private static void PollWearableChange()
    {
        var body = BodyUtil.Body;
        if (!body) return;

        var wearables = body.GetAllWearables();
        if (wearables == null) return;

        // 构建当前装备 ID 集合
        var currentIds = new HashSet<int>();
        foreach (var item in wearables.OfType<Item>())
            currentIds.Add(item.GetInstanceID());

        // 检测新装备
        foreach (var item in from item in wearables
                 where item && !string.IsNullOrEmpty(item.id)
                 let instanceId = item.GetInstanceID()
                 where currentIds.Contains(instanceId) && !KnownWearableIds.Contains(instanceId)
                 select item)
            EventUtil.Trigger(new ItemEquipEvent
            {
                ItemId = item.id,
                Item = item
            });

        // 检测卸下装备：用缓存的副本遍历，避免修改冲突
        var toRemove = KnownWearableIds.Where(id => !currentIds.Contains(id)).ToList();

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
        var body = BodyUtil.Body;
        if (!body || body.limbs == null) return;

        foreach (var limb in body.limbs)
        {
            if (!limb || limb.dismembered) continue;
            var key = limb.GetInstanceID();
            LimbConditionTracker[key] = GetLimbConditionScore(limb);
        }
    }

    private static void PollLimbChange()
    {
        var body = BodyUtil.Body;
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

    // 肢体状况综合评分（越大越差）：出血 + 感染 + 骨骼计时器 + 脱臼计时器
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

    // Harmony 前缀：检查 wearable 在装备前是否有有效 WornSprite，
    // 缺贴图时跳过原始 WearWearable 避免 NRE
    private static void TryPatchWearWearable()
    {
        PatchMethod(typeof(Body), "WearWearable", nameof(OnWearWearablePrefix),
            "Bark.WearWearableGuard");
    }

    private static bool OnWearWearablePrefix(Body __instance, Item item)
    {
        if (item == null || string.IsNullOrEmpty(item.id)) return false;

        // 检查是否为已记录的缺贴图 wearable，是则跳过原始调用避免 NRE
        if (ItemLoader.WearableWithoutWornSprite.Contains(item.id))
        {
            LogUtil.Warning("item_event.wear_blocked_no_sprite",
                item.id,
                item.fullName ?? item.id);
            return false;
        }

        return true;
    }

    // Harmony 补丁回调：统一处理 Item/Body 攻击方法
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

        EventUtil.Trigger(new ItemAttackEvent
        {
            ItemId = item.id,
            Item = item
        });
    }

    // 轮询检测手部物品 condition 下降作为攻击兜底
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
            // 同一帧不重复触发；condition 下降 >0.01 视为一次攻击消耗
            if (Time.frameCount != _lastAttackFrame
                && _lastHandCondition - currentCondition > 0.01f)
            {
                _lastAttackFrame = Time.frameCount;
                EventUtil.Trigger(new ItemAttackEvent
                {
                    ItemId = handItem.id,
                    Item = handItem
                });
            }

            _lastHandCondition = currentCondition;
        }
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