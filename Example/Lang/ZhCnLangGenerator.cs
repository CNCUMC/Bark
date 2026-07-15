using Bark.Base;

namespace Bark.Example.Lang;

public class ZhCnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-CN";
    protected override string NameSpace => "bark";

    protected override void BuildLocaleData()
    {
        // Options
        Option("game.test", "测试选项", "没有实际用处");

        // Command
        Command("catfcabl", "创建一个包含所有 Bark 本地化的txt文件");

        // Log - Console
        Log("console.null_or_empty", "命令不能为空或空值");
        Log("console.not_initialized", "ConsoleScript 未初始化");

        // Log - World
        Log("world.place_block", "在 {0} 生成物块 {1} 失败: {2}");
        Log("world.place_item", "在 {0} 生成物品 {1} 失败: {2}");

        // Log - Player
        Log("player.body_null", "玩家身体对象为空");
        Log("player.slot.out_of_range", "物品栏索引超出范围。最大槽位数: {0}");
        Log("player.load_item.fail", "加载或实例化物品资源失败: '{0}'");
        Log("player.load_item.missing_component", "资源 '{0}' 已加载但缺少所需的 Item 组件");

        // Log - Multiplayer
        Log("multiplayer.player_name.null_or_empty", "玩家名称不能为空或空值");
        Log("multiplayer.teleport.success", "已传送: '{0}' 到 {1}");
        Log("multiplayer.teleport.fail", "传送失败: {0}");

        // Log - Check
        Log("check.check_for_world", "没有加载任何世界。要不试试开始游戏?");
        Log("check.check_argument_count", "预期至少 {0} 个参数 {1}，但得到了 {2} 个");
        Log("check.parse.float_invalid", "'{0}' 不是有效的浮点数值！（2, 0.7, 14.1 等）");
        Log("check.parse.int_invalid", "'{0}' 不是有效的整数值！");
        Log("check.string.null_or_empty", "输入字符串不能为空或空值");

        // Log - Update
        Log("update.no_repo", "未指定 {0} 的 GitHub 仓库，跳过更新检查");
        Log("update.failed", "{0} 无法检查更新");
        Log("update.no_version", "{0} 无法读取最新版本号");
        Log("update.available", "{0} 有新版本可用！{1} -> {2}");
        Log("update.up_to_date", "{0} 已是最新版本 ({1})");

        // Log - TextUtil
        Log("text.font_not_found", "未找到 {0} 字体");

        // Log - BetterLocale
        Log("better_locale.placeholder_out_of_range", "占位符 {{{1}}} 超出键 '{0}' 的范围 (args.Length={2})");
    }
}