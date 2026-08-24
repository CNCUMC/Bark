***English*** | [简体中文](../zh-CN/script-events.md)

# Script Event Hooks

Bark provides built-in event hooks. When the corresponding event occurs in-game, Bark automatically calls the matching
global function in your script, passing an `event` object with relevant data.

## How to Use

Define a global function with the exact hook name. Bark calls it with an `event` object.

```js
function onPlayerJumpStart(event) {
    Log.Info('Player jumped');
}
```

If you don't need the event data, you can omit the parameter — JavaScript and Lua ignore extra arguments:

```js
function onPlayerJumpStart() {
    Log.Info('Player jumped');  // fine too
}
```

That's it. No registration, no imports required.

## Hook Reference

### Player Events

| Hook Function       | Trigger           | event Fields |
|---------------------|-------------------|--------------|
| `onPlayerJumpStart` | Jump key pressed  | —            |
| `onPlayerJumpOver`  | Landed after jump | —            |
| `onPlayerDeath`     | Player died       | —            |

```js
function onPlayerDeath(event) {
    Log.Warning('Player died');
    // auto-save on death, etc.
}
```

### Body Events

Changes to the player's vitals, consciousness, actions, sleep, and special states. Every body event carries
`event.Body` (C# Body instance) and `event.Camera` (PlayerCamera).

#### Vitals / Critical

| Hook Function             | Trigger                                            | Extra Fields                   |
|---------------------------|----------------------------------------------------|--------------------------------|
| `onBodyCardiacArrest`     | Cardiac arrest / heartbeat restored (heartRate<20) | `event.IsCardiacArrest` (bool) |
| `onBodyFibrillationStart` | Fibrillation started                               | —                              |
| `onBodyFibrillationEnd`   | Fibrillation stopped                               | —                              |
| `onBodyBreathChange`      | Breathing stopped / restored                       | `event.IsBreathing` (bool)     |

```js
function onBodyCardiacArrest(event) {
    if (event.IsCardiacArrest) {
        Log.Warning('Cardiac arrest! Start CPR!');
    } else {
        Log.Info('Heartbeat restored');
    }
}
```

#### Consciousness

| Hook Function               | Trigger                        | Extra Fields                |
|-----------------------------|--------------------------------|-----------------------------|
| `onBodyConsciousnessChange` | Unconscious / awake            | `event.IsConscious` (bool)  |
| `onBodyBrainDying`          | Entering / leaving brain-death | `event.IsBrainDying` (bool) |

```js
function onBodyConsciousnessChange(event) {
    if (event.IsConscious) {
        Log.Info('Player woke up');
    } else {
        Log.Warning('Player passed out');
    }
}
```

#### Actions

| Hook Function         | Trigger                     | Extra Fields                 |
|-----------------------|-----------------------------|------------------------------|
| `onBodyClimbStart`    | Started climbing            | —                            |
| `onBodyClimbEnd`      | Stopped climbing            | —                            |
| `onBodyExerciseStart` | Started exercising          | —                            |
| `onBodyExerciseEnd`   | Stopped exercising          | —                            |
| `onBodySwitchHands`   | Swapped hand items          | —                            |
| `onBodySwitchDir`     | Switched facing             | `event.IsRight` (bool)       |
| `onBodyCrouchChange`  | Started / stopped crouching | `event.IsCrouching` (bool)   |
| `onBodyPickUp`        | Picked up an item           | `event.ItemId`, `event.Slot` |
| `onBodyDrop`          | Dropped an item             | `event.ItemId`               |

```js
function onBodyPickUp(event) {
    Log.Info('Picked up ' + event.ItemId + ' (slot ' + event.Slot + ')');
}

function onBodySwitchHands(event) {
    Log.Info('Swapped hand items');
}
```

#### Sleep / Special States

| Hook Function       | Trigger               | Extra Fields                |
|---------------------|-----------------------|-----------------------------|
| `onBodySleepChange` | Fell asleep / woke up | `event.IsSleeping` (bool)   |
| `onBodyLastStand`   | Last Stand succeeded  | —                           |
| `onBodyDisfigure`   | Player was disfigured | —                           |
| `onBodyRemoveEye`   | Player lost an eye    | `event.BothEyesGone` (bool) |

```js
function onBodySleepChange(event) {
    if (event.IsSleeping) {
        Log.Info('Player fell asleep');
    } else {
        Log.Info('Player woke up');
    }
}
```

### Limb Events

Six hooks cover all limb status changes: fracture, dislocation, infection, dismemberment.

| Hook Function        | Trigger          | event Fields |
|----------------------|------------------|--------------|
| `onLimbBroken`       | Bone fractured   | —            |
| `onLimbMended`       | Bone healed      | —            |
| `onLimbDislocated`   | Joint dislocated | —            |
| `onLimbUnDislocated` | Joint relocated  | —            |
| `onLimbDismembered`  | Limb severed     | —            |
| `onLimbInfected`     | Wound infected   | —            |

> ℹ️ Limb hooks carry no dedicated event fields. To find which limb was affected, iterate with `Limb` inside the
> hook. e.g., `Limb.IsBroken(0)` checks if limb #0 is broken.

```js
function onLimbBroken(event) {
    // Iterate all limbs to find the broken one
    var count = Limb.GetLimbCount();
    var brokenList = [];
    for (var i = 0; i < count; i++) {
        if (Limb.IsBroken(i)) {
            brokenList.push(i);
        }
    }
    Log.Info('Broken limb indices: ' + brokenList.join(', '));
}
```

### Item Events

Global hooks for item use, hand-use, equip, unequip, limb use, and attack.

All item events pass an `event` object with these fields:

| Field          | Type     | Description                  |
|----------------|----------|------------------------------|
| `event.ItemId` | `string` | The item ID (e.g. `"arrow"`) |
| `event.Item`   | `Item`   | The C# Item instance         |

`onItemLimbUse` additionally provides:

| Field             | Type     | Description                   |
|-------------------|----------|-------------------------------|
| `event.LimbIndex` | `int`    | Target limb index, -1 unknown |
| `event.LimbName`  | `string` | Target limb name              |

| Hook Function   | Trigger                            |
|-----------------|------------------------------------|
| `onItemUse`     | Player used an item from inventory |
| `onItemHandUse` | Player used an item held in hand   |
| `onItemEquip`   | Item was equipped                  |
| `onItemUnequip` | Item was unequipped                |
| `onItemLimbUse` | Item was used on a limb            |
| `onItemAttack`  | Melee attack with an item          |

```js
function onItemUse(event) {
    Log.Info('Item used: ' + event.ItemId);
    // event.Item gives you the C# Item instance for advanced access
}

function onItemLimbUse(event) {
    Log.Info(event.ItemId + ' used on limb ' + event.LimbName);
}

function onItemAttack(event) {
    Log.Info('Attacked with ' + event.ItemId);
}
```

### Moodle Events

Lifecycle events for the custom Moodle system.

All Moodle events carry an `event` object with these fields (varies by event type):

| Field               | Type       | Description                              | Applicable Events             |
|---------------------|------------|------------------------------------------|-------------------------------|
| `event.MoodleKey`   | `string`   | Unique Moodle identifier                 | `onMoodleGet`, `onMoodleLose` |
| `event.MoodleName`  | `string`   | Moodle display name                      | `onMoodleGet`, `onMoodleLose` |
| `event.Intensity`   | `int`      | Moodle intensity                         | `onMoodleGet`                 |
| `event.Critical`    | `bool`     | Whether it's critical                    | `onMoodleGet`                 |
| `event.HoldSeconds` | `float`    | Duration in seconds                      | `onMoodleGet`                 |
| `event.ActiveKeys`  | `string[]` | List of all currently active Moodle keys | `onMoodleIterate`             |

| Hook Function     | Trigger                         |
|-------------------|---------------------------------|
| `onMoodleGet`     | Moodle is applied to the player |
| `onMoodleIterate` | Polled (every 0.5 seconds)      |
| `onMoodleLose`    | Moodle expires or is removed    |

```js
function onMoodleGet(event) {
    Log.Info('Moodle gained: ' + event.MoodleKey);

    if (event.Critical) {
        Player.Alert('Critical status: ' + event.MoodleName, true);
    }
}

function onMoodleIterate(event) {
    // Fires every 0.5s with all active statuses
    Log.Debug('Active statuses: ' + event.ActiveKeys.join(', '));
}

function onMoodleLose(event) {
    Log.Info('Moodle lost: ' + event.MoodleKey);
}
```

### Gun Events

Six hooks cover the full firearm lifecycle: fire, rack, safety, load, unload, and jam. All gun events carry the
`event.GunItem` field, which returns the C# Item instance of the firearm.

| Field                  | Type     | Description                                                  | Applicable Events   |
|------------------------|----------|--------------------------------------------------------------|---------------------|
| `event.GunItem`        | `Item`   | C# Item instance of the firearm                              | All                 |
| `event.Suicide`        | `bool`   | Whether this is a suicide shot (gun pointed at self)         | `onGunFire`         |
| `event.Racked`         | `bool`   | Rack state after toggle (true = racked/open, false = closed) | `onGunRack`         |
| `event.Safe`           | `bool`   | Safety state (true = safety on, false = safety off)          | `onGunSafetyToggle` |
| `event.AmmoItemId`     | `string` | ID of the loaded ammo or magazine                            | `onGunLoadAmmo`     |
| `event.Rounds`         | `int`    | Number of rounds loaded                                      | `onGunLoadAmmo`     |
| `event.RoundsUnloaded` | `int`    | Number of rounds unloaded                                    | `onGunUnload`       |

| Hook Function       | Trigger                                                       |
|---------------------|---------------------------------------------------------------|
| `onGunFire`         | Gun fired (Fire() called)                                     |
| `onGunRack`         | Bolt racked / returned (TryRack() called)                     |
| `onGunSafetyToggle` | Safety toggled (ToggleSafety() called)                        |
| `onGunLoadAmmo`     | Ammo loaded (fires after LoadMag() successfully loads rounds) |
| `onGunUnload`       | Magazine unloaded (fires when UnloadMag() drops a loaded mag) |
| `onGunJam`          | Gun jammed (polling detects failure to extract or chamber)    |

```js
function onGunFire(event) {
    var itemId = event.GunItem.id;
    if (event.Suicide) {
        Log.Warning('Player committed suicide with ' + itemId + '!');
        Player.Alert('It all fades to silence...', true);
        return;
    }
    Log.Info('Fired: ' + itemId);
}

function onGunLoadAmmo(event) {
    Log.Info(event.GunItem.id + ' loaded ' + event.Rounds + ' rounds of ' + event.AmmoItemId);
}

function onGunRack(event) {
    var action = event.Racked ? 'racked' : 'returned';
    Log.Debug(event.GunItem.id + ' bolt ' + action);
}

function onGunSafetyToggle(event) {
    var state = event.Safe ? 'safety on' : 'safety off';
    Log.Debug(event.GunItem.id + ' ' + state);
}

function onGunUnload(event) {
    Log.Info(event.GunItem.id + ' magazine removed, ' + event.RoundsUnloaded + ' rounds');
}

function onGunJam(event) {
    Log.Warning(event.GunItem.id + ' jammed!');
    Player.Alert('Gun jammed! Clear it!', true);
}
```

> ℹ️ `onGunJam` detects jams by polling `GunScript` state every 0.2 seconds rather than via a Harmony patch.
> Detection logic: racking fails to eject the round in chamber → jam; bolt return fails to chamber a new round
> despite rounds in magazine → jam.

### World / Menu Events

| Hook Function      | Trigger                                         | event Fields |
|--------------------|-------------------------------------------------|--------------|
| `onMainMenuLoaded` | Entered main menu                               | —            |
| `onWorldGenerated` | World finished generating, safe to access world | —            |

```js
function onWorldGenerated(event) {
    Log.Info('World ready, mod initialized');
    // Do world-dependent operations here
}

function onMainMenuLoaded(event) {
    Log.Info('Returned to main menu');
}
```

> ⚠️ `onWorldGenerated` is the first moment you can safely call `World`. Before this (including `onLoad`), the world
> doesn't exist and calling World will error.

### Minigame Events

Fired when the player performs defibrillator / bandage minigames.

| Hook Function                  | Trigger                                 | event fields                          |
|--------------------------------|-----------------------------------------|---------------------------------------|
| `onAEDMinigameStart`           | AED minigame started                    | `event.Limb`, `event.LimbIndex`       |
| `onAEDMinigameDefibrillate`    | AED defibrillation succeeded (shock)    | `event.Limb`, `event.WasFibrillating` |
| `onAEDMinigameFail`            | AED analysis failed                     | `event.Limb`                          |
| `onBandageMinigameStart`       | Bandage minigame started                | `event.Limb`, `event.BandageAngle`    |
| `onBandageMinigameWrap`        | One full bandage wrap completed         | `event.Limb`                          |
| `onDislocationMinigameStart`   | Dislocation reset minigame started      | `event.Limb`, `event.HasWrench`       |
| `onDislocationMinigameSuccess` | Limb dislocated reset successfully      | `event.Limb`                          |
| `onHandCrankMinigameStart`     | Hand crank minigame started             | —                                     |
| `onHandCrankMinigameCharge`    | Crank rotated, charging the device      | `event.Angle`                         |
| `onHandCrankMinigameEnd`       | Minigame ended (stamina exhausted)      | —                                     |
| `onKeypadMinigameStart`        | Keypad minigame started                 | `event.ToDestroy`                     |
| `onKeypadMinigameSuccess`      | Correct code, target building destroyed | `event.ToDestroy`                     |
| `onLockpingMinigameStart`      | Lockpick minigame started               | `event.ToDestroy`, `event.HasPick`    |
| `onLockpingMinigameSuccess`    | Lock picked successfully                | `event.ToDestroy`                     |
| `onLockpingMinigameStuck`      | Lockpick stuck (tool/fingers damaged)   | `event.ToDestroy`                     |
| `onManualDefibMinigameStart`   | Manual defib minigame started           | `event.Limb`, `event.OnTorso`         |
| `onManualDefibMinigameShock`   | Manual defib discharged                 | `event.Limb`, `event.Charge`          |
| `onManualDefibMinigameEnd`     | Minigame ended (battery exhausted)      | `event.Limb`                          |
| `onShrapnelMinigameStart`      | Shrapnel removal minigame started       | `event.Limb`, `event.HasTweezers`     |
| `onShrapnelMinigameSuccess`    | All shrapnel removed                    | `event.Limb`                          |
| `onShrapnelMinigameFail`       | Crushed shrapnel, wound deepened        | `event.Limb`                          |
| `onSyringeMinigameStart`       | Syringe minigame started                | `event.Limb`                          |
| `onSyringeMinigameInject`      | Syringe pushed in medicine              | `event.Limb`                          |
| `onSyringeMinigameFail`        | Injection off-target (crushed shrapnel) | `event.Limb`                          |
| `onAmputationMinigameStart`    | Amputation minigame started             | `event.Limb`                          |
| `onAmputationMinigameSuccess`  | Limb severed                            | `event.Limb`                          |

```js
function onAEDMinigameDefibrillate(event) {
    if (event.WasFibrillating) {
        Log.Info('Defibrillated, fibrillation stopped!');
    }
}
```

### World Item & Entity Events

Fired when the player interacts with items/entities or their state changes in the world.

| Hook Function          | Trigger                                  | event fields                                         |
|------------------------|------------------------------------------|------------------------------------------------------|
| `onBatteryLoad`        | Battery inserted into a device           | `event.Device`, `event.Battery`, `event.BatteryType` |
| `onBatteryUnload`      | Battery removed from a device            | `event.Device`, `event.BatteryType`                  |
| `onAutoPumpActive`     | AutoPump started boosting blood pressure | `event.Item`                                         |
| `onAutoPumpInactive`   | AutoPump stopped                         | `event.Item`                                         |
| `onBatteryRecharge`    | Battery placed in a recharger            | `event.Charger`                                      |
| `onBearTrapTrigger`    | Bear trap caught a limb                  | `event.Trap`, `event.Limb`                           |
| `onBearTrapRelease`    | Bear trap released                       | `event.Trap`                                         |
| `onBioTerminalUse`     | Bio terminal used                        | `event.Terminal`, `event.Success`                    |
| `onGroundBlood`        | Blood particle formed a ground stain     | `event.Position`, `event.Vomit`                      |
| `onBlockDamaged`       | Block damaged / destroyed                | `event.Pos`, `event.Damage`, `event.Destroyed`       |
| `onBlueprintCreate`    | Blueprint spawned with a recipe          | `event.Blueprint`, `event.RecipeIndex`               |
| `onBoughtItemExpire`   | Bought item expired and removed          | `event.Item`                                         |
| `onBounceShroomBounce` | Player bounced off a BounceShroom        | `event.Mushroom`                                     |
| `onBuildingDestroy`    | Building entity fully destroyed          | `event.Building`, `event.BuildingId`                 |

```js
function onBearTrapTrigger(event) {
    Log.Warning('A bear trap caught your limb!');
    Player.Alert('Ouch!', true);
}
```

### Crystal Events

Crystal effects touched / hit, plus crystal enemy attack / death.

| Hook Function          | Trigger                                | event fields                        |
|------------------------|----------------------------------------|-------------------------------------|
| `onCrystalTouch`       | Player / item touched a crystal effect | `event.EffectType`, `event.Crystal` |
| `onCrystalHit`         | Player attacked a crystal effect       | `event.EffectType`, `event.Crystal` |
| `onCrystalEnemyAttack` | Crystal enemy lunged at the player     | `event.Enemy`                       |
| `onCrystalEnemyDeath`  | Crystal enemy was killed               | `event.Enemy`                       |

```js
function onCrystalTouch(event) {
    Log.Info('Touched crystal effect: ' + event.EffectType);
}
```

### Environment Events

State changes of cave tick spawners, climbables, coils, corpses, etc.

| Hook Function         | Trigger                     | event fields                           |
|-----------------------|-----------------------------|----------------------------------------|
| `onCaveTickSpawn`     | Cave tick spawner triggered | `event.Position`                       |
| `onClimbableRegister` | Climbable registered        | `event.Climbable`, `event.TotalLength` |
| `onCoilShock`         | Coil shocked a limb         | `event.Coil`, `event.Limb`             |
| `onCorpseSeen`        | Player first saw a corpse   | `event.Corpse`, `event.AnimalCorpse`   |
| `onCorpseDestroy`     | Player destroyed a corpse   | `event.Corpse`                         |

```js
function onCoilShock(event) {
    Log.Warning('Electrocuted by a coil!');
}
```

### World Object Events

State changes of damageables, damaging crates, drill pods, the Elder Thornback, PDAs, geysers, the global dark, grabber
plants, and grappling hooks.

| Hook Function             | Trigger                                      | event fields                       |
|---------------------------|----------------------------------------------|------------------------------------|
| `onDamageableDamaged`     | Damageable object took damage                | `event.Damageable`, `event.Damage` |
| `onDamagingCrateHit`      | Damaging crate collided                      | `event.Crate`, `event.Type`        |
| `onDrillPodRepair`        | Drill pod repaired with a kit                | `event.Pod`                        |
| `onDrillPodUse`           | Drill pod activated (world rebuild/teleport) | `event.Pod`                        |
| `onThornbackNear`         | Elder Thornback approached the player        | `event.Thornback`                  |
| `onThornbackStage`        | Elder Thornback entered the next stage       | `event.Thornback`, `event.Stage`   |
| `onThornbackDeath`        | Elder Thornback was killed                   | `event.Thornback`                  |
| `onPdaUse`                | PDA used to read a note                      | `event.Pda`, `event.FirstRead`     |
| `onGeyserRumble`          | Geyser started rumbling                      | `event.Geyser`                     |
| `onGeyserActivate`        | Geyser erupted                               | `event.Geyser`                     |
| `onGlobalDark`            | Global dark started darkening the screen     | `event.Darkening`                  |
| `onGrabberPlantGrab`      | Grabber plant grabbed the player's limb      | `event.Plant`                      |
| `onGrapplingHookFire`     | Grappling hook fired                         | `event.Hook`                       |
| `onGrapplingHookHit`      | Grappling hook latched onto a surface        | `event.Hook`                       |
| `onGrapplingHookReturn`   | Grappling hook retracted                     | `event.Hook`                       |
| `onItemDestroy`           | Item destroyed (condition reached zero)      | `event.ItemId`, `event.Item`       |
| `onJumpPadBounce`         | Player bounced off a jump pad                | `event.Pad`                        |
| `onLifepodButtonPress`    | Lifepod button pressed                       | `event.Type`                       |
| `onLifepodShowerActivate` | Lifepod shower activated                     | `event.Shower`                     |
| `onMedStationHeal`        | Entered a med station, healing started       | `event.Station`                    |
| `onMineTrigger`           | Mine triggered                               | `event.Mine`                       |
| `onObserverLastStand`     | Last Stand succeeded (Observer approached)   | `event.Observer`                   |
| `onObserverGunSuicide`    | Gun suicide (Observer approached)            | `event.Observer`                   |
| `onOpenableUse`           | Opened an openable (door/crate)              | `event.Openable`, `event.Mode`     |
| `onPlushSqueak`           | Plush toy squeaked when squeezed             | `event.Plush`                      |
| `onPreRunStart`           | Started a new run                            | —                                  |
| `onPreRunLoad`            | Loaded a save to continue                    | —                                  |
| `onPreRunTutorial`        | Started the tutorial                         | —                                  |
| `onOpiateOverdose`        | Opiate level too high (overdose)             | —                                  |
| `onSelfDestruct`          | Self-destruct sequence triggered             | —                                  |
| `onWoundViewToggle`       | Wound panel opened/closed                    | `event.Open`                       |
| `onCraftPanelToggle`      | Craft panel opened/closed                    | `event.Open`                       |
| `onAmmoUnload`            | A round unloaded from a magazine             | `event.Magazine`                   |
| `onAmmoLoad`              | A round loaded into a magazine               | `event.Magazine`                   |
| `onAltHoverToggle`        | Alt item labels toggled on/off               | `event.Active`                     |

```js
function onThornbackStage(event) {
    Log.Warning('Elder Thornback entered stage ' + event.Stage + '!');
}

function onPdaUse(event) {
    if (event.FirstRead) {
        Log.Info('First read of PDA note, gained XP');
    }
}
```

### System Events

System-level events: mindwipe, radiation line, saving, skill level-ups, traders, turrets, world regeneration, sawblades,
and the sound cannon.

| Hook Function        | Trigger                            | event fields                                          |
|----------------------|------------------------------------|-------------------------------------------------------|
| `onMindwipe`         | Mindwipe triggered                 | —                                                     |
| `onRadiationStart`   | Radiation line began advancing     | —                                                     |
| `onGameSave`         | Game saved                         | —                                                     |
| `onSkillLevelUp`     | A stat leveled up                  | `event.Stat`, `event.OldLevel`, `event.NewLevel`      |
| `onTraderMeet`       | Conversation with a trader started | `event.Trader`, `event.Character`, `event.Reputation` |
| `onTraderHaggle`     | Haggle with a trader               | `event.Trader`, `event.Reputation`                    |
| `onTraderDeath`      | Trader killed                      | `event.Trader`                                        |
| `onTurretShoot`      | Turret fired                       | `event.Turret`                                        |
| `onTurretExplode`    | Turret destroyed and exploded      | `event.Turret`                                        |
| `onWorldRegenerate`  | World regenerated (next layer)     | `event.Twice`                                         |
| `onSawbladeHit`      | Sawblade cut a limb                | `event.Sawblade`                                      |
| `onSoundCannonShoot` | Sound cannon fired                 | `event.Cannon`                                        |

```js
function onSkillLevelUp(event) {
    var statName = ['Strength', 'Resilience', 'Intelligence'][event.Stat];
    Log.Info(statName + ' leveled up to ' + event.NewLevel);
}

function onWorldRegenerate(event) {
    Log.Info(event.Twice ? 'Skipped two layers!' : 'Descended to the next layer');
}
```

### Command Event

Fires when the player enters a custom command registered by a script mod. Commands are defined via `Command/*.json`.
See [Script Commands](script-mod/command.md) for details.

| Field               | Type       | Description                                                               |
|---------------------|------------|---------------------------------------------------------------------------|
| `event.CommandName` | `string`   | Triggered command name (without arguments)                                |
| `event.Args`        | `string[]` | All input tokens (`args[0]` = command name, `args[1..]` = user arguments) |

| Hook Function | Trigger                                    |
|---------------|--------------------------------------------|
| `onCommand`   | Player entered a registered script command |

```js
function onCommand(event) {
    Log.Info('Command: ' + event.CommandName);
    Log.Info('Args: ' + event.Args.join(', '));
}
```

## Item Scripts

In addition to global hooks, you can attach scripts to specific items via JSON. When that item triggers an action (use,
attack, equip, etc.), Bark executes the script and calls its `main()` function with arguments.

See [Custom Items](script-mod/item.md) for setup. The script side looks like this:

```js
// arrow.js — registered in arrow.json under "attack"
function main(itemId, item, action) {
    // itemId: "arrow"
    // item:    C# Item instance
    // action:  "attack" / "use" / "equip" / "unequip" / "use_in_hand" / "use_on_limb"
    Item.Destroy(itemId);
    Player.Alert('Bullseye!', true);
}
```

The `main` function accepts 0 to 3 parameters — JavaScript and Lua auto-ignore extras. Common patterns:

```js
function main(itemId)           { /* only need the ID */ }
function main(itemId, item)     { /* need the item object too */ }
function main(itemId, item, action) { /* full context */ }
```

Backward compatibility: old-style top-level `__barkItemId` global still works, but `main()` is the recommended way.

## Full Example

A mod that tracks all injury events:

```js
// Initialize counters in onLoad
var injuredCount = 0;
var brokenCount = 0;

function onLoad() {
    Log.Info('Injury tracker mod loaded');
}

function onLimbBroken(event) {
    brokenCount++;
    injuredCount++;
    Log.Warning('Fracture! Total fractures: ' + brokenCount + ', injuries: ' + injuredCount);
    Player.Alert('Another bone broken...', true);
}

function onLimbInfected(event) {
    injuredCount++;
    Log.Warning('Infection! Total injuries: ' + injuredCount);
}

function onPlayerDeath(event) {
    Log.Warning('Player died. Session stats: fractures ' + brokenCount + ', injuries ' + injuredCount);
    injuredCount = 0;
    brokenCount = 0;
}
```

## Notes

- Hook names are case-sensitive and must match exactly
- Hooks receive an `event` object — item events provide `event.ItemId` and `event.Item`
- Keep hook code fast — don't block. Use `setInterval` / `setTimeout` for heavy work
- Hook frequency varies — `onLimbBroken` may fire multiple times (multiple limbs breaking at once), so make your
  handlers idempotent
- Multiple mods can define the same hook — they don't interfere
