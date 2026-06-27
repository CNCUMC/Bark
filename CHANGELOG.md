# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## [1.0.0] - Unreleased

### Added

- **Project renamed** from [Moss Lib](https://github.com/Explosive-Hydra/Moss-Lib) to **Bark**
- CUCoreLib (CCL) integration as hard dependency (`[BepInDependency("net.cucorelib")]`)
- **BetterLocale system** — complete localization infrastructure built on CCL's `LocaleRegistry`
    - `GetItem`/`GetBuilding`/`GetMoodle`/`GetOther` for retrieving localized text
    - `SetDefault` for registering fallback translations when CCL has none
    - `Flush()` writes all defaults to CCL locale directory (`BepInEx/config/CUCoreLib/Locales/`)
    - `HasKey` for checking if a translation exists
    - `CleanCclOther` migrates CCL's auto-generated `other.gameset*` entries to Bark's `option` section
- **ModLangGenBase** — language generator base class
    - `Add(category, key, value)` with `Item`/`Building`/`Moodle`/`Other`/`Option` convenience methods
    - Generators register defaults via `BetterLocale.SetDefault`
- **BetterOptions** — CCL settings registration wrapper
    - `Float`/`Int`/`Bool`/`Dropdown`/`Keybind` for all CCL setting types
    - Both `Setting.SettingCategory` and `string customCategory` overloads
- **GameInstances** — unified game instance access (Body, World, Console, PlayerCam)
- **PlayerUtil** — merged from ScavLib-API: vitals, drugs, sleep, recovery, raw writes, thresholds, states, appearance
- **SkillUtil** — merged from ScavLib-API: skill level/XP manipulation
- **LimbUtil** — merged from ScavLib-API: limb operations, healing, damage
- **ItemUtil** — merged from ScavLib-API: FindNearby, FindClosest, SetCondition, Repair
- **LogUtil** — unified logging + validation helpers: `CheckWorld`, `CheckBody`, `CheckConsole`, `CheckArgumentCount`,
  `CheckNotNullOrEmpty`, `CheckParseFloat`, `CheckParseInt`
- **TextUtil** — some RichText tools
    - `SimpleMarkdown` converts simple Markdown to Unity Rich Text
- `Directory.Build.props` for shareable game path configuration

### Changed

- Migrated from `MossLib.*` namespaces to `Bark.*` namespaces
- **Tool/ renamed to `*Util.cs`** — all classes follow `XxxUtil` naming convention
    - `Log` → `LogUtil`, `Config` → `ConfigUtil`, `Input` → `InputUtil`, `Console` → `ConsoleUtil`
    - `World` → `WorldUtil`, `Inventory` → `InventoryUtil`, `RichText` → `TextUtil`
    - `GamePlayer` + `PlayerUtil` merged into `PlayerUtil`
- **Localization directory** — generators write to `BepInEx/config/CUCoreLib/Locales/` (CCL directory)
- **BetterOptions** passes setting ID directly to CCL (CCL handles locale internally)
- **`GameInstances`** delegates to `CUCoreUtils.IsInWorld()` / `IsWorldGenerationReady()`

### Removed

- Moss Lib `ModLocaleBase`, `ModLangGenBase`, `ModCommandBase` base classes (replaced by CCL)
- Old `Lang/` directory with `EN.json`, `zh-CN.json`, `zh-TW.json` files (replaced by CCL locale system)
- `LocaleGenerator` — replaced by direct `ModLangGenBase.Initialize()` calls
- `GameInstances.TryGetBody()` — CCL's `CUCoreUtils.TryGetBody()` covers this
- All `MossLib.Example.*` and `MossLib.Tool.*` namespace references

### Fixed

- CCL settings `gameset` entries no longer duplicated in locale files
- `BetterLocale.Flush()` writes all categories (item/building/moodle/other/option) separately
- `option` category treated as top-level alongside CCL's standard categories
- Localization now uses CCL's built-in locale file system for automatic language detection

---

## [1.1.2] - 2026-06-19 (Moss Lib)

### Added

- Initial Moss Lib release
- `ModLocaleBase` with JSON-based locale file loading
- `ModLangGenBase` for generating locale files from code
- `ModCommandBase` for registering custom console commands
- Tool classes: Log, GameConsole, World, Player, Key, Multiplayer, Config, RichText, Tools
- Constants: Blocks, Items, Backgrounds, Keys, Slots
- EN, zh-CN, zh-TW locale files
