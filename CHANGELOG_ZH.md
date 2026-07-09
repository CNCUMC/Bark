# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v1.0.0

### 新增

- **项目更名**：从 [Moss Lib](https://github.com/Explosive-Hydra/Moss-Lib) 更名为 **Bark**
- 集成 CUCoreLib (CCL) 作为硬依赖 (`[BepInDependency("net.cucorelib")]`)
- **BetterLocale 系统** — 基于 CCL `LocaleRegistry` 的完整本地化基础设施
    - `GetItem`/`GetBuilding`/`GetMoodle`/`GetOther`/`GetLog`/`GetCommand`/`GetOption`/`GetLiquid`/`GetTitle` 获取本地化文本
    - `SetDefault` 注册 CCL 缺失时的回退翻译
    - `Flush()` 将所有默认值写入 CCL 语言目录 (`BepInEx/config/CUCoreLib/Locales/`)
    - `HasKey` / `HasKeyItem` / `HasKeyBuilding` 等检查翻译是否存在
    - `ToRichText(md)` / `StripMarkdown(md)` Markdown ↔ 富文本转换
- **ModLangGenBase** — 语言生成器基类
    - `Add(category, key, value)` 配合 `Item`/`Building`/`Moodle`/`Other`/`Option`/`Log`/`Command`/`Liquid`/`Title` 便捷方法
    - 生成器通过 `BetterLocale.SetDefault` 注册回退默认值
    - 包含 EN、zh-CN、zh-TW 示例生成器
- **BetterOptions** — CCL 设置注册封装
    - `Float`/`Int`/`Bool`/`Dropdown`/`Keybind` 全部 CCL 设置类型
    - 支持 `Setting.SettingCategory` 和 `string customCategory` 两种重载
- **UpdateUtil** — 基于 GitHub 的模组更新检测
    - `Check(githubRepo, modName, currentVersion, logger)` 通过 GitHub Releases API 异步检查更新
    - 使用 `UnityWebRequest`，`System.Version` 比较版本，通过 logger + 游戏控制台通知
    - 通过 `BetterLocale` 支持 EN/zh-CN/zh-TW 本地化消息
- **PlayerUtil** — 从 ScavLib-API 合并：传送、生命体征、药物、睡眠、恢复、Raw Writes、阈值常量、状态、外观
- **SkillUtil** — 从 ScavLib-API 合并：技能等级/经验操作、经验倍率
- **LimbUtil** — 从 ScavLib-API 合并：肢体操作、治疗、伤害、感染检查
- **ItemUtil** — 从 ScavLib-API 合并：FindNearby、FindClosest、SetCondition、Repair
- **LogUtil** — 统一日志 + 校验辅助：
    - 通过 `CUCoreUtils.ConsoleLog` 输出到控制台（反射方式，与 CCL 模式一致）
    - 待处理日志队列 — ConsoleScript 就绪前的消息自动排队，就绪后批量输出
    - `Info`/`Error`/`Warning` 同时输出到控制台和 BepInEx logger
    - `CheckWorld`/`CheckBody`/`CheckConsole`/`CheckArgumentCount`/`CheckNotNullOrEmpty`/`CheckParseFloat`/`CheckParseInt`
    - `PrintList`/`PrintNumberedList`/`PrintKeyValueList`/`PrintGroupedList` 格式化控制台输出
- **TextUtil** — 富文本工具：`SimpleMarkdown` 简易 Markdown 转 Unity 富文本
- **WorldUtil** — 方块/物品放置
- **InventoryUtil** — 物品栏槽位/物品管理
- **InputUtil** — 鼠标位置、点击等待
- **ToolsUtil** — 参数验证、浮点/整数解析
- **常量定义**：`Blocks`、`Items`、`Backgrounds`、`Keys`、`Slots`
- `Directory.Build.props` 用于共享游戏路径配置

### 变更

- 命名空间从 `MossLib.*` 迁移至 `Bark.*`
- **Tool/ 重命名为 `*Util.cs`** — 所有类统一 `XxxUtil` 命名
    - `Log` → `LogUtil`、`Config` → `ConfigUtil`、`Input` → `InputUtil`
    - `World` → `WorldUtil`、`Inventory` → `InventoryUtil`、`RichText` → `TextUtil`
    - `GamePlayer` + `PlayerUtil` 合并为 `PlayerUtil`
- **本地化目录** — 生成器写入 `BepInEx/config/CUCoreLib/Locales/`（CCL 目录）
- **BetterOptions** 直接传设置 ID 给 CCL（CCL 内部处理本地化）
- **BetterLocale.Get** — `args` 现在也会格式化查找结果，而非仅替换 key

### 移除

- Moss Lib 的 `ModLocaleBase`、`ModLangGenBase`、`ModCommandBase` 基类（由 CCL 替代）
- 旧的 `Lang/` 目录及 `EN.json`、`zh-CN.json`、`zh-TW.json` 文件（由 CCL 本地化系统替代）
- `LocaleGenerator` — 替换为直接调用 `ModLangGenBase.Initialize()`
- 所有 `MossLib.Example.*` 和 `MossLib.Tool.*` 命名空间引用

### 修复

- CCL 设置 `gameset` 条目不再在语言文件中重复
- `BetterLocale.Flush()` 将各分类（item/building/moodle/other/option）分开写入
- `option` 分类与 CCL 标准分类同级处理
- `Plugin.Logger` null-forgiving 初始化 (`= null!`) — 消除 CS8618 警告
