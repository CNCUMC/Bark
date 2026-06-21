using Bark.Base;

namespace Bark.Example.Lang;

public class EnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "EN";

    protected override void BuildLocaleData()
    {
        // Command
        Add("command.test_hello.description", "Test Hello.");
        Add("command.test_hello.text", "Hello World! by Bark! {0}!");

        Add("command.spawn_block.description", "Spawns a block at the cursor position.");
        Add("command.spawn_block.block_id", "The ID of the block to spawn.");
        Add("command.spawn_block.invalid_block_id", "'{0}' is not a valid block ID.");
        Add("command.spawn_block.success", "Spawned block {0}.");

        Add("command.list_background.description", "Lists all available background IDs.");
        Add("command.list_background.header", "Found {0} background(s):");
        Add("command.list_background.none", "No background resources found.");

        Add("command.spawn_background.description", "Spawns a background at the cursor position.");
        Add("command.spawn_background.background_id", "The ID of the background to spawn.");
        Add("command.spawn_background.invalid_background_id", "Background ID cannot be empty.");
        Add("command.spawn_background.success", "Spawned background {0}.");

        // Log - Console
        Add("log.console.null_or_empty", "Command cannot be null or empty");
        Add("log.console.not_initialized", "ConsoleScript not initialized");

        // Log - World
        Add("log.world.check_for_world", "No world is loaded. Try starting a game?");
        Add("log.world.place_block", "Failed to spawn block {1} at {0}: {2}");
        Add("log.world.place_item", "Failed to spawn item {1} at {0}: {2}");
        Add("log.world.place_item.null_or_empty", "Item cannot be null or empty");
        Add("log.world.try_get_sprite", "Background sprite not found: {0}");

        // Log - Player
        Add("log.player.body_null", "Player body is null");
        Add("log.player.item.null_or_empty", "Item identifier cannot be null or whitespace");
        Add("log.player.slot.out_of_range", "Slot index out of range. Maximum slots: {0}");
        Add("log.player.load_item.fail", "Failed to load or instantiate item resource: '{0}'");
        Add("log.player.load_item.missing_component", "Resource '{0}' loaded but missing required Item component");

        // Log - Multiplayer
        Add("log.multiplayer.player_name.null_or_empty", "Player name cannot be null or empty");
        Add("log.multiplayer.teleport.success", "Teleported: {0} to {1}");
        Add("log.multiplayer.teleport.fail", "Failed to teleport: {0}");

        // Log - Config
        Add("log.config.get_config.not_exist_config", "Config '{0}' does not exist");
        Add("log.config.get_config.not_exist_key", "Key '{1}' does not exist in config '{0}'");
        Add("log.config.switch_type", "{0} has been set to {1}!");

        // Log - Utils
        Add("log.utils.check_argument_count", "Expected at least {0} argument {1}, but got {2}.");
        Add("log.utils.parse.float_invalid", "'{0}' is not a valid float value! (2, 0.7, 14.1, etc)");
        Add("log.utils.parse.int_invalid", "'{0}' is not a valid integer value!");
        Add("log.utils.string.null_or_empty", "Input string cannot be null or empty");

        // Log - Inventory
        Add("log.inventory.body_null", "Player body is null");
        Add("log.inventory.id.null_or_empty", "Item ID cannot be null or empty");
        Add("log.inventory.summary.header", "[Inventory]");
        Add("log.inventory.summary.hand_slot", "{0}* (Hand)");
        Add("log.inventory.summary.slot", "{0}");
        Add("log.inventory.summary.empty", "Empty");
        Add("log.inventory.summary.wearables", "[Wearables]");
        Add("log.inventory.empty", "(empty)");
    }
}