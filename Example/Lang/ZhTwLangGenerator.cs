using Bark.Core;

namespace Bark.Example.Lang;

public class ZhTwLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-TW";

    protected override void BuildLocaleData()
    {
        // Command
        Add("command_testhello_description", "測試Hello");
        Add("command_testhello_text", "你好世界 by Bark! {0}!");

        Add("command_spawnblock_description", "在滑鼠位置生成一個方塊。");
        Add("command_spawnblock_blockid", "要生成的方塊 ID。");
        Add("command_spawnblock_invalidblockid", "'{0}' 不是有效的方塊 ID。");
        Add("command_spawnblock_success", "已生成方塊 {0}。");

        Add("command_listbackground_description", "列出所有可用的背景 ID。");
        Add("command_listbackground_header", "找到 {0} 個背景資源：");
        Add("command_listbackground_none", "未找到背景資源。");

        Add("command_spawnbackground_description", "在滑鼠位置生成一個背景。");
        Add("command_spawnbackground_backgroundid", "要生成的背景 ID。");
        Add("command_spawnbackground_invalidbackgroundid", "背景 ID 不能為空。");
        Add("command_spawnbackground_success", "已生成背景 {0}。");

        // Tool - Console
        Add("tool_console_nullorempty", "命令不能為空或空值");
        Add("tool_console_notinitialized", "ConsoleScript 未初始化");

        // Tool - World
        Add("tool_world_checkforworld", "沒有加載任何世界。要不試試開始遊戲?");
        Add("tool_world_placeblock", "在 {0} 生成方塊 {1} 失敗:{2}");
        Add("tool_world_placeitem", "在 {0} 生成物品 {1} 失敗:{2}");
        Add("tool_world_placeitem_nullorempty", "物品不能為空或空值");
        Add("tool_world_trygetsprite", "未找到背景精靈: {0}");

        // Tool - Player
        Add("tool_player_bodynull", "玩家身體物件為空");
        Add("tool_player_item_nullorempty", "物品標識符不能為空或空白");
        Add("tool_player_slot_outofrange", "物品欄索引超出範圍。最大槽位數: {0}");
        Add("tool_player_loaditem_fail", "加載或實例化物品資源失敗: '{0}'");
        Add("tool_player_loaditem_missingcomponent", "資源 '{0}' 已加載但缺少所需的 Item 組件");

        // Tool - Multiplayer
        Add("tool_multiplayer_playername_nullorempty", "玩家名稱不能為空或空值");
        Add("tool_multiplayer_teleport_success", "已傳送: {0} 到 {1}");
        Add("tool_multiplayer_teleport_fail", "傳送失敗: {0}");

        // Tool - Config
        Add("tool_config_getconfig_notexistconfig", "不存在 {0} 這個配置");
        Add("tool_config_getconfig_notexistkey", "{0} 不存在 {1} 這個鍵");
        Add("tool_config_switchtype", "已將 {0} 設置為 {1}!");

        // Tool - Utils
        Add("tool_utils_checkargumentcount", "預期至少 {0} 個參數 {1},但得到了 {2} 個");
        Add("tool_utils_parse_float_invalid", "'{0}' 不是有效的浮點數值!(2, 0.7, 14.1 等)");
        Add("tool_utils_parse_int_invalid", "'{0}' 不是有效的整數值!");
        Add("tool_utils_string_nullorempty", "輸入字串不能為空或空值");

        // Tool - Inventory
        Add("tool_inventory_bodynull", "玩家身體物件為空");
        Add("tool_inventory_id_nullorempty", "物品 ID 不能為空或空白");
        Add("tool_inventory_summary_header", "[物品欄]");
        Add("tool_inventory_summary_handslot", "{0}* (手持)");
        Add("tool_inventory_summary_slot", "{0}");
        Add("tool_inventory_summary_empty", "空");
        Add("tool_inventory_summary_wearables", "[穿戴]");
        Add("tool_inventory_empty", "(空)");
    }
}
