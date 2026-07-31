namespace Bark.Items;

// 物品脚本执行上下文：在 ExecuteItemFile 时暂存当前物品引用和动作名，
// 供 PuerTS JS/Lua 侧通过 CS.Bark.Items.ItemScriptContext.CurrentItem 等方式访问。
// ExecuteItemFile 在 eval 前设置，main() 调用后清除。
public static class ItemScriptContext
{
    // 当前执行的物品实例（可为 null）
    public static Item? CurrentItem { get; internal set; }

    // 当前触发的动作名：use / use_in_hand / equip / unequip / use_on_limb / attack
    public static string? CurrentAction { get; internal set; }

    // ---- 条件触发器传递值（durability / capacity / charge） ----

    // 当前触发值（0.0~1.0 百分比）
    public static float? CurrentTriggerValue { get; internal set; }

    // 触发器设置的阈值（0.0~1.0）
    public static float? CurrentTriggerThreshold { get; internal set; }

    // 触发运算符（"<", "<=", "==", ">=", ">"）
    public static string? CurrentTriggerOperator { get; internal set; }

    // 清除所有条件触发器上下文（在非触发器场景调用以保证干净状态）
    internal static void ClearTriggerContext()
    {
        CurrentTriggerValue = null;
        CurrentTriggerThreshold = null;
        CurrentTriggerOperator = null;
    }
}