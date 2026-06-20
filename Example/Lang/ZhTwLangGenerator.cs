using Bark.Core;

namespace Bark.Example.Lang;

public class ZhTwLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-TW";

    protected override void BuildLocaleData()
    {
        // Command
        Add("command.test_hello.description", "測試Hello");
        Add("command.test_hello.text", "你好世界 by Bark! {0}!");

        Add("command.spawn_block.description", "在滑鼠位置生成一個方塊。");
        Add("command.spawn_block.block_id", "要生成的方塊 ID。");
        Add("command.spawn_block.invalid_block_id", "'{0}' 不是有效的方塊 ID。");
        Add("command.spawn_block.success", "已生成方塊 {0}。");

        Add("command.list_background.description", "列出所有可用的背景 ID。");
        Add("command.list_background.header", "找到 {0} 個背景資源：");
        Add("command.list_background.none", "未找到背景資源。");

        Add("command.spawn_background.description", "在滑鼠位置生成一個背景。");
        Add("command.spawn_background.background_id", "要生成的背景 ID。");
        Add("command.spawn_background.invalid_background_id", "背景 ID 不能為空。");
        Add("command.spawn_background.success", "已生成背景 {0}。");

        // Tool - Console
        Add("tool.console.null_or_empty", "命令不能為空或空值");
        Add("tool.console.not_initialized", "ConsoleScript 未初始化");

        // Tool - World
        Add("tool.world.check_for_world", "沒有加載任何世界。要不試試開始遊戲?");
        Add("tool.world.place_block", "在 {0} 生成方塊 {1} 失敗:{2}");
        Add("tool.world.place_item", "在 {0} 生成物品 {1} 失敗:{2}");
        Add("tool.world.place_item.null_or_empty", "物品不能為空或空值");
        Add("tool.world.try_get_sprite", "未找到背景精靈: {0}");

        // Tool - Player
        Add("tool.player.body_null", "玩家身體物件為空");
        Add("tool.player.item.null_or_empty", "物品標識符不能為空或空白");
        Add("tool.player.slot.out_of_range", "物品欄索引超出範圍。最大槽位數: {0}");
        Add("tool.player.load_item.fail", "加載或實例化物品資源失敗: '{0}'");
        Add("tool.player.load_item.missing_component", "資源 '{0}' 已加載但缺少所需的 Item 組件");

        // Tool - Multiplayer
        Add("tool.multiplayer.player_name.null_or_empty", "玩家名稱不能為空或空值");
        Add("tool.multiplayer.teleport.success", "已傳送: {0} 到 {1}");
        Add("tool.multiplayer.teleport.fail", "傳送失敗: {0}");

        // Tool - Config
        Add("tool.config.get_config.not_exist_config", "不存在 {0} 這個配置");
        Add("tool.config.get_config.not_exist_key", "{0} 不存在 {1} 這個鍵");
        Add("tool.config.switch_type", "已將 {0} 設置為 {1}!");

        // Tool - Utils
        Add("tool.utils.check_argument_count", "預期至少 {0} 個參數 {1},但得到了 {2} 個");
        Add("tool.utils.parse.float_invalid", "'{0}' 不是有效的浮點數值!(2, 0.7, 14.1 等)");
        Add("tool.utils.parse.int_invalid", "'{0}' 不是有效的整數值!");
        Add("tool.utils.string.null_or_empty", "輸入字串不能為空或空值");

        // Tool - Inventory
        Add("tool.inventory.body_null", "玩家身體物件為空");
        Add("tool.inventory.id.null_or_empty", "物品 ID 不能為空或空白");
        Add("tool.inventory.summary.header", "[物品欄]");
        Add("tool.inventory.summary.hand_slot", "{0}* (手持)");
        Add("tool.inventory.summary.slot", "{0}");
        Add("tool.inventory.summary.empty", "空");
        Add("tool.inventory.summary.wearables", "[穿戴]");
        Add("tool.inventory.empty", "(空)");
    }
}
