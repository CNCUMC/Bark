using System.Collections.Generic;
using System.Linq;
using Bark.Event;
using Bark.Events;
using Bark.Tool;

namespace Bark.Items;

// 物品脚本运行器：监听物品事件，通过 ScriptUtil 触发对应的脚本文件。
// 在 Plugin.Awake() 中调用 Listen() 注册事件处理器。
public static class ItemScriptRunner
{
    // 注册事件处理器（应在所有模组加载完成后调用）
    public static void Listen()
    {
        EventUtil.On<ItemUseEvent>(OnItemUse, Plugin.Guid);
        EventUtil.On<ItemHandUseEvent>(OnItemHandUse, Plugin.Guid);
        EventUtil.On<ItemEquipEvent>(OnItemEquip, Plugin.Guid);
        EventUtil.On<ItemUnequipEvent>(OnItemUnequip, Plugin.Guid);
        EventUtil.On<ItemLimbUseEvent>(OnItemLimbUse, Plugin.Guid);
        EventUtil.On<ItemAttackEvent>(OnItemAttack, Plugin.Guid);
    }

    // 停止监听（卸载时调用）
    public static void Stop()
    {
        EventUtil.UnregisterAll(Plugin.Guid);
    }

    private static void OnItemUse(ItemUseEvent evt)
    {
        ExecuteScripts(evt.ItemId, evt.Item, "use", e => e.Use);
    }

    private static void OnItemHandUse(ItemHandUseEvent evt)
    {
        ExecuteScripts(evt.ItemId, evt.Item, "use_in_hand", e => e.UseInHand);
    }

    private static void OnItemEquip(ItemEquipEvent evt)
    {
        ExecuteScripts(evt.ItemId, evt.Item, "equip", e => e.Equip);
    }

    private static void OnItemUnequip(ItemUnequipEvent evt)
    {
        ExecuteScripts(evt.ItemId, evt.Item, "unequip", e => e.Unequip);
    }

    private static void OnItemLimbUse(ItemLimbUseEvent evt)
    {
        ExecuteScripts(evt.ItemId, evt.Item, "use_on_limb", e => e.UseOnLimb);
    }

    private static void OnItemAttack(ItemAttackEvent evt)
    {
        ExecuteScripts(evt.ItemId, evt.Item, "attack", e => e.Attack);
    }

    // 从 ItemScriptRegistry 查找物品脚本，通过 ScriptUtil 按顺序执行。
    // item: 当前物品实例（可为 null）；action: 触发动作名，传入脚本的 main(itemId, item, action)
    private static void ExecuteScripts(string itemId, Item? item, string action,
        System.Func<ScriptEntry, List<string>> getScriptList)
    {
        if (string.IsNullOrEmpty(itemId))
            return;

        var entry = ItemScriptRegistry.GetEntry(itemId);
        if (entry is null)
            return;

        var scripts = getScriptList(entry);
        if (scripts.Count == 0)
            return;

        foreach (var relativePath in scripts.Where(relativePath => !string.IsNullOrEmpty(relativePath)))
        {
            ScriptUtil.Execute(entry.ModId, relativePath, itemId, item, action);
        }
    }
}
