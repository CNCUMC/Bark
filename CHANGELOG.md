# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v2.3.0

### Added

- C# mod JSON item loading: plain BepInEx mods can now register custom items from JSON via `ItemLoaderApi`, reusing the same parse / template-merge / asset-load / registration pipeline as script mods. The mod id (item namespace prefix) is read from a `mod.json` placed next to the DLL; items are loaded from `{modRoot}/Item/*.json` with sprites from `{modRoot}/Assets/Item/`. APIs: `LoadFromManifest(path)`, `LoadFromPluginDirectory(assemblyLocation)`, `LoadFromPlugins(modName)`, `Load(modId, modDir)` (low-level), and `Unload(modId)` for hot reload.
- C# mod JSON item loading docs (`docs/en-US/csharp-mod.md`, `docs/zh-CN/csharp-mod.md`) covering directory layout, API usage, hot reload, and the no-`script`-field note.
