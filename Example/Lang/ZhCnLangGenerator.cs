using Bark.Core;

namespace Bark.Example.Lang;

public class ZhCnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-CN";

    protected override void BuildLocaleData()
    {
        // Command
        Add("command_testhello_description", "测试Hello");
        Add("command_testhello_text", "你好世界 by Bark! {0}!");

        Add("command_spawnblock_description", "在鼠标位置生成一个方块。");
        Add("command_spawnblock_blockid", "要生成的方块 ID。");
        Add("command_spawnblock_invalidblockid", "'{0}' 不是有效的方块 ID。");
        Add("command_spawnblock_success", "已生成方块 {0}。");

        Add("command_listbackground_description", "列出所有可用的背景 ID。");
        Add("command_listbackground_header", "找到 {0} 个背景资源：");
        Add("command_listbackground_none", "未找到背景资源。");

        Add("command_spawnbackground_description", "在鼠标位置生成一个背景。");
        Add("command_spawnbackground_backgroundid", "要生成的背景 ID。");
        Add("command_spawnbackground_invalidbackgroundid", "背景 ID 不能为空。");
        Add("command_spawnbackground_success", "已生成背景 {0}。");

        // Tool - Console
        Add("tool_console_nullorempty", "命令不能为空或空值");
        Add("tool_console_notinitialized", "ConsoleScript 未初始化");

        // Tool - World
        Add("tool_world_checkforworld", "没有加载任何世界。要不试试开始游戏?");
        Add("tool_world_placeblock", "在 {0} 生成方块 {1} 失败:{2}");
        Add("tool_world_placeitem", "在 {0} 生成物品 {1} 失败:{2}");
        Add("tool_world_placeitem_nullorempty", "物品不能为空或空值");
        Add("tool_world_trygetsprite", "未找到背景精灵: {0}");

        // Tool - Player
        Add("tool_player_bodynull", "玩家身体对象为空");
        Add("tool_player_item_nullorempty", "物品标识符不能为空或空白");
        Add("tool_player_slot_outofrange", "物品栏索引超出范围。最大槽位数: {0}");
        Add("tool_player_loaditem_fail", "加载或实例化物品资源失败: '{0}'");
        Add("tool_player_loaditem_missingcomponent", "资源 '{0}' 已加载但缺少所需的 Item 组件");

        // Tool - Multiplayer
        Add("tool_multiplayer_playername_nullorempty", "玩家名称不能为空或空值");
        Add("tool_multiplayer_teleport_success", "已传送: {0} 到 {1}");
        Add("tool_multiplayer_teleport_fail", "传送失败: {0}");

        // Tool - Config
        Add("tool_config_getconfig_notexistconfig", "不存在 {0} 这个配置");
        Add("tool_config_getconfig_notexistkey", "{0} 不存在 {1} 这个键");
        Add("tool_config_switchtype", "已将 {0} 设置为 {1}!");

        // Tool - Utils
        Add("tool_utils_checkargumentcount", "预期至少 {0} 个参数 {1},但得到了 {2} 个");
        Add("tool_utils_parse_float_invalid", "'{0}' 不是有效的浮点数值!(2, 0.7, 14.1 等)");
        Add("tool_utils_parse_int_invalid", "'{0}' 不是有效的整数值!");
        Add("tool_utils_string_nullorempty", "输入字符串不能为空或空值");

        // Tool - Inventory
        Add("tool_inventory_bodynull", "玩家身体对象为空");
        Add("tool_inventory_id_nullorempty", "物品 ID 不能为空或空白");
        Add("tool_inventory_summary_header", "[物品栏]");
        Add("tool_inventory_summary_handslot", "{0}* (手持)");
        Add("tool_inventory_summary_slot", "{0}");
        Add("tool_inventory_summary_empty", "空");
        Add("tool_inventory_summary_wearables", "[穿戴]");
        Add("tool_inventory_empty", "(空)");
    }
}
