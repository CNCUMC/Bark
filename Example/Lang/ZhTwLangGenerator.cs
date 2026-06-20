using Bark.Core;

namespace Bark.Example.Lang;

public class ZhTwLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-TW";

    protected override void BuildLocaleData()
    {
        // Command
        Add("command.testhello.description", "測試Hello");
        Add("command.testhello.text", "你好世界 by Bark! {0}!");

        Add("command.spawnblock.description", "在滑鼠位置生成一個方塊。");
        Add("command.spawnblock.blockid", "要生成的方塊 ID。");
        Add("command.spawnblock.invalidblockid", "'{0}' 不是有效的方塊 ID。");
        Add("command.spawnblock.success", "已生成方塊 {0}。");

        Add("command.listbackground.description", "列出所有可用的背景 ID。");
        Add("command.listbackground.header", "找到 {0} 個背景資源：");
        Add("command.listbackground.none", "未找到背景資源。");

        Add("command.spawnbackground.description", "在滑鼠位置生成一個背景。");
        Add("command.spawnbackground.backgroundid", "要生成的背景 ID。");
        Add("command.spawnbackground.invalidbackgroundid", "背景 ID 不能為空。");
        Add("command.spawnbackground.success", "已生成背景 {0}。");

        // Tool - Console
        Add("tool.console.nullorempty", "命令不能為空或空值");
        Add("tool.console.notinitialized", "ConsoleScript 未初始化");

        // Tool - World
        Add("tool.world.checkforworld", "沒有加載任何世界。要不試試開始遊戲?");
        Add("tool.world.placeblock", "在 {0} 生成方塊 {1} 失敗:{2}");
        Add("tool.world.placeitem", "在 {0} 生成物品 {1} 失敗:{2}");
        Add("tool.world.placeitem.nullorempty", "物品不能為空或空值");
        Add("tool.world.trygetsprite", "未找到背景精靈: {0}");

        // Tool - Player
        Add("tool.player.bodynull", "玩家身體物件為空");
        Add("tool.player.item.nullorempty", "物品標識符不能為空或空白");
        Add("tool.player.slot.outofrange", "物品欄索引超出範圍。最大槽位數: {0}");
        Add("tool.player.loaditem.fail", "加載或實例化物品資源失敗: '{0}'");
        Add("tool.player.loaditem.missingcomponent", "資源 '{0}' 已加載但缺少所需的 Item 組件");

        // Tool - Multiplayer
        Add("tool.multiplayer.playername.nullorempty", "玩家名稱不能為空或空值");
        Add("tool.multiplayer.teleport.success", "已傳送: {0} 到 {1}");
        Add("tool.multiplayer.teleport.fail", "傳送失敗: {0}");

        // Tool - Config
        Add("tool.config.getconfig.notexistconfig", "不存在 {0} 這個配置");
        Add("tool.config.getconfig.notexistkey", "{0} 不存在 {1} 這個鍵");
        Add("tool.config.switchtype", "已將 {0} 設置為 {1}!");

        // Tool - Utils
        Add("tool.utils.checkargumentcount", "預期至少 {0} 個參數 {1},但得到了 {2} 個");
        Add("tool.utils.parse.float.invalid", "'{0}' 不是有效的浮點數值!(2, 0.7, 14.1 等)");
        Add("tool.utils.parse.int.invalid", "'{0}' 不是有效的整數值!");
        Add("tool.utils.string.nullorempty", "輸入字串不能為空或空值");

        // Tool - Inventory
        Add("tool.inventory.bodynull", "玩家身體物件為空");
        Add("tool.inventory.id.nullorempty", "物品 ID 不能為空或空白");
        Add("tool.inventory.summary.header", "[物品欄]");
        Add("tool.inventory.summary.handslot", "{0}* (手持)");
        Add("tool.inventory.summary.slot", "{0}");
        Add("tool.inventory.summary.empty", "空");
        Add("tool.inventory.summary.wearables", "[穿戴]");
        Add("tool.inventory.empty", "(空)");
    }
}
