using Bark.Tool;

namespace Bark.ScriptApi;

// 身体 API（通过 bark.body 访问）
public class BodyApi
{
    // -- Status --

    public bool IsAlive()
    {
        return BodyUtil.Status.IsAlive();
    }

    public bool IsConscious()
    {
        return BodyUtil.Status.IsConscious();
    }

    public bool IsDying()
    {
        return BodyUtil.Status.IsDying();
    }

    public bool IsCriticallyDying()
    {
        return BodyUtil.Status.IsCriticallyDying();
    }

    public bool IsInCardiacArrest()
    {
        return BodyUtil.Status.IsInCardiacArrest();
    }

    public bool IsSleeping()
    {
        return BodyUtil.Status.IsSleeping();
    }

    public bool IsExercising()
    {
        return BodyUtil.Status.IsExercising();
    }

    public bool IsBreathing()
    {
        return BodyUtil.Status.IsBreathing();
    }

    public bool IsInWater()
    {
        return BodyUtil.Status.IsInWater();
    }

    public bool HasScubaGear()
    {
        return BodyUtil.Status.HasScubaGear();
    }

    public bool IsStanding()
    {
        return BodyUtil.Status.IsStanding();
    }

    public bool IsCrouching()
    {
        return BodyUtil.Status.IsCrouching();
    }

    public bool IsOnHardStimulants()
    {
        return BodyUtil.Status.IsOnHardStimulants();
    }

    public bool UsedNeuralBooster()
    {
        return BodyUtil.Status.UsedNeuralBooster();
    }

    public bool IsUsingSleepingBag()
    {
        return BodyUtil.Status.IsUsingSleepingBag();
    }

    public bool IsBothHandsUnusable()
    {
        return BodyUtil.Status.IsBothHandsUnusable();
    }

    public bool AllowUseItem()
    {
        return BodyUtil.Status.AllowUseItem();
    }

    public bool HasPulmonaryEmbolism()
    {
        return BodyUtil.Status.HasPulmonaryEmbolism();
    }

    public bool IsFibrillationForced()
    {
        return BodyUtil.Status.IsFibrillationForced();
    }

    public bool CanTakeNap()
    {
        return BodyUtil.Status.CanTakeNap();
    }

    public bool IsAboveMedicalCutoff()
    {
        return BodyUtil.Status.IsAboveMedicalCutoff();
    }

    public bool IsDisfigured()
    {
        return BodyUtil.Status.IsDisfigured();
    }

    public bool IsEyeGone()
    {
        return BodyUtil.Status.IsEyeGone();
    }

    public bool IsBothEyesGone()
    {
        return BodyUtil.Status.IsBothEyesGone();
    }

    public bool IsMindWiped()
    {
        return BodyUtil.Status.IsMindWiped();
    }

    public bool TriedRollingLastStand()
    {
        return BodyUtil.Status.TriedRollingLastStand();
    }

    public bool SuccessfullyRolledLastStand()
    {
        return BodyUtil.Status.SuccessfullyRolledLastStand();
    }

    // -- Vitals - Getters --

    public float GetFocusedLevel()
    {
        return BodyUtil.Vitals.GetFocusedLevel();
    }

    public float GetHorrifiedLevel()
    {
        return BodyUtil.Vitals.GetHorrifiedLevel();
    }

    public float GetClawHealth()
    {
        return BodyUtil.Vitals.GetClawHealth();
    }

    public float GetWeightOffset()
    {
        return BodyUtil.Vitals.GetWeightOffset();
    }

    public float GetBloodOxygen()
    {
        return BodyUtil.Vitals.GetBloodOxygen();
    }

    public float GetBloodVolume()
    {
        return BodyUtil.Vitals.GetBloodVolume();
    }

    public float GetHeartRate()
    {
        return BodyUtil.Vitals.GetHeartRate();
    }

    public float GetBloodPressure()
    {
        return BodyUtil.Vitals.GetBloodPressure();
    }

    public float GetRespiratoryRate()
    {
        return BodyUtil.Vitals.GetRespiratoryRate();
    }

    public float GetTemperature()
    {
        return BodyUtil.Vitals.GetTemperature();
    }

    public float GetHunger()
    {
        return BodyUtil.Vitals.GetHunger();
    }

    public float GetThirst()
    {
        return BodyUtil.Vitals.GetThirst();
    }

    public float GetStamina()
    {
        return BodyUtil.Vitals.GetStamina();
    }

    public float GetEnergy()
    {
        return BodyUtil.Vitals.GetEnergy();
    }

    public float GetConsciousness()
    {
        return BodyUtil.Vitals.GetConsciousness();
    }

    public float GetBrainHealth()
    {
        return BodyUtil.Vitals.GetBrainHealth();
    }

    public float GetTotalHappiness()
    {
        return BodyUtil.Vitals.GetTotalHappiness();
    }

    public float GetBloodViscosity()
    {
        return BodyUtil.Vitals.GetBloodViscosity();
    }

    public float GetBloodVesselSize()
    {
        return BodyUtil.Vitals.GetBloodVesselSize();
    }

    public float GetFibrillationProgress()
    {
        return BodyUtil.Vitals.GetFibrillationProgress();
    }

    public float GetAdrenaline()
    {
        return BodyUtil.Vitals.GetAdrenaline();
    }

    public float GetCurAdrenaline()
    {
        return BodyUtil.Vitals.GetCurAdrenaline();
    }

    public float GetSepticShock()
    {
        return BodyUtil.Vitals.GetSepticShock();
    }

    public float GetSicknessAmount()
    {
        return BodyUtil.Vitals.GetSicknessAmount();
    }

    public float GetVenomTotal()
    {
        return BodyUtil.Vitals.GetVenomTotal();
    }

    public float GetVenomCurrent()
    {
        return BodyUtil.Vitals.GetVenomCurrent();
    }

    public float GetInternalBleeding()
    {
        return BodyUtil.Vitals.GetInternalBleeding();
    }

    public float GetHemothorax()
    {
        return BodyUtil.Vitals.GetHemothorax();
    }

    public float GetShock()
    {
        return BodyUtil.Vitals.GetShock();
    }

    public float GetPainShock()
    {
        return BodyUtil.Vitals.GetPainShock();
    }

    public float GetTraumaAmount()
    {
        return BodyUtil.Vitals.GetTraumaAmount();
    }

    public float GetRadiationSickness()
    {
        return BodyUtil.Vitals.GetRadiationSickness();
    }

    public float GetStrokeAmount()
    {
        return BodyUtil.Vitals.GetStrokeAmount();
    }

    public float GetBadSleepAmount()
    {
        return BodyUtil.Vitals.GetBadSleepAmount();
    }

    public float GetGoodSleepTime()
    {
        return BodyUtil.Vitals.GetGoodSleepTime();
    }

    public float GetLastStandTime()
    {
        return BodyUtil.Vitals.GetLastStandTime();
    }

    public float GetAntibioticImmunityTime()
    {
        return BodyUtil.Vitals.GetAntibioticImmunityTime();
    }

    public float GetDirtyness()
    {
        return BodyUtil.Vitals.GetDirtyness();
    }

    public float GetWetness()
    {
        return BodyUtil.Vitals.GetWetness();
    }

    public float GetHearingLoss()
    {
        return BodyUtil.Vitals.GetHearingLoss();
    }

    public float GetSnowAmount()
    {
        return BodyUtil.Vitals.GetSnowAmount();
    }

    public float GetRawHappiness()
    {
        return BodyUtil.Vitals.GetRawHappiness();
    }

    // -- Vitals - Setters --

    public void SetHunger(float value)
    {
        BodyUtil.Vitals.SetHunger(value);
    }

    public void SetThirst(float value)
    {
        BodyUtil.Vitals.SetThirst(value);
    }

    public void SetStamina(float value)
    {
        BodyUtil.Vitals.SetStamina(value);
    }

    public void SetEnergy(float value)
    {
        BodyUtil.Vitals.SetEnergy(value);
    }

    public void SetBloodVolume(float value)
    {
        BodyUtil.Vitals.SetBloodVolume(value);
    }

    public void SetBloodOxygen(float value)
    {
        BodyUtil.Vitals.SetBloodOxygen(value);
    }

    public void SetHeartRate(float value)
    {
        BodyUtil.Vitals.SetHeartRate(value);
    }

    public void SetBloodPressure(float value)
    {
        BodyUtil.Vitals.SetBloodPressure(value);
    }

    public void SetTemperature(float value)
    {
        BodyUtil.Vitals.SetTemperature(value);
    }

    public void SetConsciousness(float value)
    {
        BodyUtil.Vitals.SetConsciousness(value);
    }

    public void SetBrainHealth(float value)
    {
        BodyUtil.Vitals.SetBrainHealth(value);
    }

    public void SetRadiationSickness(float value)
    {
        BodyUtil.Vitals.SetRadiationSickness(value);
    }

    public void SetTraumaAmount(float value)
    {
        BodyUtil.Vitals.SetTraumaAmount(value);
    }

    public void SetInternalBleeding(float value)
    {
        BodyUtil.Vitals.SetInternalBleeding(value);
    }

    public void SetFocusedLevel(float value)
    {
        BodyUtil.Vitals.SetFocusedLevel(value);
    }

    public void SetHorrifiedLevel(float value)
    {
        BodyUtil.Vitals.SetHorrifiedLevel(value);
    }

    public void SetClawHealth(float value)
    {
        BodyUtil.Vitals.SetClawHealth(value);
    }

    public void SetWeightOffset(float value)
    {
        BodyUtil.Vitals.SetWeightOffset(value);
    }

    public void SetRespiratoryRate(float value)
    {
        BodyUtil.Vitals.SetRespiratoryRate(value);
    }

    public void SetBloodViscosity(float value)
    {
        BodyUtil.Vitals.SetBloodViscosity(value);
    }

    public void SetBloodVesselSize(float value)
    {
        BodyUtil.Vitals.SetBloodVesselSize(value);
    }

    public void SetFibrillationProgress(float value)
    {
        BodyUtil.Vitals.SetFibrillationProgress(value);
    }

    public void SetAdrenaline(float value)
    {
        BodyUtil.Vitals.SetAdrenaline(value);
    }

    public void SetCurAdrenaline(float value)
    {
        BodyUtil.Vitals.SetCurAdrenaline(value);
    }

    public void SetSepticShock(float value)
    {
        BodyUtil.Vitals.SetSepticShock(value);
    }

    public void SetSicknessAmount(float value)
    {
        BodyUtil.Vitals.SetSicknessAmount(value);
    }

    public void SetVenomTotal(float value)
    {
        BodyUtil.Vitals.SetVenomTotal(value);
    }

    public void SetVenomCurrent(float value)
    {
        BodyUtil.Vitals.SetVenomCurrent(value);
    }

    public void SetHemothorax(float value)
    {
        BodyUtil.Vitals.SetHemothorax(value);
    }

    public void SetShock(float value)
    {
        BodyUtil.Vitals.SetShock(value);
    }

    public void SetPainShock(float value)
    {
        BodyUtil.Vitals.SetPainShock(value);
    }

    public void SetStrokeAmount(float value)
    {
        BodyUtil.Vitals.SetStrokeAmount(value);
    }

    public void SetBadSleepAmount(float value)
    {
        BodyUtil.Vitals.SetBadSleepAmount(value);
    }

    public void SetGoodSleepTime(float value)
    {
        BodyUtil.Vitals.SetGoodSleepTime(value);
    }

    public void SetLastStandTime(float value)
    {
        BodyUtil.Vitals.SetLastStandTime(value);
    }

    public void SetAntibioticImmunityTime(float value)
    {
        BodyUtil.Vitals.SetAntibioticImmunityTime(value);
    }

    public void SetDirtyness(float value)
    {
        BodyUtil.Vitals.SetDirtyness(value);
    }

    public void SetWetness(float value)
    {
        BodyUtil.Vitals.SetWetness(value);
    }

    public void SetHearingLoss(float value)
    {
        BodyUtil.Vitals.SetHearingLoss(value);
    }

    public void SetSnowAmount(float value)
    {
        BodyUtil.Vitals.SetSnowAmount(value);
    }

    public void SetRawHappiness(float value)
    {
        BodyUtil.Vitals.SetRawHappiness(value);
    }

    // -- Drugs --

    public bool HasPainkillers()
    {
        return BodyUtil.Drugs.HasPainkillers();
    }

    public bool HasAntidepressants()
    {
        return BodyUtil.Drugs.HasAntidepressants();
    }

    public bool HasSleepingPills()
    {
        return BodyUtil.Drugs.HasSleepingPills();
    }

    public float GetOpiateHappiness()
    {
        return BodyUtil.Drugs.GetOpiateHappiness();
    }

    public float GetAntidepressantHappiness()
    {
        return BodyUtil.Drugs.GetAntidepressantHappiness();
    }

    public float GetCaffeinated()
    {
        return BodyUtil.Drugs.GetCaffeinated();
    }

    public void RemovePainkillers()
    {
        BodyUtil.Drugs.RemovePainkillers();
    }

    public void RemoveAntidepressants()
    {
        BodyUtil.Drugs.RemoveAntidepressants();
    }

    public void RemoveSleepingPills()
    {
        BodyUtil.Drugs.RemoveSleepingPills();
    }

    public void SetOpiateHappiness(float value)
    {
        BodyUtil.Drugs.SetOpiateHappiness(value);
    }

    public void SetAntidepressantHappiness(float value)
    {
        BodyUtil.Drugs.SetAntidepressantHappiness(value);
    }

    public void SetCaffeinated(float value)
    {
        BodyUtil.Drugs.SetCaffeinated(value);
    }

    public int GetCorpsesSeen()
    {
        return BodyUtil.Drugs.GetCorpsesSeen();
    }

    public void SetCorpsesSeen(int count)
    {
        BodyUtil.Drugs.SetCorpsesSeen(count);
    }

    // -- Recovery --

    public void Feed(float amount)
    {
        BodyUtil.Recovery.Feed(amount);
    }

    public void Hydrate(float amount)
    {
        BodyUtil.Recovery.Hydrate(amount);
    }

    public void RestoreStamina(float amount)
    {
        BodyUtil.Recovery.RestoreStamina(amount);
    }

    public void RestoreEnergy(float amount)
    {
        BodyUtil.Recovery.RestoreEnergy(amount);
    }

    public void HealAll()
    {
        BodyUtil.Recovery.HealAll();
    }
}