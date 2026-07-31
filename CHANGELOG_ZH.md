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

- **枪械事件**：6 个 `[ScriptEvent]` 钩子覆盖枪械操作全流程——`onGunFire`、`onGunRack`、
  `onGunSafetyToggle`、`onGunLoadAmmo`、`onGunUnload`、`onGunJam`。所有事件均携带 `GunItem`（C# Item
  实例），并提供 `Suicide`、`Racked`、`Safe`、`AmmoItemId`、`Rounds`、`RoundsUnloaded` 等专属字段。通过
  Harmony 补丁拦截 `GunScript` 方法实现，卡壳检测采用 0.2 秒轮询协程。

- `[EventBusSubscriber]`、`[ScriptEvent]` 和 `[ScriptApi]` 特性现在携带 `[MeansImplicitUse]`（来自 Unity
  内置的 JetBrains.Annotations），告知 Rider / VS+ReSharper / VS Code 被标记的类型及成员均通过反射使用——
  所有 IDE 不再误报「未使用」警告。

- **`LimbUtil` 名称查询 API**：`LimbNames`（公开的 15 个已知肢体名 `HashSet<string>`，不区分大小写）、
  `IsValidLimbName(name)`（`[ScriptMethod]` — 校验名称是否匹配游戏肢体）和
  `GetAllLimbNames()`（`[ScriptMethod]` — 返回全部有效肢体名的 `List<string>`）。这些 API 替代了
  各处的硬编码肢体名列表，C# 和脚本侧统一使用同一数据源。

- **可穿戴物品校验**：`ItemLoader.FinalizeItemInfo` 现在通过 `LimbUtil.IsValidLimbName()` 校验
  `wearable.desired_limb`。**`slot_id` 和 `desired_limb` 均为必填**——缺失任一字段则禁用可穿戴属性，
  防止 CUCoreLib 内部 NRE。`slot_id` 是装备槽标识符（如 `"back"`、`"Head"`），`desired_limb` 必须
  为游戏 15 个肢体名之一。

- **装备贴图自动回退**：当可穿戴物品缺少 `{itemId}_worn.png` 时，Bark 自动使用物品主贴图
  （`{itemId}.png`）作为穿着贴图。仅当两者都不存在时才将物品加入 `WearableWithoutWornSprite`
  黑名单，让 `_worn.png` 对大多数物品真正可选。

### Changed

- **选项定义与数据分离**（`OptionsUtil` 重写）：选项定义现在存放在
  `Mods/{modId}/options.json`（随模组分发，只读），用户保存值写入
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

- **物品 JSON 格式整理**：`wearable.slot_id` 是装备槽位标识符，不是身体肢体名。`wearable.desired_limb`
  现为可穿戴物品**必填字段**。更新了中英文文档。

### Fixed

- `EventHandlers.OnMainMenuLoaded()` 为 `private static`，但 `EventRegistry.ScanAndRegister()` 只扫描
  `public static` 方法，导致版本更新检测从未触发。已改为 `public static`。

- `gun_event.patch_ok` 和 `api.scanned` 本地化条目缺少 `{0}` 占位符，导致 5 条枪械补丁日志全部输出
  相同的原始 key 而无法显示具体方法名。现已正确模板化。

- `ItemLoader.FinalizeItemInfo` 中的 `LimbUtil.GetAllLimbs().Contains(limbName)` 将
  `List<Limb>` 与 `string` 比较，始终返回 `false`，导致每个有 `desired_wear_limb` 的物品
  都触发虚假警告。已替换为 `LimbUtil.IsValidLimbName()`。

- **可穿戴物品装备 NullReferenceException**：删除了 Bark 对 `Body.WearWearable` 的冗余 Harmony 补丁
  （CUCoreLib 已自行 patch 此方法）。修正了 `FinalizeItemInfo` 中将 `slot_id` 错误校验为肢体名的问题，
  此前非肢体名槽位（如 `"back"`）无法装备。
