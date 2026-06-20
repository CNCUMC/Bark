![alt text](Covor.png)

[English Guide](README.md)

# Moss Lib

[GitHub](https://github.com/Black-Moss/Moss-Lib) / [Nexus Mods](https://www.nexusmods.com/scavprototype/mods/8)

_只是一个 [Black_Moss](https://github.com/Black-Moss) 的模组前置库。:)_

---

## 目录

- [概述](#概述)
- [安装](#安装)
- [快速开始](#快速开始)
- [本地化系统](#本地化系统)
- [指令系统](#指令系统)
- [语言生成器](#语言生成器)
- [工具参考](#工具参考)
    - [Log - 日志](#log---日志)
    - [GameConsole - 游戏控制台](#gameconsole---游戏控制台)
    - [World - 世界操作](#world---世界操作)
    - [Player - 玩家操作](#player---玩家操作)
    - [Key - 输入处理](#key---输入处理)
    - [Multiplayer - 多人游戏](#multiplayer---多人游戏)
    - [Config - 配置](#config---配置)
    - [RichText - 富文本](#richtext---富文本)
    - [Tools - 工具函数](#tools---工具函数)
- [常量参考](#常量参考)
    - [Blocks - 方块](#blocks---方块)
    - [Items - 物品](#items---物品)
    - [Backgrounds - 背景](#backgrounds---背景)
    - [Keys - 按键](#keys---按键)
- [UI 工具包（开发中）](#ui-工具包开发中)

---

## 概述

**Moss Lib** 是一个针对 **Casualties Unknown**（及其 Demo）的 BepInEx 模组前置库，提供了一系列可复用的基类和实用工具，以简化模组开发流程。包含以下模块：

| 模块                                         | 说明                                 |
|--------------------------------------------|------------------------------------|
| [`ModLocaleBase`](Base/ModLocaleBase.cs)   | 多语言本地化系统，基于 JSON 语言文件              |
| [`ModCommandBase`](Base/ModCommandBase.cs) | 自定义游戏内控制台指令注册基类                    |
| [`ModLangGenBase`](Base/ModLangGenBase.cs) | 语言文件生成器，从代码自动生成 JSON 本地化文件         |
| [`Log`](Tool/Log.cs)                       | 增强的游戏内控制台日志工具                      |
| [`GameConsole`](Tool/Console.cs)           | 编程式执行游戏控制台命令的封装                    |
| [`World`](Tool/World.cs)                   | 世界操作：放置方块、物品、背景图块                  |
| [`Player`](Tool/Player.cs)                 | 玩家操作：传送、屏幕提示、物品栏管理                 |
| [`Key`](Tool/Key.cs)                       | 输入处理：按键绑定检查、鼠标点击等待、世界坐标转换          |
| [`Multiplayer`](Tool/Multiplayer.cs)       | 多人游戏支持，通过反射集成 KrokoshaCasualtiesMP |
| [`Config`](Tool/Config.cs)                 | BepInEx 配置项开关辅助                    |
| [`RichText`](Tool/RichText.cs)             | Unity 富文本格式化：颜色、透明度、粗体、斜体、字号       |
| [`Tools`](Tool/Tools.cs)                   | 参数验证、浮点/整数解析工具                     |
| [`Blocks`](Constant/Blocks.cs)             | 强类型方块定义，包含属性                       |
| [`Items`](Constant/Items.cs)               | 强类型物品定义，包含属性                       |
| [`Backgrounds`](Constant/Backgrounds.cs)   | 背景 ID 字符串常量                        |
| [`Keys`](Constant/Keys.cs)                 | 强类型按键动作常量                          |

---

## 安装

1. 为 Casualties Unknown 安装 [BepInEx 5.x](https://github.com/BepInEx/BepInEx)。
2. 从 [Releases 页面](https://github.com/Black-Moss/Moss-Lib/releases)
   （或 [Nexus Mods](https://www.nexusmods.com/scavprototype/mods/8)）下载最新版 `MossLib.dll`。
3. 将 `MossLib.dll` 放入 `BepInEx/plugins/` 文件夹。
4. （可选）如需多人游戏功能，需安装 **KrokoshaCasualtiesMP** 作为软依赖。

> **对于模组开发者：** 在项目中添加 `MossLib.dll` 引用，并在插件类上添加 `BepInDependency("blackmoss.mosslib")` 特性。

---

## 快速开始

### 1. 添加 BepInEx 依赖

```csharp
[BepInPlugin(Guid, Name, Version)]
[BepInDependency("blackmoss.mosslib")]  // 添加此行
public class MyPlugin : BaseUnityPlugin
{
    // ...
}
```

### 2. 设置本地化

```csharp
// 创建本地化类（单例封装）
public class MyLocale : ModLocaleBase
{
    private static MyLocale _instance;

    public static void Initialize(ManualLogSource logger)
    {
        _instance = new MyLocale();
        _instance.Initialize(logger, Assembly.GetExecutingAssembly());
    }

    public static string Get(string key) => _instance?.GetString(key) ?? $"[{key}]";
    public static string GetFormat(string key, params object[] args) => _instance?.GetStringFormatted(key, args) ?? $"[{key}]";
}
```

### 3. 注册自定义指令

```csharp
[HarmonyPatch(typeof(ConsoleScript))]
public class MyCommand : ModCommandBase
{
    [HarmonyPatch("RegisterAllCommands")]
    [HarmonyPostfix]
    public static void RegisterCustomCommands(ConsoleScript __instance)
    {
        ConsoleScript.Commands.Add(new Command(
            "mycommand",
            "我的指令说明",
            _ => Log.Info("指令已执行！", Plugin.Logger),
            null
        ));
    }
}
```

---

## 本地化系统

本地化系统从插件目录下的 `Lang/` 文件夹加载 JSON 语言文件。

### 文件结构

```
BepInEx/plugins/YourPlugin/
├── Lang/
│   ├── EN.json        # 英语（回退语言）
│   ├── zh-CN.json     # 简体中文
│   └── zh-TW.json     # 繁体中文
└── YourPlugin.dll
```

### JSON 格式

语言文件使用嵌套 JSON 键，以点号分隔进行组织：

```json
{
  "welcome": "欢迎！",
  "command": {
    "mycommand": {
      "description": "我的自定义指令",
      "text": "你好 {0}！"
    }
  },
  "tool": {
    "player": {
      "bodynull": "玩家身体对象为空"
    }
  }
}
```

### API 方法

| 方法                                                          | 说明                                 |
|-------------------------------------------------------------|------------------------------------|
| [`GetString(key)`](Base/ModLocaleBase.cs)                   | 根据键获取本地化字符串；依次回退到英语和 `[key]` 占位符   |
| [`GetStringFormatted(key, args...)`](Base/ModLocaleBase.cs) | 获取本地化字符串并用 `string.Format()` 格式化参数 |
| [`GetStringArray(key)`](Base/ModLocaleBase.cs)              | 获取本地化字符串数组（JSON 数组）                |
| [`GetStringDictionary(key)`](Base/ModLocaleBase.cs)         | 获取本地化字典（JSON 对象）                   |
| [`HasKey(key)`](Base/ModLocaleBase.cs)                      | 检查翻译键是否存在                          |

### 示例

完整实现参见 [`Example/ModLocale.cs`](Example/ModLocale.cs)。

```csharp
// 在插件的 Awake() 中初始化
MyLocale.Initialize(Logger);

// 使用
string welcome = MyLocale.Get("welcome");                              // "欢迎！"
string message = MyLocale.GetFormat("command.mycommand.text", "世界");  // "你好世界！"
```

---

## 指令系统

指令系统允许你通过 Harmony 补丁注册自定义游戏控制台指令。

### 基类：[`ModCommandBase`](Base/ModCommandBase.cs)

| 成员                                                                              | 说明                              |
|---------------------------------------------------------------------------------|---------------------------------|
| [`Initialize(logger, assembly, harmony?)`](Base/ModCommandBase.cs)              | 使用日志器和插件程序集初始化；可指定 Harmony 实例   |
| [`LogToConsole(text)`](Base/ModCommandBase.cs)                                  | 向游戏内控制台输出文本                     |
| [`ApplicationLogCallback(condition, stackTrace, type)`](Base/ModCommandBase.cs) | Unity 日志回调，带颜色标记的控制台输出          |
| [`Logger`](Base/ModCommandBase.cs)                                              | 受保护的 `ManualLogSource` 属性，供子类使用 |

### 示例

```csharp
[HarmonyPatch(typeof(ConsoleScript))]
public class MyCommand : ModCommandBase
{
    [HarmonyPatch("RegisterAllCommands")]
    [HarmonyPostfix]
    public static void RegisterCustomCommands(ConsoleScript __instance)
    {
        ConsoleScript.Commands.Add(new Command(
            "hello",
            "打个招呼",
            args => {
                string name = args.Length > 1 ? args[1] : "世界";
                LogToConsole($"你好，{name}！");
            },
            null
        ));
    }
}
```

完整示例参见 [`Example/ModCommand.cs`](Example/ModCommand.cs)。

---

## 语言生成器

语言生成器可以从 C# 代码自动创建 JSON 语言文件，省去手动编写和维护本地化 JSON 文件的工作。

### 基类：[`ModLangGenBase`](Base/ModLangGenBase.cs)

| 成员                                                     | 说明                              |
|--------------------------------------------------------|---------------------------------|
| [`LanguageCode`](Base/ModLangGenBase.cs)               | 抽象属性；返回语言代码（如 `"EN"`、`"zh-CN"`） |
| [`BuildLocaleData()`](Base/ModLangGenBase.cs)          | 抽象方法；调用 `Add()` 注册翻译条目          |
| [`Add(key, value)`](Base/ModLangGenBase.cs)            | 添加一条翻译条目                        |
| [`Generate(outputDirectory?)`](Base/ModLangGenBase.cs) | 生成 JSON 文件；与已有文件合并（不会覆盖已有条目）    |
| [`Count`](Base/ModLangGenBase.cs)                      | 已注册的翻译条目数量                      |

### 创建生成器

```csharp
public class EnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "EN";

    protected override void BuildLocaleData()
    {
        Add("welcome", "欢迎！");
        Add("command.hello.description", "打个招呼");
        Add("command.hello.text", "你好，{0}！");
    }
}
```

### 注册与生成

使用 [`LocaleGenerator`](Tool/LocaleGenerator.cs) 注册并生成所有语言文件：

| 方法                                                                          | 说明            |
|-----------------------------------------------------------------------------|---------------|
| [`SetLogger(logger)`](Tool/LocaleGenerator.cs)                              | 设置日志器         |
| [`Register(generator, logger)`](Tool/LocaleGenerator.cs)                    | 注册一个语言生成器     |
| [`GenerateAll(outputDirectory?)`](Tool/LocaleGenerator.cs)                  | 生成所有已注册的语言文件  |
| [`GenerateSingle(languageCode, outputDirectory?)`](Tool/LocaleGenerator.cs) | 生成单个语言文件      |
| [`PrintInfo()`](Tool/LocaleGenerator.cs)                                    | 打印所有已注册生成器的信息 |

```csharp
// 在插件的 Awake() 中
LocaleGenerator.SetLogger(Logger);
LocaleGenerator.Register(new EnLangGenerator(), Logger);
LocaleGenerator.Register(new ZhCnLangGenerator(), Logger);
LocaleGenerator.GenerateAll(); // 在 Lang/ 文件夹中创建 EN.json、zh-CN.json
```

> **注意：** 生成器会**合并**已有 JSON 文件——仅添加**新**条目，保留用户对现有翻译的任何修改。

---

## 工具参考

### Log - 日志

[`Log`](Tool/Log.cs) — 增强的游戏内控制台日志，集成 BepInEx 日志系统。

| 方法                                                      | 说明                      |
|---------------------------------------------------------|-------------------------|
| [`LogToConsole(text)`](Tool/Log.cs)                     | 向游戏内控制台写入带时间戳的消息        |
| [`Info(text, logger)`](Tool/Log.cs)                     | 同时输出到游戏控制台和 BepInEx 日志  |
| [`Debug(text, logger)`](Tool/Log.cs)                    | 输出调试消息，带 `[DEBUG]` 前缀   |
| [`Error(text, logger)`](Tool/Log.cs)                    | 输出错误消息，带 `[ERROR]` 前缀   |
| [`Warning(text, logger)`](Tool/Log.cs)                  | 输出警告消息，带 `[WARNING]` 前缀 |
| [`Alert(text, logger, important, delay?)`](Tool/Log.cs) | 输出日志并显示玩家屏幕提示           |
| [`NewLine()`](Tool/Log.cs)                              | 在控制台中插入空行               |
| [`Divider(char?, length?)`](Tool/Log.cs)                | 插入分隔线                   |

### GameConsole - 游戏控制台

[`GameConsole`](Tool/Console.cs) — 编程式执行游戏控制台命令。

```csharp
// 执行任意游戏内控制台指令
GameConsole.RunCommand("some_command arg1 arg2");
```

### World - 世界操作

[`World`](Tool/World.cs) — 世界操作工具。

| 方法                                                    | 说明                    |
|-------------------------------------------------------|-----------------------|
| [`PlaceBlock(x, y, block)`](Tool/World.cs)            | 在指定方块坐标放置方块           |
| [`PlaceBlock(vector2, block)`](Tool/World.cs)         | 在 Vector2 位置放置方块      |
| [`PlaceItem(x, y, item)`](Tool/World.cs)              | 在指定方块坐标生成物品           |
| [`PlaceItem(vector2, item)`](Tool/World.cs)           | 在 Vector2 位置生成物品      |
| [`PlaceBackground(pos, backgroundId)`](Tool/World.cs) | 放置背景图块（接受 Vector2）    |
| [`PlaceBackground(pos, backgroundId)`](Tool/World.cs) | 放置背景图块（接受 Vector2Int） |
| [`CreateTileMesh(pos)`](Tool/World.cs)                | 为背景渲染创建平铺网格           |
| [`CheckForWorld()`](Tool/World.cs)                    | 检查世界是否已加载，未加载则抛出异常    |
| [`ClearCache()`](Tool/World.cs)                       | 清除精灵/网格/材质缓存          |

### Player - 玩家操作

[`Player`](Tool/Player.cs) — 玩家操作工具。

| 方法                                                | 说明              |
|---------------------------------------------------|-----------------|
| [`Alert(text, important)`](Tool/Player.cs)        | 向玩家显示屏幕提示       |
| [`Alert(text, important, delay)`](Tool/Player.cs) | 延迟显示屏幕提示        |
| [`Tp(vector2)`](Tool/Player.cs)                   | 传送玩家（支持单人/多人模式） |
| [`Tp(x, y)`](Tool/Player.cs)                      | 传送至浮点数坐标        |
| [`PickItem(item, slot, force?)`](Tool/Player.cs)  | 将物品放入指定物品栏槽位    |

```csharp
// 示例
Player.Alert("小心！", true);                // 重要提示
Player.Tp(100.5f, 200.3f);                   // 传送
Player.PickItem("rifle", 0, true);           // 将步枪放入槽位 0
```

### Key - 输入处理

[`Key`](Tool/Key.cs) — 输入处理工具。

| 方法                                    | 说明                  |
|---------------------------------------|---------------------|
| [`HasKey(action)`](Tool/Key.cs)       | 检查按键绑定动作名是否存在       |
| [`IsKey(action)`](Tool/Key.cs)        | 检查按键当前是否被按住         |
| [`IsKeyDown(action)`](Tool/Key.cs)    | 检查按键是否在当前帧被按下       |
| [`IsKeyUp(action)`](Tool/Key.cs)      | 检查按键是否在当前帧被释放       |
| [`MouseWorldPosition()`](Tool/Key.cs) | 获取鼠标位置的世界坐标         |
| [`LeftClickPosition()`](Tool/Key.cs)  | 获取左键点击世界坐标（仅在点击帧有效） |
| [`RightClickPosition()`](Tool/Key.cs) | 获取右键点击世界坐标（仅在点击帧有效） |

**基于协程的等待方法** — 等待玩家点击后返回世界坐标：

| 方法                                                  | 说明                                |
|-----------------------------------------------------|-----------------------------------|
| [`WaitForLeftClick(Action<Vector2>)`](Tool/Key.cs)  | 协程：等待左键点击，通过回调返回坐标                |
| [`WaitForRightClick(Action<Vector2>)`](Tool/Key.cs) | 协程：等待右键点击，通过回调返回坐标                |
| [`WaitForLeftClick()`](Tool/Key.cs)                 | 协程：等待左键点击，返回 `WaitForClickResult` |
| [`WaitForRightClick()`](Tool/Key.cs)                | 协程：等待右键点击，返回 `WaitForClickResult` |

**`WaitForClickResult`** — 继承自 [
`CustomYieldInstruction`](https://docs.unity3d.com/ScriptReference/CustomYieldInstruction.html)，支持 yield return
等待，等待完成后通过 `.Result` 获取点击位置的世界坐标。

**按键动作常量** 定义在 [`Key.InputAction`](Tool/Key.cs) 中：

| 常量                       | 值                | 说明     |
|--------------------------|------------------|--------|
| `InputAction.LeftClick`  | `"attack"`       | 左键点击动作 |
| `InputAction.RightClick` | `"iteminteract"` | 右键点击动作 |

```csharp
// 检查当前帧是否左键点击
if (Key.IsKeyDown(Key.InputAction.LeftClick))
{
    Vector2 pos = Key.MouseWorldPosition();
    // 执行操作...
}

// ── 协程：等待玩家点击 ──
IEnumerator MyCoroutine()
{
    // 方式一：回调模式
    yield return Key.WaitForLeftClick(pos => {
        Player.Tp(pos);
    });

    // 方式二：结果模式（更清爽）
    var waiter = Key.WaitForRightClick();
    yield return waiter;
    Player.Tp(waiter.Result);
}
```

完整的按键动作名列表请参见 [`Constant/Keys`](Constant/Keys.cs)。

### Multiplayer - 多人游戏

[`Multiplayer`](Tool/Multiplayer.cs) — 通过 KrokoshaCasualtiesMP 实现多人游戏集成。

| 成员                                               | 说明                       |
|--------------------------------------------------|--------------------------|
| [`IsNetworkRunning`](Tool/Multiplayer.cs)        | 检查多人游戏网络是否运行             |
| [`IsClient`](Tool/Multiplayer.cs)                | 检查当前实例是否为客户端             |
| [`Tp(vector2)`](Tool/Multiplayer.cs)             | 在多人游戏中传送本地玩家             |
| [`Tp(x, y)`](Tool/Multiplayer.cs)                | 传送至浮点数坐标                 |
| [`Tp(playerName, vector2)`](Tool/Multiplayer.cs) | 传送指定玩家（使用 `"@a"` 传送所有玩家） |
| [`Tp(playerName, x, y)`](Tool/Multiplayer.cs)    | 传送指定玩家至浮点数坐标             |
| [`GetPlayerName(player)`](Tool/Multiplayer.cs)   | 获取玩家对象的名称                |

### Config - 配置

[`Config`](Tool/Config.cs) — BepInEx 配置项操作辅助。

| 方法                                                                         | 说明             |
|----------------------------------------------------------------------------|----------------|
| [`ChangeConfig(entry, value)`](Tool/Config.cs)                             | 设置配置项的值并保存     |
| [`SwitchType(configEntry, configName)`](Tool/Config.cs)                    | 切换布尔配置项的值      |
| [`SwitchType(configEntry, configName, logger, important)`](Tool/Config.cs) | 切换配置项并可选显示玩家提示 |

```csharp
// 切换配置项
ConfigEntry<bool> mySetting = Config.Bind("General", "MySetting", true, "说明");
Config.SwitchType(mySetting, "我的设置");
```

### RichText - 富文本

[`RichText`](Tool/RichText.cs) — Unity 富文本格式化工具，用于游戏控制台消息美化。

| 方法                                          | 说明                              |
|---------------------------------------------|---------------------------------|
| [`Color(text, color)`](Tool/RichText.cs)    | 用颜色名称包裹 `<color>` 标签            |
| [`Color(text, color)`](Tool/RichText.cs)    | 用 Unity `Color` 包裹 `<color>` 标签 |
| [`Hex(text, hex)`](Tool/RichText.cs)        | 用十六进制颜色包裹 `<color>` 标签          |
| [`Alpha(text, alphaHex)`](Tool/RichText.cs) | 用十六进制透明度设置文本透明度                 |
| [`Alpha(text, alpha)`](Tool/RichText.cs)    | 用浮点数（0–1）设置透明度                  |
| [`Alpha(text, alpha)`](Tool/RichText.cs)    | 用字节（0–255）设置透明度                 |
| [`Alpha(text, alpha)`](Tool/RichText.cs)    | 用整数（0–255）设置透明度                 |
| [`Bold(text)`](Tool/RichText.cs)            | 用 `<b>` 标签包裹文本                  |
| [`Italic(text)`](Tool/RichText.cs)          | 用 `<i>` 标签包裹文本                  |
| [`Size(text, size)`](Tool/RichText.cs)      | 用 `<size>` 标签设置字号               |

**快捷颜色方法：**

| 方法                                  | 颜色        |
|-------------------------------------|-----------|
| [`Blue(text)`](Tool/RichText.cs)    | `blue`    |
| [`Red(text)`](Tool/RichText.cs)     | `red`     |
| [`Green(text)`](Tool/RichText.cs)   | `green`   |
| [`Yellow(text)`](Tool/RichText.cs)  | `yellow`  |
| [`White(text)`](Tool/RichText.cs)   | `white`   |
| [`Black(text)`](Tool/RichText.cs)   | `black`   |
| [`Cyan(text)`](Tool/RichText.cs)    | `cyan`    |
| [`Magenta(text)`](Tool/RichText.cs) | `magenta` |
| [`Gray(text)`](Tool/RichText.cs)    | `gray`    |
| [`Orange(text)`](Tool/RichText.cs)  | `orange`  |
| [`Purple(text)`](Tool/RichText.cs)  | `purple`  |
| [`Pink(text)`](Tool/RichText.cs)    | `pink`    |
| [`Brown(text)`](Tool/RichText.cs)   | `brown`   |

```csharp
// 格式化彩色控制台消息
string msg = RichText.Color("警告：", "red") + RichText.Bold("生命值过低！");
Log.Info(msg, Plugin.Logger);

// 使用十六进制颜色
string fancy = RichText.Hex("你好", "#FF8800") + " " + RichText.Italic("世界");
```

### Tools - 工具函数

[`Tools`](Tool/Tools.cs) — 参数解析与验证工具函数。

| 方法                                                   | 说明                          |
|------------------------------------------------------|-----------------------------|
| [`CheckArgumentCount(args, desired)`](Tool/Tools.cs) | 确保参数数组至少有 `desired + 1` 个元素 |
| [`ParseFloat(s)`](Tool/Tools.cs)                     | 使用固定区域设置解析浮点数；失败时抛出异常       |
| [`ParseInt(s)`](Tool/Tools.cs)                       | 使用固定区域设置解析整数；失败时抛出异常        |
| [`TryParseFloat(s, out result)`](Tool/Tools.cs)      | 尝试解析浮点数，不抛出异常               |

```csharp
// 在指令处理器中
Tools.CheckArgumentCount(args, 2);          // 需要至少 2 个参数
float x = Tools.ParseFloat(args[1]);        // 解析浮点数参数
int count = Tools.ParseInt(args[2]);        // 解析整数参数
```

---

## 常量参考

[`Constant/`](Constant) 命名空间提供了游戏对象的强类型常量，让你可以更安全、更方便地引用方块、物品、背景和按键绑定，无需记忆字符串
ID。

### Blocks - 方块

[`Blocks`](Constant/Blocks.cs) — 强类型方块定义。可隐式转换为 `ushort`。

| 属性             | 类型       | 说明      |
|----------------|----------|---------|
| `Id`           | `ushort` | 方块 ID   |
| `LocaleKey`    | `string` | 本地化键    |
| `Health`       | `float`  | 方块耐久    |
| `HitSound`     | `string` | 被击打音效   |
| `StepSound`    | `string` | 行走音效    |
| `SleepQuality` | `string` | 睡眠质量评级  |
| `IsMetallic`   | `bool`   | 是否为金属   |
| `NoVariation`  | `bool`   | 是否有视觉变体 |
| `Toxicity`     | `float`  | 毒性等级    |
| `IsSlippery`   | `bool`   | 是否湿滑    |

使用 [`Blocks.FromId(ushort)`](Constant/Blocks.cs) 从 ID 解析对应的方块常量。

```csharp
// 使用强类型常量放置方块
World.PlaceBlock(10, 20, Blocks.Granite);
World.PlaceBlock(new Vector2(15, 25), Blocks.SteelTile);

// 隐式转换为 ushort
ushort blockId = Blocks.Copper; // 34
```

### Items - 物品

[`Items`](Constant/Items.cs) — 强类型物品定义。可隐式转换为 `string`。

| 属性         | 类型       | 说明                          |
|------------|----------|-----------------------------|
| `Id`       | `string` | 物品 ID                       |
| `Category` | `string` | 物品分类（food, medical, tool 等） |
| `Weight`   | `float`  | 物品重量                        |
| `Value`    | `int`    | 物品价值                        |
| `Rec`      | `int`    | 稀有度/配方等级                    |

使用 [`Items.FromId(string)`](Constant/Items.cs) 从 ID 解析对应的物品常量。

```csharp
// 使用强类型常量给予物品
Player.PickItem(Items.Rifle, 0, true);
Player.PickItem(Items.Bandage, 1);

// 隐式转换为 string
string itemId = Items.Rifle; // "rifle"
```

### Backgrounds - 背景

[`Backgrounds`](Constant/Backgrounds.cs) — 背景图块 ID 字符串常量。

| 常量                                                 | 值                       |
|----------------------------------------------------|-------------------------|
| [`Backgrounds.Fungal`](Constant/Backgrounds.cs)    | `"fungalBackground"`    |
| [`Backgrounds.Grass`](Constant/Backgrounds.cs)     | `"grassBackground"`     |
| [`Backgrounds.Ice`](Constant/Backgrounds.cs)       | `"iceBackground"`       |
| [`Backgrounds.Rock`](Constant/Backgrounds.cs)      | `"rockBackground"`      |
| [`Backgrounds.Sand`](Constant/Backgrounds.cs)      | `"sandBackground"`      |
| [`Backgrounds.Soil`](Constant/Backgrounds.cs)      | `"soilBackground"`      |
| [`Backgrounds.Steel`](Constant/Backgrounds.cs)     | `"steelBackground"`     |
| [`Backgrounds.Vents`](Constant/Backgrounds.cs)     | `"ventsBackground"`     |
| [`Backgrounds.Wasteland`](Constant/Backgrounds.cs) | `"wastelandBackground"` |

```csharp
// 使用常量放置背景
World.PlaceBackground(new Vector2Int(10, 20), Backgrounds.Grass);
World.PlaceBackground(new Vector2(15f, 25f), Backgrounds.Sand);
```

### Keys - 按键

[`Keys`](Constant/Keys.cs) — 强类型按键动作常量。可隐式转换为 `string`。

| 常量                                         | 动作名                 |
|--------------------------------------------|---------------------|
| [`Keys.Jump`](Constant/Keys.cs)            | `"jump"`            |
| [`Keys.Up`](Constant/Keys.cs)              | `"up"`              |
| [`Keys.Left`](Constant/Keys.cs)            | `"left"`            |
| [`Keys.Right`](Constant/Keys.cs)           | `"right"`           |
| [`Keys.Down`](Constant/Keys.cs)            | `"down"`            |
| [`Keys.Attack`](Constant/Keys.cs)          | `"attack"`          |
| [`Keys.ItemInteract`](Constant/Keys.cs)    | `"iteminteract"`    |
| [`Keys.Throw`](Constant/Keys.cs)           | `"throw"`           |
| [`Keys.ToggleInventory`](Constant/Keys.cs) | `"toggleinventory"` |
| [`Keys.Pause`](Constant/Keys.cs)           | `"pause"`           |
| [`Keys.Console`](Constant/Keys.cs)         | `"console"`         |
| [`Keys.Craft`](Constant/Keys.cs)           | `"craft"`           |

使用 [`Keys.FromAction(string)`](Constant/Keys.cs) 从动作名解析对应的按键常量。

```csharp
// 使用常量检查按键绑定
if (Key.IsKeyDown(Keys.Attack))
{
    // 玩家攻击了
}

// 隐式转换为 string
string action = Keys.Jump; // "jump"
```

---

## UI 工具包（开发中）

[`Tool/UI/`](Tool/UI) 目录包含正在开发中的 UI 创建工具，用于构建游戏内模组界面。目前**代码被注释掉**，正在积极开发中。

| 文件                                  | 说明                              |
|-------------------------------------|---------------------------------|
| [`UILayout`](Tool/UI/UILayout.cs)   | Canvas 管理、面板创建、RectTransform 辅助 |
| [`UIWidgets`](Tool/UI/UIWidgets.cs) | 按钮和文本创建，支持游戏风格样式和本地化            |

将在未来版本中启用。

---

## 项目结构

```
MossLib/
├── Plugin.cs                    # 插件主入口
├── Base/
│   ├── ModCommandBase.cs        # 指令注册基类
│   ├── ModLangGenBase.cs        # 语言生成器基类
│   └── ModLocaleBase.cs         # 本地化基类
├── Constant/
│   ├── Backgrounds.cs           # 背景 ID 常量
│   ├── Blocks.cs                # 强类型方块定义
│   ├── Items.cs                 # 强类型物品定义
│   └── Keys.cs                  # 按键动作名常量
├── Example/
│   ├── ModCommand.cs            # 指令实现示例
│   ├── ModLocale.cs             # 本地化实现示例
│   └── Lang/
│       ├── EnLangGenerator.cs   # 英语语言生成器
│       ├── ZhCnLangGenerator.cs # 简体中文生成器
│       └── ZhTwLangGenerator.cs # 繁体中文生成器
└── Tool/
    ├── Config.cs                # 配置操作工具
    ├── Console.cs               # 游戏控制台封装
    ├── Key.cs                   # 输入处理（按键绑定、鼠标点击等待）
    ├── LocaleGenerator.cs       # 语言文件生成器管理器
    ├── Log.cs                   # 控制台日志工具
    ├── Multiplayer.cs           # 多人游戏集成
    ├── Player.cs                # 玩家操作工具
    ├── RichText.cs              # 富文本格式化工具
    ├── Tools.cs                 # 参数解析工具
    ├── World.cs                 # 世界操作工具
    └── UI/                      # UI 工具包（开发中，代码被注释）
        ├── UILayout.cs          # Canvas 和面板创建
        └── UIWidgets.cs         # 按钮和文本创建
```
