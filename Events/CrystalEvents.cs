using Bark.Event;

namespace Bark.Events;

// ============================================================
// 水晶效果事件（所有水晶效果的通用事件）
// ============================================================

// 水晶被触碰事件：玩家/物品触碰到水晶效果时触发。
// EffectType 表示具体效果类型（如 "CrystalHealing"、"CrystalElectric"）。
[ScriptEvent("onCrystalTouch")]
public class CrystalTouchEvent : BarkEvent
{
    // 效果类型（CrystalEffect 子类名，如 "CrystalHealing"）
    public string EffectType { get; set; } = string.Empty;

    // 水晶行为组件
    public CrystalBehaviour Crystal { get; set; } = null!;
}

// 水晶被攻击事件：玩家徒手/用物品攻击水晶时触发。
// EffectType 表示具体效果类型。
[ScriptEvent("onCrystalHit")]
public class CrystalHitEvent : BarkEvent
{
    // 效果类型（CrystalEffect 子类名）
    public string EffectType { get; set; } = string.Empty;

    // 水晶行为组件
    public CrystalBehaviour Crystal { get; set; } = null!;
}

// 水晶敌人攻击事件：水晶敌人对玩家发起突刺攻击时触发
[ScriptEvent("onCrystalEnemyAttack")]
public class CrystalEnemyAttackEvent : BarkEvent
{
    // 攻击的水晶敌人
    public CrystalEnemy Enemy { get; set; } = null!;
}

// 水晶敌人死亡事件：水晶敌人被击杀时触发
[ScriptEvent("onCrystalEnemyDeath")]
public class CrystalEnemyDeathEvent : BarkEvent
{
    // 死亡的水晶敌人
    public CrystalEnemy Enemy { get; set; } = null!;
}
