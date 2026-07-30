[English](../../en-US/script-api/limbs.md) | ***简体中文***

# Limb — 肢体操作

Limb 操作角色的每个肢体：查询状态、读取数值、施加伤害、治疗。所有单肢体操作接受 `int index` 参数。

## 肢体索引

肢体用索引号访问，从 0 开始。

```js
var count = Limb.GetLimbCount();  // 总共有几个肢体
var name = Limb.GetLimbName(0);   // 第 0 号肢体的全名
var short = Limb.GetLimbShortName(0); // 简称
```

## 肢体名称校验

无需索引，按名称查询和校验肢体。

```js
// 检查名称是否有效（不区分大小写）
if (Limb.IsValidLimbName("FootF")) {
    Log.Info('FootF 是有效肢体');
}
if (!Limb.IsValidLimbName("feet")) {
    Log.Warning('"feet" 无效——应使用 FootF 或 FootB');
}

// 获取全部 15 个有效肢体名
var names = Limb.GetAllLimbNames();
// ["Head", "UpTorso", "DownTorso", "UpArmF", "DownArmF", "HandF",
//  "UpArmB", "DownArmB", "HandB", "ThighF", "CrusF", "FootF",
//  "ThighB", "CrusB", "FootB"]
```

| 方法                       | 返回类型         | 说明                                         |
|----------------------------|------------------|----------------------------------------------|
| `IsValidLimbName(name)`    | `bool`           | `name` 是否匹配已知肢体名（不区分大小写）    |
| `GetAllLimbNames()`        | `List<string>`   | 全部 15 个有效肢体名列表                     |

游戏已知 15 个肢体：`Head`、`UpTorso`、`DownTorso`、`UpArmF`、`DownArmF`、`HandF`、`UpArmB`、`DownArmB`、`HandB`、`ThighF`、`CrusF`、`FootF`、`ThighB`、`CrusB`、`FootB`

## 状态查询

`Is*` / `Has*` 开头的布尔查询，传入肢体索引。

```js
if (Limb.IsBroken(0)) {
    Log.Info('0 号肢体骨折');
}
if (Limb.IsDislocated(1)) {
    Log.Info('1 号脱臼');
}
if (Limb.IsInfected(2)) {
    Log.Info('2 号感染');
}
if (Limb.IsDismembered(3)) {
    Log.Info('3 号已被截断');
}
if (Limb.IsSplinted(0)) {
    Log.Info('0 号上了夹板');
}
```

| 方法                       | 说明       |
|----------------------------|------------|
| `IsBroken(index)`          | 骨折       |
| `IsDislocated(index)`      | 脱臼       |
| `IsInfected(index)`        | 感染       |
| `IsDismembered(index)`     | 已截断     |
| `IsSplinted(index)`        | 有夹板     |
| `IsVital(index)`           | 是要害部位 |
| `IsHead(index)`            | 是头部     |
| `IsAbdomen(index)`         | 是腹部     |
| `IsArm(index)`             | 是手臂     |
| `IsLeg(index)`             | 是腿       |
| `HasShrapnel(index)`       | 有弹片     |
| `IsBlockedBleeding(index)` | 已止血     |

## 数值查询

```js
var skin = Limb.GetSkinHealth(0);       // 皮肤健康 0-100
var muscle = Limb.GetMuscleHealth(0);   // 肌肉健康 0-100
var pain = Limb.GetPain(0);             // 疼痛 0-100
var bleed = Limb.GetBleedAmount(0);     // 当前出血量
var totalBleed = Limb.GetTotalBleedAmount(0); // 累计出血
var infection = Limb.GetInfectionAmount(0);   // 感染程度
var shrapnel = Limb.GetShrapnelCount(0);      // 弹片数量
```

## 修改操作

命名前缀：`Set*` 设绝对值，`Damage*` 减相对值，`HealLimb` 一键恢复。

```js
// 设绝对值
Limb.SetSkinHealth(0, 80);     // 皮肤恢复到 80
Limb.SetMuscleHealth(1, 100);  // 肌肉回满
Limb.SetPain(2, 0);            // 止痛
Limb.SetBleed(0, 0);           // 止血
Limb.SetInfection(1, 0);       // 消炎
Limb.SetShrapnel(3, 0);        // 取弹片

// 减相对值（施加伤害）
Limb.DamageSkin(0, 20);        // 皮肤扣 20
Limb.DamageMuscle(1, 30);      // 肌肉扣 30

// 特殊操作
Limb.BreakBone(0);             // 打骨折
Limb.MendBone(0);              // 接骨
Limb.DislocateLimb(1);         // 脱臼
Limb.UnDislocateLimb(1);       // 复位
Limb.SetBlockedBleeding(2, true);  // 上止血带
Limb.SetDisinfect(0, 60);      // 消毒 60 秒

// 一键治疗一个肢体
Limb.HealLimb(0);              // 皮肉回满 + 止血 + 接骨 + 复位 + 消炎
```

## 全局聚合查询

不加索引，查全身状态。

```js
// 是否存在
if (Limb.HasBrokenBone()) {
    Log.Info('身上有骨折');
}
if (Limb.HasDislocation()) {
    Log.Info('有脱臼');
}
if (Limb.HasInfection()) {
    Log.Info('有感染');
}
if (Limb.HasDismemberment()) {
    Log.Info('有截肢');
}

// 计数
var brokenCount = Limb.CountBroken();
var infectedCount = Limb.CountInfected();

// 全局数值
var avgPain = Limb.GetAveragePain();          // 全身平均疼痛
var avgSkin = Limb.GetAverageSkinHealth();     // 平均皮肤健康
var maxInfection = Limb.GetMaxInfection();     // 最严重的感染值
var bleedSpeed = Limb.GetTotalBleedSpeed();    // 全身出血速度
```

## 完整示例

全身检查报告：

```js
function checkAllLimbs() {
    var count = Limb.GetLimbCount();
    var issues = [];
    for (var i = 0; i < count; i++) {
        var name = Limb.GetLimbShortName(i);
        var parts = [];
        if (Limb.IsBroken(i)) parts.push('骨折');
        if (Limb.IsDislocated(i)) parts.push('脱臼');
        if (Limb.IsInfected(i)) parts.push('感染');
        if (Limb.GetPain(i) > 50) parts.push('剧痛');
        if (parts.length > 0) {
            issues.push(name + ': ' + parts.join('/'));
        }
    }
    if (issues.length === 0) {
        Log.Info('全身健康！');
    } else {
        Log.Warning(issues.join('\n'));
    }
}

// 每十秒体检一次
function onLoad() {
    setInterval(checkAllLimbs, 10000);
}
```
