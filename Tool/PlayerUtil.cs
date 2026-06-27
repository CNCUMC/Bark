using System;
using System.Diagnostics.CodeAnalysis;
using Bark.BetterCCL;
using BepInEx.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class PlayerUtil
{
    public const int MaxInventorySlots = 8;
    private static readonly ManualLogSource Logger = Plugin.Logger;
    
    public static void Tp(Vector2 pos)
    {
        LogUtil.CheckBody(Logger);
        GameInstances.Body!.transform.position = pos;
        if (GameInstances.PlayerCamera != null) GameInstances.PlayerCamera.transform.position = pos;
    }

    public static void Tp(float x, float y)
    {
        Tp(new Vector2(x, y));
    }

    public static void Alert(string text, bool important, float delay = 0f)
    {
        if (string.IsNullOrWhiteSpace(text) || GameInstances.PlayerCamera == null) return;
        if (delay <= 0f) GameInstances.PlayerCamera.DoAlert(text, important);
        else
            GameInstances.PlayerCamera.StartCoroutine(
                GameInstances.PlayerCamera.DoAlertDelayed(text, important, delay));
    }

    public static void PickItem(string item, int slot, bool force = false)
    {
        LogUtil.CheckBody(Logger);
        LogUtil.CheckNotNullOrEmpty(item, nameof(item));
        if (slot is < 0 or >= MaxInventorySlots)
            throw new ArgumentOutOfRangeException(nameof(slot), slot,
                Locale("player.slot.out_of_range", MaxInventorySlots));
        var body = GameInstances.Body!;
        var pos = body.transform.position;
        var go = Utils.Create(item, pos, 0f) ?? throw new InvalidOperationException(Locale("player.load_item.fail", item));
        var cmp = go.GetComponent<Item>() ??
                  throw new InvalidOperationException(Locale("player.load_item.missing_component", item));
        body.PickUpItem(cmp, slot, force);
    }
    
    public static bool IsAlive()
    {
        return GameInstances.Body is { alive: true };
    }

    public static bool IsConscious()
    {
        return GameInstances.Body is { conscious: true };
    }

    public static bool IsDying()
    {
        return GameInstances.Body is { isDying: true };
    }

    public static bool IsCriticallyDying()
    {
        return GameInstances.Body is { isCriticallyDying: true };
    }

    public static bool IsInCardiacArrest()
    {
        return GameInstances.Body is { inCardiacArrest: true };
    }

    public static bool IsSleeping()
    {
        return GameInstances.Body is { sleeping: true };
    }

    public static bool IsExercising()
    {
        return GameInstances.Body is { exercising: true };
    }

    public static bool IsBreathing()
    {
        return GameInstances.Body is { breathing: true };
    }

    public static bool IsInWater()
    {
        return GameInstances.Body is { inWater: true };
    }

    public static bool HasScubaGear()
    {
        return GameInstances.Body is { hasScubaGear: true };
    }

    public static bool IsStanding()
    {
        return GameInstances.Body is { standing: true };
    }

    public static bool IsCrouching()
    {
        return GameInstances.Body is { crouching: true };
    }

    public static bool IsOnHardStimulants()
    {
        return GameInstances.Body is { onHardStimulants: true };
    }

    public static bool UsedNeuralBooster()
    {
        return GameInstances.Body is { usedNeuralBooster: true };
    }

    public static bool IsUsingSleepingBag()
    {
        return GameInstances.Body is { usingSleepingBag: true };
    }

    public static bool IsBothHandsUnusable()
    {
        return GameInstances.Body is { bothHandsUnusable: true };
    }

    public static bool AllowUseItem()
    {
        return GameInstances.Body is { allowUseItem: true };
    }

    public static bool HasPulmonaryEmbolism()
    {
        return GameInstances.Body is { hasPulmonaryEmbolism: true };
    }

    public static bool IsFibrillationForced()
    {
        return GameInstances.Body is { fibrillationForced: true };
    }

    public static bool CanTakeNap()
    {
        return GameInstances.Body is { canTakeNap: true };
    }

    public static bool IsAboveMedicalCutoff()
    {
        return GameInstances.Body is { aboveMedicalCutoff: true };
    }

    public static bool IsDisfigured()
    {
        return GameInstances.Body is { disfigured: true };
    }

    public static bool IsEyeGone()
    {
        return GameInstances.Body is { eyeGone: true };
    }

    public static bool IsBothEyesGone()
    {
        return GameInstances.Body is { bothEyesGone: true };
    }

    public static bool IsMindWiped()
    {
        return GameInstances.Body?.mindWipe != null;
    }

    public static float GetHorrifiedLevel()
    {
        return GameInstances.Body?.horrifiedLevel ?? 0f;
    }

    public static float GetClawHealth()
    {
        return GameInstances.Body?.clawHealth ?? 0f;
    }

    public static float GetWeightOffset()
    {
        return GameInstances.Body?.weightOffset ?? 0f;
    }

    // ==================== 生命体征 ====================

    public static float GetBloodOxygen()
    {
        return GameInstances.Body?.bloodOxygen ?? 0f;
    }

    public static float GetBloodVolume()
    {
        return GameInstances.Body?.bloodVolume ?? 0f;
    }

    public static float GetHeartRate()
    {
        return GameInstances.Body?.heartRate ?? 0f;
    }

    public static float GetBloodPressure()
    {
        return GameInstances.Body?.bloodPressure ?? 0f;
    }

    public static float GetRespiratoryRate()
    {
        return GameInstances.Body?.respiratoryRate ?? 0f;
    }

    public static float GetTemperature()
    {
        return GameInstances.Body?.temperature ?? 0f;
    }

    public static float GetHunger()
    {
        return GameInstances.Body?.hunger ?? 0f;
    }

    public static float GetThirst()
    {
        return GameInstances.Body?.thirst ?? 0f;
    }

    public static float GetStamina()
    {
        return GameInstances.Body?.stamina ?? 0f;
    }

    public static float GetEnergy()
    {
        return GameInstances.Body?.energy ?? 0f;
    }

    public static float GetConsciousness()
    {
        return GameInstances.Body?.consciousness ?? 0f;
    }

    public static float GetBrainHealth()
    {
        return GameInstances.Body?.brainHealth ?? 0f;
    }

    public static float GetHappiness()
    {
        return GameInstances.Body?.totalHappiness ?? 0f;
    }

    public static float GetBloodViscosity()
    {
        return GameInstances.Body?.bloodViscosity ?? 0f;
    }

    public static float GetBloodVesselSize()
    {
        return GameInstances.Body?.bloodVesselSize ?? 1f;
    }

    public static float GetFibrillationProgress()
    {
        return GameInstances.Body?.fibrillationProgress ?? 0f;
    }

    public static float GetAdrenaline()
    {
        return GameInstances.Body?.adrenaline ?? 0f;
    }

    public static float GetCurAdrenaline()
    {
        return GameInstances.Body?.curAdrenaline ?? 0f;
    }

    public static float GetSepticShock()
    {
        return GameInstances.Body?.septicShock ?? 0f;
    }

    public static float GetSicknessAmount()
    {
        return GameInstances.Body?.sicknessAmount ?? 0f;
    }

    public static float GetVenomTotal()
    {
        return GameInstances.Body?.venomTotal ?? 0f;
    }

    public static float GetVenomCurrent()
    {
        return GameInstances.Body?.venomCurrent ?? 0f;
    }

    public static float GetInternalBleeding()
    {
        return GameInstances.Body?.internalBleeding ?? 0f;
    }

    public static float GetHemothorax()
    {
        return GameInstances.Body?.hemothorax ?? 0f;
    }

    public static float GetShock()
    {
        return GameInstances.Body?.shock ?? 0f;
    }

    public static float GetPainShock()
    {
        return GameInstances.Body?.painShock ?? 0f;
    }

    public static float GetTraumaAmount()
    {
        return GameInstances.Body?.traumaAmount ?? 0f;
    }

    public static float GetRadiationSickness()
    {
        return GameInstances.Body?.radiationSickness ?? 0f;
    }

    public static float GetStrokeAmount()
    {
        return GameInstances.Body?.strokeAmount ?? 0f;
    }

    public static float GetFocusedLevel()
    {
        return GameInstances.Body?.focusedLevel ?? 0f;
    }
    
    public static bool HasPainkillers()
    {
        return GameInstances.Body?.GetComponent<Painkillers>() != null;
    }

    public static bool HasAntidepressants()
    {
        return GameInstances.Body?.GetComponent<Antidepressants>() != null;
    }

    public static bool HasSleepingPills()
    {
        return GameInstances.Body?.GetComponent<SleepingPills>() != null;
    }

    public static float GetOpiateHappiness()
    {
        return GameInstances.Body?.opiateHappiness ?? 0f;
    }

    public static float GetAntidepressantHappiness()
    {
        return GameInstances.Body?.antidepressantHappiness ?? 0f;
    }

    public static float GetCaffeinated()
    {
        return GameInstances.Body?.caffeinated ?? 0f;
    }

    public static void RemovePainkillers()
    {
        if (GameInstances.Body is { } body && body.TryGetComponent<Painkillers>(out var c)) Object.Destroy(c);
    }

    public static void RemoveAntidepressants()
    {
        if (GameInstances.Body is { } body && body.TryGetComponent<Antidepressants>(out var c)) Object.Destroy(c);
    }

    public static void RemoveSleepingPills()
    {
        if (GameInstances.Body is { } body && body.TryGetComponent<SleepingPills>(out var c)) Object.Destroy(c);
    }
    
    public static float GetBadSleepAmount()
    {
        return GameInstances.Body?.badSleepAmount ?? 0f;
    }

    public static float GetGoodSleepTime()
    {
        return GameInstances.Body?.goodSleepTime ?? 0f;
    }

    public static bool TriedRollingLastStand()
    {
        return GameInstances.Body is { triedRollingLastStand: true };
    }

    public static bool SuccessfullyRolledLastStand()
    {
        return GameInstances.Body is { succesfullyRolledLastStand: true };
    }

    public static float GetLastStandTime()
    {
        return GameInstances.Body?.lastStandTime ?? 0f;
    }

    public static float GetAntibioticImmunityTime()
    {
        return GameInstances.Body?.antibioticImmunityTime ?? 0f;
    }
    
    public static void Feed(float amount)
    {
        if (GameInstances.Body is { } body) body.hunger = Mathf.Clamp(body.hunger + amount, -100f, 100f);
    }

    public static void Hydrate(float amount)
    {
        if (GameInstances.Body is { } body) body.thirst = Mathf.Clamp(body.thirst + amount, 0f, 200f);
    }

    public static void RestoreStamina(float amount)
    {
        if (GameInstances.Body is { } body) body.stamina = Mathf.Clamp(body.stamina + amount, 0f, 100f);
    }

    public static void RestoreEnergy(float amount)
    {
        if (GameInstances.Body is { } body) body.energy = Mathf.Clamp(body.energy + amount, 0f, 100f);
    }

    public static void HealAll()
    {
        if (GameInstances.Body is not { } body) return;
        foreach (var limb in body.limbs)
        {
            if (limb == null) continue;
            limb.muscleHealth = limb.skinHealth = 100f;
            limb.boneHealTimer = limb.dislocationTimer = 0f;
            limb.infectionAmount = limb.bleedAmount = limb.pain = 0f;
            limb.shrapnel = 0;
            limb.infected = false;
        }

        body.brainHealth = body.bloodVolume = body.bloodOxygen = body.consciousness = body.stamina = body.energy = 100f;
        body.bloodPressure = 120f;
        body.heartRate = 70f;
        body.bloodVesselSize = 1f;
        body.bloodViscosity = 0f;
        body.respiratoryRate = 100f;
        body.strokeAmount = 0f;
        body.fibrillationProgress = 0f;
        body.hunger = body.thirst = 100f;
        body.septicShock = body.sicknessAmount = 0f;
        body.temperature = 37f;
        body.happiness = body.radiationSickness = body.internalBleeding = body.hemothorax = body.traumaAmount = 0f;
        body.dirtyness = body.wetness = body.badSleepAmount = body.hearingLoss = 0f;
        body.antidepressantHappiness = body.opiateHappiness = body.antibioticImmunityTime = 0f;
        body.adrenaline = body.curAdrenaline = body.venomCurrent = body.venomTotal = 0f;
        body.clawHealth = 100f;
        body.focusedLevel = body.horrifiedLevel = body.snowAmount = 0f;
        body.hasPulmonaryEmbolism = false;
        if (body.TryGetComponent<Painkillers>(out var pk)) Object.Destroy(pk);
        if (body.TryGetComponent<SleepingPills>(out var sp)) Object.Destroy(sp);
        if (body.TryGetComponent<Antidepressants>(out var ad)) Object.Destroy(ad);
    }
    
    public static void SetHunger(float value)
    {
        if (GameInstances.Body is { } body) body.hunger = Mathf.Clamp(value, -50f, 125f);
    }

    public static void SetThirst(float value)
    {
        if (GameInstances.Body is { } body) body.thirst = Mathf.Clamp(value, -50f, 250f);
    }

    public static void SetStamina(float value)
    {
        if (GameInstances.Body is { } body) body.stamina = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetEnergy(float value)
    {
        if (GameInstances.Body is { } body) body.energy = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetBloodVolume(float value)
    {
        if (GameInstances.Body is { } body) body.bloodVolume = Mathf.Clamp(value, -100f, 200f);
    }

    public static void SetBloodOxygen(float value)
    {
        if (GameInstances.Body is { } body) body.bloodOxygen = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetHeartRate(float value)
    {
        if (GameInstances.Body is { } body) body.heartRate = Mathf.Clamp(value, 0f, 300f);
    }

    public static void SetBloodPressure(float value)
    {
        if (GameInstances.Body is { } body) body.bloodPressure = Mathf.Clamp(value, 0f, 250f);
    }

    public static void SetTemperature(float value)
    {
        if (GameInstances.Body is { } body) body.temperature = Mathf.Clamp(value, 20f, 50f);
    }

    public static void SetConsciousness(float value)
    {
        if (GameInstances.Body is { } body) body.consciousness = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetBrainHealth(float value)
    {
        if (GameInstances.Body is { } body) body.brainHealth = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetHappiness(float value)
    {
        if (GameInstances.Body is { } body) body.happiness = Mathf.Clamp(value, -100f, 100f);
    }

    public static void SetRadiationSickness(float value)
    {
        if (GameInstances.Body is { } body) body.radiationSickness = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetTraumaAmount(float value)
    {
        if (GameInstances.Body is { } body) body.traumaAmount = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetInternalBleeding(float value)
    {
        if (GameInstances.Body is { } body) body.internalBleeding = Mathf.Clamp(value, 0f, 100f);
    }

    private static string Locale(string key, params object[] args)
    {
        return BetterLocale.GetLog(key, args);
    }
    
    public static class Thresholds
    {
        public const float BpCriticalLow = 60f, BpLow3 = 83f, BpLow2 = 96f, BpLow1 = 110f;
        public const float BpHigh1 = 130f, BpHigh2 = 145f, BpHigh3 = 162f, BpCriticalHigh = 180f;
        public const float O2Critical = 45f, O2Severe = 60f, O2Low = 75f, O2Mild = 90f;

        public const float HrArrest = 20f,
            HrBradySevere = 40f,
            HrBradyMild = 60f,
            HrTachyMild = 110f,
            HrTachySevere = 160f,
            HrTachyCritical = 200f;

        public const float TempHypoCrit = 28f, TempHypoSev = 32.5f, TempHypoMild = 34f, TempCold = 35.5f;

        public const float TempNormal = 37f,
            TempWarm = 38f,
            TempHyperMild = 39f,
            TempHyperSev = 40.25f,
            TempHyperCrit = 41.5f;

        public const float BleedCrit = 0.3f, BleedHeavy = 0.15f, BleedMed = 0.06f;
        public const float HungerStarve = 15f, HungerVHungry = 35f, HungerHungry = 50f, HungerPeckish = 75f;
        public const float ThirstCrit = 20f, ThirstVThirsty = 35f, ThirstThirsty = 55f, ThirstMild = 75f;
        public const float StamExhaust = 15f, StamVTired = 35f, StamTired = 50f, StamMild = 70f;
        public const float EnergyExhaust = 7f, EnergyVTired = 15f, EnergyTired = 25f, EnergyMild = 35f;
        public const float ConscUncon = 20f, ConscIncap = 30f, ConscConf3 = 55f, ConscConf2 = 72f, ConscConf1 = 90f;
        public const float PainAgony = 80f, PainSevere = 55f, PainModerate = 30f, PainMild = 10f;
        public const float BrainSev = 30f, BrainMod = 60f, BrainMild = 80f, BrainSlight = 95f;
        public const float StrokeCrit = 70f, SepsisCrit = 80f, SepsisMod = 50f, SepsisMild = 10f;
        public const float RadCrit = 80f, RadSev = 50f, RadMod = 30f, RadMild = 10f;
        public const float IntBleedCrit = 50f, HemoHeavy = 70f, HemoPresent = 40f;
        public const float HappyMiserable = -75f, HappyDepressed = -50f, HappyGloomy = -30f;
    }
}