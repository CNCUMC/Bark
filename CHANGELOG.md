# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v2.4.0

### Added

- **Custom KrokMP network layer (`BarkKrokBridge`)**: Bark now talks to the KrokoshaCasualtiesMP (KrokMP 4.0.1) network
  stack directly via reflection, bypassing CUCoreLib's `MultiplayerBridge` (which fails to resolve `Server_SendTo`'s
  `knetid` parameter type and is permanently unavailable). This fixes multiplayer script mod syncing between host and
  clients.
- **Incremental file sync on host reload (`ScriptFileSync`)**: Running `sr` on the host now broadcasts a file sync to
  all connected clients. Clients report per-file hashes; the host compares against its own files (host is authoritative)
  and only pushes files whose hash differs, so unmodified files are not re-transferred. Newly added mods (joined
  mid-session)
  are fully pushed so clients can load them.
- **Body events (`BodyEventListener`)**: Added a set of events based on player body state and actions, all bridged to
  script hooks:
    - Vitals / critical: `onBodyCardiacArrest`, `onBodyFibrillationStart/End`, `onBodyBreathChange`.
    - Consciousness: `onBodyConsciousnessChange`, `onBodyBrainDying`.
    - Actions: `onBodyClimbStart/End`, `onBodyExerciseStart/End`, `onBodySwitchHands`, `onBodySwitchDir`,
      `onBodyCrouchChange`, `onBodyPickUp`, `onBodyDrop`.
    - Sleep / special: `onBodySleepChange`, `onBodyLastStand`, `onBodyDisfigure`, `onBodyRemoveEye`.
- **Minigame events (`MinigameEventListener`)**: AED defibrillator and bandage minigame hooks —
  `onAEDMinigameStart/Defibrillate/Fail`, `onBandageMinigameStart/Wrap`.
- **World item & entity events (`WorldEntityEventListener`)**: Battery/AutoPump/recharger/bear trap/bio terminal/ground
  blood/block damage/blueprint/bought item/bounce shroom/building hooks — `onBatteryLoad/Unload`,
  `onAutoPumpActive/Inactive`, `onBatteryRecharge`, `onBearTrapTrigger/Release`, `onBioTerminalUse`, `onGroundBlood`,
  `onBlockDamaged`, `onBlueprintCreate`, `onBoughtItemExpire`, `onBounceShroomBounce`, `onBuildingDestroy`.
- **Crystal events (`CrystalEventListener`)**: All crystal effects (healing/electric/burning/EMP/teleport/etc.) and the
  crystal enemy — `onCrystalTouch`, `onCrystalHit`, `onCrystalEnemyAttack`, `onCrystalEnemyDeath`.
- **Environment events (`EnvironmentEventListener`)**: Cave tick spawner, climbable, coil, and corpse hooks —
  `onCaveTickSpawn`, `onClimbableRegister`, `onCoilShock`, `onCorpseSeen`, `onCorpseDestroy`.
- **World object events (`WorldObjectEventListener`)**: Damageable, damaging crate, drill pod, Elder Thornback, PDA,
  geyser, global dark, grabber plant, and grappling hook hooks — `onDamageableDamaged`, `onDamagingCrateHit`,
  `onDrillPodRepair/Use`, `onThornbackNear/Stage/Death`, `onPdaUse`, `onGeyserRumble/Activate`, `onGlobalDark`,
  `onGrabberPlantGrab`, `onGrapplingHookFire/Hit/Return`.
- **Dislocation minigame events**: `onDislocationMinigameStart`, `onDislocationMinigameSuccess` added to
  `MinigameEvents`.
- **More minigame events**: Hand crank (`onHandCrankMinigameStart/Charge/End`), keypad
  (`onKeypadMinigameStart/Success`), and lockpick (`onLockpingMinigameStart/Success/Stuck`) hooks added to
  `MinigameEvents`.
- **More world object events**: `onItemDestroy` (durability zero), `onJumpPadBounce`, `onLifepodButtonPress`,
  `onLifepodShowerActivate` added to `WorldObjectEvents`.
- **Manual defib minigame events**: `onManualDefibMinigameStart/Shock/End` added to `MinigameEvents`.
- **More world/UI events**: `onMedStationHeal`, `onMineTrigger`, `onObserverLastStand/GunSuicide`, `onOpenableUse`,
  `onPlushSqueak`, `onPreRunStart/Load/Tutorial`, `onOpiateOverdose`, `onSelfDestruct`,
  `onWoundViewToggle`, `onCraftPanelToggle` added to `WorldObjectEvents`.
- **Shrapnel & syringe minigame events**: `onShrapnelMinigameStart/Success/Fail`,
  `onSyringeMinigameStart/Inject/Fail` added to `MinigameEvents`.
- **System events (`SystemEventListener`)**: `onMindwipe`, `onRadiationStart`, `onGameSave`, `onSkillLevelUp`,
  `onTraderMeet/Haggle/Death`, `onTurretShoot/Explode`, `onWorldRegenerate`, `onSawbladeHit`,
  `onSoundCannonShoot` added to `SystemEvents`.
- **Amputation minigame events**: `onAmputationMinigameStart/Success` added to `MinigameEvents`.
- **Ammo & AltHover events**: `onAmmoUnload/Load`, `onAltHoverToggle` added to `WorldObjectEvents`.
- **Plush template (`PlushTemplate`)**: New `"plush"` item template preset from the game's `plushie`. Squeak sound
  reuses Bark's Audio properties via `squeak_sound` (custom mod audio loaded by `AudioManager`), with automatic
  interception of `PlushScript.Squeak()` to play the custom sound.

### Refactor

- **Unified event file structure**: Merged scattered single-event files into topic-based files to remove the
  "one event per file / many events per file" mix. Gun (6), Limb (6), Player (3), and misc
  (command/main-menu/world-ready,
    3) events were consolidated into `Events/GunEvents.cs`, `LimbEvents.cs`, `PlayerEvents.cs`, and `MiscEvents.cs`. All
       event classes remain in the `Bark.Events` namespace, so listeners and script hook names are unaffected.

### Fixed

- **Client→host request never reached the host**: Several issues were resolved:
    - `Net.CreateWriter` has two one-argument overloads (`in Enum` and `ushort`); reflection now picks the `ushort` one.
    - `Client_Send`/`Server_SendToClients` use `in` (byref) parameters; the `DeliveryMethod` enum is now resolved after
      dereferencing the byref parameter type.
    - KrokMP's `Client_Send` aborts with `SENDING A PACKET TOO EARLY` and disconnects when `NetPlayer.LOCAL_PLAYER` is
      null, so clients now wait until their local player exists before sending sync requests.
    - The connection stack may not be ready immediately after joining; sync requests now retry with a delay.
    - Bark registered its message receivers only once at `Awake`; KrokMP's `ShutdownReset` clears them, so receivers are
      now re-registered idempotently before each send.
    - `HostModFetcher` used to skip registering its fetch handler unless already a server/host at init time; it now
      registers unconditionally.
    - An empty chunk at the final file boundary was treated as an error, so the last fragment of some files was never
      written; it is now treated as a normal end-of-file.
- **Mid-session mods could not sync**: The incremental sync previously only handled mods the client already had loaded.
  Clients now report `null` for mods they do not have, and the host responds with every file of that mod so it can be
  downloaded in full.
