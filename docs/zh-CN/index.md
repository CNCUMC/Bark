[English](../en-US/index.md) | ***简体中文***

# Bark 文档

Bark 是 Casualties Unknown 的 BepInEx 模组工具库，提供事件系统、双脚本引擎（JS/Lua）、本地化和配置管理。

> 先看 [快速上手](getting-started.md)，知道怎么装、怎么选路径。

## 开发指南

| 文档                                          | 内容                                             |
|-----------------------------------------------|--------------------------------------------------|
| [快速上手](getting-started.md)                | 安装、环境、路径选择                             |
| [脚本开发](script-mod.md)                     | JS/Lua 脚本模组，生命周期、全局变量、控制台指令  |
| [C# 模组开发](csharp-mod.md)                  | C# 写模组，事件订阅、Harmony Patch、API 注册     |
| [配置与本地化](configuration.md)              | 选项注册、多语言，C# 和脚本两套体系              |
| [脚本事件钩子](script-events.md)              | 脚本侧可监听的所有事件钩子一览                   |
| [C# 事件系统](csharp-events.md)               | C# 侧事件订阅/触发/自定义                        |
| [自定义物品](script-mod/item.md)              | JSON 定义自定义物品与液体，自动注册              |
| [物品模板](script-mod/item-template/index.md) | 预设物品属性，简化 JSON，支持枪械/弹匣/弹药/弹壳 |
| [自定义音效](script-mod/audio.md)             | 音效文件加载、缓存、模板中引用                   |
| [自定义 Moodle](script-mod/moodle.md)         | JSON 定义自定义状态效果，自动注册                |
| [自定义合成表](script-mod/recipe.md)          | JSON 定义合成表配方，与物品系统联动              |
| [脚本命令](script-mod/command.md)             | JSON 注册控制台命令，输入后触发脚本 onCommand    |

## 脚本 API

脚本侧（JS / Lua）可调用的工具 API：

| 文档                                  | 对应的全局变量       | 覆盖内容                                 |
|---------------------------------------|----------------------|------------------------------------------|
| [身体](script-api/body.md)            | `Body`               | 血量、饥饿、口渴、体温、疲劳、意识、药物 |
| [玩家](script-api/player.md)          | `Player`             | 传送、拾取、提示、存档                   |
| [肢体](script-api/limbs.md)           | `Limb`               | 骨折、脱臼、感染、截肢、治疗             |
| [背包与物品](script-api/inventory.md) | `Inventory` / `Item` | 背包查询、物品搜索、耐久、修理           |
| [自定义状态](script-api/moodle.md)    | `Moodle`             | 状态效果应用、移除、查询                 |
| [技能](script-api/skills.md)          | `Skill`              | 经验值、等级                             |
| [世界编辑](script-api/world.md)       | `World`              | 方块放置、区域填充、物品生成             |
| [日志](script-api/log.md)             | `Log`                | 日志输出                                 |
| [多语言](script-api/locale.md)        | `Locale`             | 本地化文本、占位符                       |
| [配置项](script-api/options.md)       | `OptionsApi`         | 读写脚本模组配置                         |
| [输入](script-api/input.md)           | `Input`              | 鼠标位置、按键友好名称                   |
| [存储](script-api/storage.md)         | `Storage`            | 基于 PlayerPrefs 的持久化数据            |
| [压缩](script-api/compress.md)        | `Compress`           | GZip / Deflate 数据压缩                  |

## C# API

C# 模组可调用的工具 API：

| 文档                                  | 覆盖内容                      |
|---------------------------------------|-------------------------------|
| [EventUtil](csharp-api/event-util.md) | 事件触发、手动注册、清理      |
| [UpdateUtil](csharp-api/update.md)    | GitHub Releases 版本检查      |
| [SaveLoader](csharp-api/save.md)      | 保存系统，自定义存档 Provider |
