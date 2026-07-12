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

### Changed

- **LogUtil** — removed broken `[HarmonyPatch]` attribute that caused `NullReferenceException: routine is null` at
  `Body.Start()`. Console output now uses `CUCoreUtils.ConsoleLog` (reflection-based, matches CCL pattern). Added
  pending log queue — messages before `ConsoleScript` is ready are queued and flushed automatically.
