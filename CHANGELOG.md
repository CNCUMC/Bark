# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres
to [Semantic Versioning](https://semver.org/).

---

## v1.1.1

### Added

- **BetterLocale** — added `LocaleGetKeys` dictionary to track localization call counts per key.
- **`catfcabl` command** — now outputs both registration stats and call stats, formatted as `key: count`.

### Changed

- **BetterLocale.SetDefault** — registration count logic fixed: existing keys increment by 1, new keys are added with count 1.
- **BetterLocale.Get** — now tracks call counts for each localization key.
- **BetterLocale** — removed `LocaleCount` field; use `LocaleKeys.Count` and `LocaleGetKeys.Count` instead.
