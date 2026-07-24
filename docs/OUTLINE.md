# Bark 文档大纲

> 本文档是文档体系的蓝图，规划每一页覆盖的内容范围。
> 正式文档位于 `zh/` 和 `en/` 目录下，按语言独立维护。

---

## 文档结构

```
docs/
  OUTLINE.md               ← 本文件，大纲蓝图
  zh/
    index.md               ← 中文入口/导航
    csharp-events.md       ← C#事件一览 
    csharp-mod.md          ← C# 模组开发   
    getting-started.md     ← 快速上手
    script-mod.md          ← 脚本开发
    script-events.md       ← 钩子一览
    api/
      body-system.md       ← BodyUtil（基础生理系统）
      player.md            ← PlayerUtil（状态/移动/物品/药物/警觉/恢复/阈值）
      world.md             ← WorldUtil
      inventory.md         ← InventoryUtil + ItemUtil
      limbs.md             ← LimbUtil + BodyUtil
      skills.md            ← SkillUtil
      options.md           ← OptionsApi（脚本侧读/写配置）
      locale.md            ← LocaleApi（多语言）
      log.md               ← LogApi + LogUtil
      event-util.md        ← EventUtil（脚本侧事件 API）
      update.md            ← UpdateUtil
    configuration.md       ← 选项注册 + 本地化数据编写
    architecture.md        ← 架构概览
  en/
    index.md               ← English entry
    ...（与 zh/ 结构镜像）
```

---

## 各页内容清单

### 1. index.md — 入口导航

- 一句话定位：Bark 是 Casualties Unknown 的 BepInEx 模组框架，支持 C# / JavaScript / Lua 三种语言开发模组
- 两个读者入口卡片：
  - "我要用 JS/Lua 写脚本" → script-mod.md
  - "我要用 C# 写模组" → csharp-mod.md
- 版本 + 安装条件（BepInEx + CUCoreLib 版本要求）

---

### 2. getting-started.md — 快速上手

**JS 脚本最快上手：**
- 第一步：找到 `ScriptMod/Mods/` 目录
- 第二步：创建 `MyMod/mod.json` + `main.js`
- 第三步：`function onLoad() { log.info("Hello!"); }`
- 第四步：启动游戏看 BepInEx 控制台输出

**C# 模组最快上手：**
- CNCUMC 的官方模板 [Moss Template](https://github.com/CNCUMC/Moss-Template)
- [CCL 文档](https://cucorelib.web.app/)

---

### 3. csharp-mod.md — C# 模组开发

**事件订阅：**
- `[EventBusSubscriber(Plugin.Guid)]` 标记类
- public static 方法，参数为 BarkEvent 子类 = 自动注册
- 代码示例：订阅 JumpStartEvent

**调用工具 API：**
- 所有 `Tool/` 下的 static 类可直接调用
- 示例：`PlayerUtil.Vitals.GetHealth()` / `BodyUtil.IsAlive()`

**写 Harmony Patch：**
- 简述 Harmony 是什么
- Patch 示例：在某个游戏方法前后插入逻辑
- 结合事件系统：Patch 内调用 `EventUtil.Trigger(new MyEvent())`

**注册为脚本 API（高级）：**
- `[ScriptMethod]` 标记你自己的工具方法
- `ApiRegistry.Register(typeof(MyTool))` 注册
- 之后 JS/Lua 脚本就能调用

---

### 4. script-mod.md — 脚本开发

JS 为主示例语言，Lua 差异单独备注。不再拆分 JS/Lua 两份文档。

**4.1 mod.json 规范**：字段表（id/name/version/author/description/bark_version/game_version/dependencies）。语言由入口文件扩展名自动判定，不需要 engine/main 字段。

**4.2 生命周期钩子**：onLoad / onEnable / onDisable / onUnload / onUpdate，带注意事项

**4.3 全局注入变量**：11 个工具类 → 全局变量映射表。名字全部 PascalCase（`BodyUtil`、`PlayerUtil`、`Log`、`Locale`、`ScriptInfo`）

**4.4 API 命名规则**（代替逐方法表格）：
- `Get*` / `Set*` — 读取/设置数值（几乎一一对应）
- `Is*` / `Has*` / `Can*` — 布尔状态查询
- `Add*` / `Remove*` — 增减/移除
- `Register*` / `Unregister*` — 注册/注销
- `Place*` / `Fill*` — 世界编辑
- `Kill` / `Resurrect` / `Break` / `Mend` / `Repair` — 特殊操作
- 可选参数可省略、枚举传数字

**4.5 事件钩子**：引用 script-events.md，给两个示例函数（onPlayerJumpStart / onLimbBroken），强调钩子不能有参数

**4.6 完整示例**：自动回血 + 受伤弹提示，setInterval 模式

**4.7 Lua 备注**：方法调用用 `:`、函数定义语法、入口文件固定 main.lua

---

### 5. script-events.md + csharp-events.md — 事件一览

拆成两页：`script-events.md`（脚本钩子 + EventUtil）和 `csharp-events.md`（C# 事件类型 + EventBus 订阅）。

**5.1 玩家事件**

| C# 类型                | 脚本钩子            | 属性                         | 触发描述   |
|------------------------|---------------------|------------------------------|------------|
| `PlayerJumpStartEvent` | `onPlayerJumpStart` | `Body body`, `Camera camera` | 按下跳跃键 |
| `PlayerJumpOverEvent`  | `onPlayerJumpOver`  | `Body body`, `Camera camera` | 起跳后触地 |
| `PlayerDeathEvent`     | `onPlayerDeath`     | `Body body`, `Camera camera` | 玩家死亡   |

**5.2 肢体事件**

| C# 类型                 | 脚本钩子             | 属性                               | 触发描述 |
|-------------------------|----------------------|------------------------------------|----------|
| `LimbBrokenEvent`       | `onLimbBroken`       | `int limbIndex`, `string limbName` | 骨骼断裂 |
| `LimbMendedEvent`       | `onLimbMended`       | `int limbIndex`, `string limbName` | 骨骼修复 |
| `LimbDislocatedEvent`   | `onLimbDislocated`   | `int limbIndex`, `string limbName` | 关节脱臼 |
| `LimbUnDislocatedEvent` | `onLimbUnDislocated` | `int limbIndex`, `string limbName` | 脱臼复位 |
| `LimbDismemberedEvent`  | `onLimbDismembered`  | `int limbIndex`, `string limbName` | 肢体断裂 |
| `LimbInfectedEvent`     | `onLimbInfected`     | `int limbIndex`, `string limbName` | 伤口感染 |

**5.3 世界事件**

| C# 类型               | 脚本钩子           | 属性                    | 触发描述     |
|-----------------------|--------------------|-------------------------|--------------|
| `MainMenuLoadedEvent` | `onMainMenuLoaded` | 无                      | 进入主菜单   |
| `WorldReadyEvent`     | `onWorldGenerated` | `WorldGeneration world` | 世界生成完毕 |

**5.4 C# 侧：EventUtil 脚本 API**

- `EventUtil.Trigger(BarkEvent evt)` — 触发自定义事件
- `EventUtil.Trigger<T>() where T : BarkEvent, new()` — 泛型触发
- `EventUtil.On<T>(Action<T> callback, string guid)` — 脚本侧注册事件监听
- `EventUtil.UnregisterAll(string guid)` — 注销模组所有监听

---

### 6. api/ — API 参考

**不做逐方法表格**。所有工具类遵循统一命名前缀（`Get`/`Set`/`Is`/`Has`/`Add`/`Remove`/...），
见 script-mod.md § 命名规则。API 页只需说明：

- 这个工具做什么
- 特殊的非标准命名方法（如 `Kill`、`Resurrect`、`PlaceBlock`）
- 一两段典型示例

#### 6.1 api/body-system.md — BodyUtil

角色生理系统，87 个方法。按功能分 6 组简述：状态检测 / 数值查询 / 数值设置 / 身体操作 / 状态施加 / 属性。

#### 6.2 api/player.md — PlayerUtil

传送、拾取、Alert 提示。方法少，直接给示例。

#### 6.3 api/world.md — WorldUtil

4 类方法：PlaceBlock / FillBlocks / PlaceItem / GetBlock。给一个画方块墙的完整示例。

#### 6.4 api/inventory.md — InventoryUtil + ItemUtil

InventoryUtil 查背包，ItemUtil 找地上东西 + 修装备。

#### 6.5 api/limbs.md — LimbUtil + BodyUtil

LimbUtil 操作肢体（骨折/脱臼/感染/出血/截肢），BodyUtil 补充手部/面部状态。

#### 6.6 api/skills.md — SkillUtil

GetXP / SetXP / AddXP / GetLevel / SetLevel，5 个方法一行说完。

#### 6.7 api/options.md — OptionsApi

GetBool / GetInt / GetFloat / GetDropdown / GetKeybind，读配置项。

#### 6.8 api/locale.md — LocaleApi

Get / GetFormatted / HasKey，多语言文本。

#### 6.9 api/log.md — LogApi + LogUtil

LogApi 脚本侧 `log.info()`，LogUtil C# 侧校验方法。

#### 6.10 api/event-util.md — EventUtil

Trigger / Trigger\<T\> / On\<T\> / UnregisterAll，脚本侧触发/监听事件。

#### 6.11 api/update.md — UpdateUtil

GitHub Release 版本检查，本地化消息。

---

### 7. configuration.md — 配置系统

**7.1 选项注册（BetterOptions）**
- Bool / Int / Float / Dropdown / Keybind 注册方法
- SettingCategory 分类
- 代码示例：注册一个布尔开关、一个浮点滑块

**7.2 本地化数据编写**
- LangGenerator 继承 ModLangGenBase
- BuildLocaleData 方法
- Other / Item / Option / Moodle / Log 等本地化帮助方法
- 刷新到 CCL locale 文件

**7.3 脚本模组的本地化**
- ScriptLocaleManager 的工作方式
- locale.json 格式规范
- 示例：在 mod.json 同级放 locale.zh.json / locale.en.json

---

### 8. architecture.md — 架构概览

**8.1 分层图（ASCII 或 mermaid）**
```
┌─────────────────────────────────┐
│  Script Mods (JS/Lua)           │
├─────────────────────────────────┤
│  Script Engine (PuerTS)         │
├─────────────────────────────────┤
│  Event System (Event/)          │
│  ScriptApi (AutoApi proxy)      │
│  Tool/ utilities                │
├─────────────────────────────────┤
│  BetterCCL (BetterLocale/Opts)  │
│  CUCoreLib (CCL)                │
├─────────────────────────────────┤
│  Casualties Unknown (game)      │
└─────────────────────────────────┘
```

**8.2 依赖流向**
- Events → Event → Tool（单向）
- ScriptApi → Script（桥接解耦）
- 禁止反向依赖

**8.3 关键设计决策**
- 为什么用 IL-emit 代理而不是反射调用：性能 + 可选参数重载
- 为什么事件系统用 Attribute 扫描而不是手动注册：零配置，减少模组作者心智负担
- 为什么 ScriptEventScanner 在 Script/ 而不在 Event/：它本质是脚本引擎的一部分，桥接 C# 事件 → 脚本钩子

---

## 文风规范

| 方面     | 做法                                                      |
|----------|-----------------------------------------------------------|
| 语言     | 中文撰写，API 签名、代码示例保留英文                      |
| 跳转     | 快捷切换到其他语言的文档，也能一键提issues                |
| 语气     | 平易近人，目标是让人看得懂                                |
| 段落     | 每段 3-5 句，超过就拆                                     |
| 标题     | 动词开头："读取玩家血量""注册一个事件"，不要"关于 xxx"    |
| 例子     | 提到的内容必给出例子，末尾给出例子项目的链接              |
| 代码块   | 必标语言 \`\`\`js / \`\`\`lua / \`\`\`csharp / \`\`\`json |
| 提示块   | `> ⚠️` 警告 / `> ℹ️` 说明 / `> 💡` 技巧                    |
| API 表格 | 统一格式：方法签名 \| 参数 \| 返回值 \| 说明              |
| 示例优先 | 先给可运行的完整代码，再解释做了什么                      |
| 链接     | 相对路径，不要硬编码 github url                           |

---

## 编写优先级

| 优先级 | 文档               | 理由                       |
|--------|--------------------|----------------------------|
| P0     | getting-started.md | 新人唯一入口，决定第一印象 |
| P0     | script-mod.md      | 最大用户群（JS/Lua 作者）  |
| P1     | script-events.md + csharp-events.md | 核心扩展点，C# 和脚本都用  |
| P1     | api/body-system.md | 调用频率最高的 API         |
| P1     | api/player.md      | 同上                       |
| P2     | api/world.md       |                            |
| P2     | api/inventory.md   |                            |
| P2     | api/limbs.md       |                            |
| P2     | api/options.md     |                            |
| P2     | api/locale.md      |                            |
| P2     | api/log.md         |                            |
| P3     | csharp-mod.md      | C# 开发者少                |
| P3     | api/skills.md      | 方法少                     |
| P3     | api/event-util.md  | 方法少                     |
| P3     | api/update.md      | 方法少                     |
| P4     | configuration.md   | 低频操作                   |
| P5     | architecture.md    | 给读源码的人               |
