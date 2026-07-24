[English](../en-US/getting-started.md) | ***简体中文***

# 快速上手

> 先确认你看过 [README_ZH](../../README_ZH.md) 了，知道 Bark 是什么、怎么装。

## 开始之前

- Bark 提供 API 给你调用，**不会直接修改游戏逻辑**。想改游戏行为？自己写 Harmony Patch。
- Bark 依赖 [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib)（CCL），**版本必须匹配**。装错了会炸。
- 出问题了？按这个顺序查：
  1. 重载/重启了没 → 2. 文件放对位置了没 → 3. 保存了没 → 4. 拼写/括号/分号
  全排完还是不行 → [排查指南](troubleshooting.md)

## 安装

1. **游戏**：Steam 最新版 Casualties Unknown
2. **CCL**：[NexusMods](https://www.nexusmods.com/scavprototype/mods/341?tab=files) / [GitHub Releases](https://github.com/jimmyking9999999/CUCoreLib/releases)
   > 想尝鲜可以试试 [Nightly Build](https://github.com/jimmyking9999999/CUCoreLib/actions)，但正式版不保证包含 Nightly 内容。
3. **Bark**：[NexusMods](https://www.nexusmods.com/scavprototype/mods/362?tab=files) / [GitHub Releases](https://github.com/CNCUMC/Bark/releases)
4. **IDE**：写脚本用 [VS Code](https://code.visualstudio.com/)，写模组用 [Rider](https://www.jetbrains.com/rider/) 或 [Visual Studio](https://visualstudio.microsoft.com/)

启动游戏，BepInEx 控制台出现 `[Bark]` 开头的日志即安装成功。

## 你想做什么

- 用 JS 或 Lua 写脚本 → [脚本开发](script-mod.md)
- 用 C# 写模组 → [C# 模组开发](csharp-mod.md)
- 查有哪些事件能接 → [脚本事件钩子](script-events.md) / [C# 事件](csharp-events.md)
- 查某个 API → [API 参考](api/)

祝你愉快！
