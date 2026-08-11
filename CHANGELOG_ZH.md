# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v2.4.0

### 新增
- **自建 KrokMP 网络层（`BarkKrokBridge`）**：Bark 现在通过反射直接对接 KrokoshaCasualtiesMP（KrokMP 4.0.1）网络栈，
  绕开 CUCoreLib 的 `MultiplayerBridge`（它因无法解析 `Server_SendTo` 的 `knetid` 参数类型而永久不可用）。
  这修复了主机与客户端之间的多人脚本模组同步。
- **主机重载时的增量文件同步（`ScriptFileSync`）**：主机执行 `sr` 后会向所有已连接客户端广播文件同步。客户端上报
  各文件 hash，主机以自身文件为准对比，只推送 hash 不同的文件——未修改的文件不会重复传输。中途新加入的模组会被
  全量推送，客户端得以加载。
- **Body 事件（`BodyEventListener`）**：新增一批基于玩家身体状态与行为的事件，均桥接到脚本钩子：
  - 生命体征临界：`onBodyCardiacArrest`（心脏骤停）、`onBodyFibrillationStart/End`（心室颤动开始/结束）、
    `onBodyBreathChange`（呼吸停止/恢复）。
  - 意识状态：`onBodyConsciousnessChange`（昏迷/苏醒）、`onBodyBrainDying`（进入/离开濒死）。
  - 行为动作：`onBodyClimbStart/End`（攀爬）、`onBodyExerciseStart/End`（锻炼）、`onBodySwitchHands`（切换手持）、
    `onBodySwitchDir`（切换朝向）、`onBodyCrouchChange`（下蹲）、`onBodyPickUp`（拾取）、`onBodyDrop`（丢弃）。
  - 睡眠/特殊：`onBodySleepChange`（入睡/醒来）、`onBodyLastStand`（最后坚持）、`onBodyDisfigure`（毁容）、
    `onBodyRemoveEye`（失去眼睛）。
- **小游戏事件（`MinigameEventListener`）**：AED 除颤与包扎小游戏钩子——
  `onAEDMinigameStart/Defibrillate/Fail`、`onBandageMinigameStart/Wrap`。
- **世界物品与实体事件（`WorldEntityEventListener`）**：电池/自动泵/充电器/捕兽夹/生物终端/地面血迹/方块伤害/蓝图/
  已购物品/弹跳蘑菇/建筑破坏钩子——`onBatteryLoad/Unload`、`onAutoPumpActive/Inactive`、`onBatteryRecharge`、
  `onBearTrapTrigger/Release`、`onBioTerminalUse`、`onGroundBlood`、`onBlockDamaged`、`onBlueprintCreate`、
  `onBoughtItemExpire`、`onBounceShroomBounce`、`onBuildingDestroy`。
- **水晶事件（`CrystalEventListener`）**：所有水晶效果（治疗/电击/燃烧/EMP/传送等）与水晶敌人——
  `onCrystalTouch`、`onCrystalHit`、`onCrystalEnemyAttack`、`onCrystalEnemyDeath`。
- **环境事件（`EnvironmentEventListener`）**：洞穴蜘蛛生成器、可攀爬物、电线圈、尸体钩子——
  `onCaveTickSpawn`、`onClimbableRegister`、`onCoilShock`、`onCorpseSeen`、`onCorpseDestroy`。
- **世界对象事件（`WorldObjectEventListener`）**：可损坏物、伤害板条箱、钻探舱、脊背兽长老、PDA、间歇泉、全局暗幕、
  捕抓植物、抓钩钩子——`onDamageableDamaged`、`onDamagingCrateHit`、`onDrillPodRepair/Use`、
  `onThornbackNear/Stage/Death`、`onPdaUse`、`onGeyserRumble/Activate`、`onGlobalDark`、`onGrabberPlantGrab`、
  `onGrapplingHookFire/Hit/Return`。
- **脱臼复位小游戏事件**：`onDislocationMinigameStart`、`onDislocationMinigameSuccess` 并入 `MinigameEvents`。
- **更多小游戏事件**：手摇曲柄（`onHandCrankMinigameStart/Charge/End`）、键盘密码
  （`onKeypadMinigameStart/Success`）、撬锁（`onLockpingMinigameStart/Success/Stuck`）钩子并入 `MinigameEvents`。
- **更多世界对象事件**：`onItemDestroy`（耐久归零销毁）、`onJumpPadBounce`、`onLifepodButtonPress`、
  `onLifepodShowerActivate` 并入 `WorldObjectEvents`。
- **手动除颤小游戏事件**：`onManualDefibMinigameStart/Shock/End` 并入 `MinigameEvents`。
- **更多世界/UI 事件**：`onMedStationHeal`、`onMineTrigger`、`onObserverLastStand/GunSuicide`、`onOpenableUse`、
  `onPlushSqueak`、`onPreRunStart/Load/Tutorial`、`onOpiateOverdose`、`onSelfDestruct`、`onWoundViewToggle`、
  `onCraftPanelToggle` 并入 `WorldObjectEvents`。
- **取弹片与注射小游戏事件**：`onShrapnelMinigameStart/Success/Fail`、`onSyringeMinigameStart/Inject/Fail`
  并入 `MinigameEvents`。
- **系统事件（`SystemEventListener`）**：`onMindwipe`、`onRadiationStart`、`onGameSave`、`onSkillLevelUp`、
  `onTraderMeet/Haggle/Death`、`onTurretShoot/Explode`、`onWorldRegenerate`、`onSawbladeHit`、
  `onSoundCannonShoot` 并入 `SystemEvents`。
- **截肢小游戏事件**：`onAmputationMinigameStart/Success` 并入 `MinigameEvents`。
- **弹药与 Alt 标签事件**：`onAmmoUnload/Load`、`onAltHoverToggle` 并入 `WorldObjectEvents`。

### 重构
- **统一事件文件结构**：将散落的事件类按主题合并到一个文件，消除"一事件一文件/多事件一文件"混用。Gun（6）、Limb（6）、
  Player（3）、杂项（命令/主菜单/世界就绪，3）各合并到 `Events/GunEvents.cs`、`LimbEvents.cs`、`PlayerEvents.cs`、
  `MiscEvents.cs`。所有事件类仍位于 `Bark.Events` 命名空间，监听器与脚本钩子名不受影响。

### 修复
- **客户端→主机请求始终无法送达**：解决了多个问题：
  - `Net.CreateWriter` 有两个单参数重载（`in Enum` 与 `ushort`）；反射现在选择 `ushort` 版本。
  - `Client_Send`/`Server_SendToClients` 使用 `in`（byref）参数；`DeliveryMethod` 枚举现先去引用再解析。
  - KrokMP 的 `Client_Send` 在 `NetPlayer.LOCAL_PLAYER` 为 null 时会触发 `SENDING A PACKET TOO EARLY` 并断开连接，
    因此客户端现在会等待本地玩家创建后再发送同步请求。
  - 刚加入时连接栈可能尚未就绪；同步请求现带延迟并自动重试。
  - Bark 原先只在 `Awake` 注册一次消息接收器；KrokMP 的 `ShutdownReset` 会清空它们，因此现在每次发送前都会
    幂等重新注册接收器。
  - `HostModFetcher` 原先仅在初始化时已处于服务端/主机才注册 fetch handler；现在无条件注册。
  - 文件末尾的空分片被误判为错误，导致部分文件的最后一个分片从未写入；现在视为正常结束。
- **中途加入的模组无法同步**：增量同步原先只处理客户端已加载的模组。客户端现在对未加载的模组上报 `null`，
  主机则返回该模组的全部文件以便全量下载。
