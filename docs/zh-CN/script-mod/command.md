[English](../../en-US/script-mod/command.md) | ***简体中文***

# 脚本命令

通过 JSON 在 `Command/` 目录下注册控制台命令，输入命令后 Bark 通过 `onCommand` 事件分发到脚本侧。命令使用游戏自带的 `ConsoleCommandRegistry` 注册，支持参数描述和自动完成。

## 目录结构

```
ScriptMod/Mods/
  MyMod/
    mod.json
    main.js
    Command/
      greet.json             ← 自定义命令
      heal.json
```

## JSON 格式

命令名由文件名决定，无需在 JSON 中声明。`greet.json` → 命令名 `greet`。

```json
{
  "description": "向指定玩家打招呼",
  "args": [
    {
      "name": "player",
      "description": "目标玩家名称",
      "suggestions": ["Alice", "Bob", "Charlie"]
    },
    {
      "name": "times",
      "description": "打招呼次数",
      "suggestions": ["1", "3", "5"]
    }
  ]
}
```

### 字段说明

| 字段          | 类型                | 必填 | 说明                                         |
|---------------|---------------------|------|----------------------------------------------|
| `description` | string              | ❌   | 帮助文本，控制台中按 Tab 或输入 help 时显示  |
| `args`        | ArgDef[]            | ❌   | 参数定义列表，按顺序排列                     |

### ArgDef 字段

| 字段          | 类型     | 必填 | 说明                                           |
|---------------|----------|------|------------------------------------------------|
| `name`        | string   | ✅   | 参数名称（简短描述），如 `"player"`            |
| `description` | string   | ❌   | 参数详细说明                                   |
| `suggestions` | string[] | ❌   | 自动完成候选值列表，输入时按 Tab 补全          |

### 命名规则

- 命令名 = 文件名（不含 `.json`），如 `greet.json` → 命令 `greet`
- 文件名 **不能包含空格**（请用下划线替代）
- 命令名不区分大小写（由游戏控制台处理）
- 同文件名覆盖：两个模组定义同名 JSON 文件时，后加载的覆盖先加载的

## 在脚本中接收命令

定义全局函数 `onCommand(event)`，当玩家输入命令时 Bark 自动调用：

```js
function onCommand(event) {
    // event.CommandName — 触发的命令名称
    // event.Args       — 用户输入的参数列表（args[0] 为命令名）
    Log.Info('收到命令: ' + event.CommandName);
    Log.Info('参数: ' + event.Args.join(', '));
}
```

**event 字段**：

| 字段                 | 类型       | 说明                                         |
|----------------------|------------|----------------------------------------------|
| `event.CommandName`  | `string`   | 触发的命令名称（不含参数）                   |
| `event.Args`         | `string[]` | 完整输入列表（`args[0]` 为命令名，`args[1..]` 为用户参数） |

## 完整示例

一个打招呼命令，参数补全玩家列表：

**`Command/greet.json`**：

```json
{
  "description": "向指定玩家打招呼",
  "args": [
    {
      "name": "player",
      "description": "目标玩家",
      "suggestions": ["Alice", "Bob", "Charlie"]
    },
    {
      "name": "times",
      "description": "重复次数",
      "suggestions": ["1", "2", "3"]
    }
  ]
}
```

**`main.js`**：

```js
function onCommand(event) {
    if (event.CommandName === 'greet') {
        // args[1] = player, args[2] = times
        var player = event.Args.length > 1 ? event.Args[1] : 'World';
        var times = event.Args.length > 2 ? parseInt(event.Args[2]) : 1;

        for (var i = 0; i < times; i++) {
            Log.Info('Hello, ' + player + '!');
        }

        PlayerUtil.Alert('Hello, ' + player + '! (x' + times + ')', true);
    }
}
```

游戏内输入 `greet Alice 3`，BepInEx 控制台输出：

```text
Hello, Alice!
Hello, Alice!
Hello, Alice!
```

## 使用技巧

### 区分不同命令

所有脚本命令都走 `onCommand`，通过 `event.CommandName` 区分：

```js
function onCommand(event) {
    switch (event.CommandName) {
        case 'heal':
            BodyUtil.HealAll();
            PlayerUtil.Alert('已恢复全部生命值');
            break;
        case 'feed':
            BodyUtil.Feed(50);
            break;
        case 'tp':
            if (event.Args.length >= 3) {
                var x = parseFloat(event.Args[1]);
                var y = parseFloat(event.Args[2]);
                PlayerUtil.Teleport(x, y);
            }
            break;
    }
}
```

### 无参数命令

省略 `args` 字段即可定义无参数命令：

```json
{
  "description": "立即满血"
}
```

```js
function onCommand(event) {
    if (event.CommandName === 'full_heal') {
        BodyUtil.HealAll();
        PlayerUtil.Alert('满血复活！', true);
    }
}
```

### Lua 写法

```lua
function onCommand(event)
    if event.CommandName == 'greet' then
        local target = event.Args[2] or 'World'
        Log:info('Hello, ' .. target .. '!')
    end
end
```

## 注意事项

- 热重载（`script reload` / `rs`）会重新加载所有命令定义
- 同文件名覆盖：后加载的模组覆盖先加载的
- JSON 字段使用 `snake_case` 命名
- 命令名 = 文件名（不含 `.json`），无需在 JSON 中写 `name` 字段
- `args` 数组中 `args[0]` 固定为命令名本身，用户参数从 `args[1]` 开始
- 命令注册到游戏内置的 `ConsoleCommandRegistry`，与 `script` 系统命令共享同一个控制台
- 参数建议（suggestions）仅在游戏控制台的自动完成中生效，不做强制校验
