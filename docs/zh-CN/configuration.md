[English](../en-US/configuration.md) | ***简体中文***

# 配置与本地化

Bark 提供两套配置系统：C# 模组用 `BetterOptions` + `LangGenerator`，脚本模组用 `options.json` + `Lang/` 语言文件。

## C# 模组

### 注册选项

用 `BetterOptions` 的静态方法注册，选项会出现在游戏的设置界面中，玩家可以手动修改。

```csharp
using Bark.BetterCCL;
using CUCoreLib.Data;
using UnityEngine;

// Bool 开关
BetterOptions.Bool("my_mod", "enable_auto_heal", Setting.SettingCategory.Game,
    true,  // 默认值
    val => Log.Info($"自动回血: {(val ? "开启" : "关闭")}"));

// Int 滑块
BetterOptions.Int("my_mod", "heal_amount", Setting.SettingCategory.Game,
    10, 0, 100);

// Float 滑块
BetterOptions.Float("my_mod", "speed_multiplier", Setting.SettingCategory.Game,
    1.0f, 0.1f, 5.0f);

// Dropdown 下拉
BetterOptions.Dropdown("my_mod", "difficulty", Setting.SettingCategory.Game,
    0,  // 默认选中索引
    new[]
    {
        new ModDropdownChoice("简单", "简单"),
        new ModDropdownChoice("普通", "普通"),
        new ModDropdownChoice("困难", "困难"),
    });

// Keybind 按键绑定
BetterOptions.Keybind("my_mod", "toggle_key", Setting.SettingCategory.Input,
    KeyCode.F5);
```

完整签名：

```
Bool(ns, key, category, defaultValue, apply?)
Int(ns, key, category, defaultValue, min, max, apply?)
Float(ns, key, category, defaultValue, min, max, apply?, formatValue?)
Dropdown(ns, key, category, defaultValue, choices[], apply?)
Keybind(ns, key, category, defaultValue, apply?)
```

**分类**：`Setting.SettingCategory` 有 `Game` / `Audio` / `Input` / `Video`，对应游戏设置面板的四个选项卡。也可以传`string`
自定义分类名，会新建一个独立选项卡。

**apply 回调**：玩家修改选项后触发。常用于即时生效的热更新。

> ℹ️ 选项的标签和描述文案不是凭空出现的——需要用 `LangGenerator` 注册对应的 locale 文本（见下一节）。

### 注册本地化文本

继承 `ModLangGenBase`（单语言）或 `ModLangGenMultiBase`（多语言），在 `BuildLocaleData()` 里调用 helper 方法写入每种语言的文本。

**单语言版**：

```csharp
using Bark.Base;

internal class MyLangGen : ModLangGenBase
{
    protected override string LanguageCode => "zh-CN";
    protected override string NameSpace => "my_mod";

    protected override void BuildLocaleData()
    {
        Option("enable_auto_heal", "自动回血", "开启后每秒回复一定血量");
        Option("heal_amount", "回血量", "每次回复的血量值");
        Option("speed_multiplier", "速度倍率", "移动速度的倍率");

        Log("loaded", "模组已加载");
        Command("help", "显示帮助");

        Item("special_sword", "特制长剑", "一把附带火焰伤害的剑");

        Other("greeting", "你好，世界");
    }
}
```

然后在 `Awake()` 里初始化：

```csharp
new MyLangGen().Initialize(Logger);
BetterLocale.Flush();
```

**多语言版**（推荐）：

```csharp
internal class MyLangGen : ModLangGenMultiBase
{
    protected override string NameSpace => "my_mod";
    protected override IEnumerable<string> LanguageCodes =>
        ["EN", "zh-CN", "zh-TW", "ru-RU"];

    protected override void BuildLocaleData()
    {
        // Option(labelEN, descEN, labelZH, descZH, labelTW, descTW, labelRU, descRU)
        Option("enable_auto_heal",
            "Auto Heal",        "Enable periodic health regen",
            "自动回血",          "开启后每秒回复一定血量",
            "自動回血",          "開啟後每秒回復一定血量",
            "Автохил",          "Включить периодическое восстановление здоровья");

        // Log(EN, ZH, TW, RU)
        Log("loaded",
            "Mod loaded",
            "模组已加载",
            "模組已載入",
            "Мод загружен");
    }
}
```

`ModLangGenMultiBase` 提供的 helper 方法：

| 方法                       | 说明                    |
|----------------------------|-------------------------|
| `Other(key, values...)`    | 通用文本                |
| `Log(key, values...)`      | 日志/控制台输出文本     |
| `Command(key, values...)`  | 控制台指令描述          |
| `Item(key, values...)`     | 物品名称 + 描述（成对） |
| `Building(key, values...)` | 建筑名称 + 描述（成对） |
| `Moodle(key, values...)`   | 情绪名称 + 描述（成对） |
| `Option(key, values...)`   | 选项标签 + 描述（成对） |
| `Liquid(key, values...)`   | 液体名称 + 描述（成对） |
| `Title(key, values...)`    | 标题名称 + 描述（成对） |

`values` 参数按 `LanguageCodes` 顺序排列。成对类方法（`Item`/`Option` 等）每语言占 2 个参数：标签 + 描述。

### Flush 本地化到文件

```csharp
BetterLocale.Flush();
```

将 `SetDefault` 注册的文本写入 `BepInEx/config/CUCoreLib/Locales/{语言}.json`。 **选项的标签/描述必须 Flush 后才会出现在游戏
UI 中**。

### 本地化查找优先级

```
CCL 注册的文本  →  Bark Fallback 默认值  →  英语 Fallback  →  原始 key
```

即：如果 CCL 已有翻译就用 CCL 的，没有就用 Bark 默认的，当前语言没有就用英语兜底，都没有就显示原始 key。

## 脚本模组

### 注册配置项

脚本模组的选项**定义**放在 `mod.json` 同层的 `options.json` 中（模组自带，只读）。用户修改后的**保存值**
写入 `ScriptMod/Configs/{模组id}.json`（简单 key-value 格式）。

**`ScriptMod/Mods/MyMod/options.json`**（选项定义，随模组分发）：

```json
{
  "_options": {
    "enable_auto_heal": {
      "type": "bool",
      "default": true,
      "category": "game"
    },
    "heal_amount": {
      "type": "int",
      "default": 10,
      "min": 1,
      "max": 100,
      "category": "game"
    },
    "speed_multiplier": {
      "type": "float",
      "default": 1.0,
      "min": 0.1,
      "max": 5.0
    },
    "mode": {
      "type": "dropdown",
      "default": 0,
      "choices": [
        "简单",
        "普通",
        "困难"
      ],
      "category": "game"
    },
    "hotkey": {
      "type": "keybind",
      "default": "F5",
      "category": "input"
    }
  }
}
```

**`ScriptMod/Configs/MyMod.json`**（用户保存值，游戏自动生成/更新）：

```json
{
  "enable_auto_heal": false,
  "heal_amount": 42,
  "speed_multiplier": 1.5,
  "mode": 1,
  "hotkey": "F6"
}
```

加载时 Bark 合并两层数据：用户保存值覆盖定义默认值后注册到游戏设置 UI。

| 字段       | 必填 | 说明                                                         |
|------------|------|--------------------------------------------------------------|
| `type`     | ✅   | `bool` / `int` / `float` / `dropdown` / `keybind`            |
| `default`  | ✅   | 默认值                                                       |
| `category` | ❌   | `game` / `audio` / `input` / `video`，任意字符串则新建选项卡 |
| `min`      | ❌   | int/float 最小值                                             |
| `max`      | ❌   | int/float 最大值                                             |
| `choices`  | ❌   | dropdown 选项列表                                            |

`category` 不填时默认用脚本模组 id 作为选项卡名。

> ℹ️ 选项标签和描述需要通过 locale 提供（见下方）。

### 本地化

在模组目录下创建 `Lang/` 文件夹，放 `zh-CN.json`（或其他语言代码）：

```
ScriptMod/Mods/MyMod/
  mod.json
  main.js
  Lang/
    EN.json
    zh-CN.json
```

**`Lang/zh-CN.json`**：

```json
{
  "option": {
    "my_mod.game.enable_auto_heal": "自动回血",
    "my_mod.game.enable_auto_healdsc": "开启后每秒回复一定血量",
    "my_mod.game.heal_amount": "回血量",
    "my_mod.game.heal_amountdsc": "每次回复的血量值"
  },
  "log": {
    "welcome": "欢迎使用本模组！",
    "damage_report": "受到 {0} 点伤害，部位: {1}"
  }
}
```

选项的 locale key 格式：`{modId}.{category}.{optionKey}`，描述在后面加 `dsc`。
`category` 不填时默认等于模组 id（如 `my_mod.my_mod.heal_amount`），填了标准分类（`game`/`audio`/`input`/`video`）或自定义字符串则对应区分。

加载时 Bark 自动把 locale 数据推入 CCL，菜单里就能看到中文了。

### 在脚本里读取配置

```js
var enabled = OptionsApi.GetBool(ScriptInfo.Id, "enable_auto_heal");
var amount = OptionsApi.GetInt(ScriptInfo.Id, "heal_amount");
var speed = OptionsApi.GetFloat(ScriptInfo.Id, "speed_multiplier");
var mode = OptionsApi.GetDropdown(ScriptInfo.Id, "mode");
```

详见 [OptionsApi](script-api/options.md)。

### 在脚本里用本地化文本

```js
var msg = Locale.Get("welcome");                     // 自动加模组 id 前缀
var msg2 = Locale.GetFormatted("damage_report", 25);  // {0} → 25
```

详见 [LocaleApi](script-api/locale.md)。
