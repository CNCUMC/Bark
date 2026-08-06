# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v2.3.1

### Added

- 纯数据模组：没有入口脚本（`main.js` / `main.mjs` / `main.lua`）的模组现在会作为纯数据模组加载，
  可正常提供 JSON 内容（物品、物块、配方、情绪、命令），但不启动脚本引擎。
- 用于生成 / 放置 Bark 注册内容的新控制台指令：`script spawn` / `basp`（物品）、
  `script tile` / `bast`（物块）、`script moodle` / `basm`（情绪）。接受 Bark 注册的字符串 ID
  （如 `modid.entryname`），支持 Tab 自动补全，列表在 `script reload` 后自动刷新。
- `script detail` / `scd` 指令：查看某个已加载模组的 `mod.json` 元数据（作者、描述、所需 Bark/游戏版本、
  仓库、依赖）及其注册内容统计（物品 / 物块 / 配方 / 情绪数量）。
- Tab 自动补全现在按子命令类型切换候选（`script spawn` → 物品、`script tile` → 物块、
  `script moodle` → 情绪 key、`script detail` / `scd` → 模组 ID），不再把所有 ID 混在一起。

### Fixed

- 修复了 `script reload` 无法运行的问题
