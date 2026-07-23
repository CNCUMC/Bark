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
        Command("script.description",
            "Script mod management commands",
            "脚本模组管理命令",
            "腳本模組管理命令",
            "Управление скриптовыми модами");
        Command("script.reload",
            "Reload Script",
            "重载脚本",
            "重載腳本",
            "Перезагрузить сценарий");
        Command("script.help.header",
            "Script mod commands:",
            "脚本模组命令:",
            "腳本模組命令:",
            "Команды скриптовых модов:");
        Command("script.help.help",
            "Show this help",
            "显示此帮助",
            "顯示此說明",
            "Показать эту справку");
        Command("script.help.reload",
            "Reload all script mods",
            "重载所有脚本模组",
            "重載所有腳本模組",
            "Перезагрузить все скриптовые моды");
        Command("script.list.header",
            "Script mod list ({0}):",
            "脚本模组列表（{0}）:",
            "腳本模組列表（{0}）:",
            "Список скриптовых модов ({0}):");
        Command("script.list.item",
            "  {0} v{1} [{2}] ({3})",
            "  {0} v{1} [{2}]（{3}）",
            "  {0} v{1} [{2}]（{3}）",
            "  {0} v{1} [{2}] ({3})");

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

        // Log - ScriptMod
        Log("native_dll_copied",
            "Copied {0} to game root directory",
            "已将 {0} 复制到游戏根目录",
            "已將 {0} 複製到遊戲根目錄",
            "{0} скопирован в корневую директорию игры");
        Log("puerts_runtime_copied",
            "Copied puerts/ runtime folder to game root directory",
            "已将 puerts/ 运行时文件夹复制到游戏根目录",
            "已將 puerts/ 運行時資料夾複製到遊戲根目錄",
            "Папка运行时 puerts/ скопирована в корневую директорию игры");
        Log("script_mod_loader.dir_not_found",
            "ScriptMods directory not found: {0}",
            "ScriptMods 目录不存在: {0}",
            "ScriptMods 目錄不存在: {0}",
            "Каталог ScriptMods не найден: {0}");
        Log("script_mod_loader.dir_created",
            "Created ScriptMods directory: {0}",
            "已创建 ScriptMods 目录: {0}",
            "已建立 ScriptMods 目錄: {0}",
            "Каталог ScriptMods создан: {0}");
        Log("script_mod_loader.no_mods",
            "No script mods found",
            "没有发现脚本模组",
            "沒有發現腳本模組",
            "Скриптовые моды не найдены");
        Log("script_mod_loader.found_manifests",
            "Found {0} mod manifest(s)",
            "发现 {0} 个模组清单",
            "發現 {0} 個模組清單",
            "Найдено {0} манифест(ов)");
        Log("script_mod_loader.skip_no_manifest",
            "Skipped (no mod.json): {0}",
            "跳过（无 mod.json）: {0}",
            "跳過（無 mod.json）: {0}",
            "Пропущено (нет mod.json): {0}");
        Log("script_mod_loader.parse_failed",
            "Failed to parse manifest: {0}",
            "解析失败: {0}",
            "解析失敗: {0}",
            "Не удалось разобрать манифест: {0}");
        Log("script_mod_loader.missing_id",
            "Missing 'id' field: {0}",
            "缺少 id 字段: {0}",
            "缺少 id 欄位: {0}",
            "Отсутствует поле 'id': {0}");
        Log("script_mod_loader.missing_version",
            "Missing 'version' field: {0}",
            "缺少 version 字段: {0}",
            "缺少 version 欄位: {0}",
            "Отсутствует поле 'version': {0}");
        Log("script_mod_loader.no_entry_file",
            "Entry file not found (main.js/lua/py): {0}",
            "未找到入口文件（main.js/lua/py）: {0}",
            "未找到入口檔案（main.js/lua/py）: {0}",
            "Файл входа не найден (main.js/lua/py): {0}");
        Log("script_mod_loader.manifest_read",
            "Manifest read: {0} v{1} ({2})",
            "已读取清单: {0} v{1} ({2})",
            "已讀取清單: {0} v{1} ({2})",
            "Манифест прочитан: {0} v{1} ({2})");
        Log("script_mod_loader.manifest_read_error",
            "Failed to read manifest: {0} - {1}",
            "读取清单失败: {0} - {1}",
            "讀取清單失敗: {0} - {1}",
            "Не удалось прочитать манифест: {0} - {1}");
        Log("script_mod_loader.loaded_count",
            "Successfully loaded {0} script mod(s)",
            "成功加载 {0} 个脚本模组",
            "成功載入 {0} 個腳本模組",
            "Успешно загружено {0} скриптовых мод(ов)");
        Log("script_mod_loader.duplicate_id",
            "Duplicate mod ID: {0}, skipped",
            "重复的模组 ID: {0}，跳过",
            "重複的模組 ID: {0}，跳過",
            "Дублирующийся ID мода: {0}, пропущено");
        Log("script_mod_loader.unsupported_language",
            "Unsupported language: {0} ({1})",
            "不支持的语言: {0} ({1})",
            "不支援的語言: {0} ({1})",
            "Неподдерживаемый язык: {0} ({1})");
        Log("script_mod_loader.mod_loaded",
            "Loaded: {0} v{1}",
            "已加载: {0} v{1}",
            "已載入: {0} v{1}",
            "Загружено: {0} v{1}");
        Log("script_mod_loader.load_failed",
            "Failed to load: {0} - {1}",
            "加载失败: {0} - {1}",
            "載入失敗: {0} - {1}",
            "Не удалось загрузить: {0} - {1}");
        Log("script_mod_loader.mod_loading",
            "[{0}] Loading {1} v{2}",
            "[{0}] 加载 {1} v{2}",
            "[{0}] 載入 {1} v{2}",
            "[{0}] Загрузка {1} v{2}");
        Log("script_mod_loader.circular_dependency",
            "Skipped (circular dependency or unmet dependency): {0}",
            "跳过（循环依赖或依赖未满足）: {0}",
            "跳過（循環依賴或依賴未滿足）: {0}",
            "Пропущено (циклическая зависимость или невыполненная зависимость): {0}");
        Log("script_mod_loader.hook_failed",
            "Hook '{1}' failed for mod '{0}': {2}",
            "模组 '{0}' 的钩子 '{1}' 执行失败: {2}",
            "模組 '{0}' 的鉤子 '{1}' 執行失敗: {2}",
            "Хук '{1}' мода '{0}' завершился ошибкой: {2}");
        Log("script_mod_loader.reload_unload_failed",
            "Failed to unload mod '{0}': {1}",
            "卸载模组 '{0}' 失败: {1}",
            "卸載模組 '{0}' 失敗: {1}",
            "Не удалось выгрузить мод '{0}': {1}");
        Log("script_mod_loader.python_not_available",
            "Python runtime not available, skipping mod '{0}'",
            "Python 运行时不可用，跳过模组 '{0}'",
            "Python 運行時不可用，跳過模組 '{0}'",
            "Python рантайм недоступен, пропуск мода '{0}'");
    }
}
