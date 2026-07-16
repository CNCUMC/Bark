# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v1.0.3

### Added

- **InventoryUtil** — expanded to a complete 4×3×3 interface matrix:
    - **Hand**: `HasItemInHand(id/tag/category)`, `IsHandEmpty()`, `IsHandOccupied()`
    - **Body**: `GetItemsByTag/Category`, `GetItemInfosByTag/Category`
    - **Wearable**: `HasWearableByTag/Category`, `GetWearablesByTag/Category`, `GetWearableInfosByTag/Category`
    - **Thorough**: `HasItemThoroughByTag/Category`, `GetItemsThoroughByTag/Category`,
      `GetItemInfosThoroughByTag/Category`
    - **All**: `GetAllItemsAll()`, `GetAllItemInfosAll()` (aggregate Hand + Body + Wearable + Thorough, deduplicated)
- **CheckUtil** — extracted validation helpers from `LogUtil` into a dedicated class:
    - `CheckWorld(logger)`, `CheckBody(logger)`, `CheckConsole(logger)`
    - `CheckArgumentCount(args, minCount, logger)`, `CheckNotNullOrEmpty(value, paramName)`
    - `CheckParseFloat(parse, logger)`, `CheckParseInt(parse, logger)`
- **TextUtil** — font properties now cached with null-guard to avoid repeated `Resources.FindObjectsOfTypeAll` calls.
    - Font return nullable types with `Debug.LogWarning` on missing font.
    - Localized warning messages via `BetterLocale` (`log.textutil.tmp_unifont_not_found`,
      `log.textutil.unifont_not_found`).
- **LogUtil** — added `Debug`, `Fatal`, `Message` methods. Added internal locale-aware overloads (`Info(text, args)`,
  `Error(text, args)`, etc.).
- **`catfcabl`** command: Create a txt file containing all Bark localizations, file at `BepInEx\cache\catfcabl.txt`.

### Changed

- **LogUtil** — removed broken `[HarmonyPatch]` attribute that caused `NullReferenceException: routine is null` at
  `Body.Start()`. Console output now uses `CUCoreUtils.ConsoleLog` (reflection-based, matches CCL pattern). Added
  pending log queue — messages before `ConsoleScript` is ready are queued and flushed automatically.
- **CheckUtil.Fail** — now uses `LogUtil.Error` to output to both game console and BepInEx logger (previously only
  BepInEx).
- **ModLangGenBase** - Now it is mandatory to add the `NameSpace` namespace value.

### Fixed

- **CheckUtil.CheckArgumentCount** — fixed comparison (`<=` → `<`) and off-by-one in error message (`args.Length - 1` →
  `args.Length`).
- **BetterLocale.Replace** — fixed `IndexOutOfRangeException` when placeholder index exceeds `args` array length. Now
  outputs localized `Debug.LogWarning` via `log.betterlocale.placeholder_out_of_range` and preserves the original
  placeholder text.
