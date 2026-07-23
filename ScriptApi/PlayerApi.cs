using Bark.Tool;

namespace Bark.ScriptApi;

// 玩家操作 API（通过 bark.player 访问）
public class PlayerApi
{
    public bool IsAlive()
    {
        return PlayerUtil.IsAlive();
    }

    public bool IsConscious()
    {
        return PlayerUtil.IsConscious();
    }

    public bool IsDying()
    {
        return PlayerUtil.IsDying();
    }

    public bool IsSleeping()
    {
        return PlayerUtil.IsSleeping();
    }

    public bool IsInWater()
    {
        return PlayerUtil.IsInWater();
    }

    public bool IsStanding()
    {
        return PlayerUtil.IsStanding();
    }

    public bool IsCrouching()
    {
        return PlayerUtil.IsCrouching();
    }

    public float GetHeartRate()
    {
        return PlayerUtil.GetHeartRate();
    }

    public float GetBloodPressure()
    {
        return PlayerUtil.GetBloodPressure();
    }

    public float GetBloodOxygen()
    {
        return PlayerUtil.GetBloodOxygen();
    }

    public float GetBloodVolume()
    {
        return PlayerUtil.GetBloodVolume();
    }

    public float GetTemperature()
    {
        return PlayerUtil.GetTemperature();
    }

    public float GetConsciousness()
    {
        return PlayerUtil.GetConsciousness();
    }

    public float GetBrainHealth()
    {
        return PlayerUtil.GetBrainHealth();
    }

    public float GetRespiratoryRate()
    {
        return PlayerUtil.GetRespiratoryRate();
    }

    public float GetHunger()
    {
        return PlayerUtil.GetHunger();
    }

    public float GetThirst()
    {
        return PlayerUtil.GetThirst();
    }

    public float GetStamina()
    {
        return PlayerUtil.GetStamina();
    }

    public float GetEnergy()
    {
        return PlayerUtil.GetEnergy();
    }

    public float GetHappiness()
    {
        return PlayerUtil.GetHappiness();
    }

    public void Feed(float amount)
    {
        PlayerUtil.Feed(amount);
    }

    public void Hydrate(float amount)
    {
        PlayerUtil.Hydrate(amount);
    }

    public void RestoreStamina(float amount)
    {
        PlayerUtil.RestoreStamina(amount);
    }

    public void RestoreEnergy(float amount)
    {
        PlayerUtil.RestoreEnergy(amount);
    }

    public void SetHunger(float value)
    {
        PlayerUtil.SetHunger(value);
    }

    public void SetThirst(float value)
    {
        PlayerUtil.SetThirst(value);
    }

    public void SetStamina(float value)
    {
        PlayerUtil.SetStamina(value);
    }

    public void SetEnergy(float value)
    {
        PlayerUtil.SetEnergy(value);
    }

    public void SetHeartRate(float value)
    {
        PlayerUtil.SetHeartRate(value);
    }

    public void SetBloodPressure(float value)
    {
        PlayerUtil.SetBloodPressure(value);
    }

    public void SetTemperature(float value)
    {
        PlayerUtil.SetTemperature(value);
    }

    public void SetConsciousness(float value)
    {
        PlayerUtil.SetConsciousness(value);
    }

    public void SetBrainHealth(float value)
    {
        PlayerUtil.SetBrainHealth(value);
    }

    public void SetBloodVolume(float value)
    {
        PlayerUtil.SetBloodVolume(value);
    }

    public void SetBloodOxygen(float value)
    {
        PlayerUtil.SetBloodOxygen(value);
    }

    public void SetHappiness(float value)
    {
        PlayerUtil.SetHappiness(value);
    }

    public void Teleport(float x, float y)
    {
        PlayerUtil.Tp(x, y);
    }

    public void PickItem(string itemId, int slot)
    {
        PlayerUtil.PickItem(itemId, slot);
    }

    public void Alert(string text, bool important)
    {
        PlayerUtil.Alert(text, important);
    }

    public void HealAll()
    {
        PlayerUtil.HealAll();
    }
}