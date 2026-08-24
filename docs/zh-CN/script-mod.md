[English](../en-US/script-mod.md) | ***简体中文***

# 脚本开发

Bark 支持用 JavaScript 或 Lua 写脚本。本文用 JavaScript 作为示例语言，Lua 用户看 [Lua 备注](#lua-备注)。

## 创建脚本

在 `ScriptMod/Mods/` 下创建你自己的文件夹，里面放一个 `mod.json`。入口脚本是 **可选的**：

```
ScriptMod/Mods/
  MyMod/
    mod.json
    main.js        ← 入口文件可选：main.js / main.mjs / main.lua
```

语言由入口文件的扩展名自动判定——放 `main.js` 就是 JS 脚本模组，放 `main.lua` 就是 Lua 脚本模组。

### 纯数据模组

没有入口脚本（`main.js` / `main.mjs` / `main.lua`）的模组会被当作 **纯数据模组**处理。 它依然会被加载，并可以通过 `Assets/`
文件夹提供 JSON 内容（物品、物块、配方、情绪、命令）， 但不会启动任何脚本引擎，也不会执行生命周期钩子。

```
ScriptMod/Mods/
  MyDataMod/
    mod.json
    Assets/
      Item/
        my_item.json
      Recipe/
        my_recipe.json
```

两者也可以同时存在：带入口脚本的模组同样可以定义 JSON 内容。

### mod.json

```json
{
  "id": "my_mod",
  "name": "我的模组",
  "version": "1.0.0",
  "author": {
    "程序": "你的名字"
  },
  "description": "这是模组描述",
  "bark_version": ">=0.1.0",
  "game_version": ">=0.9.0",
  "dependencies": []
}
```

| 字段           | 类型   | 必填 | 说明                                                            |
|----------------|--------|------|-----------------------------------------------------------------|
| `id`           | string | ✅   | 唯一标识，不要和别人重复，推荐蛇形命名                          |
| `name`         | string | ✅   | 显示名称                                                        |
| `version`      | string | ✅   | 语义化版本，如 `"1.0.0"`                                        |
| `author`       | object | ❌   | 贡献者，key 为角色（程序、美术等），value 为名字                |
| `description`  | string | ❌   | 模组描述                                                        |
| `bark_version` | string | ❌   | 要求的 Bark 版本（semver range）                                |
| `game_version` | string | ❌   | 兼容的游戏版本（semver range）                                  |
| `dependencies` | array  | ❌   | 依赖的脚本模组，格式 `[{"id": "some_mod", "version": "1.0.0"}]` |

### 入口文件

```js
function onLoad() {
    Log.Info("我的模组加载好了！");
}

function onPlayerJumpStart() {
    Log.Info("玩家跳起来了！");
}
```

保存，启动游戏。BepInEx 控制台里看到 `[你的脚本模组名] 我的模组加载好了！` 就成功了。

## 生命周期钩子

Bark 在特定时机调用你脚本里的这些函数。 **都不是必写的**，按需定义即可。

| 钩子          | 触发时机     | 注意                                              |
|---------------|--------------|---------------------------------------------------|
| `onLoad()`    | 脚本加载完成 | 主菜单阶段就调用，**不要**在这里访问 World/Player |
| `onEnable()`  | 脚本激活     | —                                                 |
| `onDisable()` | 脚本停用     | —                                                 |
| `onUnload()`  | 脚本卸载     | —                                                 |
| `onUpdate()`  | 每帧         | ***别在这里干重活***                              |

```js
function onLoad() {
    Log.info("加载完毕");
}

function onEnable() {
    // 玩家进游戏时触发，这里可以安全访问 Player 了
}
```

## 全局变量

Bark 把工具类注入为全局变量，名字为 C# 类名去掉 `Util` 后缀（PascalCase），直接用就行。

| 变量名       | 做什么的                                                                     | API 文档                              |
|--------------|------------------------------------------------------------------------------|---------------------------------------|
| `Body`       | 角色基础生理系统（血量、饥饿、体温、意识……）                                 | [身体](script-api/body.md)            |
| `Player`     | 玩家操作（传送、拾取、提示）                                                 | [玩家](script-api/player.md)          |
| `Limb`       | 肢体操作（骨折、脱臼、感染……）                                               | [肢体](script-api/limbs.md)           |
| `Inventory`  | 背包查询                                                                     | [背包与物品](script-api/inventory.md) |
| `Item`       | 物品搜索、耐久、修补                                                         | [背包与物品](script-api/inventory.md) |
| `Moodle`     | 状态效果应用、移除、查询                                                     | [自定义状态](script-api/moodle.md)    |
| `Skill`      | 技能经验/等级                                                                | [技能](script-api/skills.md)          |
| `World`      | 世界编辑（放方块、放物品）                                                   | [世界编辑](script-api/world.md)       |
| `OptionsApi` | 读写脚本模组配置项                                                           | [配置项](script-api/options.md)       |
| `Log`        | 日志输出，`Log.info()` / `Log.warning()` / `Log.error()`                     | [日志](script-api/log.md)             |
| `Locale`     | 多语言文本，`Locale.Get("key")`                                              | [多语言](script-api/locale.md)        |
| `Input`      | 鼠标位置、按键友好名称                                                       | [输入](script-api/input.md)           |
| `Storage`    | 基于 PlayerPrefs 的持久化数据                                                | [存储](script-api/storage.md)         |
| `Compress`   | GZip / Deflate 数据压缩                                                      | [压缩](script-api/compress.md)        |
| `ScriptInfo` | 当前脚本的元信息：`ScriptInfo.Id` / `ScriptInfo.Name` / `ScriptInfo.Version` | —                                     |

## 命名规则

所有工具类的方法遵循统一前缀，看懂前缀就知道方法是干嘛的。 **不需要查表，靠 IDE 补全就够了。**

| 前缀                                               | 含义           | 例子                                       |
|----------------------------------------------------|----------------|--------------------------------------------|
| `Get*`                                             | 读取数值       | `Body.GetHunger()` → 饥饿度                |
| `Set*`                                             | 设置数值       | `Body.SetHunger(50)`                       |
| `Is*`                                              | 问「是不是」   | `Body.IsAlive()` → 还活着吗                |
| `Has*`                                             | 问「有没有」   | `Inventory.HasItem("axe")`                 |
| `Can*`                                             | 问「能不能」   | `Body.CanTakeNap()`                        |
| `Add*`                                             | 增减           | `Skill.AddXP(100)`                         |
| `Remove*`                                          | 移除           | `Body.RemovePainkillers()`                 |
| `Place*`                                           | 放置           | `World.PlaceTile("marble", 10, 5)`        |
| `Fill*`                                            | 填充区域       | `World.FillTiles(0, 0, 10, 10, "marble")` |
| `Kill` / `Resurrect` / `Break` / `Mend` / `Repair` | 一眼就懂的动词 | `Limb.Break(0)`                            |

**Get/Set 对应**：能 Get 的基本都能 Set，名字一模一样，只是 Set 多一个参数。

```js
var hunger = Body.GetHunger();   // 读取
Body.SetHunger(hunger + 10);     // 设置
```

**可选参数**：C# 方法有默认值的参数，脚本里可以不传。

```js
// Alert(text, important, delay) — delay 默认 0
Player.Alert("你受伤了");               // 普通提示
Player.Alert("警告！", true);           // 重要消息
Player.Alert("立即撤离", true, 0.5);    // 全部指定
```

**枚举**：C# 的枚举在脚本里直接传数字。

```js
Player.Teleport(100, 200);   // 传送到坐标
```

## 事件钩子

在脚本里定义一个和事件钩子同名的全局函数，事件触发时 Bark 会自动调用它。函数接收一个 `event` 对象，携带事件数据 （如物品事件的
`event.ItemId` 和 `event.Item`）。不需要时可以省略参数。

完整钩子列表见 [脚本事件钩子](script-events.md)，这里举几个例子：

```js
function onPlayerJumpStart(event) {
    Log.info("起跳！");
}

function onLimbBroken(event) {
    Log.info("骨头断了！");
}

function onItemUse(event) {
    Log.info("使用了物品: " + event.ItemId);
    // event.Item 是 C# Item 实例
}

function onWorldGenerated(event) {
    Log.info("世界生成好了，可以开始搞事了");
}
```

## 物品脚本

你可以为自定义物品绑定脚本，在特定动作触发时执行。脚本中定义 `main` 函数，接收 `(itemId, item, action)` 三个参数：

```js
// arrow.js — 在 arrow.json 的 "attack" 下注册
function main(itemId, item, action) {
    // itemId: 物品 ID 字符串
    // item:    C# Item 实例（可访问 .condition 等属性）
    // action:  "attack" | "use" | "equip" | "unequip" | "use_in_hand" | "use_on_limb"
    Item.Destroy(itemId);
    Player.Alert("箭无虚发！", true);
}
```

`main` 接受 0 到 3 个参数——JavaScript 和 Lua 自动忽略多余参数：

```js
function main(itemId)               { /* 只需 ID */ }
function main(itemId, item, action) { /* 完整上下文 */ }
```

JSON 配置格式详见 [自定义物品](script-mod/item.md)。

## 自定义物块

通过 JSON 在 `Tile/` 目录下定义自定义物块（地面/墙壁方块），精灵图放在 `Assets/Tile/` 下。Bark 自动扫描并注册到
`TileRegistry`。

详见 [自定义物块](script-mod/tile.md)。

## 自定义 Moodle

通过 JSON 在 `Moodle/` 目录下定义自定义状态效果（流血、中毒、感染等），用 `Moodle`
在脚本中操作。支持三个生命周期阶段：get（获得）、iterate（轮询）、lose（消失）。

详见 [自定义 Moodle](script-mod/moodle.md)。

## 自定义物品模板

模板是一组预设的物品属性。枪械、弹匣、弹药、弹壳等常见类型已有内置模板，通过 `"template"` 字段引用即可大幅简化物品 JSON。你还可以在
`item-template/` 目录下注册自己的模板。

详见 [物品模板](script-mod/item-template/index.md)。

## 自定义音效

自定义音效文件放在模组的 `Assets/Audio/` 下，支持 `.wav` `.mp3` `.aif` 等格式。枪械模板的 `fire_sound` / `rack_sound` /
`unrack_sound` 写相对路径（纯文件名自动补全 `Assets/Audio/` 前缀），`AudioManager` 自动加载并缓存。

详见 [自定义音效](script-mod/audio.md)。

## 完整示例

一个实际可用的脚本模组，进游戏后自动回血、受伤时弹提示：

```js
// main.js

function onLoad() {
    Log.info("自动回血模组已加载");
}

function onWorldGenerated() {
    // 每 5 秒把血加满
    setInterval(function () {
        var hp = Body.GetBloodVolume();
        if (hp < 100) {
            Body.SetBloodVolume(hp + 5);
        }
    }, 5000);
}

function onLimbBroken() {
    Player.Alert("骨头断了！赶紧处理", true);
}
```

## 控制台指令

Bark 注册了几个游戏内控制台指令，开发调试时很有用。

| 指令            | 别名   | 作用                             |
|-----------------|--------|----------------------------------|
| `script help`   | —      | 显示指令帮助                     |
| `script reload` | `sr`   | 重载所有脚本模组                 |
| `script list`   | —      | 列出已加载的脚本模组             |
| `script spawn`  | `basp` | 按注册 ID 生成 Bark 物品         |
| `script tile`   | `bast` | 按注册 ID 放置 Bark 物块         |
| `script moodle` | `basm` | 按 key 应用 Bark 情绪            |
| `script detail` | `scd`  | 查看某模组注册的元数据与内容统计 |

用法：在游戏内按 `` ` `` 打开控制台，输入指令回车。

```text
> script list
脚本模组列表 (3):
  Hello World-JavaScript v1.0.0 [JavaScript] (hello_world_js)
  My Mod v1.0.0 [Lua] (my_mod)

> sr
所有脚本模组已重新加载
```

> 💡 `script reload` 是最常用的指令。改了脚本不需要重启游戏，输一下 `sr` 就能看到效果。

### 生成 / 放置 Bark 内容

这些指令用于生成由 Bark 注册的内容（来自任意脚本模组或 C# 模组的物品、物块、情绪）。 它们 **只接受 Bark 注册的 ID**——原版 CCL
物品/物块仍用游戏自带的 `cuspawn` / `settile` 生成。

内容 ID 是完整的注册名，格式为 `modid.entryname`（例如 `hello_world_js.ak47`）。

输入时按 `Tab` 会自动补全，且 **按子命令类型切换候选**：`script spawn` 只补物品、`script tile` 只补物块、
`script moodle` 只补状态 key、`script detail` / `scd` 只补模组 ID（不会把所有 ID 混在一起）。 补全列表在 `script reload`
后自动刷新。

**生成物品** —— 转发给 CCL 的 `cuspawn`，参数按顺序：`[id] [位置] [状态] [数量]`。

```text
> basp hello_world_js.ak47
> script spawn hello_world_js.improved_headlamp 100 1
```

**放置物块** —— 先把 Bark 字符串物块 ID 转换成 CCL 的物块索引，再转发给 CCL 的 `settile`
（`[tileIndex] [位置]`）。

```text
> bast hello_world_js.marble
> script tile hello_world_js.marble 12,34
```

**应用情绪** —— 给玩家应用一个已注册的情绪。CCL 没有对应指令，因此 Bark 直接调用
`MoodleUtil.ApplyMoodle`。参数：`[moodleKey] [holdSeconds]`（`holdSeconds` 可选，缺省时用 JSON 定义的持续时间）。

```text
> basm hello_world_js.bleeding
> script moodle hello_world_js.bleeding 30
```

**查看模组注册内容** —— 用 `script detail <id>`（或别名 `scd <id>`）查询某个已加载模组在 `mod.json`
里声明的元数据，以及它实际注册了多少物品 / 物块 / 配方 / 状态。输出包含：作者、描述、所需的 Bark 版本、
游戏版本、仓库地址、依赖列表，以及各类内容的统计数量。

```text
> scd hello_world_js
hello_world_js v1.0.0 [JavaScript] 注册的内容:
  作者: author: Jimmy
  描述: A hello-world demo mod
  需要 Bark: >=2.3.0
  游戏版本: >=1.0.0
  仓库: https://github.com/user/hello_world
  依赖:
    - bark_utils (>=1.0.0)
  物品: 3
  物块: 2
  配方: 1
  状态: 0
```

### 注册自定义命令

通过 `Command/` 目录下的 JSON 注册你自己的控制台命令，输入后触发脚本侧的 `onCommand` 事件。

详见 [脚本命令](script-mod/command.md)。

## 分发包（zip）

如果你想发布模组给玩家使用，可以把整个模组文件夹打包成 `.zip`，放到 `ScriptMod/Mods/` 下：

```
ScriptMod/Mods/
  MyMod.zip          ← 打包好的模组
```

Bark 启动时会自动把 `.zip` 解压到 BepInEx 缓存目录（`Paths.CachePath`），无需用户手动解压。

| 行为         | 说明                                                                                   |
|--------------|----------------------------------------------------------------------------------------|
| 首次加载     | 解压一次，之后跳过（除非 zip 被替换）                                                  |
| 同名目录优先 | 如果 `Mods/MyMod/` 和 `MyMod.zip` 同时存在且 id 相同，**目录版优先**（方便开发者调试） |
| 删除 zip     | 下次启动自动清理对应缓存，不留垃圾                                                     |
| 热重载       | zip 模组不支持——zip 是成品分发包，需要修改时解压到目录即可                             |

**开发工作流**：开发时用目录形式，发布时打包成 `.zip` 分发。两者共存时目录版覆盖 zip 版，修改即时生效。

> 💡 zip 只用于分发，不用于开发。调试时用目录版，热重载、改文件都方便。

## Lua 备注

Lua 用户只需注意以下差异，其他一切同上。

**入口文件固定 `main.lua`**，放到脚本模组文件夹里就行。

**方法调用用 `:` 而不是 `.`**：

```lua
-- JS: Body.GetHunger()
-- Lua: 用冒号
local hunger = Body:GetHunger()
```

**函数定义**：

```lua
function onLoad()
    Log:info("加载完毕")
end

function onPlayerJumpStart()
    Log:info("起跳！")
end
```

## Python 备注

PuerTS 支持 Python，但 Python 运行时包体较大（约 22MB），当前暂缓处理。如有需求会在大版本中更新。 相关实现已归档至源码仓库的
`TodoPython/` 目录。
