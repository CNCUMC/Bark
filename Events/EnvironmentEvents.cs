using Bark.Event;
using UnityEngine;

namespace Bark.Events;

[ScriptEvent("onCaveTickSpawn")]
public class CaveTickSpawnEvent : BarkEvent
{
    public Vector2 Position { get; set; }
}

[ScriptEvent("onClimbableRegister")]
public class ClimbableRegisterEvent : BarkEvent
{
    public Climbable Climbable { get; set; } = null!;
    public float TotalLength { get; set; }
}

[ScriptEvent("onCoilShock")]
public class CoilShockEvent : BarkEvent
{
    public CoilScript Coil { get; set; } = null!;
    public Limb Limb { get; set; } = null!;
}

[ScriptEvent("onCorpseSeen")]
public class CorpseSeenEvent : BarkEvent
{
    public CorpseScript Corpse { get; set; } = null!;
    public bool AnimalCorpse { get; set; }
}

[ScriptEvent("onCorpseDestroy")]
public class CorpseDestroyEvent : BarkEvent
{
    public CorpseScript Corpse { get; set; } = null!;
}
