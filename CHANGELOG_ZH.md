# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v2.0.1

### Added

- Python 支持暂缓处理：Puerts.Python.Complete 绑定的 Python 运行时（~22 MB）包体过大，不适合当前发布规模。相关实现已归档至
  `TodoPython/`，如有需求会在大版本中重新启用。

### Fixed

- Lua 脚本引擎缺少 `Log:Info()` 辅助方法，导致 Lua 模组中 `Log:Info()` 调用失败。
- 可穿戴物品加载时缺少 `_worn.png` 纹理会报错，现已改为优雅降级而非崩溃。
