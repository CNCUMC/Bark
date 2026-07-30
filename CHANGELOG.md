# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v2.0.1

### Added

- Python support placed on hold: the Python runtime (~22 MB) bundled by Puerts.Python.Complete is too large for the
  current release scope. The related implementation has been archived in `TodoPython/` and will be considered in a
  future major release if there is user demand.

### Fixed

- Lua script engine was missing the `Log:Info()` helper method, causing `Log:Info()` calls in Lua mods to fail.
- Wearable item loading threw an error when `_worn.png` texture was not present; now falls back gracefully instead of
  crashing.
