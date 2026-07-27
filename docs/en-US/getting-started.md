***English*** | [简体中文](../zh-CN/getting-started.md)

# Getting Started

> Make sure you've read [README](../../README.md) first to understand what Bark is and how to install it.

## Before You Start

- Bark provides APIs for you to call. It does **not directly modify game logic**. Want to change game behavior? Write
  your own Harmony patches.
- Bark depends on [CUCoreLib](https://github.com/jimmyking9999999/CUCoreLib) (CCL) — **versions must match**. Wrong
  version will break things.
- Something went wrong? Check in this order:
    1. Reloaded / restarted? → 2. Files in the right place? → 3. Saved? → 4. Typos / brackets / semicolons Still
       broken? → Ask others.

## Installation

1. **Game**: Latest Casualties Unknown on Steam
2. **CCL**: [NexusMods](https://www.nexusmods.com/scavprototype/mods/341?tab=files) / [GitHub Releases](https://github.com/jimmyking9999999/CUCoreLib/releases)
> Try the [Nightly Build](https://github.com/jimmyking9999999/CUCoreLib/actions) for cutting-edge features, but stable
> releases may not include nightly content.

3. **Bark**: [NexusMods](https://www.nexusmods.com/scavprototype/mods/362?tab=files) / [GitHub Releases](https://github.com/CNCUMC/Bark/releases)
4. **IDE**: [VS Code](https://code.visualstudio.com/) for scripts, [Rider](https://www.jetbrains.com/rider/)
   or [Visual Studio](https://visualstudio.microsoft.com/) for C# mods

Start the game. If you see `[Bark]` prefixed messages in the BepInEx console, you're all set.

## What Do You Want to Do

- Write JS or Lua scripts → [Script Development](script-mod.md)
- Write C# mods → [C# Mod Development](csharp-mod.md)
- See available events → [Script Event Hooks](script-events.md) / [C# Events](csharp-events.md)
- Look up an API → [API Reference](script-api)

Happy modding!
