***English*** | [简体中文](../zh-CN/index.md)

# Bark Documentation

Bark is a BepInEx mod utility library for Casualties Unknown, providing an event system, dual-scripting engine (JS/Lua),
localization, and configuration management.

> Start with [Getting Started](getting-started.md) for installation and path selection.

## Development Guides

| Document                                         | Content                                                          |
|--------------------------------------------------|------------------------------------------------------------------|
| [Getting Started](getting-started.md)            | Installation, environment, choosing your path                    |
| [Script Development](script-mod.md)              | JS/Lua mods, lifecycle hooks, global variables, console commands |
| [C# Mod Development](csharp-mod.md)              | C# mods, event subscription, Harmony patches, API registration   |
| [Configuration & Localization](configuration.md) | Options registration, multi-language, C# and script systems      |
| [Script Event Hooks](script-events.md)           | All event hooks listenable from scripts                          |
| [C# Event System](csharp-events.md)              | Event subscription / trigger / custom events from C#             |
| [Custom Items](items.md)                         | Define custom items & liquids via JSON, auto registration        |
| [Custom Recipes](recipe.md)                      | Define crafting recipes via JSON, integrate with custom items    |

## Script API

Script-side (JS / Lua) tool APIs:

| Document                                     | Global Variable              | Coverage                                                          |
|----------------------------------------------|------------------------------|-------------------------------------------------------------------|
| [Body System](script-api/body-system.md)     | `BodyUtil`                   | Blood, hunger, thirst, temperature, fatigue, consciousness, drugs |
| [Player](script-api/player.md)               | `PlayerUtil`                 | Teleport, pickup, alerts, save                                    |
| [Limbs](script-api/limbs.md)                 | `LimbUtil`                   | Fractures, dislocations, infections, dismemberment, healing       |
| [Inventory & Items](script-api/inventory.md) | `InventoryUtil` / `ItemUtil` | Inventory queries, item search, durability, repair                |
| [Skills](script-api/skills.md)               | `SkillUtil`                  | XP, levels                                                        |
| [World Editing](script-api/world.md)         | `WorldUtil`                  | Block placement, area fill, item spawning                         |
| [Logging](script-api/log.md)                 | `Log`                        | Log output                                                        |
| [Localization](script-api/locale.md)         | `Locale`                     | Localized text, placeholders                                      |
| [Options](script-api/options.md)             | `OptionsApi`                 | Reading mod configuration                                         |

## C# API

C# mod tool APIs:

| Document                              | Coverage                                    |
|---------------------------------------|---------------------------------------------|
| [EventUtil](csharp-api/event-util.md) | Event trigger, manual registration, cleanup |
| [UpdateUtil](csharp-api/update.md)    | GitHub Releases version check               |
