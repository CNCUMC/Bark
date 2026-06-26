using Bark.Base;

namespace Bark.Example.Lang;

public class ZhTwLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-TW";

    protected override void BuildLocaleData()
    {
        // Options
        Other("gameset.bark.game.test", "测试选项");
        Other("gameset.bark.game.testdsc", "没有实际用处");

        // Command
        Other("command.test_hello.description", "測試Hello");
        Other("command.test_hello.text", "你好世界 by Bark! {0}!");

        Other("command.spawn_block.description", "在滑鼠位置生成一個方塊。");
        Other("command.spawn_block.block_id", "要生成的方塊 ID。");
        Other("command.spawn_block.invalid_block_id", "'{0}' 不是有效的方塊 ID。");
        Other("command.spawn_block.success", "已生成方塊 {0}。");

        Other("command.list_background.description", "列出所有可用的背景 ID。");
        Other("command.list_background.header", "找到 {0} 個背景資源：");
        Other("command.list_background.none", "未找到背景資源。");

        Other("command.spawn_background.description", "在滑鼠位置生成一個背景。");
        Other("command.spawn_background.background_id", "要生成的背景 ID。");
        Other("command.spawn_background.invalid_background_id", "背景 ID 不能為空。");
        Other("command.spawn_background.success", "已生成背景 {0}。");

        // Log - Console
        Other("log.console.null_or_empty", "命令不能為空或空值");
        Other("log.console.not_initialized", "ConsoleScript 未初始化");

        // Log - World
        Other("log.world.check_for_world", "沒有加載任何世界。要不試試開始遊戲?");
        Other("log.world.place_block", "在 {0} 生成方塊 {1} 失敗:{2}");
        Other("log.world.place_item", "在 {0} 生成物品 {1} 失敗:{2}");
        Other("log.world.place_item.null_or_empty", "物品不能為空或空值");
        Other("log.world.try_get_sprite", "未找到背景精靈: {0}");

        // Log - Player
        Other("log.player.body_null", "玩家身體物件為空");
        Other("log.player.item.null_or_empty", "物品標識符不能為空或空白");
        Other("log.player.slot.out_of_range", "物品欄索引超出範圍。最大槽位數: {0}");
        Other("log.player.load_item.fail", "加載或實例化物品資源失敗: '{0}'");
        Other("log.player.load_item.missing_component", "資源 '{0}' 已加載但缺少所需的 Item 組件");

        // Log - Multiplayer
        Other("log.multiplayer.player_name.null_or_empty", "玩家名稱不能為空或空值");
        Other("log.multiplayer.teleport.success", "已傳送: {0} 到 {1}");
        Other("log.multiplayer.teleport.fail", "傳送失敗: {0}");

        // Log - Config
        Other("log.config.get_config.not_exist_config", "不存在 {0} 這個配置");
        Other("log.config.get_config.not_exist_key", "{0} 不存在 {1} 這個鍵");
        Other("log.config.switch_type", "已將 {0} 設置為 {1}!");

        // Log - Utils
        Other("log.utils.check_argument_count", "預期至少 {0} 個參數 {1},但得到了 {2} 個");
        Other("log.utils.parse.float_invalid", "'{0}' 不是有效的浮點數值!(2, 0.7, 14.1 等)");
        Other("log.utils.parse.int_invalid", "'{0}' 不是有效的整數值!");
        Other("log.utils.string.null_or_empty", "輸入字串不能為空或空值");

        // Log - Inventory
        Other("log.inventory.body_null", "玩家身體物件為空");
        Other("log.inventory.id.null_or_empty", "物品 ID 不能為空或空白");
        Other("log.inventory.summary.header", "[物品欄]");
        Other("log.inventory.summary.hand_slot", "{0}* (手持)");
        Other("log.inventory.summary.slot", "{0}");
        Other("log.inventory.summary.empty", "空");
        Other("log.inventory.summary.wearables", "[穿戴]");
        Other("log.inventory.empty", "(空)");
    }
}