using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onMindwipe")]
public class MindwipeEvent : BarkEvent;

[ScriptEvent("onRadiationStart")]
public class RadiationStartEvent : BarkEvent;

[ScriptEvent("onGameSave")]
public class GameSaveEvent : BarkEvent;

[ScriptEvent("onSkillLevelUp")]
public class SkillLevelUpEvent : BarkEvent
{
    public int Stat { get; set; }
    public int OldLevel { get; set; }
    public int NewLevel { get; set; }
}

[ScriptEvent("onTraderMeet")]
public class TraderMeetEvent : BarkEvent
{
    public TraderScript Trader { get; set; } = null!;
    public int Character { get; set; }
    public float Reputation { get; set; }
}

[ScriptEvent("onTraderHaggle")]
public class TraderHaggleEvent : BarkEvent
{
    public TraderScript Trader { get; set; } = null!;
    public float Reputation { get; set; }
}

[ScriptEvent("onTraderDeath")]
public class TraderDeathEvent : BarkEvent
{
    public TraderScript Trader { get; set; } = null!;
}

[ScriptEvent("onTurretShoot")]
public class TurretShootEvent : BarkEvent
{
    public TurretScript Turret { get; set; } = null!;
}

[ScriptEvent("onTurretExplode")]
public class TurretExplodeEvent : BarkEvent
{
    public TurretScript Turret { get; set; } = null!;
}

[ScriptEvent("onWorldRegenerate")]
public class WorldRegenerateEvent : BarkEvent
{
    public bool Twice { get; set; }
}

[ScriptEvent("onSawbladeHit")]
public class SawbladeHitEvent : BarkEvent
{
    public SawbladeScript Sawblade { get; set; } = null!;
}

[ScriptEvent("onSoundCannonShoot")]
public class SoundCannonShootEvent : BarkEvent
{
    public SoundCannon Cannon { get; set; } = null!;
}
