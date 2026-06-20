[English Guide](README.md)

# Bark

[GitHub](https://github.com/CNCUMC/Bark) | [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib)

_基于 [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib) 扩展的 [Casualties Unknown](https://store.steampowered.com/app/3624440/Casualties_Unknown_Demo/) 模组工具库。_

---

## 目录

- [概述](#概述)
- [安装](#安装)
- [快速开始](#快速开始)
- [本地化](#本地化)
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

---

## 概述

**Bark** 是一个针对 **Casualties Unknown** 的 BepInEx 模组工具库，在 [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib) (CCL) 基础上扩展了额外的工具和常量定义。包含以下模块：

| 模块 | 说明 |
|------|------|
| [`Log`](Tool/Log.cs) | 增强的游戏内控制台日志工具 |
| [`GameConsole`](Tool/Console.cs) | 编程式执行游戏控制台命令的封装 |
| [`World`](Tool/World.cs) | 世界操作：放置方块、物品、背景图块 |
| [`Player`](Tool/Player.cs) | 玩家操作：传送、屏幕提示、物品栏管理 |
| [`Key`](Tool/Key.cs) | 输入处理：按键绑定检查、鼠标点击等待、世界坐标转换 |
| [`Multiplayer`](Tool/Multiplayer.cs) | 多人游戏支持，通过反射集成 KrokoshaCasualtiesMP |
| [`Config`](Tool/Config.cs) | BepInEx 配置项辅助工具 |
| [`RichText`](Tool/RichText.cs) | Unity 富文本格式化：颜色、透明度、粗体、斜体、字号 |
| [`Tools`](Tool/Tools.cs) | 参数验证、浮点/整数解析工具 |
| [`Blocks`](Constant/Blocks.cs) | 强类型方块定义，包含属性 |
| [`Items`](Constant/Items.cs) | 强类型物品定义，包含属性 |
| [`Backgrounds`](Constant/Backgrounds.cs) | 背景 ID 字符串常量 |
| [`Keys`](Constant/Keys.cs) | 强类型按键动作常量 |
| [`Slots`](Constant/Slots.cs) | 物品栏槽位定义 |

---

## 安装

1. 为 Casualties Unknown 安装 [BepInEx 5.x](https://github.com/BepInEx/BepInEx)。
2. 安装 [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib) — 将 `CUCoreLib.dll` 放入 `BepInEx/plugins/CUCoreLib/`。
3. 从 [Releases](https://github.com/CNCUMC/Bark/releases) 页面下载最新版 `Bark.dll`。
4. 将 `Bark.dll` 放入 `BepInEx/plugins/` 文件夹。
5. （可选）如需多人游戏功能，需安装 **KrokoshaCasualtiesMP** 作为软依赖。

> **对于模组开发者：** 在项目中添加 `Bark.dll` 引用，并在插件类上添加 `[BepInDependency("org.cucnmc.bark")]` 特性。

---

## 快速开始

### 1. 添加依赖

```csharp
[BepInPlugin(Guid, Name, Version)]
[BepInDependency("net.cucorelib")]  // CCL 必需
[BepInDependency("org.cucnmc.bark")] // Bark 扩展 CCL
public class MyPlugin : BaseUnityPlugin
{
    // ...
}
```

### 2. 本地化（通过 CCL）

Bark 使用 [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib) 的 [`LocaleRegistry`](https://github.com/jimmyking9999999/CUCoreLib) 进行所有本地化。无需额外设置。

```csharp
using CUCoreLib.Registries;

// 在代码中直接使用 LocaleRegistry.Get
string text = LocaleRegistry.Get("other", "mykey", "Fallback English text");

// 带格式化参数
string msg = LocaleRegistry.Get("other", "mykey.arg", "Hello {0}!");
Console.WriteLine(string.Format(msg, "World"));
```

模组注册内容后，在游戏内运行 `createLocale` 命令生成基线 `EN.json` 文件：

```
createLocale
```

该文件写入 `BepInEx/config/CUCoreLib/Locales/EN.json`。翻译人员可据此创建对应语言文件（如 `CN-zh.json`）。

### 3. 注册自定义指令

```csharp
[HarmonyPatch(typeof(ConsoleScript))]
public class MyCommand
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

## 本地化

Bark 的所有本地化均由 [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib) 的 [`LocaleRegistry`](https://github.com/jimmyking9999999/CUCoreLib) 处理。

### 工作原理

1. 在代码中使用 `LocaleRegistry.Get("category", "key", "fallback")`
2. 运行 `createLocale` 控制台命令生成 `EN.json`
3. 将文件交给翻译人员，生成对应语言文件（如 `CN-zh.json`）
4. CCL 在运行时自动加载正确的语言文件

### 分类

| 分类 | 用途 |
|------|------|
| `item` | 物品名称和描述 |
| `building` | 建筑名称和描述 |
| `moodle` | 状态效果名称 |
| `other` | UI 文本、提示、控制台指令等 |

### 示例

```csharp
// 简单查询
string tooltip = LocaleRegistry.Get("other", "bark.tooltip.heat", "热到能熔化手套。");

// 带格式化参数
string msg = LocaleRegistry.Get("other", "bark.teleport.success", "已传送: {0} 到 {1}");
Console.WriteLine(string.Format(msg, "玩家1", "10,20"));
```

---

## 工具参考

### Log

`Bark.Tool.Log` — 增强的游戏内控制台日志。

| 方法 | 说明 |
|------|------|
| `Log.Info(text, logger)` | 同时输出到游戏控制台和 BepInEx 日志 |
| `Log.Error(text, logger)` | 输出错误日志，带 `[ERROR]` 前缀 |
| `Log.Warning(text, logger)` | 输出警告日志，带 `[WARNING]` 前缀 |
| `Log.LogToConsole(text)` | 直接写入游戏控制台 |
| `Log.Alert(text, logger, important, delay?)` | 屏幕显示提示 + 记录日志 |

### GameConsole

`Bark.Tool.GameConsole` — 编程式控制台命令执行。

| 方法 | 说明 |
|------|------|
| `GameConsole.RunCommand(key)` | 按名称执行已注册的控制台命令 |

### World

`Bark.Tool.World` — 世界操作工具。

| 方法 | 说明 |
|------|------|
| `World.PlaceBlock(x, y, blockId)` | 在指定位置放置方块 |
| `World.FillBlocks(startX, startY, endX, endY, blockId)` | 填充矩形区域为指定方块 |
| `World.PlaceItem(x, y, itemId)` | 在指定位置生成物品 |
| `World.PlaceBackground(pos, backgroundId)` | 放置背景图块 |
| `World.CheckForWorld()` | 检查世界是否已加载，否则抛出异常 |
| `World.ClearCache()` | 清除缓存的精灵和材质 |

### Player

`Bark.Tool.Player` — 玩家操作。

| 方法 | 说明 |
|------|------|
| `Player.Tp(x, y)` | 传送玩家到指定位置 |
| `Player.Alert(text, important, delay?)` | 在屏幕上显示提示信息 |
| `Player.PickItem(itemId, slot, force?)` | 将物品添加到物品栏指定槽位 |

### Key

`Bark.Tool.Key` — 输入处理。

| 方法 | 说明 |
|------|------|
| `Key.MouseWorldPosition()` | 获取鼠标在世界坐标中的位置 |
| `Key.LeftClickPosition()` | 获取左键点击位置（未点击则返回零） |
| `Key.WaitForLeftClick()` | 协程：等待左键点击 |
| `Key.WaitForRightClick()` | 协程：等待右键点击 |

### Multiplayer

`Bark.Tool.Multiplayer` — 通过反射实现的多人游戏支持。

| 属性 | 说明 |
|------|------|
| `Multiplayer.IsNetworkRunning` | 检查多人游戏会话是否活跃 |
| `Multiplayer.IsClient` | 检查是否为客户端 |
| `Multiplayer.Tp(playerName, x, y)` | 按名称传送玩家（支持 `@a` 传送所有玩家） |

### Config

`Bark.Tool.Config` — BepInEx 配置辅助工具。

| 方法 | 说明 |
|------|------|
| `Config.Register(...)` | 注册配置项，自动生成本地化描述 |

### RichText

`Bark.Tool.RichText` — Unity 富文本格式化。

| 方法 | 说明 |
|------|------|
| `RichText.Color(text, color)` | 用颜色标签包裹文本 |
| `RichText.Hex(text, hex)` | 用十六进制颜色包裹文本 |
| `RichText.Bold(text)` | 用粗体标签包裹文本 |
| `RichText.Size(text, size)` | 用字号标签包裹文本 |
| `RichText.Alpha(text, alpha)` | 设置文本透明度 |

### Tools

`Bark.Tool.Tools` — 工具函数。

| 方法 | 说明 |
|------|------|
| `Tools.CheckArgumentCount(args, min)` | 验证最小参数数量 |
| `Tools.ParseFloat(s)` | 带错误处理的浮点数解析 |
| `Tools.ParseInt(s)` | 带错误处理的整数解析 |

---

## 常量参考

### Blocks

`Bark.Constant.Blocks` — 强类型方块定义。

```csharp
ushort blockId = Blocks.SteelTile;        // 隐式转换为 ushort
float health = Blocks.Diamond.Health;     // 方块生命值
bool metallic = Blocks.SteelTile.IsMetallic;
Blocks block = Blocks.FromId(6);          // 按 ID 查找
```

### Items

`Bark.Constant.Items` — 强类型物品定义。

```csharp
string itemId = Items.Medkit;             // 隐式转换为 string
float weight = Items.Medkit.Weight;
string category = Items.Medkit.Category;  // "container"
Items item = Items.FromId("medkit");      // 按 ID 查找
```

### Backgrounds

`Bark.Constant.Backgrounds` — 背景 ID 常量。

```csharp
string bgId = Backgrounds.Rock;           // "rockBackground"
Backgrounds bg = Backgrounds.FromId("rockBackground");
```

### Keys

`Bark.Constant.Keys` — 按键绑定常量。

```csharp
KeyCode jumpKey = Keys.Jump;
KeyCode attackKey = Keys.Attack;
```
