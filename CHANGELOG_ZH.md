# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v1.0.3

### 新增

- **InventoryUtil** — 扩展为完整的 4×3×3 接口矩阵：
    - **手持**：`HasItemInHand(id/tag/category)`、`IsHandEmpty()`、`IsHandOccupied()`
    - **身上**：`GetItemsByTag/Category`、`GetItemInfosByTag/Category`
    - **装备**：`HasWearableByTag/Category`、`GetWearablesByTag/Category`、`GetWearableInfosByTag/Category`
    - **全身**：`HasItemThoroughByTag/Category`、`GetItemsThoroughByTag/Category`、`GetItemInfosThoroughByTag/Category`
    - **全量**：`GetAllItemsAll()`、`GetAllItemInfosAll()`（聚合 手持+身上+装备+全身，去重）

### 变更

- **LogUtil** — 移除损坏的 `[HarmonyPatch]` 特性（该特性导致 `Body.Start()` 抛出 `NullReferenceException: routine is null`）。控制台输出改用 `CUCoreUtils.ConsoleLog`（反射方式，与 CCL 模式一致）。新增待处理日志队列 — `ConsoleScript` 就绪前的消息自动排队，就绪后批量输出。

