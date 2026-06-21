using Bark.Base;

namespace Bark.Example.Lang;

public class ZhCnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-CN";

    protected override void BuildLocaleData()
    {
        // Log - Console
        Add("log.console.null_or_empty", "命令不能为空或空值");
        Add("log.console.not_initialized", "ConsoleScript 未初始化");

        // Log - World
        Add("log.world.check_for_world", "没有加载任何世界。要不试试开始游戏?");
        Add("log.world.place_block", "在 {0} 生成方块 {1} 失败:{2}");
        Add("log.world.place_item", "在 {0} 生成物品 {1} 失败:{2}");
        Add("log.world.place_item.null_or_empty", "物品不能为空或空值");
        Add("log.world.try_get_sprite", "未找到背景精灵: {0}");

        // Log - Player
        Add("log.player.body_null", "玩家身体对象为空");
        Add("log.player.item.null_or_empty", "物品标识符不能为空或空白");
        Add("log.player.slot.out_of_range", "物品栏索引超出范围。最大槽位数: {0}");
        Add("log.player.load_item.fail", "加载或实例化物品资源失败: '{0}'");
        Add("log.player.load_item.missing_component", "资源 '{0}' 已加载但缺少所需的 Item 组件");

        // Log - Multiplayer
        Add("log.multiplayer.player_name.null_or_empty", "玩家名称不能为空或空值");
        Add("log.multiplayer.teleport.success", "已传送: {0} 到 {1}");
        Add("log.multiplayer.teleport.fail", "传送失败: {0}");

        // Log - Config
        Add("log.config.get_config.not_exist_config", "不存在 {0} 这个配置");
        Add("log.config.get_config.not_exist_key", "{0} 不存在 {1} 这个键");
        Add("log.config.switch_type", "已将 {0} 设置为 {1}!");

        // Log - Utils
        Add("log.utils.check_argument_count", "预期至少 {0} 个参数 {1},但得到了 {2} 个");
        Add("log.utils.parse.float_invalid", "'{0}' 不是有效的浮点数值!(2, 0.7, 14.1 等)");
        Add("log.utils.parse.int_invalid", "'{0}' 不是有效的整数值!");
        Add("log.utils.string.null_or_empty", "输入字符串不能为空或空值");

        // Log - Inventory
        Add("log.inventory.body_null", "玩家身体对象为空");
        Add("log.inventory.id.null_or_empty", "物品 ID 不能为空或空白");
        Add("log.inventory.summary.header", "[物品栏]");
        Add("log.inventory.summary.hand_slot", "{0}* (手持)");
        Add("log.inventory.summary.slot", "{0}");
        Add("log.inventory.summary.empty", "空");
        Add("log.inventory.summary.wearables", "[穿戴]");
        Add("log.inventory.empty", "(空)");
    }
}