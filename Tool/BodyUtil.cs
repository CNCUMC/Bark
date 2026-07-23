using Bark.ScriptApi;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Tool;

// 玩家身体：状态检测、生理数值、药物、恢复、医疗阈值
// 方法加 [ScriptMethod] 后自动暴露给 Lua 脚本，无需手写 BodyApi 包装
public static class BodyUtil
{
    public static Body Body => PlayerCamera.main.body!;

    [ScriptMethod]
    public static bool IsAlive() => Body is { alive: true };

    [ScriptMethod]
    public static bool IsConscious() => Body is { conscious: true };

    [ScriptMethod]
    public static bool IsDying() => Body is { isDying: true };

    [ScriptMethod]
    public static bool IsCriticallyDying() => Body is { isCriticallyDying: true };

    [ScriptMethod]
    public static bool IsInCardiacArrest() => Body is { inCardiacArrest: true };

    [ScriptMethod]
    public static bool IsSleeping() => Body is { sleeping: true };

    [ScriptMethod]
    public static bool IsExercising() => Body is { exercising: true };

    [ScriptMethod]
    public static bool IsBreathing() => Body is { breathing: true };

    [ScriptMethod]
    public static bool IsInWater() => Body is { inWater: true };

    [ScriptMethod]
    public static bool HasScubaGear() => Body is { hasScubaGear: true };

    [ScriptMethod]
    public static bool IsStanding() => Body is { standing: true };

    [ScriptMethod]
    public static bool IsCrouching() => Body is { crouching: true };

    [ScriptMethod]
    public static bool IsOnHardStimulants() => Body is { onHardStimulants: true };

    [ScriptMethod]
    public static bool UsedNeuralBooster() => Body is { usedNeuralBooster: true };

    [ScriptMethod]
    public static bool IsUsingSleepingBag() => Body is { usingSleepingBag: true };

    [ScriptMethod]
    public static bool IsBothHandsUnusable() => Body is { bothHandsUnusable: true };

    [ScriptMethod]
    public static bool AllowUseItem() => Body is { allowUseItem: true };

    [ScriptMethod]
    public static bool HasPulmonaryEmbolism() => Body is { hasPulmonaryEmbolism: true };

    [ScriptMethod]
    public static bool IsFibrillationForced() => Body is { fibrillationForced: true };

    [ScriptMethod]
    public static bool CanTakeNap() => Body is { canTakeNap: true };

    [ScriptMethod]
    public static bool IsAboveMedicalCutoff() => Body is { aboveMedicalCutoff: true };

    [ScriptMethod]
    public static bool IsDisfigured() => Body is { disfigured: true };

    [ScriptMethod]
    public static bool IsEyeGone() => Body is { eyeGone: true };

    [ScriptMethod]
    public static bool IsBothEyesGone() => Body is { bothEyesGone: true };

    [ScriptMethod]
    public static bool IsMindWiped() => Body.mindWipe != null;

    [ScriptMethod]
    public static bool TriedRollingLastStand() => Body is { triedRollingLastStand: true };

    [ScriptMethod]
    public static bool SuccessfullyRolledLastStand() => Body is { succesfullyRolledLastStand: true };

    [ScriptMethod]
    public static float GetFocusedLevel() => Body.focusedLevel;

    [ScriptMethod]
    public static float GetHorrifiedLevel() => Body.horrifiedLevel;

    [ScriptMethod]
    public static float GetClawHealth() => Body.clawHealth;

    [ScriptMethod]
    public static float GetWeightOffset() => Body.weightOffset;

    [ScriptMethod]
    public static float GetBloodOxygen() => Body.bloodOxygen;

    [ScriptMethod]
    public static float GetBloodVolume() => Body.bloodVolume;

    [ScriptMethod]
    public static float GetHeartRate() => Body.heartRate;

    [ScriptMethod]
    public static float GetBloodPressure() => Body.bloodPressure;

    [ScriptMethod]
    public static float GetRespiratoryRate() => Body.respiratoryRate;

    [ScriptMethod]
    public static float GetTemperature() => Body.temperature;

    [ScriptMethod]
    public static float GetHunger() => Body.hunger;

    [ScriptMethod]
    public static float GetThirst() => Body.thirst;

    [ScriptMethod]
    public static float GetStamina() => Body.stamina;

    [ScriptMethod]
    public static float GetEnergy() => Body.energy;

    [ScriptMethod]
    public static float GetConsciousness() => Body.consciousness;

    [ScriptMethod]
    public static float GetBrainHealth() => Body.brainHealth;

    [ScriptMethod(Name = "GetHappiness")]
    public static float GetTotalHappiness() => Body.totalHappiness;

    [ScriptMethod]
    public static float GetBloodViscosity() => Body.bloodViscosity;

    [ScriptMethod]
    public static float GetBloodVesselSize() => Body.bloodVesselSize;

    [ScriptMethod]
    public static float GetFibrillationProgress() => Body.fibrillationProgress;

    [ScriptMethod]
    public static float GetAdrenaline() => Body.adrenaline;

    [ScriptMethod]
    public static float GetCurAdrenaline() => Body.curAdrenaline;

    [ScriptMethod]
    public static float GetSepticShock() => Body.septicShock;

    [ScriptMethod]
    public static float GetSicknessAmount() => Body.sicknessAmount;

    [ScriptMethod]
    public static float GetVenomTotal() => Body.venomTotal;

    [ScriptMethod]
    public static float GetVenomCurrent() => Body.venomCurrent;

    [ScriptMethod]
    public static float GetInternalBleeding() => Body.internalBleeding;

    [ScriptMethod]
    public static float GetHemothorax() => Body.hemothorax;

    [ScriptMethod]
    public static float GetShock() => Body.shock;

    [ScriptMethod]
    public static float GetPainShock() => Body.painShock;

    [ScriptMethod]
    public static float GetTraumaAmount() => Body.traumaAmount;

    [ScriptMethod]
    public static float GetRadiationSickness() => Body.radiationSickness;

    [ScriptMethod]
    public static float GetStrokeAmount() => Body.strokeAmount;

    [ScriptMethod]
    public static float GetBadSleepAmount() => Body.badSleepAmount;

    [ScriptMethod]
    public static float GetGoodSleepTime() => Body.goodSleepTime;

    [ScriptMethod]
    public static float GetLastStandTime() => Body.lastStandTime;

    [ScriptMethod]
    public static float GetAntibioticImmunityTime() => Body.antibioticImmunityTime;

    // -- Getters（HealAll 中使用的外部字段） --
    [ScriptMethod]
    public static float GetDirtyness() => Body.dirtyness;

    [ScriptMethod]
    public static float GetWetness() => Body.wetness;

    [ScriptMethod]
    public static float GetHearingLoss() => Body.hearingLoss;

    [ScriptMethod]
    public static float GetSnowAmount() => Body.snowAmount;

    [ScriptMethod]
    public static float GetRawHappiness() => Body.happiness;

    // -- Setters（基础字段） --

    [ScriptMethod]
    public static void SetHunger(float value)
    {
        if (Body is { } b) b.hunger = Mathf.Clamp(value, -50f, 125f);
    }

    [ScriptMethod]
    public static void SetThirst(float value)
    {
        if (Body is { } b) b.thirst = Mathf.Clamp(value, -50f, 250f);
    }

    [ScriptMethod]
    public static void SetStamina(float value)
    {
        if (Body is { } b) b.stamina = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetEnergy(float value)
    {
        if (Body is { } b) b.energy = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetBloodVolume(float value)
    {
        if (Body is { } b) b.bloodVolume = Mathf.Clamp(value, -100f, 200f);
    }

    [ScriptMethod]
    public static void SetBloodOxygen(float value)
    {
        if (Body is { } b) b.bloodOxygen = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetHeartRate(float value)
    {
        if (Body is { } b) b.heartRate = Mathf.Clamp(value, 0f, 300f);
    }

    [ScriptMethod]
    public static void SetBloodPressure(float value)
    {
        if (Body is { } b) b.bloodPressure = Mathf.Clamp(value, 0f, 250f);
    }

    [ScriptMethod]
    public static void SetTemperature(float value)
    {
        if (Body is { } b) b.temperature = Mathf.Clamp(value, 20f, 50f);
    }

    [ScriptMethod]
    public static void SetConsciousness(float value)
    {
        if (Body is { } b) b.consciousness = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetBrainHealth(float value)
    {
        if (Body is { } b) b.brainHealth = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetRadiationSickness(float value)
    {
        if (Body is { } b) b.radiationSickness = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetTraumaAmount(float value)
    {
        if (Body is { } b) b.traumaAmount = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetInternalBleeding(float value)
    {
        if (Body is { } b) b.internalBleeding = Mathf.Clamp(value, 0f, 100f);
    }

    // -- Setters（补全字段） --
    [ScriptMethod]
    public static void SetFocusedLevel(float value)
    {
        if (Body is { } b) b.focusedLevel = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetHorrifiedLevel(float value)
    {
        if (Body is { } b) b.horrifiedLevel = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetClawHealth(float value)
    {
        if (Body is { } b) b.clawHealth = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetWeightOffset(float value)
    {
        if (Body is { } b) b.weightOffset = value;
    }

    [ScriptMethod]
    public static void SetRespiratoryRate(float value)
    {
        if (Body is { } b) b.respiratoryRate = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetBloodViscosity(float value)
    {
        if (Body is { } b) b.bloodViscosity = Mathf.Clamp(value, -100f, 100f);
    }

    [ScriptMethod]
    public static void SetBloodVesselSize(float value)
    {
        if (Body is { } b) b.bloodVesselSize = Mathf.Clamp(value, 0f, 2f);
    }

    [ScriptMethod]
    public static void SetFibrillationProgress(float value)
    {
        if (Body is { } b) b.fibrillationProgress = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetAdrenaline(float value)
    {
        if (Body is { } b) b.adrenaline = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetCurAdrenaline(float value)
    {
        if (Body is { } b) b.curAdrenaline = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetSepticShock(float value)
    {
        if (Body is { } b) b.septicShock = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetSicknessAmount(float value)
    {
        if (Body is { } b) b.sicknessAmount = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetVenomTotal(float value)
    {
        if (Body is { } b) b.venomTotal = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetVenomCurrent(float value)
    {
        if (Body is { } b) b.venomCurrent = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetHemothorax(float value)
    {
        if (Body is { } b) b.hemothorax = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetShock(float value)
    {
        if (Body is { } b) b.shock = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetPainShock(float value)
    {
        if (Body is { } b) b.painShock = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetStrokeAmount(float value)
    {
        if (Body is { } b) b.strokeAmount = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetBadSleepAmount(float value)
    {
        if (Body is { } b) b.badSleepAmount = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetGoodSleepTime(float value)
    {
        if (Body is { } b) b.goodSleepTime = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetLastStandTime(float value)
    {
        if (Body is { } b) b.lastStandTime = Mathf.Clamp(value, 0f, float.MaxValue);
    }

    [ScriptMethod]
    public static void SetAntibioticImmunityTime(float value)
    {
        if (Body is { } b) b.antibioticImmunityTime = Mathf.Clamp(value, 0f, float.MaxValue);
    }

    // -- Setters（HealAll 中使用的外部字段） --
    [ScriptMethod]
    public static void SetDirtyness(float value)
    {
        if (Body is { } b) b.dirtyness = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetWetness(float value)
    {
        if (Body is { } b) b.wetness = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetHearingLoss(float value)
    {
        if (Body is { } b) b.hearingLoss = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetSnowAmount(float value)
    {
        if (Body is { } b) b.snowAmount = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetRawHappiness(float value)
    {
        if (Body is { } b) b.happiness = Mathf.Clamp(value, -100f, 100f);
    }

    [ScriptMethod]
    public static bool HasPainkillers() => Body.GetComponent<Painkillers>() != null;

    [ScriptMethod]
    public static bool HasAntidepressants() => Body.GetComponent<Antidepressants>() != null;

    [ScriptMethod]
    public static bool HasSleepingPills() => Body.GetComponent<SleepingPills>() != null;

    [ScriptMethod]
    public static float GetOpiateHappiness() => Body.opiateHappiness;

    [ScriptMethod]
    public static float GetAntidepressantHappiness() => Body.antidepressantHappiness;

    [ScriptMethod]
    public static float GetCaffeinated() => Body.caffeinated;

    [ScriptMethod]
    public static void RemovePainkillers()
    {
        if (Body is { } body && body.TryGetComponent<Painkillers>(out var c)) Object.Destroy(c);
    }

    [ScriptMethod]
    public static void RemoveAntidepressants()
    {
        if (Body is { } body && body.TryGetComponent<Antidepressants>(out var c)) Object.Destroy(c);
    }

    [ScriptMethod]
    public static void RemoveSleepingPills()
    {
        if (Body is { } body && body.TryGetComponent<SleepingPills>(out var c)) Object.Destroy(c);
    }

    [ScriptMethod]
    public static void SetOpiateHappiness(float value)
    {
        if (Body is { } b) b.opiateHappiness = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetAntidepressantHappiness(float value)
    {
        if (Body is { } b) b.antidepressantHappiness = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetCaffeinated(float value)
    {
        if (Body is { } b) b.caffeinated = Mathf.Clamp(value, 0f, 100f);
    }

    // -- 心理 --
    [ScriptMethod]
    public static int GetCorpsesSeen() => Body.corpsesSeen;

    [ScriptMethod]
    public static void SetCorpsesSeen(int count)
    {
        if (Body is { } b) b.corpsesSeen = Mathf.Max(0, count);
    }

    [ScriptMethod]
    public static void Feed(float amount)
    {
        if (Body is { } b) b.hunger = Mathf.Clamp(b.hunger + amount, -100f, 100f);
    }

    [ScriptMethod]
    public static void Hydrate(float amount)
    {
        if (Body is { } b) b.thirst = Mathf.Clamp(b.thirst + amount, 0f, 200f);
    }

    [ScriptMethod]
    public static void RestoreStamina(float amount)
    {
        if (Body is { } b) b.stamina = Mathf.Clamp(b.stamina + amount, 0f, 100f);
    }

    [ScriptMethod]
    public static void RestoreEnergy(float amount)
    {
        if (Body is { } b) b.energy = Mathf.Clamp(b.energy + amount, 0f, 100f);
    }

    [ScriptMethod]
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