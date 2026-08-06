# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v2.3.0

### Added

- C# 模组 JSON 物品加载：普通 BepInEx 模组现在可通过 `ItemLoaderApi` 从 JSON 注册自定义物品，复用与脚本模组完全相同的解析 / 模板合并 / 资产加载 / 注册流程。模组 id（物品命名空间前缀）从 DLL 同目录下的 `mod.json` 读取；物品从 `{模组根目录}/Item/*.json` 加载，贴图从 `{模组根目录}/Assets/Item/` 读取。提供 `LoadFromManifest(path)`、`LoadFromPluginDirectory(assemblyLocation)`、`LoadFromPlugins(modName)`、`Load(modId, modDir)`（底层）以及热重载用的 `Unload(modId)`。
- C# 模组 JSON 物品加载文档（`docs/en-US/csharp-mod.md`、`docs/zh-CN/csharp-mod.md`），涵盖目录结构、API 用法、热重载，以及 JSON 不含 `script` 字段的注意事项。
