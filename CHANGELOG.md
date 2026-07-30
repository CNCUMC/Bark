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

- **Gun event system**: six `[ScriptEvent]` hooks covering the full firearm lifecycle — `onGunFire`,
  `onGunRack`, `onGunSafetyToggle`, `onGunLoadAmmo`, `onGunUnload`, and `onGunJam`. All events carry
  `GunItem` (the C# Item instance), with per-event fields such as `Suicide`, `Racked`, `Safe`, `AmmoItemId`,
  `Rounds`, and `RoundsUnloaded`. Implemented via Harmony patches on `GunScript` methods plus a 0.2 s polling
  coroutine for jam detection.

- `[EventBusSubscriber]`, `[ScriptEvent]`, and `[ScriptApi]` attributes now carry `[MeansImplicitUse]`
  (from Unity's built-in JetBrains.Annotations), telling Rider / VS+ReSharper / VS Code that the decorated
  types and their members are used via reflection — no more false "unused member" warnings across all IDEs.

- **`LimbUtil` name-based APIs**: `LimbNames` (public `HashSet<string>` of 15 known limbs, case-insensitive),
  `IsValidLimbName(name)` (`[ScriptMethod]` — returns `true` if the name matches a game limb), and
  `GetAllLimbNames()` (`[ScriptMethod]` — returns all valid limb names as a `List<string>`). These replace
  hard-coded limb-name lists with a single source of truth usable from both C# and scripts.

- **Wearable item `desired_wear_limb` validation**: `ItemLoader.FinalizeItemInfo` now validates
  `desired_wear_limb` against `LimbUtil.IsValidLimbName()`. Invalid limb names trigger a warning at load time
  (locale key `item_event.wear_slot_invalid`), catching misconfigured items before they can crash the game.

- **Worn sprite automatic fallback**: when `{itemId}_worn.png` is missing for a wearable item, Bark now
  falls back to the main item texture (`{itemId}.png`) as the worn sprite. The `WearableWithoutWornSprite`
  blacklist is only populated when both are absent, making `_worn.png` truly optional for most items.

- **WearWearable 3-layer defense** (`ItemEventListener`): the `Body.WearWearable` Harmony patch now has:
  1. **Prefix null checks** — blocks equip when `item`, `item.id`, or `Body` is null
  2. **Prefix runtime sprite check** — reads `item.stats.WornSprite` via `Traverse` and blocks if null
  3. **Finalizer exception swallowing** — catches any exception thrown inside `WearWearable`, logs an error,
     and returns null to prevent a game crash

  This prevents NullReferenceException crashes from invalid `wear_slot_id`, missing sprites, or other
  runtime issues.

### Changed

- **Options definition/data separation** (`OptionsUtil` rewrite): option definitions now live in
  `Mods/{modId}/options.json` (shipped with the mod, read-only), while user saved values are stored
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

- `LimbUtil.GetAllLimbs().Contains(limbName)` in `ItemLoader.FinalizeItemInfo` was comparing a
  `List<Limb>` against a `string`, always returning `false` and spurious warnings for every item with
  `desired_wear_limb`. Replaced with `LimbUtil.IsValidLimbName()`.
