using System.Collections.Generic;
using Bark.Event;

namespace Bark.Events;

// 控制台命令事件：玩家在控制台输入脚本模组注册的命令时触发
// 脚本侧定义 function onCommand(event) 接收，通过 event.CommandName / event.Args 处理
[ScriptEvent("onCommand")]
public class CommandEvent : BarkEvent
{
    // 触发的命令名称（不含参数）
    public string CommandName { get; set; } = string.Empty;

    // 用户输入的参数列表（args[0] 为命令名，args[1..] 为用户参数）
    public List<string> Args { get; set; } = [];
}

// 主菜单加载完成事件：主菜单场景加载完成后触发
[ScriptEvent("onMainMenuLoaded")]
public class MainMenuLoadedEvent : BarkEvent;

// 世界就绪事件：世界生成完成、玩家进入世界时触发
[ScriptEvent("onWorldGenerated")]
public class WorldReadyEvent : BarkEvent
{
    public WorldGeneration World { get; set; } = WorldGeneration.world;
}
