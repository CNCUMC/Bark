# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v2.0.0

### Added

- Script system via PuerTS (V8 + Lua dual backends) with hot-reload, AutoApi IL-emit proxies, and
  mod dependency resolution. See [docs/Script.md](docs/Script.md).
- Event system bridging game actions to C# and script mods via `BarkEvent` + `[ScriptEvent]` attributes.
  See [docs/Event.md](docs/Event.md).
