using Bark.Event;

namespace Bark.Events;

// 可损坏物体事件
// 可损坏物受击事件：可损坏物体（建筑/物品）受到伤害时触发
[ScriptEvent("onDamageableDamaged")]
public class DamageableDamagedEvent : BarkEvent
{
    // 受击的可损坏物
    public Damageable Damageable { get; set; } = null!;

    // 受到的伤害量
    public float Damage { get; set; }
}

// 伤害板条箱事件
// 伤害板条箱撞击事件：高速坠落的板条箱发生碰撞时触发。
// Type：0=砸晕身体，1=砸伤肢体，2=砸断肢体。
[ScriptEvent("onDamagingCrateHit")]
public class DamagingCrateHitEvent : BarkEvent
{
    // 板条箱
    public DamagingCrate Crate { get; set; } = null!;

    // 板条箱类型（0/1/2）
    public int Type { get; set; }
}

// 钻探舱事件
// 钻探舱修复事件：用维修包修复钻探舱时触发
[ScriptEvent("onDrillPodRepair")]
public class DrillPodRepairEvent : BarkEvent
{
    // 钻探舱
    public DrillPod Pod { get; set; } = null!;
}

// 钻探舱使用事件：钻探舱激活、重建世界（传送）时触发
[ScriptEvent("onDrillPodUse")]
public class DrillPodUseEvent : BarkEvent
{
    // 钻探舱
    public DrillPod Pod { get; set; } = null!;
}

// 脊背兽长老事件
// 长老靠近事件：长老进入玩家感知范围时触发
[ScriptEvent("onThornbackNear")]
public class ThornbackNearEvent : BarkEvent
{
    // 长老
    public ElderThornbackBehaviour Thornback { get; set; } = null!;
}

// 长老阶段转换事件：长老进入下一阶段（狂暴）时触发
[ScriptEvent("onThornbackStage")]
public class ThornbackStageEvent : BarkEvent
{
    // 长老
    public ElderThornbackBehaviour Thornback { get; set; } = null!;

    // 新阶段（1 / 2）
    public int Stage { get; set; }
}

// 长老死亡事件：长老被击杀时触发
[ScriptEvent("onThornbackDeath")]
public class ThornbackDeathEvent : BarkEvent
{
    // 死亡的长老
    public ElderThornbackBehaviour Thornback { get; set; } = null!;
}

// PDA 事件
// PDA 使用事件：玩家使用 PDA（阅读笔记）时触发
[ScriptEvent("onPdaUse")]
public class PdaUseEvent : BarkEvent
{
    // PDA 物品
    public Item Pda { get; set; } = null!;

    // 是否首次阅读（获得经验）
    public bool FirstRead { get; set; }
}

// 间歇泉事件
// 间歇泉轰鸣事件：间歇泉开始轰鸣（即将喷发）时触发
[ScriptEvent("onGeyserRumble")]
public class GeyserRumbleEvent : BarkEvent
{
    // 间歇泉
    public GeyserScript Geyser { get; set; } = null!;
}

// 间歇泉喷发事件：间歇泉喷发（释放液体）时触发
[ScriptEvent("onGeyserActivate")]
public class GeyserActivateEvent : BarkEvent
{
    // 间歇泉
    public GeyserScript Geyser { get; set; } = null!;
}

// 全局暗幕事件
// 屏幕变暗事件：全局暗幕开始变暗时触发
[ScriptEvent("onGlobalDark")]
public class GlobalDarkEvent : BarkEvent
{
    // 是否正在变暗
    public bool Darkening { get; set; }
}

// 捕抓植物事件
// 捕抓植物抓住事件：捕抓植物抓住玩家肢体时触发
[ScriptEvent("onGrabberPlantGrab")]
public class GrabberPlantGrabEvent : BarkEvent
{
    // 捕抓植物
    public GrabberPlant Plant { get; set; } = null!;
}

// 抓钩事件
// 抓钩发射事件：抓钩被发射时触发
[ScriptEvent("onGrapplingHookFire")]
public class GrapplingHookFireEvent : BarkEvent
{
    // 抓钩
    public GrapplingHook Hook { get; set; } = null!;
}

// 抓钩勾住事件：抓钩勾住表面时触发
[ScriptEvent("onGrapplingHookHit")]
public class GrapplingHookHitEvent : BarkEvent
{
    // 抓钩
    public GrapplingHook Hook { get; set; } = null!;
}

// 抓钩拉回事件：抓钩收回时触发
[ScriptEvent("onGrapplingHookReturn")]
public class GrapplingHookReturnEvent : BarkEvent
{
    // 抓钩
    public GrapplingHook Hook { get; set; } = null!;
}

// 物品销毁事件
// 物品销毁事件：物品耐久归零（destroyAtZeroCondition）被销毁时触发
[ScriptEvent("onItemDestroy")]
public class ItemDestroyEvent : BarkEvent
{
    // 被销毁的物品 ID
    public string ItemId { get; set; } = string.Empty;

    // 被销毁的物品（销毁前实例）
    public Item? Item { get; set; }
}

// 跳跃平台事件
// 跳跃平台弹跳事件：玩家踩上跳跃平台被弹起时触发
[ScriptEvent("onJumpPadBounce")]
public class JumpPadBounceEvent : BarkEvent
{
    // 跳跃平台
    public JumpPadScript Pad { get; set; } = null!;
}

// 救生舱事件
// 救生舱按钮事件：玩家按下救生舱按钮（切换温度/激活淋浴）时触发
[ScriptEvent("onLifepodButtonPress")]
public class LifepodButtonPressEvent : BarkEvent
{
    // 按钮类型（0=温度切换，1=淋浴）
    public int Type { get; set; }
}

// 救生舱淋浴激活事件：淋浴被激活、开始冲洗时触发
[ScriptEvent("onLifepodShowerActivate")]
public class LifepodShowerActivateEvent : BarkEvent
{
    // 淋浴
    public LifepodShower Shower { get; set; } = null!;
}

// 医疗站事件
// 医疗站治疗事件：玩家进入医疗站、开始接受治疗时触发
[ScriptEvent("onMedStationHeal")]
public class MedStationHealEvent : BarkEvent
{
    // 医疗站
    public MedStationScript Station { get; set; } = null!;
}

// 地雷事件
// 地雷触发事件：地雷被触发（开始倒计时爆炸）时触发
[ScriptEvent("onMineTrigger")]
public class MineTriggerEvent : BarkEvent
{
    // 地雷
    public MineScript Mine { get; set; } = null!;
}

// 观察者（邪神）事件
// 观察者靠近事件：玩家成功触发"最后坚持"（观察者拉近）时触发
[ScriptEvent("onObserverLastStand")]
public class ObserverLastStandEvent : BarkEvent
{
    // 观察者
    public Observer Observer { get; set; } = null!;
}

// 观察者靠近（枪杀）事件：玩家用枪自杀、观察者拉近时触发
[ScriptEvent("onObserverGunSuicide")]
public class ObserverGunSuicideEvent : BarkEvent
{
    // 观察者
    public Observer Observer { get; set; } = null!;
}

// 可开启物事件
// 可开启物使用事件：玩家打开可开启物（门/箱）时触发。
// Mode：instant=直接打开，keypad=密码，lockpick=撬锁
[ScriptEvent("onOpenableUse")]
public class OpenableUseEvent : BarkEvent
{
    // 可开启物
    public Openable Openable { get; set; } = null!;

    // 打开方式（instant / keypad / lockpick）
    public string Mode { get; set; } = string.Empty;
}

// 毛绒玩具事件
// 毛绒玩具吱吱事件：毛绒玩具被挤压发出声音时触发
[ScriptEvent("onPlushSqueak")]
public class PlushSqueakEvent : BarkEvent
{
    // 毛绒玩具
    public PlushScript Plush { get; set; } = null!;
}

// 开局前事件
// 开局开始事件：玩家开始新游戏时触发
[ScriptEvent("onPreRunStart")]
public class PreRunStartEvent : BarkEvent
{
}

// 读取存档事件：玩家读取存档继续游戏时触发
[ScriptEvent("onPreRunLoad")]
public class PreRunLoadEvent : BarkEvent
{
}

// 开始教程事件：玩家开始教程时触发
[ScriptEvent("onPreRunTutorial")]
public class PreRunTutorialEvent : BarkEvent
{
}

// 阿片类药物事件
// 阿片过量事件：体内阿片受体水平过高（中毒）时触发
[ScriptEvent("onOpiateOverdose")]
public class OpiateOverdoseEvent : BarkEvent
{
}

// 玩家相机事件
// 自毁序列事件：玩家触发自毁（最终结局）时触发
[ScriptEvent("onSelfDestruct")]
public class SelfDestructEvent : BarkEvent
{
}

// 伤口面板开关事件：玩家打开/关闭伤口面板时触发
[ScriptEvent("onWoundViewToggle")]
public class WoundViewToggleEvent : BarkEvent
{
    // 是否打开
    public bool Open { get; set; }
}

// 制作面板开关事件：玩家打开/关闭制作面板时触发
[ScriptEvent("onCraftPanelToggle")]
public class CraftPanelToggleEvent : BarkEvent
{
    // 是否打开
    public bool Open { get; set; }
}

// 弹药事件
// 卸弹事件：从弹匣卸下一发子弹时触发
[ScriptEvent("onAmmoUnload")]
public class AmmoUnloadEvent : BarkEvent
{
    // 弹匣物品
    public Item Magazine { get; set; } = null!;
}

// 装弹事件：向弹匣装入一发子弹时触发
[ScriptEvent("onAmmoLoad")]
public class AmmoLoadEvent : BarkEvent
{
    // 弹匣物品
    public Item Magazine { get; set; } = null!;
}

// Alt 物品标签事件
// Alt 物品标签开关事件：玩家按住/切换 Alt 显示物品标签时触发
[ScriptEvent("onAltHoverToggle")]
public class AltHoverToggleEvent : BarkEvent
{
    // 是否激活（显示物品标签）
    public bool Active { get; set; }
}