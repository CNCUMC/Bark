using System;
using System.Diagnostics.CodeAnalysis;
using Bark.BetterCCL;
using BepInEx.Logging;
using CUCoreLib.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class PlayerUtil
{
    public const int MaxInventorySlots = 8;
    public static readonly Body Body = PlayerCamera.main.body;
    private static readonly ManualLogSource Logger = Plugin.Logger;

    public static void Tp(Vector2 pos)
    {
        LogUtil.CheckBody(Logger);
        Body.transform.position = pos;
        if (Body != null) Body.transform.position = pos;
    }

    public static void Tp(float x, float y)
    {
        Tp(new Vector2(x, y));
    }

    public static void Alert(string text, bool important, float delay = 0f)
    {
        if (string.IsNullOrWhiteSpace(text) || Body == null) return;
        if (delay <= 0f) PlayerCamera.main.DoAlert(text, important);
        else
            CUCoreUtils.StartCoroutine(
                PlayerCamera.main.DoAlertDelayed(text, important, delay));
    }

    public static void PickItem(string item, int slot, bool force = false)
    {
        LogUtil.CheckBody(Logger);
        LogUtil.CheckNotNullOrEmpty(item, nameof(item));
        if (slot is < 0 or >= MaxInventorySlots)
            throw new ArgumentOutOfRangeException(nameof(slot), slot,
                Locale("player.slot.out_of_range", MaxInventorySlots));
        var pos = Body.transform.position;
        var go = Utils.Create(item, pos, 0f) ??
                 throw new InvalidOperationException(Locale("player.load_item.fail", item));
        var cmp = go.GetComponent<Item>() ??
                  throw new InvalidOperationException(Locale("player.load_item.missing_component", item));
        Body.PickUpItem(cmp, slot, force);
    }

    public static bool IsAlive()
    {
        return Body is { alive: true };
    }

    public static bool IsConscious()
    {
        return Body is { conscious: true };
    }

    public static bool IsDying()
    {
        return Body is { isDying: true };
    }

    public static bool IsCriticallyDying()
    {
        return Body is { isCriticallyDying: true };
    }

    public static bool IsInCardiacArrest()
    {
        return Body is { inCardiacArrest: true };
    }

    public static bool IsSleeping()
    {
        return Body is { sleeping: true };
    }

    public static bool IsExercising()
    {
        return Body is { exercising: true };
    }

    public static bool IsBreathing()
    {
        return Body is { breathing: true };
    }

    public static bool IsInWater()
    {
        return Body is { inWater: true };
    }

    public static bool HasScubaGear()
    {
        return Body is { hasScubaGear: true };
    }

    public static bool IsStanding()
    {
        return Body is { standing: true };
    }

    public static bool IsCrouching()
    {
        return Body is { crouching: true };
    }

    public static bool IsOnHardStimulants()
    {
        return Body is { onHardStimulants: true };
    }

    public static bool UsedNeuralBooster()
    {
        return Body is { usedNeuralBooster: true };
    }

    public static bool IsUsingSleepingBag()
    {
        return Body is { usingSleepingBag: true };
    }

    public static bool IsBothHandsUnusable()
    {
        return Body is { bothHandsUnusable: true };
    }

    public static bool AllowUseItem()
    {
        return Body is { allowUseItem: true };
    }

    public static bool HasPulmonaryEmbolism()
    {
        return Body is { hasPulmonaryEmbolism: true };
    }

    public static bool IsFibrillationForced()
    {
        return Body is { fibrillationForced: true };
    }

    public static bool CanTakeNap()
    {
        return Body is { canTakeNap: true };
    }

    public static bool IsAboveMedicalCutoff()
    {
        return Body is { aboveMedicalCutoff: true };
    }

    public static bool IsDisfigured()
    {
        return Body is { disfigured: true };
    }

    public static bool IsEyeGone()
    {
        return Body is { eyeGone: true };
    }

    public static bool IsBothEyesGone()
    {
        return Body is { bothEyesGone: true };
    }

    public static bool IsMindWiped()
    {
        return Body.mindWipe != null;
    }

    public static float GetHorrifiedLevel()
    {
        return Body.horrifiedLevel;
    }

    public static float GetClawHealth()
    {
        return Body.clawHealth;
    }

    public static float GetWeightOffset()
    {
        return Body.weightOffset;
    }

    public static float GetBloodOxygen()
    {
        return Body.bloodOxygen;
    }

    public static float GetBloodVolume()
    {
        return Body.bloodVolume;
    }

    public static float GetHeartRate()
    {
        return Body.heartRate;
    }

    public static float GetBloodPressure()
    {
        return Body.bloodPressure;
    }

    public static float GetRespiratoryRate()
    {
        return Body.respiratoryRate;
    }

    public static float GetTemperature()
    {
        return Body.temperature;
    }

    public static float GetHunger()
    {
        return Body.hunger;
    }

    public static float GetThirst()
    {
        return Body.thirst;
    }

    public static float GetStamina()
    {
        return Body.stamina;
    }

    public static float GetEnergy()
    {
        return Body.energy;
    }

    public static float GetConsciousness()
    {
        return Body.consciousness;
    }

    public static float GetBrainHealth()
    {
        return Body.brainHealth;
    }

    public static float GetHappiness()
    {
        return Body.totalHappiness;
    }

    public static float GetBloodViscosity()
    {
        return Body.bloodViscosity;
    }

    public static float GetBloodVesselSize()
    {
        return Body.bloodVesselSize;
    }

    public static float GetFibrillationProgress()
    {
        return Body.fibrillationProgress;
    }

    public static float GetAdrenaline()
    {
        return Body.adrenaline;
    }

    public static float GetCurAdrenaline()
    {
        return Body.curAdrenaline;
    }

    public static float GetSepticShock()
    {
        return Body.septicShock;
    }

    public static float GetSicknessAmount()
    {
        return Body.sicknessAmount;
    }

    public static float GetVenomTotal()
    {
        return Body.venomTotal;
    }

    public static float GetVenomCurrent()
    {
        return Body.venomCurrent;
    }

    public static float GetInternalBleeding()
    {
        return Body.internalBleeding;
    }

    public static float GetHemothorax()
    {
        return Body.hemothorax;
    }

    public static float GetShock()
    {
        return Body.shock;
    }

    public static float GetPainShock()
    {
        return Body.painShock;
    }

    public static float GetTraumaAmount()
    {
        return Body.traumaAmount;
    }

    public static float GetRadiationSickness()
    {
        return Body.radiationSickness;
    }

    public static float GetStrokeAmount()
    {
        return Body.strokeAmount;
    }

    public static float GetFocusedLevel()
    {
        return Body.focusedLevel;
    }

    public static bool HasPainkillers()
    {
        return Body.GetComponent<Painkillers>() != null;
    }

    public static bool HasAntidepressants()
    {
        return Body.GetComponent<Antidepressants>() != null;
    }

    public static bool HasSleepingPills()
    {
        return Body.GetComponent<SleepingPills>() != null;
    }

    public static float GetOpiateHappiness()
    {
        return Body.opiateHappiness;
    }

    public static float GetAntidepressantHappiness()
    {
        return Body.antidepressantHappiness;
    }

    public static float GetCaffeinated()
    {
        return Body.caffeinated;
    }

    public static void RemovePainkillers()
    {
        if (Body is { } body && body.TryGetComponent<Painkillers>(out var c)) Object.Destroy(c);
    }

    public static void RemoveAntidepressants()
    {
        if (Body is { } body && body.TryGetComponent<Antidepressants>(out var c)) Object.Destroy(c);
    }

    public static void RemoveSleepingPills()
    {
        if (Body is { } body && body.TryGetComponent<SleepingPills>(out var c)) Object.Destroy(c);
    }

    public static float GetBadSleepAmount()
    {
        return Body.badSleepAmount;
    }

    public static float GetGoodSleepTime()
    {
        return Body.goodSleepTime;
    }

    public static bool TriedRollingLastStand()
    {
        return Body is { triedRollingLastStand: true };
    }

    public static bool SuccessfullyRolledLastStand()
    {
        return Body is { succesfullyRolledLastStand: true };
    }

    public static float GetLastStandTime()
    {
        return Body.lastStandTime;
    }

    public static float GetAntibioticImmunityTime()
    {
        return Body.antibioticImmunityTime;
    }

    public static void Feed(float amount)
    {
        if (Body is { } body) body.hunger = Mathf.Clamp(body.hunger + amount, -100f, 100f);
    }

    public static void Hydrate(float amount)
    {
        if (Body is { } body) body.thirst = Mathf.Clamp(body.thirst + amount, 0f, 200f);
    }

    public static void RestoreStamina(float amount)
    {
        if (Body is { } body) body.stamina = Mathf.Clamp(body.stamina + amount, 0f, 100f);
    }

    public static void RestoreEnergy(float amount)
    {
        if (Body is { } body) body.energy = Mathf.Clamp(body.energy + amount, 0f, 100f);
    }

    public static void HealAll()
    {
        if (Body is not { } body) return;
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
        if (Body is { } body) body.hunger = Mathf.Clamp(value, -50f, 125f);
    }

    public static void SetThirst(float value)
    {
        if (Body is { } body) body.thirst = Mathf.Clamp(value, -50f, 250f);
    }

    public static void SetStamina(float value)
    {
        if (Body is { } body) body.stamina = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetEnergy(float value)
    {
        if (Body is { } body) body.energy = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetBloodVolume(float value)
    {
        if (Body is { } body) body.bloodVolume = Mathf.Clamp(value, -100f, 200f);
    }

    public static void SetBloodOxygen(float value)
    {
        if (Body is { } body) body.bloodOxygen = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetHeartRate(float value)
    {
        if (Body is { } body) body.heartRate = Mathf.Clamp(value, 0f, 300f);
    }

    public static void SetBloodPressure(float value)
    {
        if (Body is { } body) body.bloodPressure = Mathf.Clamp(value, 0f, 250f);
    }

    public static void SetTemperature(float value)
    {
        if (Body is { } body) body.temperature = Mathf.Clamp(value, 20f, 50f);
    }

    public static void SetConsciousness(float value)
    {
        if (Body is { } body) body.consciousness = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetBrainHealth(float value)
    {
        if (Body is { } body) body.brainHealth = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetHappiness(float value)
    {
        if (Body is { } body) body.happiness = Mathf.Clamp(value, -100f, 100f);
    }

    public static void SetRadiationSickness(float value)
    {
        if (Body is { } body) body.radiationSickness = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetTraumaAmount(float value)
    {
        if (Body is { } body) body.traumaAmount = Mathf.Clamp(value, 0f, 100f);
    }

    public static void SetInternalBleeding(float value)
    {
        if (Body is { } body) body.internalBleeding = Mathf.Clamp(value, 0f, 100f);
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