# 更新日志

本文件记录本项目所有重要变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/)，本项目遵循 [语义化版本控制](https://semver.org/)。

---

## v2.3.2

### 新增
- **内容目录支持子目录嵌套**：`Item/`、`Tile/`、`Recipe/`、`Moodle/`、`Command/` 目录现在支持任意层级的子目录嵌套。
  加载器改为递归扫描所有子目录，而不再只扫顶层。
  - 物品 ID、物块 ID、Moodle key、命令名**均保持不变**——不包含子目录路径（如 `Item/Gun/ak47.json` → 物品 ID
    `modid.ak47`）。子目录仅用于组织文件。
  - 精灵图片仍从 `Assets/Item/`、`Assets/Tile/`、`Assets/Moodle/` 平铺读取（不跟随 JSON 的子目录），与原有物品行为一致。

### 修复
- **嵌套路径下的物块脚本绑定**：`TileLoader` 原先通过从 JSON 路径向上回退两层来推导模组根目录，当物块 JSON 位于
  子目录时推导会出错。现改为显式传入 `modDir`，无论嵌套多深，脚本绑定与资产路径都能正确解析。
