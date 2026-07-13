using Bark.Base;

namespace Bark.Example.Lang;

public class EnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "EN";

    protected override void BuildLocaleData()
    {
        // Options
        Option("bark.game.test", "Test", "just test");

        // Log - Console
        Log("console.null_or_empty", "Command cannot be null or empty");
        Log("console.not_initialized", "ConsoleScript not initialized");

        // Log - World
        Log("world.check_for_world", "No world is loaded. Try starting a game?");
        Log("world.place_block", "Failed to spawn block {1} at {0}: {2}");
        Log("world.place_item", "Failed to spawn item {1} at {0}: {2}");
        Log("world.place_item.null_or_empty", "Item cannot be null or empty");
        Log("world.try_get_sprite", "Background sprite not found: {0}");

        // Log - Player
        Log("player.body_null", "Player body is null");
        Log("player.item.null_or_empty", "Item identifier cannot be null or whitespace");
        Log("player.slot.out_of_range", "Slot index out of range. Maximum slots: {0}");
        Log("player.load_item.fail", "Failed to load or instantiate item resource: '{0}'");
        Log("player.load_item.missing_component", "Resource '{0}' loaded but missing required Item component");

        // Log - Multiplayer
        Log("multiplayer.player_name.null_or_empty", "Player name cannot be null or empty");
        Log("multiplayer.teleport.success", "Teleported: {0} to {1}");
        Log("multiplayer.teleport.fail", "Failed to teleport: {0}");

        // Log - Config
        Log("config.get_config.not_exist_config", "Config '{0}' does not exist");
        Log("config.get_config.not_exist_key", "Key '{1}' does not exist in config '{0}'");
        Log("config.switch_type", "{0} has been set to {1}!");

        // Log - Utils
        Log("utils.check_argument_count", "Expected at least {0} argument {1}, but got {2}.");
        Log("utils.parse.float_invalid", "'{0}' is not a valid float value! (2, 0.7, 14.1, etc)");
        Log("utils.parse.int_invalid", "'{0}' is not a valid integer value!");
        Log("utils.string.null_or_empty", "Input string cannot be null or empty");

        // Log - Inventory
        Log("inventory.body_null", "Player body is null");
        Log("inventory.id.null_or_empty", "Item ID cannot be null or empty");
        Log("inventory.summary.header", "[Inventory]");
        Log("inventory.summary.hand_slot", "{0}* (Hand)");
        Log("inventory.summary.slot", "{0}");
        Log("inventory.summary.empty", "Empty");
        Log("inventory.summary.wearables", "[Wearables]");
        Log("inventory.empty", "(empty)");

        // Log - Update
        Log("update.no_repo", "GitHub repo not specified for {0}, skipping update check.");
        Log("update.failed", "{0} could not check for updates.");
        Log("update.no_version", "{0} could not read the latest release version.");
        Log("update.available", "{0} update available! {1} -> {2}");
        Log("update.uptodate", "{0} is up to date ({1}).");

        // Log - TextUtil
        Log("text.tmp_unifont_not_found", "TMP Unifont font not found (unifont-16.0)");
        Log("text.unifont_not_found", "Unifont font not found");
    }
}