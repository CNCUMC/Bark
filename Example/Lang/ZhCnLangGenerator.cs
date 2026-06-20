using Bark.Core;

namespace Bark.Example.Lang;

public class ZhCnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-CN";

    protected override void BuildLocaleData()
    {
        // Command
        Add("command.test_hello.description", "测试Hello");
        Add("command.test_hello.text", "你好世界 by Bark! {0}!");

        Add("command.spawn_block.description", "在鼠标位置生成一个方块。");
        Add("command.spawn_block.block_id", "要生成的方块 ID。");
        Add("command.spawn_block.invalid_block_id", "'{0}' 不是有效的方块 ID。");
        Add("command.spawn_block.success", "已生成方块 {0}。");

        Add("command.list_background.description", "列出所有可用的背景 ID。");
        Add("command.list_background.header", "找到 {0} 个背景资源：");
        Add("command.list_background.none", "未找到背景资源。");

        Add("command.spawn_background.description", "在鼠标位置生成一个背景。");
        Add("command.spawn_background.background_id", "要生成的背景 ID。");
        Add("command.spawn_background.invalid_background_id", "背景 ID 不能为空。");
        Add("command.spawn_background.success", "已生成背景 {0}。");

        // Tool - Console
        Add("tool.console.null_or_empty", "命令不能为空或空值");
        Add("tool.console.not_initialized", "ConsoleScript 未初始化");

        // Tool - World
        Add("tool.world.check_for_world", "没有加载任何世界。要不试试开始游戏?");
        Add("tool.world.place_block", "在 {0} 生成方块 {1} 失败:{2}");
        Add("tool.world.place_item", "在 {0} 生成物品 {1} 失败:{2}");
        Add("tool.world.place_item.null_or_empty", "物品不能为空或空值");
        Add("tool.world.try_get_sprite", "未找到背景精灵: {0}");

        // Tool - Player
        Add("tool.player.body_null", "玩家身体对象为空");
        Add("tool.player.item.null_or_empty", "物品标识符不能为空或空白");
        Add("tool.player.slot.out_of_range", "物品栏索引超出范围。最大槽位数: {0}");
        Add("tool.player.load_item.fail", "加载或实例化物品资源失败: '{0}'");
        Add("tool.player.load_item.missing_component", "资源 '{0}' 已加载但缺少所需的 Item 组件");

        // Tool - Multiplayer
        Add("tool.multiplayer.player_name.null_or_empty", "玩家名称不能为空或空值");
        Add("tool.multiplayer.teleport.success", "已传送: {0} 到 {1}");
        Add("tool.multiplayer.teleport.fail", "传送失败: {0}");

        // Tool - Config
        Add("tool.config.get_config.not_exist_config", "不存在 {0} 这个配置");
        Add("tool.config.get_config.not_exist_key", "{0} 不存在 {1} 这个键");
        Add("tool.config.switch_type", "已将 {0} 设置为 {1}!");

        // Tool - Utils
        Add("tool.utils.check_argument_count", "预期至少 {0} 个参数 {1},但得到了 {2} 个");
        Add("tool.utils.parse.float_invalid", "'{0}' 不是有效的浮点数值!(2, 0.7, 14.1 等)");
        Add("tool.utils.parse.int_invalid", "'{0}' 不是有效的整数值!");
        Add("tool.utils.string.null_or_empty", "输入字符串不能为空或空值");

        // Tool - Inventory
        Add("tool.inventory.body_null", "玩家身体对象为空");
        Add("tool.inventory.id.null_or_empty", "物品 ID 不能为空或空白");
        Add("tool.inventory.summary.header", "[物品栏]");
        Add("tool.inventory.summary.hand_slot", "{0}* (手持)");
        Add("tool.inventory.summary.slot", "{0}");
        Add("tool.inventory.summary.empty", "空");
        Add("tool.inventory.summary.wearables", "[穿戴]");
        Add("tool.inventory.empty", "(空)");
    }
}