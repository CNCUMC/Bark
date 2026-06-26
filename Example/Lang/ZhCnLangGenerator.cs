using Bark.Base;

namespace Bark.Example.Lang;

public class ZhCnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-CN";

    protected override void BuildLocaleData()
    {
        // Options
        Other("gameset.bark.game.test", "测试选项");
        Other("gameset.bark.game.testdsc", "没有实际用处");

        // Command
        Other("command.test_hello.description", "测试Hello");
        Other("command.test_hello.text", "你好世界 by Bark! {0}!");

        Other("command.spawn_block.description", "在鼠标位置生成一个方块。");
        Other("command.spawn_block.block_id", "要生成的方块 ID。");
        Other("command.spawn_block.invalid_block_id", "'{0}' 不是有效的方块 ID。");
        Other("command.spawn_block.success", "已生成方块 {0}。");

        Other("command.list_background.description", "列出所有可用的背景 ID。");
        Other("command.list_background.header", "找到 {0} 个背景资源：");
        Other("command.list_background.none", "未找到背景资源。");

        Other("command.spawn_background.description", "在鼠标位置生成一个背景。");
        Other("command.spawn_background.background_id", "要生成的背景 ID。");
        Other("command.spawn_background.invalid_background_id", "背景 ID 不能为空。");
        Other("command.spawn_background.success", "已生成背景 {0}。");

        // Log - Console
        Other("log.console.null_or_empty", "命令不能为空或空值");
        Other("log.console.not_initialized", "ConsoleScript 未初始化");

        // Log - World
        Other("log.world.check_for_world", "没有加载任何世界。要不试试开始游戏?");
        Other("log.world.place_block", "在 {0} 生成方块 {1} 失败:{2}");
        Other("log.world.place_item", "在 {0} 生成物品 {1} 失败:{2}");
        Other("log.world.place_item.null_or_empty", "物品不能为空或空值");
        Other("log.world.try_get_sprite", "未找到背景精灵: {0}");

        // Log - Player
        Other("log.player.body_null", "玩家身体对象为空");
        Other("log.player.item.null_or_empty", "物品标识符不能为空或空白");
        Other("log.player.slot.out_of_range", "物品栏索引超出范围。最大槽位数: {0}");
        Other("log.player.load_item.fail", "加载或实例化物品资源失败: '{0}'");
        Other("log.player.load_item.missing_component", "资源 '{0}' 已加载但缺少所需的 Item 组件");

        // Log - Multiplayer
        Other("log.multiplayer.player_name.null_or_empty", "玩家名称不能为空或空值");
        Other("log.multiplayer.teleport.success", "已传送: {0} 到 {1}");
        Other("log.multiplayer.teleport.fail", "传送失败: {0}");

        // Log - Config
        Other("log.config.get_config.not_exist_config", "不存在 {0} 这个配置");
        Other("log.config.get_config.not_exist_key", "{0} 不存在 {1} 这个键");
        Other("log.config.switch_type", "已将 {0} 设置为 {1}!");

        // Log - Utils
        Other("log.utils.check_argument_count", "预期至少 {0} 个参数 {1},但得到了 {2} 个");
        Other("log.utils.parse.float_invalid", "'{0}' 不是有效的浮点数值!(2, 0.7, 14.1 等)");
        Other("log.utils.parse.int_invalid", "'{0}' 不是有效的整数值!");
        Other("log.utils.string.null_or_empty", "输入字符串不能为空或空值");

        // Log - Inventory
        Other("log.inventory.body_null", "玩家身体对象为空");
        Other("log.inventory.id.null_or_empty", "物品 ID 不能为空或空白");
        Other("log.inventory.summary.header", "[物品栏]");
        Other("log.inventory.summary.hand_slot", "{0}* (手持)");
        Other("log.inventory.summary.slot", "{0}");
        Other("log.inventory.summary.empty", "空");
        Other("log.inventory.summary.wearables", "[穿戴]");
        Other("log.inventory.empty", "(空)");
    }
}