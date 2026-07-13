using Bark.Base;

namespace Bark.Example.Lang;

public class ZhCnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-CN";

    protected override void BuildLocaleData()
    {
        // Options
        Option("bark.game.test", "测试选项", "没有实际用处");

        // Log - Console
        Log("console.null_or_empty", "命令不能为空或空值");
        Log("console.not_initialized", "ConsoleScript 未初始化");

        // Log - World
        Log("world.check_for_world", "没有加载任何世界。要不试试开始游戏?");
        Log("world.place_block", "在 {0} 生成方块 {1} 失败:{2}");
        Log("world.place_item", "在 {0} 生成物品 {1} 失败:{2}");
        Log("world.place_item.null_or_empty", "物品不能为空或空值");
        Log("world.try_get_sprite", "未找到背景精灵: {0}");

        // Log - Player
        Log("player.body_null", "玩家身体对象为空");
        Log("player.item.null_or_empty", "物品标识符不能为空或空白");
        Log("player.slot.out_of_range", "物品栏索引超出范围。最大槽位数: {0}");
        Log("player.load_item.fail", "加载或实例化物品资源失败: '{0}'");
        Log("player.load_item.missing_component", "资源 '{0}' 已加载但缺少所需的 Item 组件");

        // Log - Multiplayer
        Log("multiplayer.player_name.null_or_empty", "玩家名称不能为空或空值");
        Log("multiplayer.teleport.success", "已传送: {0} 到 {1}");
        Log("multiplayer.teleport.fail", "传送失败: {0}");

        // Log - Config
        Log("config.get_config.not_exist_config", "不存在 {0} 这个配置");
        Log("config.get_config.not_exist_key", "{0} 不存在 {1} 这个键");
        Log("config.switch_type", "已将 {0} 设置为 {1}!");

        // Log - Utils
        Log("utils.check_argument_count", "预期至少 {0} 个参数 {1},但得到了 {2} 个");
        Log("utils.parse.float_invalid", "'{0}' 不是有效的浮点数值!(2, 0.7, 14.1 等)");
        Log("utils.parse.int_invalid", "'{0}' 不是有效的整数值!");
        Log("utils.string.null_or_empty", "输入字符串不能为空或空值");

        // Log - Inventory
        Log("inventory.body_null", "玩家身体对象为空");
        Log("inventory.id.null_or_empty", "物品 ID 不能为空或空白");
        Log("inventory.summary.header", "[物品栏]");
        Log("inventory.summary.hand_slot", "{0}* (手持)");
        Log("inventory.summary.slot", "{0}");
        Log("inventory.summary.empty", "空");
        Log("inventory.summary.wearables", "[穿戴]");
        Log("inventory.empty", "(空)");

        // Log - Update
        Log("update.no_repo", "未指定 {0} 的 GitHub 仓库，跳过更新检查");
        Log("update.failed", "{0} 无法检查更新");
        Log("update.no_version", "{0} 无法读取最新版本号");
        Log("update.available", "{0} 有新版本可用！{1} -> {2}");
        Log("update.uptodate", "{0} 已是最新版本 ({1})");

        // Log - TextUtil
        Log("text.tmp_unifont_not_found", "未找到 TMP Unifont 字体 (unifont-16.0)");
        Log("text.unifont_not_found", "未找到 Unifont 字体");
    }
}