using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onDamageableDamaged")]
public class DamageableDamagedEvent : BarkEvent
{
    public Damageable Damageable { get; set; } = null!;
    public float Damage { get; set; }
}

[ScriptEvent("onDamagingCrateHit")]
public class DamagingCrateHitEvent : BarkEvent
{
    public DamagingCrate Crate { get; set; } = null!;
    public int Type { get; set; }
}

[ScriptEvent("onDrillPodRepair")]
public class DrillPodRepairEvent : BarkEvent
{
    public DrillPod Pod { get; set; } = null!;
}

[ScriptEvent("onDrillPodUse")]
public class DrillPodUseEvent : BarkEvent
{
    public DrillPod Pod { get; set; } = null!;
}

[ScriptEvent("onThornbackNear")]
public class ThornbackNearEvent : BarkEvent
{
    public ElderThornbackBehaviour Thornback { get; set; } = null!;
}

[ScriptEvent("onThornbackStage")]
public class ThornbackStageEvent : BarkEvent
{
    public ElderThornbackBehaviour Thornback { get; set; } = null!;
    public int Stage { get; set; }
}

[ScriptEvent("onThornbackDeath")]
public class ThornbackDeathEvent : BarkEvent
{
    public ElderThornbackBehaviour Thornback { get; set; } = null!;
}

[ScriptEvent("onPdaUse")]
public class PdaUseEvent : BarkEvent
{
    public Item Pda { get; set; } = null!;
    public bool FirstRead { get; set; }
}

[ScriptEvent("onGeyserRumble")]
public class GeyserRumbleEvent : BarkEvent
{
    public GeyserScript Geyser { get; set; } = null!;
}

[ScriptEvent("onGeyserActivate")]
public class GeyserActivateEvent : BarkEvent
{
    public GeyserScript Geyser { get; set; } = null!;
}

[ScriptEvent("onGlobalDark")]
public class GlobalDarkEvent : BarkEvent
{
    public bool Darkening { get; set; }
}

[ScriptEvent("onGrabberPlantGrab")]
public class GrabberPlantGrabEvent : BarkEvent
{
    public GrabberPlant Plant { get; set; } = null!;
}

[ScriptEvent("onGrapplingHookFire")]
public class GrapplingHookFireEvent : BarkEvent
{
    public GrapplingHook Hook { get; set; } = null!;
}

[ScriptEvent("onGrapplingHookHit")]
public class GrapplingHookHitEvent : BarkEvent
{
    public GrapplingHook Hook { get; set; } = null!;
}

[ScriptEvent("onGrapplingHookReturn")]
public class GrapplingHookReturnEvent : BarkEvent
{
    public GrapplingHook Hook { get; set; } = null!;
}

[ScriptEvent("onItemDestroy")]
public class ItemDestroyEvent : BarkEvent
{
    public string ItemId { get; set; } = string.Empty;
    public Item? Item { get; set; }
}

[ScriptEvent("onJumpPadBounce")]
public class JumpPadBounceEvent : BarkEvent
{
    public JumpPadScript Pad { get; set; } = null!;
}

[ScriptEvent("onLifepodButtonPress")]
public class LifepodButtonPressEvent : BarkEvent
{
    public int Type { get; set; }
}

[ScriptEvent("onLifepodShowerActivate")]
public class LifepodShowerActivateEvent : BarkEvent
{
    public LifepodShower Shower { get; set; } = null!;
}

[ScriptEvent("onMedStationHeal")]
public class MedStationHealEvent : BarkEvent
{
    public MedStationScript Station { get; set; } = null!;
}

[ScriptEvent("onMineTrigger")]
public class MineTriggerEvent : BarkEvent
{
    public MineScript Mine { get; set; } = null!;
}

[ScriptEvent("onObserverLastStand")]
public class ObserverLastStandEvent : BarkEvent
{
    public Observer Observer { get; set; } = null!;
}

[ScriptEvent("onObserverGunSuicide")]
public class ObserverGunSuicideEvent : BarkEvent
{
    public Observer Observer { get; set; } = null!;
}

[ScriptEvent("onOpenableUse")]
public class OpenableUseEvent : BarkEvent
{
    public Openable Openable { get; set; } = null!;
    public string Mode { get; set; } = string.Empty;
}

[ScriptEvent("onPlushSqueak")]
public class PlushSqueakEvent : BarkEvent
{
    public PlushScript Plush { get; set; } = null!;
}

[ScriptEvent("onPreRunStart")]
public class PreRunStartEvent : BarkEvent;

[ScriptEvent("onPreRunLoad")]
public class PreRunLoadEvent : BarkEvent;

[ScriptEvent("onPreRunTutorial")]
public class PreRunTutorialEvent : BarkEvent;

[ScriptEvent("onOpiateOverdose")]
public class OpiateOverdoseEvent : BarkEvent;

[ScriptEvent("onSelfDestruct")]
public class SelfDestructEvent : BarkEvent;

[ScriptEvent("onWoundViewToggle")]
public class WoundViewToggleEvent : BarkEvent
{
    public bool Open { get; set; }
}

[ScriptEvent("onCraftPanelToggle")]
public class CraftPanelToggleEvent : BarkEvent
{
    public bool Open { get; set; }
}

[ScriptEvent("onAmmoUnload")]
public class AmmoUnloadEvent : BarkEvent
{
    public Item Magazine { get; set; } = null!;
}

[ScriptEvent("onAmmoLoad")]
public class AmmoLoadEvent : BarkEvent
{
    public Item Magazine { get; set; } = null!;
}

[ScriptEvent("onAltHoverToggle")]
public class AltHoverToggleEvent : BarkEvent
{
    public bool Active { get; set; }
}
