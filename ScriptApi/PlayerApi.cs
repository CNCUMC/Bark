using Bark.Tool;

namespace Bark.ScriptApi;

// 玩家操作 API（通过 bark.player 访问）
public class PlayerApi
{
    public bool IsAlive() => PlayerUtil.IsAlive();
    public bool IsConscious() => PlayerUtil.IsConscious();
    public bool IsDying() => PlayerUtil.IsDying();
    public bool IsSleeping() => PlayerUtil.IsSleeping();
    public bool IsInWater() => PlayerUtil.IsInWater();
    public bool IsStanding() => PlayerUtil.IsStanding();
    public bool IsCrouching() => PlayerUtil.IsCrouching();
    public float GetHeartRate() => PlayerUtil.GetHeartRate();
    public float GetBloodPressure() => PlayerUtil.GetBloodPressure();
    public float GetBloodOxygen() => PlayerUtil.GetBloodOxygen();
    public float GetBloodVolume() => PlayerUtil.GetBloodVolume();
    public float GetTemperature() => PlayerUtil.GetTemperature();
    public float GetConsciousness() => PlayerUtil.GetConsciousness();
    public float GetBrainHealth() => PlayerUtil.GetBrainHealth();
    public float GetRespiratoryRate() => PlayerUtil.GetRespiratoryRate();
    public float GetHunger() => PlayerUtil.GetHunger();
    public float GetThirst() => PlayerUtil.GetThirst();
    public float GetStamina() => PlayerUtil.GetStamina();
    public float GetEnergy() => PlayerUtil.GetEnergy();
    public float GetHappiness() => PlayerUtil.GetHappiness();
    public void Feed(float amount) => PlayerUtil.Feed(amount);
    public void Hydrate(float amount) => PlayerUtil.Hydrate(amount);
    public void RestoreStamina(float amount) => PlayerUtil.RestoreStamina(amount);
    public void RestoreEnergy(float amount) => PlayerUtil.RestoreEnergy(amount);
    public void SetHunger(float value) => PlayerUtil.SetHunger(value);
    public void SetThirst(float value) => PlayerUtil.SetThirst(value);
    public void SetStamina(float value) => PlayerUtil.SetStamina(value);
    public void SetEnergy(float value) => PlayerUtil.SetEnergy(value);
    public void SetHeartRate(float value) => PlayerUtil.SetHeartRate(value);
    public void SetBloodPressure(float value) => PlayerUtil.SetBloodPressure(value);
    public void SetTemperature(float value) => PlayerUtil.SetTemperature(value);
    public void SetConsciousness(float value) => PlayerUtil.SetConsciousness(value);
    public void SetBrainHealth(float value) => PlayerUtil.SetBrainHealth(value);
    public void SetBloodVolume(float value) => PlayerUtil.SetBloodVolume(value);
    public void SetBloodOxygen(float value) => PlayerUtil.SetBloodOxygen(value);
    public void SetHappiness(float value) => PlayerUtil.SetHappiness(value);
    public void Teleport(float x, float y) => PlayerUtil.Tp(x, y);
    public void PickItem(string itemId, int slot) => PlayerUtil.PickItem(itemId, slot);
    public void Alert(string text, bool important) => PlayerUtil.Alert(text, important);
    public void HealAll() => PlayerUtil.HealAll();
}
