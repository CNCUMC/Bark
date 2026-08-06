using System.Collections.Generic;
using Bark.Base;

namespace Bark;

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
        Command("script.reload.completed",
            "Reload script completed!",
            "重载脚本完成!",
            "重載腳本完成!",
            "Перезарядка сценария завершена!");
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
        Command("script.list",
            "Script mod list",
            "脚本模组列表",
            "腳本模組列表",
            "Список скриптовых модов");
        Command("script.list.header",
            "Script mod list ({0}):",
            "脚本模组列表 ({0}):",
            "腳本模組列表 ({0}):",
            "Список скриптовых модов ({0}):");
        Command("script.list.item",
            "  {0} v{1} [{2}] ({3})",
            "  {0} v{1} [{2}] ({3}) ",
            "  {0} v{1} [{2}] ({3}) ",
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
            "'{0}' 不是有效的浮点数值！ (2, 0.7, 14.1 等) ",
            "'{0}' 不是有效的浮點數值！ (2, 0.7, 14.1 等) ",
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
        Log("puerpython.bundled_ready",
            "Python runtime ready at {0}",
            "Python 运行环境已就绪 {0}",
            "Python 執行環境已就緒 {0}",
            "Среда выполнения Python готова в {0}");
        Log("puerpython.system_not_found",
            "Python runtime not found in Python/Python3142/, Python scripts will be skipped",
            "Python/Python3142/ 中未找到 Python 运行环境，Python 脚本将被跳过",
            "Python/Python3142/ 中未找到 Python 執行環境，Python 腳本將被跳過",
            "Среда выполнения Python не найдена в Python/Python3142/, скрипты Python будут пропущены");
        Log("puerpython.wrong_version",
            "Python version mismatch: expected 3.14.2, got {0}. Python scripts will be skipped",
            "Python 版本不匹配：需要 3.14.2，当前 {0}。Python 脚本将被跳过",
            "Python 版本不符：需要 3.14.2，當前 {0}。Python 腳本將被跳過",
            "Несовместимая версия Python: требуется 3.14.2, найдена {0}. Скрипты Python будут пропущены");
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
        Log("script_mod_loader.zip_extracted",
            "Extracted zip mod '{0}' to cache",
            "已将 zip 模组 '{0}' 解压到缓存",
            "已將 zip 模組 '{0}' 解壓到快取",
            "Zip-мод '{0}' распакован в кэш");
        Log("script_mod_loader.zip_extract_failed",
            "Failed to extract zip mod '{0}': {1}",
            "解压 zip 模组 '{0}' 失败: {1}",
            "解壓 zip 模組 '{0}' 失敗: {1}",
            "Не удалось распаковать zip-мод '{0}': {1}");
        Log("script_mod_loader.cache_cleaned",
            "Cleaned orphaned cache '{0}' (zip no longer exists)",
            "已清理孤儿缓存 '{0}'（zip 已被删除）",
            "已清理孤兒快取 '{0}'（zip 已被刪除）",
            "Очищен потерянный кэш '{0}' (zip больше не существует)");
        Log("script_mod_loader.found_manifests",
            "Found {0} mod manifest(s)",
            "发现 {0} 个模组清单",
            "發現 {0} 個模組清單",
            "Найдено {0} манифест(ов)");
        Log("script_mod_loader.skip_no_manifest",
            "Skipped (no mod.json): {0}",
            "跳过 (无 mod.json): {0}",
            "跳過 (無 mod.json): {0}",
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
        Log("script_mod_loader.id_not_snake_case",
            "Mod ID '{0}' must be snake_case: {1}",
            "模组 ID '{0}' 必须使用蛇形命名: {1}",
            "模組 ID '{0}' 必須使用蛇形命名: {1}",
            "ID мода '{0}' должен быть snake_case: {1}");
        Log("script_mod_loader.missing_version",
            "Missing 'version' field: {0}",
            "缺少 version 字段: {0}",
            "缺少 version 欄位: {0}",
            "Отсутствует поле 'version': {0}");
        Log("script_mod_loader.no_entry_file",
            "Entry file not found (main.js/lua/py): {0}",
            "未找到入口文件 (main.js/lua/py): {0}",
            "未找到入口檔案 (main.js/lua/py): {0}",
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
            "跳过 (循环依赖或依赖未满足): {0}",
            "跳過 (循環依賴或依賴未滿足): {0}",
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

        // Log - OptionsUtil
        Log("options_util.def_parse_failed",
            "Option definition parse failed for mod '{0}': {1}",
            "模组 '{0}' 的选项定义解析失败: {1}",
            "模組 '{0}' 的選項定義解析失敗: {1}",
            "Ошибка разбора определения опций мода '{0}': {1}");
        Log("options_util.config_parse_failed",
            "Config parse failed for '{0}': {1}",
            "配置解析失败 '{0}': {1}",
            "配置解析失敗 '{0}': {1}",
            "Ошибка разбора конфига '{0}': {1}");
        Log("options_util.missing_type",
            "'type' field missing for option '{0}.{1}' in config",
            "配置选项 '{0}.{1}' 缺少 'type' 字段",
            "配置選項 '{0}.{1}' 缺少 'type' 欄位",
            "Отсутствует поле 'type' у опции '{0}.{1}' в конфиге");
        Log("options_util.registered_options",
            "Registered {1} option(s) for mod '{0}'",
            "已为模组 '{0}' 注册 {1} 个设置选项",
            "已為模組 '{0}' 註冊 {1} 個設定選項",
            "Зарегистрировано {1} опций для мода '{0}'");
        Log("options_util.unknown_type",
            "Unknown option type '{2}' for '{0}.{1}'",
            "未知的选项类型 '{2}'：'{0}.{1}'",
            "未知的選項類型 '{2}'：'{0}.{1}'",
            "Неизвестный тип опции '{2}' для '{0}.{1}'");
        Log("options_util.dropdown_no_choices",
            "Dropdown option '{0}.{1}' has no choices array",
            "下拉选项 '{0}.{1}' 缺少 choices 数组",
            "下拉選項 '{0}.{1}' 缺少 choices 陣列",
            "У выпадающего списка '{0}.{1}' отсутствует массив choices");
        Log("options_util.write_config_failed",
            "Failed to write config '{0}' key '{1}': {2}",
            "写入配置 '{0}' 键 '{1}' 失败: {2}",
            "寫入配置 '{0}' 鍵 '{1}' 失敗: {2}",
            "Не удалось записать конфиг '{0}' ключ '{1}': {2}");
        Log("options_util.keycode_parse_failed",
            "Failed to parse key code: '{0}'",
            "按键码解析失败: '{0}'",
            "按鍵碼解析失敗: '{0}'",
            "Не удалось разобрать код клавиши: '{0}'");
        Log("options_util.config_changed_restart_required",
            "Config changes detected that require a game restart to take effect",
            "检测到配置变更，需要重启游戏才能生效",
            "檢測到配置變更，需要重新啟動遊戲才能生效",
            "Обнаружены изменения конфига, требуется перезапуск игры");

        // Log - Api
        Log("api.scanned",
            "Scanned and registered {0} script API class(es)",
            "扫描并注册了 {0} 个脚本 API 类",
            "掃描並註冊了 {0} 個腳本 API 類",
            "Просканировано и зарегистрировано {0} классов скриптовых API");

        // Log - Event
        Log("event.scanned",
            "Scanned and subscribed {0} event bus method(s)",
            "扫描并订阅了 {0} 个事件总线方法",
            "掃描並訂閱了 {0} 個事件總線方法",
            "Просканировано и подписано {0} методов событийной шины");
        Log("event.invalid_handler",
            "Type '{0}' method '{1}' is not a valid event handler (must be public static, single BarkEvent parameter)",
            "类型 '{0}' 的方法 '{1}' 不是有效的事件处理器（需为 public static，参数为单个 BarkEvent 子类）",
            "類型 '{0}' 的方法 '{1}' 不是有效的事件處理器（需為 public static，參數為單個 BarkEvent 子類）",
            "Метод '{1}' типа '{0}' не является допустимым обработчиком событий");
        Log("event.handler_failed",
            "[{0}.{1}] {2}",
            "[{0}.{1}] {2}",
            "[{0}.{1}] {2}",
            "[{0}.{1}] {2}");
        Log("script_event.scanned",
            "Scanned {0} script event hook(s)",
            "扫描到 {0} 个脚本事件钩子",
            "掃描到 {0} 個腳本事件鉤子",
            "Просканировано {0} хуков скриптовых событий");
        Log("item_event.patch_use_ok",
            "Patched {0} for item use detection",
            "已补丁 {0} 用于物品使用检测",
            "已補丁 {0} 用於物品使用檢測",
            "Запатчен {0} для обнаружения использования предметов");
        Log("item_event.patch_attack_ok",
            "Patched {0} for item attack detection",
            "已补丁 {0} 用于物品攻击检测",
            "已補丁 {0} 用於物品攻擊檢測",
            "Запатчен {0} для обнаружения атак предметами");
        Log("item_event.wear_blocked_null_item",
            "Cannot equip: item is null",
            "无法装备：物品为空",
            "無法裝備：物品為空",
            "Невозможно надеть: предмет равен null");
        Log("item_event.wear_blocked_empty_id",
            "Cannot equip: item id is empty",
            "无法装备：物品 ID 为空",
            "無法裝備：物品 ID 為空",
            "Невозможно надеть: id предмета пуст");
        Log("item_event.wear_blocked_null_body",
            "Cannot equip '{0}': player body is null",
            "无法装备 '{0}'：玩家身体为空",
            "無法裝備 '{0}'：玩家身體為空",
            "Невозможно надеть '{0}': тело игрока равно null");
        Log("item_event.wear_blocked_no_sprite",
            "Cannot equip '{0}': {1}_worn.png not found, equip blocked to prevent crash",
            "无法装备 '{0}'：{1}_worn.png 不存在，已阻止装备防止崩溃",
            "無法裝備 '{0}'：{1}_worn.png 不存在，已阻止裝備防止崩潰",
            "Невозможно надеть '{0}': {1}_worn.png не найден, надевание заблокировано");
        Log("item_event.wear_slot_invalid",
            "Wear limb '{0}' for '{1}' is not a valid game limb. Valid: Head, UpTorso, DownTorso, UpArmF, DownArmF, HandF, UpArmB, DownArmB, HandB, ThighF, CrusF, FootF, ThighB, CrusB, FootB",
            "物品 '{1}' 的穿戴肢体 '{0}' 不是有效的游戏肢体。有效值：Head, UpTorso, DownTorso, UpArmF, DownArmF, HandF, UpArmB, DownArmB, HandB, ThighF, CrusF, FootF, ThighB, CrusB, FootB",
            "物品 '{1}' 的穿戴肢體 '{0}' 不是有效的遊戲肢體。有效值：Head, UpTorso, DownTorso, UpArmF, DownArmF, HandF, UpArmB, DownArmB, HandB, ThighF, CrusF, FootF, ThighB, CrusB, FootB",
            "Лимб '{0}' для '{1}' не является допустимым. Допустимые: Head, UpTorso, DownTorso, UpArmF, DownArmF, HandF, UpArmB, DownArmB, HandB, ThighF, CrusF, FootF, ThighB, CrusB, FootB");
        Log("item_event.wear_blocked_null_sprite",
            "Cannot equip '{0}': WornSprite is null on game ItemStats (check {1}_worn.png)",
            "无法装备 '{0}'：游戏内 WornSprite 为空（请检查 {1}_worn.png）",
            "無法裝備 '{0}'：遊戲內 WornSprite 為空（請檢查 {1}_worn.png）",
            "Невозможно надеть '{0}': WornSprite равен null в ItemStats (проверьте {1}_worn.png)");
        Log("item_event.wear_exception",
            "Equip '{0}' ({1}) crashed: {2}: {3}",
            "装备 '{0}' ({1}) 异常：{2}：{3}",
            "裝備 '{0}' ({1}) 異常：{2}：{3}",
            "Надевание '{0}' ({1}) вызвало ошибку: {2}: {3}");

        // Log - Item Loader
        Log("item_loader.wearable_no_worn_sprite",
            "Wearable '{0}' has no _worn texture ({1}) and no fallback item texture ({2}), equip blocked",
            "可穿戴物品 '{0}' 缺少 _worn 贴图 ({1}) 且无回退主贴图 ({2})，装备已阻止",
            "可穿戴物品 '{0}' 缺少 _worn 貼圖 ({1}) 且無回退主貼圖 ({2})，裝備已阻止",
            "Надеваемый '{0}': нет _worn ({1}) и нет запасной текстуры ({2}), надевание заблокировано");

        Log("gun_event.patch_ok",
            "Patched GunScript.{0}",
            "已补丁 GunScript.{0}",
            "已補丁 GunScript.{0}",
            "Запатчен GunScript.{0}");

        // Log - Gun Runtime
        Log("gun_runtime.gunscript_not_found",
            "GunScript type not found in game assemblies, gun runtime patches skipped",
            "未在游戏程序集中找到 GunScript 类型，已跳过枪械运行时补丁",
            "未在遊戲組件中找到 GunScript 類型，已跳過槍械運行時補丁",
            "Тип GunScript не найден в сборках игры, патчи пропущены");
        Log("gun_runtime.patches_applied",
            "Gun runtime patches applied",
            "枪械运行时补丁已应用",
            "槍械運行時補丁已應用",
            "Патчи GunRuntime применены");
        Log("gun_runtime.patch_failed",
            "Gun runtime patch failed: {0}",
            "枪械运行时补丁失败: {0}",
            "槍械運行時補丁失敗: {0}",
            "Ошибка патча GunRuntime: {0}");
        Log("gun_runtime.incompatible_ammo_type",
            "Ammo type '{0}' incompatible with gun ammo type '{1}'",
            "弹药类型 '{0}' 与枪械弹药类型 '{1}' 不兼容",
            "彈藥類型 '{0}' 與槍械彈藥類型 '{1}' 不相容",
            "Тип боеприпаса '{0}' несовместим с типом '{1}' оружия");
        Log("gun_runtime.gun_init_no_sprite",
            "gun_init_no_sprite itemId={0} — cannot find sprites, placeholder texture used. Check SpriteRenderer and Resources prefab.",
            "gun_init_no_sprite itemId={0} — 无法获取任何精灵，使用占位纹理。请检查预制体 SpriteRenderer 和 Resources 中是否存在对应枪械预制体。",
            "gun_init_no_sprite itemId={0} — 無法獲取任何精靈，使用佔位紋理。請檢查預製體 SpriteRenderer 和 Resources 中是否存在對應槍械預製體。",
            "gun_init_no_sprite itemId={0} — не удалось получить спрайты, используется заглушка. Проверьте SpriteRenderer и префаб.");
        Log("gun_runtime.gun_init_capacity_zero",
            "gun_init_capacity_zero itemId={0} feedType={1} — capacity not set, using fallback={2}. Add \"capacity\": XX in template JSON.",
            "gun_init_capacity_zero itemId={0} feedType={1} — 未在模板中设置 capacity，使用回退值 {2}。请在 JSON 中添加 \"capacity\": XX",
            "gun_init_capacity_zero itemId={0} feedType={1} — 未在模板中設定 capacity，使用回退值 {2}。請在 JSON 中添加 \"capacity\": XX",
            "gun_init_capacity_zero itemId={0} feedType={1} — вместимость не задана, резерв={2}. Добавьте \"capacity\": XX в JSON шаблона.");
        Log("gun_runtime.gun_init_no_mag_by_type",
            "gun_init_no_mag_by_type itemId={0} gun_mag_type={1} gun_ammo_type={2} — no registered mag matching mag_type, falling back to ammo_type lookup",
            "gun_init_no_mag_by_type itemId={0} gun_mag_type={1} gun_ammo_type={2} — 没有已注册弹匣的 mag_type 匹配此枪，将按 ammo_type 回退查找",
            "gun_init_no_mag_by_type itemId={0} gun_mag_type={1} gun_ammo_type={2} — 沒有已註冊彈匣的 mag_type 匹配此槍，將按 ammo_type 回退查找",
            "gun_init_no_mag_by_type itemId={0} gun_mag_type={1} gun_ammo_type={2} — нет магазина с mag_type, откат к поиску по ammo_type");
        Log("gun_runtime.gun_init_mag_type_mismatch",
            "gun_init_mag_type_mismatch itemId={0} gun_mag_type={1} mag_id={2} mag_mag_type={3} — mag_type mismatch! Add \"mag_type\": \"{4}\" to the magazine JSON.",
            "gun_init_mag_type_mismatch itemId={0} gun_mag_type={1} mag_id={2} mag_mag_type={3} — 弹匣 mag_type 与枪械不匹配，装弹会失败！请在弹匣 JSON 中设置 \"mag_type\": \"{4}\"",
            "gun_init_mag_type_mismatch itemId={0} gun_mag_type={1} mag_id={2} mag_mag_type={3} — 彈匣 mag_type 與槍械不匹配，裝彈會失敗！請在彈匣 JSON 中設置 \"mag_type\": \"{4}\"",
            "gun_init_mag_type_mismatch itemId={0} gun_mag_type={1} mag_id={2} mag_mag_type={3} — несовпадение mag_type! Добавьте \"mag_type\": \"{4}\" в JSON магазина.");
        Log("gun_runtime.handle_gun_menu_null_barrel",
            "handle_gun_menu_null_barrel — GunScript.barrel is null",
            "handle_gun_menu_null_barrel — GunScript.barrel 为 null",
            "handle_gun_menu_null_barrel — GunScript.barrel 為 null",
            "handle_gun_menu_null_barrel — GunScript.barrel равен null");
        Log("gun_runtime.handle_gun_menu_null_gunscript",
            "handle_gun_menu_null_gunscript — item has 'gun' tag but GetComponent<GunScript>() returns null",
            "handle_gun_menu_null_gunscript — 物品有 'gun' 标签但 GetComponent<GunScript>() 返回 null",
            "handle_gun_menu_null_gunscript — 物品有 'gun' 標籤但 GetComponent<GunScript>() 返回 null",
            "handle_gun_menu_null_gunscript — предмет с тегом 'gun' но GetComponent<GunScript>() возвращает null");
        Log("gun_runtime.handle_gun_menu_prefix_error",
            "handle_gun_menu_prefix_error: {0}: {1}",
            "handle_gun_menu_prefix_error: {0}: {1}",
            "handle_gun_menu_prefix_error: {0}: {1}",
            "handle_gun_menu_prefix_error: {0}: {1}");
        Log("gun_runtime.handle_gun_menu_null_pc_field",
            "handle_gun_menu_null_pc_field field={0}",
            "handle_gun_menu_null_pc_field 字段={0}",
            "handle_gun_menu_null_pc_field 欄位={0}",
            "handle_gun_menu_null_pc_field поле={0}");
        Log("gun_runtime.load_mag_incompatible_mag_type",
            "load_mag_incompatible_mag_type ammoId={0} ammo.mag_type={1} gun.mag_type={2}",
            "load_mag_incompatible_mag_type ammoId={0} ammo.mag_type={1} gun.mag_type={2}",
            "load_mag_incompatible_mag_type ammoId={0} ammo.mag_type={1} gun.mag_type={2}",
            "load_mag_incompatible_mag_type ammoId={0} ammo.mag_type={1} gun.mag_type={2}");
        Log("gun_runtime.load_mag_incompatible_ammo_type",
            "load_mag_incompatible_ammo_type ammoId={0} ammo.ammo_type={1} gun.ammo_type={2}",
            "load_mag_incompatible_ammo_type ammoId={0} ammo.ammo_type={1} gun.ammo_type={2}",
            "load_mag_incompatible_ammo_type ammoId={0} ammo.ammo_type={1} gun.ammo_type={2}",
            "load_mag_incompatible_ammo_type ammoId={0} ammo.ammo_type={1} gun.ammo_type={2}");
        // Log - Script Engine
        Log("script_engine.lua_load_failed",
            "Lua mod '{0}' failed to load: {1}",
            "Lua 模组 '{0}' 加载失败: {1}",
            "Lua 模組 '{0}' 載入失敗: {1}",
            "Lua-мод '{0}' не удалось загрузить: {1}");
        Log("script_engine.lua_exec_file_failed",
            "Lua mod '{0}' failed to execute '{1}': {2}",
            "Lua 模组 '{0}' 执行脚本 '{1}' 失败: {2}",
            "Lua 模組 '{0}' 執行腳本 '{1}' 失敗: {2}",
            "Lua-мод '{0}' не удалось выполнить '{1}': {2}");
        Log("script_engine.lua_dispose_error",
            "Lua engine dispose error '{0}': {1}",
            "Lua 引擎释放错误 '{0}': {1}",
            "Lua 引擎釋放錯誤 '{0}': {1}",
            "Ошибка освобождения Lua-движка '{0}': {1}");
        Log("script_engine.js_load_failed",
            "JS mod '{0}' failed to load: {1}",
            "JS 模组 '{0}' 加载失败: {1}",
            "JS 模組 '{0}' 載入失敗: {1}",
            "JS-мод '{0}' не удалось загрузить: {1}");
        Log("script_engine.js_exec_file_failed",
            "JS mod '{0}' failed to execute '{1}': {2}",
            "JS 模组 '{0}' 执行脚本 '{1}' 失败: {2}",
            "JS 模組 '{0}' 執行腳本 '{1}' 失敗: {2}",
            "JS-мод '{0}' не удалось выполнить '{1}': {2}");
        Log("script_engine.js_dispose_error",
            "JS engine dispose error '{0}': {1}",
            "JS 引擎释放错误 '{0}': {1}",
            "JS 引擎釋放錯誤 '{0}': {1}",
            "Ошибка освобождения JS-движка '{0}': {1}");
        Log("script_engine.py_load_failed",
            "Python mod '{0}' failed to load: {1}",
            "Python 模组 '{0}' 加载失败: {1}",
            "Python 模組 '{0}' 載入失敗: {1}",
            "Python-мод '{0}' не удалось загрузить: {1}");
        Log("script_engine.py_exec_file_failed",
            "Python mod '{0}' failed to execute '{1}': {2}",
            "Python 模组 '{0}' 执行脚本 '{1}' 失败: {2}",
            "Python 模組 '{0}' 執行腳本 '{1}' 失敗: {2}",
            "Python-мод '{0}' не удалось выполнить '{1}': {2}");
        Log("script_engine.py_dispose_error",
            "Python engine dispose error '{0}': {1}",
            "Python 引擎释放错误 '{0}': {1}",
            "Python 引擎釋放錯誤 '{0}': {1}",
            "Ошибка освобождения Python-движка '{0}': {1}");

        // Log - Items
        Log("items.load_error",
            "Error loading '{0}' in mod '{1}': {2}",
            "加载 '{0}' 错误 (模组 '{1}')：{2}",
            "載入 '{0}' 錯誤 (模組 '{1}')：{2}",
            "Ошибка загрузки '{0}' в моде '{1}': {2}");
        Log("items.read_failed",
            "Failed to read '{0}': {1}",
            "读取 '{0}' 失败：{1}",
            "讀取 '{0}' 失敗：{1}",
            "Не удалось прочитать '{0}': {1}");
        Log("items.invalid_json",
            "Invalid JSON in '{0}': {1}",
            "'{0}' 中的 JSON 无效：{1}",
            "'{0}' 中的 JSON 無效：{1}",
            "Неверный JSON в '{0}': {1}");
        Log("items.missing_id",
            "Missing 'id' in '{0}'",
            "'{0}' 缺少 id 字段",
            "'{0}' 缺少 id 欄位",
            "Отсутствует 'id' в '{0}'");
        Log("items.item_registered",
            "Item '{0}' registered (mod: {1})",
            "物品 '{0}' 已注册 (模组: {1})",
            "物品 '{0}' 已註冊 (模組: {1})",
            "Предмет '{0}' зарегистрирован (мод: {1})");
        Log("items.liquid_registered",
            "Liquid '{0}' registered (mod: {1})",
            "液体 '{0}' 已注册 (模组: {1})",
            "液體 '{0}' 已註冊 (模組: {1})",
            "Жидкость '{0}' зарегистрирована (мод: {1})");
        Log("items.format_migrated",
            "Item JSON '{0}' migrated to new grouped format (backup saved as .backup)",
            "物品 JSON '{0}' 已迁移为新分组格式（原文件备份为 .backup）",
            "物品 JSON '{0}' 已遷移為新分組格式（原檔案備份為 .backup）",
            "JSON предмета '{0}' перенесён в новый сгруппированный формат (резервная копия сохранена)");
        Log("items.format_migrate_failed",
            "Failed to migrate item JSON '{0}' to new format: {1}",
            "物品 JSON '{0}' 迁移为新格式失败：{1}",
            "物品 JSON '{0}' 遷移為新格式失敗：{1}",
            "Не удалось перенести JSON предмета '{0}' в новый формат: {1}");
        Log("items.wearable_disabled",
            "Wearable disabled for '{0}' because wear_slot_id is invalid or empty",
            "已禁用 '{0}' 的可穿戴属性，因为 wear_slot_id 无效或为空",
            "已禁用 '{0}' 的可穿戴屬性，因為 wear_slot_id 無效或為空",
            "Надевание для '{0}' отключено: wear_slot_id недействителен или пуст");
        Log("items.loaded_count",
            "Mod '{0}' loaded {1} custom item(s)",
            "模组 '{0}' 加载了 {1} 个自定义物品",
            "模組 '{0}' 載入了 {1} 個自定義物品",
            "Мод '{0}' загрузил {1} предмет(ов)");
        Log("items.scripts_pending",
            "Mod '{0}' has {1} item script(s) pending registration",
            "模组 '{0}' 有 {1} 个物品脚本待注册",
            "模組 '{0}' 有 {1} 個物品腳本待註冊",
            "Мод '{0}' имеет {1} скрипт(ов) ожидающих регистрации");
        Log("items.scripts_registered",
            "Mod '{0}' registered {1} item script(s)",
            "模组 '{0}' 注册了 {1} 个物品脚本",
            "模組 '{0}' 註冊了 {1} 個物品腳本",
            "Мод '{0}' зарегистрировал {1} скрипт(ов) предметов");

        // C# 端 JSON 物品加载相关日志
        Log("items.csharp.dir_missing",
            "C# item load skipped for mod '{0}': directory not found: {1}",
            "C# 物品加载跳过（模组 '{0}'）：目录不存在：{1}",
            "C# 物品載入跳過（模組 '{0}'）：目錄不存在：{1}",
            "C# загрузка предметов пропущена для мода '{0}': каталог не найден: {1}");
        Log("items.csharp.assembly_dir_missing",
            "C# item load skipped for mod '{0}': cannot resolve assembly directory from: {1}",
            "C# 物品加载跳过（模组 '{0}'）：无法从以下路径解析程序集目录：{1}",
            "C# 物品載入跳過（模組 '{0}'）：無法從以下路徑解析組件目錄：{1}",
            "C# загрузка предметов пропущена для мода '{0}': не удалось определить каталог сборки: {1}");
        Log("items.csharp.scripts_ignored",
            "Mod '{0}' has {1} item script(s) in JSON but C# mods have no script engine; script bindings ignored",
            "模组 '{0}' 的 JSON 含 {1} 个物品脚本，但 C# 模组无脚本引擎，脚本绑定已忽略",
            "模組 '{0}' 的 JSON 含 {1} 個物品腳本，但 C# 模組無腳本引擎，腳本綁定已忽略",
            "Мод '{0}' содержит {1} скрипт(ов) в JSON, но у C# модов нет движка скриптов; привязки проигнорированы");
        Log("items.csharp.manifest_missing",
            "C# item load skipped: mod.json not found at: {0}",
            "C# 物品加载跳过：未找到 mod.json：{0}",
            "C# 物品載入跳過：找不到 mod.json：{0}",
            "C# загрузка предметов пропущена: mod.json не найден: {0}");
        Log("items.csharp.manifest_parse_failed",
            "Failed to parse mod.json: {0}",
            "解析 mod.json 失败：{0}",
            "解析 mod.json 失敗：{0}",
            "Не удалось разобрать mod.json: {0}");
        Log("items.csharp.manifest_no_id",
            "mod.json is missing required 'id' field: {0}",
            "mod.json 缺少必填的 'id' 字段：{0}",
            "mod.json 缺少必填的 'id' 欄位：{0}",
            "В mod.json отсутствует обязательное поле 'id': {0}");
        Log("items.csharp.manifest_read_error",
            "Error reading mod.json '{0}': {1}",
            "读取 mod.json 错误 '{0}'：{1}",
            "讀取 mod.json 錯誤 '{0}'：{1}",
            "Ошибка чтения mod.json '{0}': {1}");

        // Log - Template
        Log("template.registered",
            "Template '{0}' registered",
            "模板 '{0}' 已注册",
            "模板 '{0}' 已註冊",
            "Шаблон '{0}' зарегистрирован");
        Log("template.missing_type",
            "Template is missing 'type' field",
            "模板缺少 'type' 字段",
            "模板缺少 'type' 欄位",
            "В шаблоне отсутствует поле 'type'");
        Log("template.not_registered",
            "Template '{0}' is not registered",
            "模板 '{0}' 未注册",
            "模板 '{0}' 未註冊",
            "Шаблон '{0}' не зарегистрирован");
        Log("template.merge_error",
            "Failed to merge template '{0}': {1}",
            "合并模板 '{0}' 失败：{1}",
            "合併模板 '{0}' 失敗：{1}",
            "Не удалось объединить шаблон '{0}': {1}");

        // Log - Mod Content (C# mod JSON loading)
        Log("mod_content.manifest_missing",
            "C# mod content load skipped: mod.json not found at: {0}",
            "C# 模组内容加载跳过：未找到 mod.json：{0}",
            "C# 模組內容載入跳過：找不到 mod.json：{0}",
            "C# загрузка контента мода пропущена: mod.json не найден: {0}");
        Log("mod_content.manifest_parse_failed",
            "Failed to parse mod.json: {0}",
            "解析 mod.json 失败：{0}",
            "解析 mod.json 失敗：{0}",
            "Не удалось разобрать mod.json: {0}");
        Log("mod_content.manifest_no_id",
            "mod.json is missing required 'id' field: {0}",
            "mod.json 缺少必填的 'id' 字段：{0}",
            "mod.json 缺少必填的 'id' 欄位：{0}",
            "В mod.json отсутствует обязательное поле 'id': {0}");
        Log("mod_content.manifest_read_error",
            "Error reading mod.json '{0}': {1}",
            "读取 mod.json 错误 '{0}'：{1}",
            "讀取 mod.json 錯誤 '{0}'：{1}",
            "Ошибка чтения mod.json '{0}': {1}");
        Log("mod_content.assembly_dir_missing",
            "C# mod content load skipped: cannot resolve assembly directory from: {0}",
            "C# 模组内容加载跳过：无法从以下路径解析程序集目录：{0}",
            "C# 模組內容載入跳過：無法從以下路徑解析組件目錄：{0}",
            "C# загрузка контента мода пропущена: не удалось определить каталог сборки: {0}");
        Log("mod_content.loaded",
            "Mod '{0}' loaded {1} item(s), {2} tile(s), {3} recipe(s), {4} moodle(s)",
            "模组 '{0}' 加载了 {1} 个物品、{2} 个物块、{3} 个配方、{4} 个状态",
            "模組 '{0}' 載入了 {1} 個物品、{2} 個物塊、{3} 個配方、{4} 個狀態",
            "Мод '{0}' загрузил {1} предм., {2} блоков, {3} рецептов, {4} настроений");
        Log("tiles.csharp.scripts_ignored",
            "Mod '{0}' has {1} tile script(s) in JSON but C# mods have no script engine; script bindings ignored",
            "模组 '{0}' 的 JSON 含 {1} 个物块脚本，但 C# 模组无脚本引擎，脚本绑定已忽略",
            "模組 '{0}' 的 JSON 含 {1} 個物塊腳本，但 C# 模組無腳本引擎，腳本綁定已忽略",
            "Мод '{0}' содержит {1} скрипт(ов) блоков в JSON, но у C# модов нет движка; привязки проигнорированы");
        Log("moodle.csharp.scripts_ignored",
            "Mod '{0}' has {1} moodle script(s) in JSON but C# mods have no script engine; script bindings ignored",
            "模组 '{0}' 的 JSON 含 {1} 个状态脚本，但 C# 模组无脚本引擎，脚本绑定已忽略",
            "模組 '{0}' 的 JSON 含 {1} 個狀態腳本，但 C# 模組無腳本引擎，腳本綁定已忽略",
            "Мод '{0}' содержит {1} скрипт(ов) настроений в JSON, но у C# модов нет движка; привязки проигнорированы");

        // Log - Recipe
        Log("recipe.load_error",
            "Error loading '{0}' in mod '{1}': {2}",
            "加载 '{0}' 错误 (模组 '{1}')：{2}",
            "載入 '{0}' 錯誤 (模組 '{1}')：{2}",
            "Ошибка загрузки '{0}' в моде '{1}': {2}");
        Log("recipe.parse_failed",
            "Failed to parse '{0}': {1}",
            "解析 '{0}' 失败：{1}",
            "解析 '{0}' 失敗：{1}",
            "Не удалось разобрать '{0}': {1}");
        Log("recipe.missing_id",
            "Recipe missing 'id' in '{0}'",
            "'{0}' 中配方缺少 id 字段",
            "'{0}' 中配方缺少 id 欄位",
            "Рецепт без 'id' в '{0}'");
        Log("recipe.registered",
            "Recipe for '{0}' registered",
            "配方 '{0}' 已注册",
            "配方 '{0}' 已註冊",
            "Рецепт '{0}' зарегистрирован");
        Log("recipe.replaced",
            "Replaced {1} existing recipe(s) for '{0}'",
            "已替换 {1} 个 '{0}' 的现有配方",
            "已替換 {1} 個 '{0}' 的現有配方",
            "Заменено {1} существующих рецептов для '{0}'");
        Log("recipe.loaded_count",
            "Mod '{0}' loaded {1} recipe(s)",
            "模组 '{0}' 加载了 {1} 个配方",
            "模組 '{0}' 載入了 {1} 個配方",
            "Мод '{0}' загрузил {1} рецепт(ов)");

        // Log - Moodle
        Log("moodle.load_error",
            "Error loading '{0}' in script '{1}': {2}",
            "加载 '{0}' 错误 (脚本 '{1}')：{2}",
            "載入 '{0}' 錯誤 (腳本 '{1}')：{2}",
            "Ошибка загрузки '{0}' в моде '{1}': {2}");
        Log("moodle.read_failed",
            "Failed to read '{0}': {1}",
            "读取 '{0}' 失败：{1}",
            "讀取 '{0}' 失敗：{1}",
            "Не удалось прочитать '{0}': {1}");
        Log("moodle.invalid_json",
            "Invalid JSON in '{0}': {1}",
            "'{0}' 中的 JSON 无效：{1}",
            "'{0}' 中的 JSON 無效：{1}",
            "Неверный JSON в '{0}': {1}");
        Log("moodle.missing_name",
            "Moodle missing 'name' in '{0}'",
            "'{0}' 中状态缺少 name 字段",
            "'{0}' 中狀態缺少 name 欄位",
            "Moodle без 'name' в '{0}'");
        Log("moodle.no_icon",
            "Moodle has no icon source in '{0}'",
            "'{0}' 中状态未指定图标来源",
            "'{0}' 中狀態未指定圖示來源",
            "Moodle без источника иконки в '{0}'");
        Log("moodle.sprite_load_failed",
            "Failed to load moodle sprite '{0}'. Moodle '{1}' may not display.",
            "加载状态精灵图 '{0}' 失败，状态 '{1}' 可能无法显示",
            "載入狀態精靈圖 '{0}' 失敗，狀態 '{1}' 可能無法顯示",
            "Не удалось загрузить спрайт '{0}'. Moodle '{1}' может не отображаться.");
        Log("moodle.registered",
            "Moodle '{0}' registered (script: {1})",
            "状态 '{0}' 已注册 (脚本: {1})",
            "狀態 '{0}' 已註冊 (腳本: {1})",
            "Moodle '{0}' зарегистрирован (мод: {1})");
        Log("moodle.loaded_count",
            "Script '{0}' loaded {1} moodle(s)",
            "脚本 '{0}' 加载了 {1} 个状态",
            "腳本 '{0}' 載入了 {1} 個狀態",
            "Мод '{0}' загрузил {1} moodle(ов)");

        // Log - Moodle Event
        Log("moodle_event.patched",
            "MoodleEventListener patched {0} AddMoodle/AddAnimatedMoodle method(s)",
            "MoodleEventListener 已补丁 {0} 个 AddMoodle/AddAnimatedMoodle 方法",
            "MoodleEventListener 已補丁 {0} 個 AddMoodle/AddAnimatedMoodle 方法",
            "MoodleEventListener запатчил {0} методов AddMoodle/AddAnimatedMoodle");

        // Log - Moodle Script
        Log("moodle.scripts_registered",
            "Script '{0}' registered {1} moodle script(s)",
            "脚本 '{0}' 注册了 {1} 个状态脚本",
            "腳本 '{0}' 註冊了 {1} 個狀態腳本",
            "Мод '{0}' зарегистрировал {1} скрипт(ов) moodle");

        // Log - Moodle Util
        Log("moodle.apply_not_found",
            "Moodle key '{0}' not found in loaded definitions, cannot apply",
            "未找到状态 key '{0}' 的定义，无法应用",
            "未找到狀態 key '{0}' 的定義，無法應用",
            "Moodle с ключом '{0}' не найден в определениях, невозможно применить");
        Log("moodle.apply_no_icon_source",
            "Moodle '{0}' has no valid icon source (icon_id / icon_asset / animated), cannot apply",
            "状态 '{0}' 没有有效的图标来源（icon_id / icon_asset / animated），无法应用",
            "狀態 '{0}' 沒有有效的圖標來源（icon_id / icon_asset / animated），無法應用",
            "Moodle '{0}' не имеет источника иконки (icon_id / icon_asset / animated), невозможно применить");

        // Log - Tile（索引由 Bark 自动分配，从 36 开始递增）
        Log("tiles.load_error",
            "Error loading tile '{0}' in mod '{1}': {2}",
            "加载物块 '{0}' 错误 (模组 '{1}')：{2}",
            "載入物塊 '{0}' 錯誤 (模組 '{1}')：{2}",
            "Ошибка загрузки тайла '{0}' в моде '{1}': {2}");
        Log("tiles.parse_failed",
            "Failed to parse tile JSON '{0}': {1}",
            "解析物块 JSON '{0}' 失败：{1}",
            "解析物塊 JSON '{0}' 失敗：{1}",
            "Не удалось разобрать JSON тайла '{0}': {1}");
        Log("tiles.index_too_low",
            "Tile '{0}' auto-assigned index {1} is below 36, check _nextTileIndex",
            "物块 '{0}' 自动分配的索引 {1} 低于 36，请检查 _nextTileIndex",
            "物塊 '{0}' 自動分配的索引 {1} 低於 36，請檢查 _nextTileIndex",
            "У тайла '{0}' автоматически назначенный индекс {1} меньше 36, проверьте _nextTileIndex");
        Log("tiles.sprite_not_found",
            "Tile sprite not found '{0}' for tile '{1}'",
            "未找到物块 '{1}' 的精灵图 '{0}'",
            "未找到物塊 '{1}' 的精靈圖 '{0}'",
            "Спрайт тайла не найден '{0}' для тайла '{1}'");
        Log("tiles.registered",
            "Tile '{0}' auto-assigned to index {1} (mod: {2})",
            "物块 '{0}' 已自动分配索引 {1} 并注册 (模组: {2})",
            "物塊 '{0}' 已自動分配索引 {1} 並註冊 (模組: {2})",
            "Тайл '{0}' автоматически назначен индексу {1} (мод: {2})");
        Log("tiles.loaded_count",
            "Mod '{0}' loaded {1} tile(s)",
            "模组 '{0}' 加载了 {1} 个物块",
            "模組 '{0}' 載入了 {1} 個物塊",
            "Мод '{0}' загрузил {1} тайл(ов)");
        Log("tiles.no_index",
            "Tile '{0}' in mod '{1}' was not assigned an index (Bark auto-assignment skipped)",
            "物块 '{0}' 未分配索引 (模组 '{1}')，Bark 自动分配已跳过",
            "物塊 '{0}' 未分配索引 (模組 '{1}')，Bark 自動分配已跳過",
            "Тайл '{0}' в моде '{1}' не получил индекс (автоназначение Bark пропущено)");
        Log("tiles.scripts_pending",
            "Mod '{0}' has {1} tile(s) with pending scripts",
            "模组 '{0}' 有 {1} 个物块包含待注册脚本",
            "模組 '{0}' 有 {1} 個物塊包含待註冊腳本",
            "Мод '{0}' имеет {1} тайл(ов) с ожидающими скриптами");
        Log("tiles.scripts_registered",
            "Mod '{0}' registered scripts for {1} tile(s)",
            "模组 '{0}' 为 {1} 个物块注册了脚本",
            "模組 '{0}' 為 {1} 個物塊註冊了腳本",
            "Мод '{0}' зарегистрировал скрипты для {1} тайл(ов)");

        // Log - Save
        Log("save.provider_registered",
            "Save provider '{0}' registered",
            "存档 Provider '{0}' 已注册",
            "存檔 Provider '{0}' 已註冊",
            "Save-провайдер '{0}' зарегистрирован");
        Log("save.provider_unregistered",
            "Save provider '{0}' unregistered",
            "存档 Provider '{0}' 已取消注册",
            "存檔 Provider '{0}' 已取消註冊",
            "Save-провайдер '{0}' разрегистрирован");
        Log("save.capture_error",
            "Save provider '{0}' failed to capture data: {1}",
            "存档 Provider '{0}' 捕获数据失败：{1}",
            "存檔 Provider '{0}' 擷取資料失敗：{1}",
            "Save-провайдеру '{0}' не удалось захватить данные: {1}");
        Log("save.restore_error",
            "Save provider '{0}' failed to restore data: {1}",
            "存档 Provider '{0}' 恢复数据失败：{1}",
            "存檔 Provider '{0}' 恢復資料失敗：{1}",
            "Save-провайдеру '{0}' не удалось восстановить данные: {1}");
        Log("save.key_empty",
            "Save provider key is empty or whitespace",
            "存档 Provider 的 key 为空或空白",
            "存檔 Provider 的 key 為空白",
            "Ключ save-провайдера пуст");
        Log("save.namespace_empty",
            "Save provider namespace is empty or whitespace",
            "存档 Provider 的命名空间为空或空白",
            "存檔 Provider 的命名空間為空白",
            "Пространство имён save-провайдера пусто");
        Log("save.provider_null",
            "Save provider is null",
            "存档 Provider 为 null",
            "存檔 Provider 為 null",
            "Save-провайдер равен null");

        // Log - Command
        Log("command.parse_failed",
            "Error loading command '{0}' in mod '{1}': {2}",
            "加载命令 '{0}' 错误 (模组 '{1}')：{2}",
            "載入命令 '{0}' 錯誤 (模組 '{1}')：{2}",
            "Ошибка загрузки команды '{0}' в моде '{1}': {2}");
        Log("command.name_has_spaces",
            "Command filename '{0}' has spaces in name '{1}' — use underscores instead",
            "'{0}' 文件名含空格，命令名 '{1}' 不能有空格 — 请用下划线",
            "'{0}' 檔名含空格，命令名 '{1}' 不能有空格 — 請用底線",
            "Имя файла команды '{0}' содержит пробелы в '{1}' — используйте подчёркивания");
        Log("command.pending_count",
            "Mod '{0}' has {1} command(s) pending registration",
            "模组 '{0}' 有 {1} 个命令待注册",
            "模組 '{0}' 有 {1} 個命令待註冊",
            "Мод '{0}' имеет {1} команд(ы) ожидающих регистрации");
        Log("command.scripts_registered",
            "Mod '{0}' registered {1} script command(s)",
            "模组 '{0}' 注册了 {1} 个脚本命令",
            "模組 '{0}' 註冊了 {1} 個腳本命令",
            "Мод '{0}' зарегистрировал {1} скриптовых команд(ы)");
        Log("command.script_error",
            "Command '{0}' in mod '{1}' script error: {2}",
            "命令 '{0}' (模组 '{1}') 脚本错误：{2}",
            "命令 '{0}' (模組 '{1}') 腳本錯誤：{2}",
            "Ошибка скрипта команды '{0}' в моде '{1}': {2}");

        // Log - Network Mod Sync
        Log("network_sync.ready",
            "Network mod sync registered (multiplayer bridge ready)",
            "网络模组同步已注册（多人游戏网桥就绪）",
            "網路模組同步已註冊（多人遊戲網橋就緒）",
            "Сетевая синхронизация модов зарегистрирована (мост готов)");
        Log("network_sync.requesting",
            "Requesting mod list from host...",
            "正在向主机请求模组列表...",
            "正在向主機請求模組列表...",
            "Запрос списка модов у хоста...");
        Log("network_sync.no_host_mods",
            "No script mods on host, sync skipped",
            "主机上没有脚本模组，跳过同步",
            "主機上沒有腳本模組，跳過同步",
            "На хосте нет скриптовых модов, синхронизация пропущена");
        Log("network_sync.skip_server_only",
            "Mod '{0}' is server-only, skipping download",
            "模组 '{0}' 为服务端专属，跳过下载",
            "模組 '{0}' 為伺服器專屬，跳過下載",
            "Мод '{0}' только для сервера, пропускаю загрузку");
        Log("network_sync.no_repo",
            "Mod '{0}' has no repository URL, cannot auto-download",
            "模组 '{0}' 未配置 GitHub 仓库地址，无法自动下载",
            "模組 '{0}' 未設定 GitHub 儲存庫地址，無法自動下載",
            "Мод '{0}' не имеет URL репозитория, невозможно скачать автоматически");
        Log("network_sync.already_match",
            "All {0} script mod(s) match the host, nothing to download",
            "全部 {0} 个脚本模组与主机一致，无需下载",
            "全部 {0} 個腳本模組與主機一致，無需下載",
            "Все {0} мод(ов) совпадают с хостом, загрузка не требуется");
        Log("network_sync.found_missing",
            "Found {0} missing script mod(s) (local: {1}), downloading...",
            "发现 {0} 个缺失的脚本模组 (本地: {1})，开始下载...",
            "發現 {0} 個缺失的腳本模組 (本地: {1})，開始下載...",
            "Найдено {0} отсутствующих мод(ов) (локально: {1}), загрузка...");
        Log("network_sync.downloading",
            "Downloading mod '{0}' v{1} from GitHub...",
            "正在从 GitHub 下载模组 '{0}' v{1}...",
            "正在從 GitHub 下載模組 '{0}' v{1}...",
            "Загрузка мода '{0}' v{1} с GitHub...");
        Log("network_sync.downloaded",
            "Downloaded '{0}', saved to ScriptMod/Mods/",
            "已下载 '{0}'，保存到 ScriptMod/Mods/",
            "已下載 '{0}'，儲存至 ScriptMod/Mods/",
            "Мод '{0}' загружен и сохранён в ScriptMod/Mods/");
        Log("network_sync.download_failed",
            "Failed to download mod '{0}': {1}",
            "模组 '{0}' 下载失败: {1}",
            "模組 '{0}' 下載失敗: {1}",
            "Не удалось загрузить мод '{0}': {1}");
        Log("network_sync.summary",
            "Mod sync complete: {0} downloaded, {1} failed",
            "模组同步完成: {0} 下载成功, {1} 失败",
            "模組同步完成: {0} 下載成功, {1} 失敗",
            "Синхронизация модов завершена: {0} загружено, {1} ошибок");
        Log("network_sync.reloading",
            "Reloading script mods after download...",
            "正在重载脚本模组...",
            "正在重載腳本模組...",
            "Перезагрузка скриптовых модов после загрузки...");
    }
}