# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v2.3.0

### Added

- C# 模组 JSON 内容加载：普通 BepInEx 模组现在可通过 `ModContentApi` 从 JSON 注册自定义**物品、物块、配方、状态**，复用与脚本模组完全相同的解析 / 模板合并 / 资产加载 / 注册流程。模组 id 从 DLL 同目录下的 `mod.json` 读取；内容从 `{模组根目录}/{Item|Tile|Recipe|Moodle}/*.json` 加载，贴图从 `{模组根目录}/Assets/{Item|Tile|Moodle}/` 读取。提供 `LoadFromManifest(path)`、`LoadFromPluginDirectory(assemblyLocation)`、`LoadFromPlugins(modName)`、`Load(modId, modDir)`（底层）以及热重载用的 `Unload(modId)`。配置项用 BetterOptions、命令用 CCL 的 `ConsoleCommandRegistry` 直接注册（不走 JSON）。
- C# 模组 JSON 内容加载文档（`docs/en-US/csharp-mod.md`、`docs/zh-CN/csharp-mod.md`），涵盖目录结构、API 用法、热重载，以及 JSON 不含 `script` 字段的注意事项。
- JSON 物品现已覆盖全部 `CustomItemInfo` 字段：新增 `light`（LightProperties）、`bandage`（BandageProperties）、`syringe`（SyringeProperties）、`tool`（ToolProperties）、`spawn_components`（List<string>）、`icon_animation_id`、`worn_sprite_animation_id`、`held_sprite_offset`，以及 `custom_data` 映射到物品构建器。新增字段说明见 `docs/en-US/script-mod/item.md` / `docs/zh-CN/script-mod/item.md`。

### Fixed

- 枪械抛壳：模板枪现正确抛出匹配的自定义弹壳（`CasingTemplate`），而非原版 `casing` 预制体。`GunScript.Update` 的 transpiler 抛壳分支已重写以匹配当前游戏 IL——三元表达式 `roundInChamber == Casing ? "casing" : AmmoTypeToItem(...)` 由 `beq` 分支编译，因此 `ldstr "casing"` 后紧跟的是 `call Resources.Load` 而非 `br`。旧的匹配逻辑因找不到 `br` 提前返回、整段跳过重写，导致自定义弹壳注入从未生效。同时要求弹壳物品的 `casing_type` 与弹药的 `casing_type` 一致。
- 枪膛退实弹：从枪膛退出一发实弹（`roundInChamber == Round` 的拉栓分支）时，现通过 `CustomInstantiate` 生成匹配的自定义弹药，而非原版 `AmmoTypeToItem`（其固定产出默认的 5.56 弹药）。`GunScript.Update` 的 transpiler false 分支现改由 `DoSpawnRound` 处理。
