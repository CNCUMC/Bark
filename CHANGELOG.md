# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v2.3.2

### Added
- **Subdirectory nesting for content folders**: The `Item/`, `Tile/`, `Recipe/`, `Moodle/`, and `Command/`
  directories now support arbitrary subdirectory nesting. The loaders recursively scan all subdirectories instead of
  only the top level.
  - Item IDs, tile IDs, moodle keys, and command names are **unchanged** — they never include the subdirectory path
    (e.g. `Item/Gun/ak47.json` → item ID `modid.ak47`). Subdirectories are purely for organizing files.
  - Sprite assets are still read flat from `Assets/Item/`, `Assets/Tile/`, `Assets/Moodle/` (not from the JSON's
    subdirectory), matching the existing item behavior.

### Fixed
- **Tile script binding on nested paths**: `TileLoader` previously derived the mod root directory by walking two levels
  up from the JSON path. This broke when a tile JSON lived in a subdirectory. It now receives `modDir` explicitly, so
  script binding and asset paths resolve correctly regardless of nesting depth.