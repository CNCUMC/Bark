***English*** | [简体中文](../../zh-CN/script-api/limbs.md)

# LimbUtil — Limb Operations

LimbUtil operates on individual limbs: query status, read values, apply damage, heal. All single-limb methods accept an
`int index` parameter.

## Limb Indexes

Limbs are accessed by zero-based index.

```js
var count = LimbUtil.GetLimbCount();  // total limb count
var name = LimbUtil.GetLimbName(0);   // full name of limb #0
var short = LimbUtil.GetLimbShortName(0); // short name
```

## Status Queries

`Is*` / `Has*` boolean queries with a limb index.

```js
if (LimbUtil.IsBroken(0)) {
    Log.Info('Limb #0 is fractured');
}
if (LimbUtil.IsDislocated(1)) {
    Log.Info('Limb #1 is dislocated');
}
if (LimbUtil.IsInfected(2)) {
    Log.Info('Limb #2 is infected');
}
if (LimbUtil.IsDismembered(3)) {
    Log.Info('Limb #3 is severed');
}
if (LimbUtil.IsSplinted(0)) {
    Log.Info('Limb #0 is splinted');
}
```

| Method                     | Description      |
|----------------------------|------------------|
| `IsBroken(index)`          | Fractured        |
| `IsDislocated(index)`      | Dislocated       |
| `IsInfected(index)`        | Infected         |
| `IsDismembered(index)`     | Severed          |
| `IsSplinted(index)`        | Has splint       |
| `IsVital(index)`           | Vital body part  |
| `IsHead(index)`            | Is head          |
| `IsAbdomen(index)`         | Is abdomen       |
| `IsArm(index)`             | Is arm           |
| `IsLeg(index)`             | Is leg           |
| `HasShrapnel(index)`       | Has shrapnel     |
| `IsBlockedBleeding(index)` | Bleeding stopped |

## Value Queries

```js
var skin = LimbUtil.GetSkinHealth(0);           // skin health 0-100
var muscle = LimbUtil.GetMuscleHealth(0);       // muscle health 0-100
var pain = LimbUtil.GetPain(0);                 // pain 0-100
var bleed = LimbUtil.GetBleedAmount(0);         // current bleed
var totalBleed = LimbUtil.GetTotalBleedAmount(0); // total blood lost
var infection = LimbUtil.GetInfectionAmount(0); // infection level
var shrapnel = LimbUtil.GetShrapnelCount(0);    // shrapnel count
```

## Modification Operations

Naming: `Set*` = absolute value, `Damage*` = relative decrease, `HealLimb` = one-click restore.

```js
// Set absolute values
LimbUtil.SetSkinHealth(0, 80);     // restore skin to 80
LimbUtil.SetMuscleHealth(1, 100);  // max muscle
LimbUtil.SetPain(2, 0);            // pain relief
LimbUtil.SetBleed(0, 0);           // stop bleeding
LimbUtil.SetInfection(1, 0);       // clear infection
LimbUtil.SetShrapnel(3, 0);        // remove shrapnel

// Apply damage (relative)
LimbUtil.DamageSkin(0, 20);        // skin -20
LimbUtil.DamageMuscle(1, 30);      // muscle -30

// Special operations
LimbUtil.BreakBone(0);             // fracture
LimbUtil.MendBone(0);              // set bone
LimbUtil.DislocateLimb(1);         // dislocate
LimbUtil.UnDislocateLimb(1);       // relocate
LimbUtil.SetBlockedBleeding(2, true);  // apply tourniquet
LimbUtil.SetDisinfect(0, 60);      // disinfect for 60 sec

// One-click heal for one limb
LimbUtil.HealLimb(0);              // skin + muscle max, stop bleed, mend bone, relocate, clear infection
```

## Global Aggregate Queries

No index — check the whole body.

```js
// Presence
if (LimbUtil.HasBrokenBone()) {
    Log.Info('Has a fracture somewhere');
}
if (LimbUtil.HasDislocation()) {
    Log.Info('Has a dislocation');
}
if (LimbUtil.HasInfection()) {
    Log.Info('Has an infection');
}
if (LimbUtil.HasDismemberment()) {
    Log.Info('Has a dismemberment');
}

// Counts
var brokenCount = LimbUtil.CountBroken();
var infectedCount = LimbUtil.CountInfected();

// Global values
var avgPain = LimbUtil.GetAveragePain();           // average body pain
var avgSkin = LimbUtil.GetAverageSkinHealth();      // average skin health
var maxInfection = LimbUtil.GetMaxInfection();      // worst infection
var bleedSpeed = LimbUtil.GetTotalBleedSpeed();     // total bleed rate
```

## Full Example

Full-body health report:

```js
function checkAllLimbs() {
    var count = LimbUtil.GetLimbCount();
    var issues = [];
    for (var i = 0; i < count; i++) {
        var name = LimbUtil.GetLimbShortName(i);
        var parts = [];
        if (LimbUtil.IsBroken(i)) parts.push('fracture');
        if (LimbUtil.IsDislocated(i)) parts.push('dislocation');
        if (LimbUtil.IsInfected(i)) parts.push('infection');
        if (LimbUtil.GetPain(i) > 50) parts.push('severe pain');
        if (parts.length > 0) {
            issues.push(name + ': ' + parts.join('/'));
        }
    }
    if (issues.length === 0) {
        Log.Info('All limbs healthy!');
    } else {
        Log.Warning(issues.join('\n'));
    }
}

// Check every 10 seconds
function onLoad() {
    setInterval(checkAllLimbs, 10000);
}
```
