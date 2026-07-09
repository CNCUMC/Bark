# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v1.0.1

### 新增

- **UpdateUtil** — 基于 GitHub 的模组更新检测
    - `Check(githubRepo, modName, currentVersion, logger)` 通过 GitHub Releases API 异步检查更新
    - 使用 `UnityWebRequest`，`System.Version` 比较版本，通过 logger + 游戏控制台通知
    - 通过 `BetterLocale` 支持 EN/zh-CN/zh-TW 本地化消息（`log.update.*` 键）
- **LogUtil** 待处理控制台日志队列 — `ConsoleScript` 就绪前的消息自动排队，就绪后批量输出
- **LogUtil** 控制台输出改用 `CUCoreUtils.ConsoleLog`（反射方式，与 CCL 模式一致）

### 变更

- **BetterLocale.Get** — `args` 现在也会格式化查找结果中的占位符（locale 值中的 `{0}`、`{1}` 现在能被正确替换）

### 修复

- `Plugin.Logger` null-forgiving 初始化 (`= null!`) — 消除 CS8618 警告
- 多项文档完善（项目结构、缺失工具、版本号修正）
