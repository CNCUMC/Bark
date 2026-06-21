using Bark.Base;

namespace Bark.Example.Lang;

public class ZhTwLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-TW";

    protected override void BuildLocaleData()
    {
        // Log - Console
        Add("log.console.null_or_empty", "命令不能為空或空值");
        Add("log.console.not_initialized", "ConsoleScript 未初始化");

        // Log - World
        Add("log.world.check_for_world", "沒有加載任何世界。要不試試開始遊戲?");
        Add("log.world.place_block", "在 {0} 生成方塊 {1} 失敗:{2}");
        Add("log.world.place_item", "在 {0} 生成物品 {1} 失敗:{2}");
        Add("log.world.place_item.null_or_empty", "物品不能為空或空值");
        Add("log.world.try_get_sprite", "未找到背景精靈: {0}");

        // Log - Player
        Add("log.player.body_null", "玩家身體物件為空");
        Add("log.player.item.null_or_empty", "物品標識符不能為空或空白");
        Add("log.player.slot.out_of_range", "物品欄索引超出範圍。最大槽位數: {0}");
        Add("log.player.load_item.fail", "加載或實例化物品資源失敗: '{0}'");
        Add("log.player.load_item.missing_component", "資源 '{0}' 已加載但缺少所需的 Item 組件");

        // Log - Multiplayer
        Add("log.multiplayer.player_name.null_or_empty", "玩家名稱不能為空或空值");
        Add("log.multiplayer.teleport.success", "已傳送: {0} 到 {1}");
        Add("log.multiplayer.teleport.fail", "傳送失敗: {0}");

        // Log - Config
        Add("log.config.get_config.not_exist_config", "不存在 {0} 這個配置");
        Add("log.config.get_config.not_exist_key", "{0} 不存在 {1} 這個鍵");
        Add("log.config.switch_type", "已將 {0} 設置為 {1}!");

        // Log - Utils
        Add("log.utils.check_argument_count", "預期至少 {0} 個參數 {1},但得到了 {2} 個");
        Add("log.utils.parse.float_invalid", "'{0}' 不是有效的浮點數值!(2, 0.7, 14.1 等)");
        Add("log.utils.parse.int_invalid", "'{0}' 不是有效的整數值!");
        Add("log.utils.string.null_or_empty", "輸入字串不能為空或空值");

        // Log - Inventory
        Add("log.inventory.body_null", "玩家身體物件為空");
        Add("log.inventory.id.null_or_empty", "物品 ID 不能為空或空白");
        Add("log.inventory.summary.header", "[物品欄]");
        Add("log.inventory.summary.hand_slot", "{0}* (手持)");
        Add("log.inventory.summary.slot", "{0}");
        Add("log.inventory.summary.empty", "空");
        Add("log.inventory.summary.wearables", "[穿戴]");
        Add("log.inventory.empty", "(空)");
    }
}