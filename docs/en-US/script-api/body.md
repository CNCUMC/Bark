***English*** | [简体中文](../../zh-CN/script-api/body.md)

# Body — Body

Body is the most frequently called API. It controls body state, vital signs, and drug effects. All methods follow
consistent naming prefixes.

## State Checks

`Is*` / `Has*` / `Can*` boolean queries. Returns `false` when Body doesn't exist.

| Prefix          | Meaning                | Example                                    |
|-----------------|------------------------|--------------------------------------------|
| `Is*`           | Currently in a state   | `IsAlive()`, `IsSleeping()`, `IsDying()`   |
| `Has*`          | Has something / effect | `HasScubaGear()`, `HasPulmonaryEmbolism()` |
| `Can*`          | Can do something       | `CanTakeNap()`                             |
| `Allow*`        | Is allowed             | `AllowUseItem()`                           |
| `Used*`         | Has used               | `UsedNeuralBooster()`                      |
| `Tried*`        | Has attempted          | `TriedRollingLastStand()`                  |
| `Successfully*` | Successfully did       | `SuccessfullyRolledLastStand()`            |

```js
if (Body.IsDying()) {
    Player.Alert('You are dying!', true);
}

if (Body.HasScubaGear() && Body.IsInWater()) {
    Log.Info('Safe to dive');
}
```

Full list (28): `IsAlive`, `IsConscious`, `IsDying`, `IsCriticallyDying`, `IsInCardiacArrest`, `IsSleeping`,
`IsExercising`, `IsBreathing`, `IsInWater`, `IsStanding`, `IsCrouching`, `IsOnHardStimulants`, `IsUsingSleepingBag`,
`IsBothHandsUnusable`, `IsFibrillationForced`, `IsAboveMedicalCutoff`, `IsDisfigured`, `IsEyeGone`, `IsBothEyesGone`,
`IsMindWiped`, `HasScubaGear`, `HasPulmonaryEmbolism`, `HasPainkillers`, `HasAntidepressants`, `HasSleepingPills`,
`CanTakeNap`, `AllowUseItem`, `UsedNeuralBooster`, `TriedRollingLastStand`, `SuccessfullyRolledLastStand`

## Value Queries

`Get*` float queries. Returns `0f` when Body doesn't exist.

```js
var hunger = Body.GetHunger();           // 0-125
var bloodVolume = Body.GetBloodVolume(); // 0-200
var temp = Body.GetTemperature();        // 20-50
var heartRate = Body.GetHeartRate();     // 0-300
```

> ℹ️ `GetHappiness` maps to the `totalHappiness` field internally.

Full list (36): `GetHunger`, `GetThirst`, `GetStamina`, `GetEnergy`, `GetConsciousness`, `GetBrainHealth`,
`GetBloodVolume`, `GetBloodOxygen`, `GetHeartRate`, `GetBloodPressure`, `GetRespiratoryRate`, `GetTemperature`,
`GetBloodViscosity`, `GetBloodVesselSize`, `GetFibrillationProgress`, `GetAdrenaline`, `GetCurAdrenaline`,
`GetSepticShock`, `GetSicknessAmount`, `GetVenomTotal`, `GetVenomCurrent`, `GetInternalBleeding`, `GetHemothorax`,
`GetShock`, `GetPainShock`, `GetTraumaAmount`, `GetRadiationSickness`, `GetStrokeAmount`, `GetBadSleepAmount`,
`GetGoodSleepTime`, `GetLastStandTime`, `GetAntibioticImmunityTime`, `GetDirtyness`, `GetWetness`, `GetHearingLoss`,
`GetSnowAmount`, `GetRawHappiness`, `GetFocusedLevel`, `GetHorrifiedLevel`, `GetClawHealth`, `GetWeightOffset`,
`GetOpiateHappiness`, `GetAntidepressantHappiness`, `GetCaffeinated`, `GetCorpsesSeen`

## Setting Values

`Set*` write methods. Takes `float`, values are clamped to valid ranges.

```js
Body.SetHunger(80);       // full
Body.SetStamina(100);     // max stamina
Body.SetBloodVolume(90);  // restore blood
Body.SetTemperature(37);  // normal temp
```

Full list (33 Setters): `SetHunger`, `SetThirst`, `SetStamina`, `SetEnergy`, `SetBloodVolume`, `SetBloodOxygen`,
`SetHeartRate`, `SetBloodPressure`, `SetTemperature`, `SetConsciousness`, `SetBrainHealth`, `SetRadiationSickness`,
`SetTraumaAmount`, `SetInternalBleeding`, `SetFocusedLevel`, `SetHorrifiedLevel`, `SetClawHealth`, `SetWeightOffset`,
`SetRespiratoryRate`, `SetBloodViscosity`, `SetBloodVesselSize`, `SetFibrillationProgress`, `SetAdrenaline`,
`SetCurAdrenaline`, `SetSepticShock`, `SetSicknessAmount`, `SetVenomTotal`, `SetVenomCurrent`, `SetHemothorax`,
`SetShock`, `SetPainShock`, `SetStrokeAmount`, `SetBadSleepAmount`, `SetGoodSleepTime`, `SetLastStandTime`,
`SetAntibioticImmunityTime`, `SetDirtyness`, `SetWetness`, `SetHearingLoss`, `SetSnowAmount`, `SetRawHappiness`,
`SetOpiateHappiness`, `SetAntidepressantHappiness`, `SetCaffeinated`

`SetCorpsesSeen(int count)` is the only method accepting int — sets the number of corpses seen.

## Drug-Related

`Has* / Remove* / Set*` — check if a drug is active, remove drug effects, set drug-related values.

```js
// Check
if (Body.HasPainkillers()) {
    Log.Info('Painkillers active');
}

// Remove
Body.RemovePainkillers();       // force end painkiller effect
Body.RemoveAntidepressants();   // end antidepressant
Body.RemoveSleepingPills();     // end sleeping pills

// Set values
Body.SetOpiateHappiness(50);          // opiate happiness
Body.SetAntidepressantHappiness(80);  // antidepressant happiness
Body.SetCaffeinated(100);             // caffeine level
```

## Compound Operations

These methods don't follow `Get/Set` prefix — they're special operations.

| Method                   | Description                                                                 |
|--------------------------|-----------------------------------------------------------------------------|
| `Feed(amount)`           | Increase hunger (relative)                                                  |
| `Hydrate(amount)`        | Increase thirst (relative)                                                  |
| `RestoreStamina(amount)` | Restore stamina                                                             |
| `RestoreEnergy(amount)`  | Restore energy                                                              |
| `HealAll()`              | Full restore: heal all limbs + restore all vitals + remove all drug effects |

```js
Body.Feed(20);             // take a bite
Body.Hydrate(30);          // take a sip
Body.RestoreStamina(50);   // energy drink
Body.HealAll();            // god mode
```

## Examples

### Status Monitor HUD

```js
function onUpdate() {
    // Danger alerts
    if (Body.IsDying()) {
        Player.Alert('Critically dying!', true);
    }
    if (Body.GetBloodVolume() < 40) {
        Player.Alert('Severe blood loss!', true);
    }
    if (Body.GetBrainHealth() < 30) {
        Player.Alert('Severe brain damage!', true);
    }
}
```

### Auto-Heal Script

```js
function onLoad() {
    // Check every 5 seconds
    setInterval(function () {
        if (Body.GetBloodVolume() < 80) {
            Body.SetBloodVolume(90);
            Log.Info('Auto-healed');
        }
    }, 5000);
}
```

## Notes

- All `Get*` return `0f` when Body is absent, not null
- `Set*` values are auto-clamped — overshooting is harmless
- `HealAll()` is the ultimate recovery — it clears all negative states including drug effects
- `Feed/Hydrate` are relative — pass negative to decrease
- Dual-limb checks (e.g., `IsBothHandsUnusable`) check both hands; for fine-grained operations see [Limb](limbs.md)
