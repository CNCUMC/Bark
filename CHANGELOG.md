# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v2.3.0

### Added

- C# mod JSON content loading: plain BepInEx mods can now register custom **items, tiles, recipes and moodles** from JSON via `ModContentApi`, reusing the same parse / template-merge / asset-load / registration pipeline as script mods. The mod id is read from a `mod.json` next to the DLL; content is loaded from `{modRoot}/{Item|Tile|Recipe|Moodle}/*.json` with sprites from `{modRoot}/Assets/{Item|Tile|Moodle}/`. APIs: `LoadFromManifest(path)`, `LoadFromPluginDirectory(assemblyLocation)`, `LoadFromPlugins(modName)`, `Load(modId, modDir)` (low-level), and `Unload(modId)` for hot reload. Config options use BetterOptions and commands use CCL's `ConsoleCommandRegistry` directly (not JSON).
- C# mod JSON content loading docs (`docs/en-US/csharp-mod.md`, `docs/zh-CN/csharp-mod.md`) covering directory layout, API usage, hot reload, and the no-`script`-field note.
- Full `CustomItemInfo` field coverage for JSON items: added `light` (LightProperties), `bandage` (BandageProperties), `syringe` (SyringeProperties), `tool` (ToolProperties), `spawn_components` (List<string>), `icon_animation_id`, `worn_sprite_animation_id`, `held_sprite_offset`, and `custom_data` mapping to the item builder. See `docs/en-US/script-mod/item.md` / `docs/zh-CN/script-mod/item.md` for the new field reference.

### Fixed

- Gun casing ejection: custom-template guns now eject the matching custom casing (`CasingTemplate`) instead of the vanilla `casing` prefab. The `GunScript.Update` transpiler's casing branch was rewritten to match the current game IL (the ternary `roundInChamber == Casing ? "casing" : AmmoTypeToItem(...)` is compiled with a `beq` branch, so `ldstr "casing"` is directly followed by `call Resources.Load` rather than a `br`). The previous matcher bailed out early and left the vanilla path intact, so the custom-casing injection never ran. Also requires the casing item's `casing_type` to match the ammo's `casing_type`.
- Chamber round ejection: ejecting a live round from the chamber (the `roundInChamber == Round` rack branch) now spawns the matching custom ammo via `CustomInstantiate` instead of the vanilla `AmmoTypeToItem` (which always produced the default 5.56 round). The `GunScript.Update` transpiler's false branch now routes through `DoSpawnRound`.
