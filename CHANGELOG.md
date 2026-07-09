# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v1.0.1

### Added

- **UpdateUtil** — GitHub-based mod update checker
    - `Check(githubRepo, modName, currentVersion, logger)` async update check via GitHub Releases API
    - Uses `UnityWebRequest`, compares `System.Version`, notifies via logger + game console
    - Localized messages (EN/zh-CN/zh-TW) via `BetterLocale` (`log.update.*` keys)
- **LogUtil** pending console log queue — messages before `ConsoleScript` is ready are queued and flushed automatically
- **LogUtil** console output via `CUCoreUtils.ConsoleLog` (reflection-based, matches CCL pattern)

### Changed

- **BetterLocale.Get** — `args` now also format the resolved locale value, not just the key (`{0}`, `{1}` placeholders in locale values are now properly replaced)

### Fixed

- `Plugin.Logger` null-forgiving initialization (`= null!`) — suppresses CS8618 warning
- Various documentation refinements (project structure, missing tools, version corrections)
