using Bark.Event;

namespace Bark.Events;

// ============================================================
// 洞穴蜘蛛生成事件
// ============================================================

// 洞穴蜘蛛生成事件：玩家触发洞穴蜘蛛生成器，开始生成蜘蛛时触发
[ScriptEvent("onCaveTickSpawn")]
public class CaveTickSpawnEvent : BarkEvent
{
    // 生成器位置
    public UnityEngine.Vector2 Position { get; set; }
}

// ============================================================
// 可攀爬物事件
// ============================================================

// 可攀爬物注册事件：可攀爬物在世界中初始化并注册时触发
[ScriptEvent("onClimbableRegister")]
public class ClimbableRegisterEvent : BarkEvent
{
    // 可攀爬物组件
    public Climbable Climbable { get; set; } = null!;

    // 攀爬总长度
    public float TotalLength { get; set; }
}

// ============================================================
// 电线圈事件
// ============================================================

// 线圈电击事件：电线圈对肢体放电、玩家被电击时触发
[ScriptEvent("onCoilShock")]
public class CoilShockEvent : BarkEvent
{
    // 电线圈
    public CoilScript Coil { get; set; } = null!;

    // 被电击的肢体
    public Limb Limb { get; set; } = null!;
}

// ============================================================
// 尸体事件
// ============================================================

// 尸体发现事件：玩家首次看到尸体并触发心理反应时触发
[ScriptEvent("onCorpseSeen")]
public class CorpseSeenEvent : BarkEvent
{
    // 尸体
    public CorpseScript Corpse { get; set; } = null!;

    // 是否动物尸体
    public bool AnimalCorpse { get; set; }
}

// 尸体破坏事件：玩家破坏尸体时触发
[ScriptEvent("onCorpseDestroy")]
public class CorpseDestroyEvent : BarkEvent
{
    // 被破坏的尸体
    public CorpseScript Corpse { get; set; } = null!;
}
