![Cover](Cover.png)

[English Guide](README.md)

# Bark

[GitHub](https://github.com/CNCUMC/Bark) | [NexusMods](https://www.nexusmods.com/scavprototype/mods/362) | [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib)

_基于 [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib)
扩展的 [Casualties Unknown](https://store.steampowered.com/app/4576490/) 模组工具库。_

_由 [Moss Lib](https://github.com/Explosive-Hydra/Moss-Lib) 演进而来。_

---

## 目录

- [概述](#概述)
- [安装](#安装)
- [快速开始](#快速开始)
- [本地化](#本地化)
- [设置选项 (BetterOptions)](#设置选项-betteroptions)
- [更新检测 (UpdateUtil)](#更新检测-updateutil)
- [工具参考](#工具参考)
- [常量参考](#常量参考)
- [许可证](#许可证)

---

## 概述

**Bark** 是一个针对 **Casualties Unknown** 的 BepInEx
模组工具库，在 [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib) (CCL) 基础上扩展了增强的本地化、设置和游戏工具。

| 模块                                            | 说明                                          |
|-----------------------------------------------|---------------------------------------------|
| [`BetterLocale`](BetterCCL/BetterLocale.cs)   | 基于 CCL `LocaleRegistry` 的本地化系统              |
| [`BetterOptions`](BetterCCL/BetterOptions.cs) | CCL 设置注册封装（Float/Int/Bool/Dropdown/Keybind） |
| [`ModLangGenBase`](Base/ModLangGenBase.cs)    | 语言生成器基类                                     |
| [`UpdateUtil`](Tool/UpdateUtil.cs)            | 基于 GitHub 的模组更新检测                           |
| [`PlayerUtil`](Tool/PlayerUtil.cs)            | 玩家操作：传送、生命体征、药物、恢复、Raw Writes               |
| [`SkillUtil`](Tool/SkillUtil.cs)              | 技能等级/经验操作                                   |
| [`LimbUtil`](Tool/LimbUtil.cs)                | 肢体操作：治疗、伤害、状态检查                             |
| [`WorldUtil`](Tool/WorldUtil.cs)              | 世界操作：放置方块、物品                                |
| [`InventoryUtil`](Tool/InventoryUtil.cs)      | 物品栏操作                                       |
| [`ItemUtil`](Tool/ItemUtil.cs)                | 物品工具：附近搜索、修复、耐久度                            |
| [`InputUtil`](Tool/InputUtil.cs)              | 输入处理：鼠标位置、点击等待                              |
| [`LogUtil`](Tool/LogUtil.cs)                  | 控制台日志 + 校验辅助                                |
| [`TextUtil`](Tool/TextUtil.cs)                | 富文本格式化：颜色、透明度、粗体、斜体、字号                      |
| [`ToolsUtil`](Tool/ToolsUtil.cs)              | 参数验证、浮点/整数解析                                |
| [`Blocks`](Constant/Blocks.cs)                | 强类型方块定义                                     |
| [`Items`](Constant/Items.cs)                  | 强类型物品定义                                     |
| [`Backgrounds`](Constant/Backgrounds.cs)      | 背景 ID 字符串常量                                 |
| [`Keys`](Constant/Keys.cs)                    | 按键动作常量                                      |
| [`Slots`](Constant/Slots.cs)                  | 物品栏槽位定义                                     |

---

## 安装

1. 为 Casualties Unknown 安装 [BepInEx 5.x](https://github.com/BepInEx/BepInEx)。
2. 安装 [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib) ≥ 1.0.2 —
   将 `CUCoreLib.dll` 放入 `BepInEx/plugins/CUCoreLib/`。
3. 从 [Releases](https://github.com/CNCUMC/Bark/releases) 页面下载最新版 `Bark.dll`。
4. 将 `Bark.dll` 放入 `BepInEx/plugins/` 文件夹。

> **对于模组开发者：** 在项目中添加 `Bark.dll` 引用，在插件类上添加 `[BepInDependency("org.cucnmc.bark")]` 特性。

---

## 快速开始

### 1. 添加依赖

```csharp
[BepInPlugin(Guid, Name, Version)]
[BepInDependency("net.cucorelib")]     // CCL 必需
[BepInDependency("org.cucnmc.bark")]   // Bark 扩展 CCL
public class MyPlugin : BaseUnityPlugin
{
    // ...
}
```

### 2. 本地化

Bark 在 CCL 的 `LocaleRegistry` 之上提供 `BetterLocale`：

```csharp
using Bark.BetterCCL;

// 获取本地化文本（CCL → Bark 默认值 → key 回退）
string text = BetterLocale.GetOther("bark.feature.enabled");

// 通过语言生成器定义回退翻译：
// EnLangGenerator.cs
Other("bark.feature.enabled", "Enable Feature");

// ZhCnLangGenerator.cs
Other("bark.feature.enabled", "启用功能");
```

见 [Example/Lang/](Example/Lang) 示例生成器。

### 3. 注册设置

```csharp
using Bark.BetterCCL;

BetterOptions.Bool("bark", "feature_enabled", Setting.SettingCategory.Game, true);

// 自定义分类标签页
BetterOptions.Bool("bark", "advanced_mode", "Bark", false);
```

### 4. 检查更新

```csharp
using Bark.Tool;

// 在 Awake() 中调用 — 异步执行，结果输出到日志和控制台
UpdateUtil.Check("YourName/YourRepo", "你的模组", "1.0.0", Logger);
```

---

## 本地化

### 生成器 (`ModLangGenBase`)

```csharp
public class EnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "EN";
    protected override void BuildLocaleData()
    {
        Other("bark.tooltip.heat", "Hot enough to warp.");
        Option("bark.game.test", "Test Mode", "Turns on the test mode");
    }
}
```

| 方法                           | 分类             |
|------------------------------|----------------|
| `Item(key, value, desc)`     | `item`         |
| `Building(key, value, desc)` | `build`        |
| `Moodle(key, value, desc)`   | `moodle`       |
| `Other(key, value)`          | `other`        |
| `Option(key, label, desc)`   | `option`（设置标签） |
| `Log(key, value)`            | `log`          |
| `Command(key, value, desc)`  | `command`      |
| `Liquid(key, value, desc)`   | `liquid`       |
| `Title(key, value, desc)`    | `title`        |

### BetterLocale API

#### Get（获取本地化文本）

| 方法                        | 分类        |
|---------------------------|-----------|
| `GetItem(key, args?)`     | `item`    |
| `GetBuilding(key, args?)` | `build`   |
| `GetMoodle(key, args?)`   | `moodle`  |
| `GetOther(key, args?)`    | `other`   |
| `GetLog(key, args?)`      | `log`     |
| `GetCommand(key, args?)`  | `command` |
| `GetOption(key, args?)`   | `option`  |
| `GetLiquid(key, args?)`   | `liquid`  |
| `GetTitle(key, args?)`    | `title`   |

> **注意：** `args` 会替换查找结果中的 `{0}`、`{1}` 等占位符。
> 例如 `BetterLocale.GetLog("update.available", "Bark", "1.0", "2.0")` 返回
> `"Bark 有新版本可用！1.0 -> 2.0"`。

#### Has（检查翻译是否存在）

| 方法                      | 分类        |
|-------------------------|-----------|
| `HasKey(category, key)` | 任意分类      |
| `HasKeyItem(key)`       | `item`    |
| `HasKeyBuilding(key)`   | `build`   |
| `HasKeyMoodle(key)`     | `moodle`  |
| `HasKeyOther(key)`      | `other`   |
| `HasKeyLog(key)`        | `log`     |
| `HasKeyCommand(key)`    | `command` |
| `HasKeyOption(key)`     | `option`  |
| `HasKeyLiquid(key)`     | `liquid`  |
| `HasKeyTitle(key)`      | `title`   |

#### 其他

| 方法                                | 说明                          |
|-----------------------------------|-----------------------------|
| `SetDefault(lang, cat, key, val)` | 注册回退值                       |
| `Flush()`                         | 将所有默认值写入 CCL 语言目录           |
| `ToRichText(md)`                  | 将 **Markdown** 转为 Unity 富文本 |
| `StripMarkdown(md)`               | 去除 Markdown 标记              |

---

## 设置选项 (BetterOptions)

```csharp
BetterOptions.Bool("ns", "key", Setting.SettingCategory.Game, true);
BetterOptions.Int("ns", "level", Setting.SettingCategory.Game, 5, 1, 10);
BetterOptions.Float("ns", "volume", Setting.SettingCategory.Audio, 0.8f, 0f, 1f);
BetterOptions.Dropdown("ns", "mode", Setting.SettingCategory.Game, 0, choices);
BetterOptions.Keybind("ns", "hotkey", Setting.SettingCategory.Input, KeyCode.F5);

// 自定义分类标签页
BetterOptions.Bool("ns", "key", "我的模组", false);
```

---

## 更新检测 (UpdateUtil)

```csharp
using Bark.Tool;

// 通过 GitHub Releases API 异步检查 — 支持本地化消息
UpdateUtil.Check("CNCUMC/Bark", "我的模组", "1.0.0", Logger);
```

| 参数               | 说明                                 |
|------------------|------------------------------------|
| `githubRepo`     | GitHub 仓库路径，如 `"CNCUMC/Bark"`      |
| `modName`        | 日志和控制台消息中使用的显示名称                   |
| `currentVersion` | 当前版本号，支持 `"1.0.0"` 或 `"v1.0.0"` 格式 |
| `logger`         | 模组的 BepInEx `ManualLogSource`      |

结果同时输出到 BepInEx 日志和游戏控制台。消息通过 `BetterLocale` 本地化
（`update.no_repo`、`update.failed`、`update.no_version`、`update.available`、`update.uptodate`）。

---

## 工具参考

### LogUtil

| 方法                                           | 说明               |
|----------------------------------------------|------------------|
| `Info(text, logger)`                         | 输出到控制台 + BepInEx |
| `Error(text, logger)`                        | 输出错误             |
| `Warning(text, logger)`                      | 输出警告             |
| `CheckWorld(logger?)`                        | 世界未加载时抛异常        |
| `CheckBody(logger?)`                         | 玩家身体为空时抛异常       |
| `CheckArgumentCount(args, min, logger?)`     | 验证参数数量           |
| `CheckNotNullOrEmpty(val, name, logger?)`    | 验证字符串非空          |
| `CheckParseFloat(s, logger?)`                | 解析浮点数或抛异常        |
| `CheckParseInt(s, logger?)`                  | 解析整数或抛异常         |
| `PrintList(header, items, logger)`           | 格式化列表输出          |
| `PrintNumberedList(header, items, logger)`   | 带编号列表输出          |
| `PrintKeyValueList(header, entries, logger)` | 键值对列表输出          |
| `PrintGroupedList(header, groups, logger)`   | 分组列表输出           |

### PlayerUtil

| 方法                                    | 说明       |
|---------------------------------------|----------|
| `Tp(x, y)`                            | 传送玩家     |
| `PickItem(id, slot, force?)`          | 添加物品到物品栏 |
| `IsAlive()` / `IsConscious()`         | 状态检查     |
| `GetBloodOxygen()` / `GetHeartRate()` | 生命体征     |
| `HealAll()`                           | 完全治愈玩家   |
| `SetHunger(val)` / `SetThirst(val)`   | 原始写入     |
| `Thresholds.*`                        | 常量阈值     |

### WorldUtil

| 方法                               | 说明   |
|----------------------------------|------|
| `PlaceBlock(x, y, id)`           | 放置方块 |
| `FillBlocks(sx, sy, ex, ey, id)` | 填充区域 |
| `PlaceItem(x, y, id)`            | 生成物品 |

### SkillUtil

| 方法                          | 说明        |
|-----------------------------|-----------|
| `GetLevel(skill)`           | 获取技能等级    |
| `GetExperience(skill)`      | 获取经验值     |
| `SetLevelRaw(skill, level)` | 直接设置等级    |
| `AddExperience(skill, xp)`  | 添加经验      |
| `XpMultiplier`              | 获取/设置经验倍率 |

### LimbUtil

| 方法                                   | 说明      |
|--------------------------------------|---------|
| `GetLimb(index/slot/name)`           | 按索引获取肢体 |
| `HasBrokenBone()` / `HasInfection()` | 状态检查    |
| `HealLimb(limb)`                     | 完全治愈肢体  |
| `SetSkinHealthRaw(limb, value)`      | 原始写入    |

### InventoryUtil

| 方法                       | 说明        |
|--------------------------|-----------|
| `HasItem(id)`            | 检查是否持有物品  |
| `GetItem(slot)`          | 按槽位获取物品   |
| `GetAllItems()`          | 获取所有物品    |
| `GetAllItemInfos()`      | 获取所有物品信息  |
| `FindById(id, out item)` | 按 ID 查找物品 |

### ItemUtil

| 方法                               | 说明         |
|----------------------------------|------------|
| `FindNearby(center, radius)`     | 圆形范围查找物品   |
| `FindClosest(center, maxRadius)` | 查找最近物品     |
| `Repair(item)`                   | 修复到满耐久     |
| `SetCondition(item, val)`        | 设置耐久度（0~1） |

### InputUtil

| 方法                    | 说明      |
|-----------------------|---------|
| `WaitForLeftClick()`  | 协程：等待点击 |
| `WaitForRightClick()` | 协程：等待点击 |

---

## 常量参考

### Blocks

```csharp
ushort blockId = Blocks.SteelTile;  // 隐式转换
Blocks block = Blocks.FromId(6);
```

### Items

```csharp
string itemId = Items.Medkit;       // 隐式转换
Items item = Items.FromId("medkit");
```

### Backgrounds / Keys / Slots

```csharp
string bgId = Backgrounds.Rock;
KeyCode key = Keys.Jump;
int slotId = Slots.MainHand;
```

---

## 许可证

[GPL v3](LICENSE.md)
