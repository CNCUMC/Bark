# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v1.1.1

### 新增

- **BetterLocale** — 新增 `LocaleGetKeys` 字典，用于统计每个本地化键的调用次数。
- **`catfcabl` 指令** — 现在同时输出注册统计和调用统计，格式为 `键: 次数`。

### 变更

- **BetterLocale.SetDefault** — 修复注册计数逻辑：已存在的键计数+1，新键添加并设置计数为1。
- **BetterLocale.Get** — 现在会统计每个本地化键的调用次数。
- **BetterLocale** — 移除 `LocaleCount` 字段；改用 `LocaleKeys.Count` 和 `LocaleGetKeys.Count` 获取数量。
