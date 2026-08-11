using Bark.Event;

namespace Bark.Events;

// ============================================================
// 电池物品事件
// ============================================================

// 装电池事件：给设备装入电池时触发
[ScriptEvent("onBatteryLoad")]
public class BatteryLoadEvent : BarkEvent
{
    // 目标设备物品
    public Item Device { get; set; } = null!;

    // 装入的电池物品（LoadBattery 中电池被销毁，通常为 null，用 BatteryType 标识）
    public Item? Battery { get; set; }

    // 电池类型（如 "smallbattery" / "mediumbattery" / "largebattery"）
    public string BatteryType { get; set; } = string.Empty;
}

// 卸电池事件：从设备卸下电池时触发
[ScriptEvent("onBatteryUnload")]
public class BatteryUnloadEvent : BarkEvent
{
    // 目标设备物品
    public Item Device { get; set; } = null!;

    // 卸下的电池类型（如 "smallbattery" / "mediumbattery" / "largebattery"）
    public string BatteryType { get; set; } = string.Empty;
}

// ============================================================
// 自动泵事件
// ============================================================

// 自动泵运作事件：自动泵在血压偏低时补充血压
[ScriptEvent("onAutoPumpActive")]
public class AutoPumpActiveEvent : BarkEvent
{
    // 自动泵物品
    public Item Item { get; set; } = null!;
}

// 自动泵停止事件：自动泵停止运作（断开或电量耗尽）
[ScriptEvent("onAutoPumpInactive")]
public class AutoPumpInactiveEvent : BarkEvent
{
    // 自动泵物品
    public Item Item { get; set; } = null!;
}

// ============================================================
// 电池充电器事件
// ============================================================

// 电池充电事件：将电池放入充电器充电时触发
[ScriptEvent("onBatteryRecharge")]
public class BatteryRechargeEvent : BarkEvent
{
    // 充电器建筑实体
    public BuildingEntity Charger { get; set; } = null!;
}

// ============================================================
// 捕兽夹事件
// ============================================================

// 捕兽夹夹住事件：捕兽夹夹住肢体时触发
[ScriptEvent("onBearTrapTrigger")]
public class BearTrapTriggerEvent : BarkEvent
{
    // 捕兽夹
    public BearTrap Trap { get; set; } = null!;

    // 被夹住的肢体
    public Limb Limb { get; set; } = null!;
}

// 捕兽夹松开事件：被夹住的肢体挣脱后触发
[ScriptEvent("onBearTrapRelease")]
public class BearTrapReleaseEvent : BarkEvent
{
    // 捕兽夹
    public BearTrap Trap { get; set; } = null!;
}

// ============================================================
// 生物终端事件
// ============================================================

// 生物终端使用事件：玩家使用生物终端成功兑换物品时触发
[ScriptEvent("onBioTerminalUse")]
public class BioTerminalUseEvent : BarkEvent
{
    // 生物终端建筑实体
    public BuildingEntity Terminal { get; set; } = null!;

    // 是否成功（兑出物品）
    public bool Success { get; set; }
}

// ============================================================
// 地面血迹事件
// ============================================================

// 地面血迹生成事件：流血粒子落地形成地面血迹时触发
[ScriptEvent("onGroundBlood")]
public class GroundBloodEvent : BarkEvent
{
    // 地面血迹生成的世界坐标
    public UnityEngine.Vector2 Position { get; set; }

    // 是否呕吐物（vomit=true）
    public bool Vomit { get; set; }
}

// ============================================================
// 方块伤害事件
// ============================================================

// 方块受损事件：方块受到伤害、更新受损贴图时触发
[ScriptEvent("onBlockDamaged")]
public class BlockDamagedEvent : BarkEvent
{
    // 受损方块的世界格子坐标
    public UnityEngine.Vector2Int Pos { get; set; }

    // 当前伤害量
    public float Damage { get; set; }

    // 是否被完全破坏（damage >= health，贴图销毁）
    public bool Destroyed { get; set; }
}

// ============================================================
// 蓝图事件
// ============================================================

// 蓝图生成事件：蓝图物品在世界中生成并分配配方时触发
[ScriptEvent("onBlueprintCreate")]
public class BlueprintCreateEvent : BarkEvent
{
    // 蓝图物品
    public Item Blueprint { get; set; } = null!;

    // 分配的配方索引（Recipes.recipes 中的下标）
    public int RecipeIndex { get; set; }
}

// ============================================================
// 已购买物品事件
// ============================================================

// 已购买物品到期事件：商店购买的物品到达时限被移除时触发
[ScriptEvent("onBoughtItemExpire")]
public class BoughtItemExpireEvent : BarkEvent
{
    // 过期的已购买物品
    public Item Item { get; set; } = null!;
}

// ============================================================
// 弹跳蘑菇事件
// ============================================================

// 蘑菇弹跳事件：玩家踩到弹跳蘑菇被弹起时触发
[ScriptEvent("onBounceShroomBounce")]
public class BounceShroomBounceEvent : BarkEvent
{
    // 弹跳蘑菇
    public BounceShroom Mushroom { get; set; } = null!;
}

// ============================================================
// 建筑实体事件
// ============================================================

// 建筑破坏事件：建筑实体被完全破坏时触发
[ScriptEvent("onBuildingDestroy")]
public class BuildingDestroyEvent : BarkEvent
{
    // 被破坏的建筑实体
    public BuildingEntity Building { get; set; } = null!;

    // 建筑 ID
    public string BuildingId { get; set; } = string.Empty;
}
