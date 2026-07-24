using Bark.ScriptApi;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Tool;

// 玩家身体：状态检测、生理数值、药物、恢复、医疗阈值
// 方法加 [ScriptMethod] 后自动暴露给 Lua/JS 脚本
public static class BodyUtil
{
    // 保留公开属性以兼容外部调用，内部逻辑使用 GetBody() 处理空安全
    public static Body Body => PlayerCamera.main.body!;

    // ============================================================
    // 内部辅助 - 统一 Body 获取入口
    // ============================================================

    private static Body? GetBody()
    {
        return PlayerCamera.main?.body;
    }

    // ============================================================
    // 状态检测 - bool 查询（Body 为 null 时统一返回 false）
    // ============================================================

    [ScriptMethod]
    public static bool IsAlive()
    {
        return GetBody() is { alive: true };
    }

    [ScriptMethod]
    public static bool IsConscious()
    {
        return GetBody() is { conscious: true };
    }

    [ScriptMethod]
    public static bool IsDying()
    {
        return GetBody() is { isDying: true };
    }

    [ScriptMethod]
    public static bool IsCriticallyDying()
    {
        return GetBody() is { isCriticallyDying: true };
    }

    [ScriptMethod]
    public static bool IsInCardiacArrest()
    {
        return GetBody() is { inCardiacArrest: true };
    }

    [ScriptMethod]
    public static bool IsSleeping()
    {
        return GetBody() is { sleeping: true };
    }

    [ScriptMethod]
    public static bool IsExercising()
    {
        return GetBody() is { exercising: true };
    }

    [ScriptMethod]
    public static bool IsBreathing()
    {
        return GetBody() is { breathing: true };
    }

    [ScriptMethod]
    public static bool IsInWater()
    {
        return GetBody() is { inWater: true };
    }

    [ScriptMethod]
    public static bool HasScubaGear()
    {
        return GetBody() is { hasScubaGear: true };
    }

    [ScriptMethod]
    public static bool IsStanding()
    {
        return GetBody() is { standing: true };
    }

    [ScriptMethod]
    public static bool IsCrouching()
    {
        return GetBody() is { crouching: true };
    }

    [ScriptMethod]
    public static bool IsOnHardStimulants()
    {
        return GetBody() is { onHardStimulants: true };
    }

    [ScriptMethod]
    public static bool UsedNeuralBooster()
    {
        return GetBody() is { usedNeuralBooster: true };
    }

    [ScriptMethod]
    public static bool IsUsingSleepingBag()
    {
        return GetBody() is { usingSleepingBag: true };
    }

    [ScriptMethod]
    public static bool IsBothHandsUnusable()
    {
        return GetBody() is { bothHandsUnusable: true };
    }

    [ScriptMethod]
    public static bool AllowUseItem()
    {
        return GetBody() is { allowUseItem: true };
    }

    [ScriptMethod]
    public static bool HasPulmonaryEmbolism()
    {
        return GetBody() is { hasPulmonaryEmbolism: true };
    }

    [ScriptMethod]
    public static bool IsFibrillationForced()
    {
        return GetBody() is { fibrillationForced: true };
    }

    [ScriptMethod]
    public static bool CanTakeNap()
    {
        return GetBody() is { canTakeNap: true };
    }

    [ScriptMethod]
    public static bool IsAboveMedicalCutoff()
    {
        return GetBody() is { aboveMedicalCutoff: true };
    }

    [ScriptMethod]
    public static bool IsDisfigured()
    {
        return GetBody() is { disfigured: true };
    }

    [ScriptMethod]
    public static bool IsEyeGone()
    {
        return GetBody() is { eyeGone: true };
    }

    [ScriptMethod]
    public static bool IsBothEyesGone()
    {
        return GetBody() is { bothEyesGone: true };
    }

    [ScriptMethod]
    public static bool IsMindWiped()
    {
        return GetBody()?.mindWipe != null;
    }

    [ScriptMethod]
    public static bool TriedRollingLastStand()
    {
        return GetBody() is { triedRollingLastStand: true };
    }

    [ScriptMethod]
    public static bool SuccessfullyRolledLastStand()
    {
        return GetBody() is { succesfullyRolledLastStand: true };
    }

    // ============================================================
    // 生理数值查询 - float getter（Body 为 null 时返回 0f）
    // ============================================================

    [ScriptMethod]
    public static float GetFocusedLevel()
    {
        return GetBody()?.focusedLevel ?? 0f;
    }

    [ScriptMethod]
    public static float GetHorrifiedLevel()
    {
        return GetBody()?.horrifiedLevel ?? 0f;
    }

    [ScriptMethod]
    public static float GetClawHealth()
    {
        return GetBody()?.clawHealth ?? 0f;
    }

    [ScriptMethod]
    public static float GetWeightOffset()
    {
        return GetBody()?.weightOffset ?? 0f;
    }

    [ScriptMethod]
    public static float GetBloodOxygen()
    {
        return GetBody()?.bloodOxygen ?? 0f;
    }

    [ScriptMethod]
    public static float GetBloodVolume()
    {
        return GetBody()?.bloodVolume ?? 0f;
    }

    [ScriptMethod]
    public static float GetHeartRate()
    {
        return GetBody()?.heartRate ?? 0f;
    }

    [ScriptMethod]
    public static float GetBloodPressure()
    {
        return GetBody()?.bloodPressure ?? 0f;
    }

    [ScriptMethod]
    public static float GetRespiratoryRate()
    {
        return GetBody()?.respiratoryRate ?? 0f;
    }

    [ScriptMethod]
    public static float GetTemperature()
    {
        return GetBody()?.temperature ?? 0f;
    }

    [ScriptMethod]
    public static float GetHunger()
    {
        return GetBody()?.hunger ?? 0f;
    }

    [ScriptMethod]
    public static float GetThirst()
    {
        return GetBody()?.thirst ?? 0f;
    }

    [ScriptMethod]
    public static float GetStamina()
    {
        return GetBody()?.stamina ?? 0f;
    }

    [ScriptMethod]
    public static float GetEnergy()
    {
        return GetBody()?.energy ?? 0f;
    }

    [ScriptMethod]
    public static float GetConsciousness()
    {
        return GetBody()?.consciousness ?? 0f;
    }

    [ScriptMethod]
    public static float GetBrainHealth()
    {
        return GetBody()?.brainHealth ?? 0f;
    }

    [ScriptMethod(Name = "GetHappiness")]
    public static float GetTotalHappiness()
    {
        return GetBody()?.totalHappiness ?? 0f;
    }

    [ScriptMethod]
    public static float GetBloodViscosity()
    {
        return GetBody()?.bloodViscosity ?? 0f;
    }

    [ScriptMethod]
    public static float GetBloodVesselSize()
    {
        return GetBody()?.bloodVesselSize ?? 0f;
    }

    [ScriptMethod]
    public static float GetFibrillationProgress()
    {
        return GetBody()?.fibrillationProgress ?? 0f;
    }

    [ScriptMethod]
    public static float GetAdrenaline()
    {
        return GetBody()?.adrenaline ?? 0f;
    }

    [ScriptMethod]
    public static float GetCurAdrenaline()
    {
        return GetBody()?.curAdrenaline ?? 0f;
    }

    [ScriptMethod]
    public static float GetSepticShock()
    {
        return GetBody()?.septicShock ?? 0f;
    }

    [ScriptMethod]
    public static float GetSicknessAmount()
    {
        return GetBody()?.sicknessAmount ?? 0f;
    }

    [ScriptMethod]
    public static float GetVenomTotal()
    {
        return GetBody()?.venomTotal ?? 0f;
    }

    [ScriptMethod]
    public static float GetVenomCurrent()
    {
        return GetBody()?.venomCurrent ?? 0f;
    }

    [ScriptMethod]
    public static float GetInternalBleeding()
    {
        return GetBody()?.internalBleeding ?? 0f;
    }

    [ScriptMethod]
    public static float GetHemothorax()
    {
        return GetBody()?.hemothorax ?? 0f;
    }

    [ScriptMethod]
    public static float GetShock()
    {
        return GetBody()?.shock ?? 0f;
    }

    [ScriptMethod]
    public static float GetPainShock()
    {
        return GetBody()?.painShock ?? 0f;
    }

    [ScriptMethod]
    public static float GetTraumaAmount()
    {
        return GetBody()?.traumaAmount ?? 0f;
    }

    [ScriptMethod]
    public static float GetRadiationSickness()
    {
        return GetBody()?.radiationSickness ?? 0f;
    }

    [ScriptMethod]
    public static float GetStrokeAmount()
    {
        return GetBody()?.strokeAmount ?? 0f;
    }

    [ScriptMethod]
    public static float GetBadSleepAmount()
    {
        return GetBody()?.badSleepAmount ?? 0f;
    }

    [ScriptMethod]
    public static float GetGoodSleepTime()
    {
        return GetBody()?.goodSleepTime ?? 0f;
    }

    [ScriptMethod]
    public static float GetLastStandTime()
    {
        return GetBody()?.lastStandTime ?? 0f;
    }

    [ScriptMethod]
    public static float GetAntibioticImmunityTime()
    {
        return GetBody()?.antibioticImmunityTime ?? 0f;
    }

    [ScriptMethod]
    public static float GetDirtyness()
    {
        return GetBody()?.dirtyness ?? 0f;
    }

    [ScriptMethod]
    public static float GetWetness()
    {
        return GetBody()?.wetness ?? 0f;
    }

    [ScriptMethod]
    public static float GetHearingLoss()
    {
        return GetBody()?.hearingLoss ?? 0f;
    }

    [ScriptMethod]
    public static float GetSnowAmount()
    {
        return GetBody()?.snowAmount ?? 0f;
    }

    [ScriptMethod]
    public static float GetRawHappiness()
    {
        return GetBody()?.happiness ?? 0f;
    }

    // ============================================================
    // 基础生理数值设置 - 核心字段 Setter
    // ============================================================

    [ScriptMethod]
    public static void SetHunger(float value)
    {
        if (GetBody() is { } b) b.hunger = Mathf.Clamp(value, -50f, 125f);
    }

    [ScriptMethod]
    public static void SetThirst(float value)
    {
        if (GetBody() is { } b) b.thirst = Mathf.Clamp(value, -50f, 250f);
    }

    [ScriptMethod]
    public static void SetStamina(float value)
    {
        if (GetBody() is { } b) b.stamina = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetEnergy(float value)
    {
        if (GetBody() is { } b) b.energy = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetBloodVolume(float value)
    {
        if (GetBody() is { } b) b.bloodVolume = Mathf.Clamp(value, -100f, 200f);
    }

    [ScriptMethod]
    public static void SetBloodOxygen(float value)
    {
        if (GetBody() is { } b) b.bloodOxygen = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetHeartRate(float value)
    {
        if (GetBody() is { } b) b.heartRate = Mathf.Clamp(value, 0f, 300f);
    }

    [ScriptMethod]
    public static void SetBloodPressure(float value)
    {
        if (GetBody() is { } b) b.bloodPressure = Mathf.Clamp(value, 0f, 250f);
    }

    [ScriptMethod]
    public static void SetTemperature(float value)
    {
        if (GetBody() is { } b) b.temperature = Mathf.Clamp(value, 20f, 50f);
    }

    [ScriptMethod]
    public static void SetConsciousness(float value)
    {
        if (GetBody() is { } b) b.consciousness = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetBrainHealth(float value)
    {
        if (GetBody() is { } b) b.brainHealth = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetRadiationSickness(float value)
    {
        if (GetBody() is { } b) b.radiationSickness = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetTraumaAmount(float value)
    {
        if (GetBody() is { } b) b.traumaAmount = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetInternalBleeding(float value)
    {
        if (GetBody() is { } b) b.internalBleeding = Mathf.Clamp(value, 0f, 100f);
    }

    // ============================================================
    // 扩展数值设置 - 补全字段 Setter
    // ============================================================

    [ScriptMethod]
    public static void SetFocusedLevel(float value)
    {
        if (GetBody() is { } b) b.focusedLevel = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetHorrifiedLevel(float value)
    {
        if (GetBody() is { } b) b.horrifiedLevel = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetClawHealth(float value)
    {
        if (GetBody() is { } b) b.clawHealth = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetWeightOffset(float value)
    {
        if (GetBody() is { } b) b.weightOffset = value;
    }

    [ScriptMethod]
    public static void SetRespiratoryRate(float value)
    {
        if (GetBody() is { } b) b.respiratoryRate = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetBloodViscosity(float value)
    {
        if (GetBody() is { } b) b.bloodViscosity = Mathf.Clamp(value, -100f, 100f);
    }

    [ScriptMethod]
    public static void SetBloodVesselSize(float value)
    {
        if (GetBody() is { } b) b.bloodVesselSize = Mathf.Clamp(value, 0f, 2f);
    }

    [ScriptMethod]
    public static void SetFibrillationProgress(float value)
    {
        if (GetBody() is { } b) b.fibrillationProgress = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetAdrenaline(float value)
    {
        if (GetBody() is { } b) b.adrenaline = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetCurAdrenaline(float value)
    {
        if (GetBody() is { } b) b.curAdrenaline = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetSepticShock(float value)
    {
        if (GetBody() is { } b) b.septicShock = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetSicknessAmount(float value)
    {
        if (GetBody() is { } b) b.sicknessAmount = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetVenomTotal(float value)
    {
        if (GetBody() is { } b) b.venomTotal = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetVenomCurrent(float value)
    {
        if (GetBody() is { } b) b.venomCurrent = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetHemothorax(float value)
    {
        if (GetBody() is { } b) b.hemothorax = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetShock(float value)
    {
        if (GetBody() is { } b) b.shock = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetPainShock(float value)
    {
        if (GetBody() is { } b) b.painShock = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetStrokeAmount(float value)
    {
        if (GetBody() is { } b) b.strokeAmount = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetBadSleepAmount(float value)
    {
        if (GetBody() is { } b) b.badSleepAmount = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetGoodSleepTime(float value)
    {
        if (GetBody() is { } b) b.goodSleepTime = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetLastStandTime(float value)
    {
        if (GetBody() is { } b) b.lastStandTime = Mathf.Clamp(value, 0f, float.MaxValue);
    }

    [ScriptMethod]
    public static void SetAntibioticImmunityTime(float value)
    {
        if (GetBody() is { } b) b.antibioticImmunityTime = Mathf.Clamp(value, 0f, float.MaxValue);
    }

    // ============================================================
    // 卫生/环境数值设置
    // ============================================================

    [ScriptMethod]
    public static void SetDirtyness(float value)
    {
        if (GetBody() is { } b) b.dirtyness = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetWetness(float value)
    {
        if (GetBody() is { } b) b.wetness = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetHearingLoss(float value)
    {
        if (GetBody() is { } b) b.hearingLoss = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetSnowAmount(float value)
    {
        if (GetBody() is { } b) b.snowAmount = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetRawHappiness(float value)
    {
        if (GetBody() is { } b) b.happiness = Mathf.Clamp(value, -100f, 100f);
    }

    // ============================================================
    // 药物效果 - 查询、修改、移除
    // ============================================================

    [ScriptMethod]
    public static bool HasPainkillers()
    {
        return GetBody()?.GetComponent<Painkillers>() != null;
    }

    [ScriptMethod]
    public static bool HasAntidepressants()
    {
        return GetBody()?.GetComponent<Antidepressants>() != null;
    }

    [ScriptMethod]
    public static bool HasSleepingPills()
    {
        return GetBody()?.GetComponent<SleepingPills>() != null;
    }

    [ScriptMethod]
    public static float GetOpiateHappiness()
    {
        return GetBody()?.opiateHappiness ?? 0f;
    }

    [ScriptMethod]
    public static float GetAntidepressantHappiness()
    {
        return GetBody()?.antidepressantHappiness ?? 0f;
    }

    [ScriptMethod]
    public static float GetCaffeinated()
    {
        return GetBody()?.caffeinated ?? 0f;
    }

    [ScriptMethod]
    public static void RemovePainkillers()
    {
        if (GetBody() is { } body && body.TryGetComponent<Painkillers>(out var c)) Object.Destroy(c);
    }

    [ScriptMethod]
    public static void RemoveAntidepressants()
    {
        if (GetBody() is { } body && body.TryGetComponent<Antidepressants>(out var c)) Object.Destroy(c);
    }

    [ScriptMethod]
    public static void RemoveSleepingPills()
    {
        if (GetBody() is { } body && body.TryGetComponent<SleepingPills>(out var c)) Object.Destroy(c);
    }

    [ScriptMethod]
    public static void SetOpiateHappiness(float value)
    {
        if (GetBody() is { } b) b.opiateHappiness = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetAntidepressantHappiness(float value)
    {
        if (GetBody() is { } b) b.antidepressantHappiness = Mathf.Clamp(value, 0f, 100f);
    }

    [ScriptMethod]
    public static void SetCaffeinated(float value)
    {
        if (GetBody() is { } b) b.caffeinated = Mathf.Clamp(value, 0f, 100f);
    }

    // ============================================================
    // 心理状态
    // ============================================================

    [ScriptMethod]
    public static int GetCorpsesSeen()
    {
        return GetBody()?.corpsesSeen ?? 0;
    }

    [ScriptMethod]
    public static void SetCorpsesSeen(int count)
    {
        if (GetBody() is { } b) b.corpsesSeen = Mathf.Max(0, count);
    }

    // ============================================================
    // 复合操作 - 修改/恢复/治疗
    // ============================================================

    [ScriptMethod]
    public static void Feed(float amount)
    {
        if (GetBody() is { } b) b.hunger = Mathf.Clamp(b.hunger + amount, -50f, 125f);
    }

    [ScriptMethod]
    public static void Hydrate(float amount)
    {
        if (GetBody() is { } b) b.thirst = Mathf.Clamp(b.thirst + amount, -50f, 250f);
    }

    [ScriptMethod]
    public static void RestoreStamina(float amount)
    {
        if (GetBody() is { } b) b.stamina = Mathf.Clamp(b.stamina + amount, 0f, 100f);
    }

    [ScriptMethod]
    public static void RestoreEnergy(float amount)
    {
        if (GetBody() is { } b) b.energy = Mathf.Clamp(b.energy + amount, 0f, 100f);
    }

    [ScriptMethod]
    public static void HealAll()
    {
        if (GetBody() is not { } body) return;

        // 治疗所有肢体
        foreach (var limb in body.limbs)
        {
            if (limb == null) continue;
            limb.muscleHealth = limb.skinHealth = 100f;
            limb.boneHealTimer = limb.dislocationTimer = 0f;
            limb.infectionAmount = limb.bleedAmount = limb.pain = 0f;
            limb.shrapnel = 0;
            limb.infected = false;
        }

        // 恢复全身数值
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

        // 移除药物组件
        if (body.TryGetComponent<Painkillers>(out var pk)) Object.Destroy(pk);
        if (body.TryGetComponent<SleepingPills>(out var sp)) Object.Destroy(sp);
        if (body.TryGetComponent<Antidepressants>(out var ad)) Object.Destroy(ad);
    }
}