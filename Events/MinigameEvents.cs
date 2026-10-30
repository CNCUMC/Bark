using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onAEDMinigameStart")]
public class AEDMinigameStartEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
    public int LimbIndex { get; set; }
}

[ScriptEvent("onAEDMinigameDefibrillate")]
public class AEDMinigameDefibrillateEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
    public bool WasFibrillating { get; set; }
}

[ScriptEvent("onAEDMinigameFail")]
public class AEDMinigameFailEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
}

[ScriptEvent("onBandageMinigameStart")]
public class BandageMinigameStartEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
    public float BandageAngle { get; set; }
}

[ScriptEvent("onBandageMinigameWrap")]
public class BandageMinigameWrapEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
}

[ScriptEvent("onDislocationMinigameStart")]
public class DislocationMinigameStartEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
    public bool HasWrench { get; set; }
}

[ScriptEvent("onDislocationMinigameSuccess")]
public class DislocationMinigameSuccessEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
}

[ScriptEvent("onDislocationMinigameBreak")]
public class DislocationMinigameBreakEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
}

[ScriptEvent("onHandCrankMinigameStart")]
public class HandCrankMinigameStartEvent : BarkEvent;

[ScriptEvent("onHandCrankMinigameCharge")]
public class HandCrankMinigameChargeEvent : BarkEvent
{
    public float Angle { get; set; }
}

[ScriptEvent("onHandCrankMinigameEnd")]
public class HandCrankMinigameEndEvent : BarkEvent;

[ScriptEvent("onKeypadMinigameStart")]
public class KeypadMinigameStartEvent : BarkEvent
{
    public BuildingEntity ToDestroy { get; set; } = null!;
}

[ScriptEvent("onKeypadMinigameSuccess")]
public class KeypadMinigameSuccessEvent : BarkEvent
{
    public BuildingEntity ToDestroy { get; set; } = null!;
}

[ScriptEvent("onLockpingMinigameStart")]
public class LockpingMinigameStartEvent : BarkEvent
{
    public BuildingEntity ToDestroy { get; set; } = null!;
    public bool HasPick { get; set; }
}

[ScriptEvent("onLockpingMinigameSuccess")]
public class LockpingMinigameSuccessEvent : BarkEvent
{
    public BuildingEntity ToDestroy { get; set; } = null!;
}

[ScriptEvent("onLockpingMinigameStuck")]
public class LockpingMinigameStuckEvent : BarkEvent
{
    public BuildingEntity ToDestroy { get; set; } = null!;
}

[ScriptEvent("onManualDefibMinigameStart")]
public class ManualDefibMinigameStartEvent : BarkEvent
{
    public Limb Limb { get; set; }
    public bool OnTorso { get; set; }
}

[ScriptEvent("onManualDefibMinigameShock")]
public class ManualDefibMinigameShockEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
    public float Charge { get; set; }
}

[ScriptEvent("onManualDefibMinigameEnd")]
public class ManualDefibMinigameEndEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
}

[ScriptEvent("onShrapnelMinigameStart")]
public class ShrapnelMinigameStartEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
    public bool HasTweezers { get; set; }
}

[ScriptEvent("onShrapnelMinigameSuccess")]
public class ShrapnelMinigameSuccessEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
}

[ScriptEvent("onShrapnelMinigameFail")]
public class ShrapnelMinigameFailEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
}

[ScriptEvent("onSyringeMinigameStart")]
public class SyringeMinigameStartEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
}

[ScriptEvent("onSyringeMinigameInject")]
public class SyringeMinigameInjectEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
}

[ScriptEvent("onSyringeMinigameFail")]
public class SyringeMinigameFailEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
}

[ScriptEvent("onAmputationMinigameStart")]
public class AmputationMinigameStartEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
}

[ScriptEvent("onAmputationMinigameSuccess")]
public class AmputationMinigameSuccessEvent : BarkEvent
{
    public Limb Limb { get; set; } = null!;
}
