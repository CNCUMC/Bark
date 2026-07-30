using Bark.Event;

namespace Bark.Script;

// 事件脚本执行上下文：在 CallTriggerEvent 时暂存当前事件对象，
// 供 PuerTS JS/Lua 侧通过 CS.Bark.Script.EventScriptContext.CurrentEvent 访问。
public static class EventScriptContext
{
    // 当前触发的事件实例（可为 null）
    public static BarkEvent? CurrentEvent { get; internal set; }
}