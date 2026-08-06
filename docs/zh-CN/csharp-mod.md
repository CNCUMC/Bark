[English](../en-US/csharp-mod.md) | ***简体中文***

# C# 模组开发

如果你更喜欢用 C# 而不是脚本语言， Bark 提供了一套完整的事件系统和工具 API，让你可以直接写 C# 代码扩展游戏功能。

## 创建项目

推荐用 CNCUMC 的 [Moss Template](https://github.com/CNCUMC/Moss-Template) 作为模板，它已经配好了 BepInEx + CCL 的引用，开箱即用。

在 `Plugin.Awake()` 里写你的初始化逻辑。

## 订阅事件

用 `[EventBusSubscriber]` 标记你的类，再写 `public static` 方法，参数是 `BarkEvent` 子类。Bark 启动时自动扫描并注册，不需要手动
`+=`。

```csharp
using Bark.Event;
using Bark.Events;

[EventBusSubscriber("org.cncumc.bark")]
public static class MyEventHandlers
{
    public static void OnPlayerJump(PlayerJumpStartEvent evt)
    {
        Logger.LogInfo($"玩家起跳，时间: {evt.Time}");
    }

    public static void OnPlayerDeath(PlayerDeathEvent evt)
    {
        Logger.LogWarning("玩家死了");
    }
}
```

- 类上打 `[EventBusSubscriber("你的 GUID")]`
- 方法必须是 `public static`，接受一个 `BarkEvent` 子类参数
- 方法名随意， Bark 依据参数类型匹配
- 一个类可以写多个处理方法，一个处理一个事件类型

完整事件类型列表见 [C# 事件一览](csharp-events.md)。

## 调用工具 API

`Tool/` 下的所有静态类都可以直接调用。C# 方法和脚本 API 一一对应，参考脚本侧文档即可：

| 类                           | 文档                                  |
|------------------------------|---------------------------------------|
| `BodyUtil`                   | [生理系统](script-api/body-system.md) |
| `PlayerUtil`                 | [玩家](script-api/player.md)          |
| `LimbUtil`                   | [肢体](script-api/limbs.md)           |
| `InventoryUtil` / `ItemUtil` | [背包与物品](script-api/inventory.md) |
| `SkillUtil`                  | [技能](script-api/skills.md)          |
| `WorldUtil`                  | [世界编辑](script-api/world.md)       |
| `LogUtil`                    | [日志](script-api/log.md)             |
| `OptionsApi`                 | [配置项](script-api/options.md)       |
| `Locale`                     | [多语言](script-api/locale.md)        |

```csharp
using Bark.Tool;

// 读写玩家生理数据
var hunger = BodyUtil.GetHunger();
BodyUtil.SetHunger(hunger + 10);

// 操作肢体
LimbUtil.Break(0);          // 折断第 0 号肢体
LimbUtil.Mend(0);           // 治疗第 0 号肢体

// 操作世界
WorldUtil.PlaceBlock("marble", 10, 5);

// 操作玩家
PlayerUtil.Teleport(100, 200);
PlayerUtil.Alert("你有一封新邮件", true);
```

所有方法空值安全：如果游戏状态不满足（比如世界没生成），方法会静默返回或记录警告日志，不会抛异常让游戏崩掉。

## 写 Harmony Patch

Bark 本身就是一个 Harmony 模组，`Plugin.Awake()` 里已经执行了 `_harmony.PatchAll()`，所以你的程序集里所有 `[HarmonyPatch]`
都会自动生效。

```csharp
using HarmonyLib;

[HarmonyPatch(typeof(Body), nameof(Body.Jump))]
public static class JumpPatch
{
    // 前缀：在 Body.Jump() 执行前触发
    public static void Prefix(Body __instance)
    {
        Logger.LogInfo($"角色 {__instance.name} 即将跳跃");
    }

    // 后缀：在 Body.Jump() 执行后触发
    public static void Postfix(Body __instance)
    {
        Logger.LogInfo($"角色 {__instance.name} 跳跃完成");
    }
}
```

> 💡 Harmony Patch 配合 `EventUtil.Trigger()` 使用是推荐模式——在 Patch 里发事件，让其他模组通过事件系统响应，而不是都在同一个
> Patch 里塞逻辑。

## 注册为脚本 API（高级）

如果你的 C# 模组想把自己的工具方法暴露给 JS/Lua 脚本调用，有两种方式。

### 使用 [ScriptApi] 自动注册（推荐）

工具类是纯 `public static` 方法的，给类打上 `[ScriptApi]` 即可——Bark 启动时自动扫描注册，不需要手动调用
`ApiRegistry.Register()`。

```csharp
using Bark.ScriptApi;

[ScriptApi]
public static class MyMathTool
{
    [ScriptMethod]
    public static int Double(int value) => value * 2;

    [ScriptMethod]
    public static string Greet(string name) => $"你好, {name}!";
}
```

脚本侧通过小驼峰命名的全局变量访问：`myMathTool.Double(5)`。

- 全局变量名默认取类名去掉 `Util` 后缀（如 `BodyUtil` → `Body`），也可通过 `[ScriptApi(Name = "MyApi")]` 手动指定。
- 不需要在 `Awake()` 里做任何事——Bark 的 `ApiRegistry.ScanAndRegister()` 会自动找到所有 `[ScriptApi]` 类。

### 手动注册（兼容旧方式）

如果需要在代码里控制注册时机或做额外初始化，仍可用 `ApiRegistry.Register()`：

```csharp
using Bark.ScriptApi;

public static class MyTool
{
    [ScriptMethod]
    public static int Double(int value) => value * 2;

    [ScriptMethod]
    public static string Greet(string name) => $"你好, {name}!";
}

// 在 Awake() 里注册
ApiRegistry.Register(typeof(MyTool));
```

注册后，脚本侧可以通过小驼峰命名的全局变量访问：`myTool.Double(5)`。

可选参数会自动生成重载（和 Bark 内置 API 行为一致）：

```csharp
[ScriptMethod]
public static void Alert(string text, bool important = false, float delay = 0f)
{
    // 脚本可以写 myTool.Alert("hello")、myTool.Alert("hello", true)、myTool.Alert("hello", true, 0.5)
}
```

## 触发自定义事件

```csharp
using Bark.Tool;

// 触发一个 Bark 事件，所有订阅者都会收到
EventUtil.Trigger<PlayerJumpStartEvent>();

// 或者用实例
var evt = new PlayerJumpStartEvent { Body = someBody, Camera = someCamera };
EventUtil.Trigger(evt);
```

详细 API 见 [EventUtil](csharp-api/event-util.md)。

## 版本检查

```csharp
using Bark.Tool;

// 在 Awake() 末尾调用，BepInEx 控制台会输出是否有新版本
UpdateUtil.Check("你的GitHub用户名/仓库名", "模组名", "当前版本", Logger);
```

详细 API 见 [UpdateUtil](csharp-api/update.md)。

## 加载 JSON 物品

除了写代码创建物品，你的 C# 模组也能像脚本模组一样，把物品定义写在 JSON 文件里，由 Bark 解析并注册到游戏的 `ItemRegistry`。
这样你可以用纯数据描述物品，复用 Bark 的物品模板系统（gun / mag / ammo / casing / clothing / food），无需手写注册代码。

### 目录结构

把物品 JSON 和贴图放到你的插件目录下，约定与脚本模组一致：

```
BepInEx/
  plugins/
    your_mod/                  ← 模组根目录（即你的 DLL 所在目录）
      your_mod.dll
      mod.json                 ← 模组清单（至少含 id）
      Item/                    ← 物品 JSON（文件名即物品本地名）
        my_rifle.json
        my_armor.json
      Assets/
        Item/                  ← 贴图等资产
          my_rifle.png
          my_armor.png
          my_armor_worn.png
```

- `Item/*.json`：每个文件定义一个物品，物品 ID 为 `{modId}.{文件名}`（例如 `your_mod.my_rifle`）。
- `Assets/Item/*.png`：贴图文件，命名规则与脚本模组相同（详见[物品文档](script-mod/item.md)）。

### mod.json

在模组根目录（DLL 所在目录）放一个 `mod.json`，至少包含 `id` 字段——它就是物品 ID 的命名空间前缀。
`id` 用 snake_case（如 `your_mod`），物品 ID 为 `{id}.{文件名}`（例如 `your_mod.my_rifle`）。
其余字段（`name`、`version` 等）为可选元数据，C# 端只读取 `id`。

```json
{
  "id": "your_mod",
  "name": "My C# Mod",
  "version": "1.0.0"
}
```

### 调用方式

在 `Plugin.Awake()` 里调用 `ItemLoaderApi` 即可，模组 id 来自 `mod.json`，无需在代码里硬编码。

```csharp
using Bark.Items;

public class MyPlugin : BaseUnityPlugin
{
    public void Awake()
    {
        // 方式一（推荐）：自动在 DLL 目录查找 mod.json
        var count = ItemLoaderApi.LoadFromPluginDirectory(GetType().Assembly.Location);
        Logger.LogInfo($"加载了 {count} 个物品");

        // 方式二：明确指定 mod.json 路径
        ItemLoaderApi.LoadFromManifest(Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, "mod.json"));

        // 方式三：从 BepInEx/plugins/{modName} 自动查找 mod.json
        ItemLoaderApi.LoadFromPlugins("your_mod");
    }
}
```

如果你不想用 `mod.json`，也可以直接传 `modId` 和根目录（底层接口）：

```csharp
ItemLoaderApi.Load("your_mod", Path.GetDirectoryName(GetType().Assembly.Location)!);
```

### 热重载 / 卸载

如果需要支持热重载或卸载模组，先调用 `Unload` 清除此前注册的物品，再重新加载：

```csharp
ItemLoaderApi.Unload("your_mod");
ItemLoaderApi.LoadFromPluginDirectory(GetType().Assembly.Location);
```

### 注意事项

- 物品 JSON 的格式、模板用法与脚本模组**完全一致**，可参考[物品模板文档](script-mod/item-template/index.md)。
- C# 模组的 JSON 中**不要包含 `script` 字段**——C# 模组没有脚本引擎，JSON 里的脚本绑定会被忽略并输出警告。
  若需要物品行为逻辑，请用 C# 的 `[EventBusSubscriber]` + `[HarmonyPatch]` 实现。
- 贴图资产从 `Assets/Item/` 加载，缺失时回退到 `origin_prefab` 引用的原版精灵。
