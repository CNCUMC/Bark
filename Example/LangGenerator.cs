using System.Collections.Generic;
using Bark.Base;

namespace Bark.Example;

internal class LangGenerator : ModLangGenMultiBase
{
    protected override string NameSpace => Plugin.NameSpace;

    protected override IEnumerable<string> LanguageCodes =>
    [
        "EN",
        "zh-CN",
        "zh-TW",
        "ru-RU"
    ];

    protected override void BuildLocaleData()
    {
        // Options
        Option("game.test",
            "Test Option", "No practical use",
            "测试选项", "没有实际用处",
            "測試選項", "沒有實際用處",
            "Тестовая опция", "Нет практического использования");

        // Command
        Command("catfcabl",
            "Create a txt file containing all Bark localizations",
            "创建一个包含所有 Bark 本地化的txt文件",
            "建立一個包含所有 Bark 本地化的txt檔案",
            "Создать txt-файл, содержащий все локализации Bark");

        // Log - Console
        Log("console.null_or_empty",
            "Command cannot be null or empty",
            "命令不能为空或空值",
            "命令不能為空或空值",
            "Команда не может быть пустой или иметь пустое значение");
        Log("console.not_initialized",
            "ConsoleScript is not initialized",
            "ConsoleScript 未初始化",
            "ConsoleScript 未初始化",
            "ConsoleScript не инициализирован");

        // Log - World
        Log("world.place_block",
            "At {0} place block {1} failed: {2}",
            "在 {0} 生成物块 {1} 失败: {2}",
            "在 {0} 生成方塊 {1} 失敗: {2}",
            "В {0} не удалось разместить блок {1}: {2}");
        Log("world.place_item",
            "At {0} place item {1} failed: {2}",
            "在 {0} 生成物品 {1} 失败: {2}",
            "在 {0} 生成物品 {1} 失敗: {2}",
            "В {0} не удалось разместить предмет {1}: {2}");

        // Log - Player
        Log("player.body_null",
            "Player body object is null",
            "玩家身体对象为空",
            "玩家身體物件為空",
            "Объект тела игрока равен null");
        Log("player.slot.out_of_range",
            "Inventory slot index out of range. Max slots: {0}",
            "物品栏索引超出范围。最大槽位数: {0}",
            "物品欄索引超出範圍。最大槽位數: {0}",
            "Индекс слота инвентаря вне диапазона. Максимум слотов: {0}");
        Log("player.load_item.fail",
            "Failed to load or instantiate item resource: '{0}'",
            "加载或实例化物品资源失败: '{0}'",
            "載入或實例化物品資源失敗: '{0}'",
            "Не удалось загрузить или создать ресурс предмета: '{0}'");
        Log("player.load_item.missing_component",
            "Resource '{0}' loaded but missing required Item component",
            "资源 '{0}' 已加载但缺少所需的 Item 组件",
            "資源 '{0}' 已載入但缺少所需的 Item 元件",
            "Ресурс '{0}' загружен, но отсутствует необходимый компонент Item");

        // Log - Multiplayer
        Log("multiplayer.player_name.null_or_empty",
            "Player name cannot be null or empty",
            "玩家名称不能为空或空值",
            "玩家名稱不能為空或空值",
            "Имя игрока не может быть пустым или иметь пустое значение");
        Log("multiplayer.teleport.success",
            "Teleported: '{0}' to {1}",
            "已传送: '{0}' 到 {1}",
            "已傳送: '{0}' 到 {1}",
            "Телепортировано: '{0}' в {1}");
        Log("multiplayer.teleport.fail",
            "Teleport failed: {0}",
            "传送失败: {0}",
            "傳送失敗: {0}",
            "Не удалось телепортироваться: {0}");

        // Log - Check
        Log("check.check_for_world",
            "No world loaded. Want to start a game?",
            "没有加载任何世界。要不试试开始游戏?",
            "沒有載入任何世界。要不試試開始遊戲?",
            "Мир не загружен. Хотите начать игру?");
        Log("check.check_argument_count",
            "Expected at least {0} argument(s) {1}, but got {2}",
            "预期至少 {0} 个参数 {1}，但得到了 {2} 个",
            "預期至少 {0} 個參數 {1}，但得到了 {2} 個",
            "Ожидается как минимум {0} аргумент(ов) {1}, но получено {2}");
        Log("check.parse.float_invalid",
            "'{0}' is not a valid floating-point number! (2, 0.7, 14.1, etc.)",
            "'{0}' 不是有效的浮点数值！（2, 0.7, 14.1 等）",
            "'{0}' 不是有效的浮點數值！（2, 0.7, 14.1 等）",
            "'{0}' не является допустимым числом с плавающей точкой! (2, 0.7, 14.1 и т.д.)");
        Log("check.parse.int_invalid",
            "'{0}' is not a valid integer!",
            "'{0}' 不是有效的整数值！",
            "'{0}' 不是有效的整數值！",
            "'{0}' не является допустимым целым числом!");
        Log("check.string.null_or_empty",
            "Input string cannot be null or empty",
            "输入字符串不能为空或空值",
            "輸入字串不能為空或空值",
            "Входная строка не может быть пустой или иметь пустое значение");
        Log("check.body_null",
            "Player body is null",
            "玩家身体对象为空",
            "玩家身體物件為空",
            "Тело игрока равно null");
        Log("check.console_not_initialized",
            "ConsoleScript is not initialized",
            "ConsoleScript 未初始化",
            "ConsoleScript 未初始化",
            "ConsoleScript не инициализирован");

        // Log - Update
        Log("update.no_repo",
            "No GitHub repository specified for {0}, skipping update check",
            "未指定 {0} 的 GitHub 仓库，跳过更新检查",
            "未指定 {0} 的 GitHub 儲存庫，跳過更新檢查",
            "Не указан репозиторий GitHub для {0}, проверка обновлений пропущена");
        Log("update.failed",
            "{0} failed to check for updates",
            "{0} 无法检查更新",
            "{0} 無法檢查更新",
            "{0} не удалось проверить обновления");
        Log("update.no_version",
            "{0} failed to read latest version number",
            "{0} 无法读取最新版本号",
            "{0} 無法讀取最新版本號",
            "{0} не удалось прочитать номер последней версии");
        Log("update.available",
            "{0} has a new version available! {1} -> {2}",
            "{0} 有新版本可用！{1} -> {2}",
            "{0} 有新版本可用！{1} -> {2}",
            "{0} доступна новая версия! {1} -> {2}");
        Log("update.up_to_date",
            "{0} is up to date ({1})",
            "{0} 已是最新版本 ({1})",
            "{0} 已是最新版本 ({1})",
            "{0} обновлён до последней версии ({1})");

        // Log - TextUtil
        Log("text.font_not_found",
            "Font '{0}' not found",
            "未找到 {0} 字体",
            "未找到 {0} 字型",
            "Шрифт '{0}' не найден");

        // Log - BetterLocale
        Log("better_locale.placeholder_out_of_range",
            "Placeholder {{{1}}} is out of range for key '{0}' (args.Length={2})",
            "占位符 {{{1}}} 超出键 '{0}' 的范围 (args.Length={2})",
            "佔位符 {{{1}}} 超出鍵 '{0}' 的範圍 (args.Length={2})",
            "Заполнитель {{{1}}} выходит за пределы диапазона для ключа '{0}' (args.Length={2})");
    }
}
