# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v2.0.0

### Added

- 脚本系统：基于 PuerTS 的双引擎（V8 + Lua），支持热重载、AutoApi IL-emit 代理和模组依赖解析。
  详见 [docs/Script.md](docs/Script.md)。
- 事件系统：通过 `BarkEvent` + `[ScriptEvent]` 特性将游戏动作桥接到 C# 和脚本模组。
  详见 [docs/Event.md](docs/Event.md)。
