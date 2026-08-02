# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v2.2.0

### Added

- Item template system: script mods can define custom items via JSON templates with type-specific schemas. Supported template types: gun, mag, ammo, casing. Includes runtime tracking (`GunMagTracker`, `GunRuntimeManager`) for magazine reloads, ammo consumption, casing spawning, and firing sounds.
- `ItemUtil` utility class with `LoadSprite(path)` and `HexToColor(hex)` helpers for item asset loading.

### Changed

- **Breaking** Item ID now uses `{modId}.{filename}` namespaced format (e.g. `my_mod.ak47`) instead of bare filename (e.g. `ak47`) to prevent conflicts between script mods. Vanilla item IDs (e.g. `bandage`, `pistol`) are unaffected.

### Fixed

- Gun template casing spawn: custom casings (registered via `CasingTemplate`) now spawn correctly. Previously the Transpiler only replaced the item ID string passed to `Resources.Load`, which cannot load CCL `CustomInstantiate`-registered items. Now the Transpiler bypasses `Resources.Load + Instantiate` entirely and calls `Utils.Create` directly to spawn custom casings through the CCL registry.
