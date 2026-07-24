[English](../en-US/script-mod.md) | ***简体中文***

# 脚本开发

Bark 支持用 JavaScript 或 Lua 写脚本。本文用 JavaScript 作为示例语言，Lua 用户看 [Lua 备注](#lua-备注) 小节即可。

## 创建脚本

在 `ScriptMod/Mods/` 下创建你自己的文件夹，里面放一个 `mod.json` 和入口脚本：

```
ScriptMod/Mods/
  MyMod/
    mod.json
    main.js        ← 入口文件固定为 main.js / main.mjs / main.lua
```

语言由入口文件的扩展名自动判定——放 `main.js` 就是 JS 模组，放 `main.lua` 就是 Lua 模组。

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

| 字段           | 类型   | 必填 | 说明                                                   |
|----------------|--------|------|--------------------------------------------------------|
| `id`           | string | ✅   | 唯一标识，不要和别人重复，推荐蛇形命名                 |
| `name`         | string | ✅   | 显示名称                                               |
| `version`      | string | ✅   | 语义化版本，如 `"1.0.0"`                               |
| `author`       | object | ❌   | 贡献者，key 为角色（程序、美术等），value 为名字       |
| `description`  | string | ❌   | 模组描述                                               |
| `bark_version` | string | ❌   | 要求的 Bark 版本（semver range）                       |
| `game_version` | string | ❌   | 兼容的游戏版本（semver range）                         |
| `dependencies` | array  | ❌   | 依赖的模组，格式 `[{"id": "xxx", "version": "1.0.0"}]` |

### 入口文件

```js
function onLoad() {
    Log.Info("我的模组加载好了！");
}

function onPlayerJumpStart() {
    Log.Info("玩家跳起来了！");
}
```

保存，启动游戏。BepInEx 控制台里看到 `[你的模组名] 我的模组加载好了！` 就成功了。

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

Bark 把工具类注入为全局变量，名字和 C# 类名一致（PascalCase），直接用就行。

| 变量名          | 做什么的                                                                     |
|-----------------|------------------------------------------------------------------------------|
| `BodyUtil`      | 角色基础生理系统（血量、饥饿、体温、意识……）                                 |
| `PlayerUtil`    | 玩家操作（传送、拾取、提示）                                                 |
| `LimbUtil`      | 肢体操作（骨折、脱臼、感染……）                                               |
| `InventoryUtil` | 背包查询                                                                     |
| `ItemUtil`      | 物品搜索、耐久、修理                                                         |
| `SkillUtil`     | 技能经验/等级                                                                |
| `WorldUtil`     | 世界编辑（放方块、放物品）                                                   |
| `OptionsApi`    | 读写模组配置项                                                               |
| `Log`           | 日志输出，`Log.info()` / `Log.warning()` / `Log.error()`                     |
| `Locale`        | 多语言文本，`Locale.Get("key")`                                              |
| `ScriptInfo`    | 当前脚本的元信息：`ScriptInfo.Id` / `ScriptInfo.Name` / `ScriptInfo.Version` |

## 命名规则

所有工具类的方法遵循统一前缀，看懂前缀就知道方法是干嘛的。 **不需要查表，靠 IDE 补全就够了。**

| 前缀                                               | 含义           | 例子                                           |
|----------------------------------------------------|----------------|------------------------------------------------|
| `Get*`                                             | 读取数值       | `BodyUtil.GetHunger()` → 饥饿度                |
| `Set*`                                             | 设置数值       | `BodyUtil.SetHunger(50)`                       |
| `Is*`                                              | 问「是不是」   | `BodyUtil.IsAlive()` → 还活着吗                |
| `Has*`                                             | 问「有没有」   | `InventoryUtil.HasItem("axe")`                 |
| `Can*`                                             | 问「能不能」   | `BodyUtil.CanTakeNap()`                        |
| `Add*`                                             | 增减           | `SkillUtil.AddXP(100)`                         |
| `Remove*`                                          | 移除           | `BodyUtil.RemovePainkillers()`                 |
| `Place*`                                           | 放置           | `WorldUtil.PlaceBlock("marble", 10, 5)`        |
| `Fill*`                                            | 填充区域       | `WorldUtil.FillBlocks(0, 0, 10, 10, "marble")` |
| `Kill` / `Resurrect` / `Break` / `Mend` / `Repair` | 一眼就懂的动词 | `LimbUtil.Break(0)`                            |

**Get/Set 对应**：能 Get 的基本都能 Set，名字一模一样，只是 Set 多一个参数。

```js
var hunger = BodyUtil.GetHunger();   // 读取
BodyUtil.SetHunger(hunger + 10);     // 设置
```

**可选参数**：C# 方法有默认值的参数，脚本里可以不传。

```js
// Alert(text, important, delay) — delay 默认 0
PlayerUtil.Alert("你受伤了");               // 普通提示
PlayerUtil.Alert("警告！", true);           // 重要消息
PlayerUtil.Alert("立即撤离", true, 0.5);    // 全部指定
```

**枚举**：C# 的枚举在脚本里直接传数字。

```js
PlayerUtil.Teleport(100, 200);   // 传送到坐标
```

## 事件钩子

在脚本里定义一个和事件钩子同名的全局函数，事件触发时 Bark 会自动调用它。

完整钩子列表见 [脚本事件钩子](script-events.md)，这里举几个例子：

```js
function onPlayerJumpStart() {
    Log.info("起跳！");
}

function onLimbBroken() {
    Log.info("骨头断了！");
}

function onWorldGenerated() {
    Log.info("世界生成好了，可以开始搞事了");
}
```

> ⚠️ 钩子函数 **不能有参数**。需要获取具体数据时，在函数体内调用相应的工具 API。

## 完整示例

一个实际可用的模组，进游戏后自动回血、受伤时弹提示：

```js
// main.js

function onLoad() {
    Log.info("自动回血模组已加载");
}

function onWorldGenerated() {
    // 每 5 秒把血加满
    setInterval(function () {
        var hp = BodyUtil.GetBloodVolume();
        if (hp < 100) {
            BodyUtil.SetBloodVolume(hp + 5);
        }
    }, 5000);
}

function onLimbBroken() {
    PlayerUtil.Alert("骨头断了！赶紧处理", true);
}
```

更多示例见 [Example 目录](../../Example/)。

## Lua 备注

Lua 用户只需注意以下差异，其他一切同上。

**入口文件固定 `main.lua`**，放到模组文件夹里就行，不需要在 mod.json 里声明语言。

**方法调用用 `:` 而不是 `.`**：

```lua
-- JS: BodyUtil.GetHunger()
-- Lua: 用冒号
local hunger = BodyUtil:GetHunger()
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
