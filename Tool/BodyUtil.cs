using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Tool;

// 玩家身体：状态检测、生理数值、药物、恢复、医疗阈值
public static class BodyUtil
{
    public static Body Body => PlayerCamera.main.body!;

    // ==================== Status - 布尔状态检查 ====================
    public static class Status
    {
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

        public static bool TriedRollingLastStand()
        {
            return Body is { triedRollingLastStand: true };
        }

        public static bool SuccessfullyRolledLastStand()
        {
            return Body is { succesfullyRolledLastStand: true };
        }
    }

    // ==================== Vitals - 生理数值读写 ====================
    public static class Vitals
    {
        // -- 已有 Getter --

        public static float GetFocusedLevel()
        {
            return Body.focusedLevel;
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

        public static float GetTotalHappiness()
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

        public static float GetBadSleepAmount()
        {
            return Body.badSleepAmount;
        }

        public static float GetGoodSleepTime()
        {
            return Body.goodSleepTime;
        }

        public static float GetLastStandTime()
        {
            return Body.lastStandTime;
        }

        public static float GetAntibioticImmunityTime()
        {
            return Body.antibioticImmunityTime;
        }

        // -- 新增 Getter（HealAll 中使用但未暴露的字段） --

        public static float GetDirtyness()
        {
            return Body.dirtyness;
        }

        public static float GetWetness()
        {
            return Body.wetness;
        }

        public static float GetHearingLoss()
        {
            return Body.hearingLoss;
        }

        public static float GetSnowAmount()
        {
            return Body.snowAmount;
        }

        public static float GetRawHappiness()
        {
            return Body.happiness;
        }

        // -- 已有 Setter --

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

        // -- 新增 Setter（补全所有已有 Getter 对应的 Setter） --

        public static void SetFocusedLevel(float value)
        {
            if (Body is { } body) body.focusedLevel = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetHorrifiedLevel(float value)
        {
            if (Body is { } body) body.horrifiedLevel = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetClawHealth(float value)
        {
            if (Body is { } body) body.clawHealth = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetWeightOffset(float value)
        {
            if (Body is { } body) body.weightOffset = value;
        }

        public static void SetRespiratoryRate(float value)
        {
            if (Body is { } body) body.respiratoryRate = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetBloodViscosity(float value)
        {
            if (Body is { } body) body.bloodViscosity = Mathf.Clamp(value, -100f, 100f);
        }

        public static void SetBloodVesselSize(float value)
        {
            if (Body is { } body) body.bloodVesselSize = Mathf.Clamp(value, 0f, 2f);
        }

        public static void SetFibrillationProgress(float value)
        {
            if (Body is { } body) body.fibrillationProgress = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetAdrenaline(float value)
        {
            if (Body is { } body) body.adrenaline = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetCurAdrenaline(float value)
        {
            if (Body is { } body) body.curAdrenaline = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetSepticShock(float value)
        {
            if (Body is { } body) body.septicShock = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetSicknessAmount(float value)
        {
            if (Body is { } body) body.sicknessAmount = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetVenomTotal(float value)
        {
            if (Body is { } body) body.venomTotal = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetVenomCurrent(float value)
        {
            if (Body is { } body) body.venomCurrent = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetHemothorax(float value)
        {
            if (Body is { } body) body.hemothorax = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetShock(float value)
        {
            if (Body is { } body) body.shock = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetPainShock(float value)
        {
            if (Body is { } body) body.painShock = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetStrokeAmount(float value)
        {
            if (Body is { } body) body.strokeAmount = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetBadSleepAmount(float value)
        {
            if (Body is { } body) body.badSleepAmount = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetGoodSleepTime(float value)
        {
            if (Body is { } body) body.goodSleepTime = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetLastStandTime(float value)
        {
            if (Body is { } body) body.lastStandTime = Mathf.Clamp(value, 0f, float.MaxValue);
        }

        public static void SetAntibioticImmunityTime(float value)
        {
            if (Body is { } body) body.antibioticImmunityTime = Mathf.Clamp(value, 0f, float.MaxValue);
        }

        // -- 新增 Setter（HealAll 中使用但未暴露的字段） --

        public static void SetDirtyness(float value)
        {
            if (Body is { } body) body.dirtyness = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetWetness(float value)
        {
            if (Body is { } body) body.wetness = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetHearingLoss(float value)
        {
            if (Body is { } body) body.hearingLoss = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetSnowAmount(float value)
        {
            if (Body is { } body) body.snowAmount = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetRawHappiness(float value)
        {
            if (Body is { } body) body.happiness = Mathf.Clamp(value, -100f, 100f);
        }
    }

    // ==================== Drugs - 药物组件与心理效果 ====================
    public static class Drugs
    {
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

        public static void SetOpiateHappiness(float value)
        {
            if (Body is { } body) body.opiateHappiness = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetAntidepressantHappiness(float value)
        {
            if (Body is { } body) body.antidepressantHappiness = Mathf.Clamp(value, 0f, 100f);
        }

        public static void SetCaffeinated(float value)
        {
            if (Body is { } body) body.caffeinated = Mathf.Clamp(value, 0f, 100f);
        }

        // -- 心理相关 --

        public static int GetCorpsesSeen()
        {
            return Body.corpsesSeen;
        }

        public static void SetCorpsesSeen(int count)
        {
            if (Body is { } body) body.corpsesSeen = Mathf.Max(0, count);
        }
    }

    // ==================== Recovery - 恢复与治疗 ====================
    public static class Recovery
    {
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

            body.brainHealth = body.bloodVolume =
                body.bloodOxygen = body.consciousness = body.stamina = body.energy = 100f;
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
    }

    // ==================== Thresholds - 医学生理阈值常量 ====================
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
