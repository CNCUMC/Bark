[English](../../en-US/script-api/skills.md) | ***简体中文***

# 技能系统

SkillUtil 操作三个技能的等级和经验：力量、韧性、智力。

## 技能标识

用字符串指定技能，大小写不敏感。支持简称和全称：

| 值               | 含义             |
|------------------|------------------|
| `"str"`          | 力量             |
| `"strength"`     | 力量             |
| `"res"`          | 韧性             |
| `"resilience"`   | 韧性             |
| `"int"`          | 智力             |
| `"intelligence"` | 智力             |
| 其他任意值       | 智力（默认回退） |

## 方法

```js
// 读取等级（整数）
var strLevel = SkillUtil.GetLevel("str");

// 读取当前经验
var exp = SkillUtil.GetExperience("res");

// 读取升级进度 0~1
var progress = SkillUtil.GetProgress("int");

// 增加经验
SkillUtil.AddExperience("str", 500);

// 设置等级（同时重置经验到该等级起点）
SkillUtil.SetLevel("res", 10);

// 查看当前等级所需总经验
var needed = SkillUtil.GetExperienceForNextLevel("int");
```

> ⚠️ `SetLevel` 会把你设置到的等级经验清零。比如从 5 级设到 10 级，你损失了 5→10 之间的所有进度。

## 经验倍率

通过 `ScriptInfo` 可获取脚本模组 id，然后配合选项系统控制全局经验倍率。`XpMultiplier` 是 C# 端属性，脚本侧通过 OptionsApi 等价控制。

```js
// 读取脚本模组自己的倍率配置
var multiplier = OptionsApi.GetFloat("xp_multiplier");
SkillUtil.AddExperience("int", 100 * multiplier);
```

## 完整示例

一个双倍经验脚本模组：

```js
function onLoad() {
    Log.Info("双倍经验已启用");
}

function onWorldGenerated() {
    // 每秒检查一次，给所有技能加双倍经验
    setInterval(function () {
        for (var i = 0; i < 3; i++) {
            var skill = ["str", "res", "int"][i];
            var currentExp = SkillUtil.GetExperience(skill);
            // 这里只是演示读/写，实际逻辑应按需
        }
    }, 1000);
}
```
