namespace Bark.Items;

// 物品脚本执行上下文：在 ExecuteFile 时暂存当前物品引用和动作名，
// 供 PuerTS JS/Lua 侧通过 CS.Bark.Items.ItemScriptContext.CurrentItem 等方式访问。
// ExecuteFile 在 eval 前设置，main() 调用后清除。
public static class ItemScriptContext
{
    // 当前执行的物品实例（可为 null）
    public static Item? CurrentItem { get; internal set; }

    // 当前触发的动作名：use / use_in_hand / equip / unequip / use_on_limb / attack
    public static string? CurrentAction { get; internal set; }
}
