using Bark.Base;

namespace Bark.Example.Lang;

public class ZhTwLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-TW";

    protected override void BuildLocaleData()
    {
        // Options
        Option("bark.game.test", "测试选项", "没有实际用处");

        // Log - Console
        Log("console.null_or_empty", "命令不能為空或空值");
        Log("console.not_initialized", "ConsoleScript 未初始化");

        // Log - World
        Log("world.check_for_world", "沒有加載任何世界。要不試試開始遊戲?");
        Log("world.place_block", "在 {0} 生成方塊 {1} 失敗:{2}");
        Log("world.place_item", "在 {0} 生成物品 {1} 失敗:{2}");
        Log("world.place_item.null_or_empty", "物品不能為空或空值");
        Log("world.try_get_sprite", "未找到背景精靈: {0}");

        // Log - Player
        Log("player.body_null", "玩家身體物件為空");
        Log("player.item.null_or_empty", "物品標識符不能為空或空白");
        Log("player.slot.out_of_range", "物品欄索引超出範圍。最大槽位數: {0}");
        Log("player.load_item.fail", "加載或實例化物品資源失敗: '{0}'");
        Log("player.load_item.missing_component", "資源 '{0}' 已加載但缺少所需的 Item 組件");

        // Log - Multiplayer
        Log("multiplayer.player_name.null_or_empty", "玩家名稱不能為空或空值");
        Log("multiplayer.teleport.success", "已傳送: {0} 到 {1}");
        Log("multiplayer.teleport.fail", "傳送失敗: {0}");

        // Log - Config
        Log("config.get_config.not_exist_config", "不存在 {0} 這個配置");
        Log("config.get_config.not_exist_key", "{0} 不存在 {1} 這個鍵");
        Log("config.switch_type", "已將 {0} 設置為 {1}!");

        // Log - Utils
        Log("utils.check_argument_count", "預期至少 {0} 個參數 {1},但得到了 {2} 個");
        Log("utils.parse.float_invalid", "'{0}' 不是有效的浮點數值!(2, 0.7, 14.1 等)");
        Log("utils.parse.int_invalid", "'{0}' 不是有效的整數值!");
        Log("utils.string.null_or_empty", "輸入字串不能為空或空值");

        // Log - Inventory
        Log("inventory.body_null", "玩家身體物件為空");
        Log("inventory.id.null_or_empty", "物品 ID 不能為空或空白");
        Log("inventory.summary.header", "[物品欄]");
        Log("inventory.summary.hand_slot", "{0}* (手持)");
        Log("inventory.summary.slot", "{0}");
        Log("inventory.summary.empty", "空");
        Log("inventory.summary.wearables", "[穿戴]");
        Log("inventory.empty", "(空)");

        // Log - Update
        Log("update.no_repo", "未指定 {0} 的 GitHub 倉庫，跳過更新檢查。");
        Log("update.failed", "{0} 無法檢查更新。");
        Log("update.no_version", "{0} 無法讀取最新版本號。");
        Log("update.available", "{0} 有新版本可用！{1} -> {2}");
        Log("update.uptodate", "{0} 已是最新版本 ({1})。");

        // Log - TextUtil
        Log("text.tmp_unifont_not_found", "未找到 TMP Unifont 字體 (unifont-16.0)");
        Log("text.unifont_not_found", "未找到 Unifont 字體");

        // Log - BetterLocale
        Log("betterlocale.placeholder_out_of_range", "佔位符 {{{1}}} 超出鍵 '{0}' 的範圍 (args.Length={2})");
    }
}