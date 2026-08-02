# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v2.2.0

### Added

- 物品模板系统：脚本模组可通过 JSON 模板定义自定义物品，不同模板类型使用各自的 schema。支持模板类型：gun、mag、ammo、casing。包含运行时跟踪（`GunMagTracker`、`GunRuntimeManager`）实现弹匣换弹、弹药消耗、弹壳抛射、开火音效等功能。
- `ItemUtil` 工具类，提供 `LoadSprite(path)` 和 `HexToColor(hex)` 辅助方法用于物品资产加载。

### Changed

- **Breaking** 物品 ID 从单一文件名（如 `ak47`）改为 `{模组ID}.{文件名}` 命名空间格式（如 `my_mod.ak47`），防止跨模组物品冲突。原版物品 ID（如 `bandage`、`pistol`）不受影响。

### Fixed

- 枪械模板抛壳：通过 `CasingTemplate` 注册的自定义弹壳现在能正确生成。旧版 Transpiler 仅替换了传给 `Resources.Load` 的弹壳 ID 字符串，但 `Resources.Load` 无法加载 CCL `CustomInstantiate` 注册的自定义物品。新版 Transpiler 完全绕过 `Resources.Load + Instantiate` 流程，直接调用 `Utils.Create` 通过 CCL 注册表生成自定义弹壳。
