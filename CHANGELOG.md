# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v2.3.1

### Added

- Data-only mods: a mod without an entry script (`main.js` / `main.mjs` / `main.lua`) is now loaded
  as a data-only mod, providing JSON content (items, tiles, recipes, moodles, commands) without
  starting a script engine.
- Console commands to spawn/place Bark-registered content: `script spawn` / `basp` (item),
  `script tile` / `bast` (tile), `script moodle` / `basm` (moodle). Accepts Bark-registered string
  IDs (e.g. `modid.entryname`) with Tab auto-completion; the list refreshes after `script reload`.

### Fixed

- Fixed the issue of `script reload` not running