using Bark.Event;

namespace Bark.Events;

// ============================================================
// 精神抹除事件
// ============================================================

// 精神抹除事件：玩家触发精神抹除（mindwipe）时触发
[ScriptEvent("onMindwipe")]
public class MindwipeEvent : BarkEvent
{
}

// ============================================================
// 辐射线事件
// ============================================================

// 辐射线启动事件：辐射线（Radline）开始向玩家逼近时触发
[ScriptEvent("onRadiationStart")]
public class RadiationStartEvent : BarkEvent
{
}

// ============================================================
// 游戏存档事件
// ============================================================

// 游戏存档事件：玩家保存游戏时触发
[ScriptEvent("onGameSave")]
public class GameSaveEvent : BarkEvent
{
}

// ============================================================
// 技能升级事件
// ============================================================

// 技能升级事件：玩家某项属性升级时触发
[ScriptEvent("onSkillLevelUp")]
public class SkillLevelUpEvent : BarkEvent
{
    // 升级的属性（0=力量，1=耐力，2=智力）
    public int Stat { get; set; }

    // 升级前等级
    public int OldLevel { get; set; }

    // 升级后等级
    public int NewLevel { get; set; }
}

// ============================================================
// 商人事件
// ============================================================

// 商人相遇事件：玩家接近商人、开始对话时触发
[ScriptEvent("onTraderMeet")]
public class TraderMeetEvent : BarkEvent
{
    // 商人
    public TraderScript Trader { get; set; } = null!;

    // 商人角色编号
    public int Character { get; set; }

    // 初始好感度
    public float Reputation { get; set; }
}

// 商人讲价事件：玩家与商人讲价时触发
[ScriptEvent("onTraderHaggle")]
public class TraderHaggleEvent : BarkEvent
{
    // 商人
    public TraderScript Trader { get; set; } = null!;

    // 讲价后的好感度
    public float Reputation { get; set; }
}

// 商人死亡事件：商人被击杀时触发
[ScriptEvent("onTraderDeath")]
public class TraderDeathEvent : BarkEvent
{
    // 死亡的商人
    public TraderScript Trader { get; set; } = null!;
}

// ============================================================
// 炮塔事件
// ============================================================

// 炮塔开火事件：炮塔发现并射击玩家时触发
[ScriptEvent("onTurretShoot")]
public class TurretShootEvent : BarkEvent
{
    // 炮塔
    public TurretScript Turret { get; set; } = null!;
}

// 炮塔爆炸事件：炮塔被摧毁爆炸时触发
[ScriptEvent("onTurretExplode")]
public class TurretExplodeEvent : BarkEvent
{
    // 爆炸的炮塔
    public TurretScript Turret { get; set; } = null!;
}

// ============================================================
// 世界重生事件
// ============================================================

// 世界重生事件：玩家进入下一层、世界重新生成时触发
[ScriptEvent("onWorldRegenerate")]
public class WorldRegenerateEvent : BarkEvent
{
    // 是否连续跨越两层
    public bool Twice { get; set; }
}

// ============================================================
// 电锯事件
// ============================================================

// 电锯切割事件：电锯锯到玩家肢体时触发
[ScriptEvent("onSawbladeHit")]
public class SawbladeHitEvent : BarkEvent
{
    // 电锯
    public SawbladeScript Sawblade { get; set; } = null!;
}

// ============================================================
// 声波炮事件
// ============================================================

// 声波炮发射事件：声波炮完成充能并对玩家发射时触发
[ScriptEvent("onSoundCannonShoot")]
public class SoundCannonShootEvent : BarkEvent
{
    // 声波炮
    public SoundCannon Cannon { get; set; } = null!;
}
