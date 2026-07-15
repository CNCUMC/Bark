using Bark.Base;

namespace Bark.Example.Lang;

public class ZhTwLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-TW";
    protected override string NameSpace => "bark";

    protected override void BuildLocaleData()
    {
        // Options
        Option("game.test", "測試選項", "沒有實際用處");

        // Command
        Command("catfcabl", "建立一個包含所有 Bark 本地化的txt檔案");

        // Log - Console
        Log("console.null_or_empty", "命令不能為空或空值");
        Log("console.not_initialized", "ConsoleScript 未初始化");

        // Log - World
        Log("world.place_block", "在 {0} 生成方塊 {1} 失敗: {2}");
        Log("world.place_item", "在 {0} 生成物品 {1} 失敗: {2}");

        // Log - Player
        Log("player.body_null", "玩家身體物件為空");
        Log("player.slot.out_of_range", "物品欄索引超出範圍。最大槽位數: {0}");
        Log("player.load_item.fail", "載入或實例化物品資源失敗: '{0}'");
        Log("player.load_item.missing_component", "資源 '{0}' 已載入但缺少所需的 Item 元件");

        // Log - Multiplayer
        Log("multiplayer.player_name.null_or_empty", "玩家名稱不能為空或空值");
        Log("multiplayer.teleport.success", "已傳送: '{0}' 到 {1}");
        Log("multiplayer.teleport.fail", "傳送失敗: {0}");

        // Log - Check
        Log("check.check_for_world", "沒有載入任何世界。要不試試開始遊戲?");
        Log("check.check_argument_count", "預期至少 {0} 個參數 {1}，但得到了 {2} 個");
        Log("check.parse.float_invalid", "'{0}' 不是有效的浮點數值！（2, 0.7, 14.1 等）");
        Log("check.parse.int_invalid", "'{0}' 不是有效的整數值！");
        Log("check.string.null_or_empty", "輸入字串不能為空或空值");

        // Log - Update
        Log("update.no_repo", "未指定 {0} 的 GitHub 儲存庫，跳過更新檢查");
        Log("update.failed", "{0} 無法檢查更新");
        Log("update.no_version", "{0} 無法讀取最新版本號");
        Log("update.available", "{0} 有新版本可用！{1} -> {2}");
        Log("update.up_to_date", "{0} 已是最新版本 ({1})");

        // Log - TextUtil
        Log("text.font_not_found", "未找到 {0} 字型");

        // Log - BetterLocale
        Log("better_locale.placeholder_out_of_range", "佔位符 {{{1}}} 超出鍵 '{0}' 的範圍 (args.Length={2})");
    }
}