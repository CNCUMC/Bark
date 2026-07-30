# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v2.1.0

### Added

- **`[ScriptApi]` 注解驱动 API 系统**（`ScriptApiAttribute` + `ApiRegistry`）：替代手动脚本全局注入，改为
  自动发现。为静态工具类标记 `[ScriptApi]` 特性后，其 `[ScriptMethod]` 方法会被自动注册、通过运行时 IL
  发射生成代理、并以 camelCase 命名（如 `BodyUtil` → `Body`）注入所有脚本引擎。可选参数自动生成重载链表，
  Lua / JS 脚本可省略尾部默认参数。

- **自定义物块系统**（`TileLoader`、`TileScriptRegistry`、`TileScriptRunner`、`TileEventListener`）：脚本模组
  可通过 `Tile/*.json` 定义自定义物块（ID 从 36 自动分配），并绑定 4 个 `[ScriptEvent]` 钩子——
  `onTilePlace`、`onTileExist`、`onTileDamaging`、`onTileDestroyed`。检测基于 Harmony 补丁拦截
  `SetBlock` / `DamageBlock` 配合轮询协程兜底破坏检测。物块支持热重载（旧 ID 复用）。

- **命令系统**（`CommandLoader` + `CommandEvent`）：脚本模组可通过 `Command/*.json` 注册控制台命令，支持参数
  自动完成建议和可本地化的描述。执行时命令以 `CommandEvent` 桥接到所有已加载脚本引擎的 `onCommand` 钩子。

- **脚本本地化系统**（`ScriptLocaleManager` + `LocaleBridge`）：脚本模组语言文件从
  `ModDir/Lang/{langCode}.json` 加载，同时存入本地字典和 CCL 全局 `BetterLocale`，key 含命名空间前缀
  （如 `quantum.auto_rack`）避免冲突。`LocaleBridge` 委托模式解耦 `ScriptApi` 与 `Script` 的静态依赖。

- **`OptionsApi`**：脚本侧现在可通过 `Options.GetBool(id, key)`、`GetInt`、`GetFloat`、`GetDropdown`、
  `GetKeybind` 在运行时读取自身配置项，基于新的定义/数据分离架构。

- **物品攻击脚本**：`ItemScriptDef` 新增 `Attack` 字段；`ItemScriptRunner` 在脚本化物品用于攻击时分发
  `onItemAttack` 事件。

- **`SaveLoader`**：为脚本模组提供抽象化的存档访问层，封装游戏内部存档系统。

- **枪械事件系统**：6 个 `[ScriptEvent]` 钩子覆盖枪械操作全流程——`onGunFire`、`onGunRack`、
  `onGunSafetyToggle`、`onGunLoadAmmo`、`onGunUnload`、`onGunJam`。所有事件均携带 `GunItem`（C# Item
  实例），并提供 `Suicide`、`Racked`、`Safe`、`AmmoItemId`、`Rounds`、`RoundsUnloaded` 等专属字段。通过
  Harmony 补丁拦截 `GunScript` 方法实现，卡壳检测采用 0.2 秒轮询协程。

- `[EventBusSubscriber]`、`[ScriptEvent]` 和 `[ScriptApi]` 特性现在携带 `[MeansImplicitUse]`（来自 Unity
  内置的 JetBrains.Annotations），告知 Rider / VS+ReSharper / VS Code 被标记的类型及成员均通过反射使用——
  所有 IDE 不再误报「未使用」警告。

- `.editorconfig` 配置 `IDE0051` / `IDE0060` 抑制，为纯 Visual Studio 用户提供兜底方案。

### Changed

- **选项定义与数据分离**（`OptionsUtil` 重写）：选项定义现在存放在
  `Mods/{modId}/Config/options.json`（随模组分发，只读），用户保存值写入
  `ScriptMod/Configs/{modId}.json`（扁平 key-value）。加载时自动合并用户保存值覆盖默认值。
  避免了模组更新时覆盖用户设置，也让选项定义更便携。

- **`ScriptModLoader` 加载流程重构**：加载管线按明确顺序运行——
  ItemLoader → TileLoader → RecipeLoader → MoodleLoader → CommandLoader——热重载时有完整清理。
  每个阶段同时注册数据定义和对应的脚本。

- **脚本 API 注入统一**：所有工具类（`BodyUtil`、`InventoryUtil`、`ItemUtil`、`LimbUtil`、`MoodleUtil`、
  `PlayerUtil`、`ScriptUtil`、`SkillUtil`、`WorldUtil`）现通过 `ApiRegistry` 统一注册，
  以单一一致路径注入 PuerJS / PuerLua 引擎全局作用域。

- **`Plugin` 初始化流程梳理**：`AwakeInternal()` 现在按 `RegisterScriptApis()` →
  `EventRegistry.ScanAndRegister()` → `ScriptEventScanner.Scan()` → `ScriptModLoader.LoadAll()` 顺序
  编排子系统初始化。`OnDestroy()` 统一停止所有轮询监听器。

### Fixed

- `EventHandlers.OnMainMenuLoaded()` 为 `private static`，但 `EventRegistry.ScanAndRegister()` 只扫描
  `public static` 方法，导致版本更新检测从未触发。已改为 `public static`。

- `gun_event.patch_ok` 和 `api.scanned` 本地化条目缺少 `{0}` 占位符，导致 5 条枪械补丁日志全部输出
  相同的原始 key 而无法显示具体方法名。现已正确模板化。
