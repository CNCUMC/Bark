# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v2.2.0

### Added

- Item template system: script mods can define custom items via JSON templates with type-specific schemas. Supported template types: gun, mag, ammo, casing, clothing, food. Includes runtime tracking (`GunMagTracker`, `GunRuntimeManager`) for magazine reloads, ammo consumption, casing spawning, and firing sounds.
- Clothing template: define custom clothing/armor items via JSON templates (`ClothingTemplate`) with `ClothingData` schema, supporting damage resistance, insulation, volume, and equipment categories. Includes documentation in `docs/en-US/script-mod/item-template/clothing.md`.
- Food template: define custom food/drink items via JSON templates (`FoodTemplate`), supporting nutrition values, spoilage, container items, and effects. Includes documentation in `docs/en-US/script-mod/item-template/food.md`.
- `ItemUtil` utility class with `LoadSprite(path)` and `HexToColor(hex)` helpers for item asset loading.
- Gun sound profile system (`GunSoundProfile`): JSON-defined multi-category sound profiles (fire/rack/unrack/load_mag/load_shell/unload_mag/trigger/jam/safety) with per-entry volume, pitch, and weighted random selection. Audio files are automatically preloaded and cached.
- Audio system documentation (`docs/zh-CN/script-mod/audio.md`, `docs/en-US/script-mod/audio.md`) covering profile JSON schema, AudioManager API, simple vs profile mode fallback chain, and performance notes.

### Changed

- **Breaking** Item ID now uses `{modId}.{filename}` namespaced format (e.g. `my_mod.ak47`) instead of bare filename (e.g. `ak47`) to prevent conflicts between script mods. Vanilla item IDs (e.g. `bandage`, `pistol`) are unaffected.

### Fixed

- Gun template casing spawn: custom casings (registered via `CasingTemplate`) now spawn correctly. Previously the Transpiler only replaced the item ID string passed to `Resources.Load`, which cannot load CCL `CustomInstantiate`-registered items. Now the Transpiler bypasses `Resources.Load + Instantiate` entirely and calls `Utils.Create` directly to spawn custom casings through the CCL registry.
- Fixed magazine reload sound fallback being accidentally overwritten by `LoadShell` logic.
- Shell-by-shell loading sound: supports `LoadShell` sound profile, preferring profile `load_shell` entries with fallback to default `"gunloadshell"`.
- Trigger/jam sounds: Transpiler hooks in `GunScript.Update()` and `GunScript.Fire()` replace hardcoded `Sound.Play("guntrigger")` / `Sound.Play("gunjam")` with `GunSoundProfile`-aware callbacks.
- Safety sound: Prefix hook on `GunScript.ToggleSafety()` supports `Safety` sound profile with fallback to default `"gunsafety"`.
