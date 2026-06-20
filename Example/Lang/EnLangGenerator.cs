using Bark.Core;

namespace Bark.Example.Lang;

public class EnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "EN";

    protected override void BuildLocaleData()
    {
        // Command
        Add("command.testhello.description", "Test Hello.");
        Add("command.testhello.text", "Hello World! by Bark! {0}!");

        Add("command.spawnblock.description", "Spawns a block at the cursor position.");
        Add("command.spawnblock.blockid", "The ID of the block to spawn.");
        Add("command.spawnblock.invalidblockid", "'{0}' is not a valid block ID.");
        Add("command.spawnblock.success", "Spawned block {0}.");

        Add("command.listbackground.description", "Lists all available background IDs.");
        Add("command.listbackground.header", "Found {0} background(s):");
        Add("command.listbackground.none", "No background resources found.");

        Add("command.spawnbackground.description", "Spawns a background at the cursor position.");
        Add("command.spawnbackground.backgroundid", "The ID of the background to spawn.");
        Add("command.spawnbackground.invalidbackgroundid", "Background ID cannot be empty.");
        Add("command.spawnbackground.success", "Spawned background {0}.");

        // Tool - Console
        Add("tool.console.nullorempty", "Command cannot be null or empty");
        Add("tool.console.notinitialized", "ConsoleScript not initialized");

        // Tool - World
        Add("tool.world.checkforworld", "No world is loaded. Try starting a game?");
        Add("tool.world.placeblock", "Failed to spawn block {1} at {0}: {2}");
        Add("tool.world.placeitem", "Failed to spawn item {1} at {0}: {2}");
        Add("tool.world.placeitem.nullorempty", "Item cannot be null or empty");
        Add("tool.world.trygetsprite", "Background sprite not found: {0}");

        // Tool - Player
        Add("tool.player.bodynull", "Player body is null");
        Add("tool.player.item.nullorempty", "Item identifier cannot be null or whitespace");
        Add("tool.player.slot.outofrange", "Slot index out of range. Maximum slots: {0}");
        Add("tool.player.loaditem.fail", "Failed to load or instantiate item resource: '{0}'");
        Add("tool.player.loaditem.missingcomponent", "Resource '{0}' loaded but missing required Item component");

        // Tool - Multiplayer
        Add("tool.multiplayer.playername.nullorempty", "Player name cannot be null or empty");
        Add("tool.multiplayer.teleport.success", "Teleported: {0} to {1}");
        Add("tool.multiplayer.teleport.fail", "Failed to teleport: {0}");

        // Tool - Config
        Add("tool.config.getconfig.notexistconfig", "Config '{0}' does not exist");
        Add("tool.config.getconfig.notexistkey", "Key '{1}' does not exist in config '{0}'");
        Add("tool.config.switchtype", "{0} has been set to {1}!");

        // Tool - Utils
        Add("tool.utils.checkargumentcount", "Expected at least {0} argument {1}, but got {2}.");
        Add("tool.utils.parse.float.invalid", "'{0}' is not a valid float value! (2, 0.7, 14.1, etc)");
        Add("tool.utils.parse.int.invalid", "'{0}' is not a valid integer value!");
        Add("tool.utils.string.nullorempty", "Input string cannot be null or empty");

        // Tool - Inventory
        Add("tool.inventory.bodynull", "Player body is null");
        Add("tool.inventory.id.nullorempty", "Item ID cannot be null or empty");
        Add("tool.inventory.summary.header", "[Inventory]");
        Add("tool.inventory.summary.handslot", "{0}* (Hand)");
        Add("tool.inventory.summary.slot", "{0}");
        Add("tool.inventory.summary.empty", "Empty");
        Add("tool.inventory.summary.wearables", "[Wearables]");
        Add("tool.inventory.empty", "(empty)");
    }
}
