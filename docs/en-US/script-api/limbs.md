***English*** | [简体中文](../../zh-CN/script-api/limbs.md)

# Limb — Limb Operations

Limb operates on individual limbs: query status, read values, apply damage, heal. All single-limb methods accept an
`int index` parameter.

## Limb Indexes

Limbs are accessed by zero-based index.

```js
var count = Limb.GetLimbCount();  // total limb count
var name = Limb.GetLimbName(0);   // full name of limb #0
var short = Limb.GetLimbShortName(0); // short name
```

## Limb Name Validation

Query and validate limb names without relying on indexes.

```js
// Check if a name is valid (case-insensitive)
if (Limb.IsValidLimbName("FootF")) {
    Log.Info('FootF is a valid limb');
}
if (!Limb.IsValidLimbName("feet")) {
    Log.Warning('"feet" is not valid — use FootF or FootB');
}

// Get all 15 valid limb names
var names = Limb.GetAllLimbNames();
// ["Head", "UpTorso", "DownTorso", "UpArmF", "DownArmF", "HandF",
//  "UpArmB", "DownArmB", "HandB", "ThighF", "CrusF", "FootF",
//  "ThighB", "CrusB", "FootB"]
```

| Method                  | Returns        | Description                                              |
|-------------------------|----------------|----------------------------------------------------------|
| `IsValidLimbName(name)` | `bool`         | `true` if `name` matches a known limb (case-insensitive) |
| `GetAllLimbNames()`     | `List<string>` | All 15 valid limb names in a list                        |

The 15 known limbs: `Head`, `UpTorso`, `DownTorso`, `UpArmF`, `DownArmF`, `HandF`, `UpArmB`, `DownArmB`, `HandB`,
`ThighF`, `CrusF`, `FootF`, `ThighB`, `CrusB`, `FootB`.

## Status Queries

`Is*` / `Has*` boolean queries with a limb index.

```js
if (Limb.IsBroken(0)) {
    Log.Info('Limb #0 is fractured');
}
if (Limb.IsDislocated(1)) {
    Log.Info('Limb #1 is dislocated');
}
if (Limb.IsInfected(2)) {
    Log.Info('Limb #2 is infected');
}
if (Limb.IsDismembered(3)) {
    Log.Info('Limb #3 is severed');
}
if (Limb.IsSplinted(0)) {
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
var skin = Limb.GetSkinHealth(0);           // skin health 0-100
var muscle = Limb.GetMuscleHealth(0);       // muscle health 0-100
var pain = Limb.GetPain(0);                 // pain 0-100
var bleed = Limb.GetBleedAmount(0);         // current bleed
var totalBleed = Limb.GetTotalBleedAmount(0); // total blood lost
var infection = Limb.GetInfectionAmount(0); // infection level
var shrapnel = Limb.GetShrapnelCount(0);    // shrapnel count
```

## Modification Operations

Naming: `Set*` = absolute value, `Damage*` = relative decrease, `HealLimb` = one-click restore.

```js
// Set absolute values
Limb.SetSkinHealth(0, 80);     // restore skin to 80
Limb.SetMuscleHealth(1, 100);  // max muscle
Limb.SetPain(2, 0);            // pain relief
Limb.SetBleed(0, 0);           // stop bleeding
Limb.SetInfection(1, 0);       // clear infection
Limb.SetShrapnel(3, 0);        // remove shrapnel

// Apply damage (relative)
Limb.DamageSkin(0, 20);        // skin -20
Limb.DamageMuscle(1, 30);      // muscle -30

// Special operations
Limb.BreakBone(0);             // fracture
Limb.MendBone(0);              // set bone
Limb.DislocateLimb(1);         // dislocate
Limb.UnDislocateLimb(1);       // relocate
Limb.SetBlockedBleeding(2, true);  // apply tourniquet
Limb.SetDisinfect(0, 60);      // disinfect for 60 sec

// One-click heal for one limb
Limb.HealLimb(0);              // skin + muscle max, stop bleed, mend bone, relocate, clear infection
```

## Amputation

Sever a limb using the player's currently held item as the cutting tool. No-op if the player holds no usable item.

```js
// Sever limb #0 using the held item
Limb.DoAmputate(0);
```

| Method               | Description                                      |
|----------------------|--------------------------------------------------|
| `DoAmputate(index)`  | Sever the limb using the currently held item     |

> The player must hold an item that supports amputation (e.g. a saw or scalpel). Doing nothing otherwise.

## Global Aggregate Queries

No index — check the whole body.

```js
// Presence
if (Limb.HasBrokenBone()) {
    Log.Info('Has a fracture somewhere');
}
if (Limb.HasDislocation()) {
    Log.Info('Has a dislocation');
}
if (Limb.HasInfection()) {
    Log.Info('Has an infection');
}
if (Limb.HasDismemberment()) {
    Log.Info('Has a dismemberment');
}

// Counts
var brokenCount = Limb.CountBroken();
var infectedCount = Limb.CountInfected();

// Global values
var avgPain = Limb.GetAveragePain();           // average body pain
var avgSkin = Limb.GetAverageSkinHealth();      // average skin health
var maxInfection = Limb.GetMaxInfection();      // worst infection
var bleedSpeed = Limb.GetTotalBleedSpeed();     // total bleed rate
```

## Full Example

Full-body health report:

```js
function checkAllLimbs() {
    var count = Limb.GetLimbCount();
    var issues = [];
    for (var i = 0; i < count; i++) {
        var name = Limb.GetLimbShortName(i);
        var parts = [];
        if (Limb.IsBroken(i)) parts.push('fracture');
        if (Limb.IsDislocated(i)) parts.push('dislocation');
        if (Limb.IsInfected(i)) parts.push('infection');
        if (Limb.GetPain(i) > 50) parts.push('severe pain');
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
