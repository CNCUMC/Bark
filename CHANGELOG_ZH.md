# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v1.0.0

### 新增

- **项目更名**：从 [Moss Lib](https://github.com/Explosive-Hydra/Moss-Lib) 更名为 **Bark**
- 集成 CUCoreLib (CCL) 作为硬依赖 (`[BepInDependency("net.cucorelib")]`)
- **BetterLocale 系统** — 基于 CCL `LocaleRegistry` 的完整本地化基础设施
    - `GetItem`/`GetBuilding`/`GetMoodle`/`GetOther` 获取本地化文本
    - `SetDefault` 注册 CCL 缺失时的回退翻译
    - `Flush()` 将所有默认值写入 CCL 语言目录 (`BepInEx/config/CUCoreLib/Locales/`)
    - `HasKey` 检查翻译是否存在
    - `CleanCclOther` 将 CCL 自动生成的 `other.gameset*` 条目迁移到 Bark 的 `option` 分类
- **ModLangGenBase** — 语言生成器基类
    - `Add(category, key, value)` 配合 `Item`/`Building`/`Moodle`/`Other`/`Option` 便捷方法
    - 生成器通过 `BetterLocale.SetDefault` 注册默认值
- **BetterOptions** — CCL 设置注册封装
    - `Float`/`Int`/`Bool`/`Dropdown`/`Keybind` 全部 CCL 设置类型
    - 支持 `Setting.SettingCategory` 和 `string customCategory` 两种重载
- **GameInstances** — 统一游戏实例访问入口 (Body, World, Console, PlayerCam)
- **PlayerUtil** — 从 ScavLib-API 合并：生命体征、药物、睡眠、恢复、Raw Writes、阈值常量、状态、外观
- **SkillUtil** — 从 ScavLib-API 合并：技能等级/经验操作
- **LimbUtil** — 从 ScavLib-API 合并：肢体操作、治疗、伤害
- **ItemUtil** — 从 ScavLib-API 合并：FindNearby、FindClosest、SetCondition、Repair
- **LogUtil** — 统一日志 + 校验辅助：`CheckWorld`、`CheckBody`、`CheckConsole`、`CheckArgumentCount`、`CheckNotNullOrEmpty`、
  `CheckParseFloat`、`CheckParseInt`
- **TextUtil** — 一些富文本工具
    - `SimpleMarkdown` 简易 Markdown 转 Unity 富文本
- `Directory.Build.props` 用于共享游戏路径配置

### 变更

- 命名空间从 `MossLib.*` 迁移至 `Bark.*`
- **Tool/ 重命名为 `*Util.cs`** — 所有类统一 `XxxUtil` 命名
    - `Log` → `LogUtil`、`Config` → `ConfigUtil`、`Input` → `InputUtil`、`Console` → `ConsoleUtil`
    - `World` → `WorldUtil`、`Inventory` → `InventoryUtil`、`RichText` → `TextUtil`
    - `GamePlayer` + `PlayerUtil` 合并为 `PlayerUtil`
- **本地化目录** — 生成器写入 `BepInEx/config/CUCoreLib/Locales/`（CCL 目录）
- **BetterOptions** 直接传设置 ID 给 CCL（CCL 内部处理本地化）
- **`GameInstances`** 委托给 `CUCoreUtils.IsInWorld()` / `IsWorldGenerationReady()`

### 移除

- Moss Lib 的 `ModLocaleBase`、`ModLangGenBase`、`ModCommandBase` 基类（由 CCL 替代）
- 旧的 `Lang/` 目录及 `EN.json`、`zh-CN.json`、`zh-TW.json` 文件（由 CCL 本地化系统替代）
- `LocaleGenerator` — 替换为直接调用 `ModLangGenBase.Initialize()`
- `GameInstances.TryGetBody()` — CCL 的 `CUCoreUtils.TryGetBody()` 已覆盖
- 所有 `MossLib.Example.*` 和 `MossLib.Tool.*` 命名空间引用

### 修复

- CCL 设置 `gameset` 条目不再在语言文件中重复
- `BetterLocale.Flush()` 将各分类（item/building/moodle/other/option）分开写入
- `option` 分类与 CCL 标准分类同级处理
- 本地化现使用 CCL 内置的语言文件系统实现自动语言检测

---

## v1.1.2

### 新增

- Moss Lib 初始发布
- `ModLocaleBase`：基于 JSON 的语言文件加载
- `ModLangGenBase`：从代码生成语言文件
- `ModCommandBase`：注册自定义控制台指令
- 工具类：Log、GameConsole、World、Player、Key、Multiplayer、Config、RichText、Tools
- 常量类：Blocks、Items、Backgrounds、Keys、Slots
- 英语、简体中文、繁体中文语言文件
