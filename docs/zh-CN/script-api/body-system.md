# BodyUtil — 角色生理系统

BodyUtil 是调用频率最高的 API，控制角色的身体状态、生理数值、药物效果。所有方法遵循统一的命名前缀。

## 状态检测

`Is*` / `Has*` / `Can*` 开头的布尔查询。Body 不存在时统一返回 `false`。

| 前缀            | 含义               | 示例                                       |
|-----------------|--------------------|--------------------------------------------|
| `Is*`           | 当前是否处于某状态 | `IsAlive()`, `IsSleeping()`, `IsDying()`   |
| `Has*`          | 是否有某物/某效果  | `HasScubaGear()`, `HasPulmonaryEmbolism()` |
| `Can*`          | 是否可以做某事     | `CanTakeNap()`                             |
| `Allow*`        | 是否允许           | `AllowUseItem()`                           |
| `Used*`         | 是否用过           | `UsedNeuralBooster()`                      |
| `Tried*`        | 是否尝试过         | `TriedRollingLastStand()`                  |
| `Successfully*` | 是否成功           | `SuccessfullyRolledLastStand()`            |

```js
if (BodyUtil.IsDying()) {
    PlayerUtil.Alert('你快死了！', true);
}

if (BodyUtil.HasScubaGear() && BodyUtil.IsInWater()) {
    Log.Info('安全潜水');
}
```

完整列表（28 个）：`IsAlive`, `IsConscious`, `IsDying`, `IsCriticallyDying`, `IsInCardiacArrest`, `IsSleeping`,
`IsExercising`, `IsBreathing`, `IsInWater`, `IsStanding`, `IsCrouching`, `IsOnHardStimulants`, `IsUsingSleepingBag`,
`IsBothHandsUnusable`, `IsFibrillationForced`, `IsAboveMedicalCutoff`, `IsDisfigured`, `IsEyeGone`, `IsBothEyesGone`,
`IsMindWiped`, `HasScubaGear`, `HasPulmonaryEmbolism`, `HasPainkillers`, `HasAntidepressants`, `HasSleepingPills`,
`CanTakeNap`, `AllowUseItem`, `UsedNeuralBooster`, `TriedRollingLastStand`, `SuccessfullyRolledLastStand`

## 数值查询

`Get*` 开头的 float 查询。Body 不存在时返回 `0f`。

```js
var hunger = BodyUtil.GetHunger();        // 0-125
var bloodVolume = BodyUtil.GetBloodVolume(); // 0-200
var temp = BodyUtil.GetTemperature();      // 20-50
var heartRate = BodyUtil.GetHeartRate();   // 0-300
```

> ℹ️ `GetHappiness` 实际映射到 `BodyUtil.GetHappiness()`，背后是 `totalHappiness` 字段。

完整列表（36 个）：`GetHunger`, `GetThirst`, `GetStamina`, `GetEnergy`, `GetConsciousness`, `GetBrainHealth`,
`GetBloodVolume`, `GetBloodOxygen`, `GetHeartRate`, `GetBloodPressure`, `GetRespiratoryRate`, `GetTemperature`,
`GetBloodViscosity`, `GetBloodVesselSize`, `GetFibrillationProgress`, `GetAdrenaline`, `GetCurAdrenaline`,
`GetSepticShock`, `GetSicknessAmount`, `GetVenomTotal`, `GetVenomCurrent`, `GetInternalBleeding`, `GetHemothorax`,
`GetShock`, `GetPainShock`, `GetTraumaAmount`, `GetRadiationSickness`, `GetStrokeAmount`, `GetBadSleepAmount`,
`GetGoodSleepTime`, `GetLastStandTime`, `GetAntibioticImmunityTime`, `GetDirtyness`, `GetWetness`, `GetHearingLoss`,
`GetSnowAmount`, `GetRawHappiness`, `GetFocusedLevel`, `GetHorrifiedLevel`, `GetClawHealth`, `GetWeightOffset`,
`GetOpiateHappiness`, `GetAntidepressantHappiness`, `GetCaffeinated`, `GetCorpsesSeen`

## 数值修改

`Set*` 开头的写入方法。参数为 `float`，值会被 clamp 到合法范围。

```js
BodyUtil.SetHunger(80);       // 吃饱
BodyUtil.SetStamina(100);     // 满体力
BodyUtil.SetBloodVolume(90);  // 补血
BodyUtil.SetTemperature(37);  // 正常体温
```

完整列表（33 个 Setter）：`SetHunger`, `SetThirst`, `SetStamina`, `SetEnergy`, `SetBloodVolume`, `SetBloodOxygen`,
`SetHeartRate`, `SetBloodPressure`, `SetTemperature`, `SetConsciousness`, `SetBrainHealth`, `SetRadiationSickness`,
`SetTraumaAmount`, `SetInternalBleeding`, `SetFocusedLevel`, `SetHorrifiedLevel`, `SetClawHealth`, `SetWeightOffset`,
`SetRespiratoryRate`, `SetBloodViscosity`, `SetBloodVesselSize`, `SetFibrillationProgress`, `SetAdrenaline`,
`SetCurAdrenaline`, `SetSepticShock`, `SetSicknessAmount`, `SetVenomTotal`, `SetVenomCurrent`, `SetHemothorax`,
`SetShock`, `SetPainShock`, `SetStrokeAmount`, `SetBadSleepAmount`, `SetGoodSleepTime`, `SetLastStandTime`,
`SetAntibioticImmunityTime`, `SetDirtyness`, `SetWetness`, `SetHearingLoss`, `SetSnowAmount`, `SetRawHappiness`,
`SetOpiateHappiness`, `SetAntidepressantHappiness`, `SetCaffeinated`

`SetCorpsesSeen(int count)` 是唯一接受 int 的方法，设置见过的尸体数量。

## 药物相关

`Has* / Remove* / Set*` — 检测药物是否生效、移除药物效果、设置药物相关数值。

```js
// 检测
if (BodyUtil.HasPainkillers()) {
    Log.Info('止痛药生效中');
}

// 移除
BodyUtil.RemovePainkillers();       // 强制结束止痛药效果
BodyUtil.RemoveAntidepressants();   // 结束抗抑郁药
BodyUtil.RemoveSleepingPills();     // 结束安眠药

// 数值
BodyUtil.SetOpiateHappiness(50);          // 阿片类愉悦度
BodyUtil.SetAntidepressantHappiness(80);  // 抗抑郁愉悦度
BodyUtil.SetCaffeinated(100);             // 咖啡因
```

## 复合操作

这些方法不遵循 `Get/Set` 前缀，属于特殊操作。

| 方法                     | 说明                                                         |
|--------------------------|--------------------------------------------------------------|
| `Feed(amount)`           | 增加饱食度（相对值）                                         |
| `Hydrate(amount)`        | 增加水分（相对值）                                           |
| `RestoreStamina(amount)` | 恢复体力                                                     |
| `RestoreEnergy(amount)`  | 恢复精力                                                     |
| `HealAll()`              | 一键满血：治疗全部肢体 + 恢复所有生理数值 + 移除全部药物效果 |

```js
BodyUtil.Feed(20);             // 吃一口
BodyUtil.Hydrate(30);          // 喝一口
BodyUtil.RestoreStamina(50);   // 喝瓶能量饮料
BodyUtil.HealAll();            // 恢复
```

## 典型示例

### 状态监控 HUD

```js
function onUpdate() {
    // 危险状态警告
    if (BodyUtil.IsDying()) {
        PlayerUtil.Alert('生命垂危！', true);
    }
    if (BodyUtil.GetBloodVolume() < 40) {
        PlayerUtil.Alert('失血过多！', true);
    }
    if (BodyUtil.GetBrainHealth() < 30) {
        PlayerUtil.Alert('脑损伤严重！', true);
    }
}
```

### 自动治疗脚本

```js
function onLoad() {
    // 每 5 秒检查一次
    setInterval(function () {
        if (BodyUtil.GetBloodVolume() < 80) {
            BodyUtil.SetBloodVolume(90);
            Log.Info('自动补血');
        }
    }, 5000);
}
```

## 注意事项

- 所有 `Get*` 在 Body 不存在时返回 `0f`，不是 null
- `Set*` 的值会被自动 clamp，超出范围也没关系
- `HealAll()` 是终极恢复，会清除所有负面状态包括药物效果
- `Feed/Hydrate` 是相对增减，传入负数可以降低数值
- 复数肢体操作（如 `IsBothHandsUnusable`）检测两只手，精细操作见 [LimbUtil](limbs.md)
