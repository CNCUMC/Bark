var ALL = 'onAEDMinigameDefibrillate onAEDMinigameFail onAEDMinigameStart onAltHoverToggle onAmmoLoad onAmmoUnload onAmputationMinigameStart onAmputationMinigameSuccess onAutoPumpActive onAutoPumpInactive onBandageMinigameStart onBandageMinigameWrap onBatteryLoad onBatteryRecharge onBatteryUnload onBearTrapRelease onBearTrapTrigger onBioTerminalUse onBlockDamaged onBlueprintCreate onBodyBrainDying onBodyBreathChange onBodyCardiacArrest onBodyClimbEnd onBodyClimbStart onBodyConsciousnessChange onBodyCrouchChange onBodyDisfigure onBodyDrop onBodyExerciseEnd onBodyExerciseStart onBodyFibrillationEnd onBodyFibrillationStart onBodyLastStand onBodyPickUp onBodyRemoveEye onBodySleepChange onBodySwitchDir onBodySwitchHands onBoughtItemExpire onBounceShroomBounce onBuildingDestroy onCaveTickSpawn onClimbableRegister onCoilShock onCommand onCorpseDestroy onCorpseSeen onCraftPanelToggle onCrystalEnemyAttack onCrystalEnemyDeath onCrystalHit onCrystalTouch onDamageableDamaged onDamagingCrateHit onDislocationMinigameBreak onDislocationMinigameStart onDislocationMinigameSuccess onDrillPodRepair onDrillPodUse onGameSave onGeyserActivate onGeyserRumble onGlobalDark onGrabberPlantGrab onGrapplingHookFire onGrapplingHookHit onGrapplingHookReturn onGroundBlood onGunFire onGunJam onGunLoadAmmo onGunRack onGunSafetyToggle onGunUnload onHandCrankMinigameCharge onHandCrankMinigameEnd onHandCrankMinigameStart onItemAttack onItemCapacity onItemCharge onItemDestroy onItemDurability onItemEquip onItemHandUse onItemLimbUse onItemUnequip onItemUse onItemWearAttack onItemWearDamage onJumpPadBounce onKeypadMinigameStart onKeypadMinigameSuccess onLifepodButtonPress onLifepodShowerActivate onLimbBroken onLimbDislocated onLimbDismembered onLimbInfected onLimbMended onLimbUnDislocated onLockpingMinigameStart onLockpingMinigameStuck onLockpingMinigameSuccess onMainMenuLoaded onManualDefibMinigameEnd onManualDefibMinigameShock onManualDefibMinigameStart onMedStationHeal onMindwipe onMineTrigger onMoodleGet onMoodleIterate onMoodleLose onObserverGunSuicide onObserverLastStand onOpenableUse onOpiateOverdose onPdaUse onPlayerDeath onPlayerJumpOver onPlayerJumpStart onPlushSqueak onPreRunLoad onPreRunStart onPreRunTutorial onRadiationStart onSawbladeHit onSelfDestruct onShrapnelMinigameFail onShrapnelMinigameStart onShrapnelMinigameSuccess onSkillLevelUp onSoundCannonShoot onSyringeMinigameFail onSyringeMinigameInject onSyringeMinigameStart onThornbackDeath onThornbackNear onThornbackStage onTileDamaging onTileDestroyed onTileExist onTilePlace onTraderDeath onTraderHaggle onTraderMeet onTurretExplode onTurretShoot onWorldGenerated onWorldRegenerate onWoundViewToggle'.split(' ');

var PROBES = {
    'onTilePlace': ['TileId', 'TileIndex', 'PosX', 'PosY'],
    'onTileExist': ['TileId', 'TileIndex', 'PosX', 'PosY'],
    'onTileDamaging': ['TileId', 'TileIndex', 'PosX', 'PosY'],
    'onTileDestroyed': ['TileId', 'TileIndex', 'PosX', 'PosY'],
    'onCommand': ['CommandName', 'Args'],
    'onSkillLevelUp': ['Stat', 'OldLevel', 'NewLevel'],
    'onTraderMeet': ['Character', 'Reputation'],
    'onTraderHaggle': ['Reputation'],
    'onWorldRegenerate': ['Twice'],
    'onItemUse': ['ItemId'],
    'onItemHandUse': ['ItemId'],
    'onItemEquip': ['ItemId'],
    'onItemUnequip': ['ItemId'],
    'onItemLimbUse': ['ItemId', 'LimbIndex'],
    'onItemAttack': ['ItemId'],
    'onItemWearAttack': ['ItemId'],
    'onItemWearDamage': ['ItemId', 'DamageAmount'],
    'onItemDurability': ['ItemId', 'CurrentValue'],
    'onItemCapacity': ['ItemId', 'CurrentValue'],
    'onItemCharge': ['ItemId', 'CurrentValue'],
    'onBlockDamaged': ['Pos', 'Damage', 'Destroyed'],
    'onBodyPickUp': ['ItemId', 'Slot'],
    'onBodyDrop': ['ItemId'],
    'onBodySwitchDir': ['IsRight'],
    'onBodySleepChange': ['IsSleeping'],
    'onBodyConsciousnessChange': ['IsConscious'],
    'onBodyBreathChange': ['IsBreathing'],
    'onBodyCardiacArrest': ['IsCardiacArrest'],
    'onBodyRemoveEye': ['BothEyesGone'],
    'onBatteryLoad': ['BatteryType'],
    'onGroundBlood': ['Vomit'],
    'onOpenableUse': ['Mode'],
    'onAEDMinigameStart': ['LimbIndex']
};

var GROUP_RULES = [
    ['Minigame', 'Minigame'],
    ['onBody', 'Body'],
    ['onLimb', 'Limb'],
    ['onPlayer', 'Player'],
    ['onItem', 'Item'],
    ['onGun', 'Gun'],
    ['onTile', 'Tile'],
    ['onMoodle', 'Moodle'],
    ['onCrystal', 'Crystal'],
    ['onTrader', 'WorldObject'],
    ['onTurret', 'WorldObject'],
    ['onThornback', 'WorldObject'],
    ['onObserver', 'WorldObject'],
    ['onGrappling', 'WorldObject'],
    ['onGeyser', 'WorldObject'],
    ['onLifepod', 'WorldObject'],
    ['onDrillPod', 'WorldObject'],
    ['onCorpse', 'WorldObject'],
    ['onAmmo', 'WorldObject'],
    ['onBattery', 'WorldEntity'],
    ['onAutoPump', 'WorldEntity'],
    ['onBearTrap', 'WorldEntity'],
    ['onBounce', 'WorldEntity'],
    ['onBlock', 'WorldEntity'],
    ['onBlueprint', 'WorldEntity'],
    ['onBuilding', 'WorldEntity'],
    ['onBioTerminal', 'WorldEntity'],
    ['onBought', 'WorldEntity'],
    ['onGroundBlood', 'WorldEntity'],
    ['onPreRun', 'Lifecycle'],
    ['onCommand', 'Lifecycle'],
    ['onMainMenu', 'Lifecycle'],
    ['onWorld', 'Lifecycle'],
    ['onGameSave', 'Lifecycle'],
    ['onSkillLevelUp', 'Lifecycle'],
    ['onMindwipe', 'Lifecycle'],
    ['onRadiation', 'Lifecycle'],
    ['onClimbableRegister', 'Lifecycle'],
    ['onCaveTickSpawn', 'Lifecycle'],
    ['onGlobalDark', 'Lifecycle']
];

var HITS = {};
var PROBE_FAILS = {};
var API_RESULTS = [];
var FRAMES = 0;
var AUTO_REPORTED = false;

function say(msg) {
    Log.Info(msg);
}

function line() {
    say('============================================================');
}

function groupOf(name) {
    for (var i = 0; i < GROUP_RULES.length; i++)
        if (name.indexOf(GROUP_RULES[i][0]) !== -1) return GROUP_RULES[i][1];
    return 'Other';
}

function record(name, evt) {
    HITS[name] = (HITS[name] || 0) + 1;
    if (HITS[name] !== 1) return;

    var hasPayload = (evt !== null && evt !== undefined);
    say('[EVT] ' + name + (hasPayload ? '' : ' (no payload)'));

    var probes = PROBES[name];
    if (!probes || !hasPayload) return;
    for (var i = 0; i < probes.length; i++) {
        var field = probes[i];
        try {
            var v = evt[field];
            if (v === undefined) PROBE_FAILS[name + '.' + field] = 'undefined';
        } catch (ex) {
            PROBE_FAILS[name + '.' + field] = String(ex);
        }
    }
}

function check(label, fn) {
    try {
        var v = fn();
        API_RESULTS.push('PASS ' + label +
            (v === undefined || v === null ? '' : ' -> ' + String(v)));
    } catch (ex) {
        API_RESULTS.push('FAIL ' + label + ' -> ' + String(ex));
    }
}

function apiSmoke() {
    API_RESULTS = [];
    check('ScriptInfo.Id', function () { return ScriptInfo.Id; });
    check('Storage.Set/GetString', function () {
        Storage.SetString('bark_compat_test', 'pong');
        return Storage.GetString('bark_compat_test', 'missing');
    });
    check('Compress.GZip.roundtrip', function () {
        return Compress.DecompressGZip(Compress.CompressGZip('compat'));
    });
    check('World.GetWidth', function () { return World.GetWidth(); });
    check('World.GetHeight', function () { return World.GetHeight(); });
    check('World.TileAt', function () { return World.TileAt(0, 0); });
    check('World.ToTilePosition', function () {
        var p = World.ToTilePosition(1.5, 2.5);
        return p.x + ',' + p.y;
    });
    check('World.ToRelativePosition', function () {
        var p = World.ToRelativePosition(1.5, 2.5);
        return p.x + ',' + p.y;
    });
    check('Player.GetPosition', function () {
        var p = Player.GetPosition();
        return p.x + ',' + p.y;
    });
    check('Body.IsAlive', function () { return Body.IsAlive(); });
    check('Body.GetHeartRate', function () { return Body.GetHeartRate(); });
    check('Body.GetBloodPressure', function () { return Body.GetBloodPressure(); });
    check('Body.GetHunger', function () { return Body.GetHunger(); });
    check('Limb.GetAveragePain', function () { return Limb.GetAveragePain(); });
    check('Inventory.HasItem', function () { return Inventory.HasItem('__none__'); });
    check('Inventory.GetSlotCount', function () { return Inventory.GetSlotCount(); });
    check('Input.GetMousePosition', function () {
        var p = Input.GetMousePosition();
        return p.x + ',' + p.y;
    });

    var pass = 0;
    for (var i = 0; i < API_RESULTS.length; i++)
        if (API_RESULTS[i].indexOf('PASS') === 0) pass++;
    say('[API] ' + pass + ' PASS / ' + (API_RESULTS.length - pass) + ' FAIL');
    for (var j = 0; j < API_RESULTS.length; j++)
        if (API_RESULTS[j].indexOf('FAIL') === 0) say('  ' + API_RESULTS[j]);
}

function report() {
    var hit = 0;
    var byGroup = {};
    for (var i = 0; i < ALL.length; i++) {
        var n = ALL[i];
        if (HITS[n]) { hit++; continue; }
        var g = groupOf(n);
        if (!byGroup[g]) byGroup[g] = [];
        byGroup[g].push(n);
    }

    line();
    say('[TEST] Bark 正式版兼容性报告');
    say('[TEST] 事件命中 ' + hit + '/' + ALL.length + ' | 未命中 ' + (ALL.length - hit));

    var probeFails = Object.keys(PROBE_FAILS);
    say('[TEST] payload 探针失败 ' + probeFails.length + ' 项');
    for (var a = 0; a < probeFails.length; a++)
        say('  [PROBE] ' + probeFails[a] + ' -> ' + PROBE_FAILS[probeFails[a]]);

    say('[TEST] --- 已命中 ---');
    var hitNames = Object.keys(HITS);
    hitNames.sort();
    for (var b = 0; b < hitNames.length; b++)
        say('  ' + hitNames[b] + ' x' + HITS[hitNames[b]]);

    say('[TEST] --- 未命中 (按组, 需按规划表手动触发复核) ---');
    var groups = Object.keys(byGroup);
    groups.sort();
    for (var c = 0; c < groups.length; c++) {
        var arr = byGroup[groups[c]];
        arr.sort();
        for (var d = 0; d < arr.length; d += 8)
            say('  [' + groups[c] + '] ' + arr.slice(d, d + 8).join(', '));
    }

    say('[TEST] --- 脚本 API 冒烟 ---');
    if (API_RESULTS.length === 0)
        say('  (尚未运行; 进入世界后自动执行, 或输入 barktestapi)');
    else
        for (var e = 0; e < API_RESULTS.length; e++)
            say('  ' + API_RESULTS[e]);
    line();
}

function reset() {
    HITS = {};
    PROBE_FAILS = {};
    AUTO_REPORTED = false;
}

function onLoad() {
    line();
    say('[TEST] 兼容性测试模组已加载 | 事件总数 ' + ALL.length);
    say('[TEST] 命令: barktest / barktestreset / barktestapi / barktestboom');
    line();
}

function onEnable() { say('[LIFE] onEnable'); }
function onDisable() { say('[LIFE] onDisable'); }
function onUnload() { say('[LIFE] onUnload'); }

function onUpdate() {
    FRAMES++;
    if (!AUTO_REPORTED && FRAMES >= 3600) {
        AUTO_REPORTED = true;
        report();
    }
}

function onCommand(e) {
    record('onCommand', e);
    var cmd = '';
    try { cmd = String(e.CommandName || '').toLowerCase(); } catch (ex) { }
    if (cmd === 'barktest') { report(); return; }
    if (cmd === 'barktestreset') { reset(); say('[TEST] 计数已重置'); return; }
    if (cmd === 'barktestapi') {
        apiSmoke();
        say('[TEST] API 冒烟完成, 输入 barktest 查看明细');
        return;
    }
    if (cmd === 'barktestboom') {
        try {
            var p = Player.GetPosition();
            World.Explode(p.x + 3, p.y, 5, 5, 0);
            say('[TEST] World.Explode 调用成功 (range 5 / velocity 5 / 结构伤害 0)');
        } catch (ex) {
            say('[TEST] World.Explode 失败: ' + String(ex));
        }
        return;
    }
    if (cmd === 'barktesttile') {
        try {
            var tp = Player.GetPosition();
            var target = World.ToTilePosition(tp.x + 3, tp.y);
            var before = World.TileAt(target.x, target.y);
            World.PlaceTile(tp.x + 3, tp.y, 'compat_test_block');
            var after = World.TileAt(target.x, target.y);
            say('[TEST] 目标瓦片 (' + target.x + ',' + target.y + ') 放置前='
                + String(before) + ' 放置后=' + String(after));
        } catch (ex) {
            say('[TEST] World.PlaceTile 失败: ' + String(ex));
        }
        return;
    }
}

function onWorldGenerated(e) {
    record('onWorldGenerated', e);
    say('[TEST] 世界已生成, 运行脚本 API 冒烟...');
    apiSmoke();
}

function onMainMenuLoaded(e) {
    record('onMainMenuLoaded', e);
}

function onAEDMinigameDefibrillate(e) { record('onAEDMinigameDefibrillate', e); }
function onAEDMinigameFail(e) { record('onAEDMinigameFail', e); }
function onAEDMinigameStart(e) { record('onAEDMinigameStart', e); }
function onAltHoverToggle(e) { record('onAltHoverToggle', e); }
function onAmmoLoad(e) { record('onAmmoLoad', e); }
function onAmmoUnload(e) { record('onAmmoUnload', e); }
function onAmputationMinigameStart(e) { record('onAmputationMinigameStart', e); }
function onAmputationMinigameSuccess(e) { record('onAmputationMinigameSuccess', e); }
function onAutoPumpActive(e) { record('onAutoPumpActive', e); }
function onAutoPumpInactive(e) { record('onAutoPumpInactive', e); }
function onBandageMinigameStart(e) { record('onBandageMinigameStart', e); }
function onBandageMinigameWrap(e) { record('onBandageMinigameWrap', e); }
function onBatteryLoad(e) { record('onBatteryLoad', e); }
function onBatteryRecharge(e) { record('onBatteryRecharge', e); }
function onBatteryUnload(e) { record('onBatteryUnload', e); }
function onBearTrapRelease(e) { record('onBearTrapRelease', e); }
function onBearTrapTrigger(e) { record('onBearTrapTrigger', e); }
function onBioTerminalUse(e) { record('onBioTerminalUse', e); }
function onBlockDamaged(e) { record('onBlockDamaged', e); }
function onBlueprintCreate(e) { record('onBlueprintCreate', e); }
function onBodyBrainDying(e) { record('onBodyBrainDying', e); }
function onBodyBreathChange(e) { record('onBodyBreathChange', e); }
function onBodyCardiacArrest(e) { record('onBodyCardiacArrest', e); }
function onBodyClimbEnd(e) { record('onBodyClimbEnd', e); }
function onBodyClimbStart(e) { record('onBodyClimbStart', e); }
function onBodyConsciousnessChange(e) { record('onBodyConsciousnessChange', e); }
function onBodyCrouchChange(e) { record('onBodyCrouchChange', e); }
function onBodyDisfigure(e) { record('onBodyDisfigure', e); }
function onBodyDrop(e) { record('onBodyDrop', e); }
function onBodyExerciseEnd(e) { record('onBodyExerciseEnd', e); }
function onBodyExerciseStart(e) { record('onBodyExerciseStart', e); }
function onBodyFibrillationEnd(e) { record('onBodyFibrillationEnd', e); }
function onBodyFibrillationStart(e) { record('onBodyFibrillationStart', e); }
function onBodyLastStand(e) { record('onBodyLastStand', e); }
function onBodyPickUp(e) { record('onBodyPickUp', e); }
function onBodyRemoveEye(e) { record('onBodyRemoveEye', e); }
function onBodySleepChange(e) { record('onBodySleepChange', e); }
function onBodySwitchDir(e) { record('onBodySwitchDir', e); }
function onBodySwitchHands(e) { record('onBodySwitchHands', e); }
function onBoughtItemExpire(e) { record('onBoughtItemExpire', e); }
function onBounceShroomBounce(e) { record('onBounceShroomBounce', e); }
function onBuildingDestroy(e) { record('onBuildingDestroy', e); }
function onCaveTickSpawn(e) { record('onCaveTickSpawn', e); }
function onClimbableRegister(e) { record('onClimbableRegister', e); }
function onCoilShock(e) { record('onCoilShock', e); }
function onCorpseDestroy(e) { record('onCorpseDestroy', e); }
function onCorpseSeen(e) { record('onCorpseSeen', e); }
function onCraftPanelToggle(e) { record('onCraftPanelToggle', e); }
function onCrystalEnemyAttack(e) { record('onCrystalEnemyAttack', e); }
function onCrystalEnemyDeath(e) { record('onCrystalEnemyDeath', e); }
function onCrystalHit(e) { record('onCrystalHit', e); }
function onCrystalTouch(e) { record('onCrystalTouch', e); }
function onDamageableDamaged(e) { record('onDamageableDamaged', e); }
function onDamagingCrateHit(e) { record('onDamagingCrateHit', e); }
function onDislocationMinigameBreak(e) { record('onDislocationMinigameBreak', e); }
function onDislocationMinigameStart(e) { record('onDislocationMinigameStart', e); }
function onDislocationMinigameSuccess(e) { record('onDislocationMinigameSuccess', e); }
function onDrillPodRepair(e) { record('onDrillPodRepair', e); }
function onDrillPodUse(e) { record('onDrillPodUse', e); }
function onGameSave(e) { record('onGameSave', e); }
function onGeyserActivate(e) { record('onGeyserActivate', e); }
function onGeyserRumble(e) { record('onGeyserRumble', e); }
function onGlobalDark(e) { record('onGlobalDark', e); }
function onGrabberPlantGrab(e) { record('onGrabberPlantGrab', e); }
function onGrapplingHookFire(e) { record('onGrapplingHookFire', e); }
function onGrapplingHookHit(e) { record('onGrapplingHookHit', e); }
function onGrapplingHookReturn(e) { record('onGrapplingHookReturn', e); }
function onGroundBlood(e) { record('onGroundBlood', e); }
function onGunFire(e) { record('onGunFire', e); }
function onGunJam(e) { record('onGunJam', e); }
function onGunLoadAmmo(e) { record('onGunLoadAmmo', e); }
function onGunRack(e) { record('onGunRack', e); }
function onGunSafetyToggle(e) { record('onGunSafetyToggle', e); }
function onGunUnload(e) { record('onGunUnload', e); }
function onHandCrankMinigameCharge(e) { record('onHandCrankMinigameCharge', e); }
function onHandCrankMinigameEnd(e) { record('onHandCrankMinigameEnd', e); }
function onHandCrankMinigameStart(e) { record('onHandCrankMinigameStart', e); }
function onItemAttack(e) { record('onItemAttack', e); }
function onItemCapacity(e) { record('onItemCapacity', e); }
function onItemCharge(e) { record('onItemCharge', e); }
function onItemDestroy(e) { record('onItemDestroy', e); }
function onItemDurability(e) { record('onItemDurability', e); }
function onItemEquip(e) { record('onItemEquip', e); }
function onItemHandUse(e) { record('onItemHandUse', e); }
function onItemLimbUse(e) { record('onItemLimbUse', e); }
function onItemUnequip(e) { record('onItemUnequip', e); }
function onItemUse(e) { record('onItemUse', e); }
function onItemWearAttack(e) { record('onItemWearAttack', e); }
function onItemWearDamage(e) { record('onItemWearDamage', e); }
function onJumpPadBounce(e) { record('onJumpPadBounce', e); }
function onKeypadMinigameStart(e) { record('onKeypadMinigameStart', e); }
function onKeypadMinigameSuccess(e) { record('onKeypadMinigameSuccess', e); }
function onLifepodButtonPress(e) { record('onLifepodButtonPress', e); }
function onLifepodShowerActivate(e) { record('onLifepodShowerActivate', e); }
function onLimbBroken(e) { record('onLimbBroken', e); }
function onLimbDislocated(e) { record('onLimbDislocated', e); }
function onLimbDismembered(e) { record('onLimbDismembered', e); }
function onLimbInfected(e) { record('onLimbInfected', e); }
function onLimbMended(e) { record('onLimbMended', e); }
function onLimbUnDislocated(e) { record('onLimbUnDislocated', e); }
function onLockpingMinigameStart(e) { record('onLockpingMinigameStart', e); }
function onLockpingMinigameStuck(e) { record('onLockpingMinigameStuck', e); }
function onLockpingMinigameSuccess(e) { record('onLockpingMinigameSuccess', e); }
function onManualDefibMinigameEnd(e) { record('onManualDefibMinigameEnd', e); }
function onManualDefibMinigameShock(e) { record('onManualDefibMinigameShock', e); }
function onManualDefibMinigameStart(e) { record('onManualDefibMinigameStart', e); }
function onMedStationHeal(e) { record('onMedStationHeal', e); }
function onMindwipe(e) { record('onMindwipe', e); }
function onMineTrigger(e) { record('onMineTrigger', e); }
function onMoodleGet(e) { record('onMoodleGet', e); }
function onMoodleIterate(e) { record('onMoodleIterate', e); }
function onMoodleLose(e) { record('onMoodleLose', e); }
function onObserverGunSuicide(e) { record('onObserverGunSuicide', e); }
function onObserverLastStand(e) { record('onObserverLastStand', e); }
function onOpenableUse(e) { record('onOpenableUse', e); }
function onOpiateOverdose(e) { record('onOpiateOverdose', e); }
function onPdaUse(e) { record('onPdaUse', e); }
function onPlayerDeath(e) { record('onPlayerDeath', e); }
function onPlayerJumpOver(e) { record('onPlayerJumpOver', e); }
function onPlayerJumpStart(e) { record('onPlayerJumpStart', e); }
function onPlushSqueak(e) { record('onPlushSqueak', e); }
function onPreRunLoad(e) { record('onPreRunLoad', e); }
function onPreRunStart(e) { record('onPreRunStart', e); }
function onPreRunTutorial(e) { record('onPreRunTutorial', e); }
function onRadiationStart(e) { record('onRadiationStart', e); }
function onSawbladeHit(e) { record('onSawbladeHit', e); }
function onSelfDestruct(e) { record('onSelfDestruct', e); }
function onShrapnelMinigameFail(e) { record('onShrapnelMinigameFail', e); }
function onShrapnelMinigameStart(e) { record('onShrapnelMinigameStart', e); }
function onShrapnelMinigameSuccess(e) { record('onShrapnelMinigameSuccess', e); }
function onSkillLevelUp(e) { record('onSkillLevelUp', e); }
function onSoundCannonShoot(e) { record('onSoundCannonShoot', e); }
function onSyringeMinigameFail(e) { record('onSyringeMinigameFail', e); }
function onSyringeMinigameInject(e) { record('onSyringeMinigameInject', e); }
function onSyringeMinigameStart(e) { record('onSyringeMinigameStart', e); }
function onThornbackDeath(e) { record('onThornbackDeath', e); }
function onThornbackNear(e) { record('onThornbackNear', e); }
function onThornbackStage(e) { record('onThornbackStage', e); }
function onTileDamaging(e) { record('onTileDamaging', e); }
function onTileDestroyed(e) { record('onTileDestroyed', e); }
function onTileExist(e) { record('onTileExist', e); }
function onTilePlace(e) { record('onTilePlace', e); }
function onTraderDeath(e) { record('onTraderDeath', e); }
function onTraderHaggle(e) { record('onTraderHaggle', e); }
function onTraderMeet(e) { record('onTraderMeet', e); }
function onTurretExplode(e) { record('onTurretExplode', e); }
function onTurretShoot(e) { record('onTurretShoot', e); }
function onWorldRegenerate(e) { record('onWorldRegenerate', e); }
function onWoundViewToggle(e) { record('onWoundViewToggle', e); }
