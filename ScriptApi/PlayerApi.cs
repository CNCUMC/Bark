using Bark.Tool;

namespace Bark.ScriptApi;

// 玩家操作 API（通过 bark.player 访问）
public class PlayerApi
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

    public bool IsSleeping()
    {
        return BodyUtil.Status.IsSleeping();
    }

    public bool IsInWater()
    {
        return BodyUtil.Status.IsInWater();
    }

    public bool IsStanding()
    {
        return BodyUtil.Status.IsStanding();
    }

    public bool IsCrouching()
    {
        return BodyUtil.Status.IsCrouching();
    }

    // -- Vitals - Getters --

    public float GetHeartRate()
    {
        return BodyUtil.Vitals.GetHeartRate();
    }

    public float GetBloodPressure()
    {
        return BodyUtil.Vitals.GetBloodPressure();
    }

    public float GetBloodOxygen()
    {
        return BodyUtil.Vitals.GetBloodOxygen();
    }

    public float GetBloodVolume()
    {
        return BodyUtil.Vitals.GetBloodVolume();
    }

    public float GetTemperature()
    {
        return BodyUtil.Vitals.GetTemperature();
    }

    public float GetConsciousness()
    {
        return BodyUtil.Vitals.GetConsciousness();
    }

    public float GetBrainHealth()
    {
        return BodyUtil.Vitals.GetBrainHealth();
    }

    public float GetRespiratoryRate()
    {
        return BodyUtil.Vitals.GetRespiratoryRate();
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

    public float GetHappiness()
    {
        return BodyUtil.Vitals.GetTotalHappiness();
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

    public void SetBloodVolume(float value)
    {
        BodyUtil.Vitals.SetBloodVolume(value);
    }

    public void SetBloodOxygen(float value)
    {
        BodyUtil.Vitals.SetBloodOxygen(value);
    }

    public void SetHappiness(float value)
    {
        BodyUtil.Vitals.SetRawHappiness(value);
    }
    
    public void Teleport(float x, float y)
    {
        PlayerUtil.Teleport(x, y);
    }
    
    public void PickItem(string itemId, int slot)
    {
        PlayerUtil.PickUpItem(itemId, slot);
    }
    
    public void Alert(string text, bool important)
    {
        PlayerUtil.Alert(text, important);
    }
    
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
