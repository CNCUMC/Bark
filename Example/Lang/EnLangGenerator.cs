using Bark.Base;

namespace Bark.Example.Lang;

public class EnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "EN";

    protected override void BuildLocaleData()
    {
        // Options
        Other("gameset.bark.game.test", "Test");
        Other("gameset.bark.game.testdsc", "test");

        // Command
        Other("command.test_hello.description", "Test Hello.");
        Other("command.test_hello.text", "Hello World! by Bark! {0}!");

        Other("command.spawn_block.description", "Spawns a block at the cursor position.");
        Other("command.spawn_block.block_id", "The ID of the block to spawn.");
        Other("command.spawn_block.invalid_block_id", "'{0}' is not a valid block ID.");
        Other("command.spawn_block.success", "Spawned block {0}.");

        Other("command.list_background.description", "Lists all available background IDs.");
        Other("command.list_background.header", "Found {0} background(s):");
        Other("command.list_background.none", "No background resources found.");

        Other("command.spawn_background.description", "Spawns a background at the cursor position.");
        Other("command.spawn_background.background_id", "The ID of the background to spawn.");
        Other("command.spawn_background.invalid_background_id", "Background ID cannot be empty.");
        Other("command.spawn_background.success", "Spawned background {0}.");

        // Log - Console
        Other("log.console.null_or_empty", "Command cannot be null or empty");
        Other("log.console.not_initialized", "ConsoleScript not initialized");

        // Log - World
        Other("log.world.check_for_world", "No world is loaded. Try starting a game?");
        Other("log.world.place_block", "Failed to spawn block {1} at {0}: {2}");
        Other("log.world.place_item", "Failed to spawn item {1} at {0}: {2}");
        Other("log.world.place_item.null_or_empty", "Item cannot be null or empty");
        Other("log.world.try_get_sprite", "Background sprite not found: {0}");

        // Log - Player
        Other("log.player.body_null", "Player body is null");
        Other("log.player.item.null_or_empty", "Item identifier cannot be null or whitespace");
        Other("log.player.slot.out_of_range", "Slot index out of range. Maximum slots: {0}");
        Other("log.player.load_item.fail", "Failed to load or instantiate item resource: '{0}'");
        Other("log.player.load_item.missing_component", "Resource '{0}' loaded but missing required Item component");

        // Log - Multiplayer
        Other("log.multiplayer.player_name.null_or_empty", "Player name cannot be null or empty");
        Other("log.multiplayer.teleport.success", "Teleported: {0} to {1}");
        Other("log.multiplayer.teleport.fail", "Failed to teleport: {0}");

        // Log - Config
        Other("log.config.get_config.not_exist_config", "Config '{0}' does not exist");
        Other("log.config.get_config.not_exist_key", "Key '{1}' does not exist in config '{0}'");
        Other("log.config.switch_type", "{0} has been set to {1}!");

        // Log - Utils
        Other("log.utils.check_argument_count", "Expected at least {0} argument {1}, but got {2}.");
        Other("log.utils.parse.float_invalid", "'{0}' is not a valid float value! (2, 0.7, 14.1, etc)");
        Other("log.utils.parse.int_invalid", "'{0}' is not a valid integer value!");
        Other("log.utils.string.null_or_empty", "Input string cannot be null or empty");

        // Log - Inventory
        Other("log.inventory.body_null", "Player body is null");
        Other("log.inventory.id.null_or_empty", "Item ID cannot be null or empty");
        Other("log.inventory.summary.header", "[Inventory]");
        Other("log.inventory.summary.hand_slot", "{0}* (Hand)");
        Other("log.inventory.summary.slot", "{0}");
        Other("log.inventory.summary.empty", "Empty");
        Other("log.inventory.summary.wearables", "[Wearables]");
        Other("log.inventory.empty", "(empty)");
    }
}