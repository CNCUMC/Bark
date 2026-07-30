# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v2.1.0

### Added

- **`[ScriptApi]` annotation-driven API system** (`ScriptApiAttribute` + `ApiRegistry`): replace manual script
  global injection with automatic discovery. Mark a static utility class with `[ScriptApi]` and its
  `[ScriptMethod]` members are registered, proxied via runtime IL-emit, and injected into all script engines
  with camelCase naming (e.g. `BodyUtil` → `Body`). Optional-parameter overload chains are generated
  automatically so Lua / JS scripts can omit trailing default parameters.

- **Custom Tile system** (`TileLoader`, `TileScriptRegistry`, `TileScriptRunner`, `TileEventListener`):
  script mods can define custom tiles in `Tile/*.json` (auto-assigned IDs from 36), with per-tile scripts
  that respond to 4 `[ScriptEvent]` hooks — `onTilePlace`, `onTileExist`, `onTileDamaging`, `onTileDestroyed`.
  Detection leverages Harmony patches on `SetBlock` / `DamageBlock` plus a polling coroutine for destruction
  edge cases. Tiles are fully hot-reloadable (old IDs reused).

- **Command system** (`CommandLoader` + `CommandEvent`): script mods can register console commands via
  `Command/*.json` with parameter auto-complete suggestions and localizable descriptions. When invoked,
  commands fire a `CommandEvent` that bridges to every loaded script engine's `onCommand` hook.

- **Script-side locale system** (`ScriptLocaleManager` + `LocaleBridge`): script mod language files in
  `ModDir/Lang/{langCode}.json` are loaded per-mod and synced into both a local dictionary and CCL's
  global `BetterLocale`, with namespace-prefixed keys (e.g. `quantum.auto_rack`) to avoid collisions.
  A `LocaleBridge` delegate pattern decouples `ScriptApi` from `Script`.

- **`OptionsApi`**: scripts can now read their own config values at runtime via `Options.GetBool(id, key)`,
  `GetInt`, `GetFloat`, `GetDropdown`, `GetKeybind` — powered by the new definition/data separation.

- **Item attack script**: `ItemScriptDef` gained an `Attack` field; `ItemScriptRunner` dispatches
  `onItemAttack` when a scripted item is used to attack.

- **`SaveLoader`**: abstract save-file access for script mods, wrapping the game's internal save system.

- **Gun event system**: six `[ScriptEvent]` hooks covering the full firearm lifecycle — `onGunFire`,
  `onGunRack`, `onGunSafetyToggle`, `onGunLoadAmmo`, `onGunUnload`, and `onGunJam`. All events carry
  `GunItem` (the C# Item instance), with per-event fields such as `Suicide`, `Racked`, `Safe`, `AmmoItemId`,
  `Rounds`, and `RoundsUnloaded`. Implemented via Harmony patches on `GunScript` methods plus a 0.2 s polling
  coroutine for jam detection.

- `[EventBusSubscriber]`, `[ScriptEvent]`, and `[ScriptApi]` attributes now carry `[MeansImplicitUse]`
  (from Unity's built-in JetBrains.Annotations), telling Rider / VS+ReSharper / VS Code that the decorated
  types and their members are used via reflection — no more false "unused member" warnings across all IDEs.

- `.editorconfig` with `IDE0051` and `IDE0060` suppression as a fallback for vanilla Visual Studio users.

### Changed

- **Options definition/data separation** (`OptionsUtil` rewrite): option definitions now live in
  `Mods/{modId}/Config/options.json` (shipped with the mod, read-only), while user saved values are stored
  in `ScriptMod/Configs/{modId}.json` as a flat key-value map. On load, saved values are merged over
  defaults. This prevents user settings from being overwritten by mod updates and keeps definitions portable.

- **`ScriptModLoader` loading flow restructured**: the load pipeline now runs in a well-defined order —
  ItemLoader → TileLoader → RecipeLoader → MoodleLoader → CommandLoader — with proper cleanup for hot
  reload. Each stage registers both data definitions and per-item/tile/recipe scripts.

- **Script API injection unified**: all tool classes (`BodyUtil`, `InventoryUtil`, `ItemUtil`, `LimbUtil`,
  `MoodleUtil`, `PlayerUtil`, `ScriptUtil`, `SkillUtil`, `WorldUtil`) are now registered through
  `ApiRegistry` and injected as globals into PuerJS / PuerLua engines via a single consistent path.

- **`Plugin` initialization flow**: `AwakeInternal()` now orchestrates `RegisterScriptApis()` →
  `EventRegistry.ScanAndRegister()` → `ScriptEventScanner.Scan()` → `ScriptModLoader.LoadAll()` with
  clear subsystem initialization ordering. `OnDestroy()` stops all polling listeners cleanly.

### Fixed

- `EventHandlers.OnMainMenuLoaded()` was `private static` but `EventRegistry.ScanAndRegister()` only
  discovers `public static` methods, so the version update check was never actually firing. Changed to
  `public static`.

- `gun_event.patch_ok` and `api.scanned` locale keys lacked `{0}` placeholders, causing all 5 gun-patch
  log lines to print the identical raw key instead of individual method names. Now properly templated.
