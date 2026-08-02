# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v2.2.0

### Added

- 物品模板系统：脚本模组可通过 JSON 模板定义自定义物品，不同模板类型使用各自的 schema。支持模板类型：gun、mag、ammo、casing。包含运行时跟踪（`GunMagTracker`、`GunRuntimeManager`）实现弹匣换弹、弹药消耗、弹壳抛射、开火音效等功能。
- `ItemUtil` 工具类，提供 `LoadSprite(path)` 和 `HexToColor(hex)` 辅助方法用于物品资产加载。
- 枪械音效档案系统（`GunSoundProfile`）：支持 JSON 定义多维音效（fire/rack/unrack/load_mag/load_shell/unload_mag/trigger/jam/safety），每条音效支持音量、音高、权重随机，音频文件自动预加载和缓存。
- 音效档案系统文档（`docs/zh-CN/script-mod/audio.md`、`docs/en-US/script-mod/audio.md`），覆盖档案 JSON schema、AudioManager API、简单模式与档案模式回退链、性能注意事项。

### Changed

- **Breaking** 物品 ID 从单一文件名（如 `ak47`）改为 `{模组ID}.{文件名}` 命名空间格式（如 `my_mod.ak47`），防止跨模组物品冲突。原版物品 ID（如 `bandage`、`pistol`）不受影响。

### Fixed

- 枪械模板抛壳：通过 `CasingTemplate` 注册的自定义弹壳现在能正确生成。旧版 Transpiler 仅替换了传给 `Resources.Load` 的弹壳 ID 字符串，但 `Resources.Load` 无法加载 CCL `CustomInstantiate` 注册的自定义物品。新版 Transpiler 完全绕过 `Resources.Load + Instantiate` 流程，直接调用 `Utils.Create` 通过 CCL 注册表生成自定义弹壳。
- 修复弹匣装填分支的音效回退代码被 `LoadShell` 逻辑意外覆盖的 bug。
- 逐发装弹音效：支持 `LoadShell` 音效档案，优先播放 profile 中的 `load_shell`，回退到默认 `"gunloadshell"`。
- 扳机/卡壳音效：通过 Transpiler 挂钩 `GunScript.Update()` 和 `GunScript.Fire()` 中的 `Sound.Play("guntrigger")` / `Sound.Play("gunjam")`，替换为 `GunSoundProfile` 优先的回调。
- 保险音效：通过 Prefix 挂钩 `GunScript.ToggleSafety()`，支持 `Safety` 音效档案，回退到默认 `"gunsafety"`。
