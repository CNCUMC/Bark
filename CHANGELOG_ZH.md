# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## [1.0.0] - 2026-06-20

### 变更

- **项目更名**：从 [Moss Lib](https://github.com/Explosive-Hydra/Moss-Lib) 更名为 **Bark**
- 命名空间从 `MossLib.*` 迁移至 `Bark.*`
- 所有源文件从 Moss-Lib 仓库迁移至独立的 Bark 仓库

### 新增

- 集成 CUCoreLib (CCL) 作为硬依赖 (`[BepInDependency("net.cucorelib")]`)
- 使用 [`LocaleRegistry`](https://github.com/jimmyking9999999/CUCoreLib) 替代旧的 `ModLocaleBase` / `ModLangGenBase` / `ModLangGen` 本地化系统
- 新增 `Bark.Core` 基类：`ModLocaleBase`、`ModLangGenBase`、`ModLocale`、`ModCommandBase`
- 通过 `LocaleRegistry.Get("category", "key", "fallback")` 支持 CCL 本地化
- 支持 CCL 的 `createLocale` 控制台命令生成 EN.json 基线文件

### 移除

- Moss Lib 的 `ModLocaleBase`、`ModLangGenBase`、`ModCommandBase` 基类（由 CCL 替代）
- 旧的 `Lang/` 目录及 `EN.json`、`zh-CN.json`、`zh-TW.json` 文件（由 CCL 本地化系统替代）
- `ModLocale` 单例封装（由 CCL 的 `LocaleRegistry` 替代）
- 所有 `MossLib.Example.*` 和 `MossLib.Tool.*` 命名空间引用

### 修复

- 本地化现使用 CCL 内置的语言文件系统实现自动语言检测
- 移除重复的本地化生成代码（CCL 通过 `createLocale` 命令处理）

---

## [0.1.0] - 2026-06-19

### 新增

- Moss Lib 初始发布
- `ModLocaleBase`：基于 JSON 的语言文件加载
- `ModLangGenBase`：从代码生成语言文件
- `ModCommandBase`：注册自定义控制台指令
- 工具类：Log、GameConsole、World、Player、Key、Multiplayer、Config、RichText、Tools
- 常量类：Blocks、Items、Backgrounds、Keys、Slots
- 英语、简体中文、繁体中文语言文件
