# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres to [Semantic Versioning](https://semver.org/).

---

## [1.0.0] - 2026-06-20

### Changed

- **Project renamed** from [Moss Lib](https://github.com/Explosive-Hydra/Moss-Lib) to **Bark**
- Migrated from `MossLib.*` namespaces to `Bark.*` namespaces
- All source files moved from Moss-Lib repository to standalone Bark repository

### Added

- CUCoreLib (CCL) integration as hard dependency (`[BepInDependency("net.cucorelib")]`)
- [`LocaleRegistry`](https://github.com/jimmyking9999999/CUCoreLib) for all localization, replacing the old `ModLocaleBase` / `ModLangGenBase` / `ModLangGen` system
- `Bark.Core` base classes: `ModLocaleBase`, `ModLangGenBase`, `ModLocale`, `ModCommandBase`
- CCL locale support via `LocaleRegistry.Get("category", "key", "fallback")`
- `createLocale` console command support (via CCL) for generating EN.json baseline files

### Removed

- Moss Lib `ModLocaleBase`, `ModLangGenBase`, `ModCommandBase` base classes (replaced by CCL)
- Old `Lang/` directory with `EN.json`, `zh-CN.json`, `zh-TW.json` files (replaced by CCL locale system)
- `ModLocale` singleton wrapper (replaced by CCL's `LocaleRegistry`)
- `ModCommand` base class registration via `BepInDependency("blackmoss.mosslib")`
- All `MossLib.Example.*` and `MossLib.Tool.*` namespace references

### Fixed

- Localization now uses CCL's built-in locale file system for automatic language detection
- Removed duplicate locale generation code (CCL handles this via `createLocale` command)

---

## [0.1.0] - 2026-06-19

### Added

- Initial Moss Lib release
- `ModLocaleBase` with JSON-based locale file loading
- `ModLangGenBase` for generating locale files from code
- `ModCommandBase` for registering custom console commands
- Tool classes: Log, GameConsole, World, Player, Key, Multiplayer, Config, RichText, Tools
- Constants: Blocks, Items, Backgrounds, Keys, Slots
- English, Simplified Chinese, and Traditional Chinese locale files
