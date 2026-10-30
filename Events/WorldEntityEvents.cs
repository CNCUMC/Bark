using Bark.Event;
using UnityEngine;

namespace Bark.Events;

[ScriptEvent("onBatteryLoad")]
public class BatteryLoadEvent : BarkEvent
{
    public Item Device { get; set; } = null!;
    public Item? Battery { get; set; }
    public string BatteryType { get; set; } = string.Empty;
}

[ScriptEvent("onBatteryUnload")]
public class BatteryUnloadEvent : BarkEvent
{
    public Item Device { get; set; } = null!;
    public string BatteryType { get; set; } = string.Empty;
}

[ScriptEvent("onAutoPumpActive")]
public class AutoPumpActiveEvent : BarkEvent
{
    public Item Item { get; set; } = null!;
}

[ScriptEvent("onAutoPumpInactive")]
public class AutoPumpInactiveEvent : BarkEvent
{
    public Item Item { get; set; } = null!;
}

[ScriptEvent("onBatteryRecharge")]
public class BatteryRechargeEvent : BarkEvent
{
    public BuildingEntity Charger { get; set; } = null!;
}

[ScriptEvent("onBearTrapTrigger")]
public class BearTrapTriggerEvent : BarkEvent
{
    public BearTrap Trap { get; set; } = null!;
    public Limb Limb { get; set; } = null!;
}

[ScriptEvent("onBearTrapRelease")]
public class BearTrapReleaseEvent : BarkEvent
{
    public BearTrap Trap { get; set; } = null!;
}

[ScriptEvent("onBioTerminalUse")]
public class BioTerminalUseEvent : BarkEvent
{
    public BuildingEntity Terminal { get; set; } = null!;
    public bool Success { get; set; }
}

[ScriptEvent("onGroundBlood")]
public class GroundBloodEvent : BarkEvent
{
    public Vector2 Position { get; set; }
    public bool Vomit { get; set; }
}

[ScriptEvent("onBlockDamaged")]
public class BlockDamagedEvent : BarkEvent
{
    public Vector2Int Pos { get; set; }
    public float Damage { get; set; }
    public bool Destroyed { get; set; }
}

[ScriptEvent("onBlueprintCreate")]
public class BlueprintCreateEvent : BarkEvent
{
    public Item Blueprint { get; set; } = null!;
    public int RecipeIndex { get; set; }
}

[ScriptEvent("onBoughtItemExpire")]
public class BoughtItemExpireEvent : BarkEvent
{
    public Item Item { get; set; } = null!;
}

[ScriptEvent("onBounceShroomBounce")]
public class BounceShroomBounceEvent : BarkEvent
{
    public BounceShroom Mushroom { get; set; } = null!;
}

[ScriptEvent("onBuildingDestroy")]
public class BuildingDestroyEvent : BarkEvent
{
    public BuildingEntity Building { get; set; } = null!;
    public string BuildingId { get; set; } = string.Empty;
}
