using Bark.Core;

namespace Bark.Example.Lang;

public class EnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "EN";

    protected override void BuildLocaleData()
    {
        // Command
        Add("command_testhello_description", "Test Hello.");
        Add("command_testhello_text", "Hello World! by Bark! {0}!");

        Add("command_spawnblock_description", "Spawns a block at the cursor position.");
        Add("command_spawnblock_blockid", "The ID of the block to spawn.");
        Add("command_spawnblock_invalidblockid", "'{0}' is not a valid block ID.");
        Add("command_spawnblock_success", "Spawned block {0}.");

        Add("command_listbackground_description", "Lists all available background IDs.");
        Add("command_listbackground_header", "Found {0} background(s):");
        Add("command_listbackground_none", "No background resources found.");

        Add("command_spawnbackground_description", "Spawns a background at the cursor position.");
        Add("command_spawnbackground_backgroundid", "The ID of the background to spawn.");
        Add("command_spawnbackground_invalidbackgroundid", "Background ID cannot be empty.");
        Add("command_spawnbackground_success", "Spawned background {0}.");

        // Tool - Console
        Add("tool_console_nullorempty", "Command cannot be null or empty");
        Add("tool_console_notinitialized", "ConsoleScript not initialized");

        // Tool - World
        Add("tool_world_checkforworld", "No world is loaded. Try starting a game?");
        Add("tool_world_placeblock", "Failed to spawn block {1} at {0}: {2}");
        Add("tool_world_placeitem", "Failed to spawn item {1} at {0}: {2}");
        Add("tool_world_placeitem_nullorempty", "Item cannot be null or empty");
        Add("tool_world_trygetsprite", "Background sprite not found: {0}");

        // Tool - Player
        Add("tool_player_bodynull", "Player body is null");
        Add("tool_player_item_nullorempty", "Item identifier cannot be null or whitespace");
        Add("tool_player_slot_outofrange", "Slot index out of range. Maximum slots: {0}");
        Add("tool_player_loaditem_fail", "Failed to load or instantiate item resource: '{0}'");
        Add("tool_player_loaditem_missingcomponent", "Resource '{0}' loaded but missing required Item component");

        // Tool - Multiplayer
        Add("tool_multiplayer_playername_nullorempty", "Player name cannot be null or empty");
        Add("tool_multiplayer_teleport_success", "Teleported: {0} to {1}");
        Add("tool_multiplayer_teleport_fail", "Failed to teleport: {0}");

        // Tool - Config
        Add("tool_config_getconfig_notexistconfig", "Config '{0}' does not exist");
        Add("tool_config_getconfig_notexistkey", "Key '{1}' does not exist in config '{0}'");
        Add("tool_config_switchtype", "{0} has been set to {1}!");

        // Tool - Utils
        Add("tool_utils_checkargumentcount", "Expected at least {0} argument {1}, but got {2}.");
        Add("tool_utils_parse_float_invalid", "'{0}' is not a valid float value! (2, 0.7, 14.1, etc)");
        Add("tool_utils_parse_int_invalid", "'{0}' is not a valid integer value!");
        Add("tool_utils_string_nullorempty", "Input string cannot be null or empty");

        // Tool - Inventory
        Add("tool_inventory_bodynull", "Player body is null");
        Add("tool_inventory_id_nullorempty", "Item ID cannot be null or empty");
        Add("tool_inventory_summary_header", "[Inventory]");
        Add("tool_inventory_summary_handslot", "{0}* (Hand)");
        Add("tool_inventory_summary_slot", "{0}");
        Add("tool_inventory_summary_empty", "Empty");
        Add("tool_inventory_summary_wearables", "[Wearables]");
        Add("tool_inventory_empty", "(empty)");
    }
}
