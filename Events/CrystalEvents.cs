using Bark.Event;

namespace Bark.Events;

[ScriptEvent("onCrystalTouch")]
public class CrystalTouchEvent : BarkEvent
{
    public string EffectType { get; set; } = string.Empty;
    public CrystalBehaviour Crystal { get; set; } = null!;
}

[ScriptEvent("onCrystalHit")]
public class CrystalHitEvent : BarkEvent
{
    public string EffectType { get; set; } = string.Empty;
    public CrystalBehaviour Crystal { get; set; } = null!;
}

[ScriptEvent("onCrystalEnemyAttack")]
public class CrystalEnemyAttackEvent : BarkEvent
{
    public CrystalEnemy Enemy { get; set; } = null!;
}

[ScriptEvent("onCrystalEnemyDeath")]
public class CrystalEnemyDeathEvent : BarkEvent
{
    public CrystalEnemy Enemy { get; set; } = null!;
}
