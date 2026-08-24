using System.Collections.Generic;
using Bark.Events;
using Bark.Tool;
using HarmonyLib;
using UnityEngine;

namespace Bark.Event.Listener;

// 世界物品/实体事件监听器：通过 Harmony 补丁拦截游戏中散落的物品与实体组件方法，
// 触发电池、自动泵、充电器、捕兽夹、生物终端、血迹、方块伤害、蓝图、已购物品、
// 弹跳蘑菇、建筑破坏等事件。
// 对 Update 类方法采用状态翻转检测，避免每帧重复触发。
public static class WorldEntityEventListener
{
    // 自动泵激活实例缓存（instanceId → 运作中）
    private static readonly HashSet<int> ActiveAutoPumps = [];

    // 捕兽夹已捕获实例（instanceId → 已夹住）
    private static readonly HashSet<int> TriggeredTraps = [];

    // 已购物品到期标记
    private static readonly HashSet<int> ExpiredBoughtItems = [];

    // 建筑破坏标记（instanceId → 已触发破坏）
    private static readonly HashSet<int> DestroyedBuildings = [];

    // 血迹生成计数缓存（instanceId → 上次 spawned）
    private static readonly Dictionary<int, byte> BloodSpawnCounts = new();

    // BleedParticle.spawned 私有字段反射缓存（避免每次 Traverse 反射开销）
    private static System.Reflection.FieldInfo? s_spawnedField;

    internal static void Listen()
    {
    }

    internal static void Stop()
    {
        ActiveAutoPumps.Clear();
        TriggeredTraps.Clear();
        ExpiredBoughtItems.Clear();
        DestroyedBuildings.Clear();
        BloodSpawnCounts.Clear();
    }

    private static byte? GetSpawnedCount(BleedParticle instance)
    {
        try
        {
            s_spawnedField ??= AccessTools.Field(typeof(BleedParticle), "spawned");
            var value = s_spawnedField?.GetValue(instance);
            return value is byte b
                ? b
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static float GetBlockHealth(Vector2Int pos)
    {
        var world = WorldGeneration.world;
        if (world == null) return 0f;
        try
        {
            var blockInfo = world.GetBlockInfo(world.GetBlock(pos));
            return blockInfo?.health ?? 0f;
        }
        catch
        {
            return 0f;
        }
    }

    // 辅助
    private static bool IsPlayerRelated(Item item)
    {
        var body = BodyUtil.Body;
        return item != null
               && body != null
               && item.transform != null
               && item.transform.IsChildOf(body.transform);
    }

    // 电池物品
    [HarmonyPatch(typeof(BatteryItem))]
    public static class BatteryItemPatch
    {
        [HarmonyPatch("LoadBattery")]
        [HarmonyPostfix]
        private static void LoadBatteryPostfix(BatteryItem __instance)
        {
            if (__instance == null || !__instance) return;
            var device = __instance.GetComponent<Item>();
            if (!device || !IsPlayerRelated(device)) return;

            EventUtil.Trigger(new BatteryLoadEvent
            {
                Device = device,
                Battery = null,
                BatteryType = __instance.batteryType ?? string.Empty
            });
        }

        [HarmonyPatch("UnloadBattery")]
        [HarmonyPrefix]
        private static void UnloadBatteryPrefix(BatteryItem __instance)
        {
            if (__instance == null || !__instance) return;
            var device = __instance.GetComponent<Item>();
            if (!device || !IsPlayerRelated(device)) return;

            EventUtil.Trigger(new BatteryUnloadEvent
            {
                Device = device,
                BatteryType = __instance.batteryType ?? string.Empty
            });
        }
    }

    // 自动泵
    [HarmonyPatch(typeof(AutoPump), "Update")]
    [HarmonyPostfix]
    private static void AutoPumpUpdatePostfix(AutoPump __instance)
    {
        if (__instance == null || !__instance) return;
        var item = __instance.GetComponent<Item>();
        if (!item) return;

        var body = PlayerCamera.main != null
            ? PlayerCamera.main.body
            : null;
        var active = body != null
                     && item.battery != null
                     && item.battery.hasCharge
                     && body.HasWearable(item)
                     && body.bloodPressure < 85f;

        var id = __instance.GetInstanceID();
        var wasActive = ActiveAutoPumps.Contains(id);
        switch (active)
        {
            case true when !wasActive:
                ActiveAutoPumps.Add(id);
                EventUtil.Trigger(new AutoPumpActiveEvent { Item = item });
                break;
            case false when wasActive:
                ActiveAutoPumps.Remove(id);
                EventUtil.Trigger(new AutoPumpInactiveEvent { Item = item });
                break;
        }
    }

    // 手摇充电器
    [HarmonyPatch(typeof(BatteryRecharger), "OnUse")]
    [HarmonyPostfix]
    private static void BatteryRechargerOnUsePostfix(BatteryRecharger __instance)
    {
        if (__instance == null || !__instance) return;
        var building = __instance.GetComponent<BuildingEntity>();
        EventUtil.Trigger(new BatteryRechargeEvent { Charger = building });
    }

    // 捕兽夹
    [HarmonyPatch(typeof(BearTrap))]
    public static class BearTrapPatch
    {
        [HarmonyPatch("OnTriggerEnter2D")]
        [HarmonyPostfix]
        private static void OnTriggerEnter2DPostfix(BearTrap __instance)
        {
            if (__instance == null || !__instance) return;
            if (__instance.caughtLimb == null) return;

            var id = __instance.GetInstanceID();
            if (!TriggeredTraps.Add(id)) return;

            EventUtil.Trigger(new BearTrapTriggerEvent { Trap = __instance, Limb = __instance.caughtLimb });
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdatePostfix(BearTrap __instance)
        {
            if (__instance == null || !__instance) return;
            var id = __instance.GetInstanceID();
            if (__instance.caughtLimb == null && TriggeredTraps.Remove(id))
                EventUtil.Trigger(new BearTrapReleaseEvent { Trap = __instance });
        }
    }

    // 生物终端
    [HarmonyPatch(typeof(BioTerminalScript), "OnUse")]
    [HarmonyPostfix]
    private static void BioTerminalScriptOnUsePostfix(BioTerminalScript __instance)
    {
        if (__instance == null || !__instance) return;
        var building = __instance.GetComponent<BuildingEntity>();
        EventUtil.Trigger(new BioTerminalUseEvent { Terminal = building, Success = building != null });
    }

    // 流血粒子（地面血迹）
    [HarmonyPatch(typeof(BleedParticle), "Update")]
    [HarmonyPostfix]
    private static void BleedParticleUpdatePostfix(BleedParticle __instance)
    {
        if (__instance == null || !__instance) return;

        // 读取私有 spawned 字段：spawned 归零表示刚生成了血迹（spawned>=every 时重置为 0）
        var spawned = GetSpawnedCount(__instance);
        if (spawned == null) return;

        var id = __instance.GetInstanceID();
        var value = spawned.Value;

        BloodSpawnCounts.TryGetValue(id, out var prev);
        BloodSpawnCounts[id] = value;

        // 从非 0 变为 0 → 刚生成血迹；首次无记录且为 0 不触发
        if (prev != 0 && value == 0)
            EventUtil.Trigger(new GroundBloodEvent
            {
                Position = __instance.transform.position,
                Vomit = __instance.vomit
            });
    }


    // 方块伤害
    [HarmonyPatch(typeof(BlockDamage), "UpdateSprite")]
    [HarmonyPostfix]
    private static void BlockDamageUpdateSpritePostfix(BlockDamage __instance)
    {
        var health = GetBlockHealth(__instance.pos);
        var destroyed = health > 0f && __instance.damage >= health;

        EventUtil.Trigger(new BlockDamagedEvent
        {
            Pos = __instance.pos,
            Damage = __instance.damage,
            Destroyed = destroyed
        });
    }


    // 蓝图
    [HarmonyPatch(typeof(BlueprintScript), "Awake")]
    private static void BlueprintScriptAwakePostfix(BlueprintScript __instance)
    {
        if (__instance == null || !__instance) return;
        var item = __instance.GetComponent<Item>();
        if (!item) return;

        EventUtil.Trigger(new BlueprintCreateEvent
        {
            Blueprint = item,
            RecipeIndex = __instance.recipeIndex
        });
    }

    // 已购物品到期
    [HarmonyPatch(typeof(BoughtItem), "Update")]
    [HarmonyPostfix]
    private static void BoughtItemUpdatePostfix(BoughtItem __instance)
    {
        if (__instance == null || !__instance) return;
        if (__instance.time >= 0f) return;

        var id = __instance.GetInstanceID();
        if (!ExpiredBoughtItems.Add(id)) return;

        var item = __instance.GetComponent<Item>();
        if (item)
            EventUtil.Trigger(new BoughtItemExpireEvent { Item = item });
    }

    // 弹跳蘑菇
    [HarmonyPatch(typeof(BounceShroom), "OnTriggerEnter2D")]
    [HarmonyPostfix]
    private static void BounceShroomOnTriggerEnter2DPostfix(BounceShroom __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new BounceShroomBounceEvent { Mushroom = __instance });
    }

    // 建筑实体破坏
    [HarmonyPatch(typeof(BuildingEntity), "Update")]
    [HarmonyPostfix]
    private static void BuildingEntityUpdatePostfix(BuildingEntity __instance)
    {
        if (__instance == null || !__instance) return;
        // health < 0.5 时方法内执行破坏逻辑
        if (__instance.health >= 0.5f) return;

        var id = __instance.GetInstanceID();
        if (!DestroyedBuildings.Add(id)) return;

        EventUtil.Trigger(new BuildingDestroyEvent
        {
            Building = __instance,
            BuildingId = __instance.id ?? string.Empty
        });
    }
}