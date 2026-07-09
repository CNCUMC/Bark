# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v1.0.0

### Added

- **Project renamed** from [Moss Lib](https://github.com/Explosive-Hydra/Moss-Lib) to **Bark**
- CUCoreLib (CCL) integration as hard dependency (`[BepInDependency("net.cucorelib")]`)
- **BetterLocale system** — complete localization infrastructure built on CCL's `LocaleRegistry`
    - `GetItem`/`GetBuilding`/`GetMoodle`/`GetOther`/`GetLog`/`GetCommand`/`GetOption`/`GetLiquid`/`GetTitle` for retrieving localized text
    - `SetDefault` for registering fallback translations when CCL has none
    - `Flush()` writes all defaults to CCL locale directory (`BepInEx/config/CUCoreLib/Locales/`)
    - `HasKey` / `HasKeyItem` / `HasKeyBuilding` / etc. for checking if a translation exists
    - `ToRichText(md)` / `StripMarkdown(md)` for Markdown ↔ Rich Text conversion
- **ModLangGenBase** — language generator base class
    - `Add(category, key, value)` with `Item`/`Building`/`Moodle`/`Other`/`Option`/`Log`/`Command`/`Liquid`/`Title` convenience methods
    - Generators register fallback defaults via `BetterLocale.SetDefault`
    - EN, zh-CN, zh-TW example generators included
- **BetterOptions** — CCL settings registration wrapper
    - `Float`/`Int`/`Bool`/`Dropdown`/`Keybind` for all CCL setting types
    - Both `Setting.SettingCategory` and `string customCategory` overloads
- **UpdateUtil** — GitHub-based mod update checker
    - `Check(githubRepo, modName, currentVersion, logger)` async update check via GitHub Releases API
    - Uses `UnityWebRequest`, compares `System.Version`, notifies via logger + game console
    - Localized messages (EN/zh-CN/zh-TW) via `BetterLocale`
- **PlayerUtil** — merged from ScavLib-API: teleport, vitals, drugs, sleep, recovery, raw writes, thresholds, states, appearance
- **SkillUtil** — merged from ScavLib-API: skill level/XP manipulation, XP multiplier
- **LimbUtil** — merged from ScavLib-API: limb operations, healing, damage, infection checks
- **ItemUtil** — merged from ScavLib-API: FindNearby, FindClosest, SetCondition, Repair
- **LogUtil** — unified logging + validation helpers:
    - Console output via `CUCoreUtils.ConsoleLog` (reflection-based, matches CCL pattern)
    - Pending log queue — messages before ConsoleScript is ready are queued and flushed automatically
    - `Info`/`Error`/`Warning` output to both console and BepInEx logger
    - `CheckWorld`/`CheckBody`/`CheckConsole`/`CheckArgumentCount`/`CheckNotNullOrEmpty`/`CheckParseFloat`/`CheckParseInt`
    - `PrintList`/`PrintNumberedList`/`PrintKeyValueList`/`PrintGroupedList` formatted console output
- **TextUtil** — Rich Text tools: `SimpleMarkdown` converts Markdown to Unity Rich Text
- **WorldUtil** — block/item placement
- **InventoryUtil** — inventory slot/item management
- **InputUtil** — mouse position, click waiting
- **ToolsUtil** — argument validation, float/int parsing
- **Constant definitions**: `Blocks`, `Items`, `Backgrounds`, `Keys`, `Slots`
- `Directory.Build.props` for shareable game path configuration

### Changed

- Migrated from `MossLib.*` namespaces to `Bark.*` namespaces
- **Tool/ renamed to `*Util.cs`** — all classes follow `XxxUtil` naming convention
    - `Log` → `LogUtil`, `Config` → `ConfigUtil`, `Input` → `InputUtil`
    - `World` → `WorldUtil`, `Inventory` → `InventoryUtil`, `RichText` → `TextUtil`
    - `GamePlayer` + `PlayerUtil` merged into `PlayerUtil`
- **Localization directory** — generators write to `BepInEx/config/CUCoreLib/Locales/` (CCL directory)
- **BetterOptions** passes setting ID directly to CCL (CCL handles locale internally)
- **BetterLocale.Get** — `args` now also format the resolved value, not just the key

### Removed

- Moss Lib `ModLocaleBase`, `ModLangGenBase`, `ModCommandBase` base classes (replaced by CCL)
- Old `Lang/` directory with `EN.json`, `zh-CN.json`, `zh-TW.json` files (replaced by CCL locale system)
- `LocaleGenerator` — replaced by direct `ModLangGenBase.Initialize()` calls
- All `MossLib.Example.*` and `MossLib.Tool.*` namespace references

### Fixed

- CCL settings `gameset` entries no longer duplicated in locale files
- `BetterLocale.Flush()` writes all categories (item/building/moodle/other/option) separately
- `option` category treated as top-level alongside CCL's standard categories
- `Plugin.Logger` null-forgiving initialization (`= null!`) — suppresses CS8618 warning
