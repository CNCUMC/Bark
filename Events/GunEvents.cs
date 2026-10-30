using Bark.Event;
using JetBrains.Annotations;

namespace Bark.Events;

[ScriptEvent("onGunFire")]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GunFireEvent : BarkEvent
{
    public Item GunItem { get; set; } = null!;
    public bool Suicide { get; set; }
}

[ScriptEvent("onGunJam")]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GunJamEvent : BarkEvent
{
    public Item GunItem { get; set; } = null!;
}

[ScriptEvent("onGunLoadAmmo")]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GunLoadAmmoEvent : BarkEvent
{
    public Item GunItem { get; set; } = null!;
    public string AmmoItemId { get; set; } = string.Empty;
    public int Rounds { get; set; }
}

[ScriptEvent("onGunRack")]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GunRackEvent : BarkEvent
{
    public Item GunItem { get; set; } = null!;
    public bool Racked { get; set; }
}

[ScriptEvent("onGunSafetyToggle")]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GunSafetyToggleEvent : BarkEvent
{
    public Item GunItem { get; set; } = null!;
    public bool Safe { get; set; }
}

[ScriptEvent("onGunUnload")]
[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GunUnloadEvent : BarkEvent
{
    public Item GunItem { get; set; } = null!;
    public int RoundsUnloaded { get; set; }
}
