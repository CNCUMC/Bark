***English*** | [简体中文](../../zh-CN/script-mod/command.md)

# Script Commands

Register console commands via JSON in the `Command/` directory. When a player enters a command, Bark dispatches it to your script via the `onCommand` event. Commands are registered through the game's built-in `ConsoleCommandRegistry`, with support for argument descriptions and auto-completion.

## Directory Layout

```
ScriptMod/Mods/
  MyMod/
    mod.json
    main.js
    Command/
      greet.json             ← custom command
      heal.json
```

## JSON Format

The command name is derived from the filename — no need to declare it in JSON. `greet.json` → command `greet`.

```json
{
  "description": "Greet a target player",
  "args": [
    {
      "name": "player",
      "description": "Target player name",
      "suggestions": ["Alice", "Bob", "Charlie"]
    },
    {
      "name": "times",
      "description": "Number of greetings",
      "suggestions": ["1", "3", "5"]
    }
  ]
}
```

### Fields

| Field         | Type               | Required | Description                                               |
|---------------|--------------------|----------|-----------------------------------------------------------|
| `description` | string             | No       | Help text, shown on Tab or help command                   |
| `args`        | ArgDef[]           | No       | Argument definitions, in order                            |

### ArgDef Fields

| Field         | Type     | Required | Description                                               |
|---------------|----------|----------|-----------------------------------------------------------|
| `name`        | string   | Yes      | Argument name (short label), e.g. `"player"`              |
| `description` | string   | No       | Detailed argument description                             |
| `suggestions` | string[] | No       | Auto-completion hint list, press Tab to cycle through      |

### Naming Rules

- Command name = filename (without `.json`), e.g. `greet.json` → command `greet`
- Filenames **must not contain spaces** (use underscores instead)
- Command names are case-insensitive (handled by game console)
- Same filename overwrites: later-loaded mod commands override earlier ones

## Receiving Commands in Scripts

Define a global `onCommand(event)` function. Bark calls it whenever a player enters a command:

```js
function onCommand(event) {
    // event.CommandName — the triggered command name
    // event.Args       — all input tokens (args[0] = command name)
    Log.Info('Command: ' + event.CommandName);
    Log.Info('Args: ' + event.Args.join(', '));
}
```

**event fields**:

| Field                | Type       | Description                                                |
|----------------------|------------|------------------------------------------------------------|
| `event.CommandName`  | `string`   | Triggered command name (without arguments)                 |
| `event.Args`         | `string[]` | All input tokens (`args[0]` = command name, `args[1..]` = user arguments) |

## Full Example

A greet command with player name auto-completion:

**`Command/greet.json`**:

```json
{
  "description": "Greet a target player",
  "args": [
    {
      "name": "player",
      "description": "Target player",
      "suggestions": ["Alice", "Bob", "Charlie"]
    },
    {
      "name": "times",
      "description": "Repeat count",
      "suggestions": ["1", "2", "3"]
    }
  ]
}
```

**`main.js`**:

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

In-game, type `greet Alice 3`. BepInEx console output:

```text
Hello, Alice!
Hello, Alice!
Hello, Alice!
```

## Tips

### Distinguishing Commands

All script commands go through `onCommand`. Use `event.CommandName` to route:

```js
function onCommand(event) {
    switch (event.CommandName) {
        case 'heal':
            BodyUtil.HealAll();
            PlayerUtil.Alert('Fully healed!');
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

### Argument-less Commands

Omit the `args` field:

```json
{
  "description": "Instantly restore full health"
}
```

```js
function onCommand(event) {
    if (event.CommandName === 'full_heal') {
        BodyUtil.HealAll();
        PlayerUtil.Alert('Full health restored!', true);
    }
}
```

### Lua

```lua
function onCommand(event)
    if event.CommandName == 'greet' then
        local target = event.Args[2] or 'World'
        Log:info('Hello, ' .. target .. '!')
    end
end
```

## Notes

- `script reload` / `rs` reloads all command definitions
- Same filename overwrites: later-loaded mods override earlier ones
- JSON fields use `snake_case` naming
- Command name = filename (without `.json`), no `name` field needed in JSON
- `args[0]` is always the command name itself. User arguments start at `args[1]`
- Commands register to the game's built-in `ConsoleCommandRegistry`, sharing the console with `script` system commands
- Suggestions are auto-completion hints only — no enforcement
