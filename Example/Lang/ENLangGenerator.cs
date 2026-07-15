using Bark.Base;

namespace Bark.Example.Lang;

public class ENLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "EN";
    protected override string NameSpace => "bark";

    protected override void BuildLocaleData()
    {
        // Options
        Option("game.test", "Test Option", "No practical use");

        // Command
        Command("catfcabl", "Create a txt file containing all Bark localizations");

        // Log - Console
        Log("console.null_or_empty", "Command cannot be null or empty");
        Log("console.not_initialized", "ConsoleScript is not initialized");

        // Log - World
        Log("world.place_block", "At {0} place block {1} failed: {2}");
        Log("world.place_item", "At {0} place item {1} failed: {2}");

        // Log - Player
        Log("player.body_null", "Player body object is null");
        Log("player.slot.out_of_range", "Inventory slot index out of range. Max slots: {0}");
        Log("player.load_item.fail", "Failed to load or instantiate item resource: '{0}'");
        Log("player.load_item.missing_component", "Resource '{0}' loaded but missing required Item component");

        // Log - Multiplayer
        Log("multiplayer.player_name.null_or_empty", "Player name cannot be null or empty");
        Log("multiplayer.teleport.success", "Teleported: '{0}' to {1}");
        Log("multiplayer.teleport.fail", "Teleport failed: {0}");

        // Log - Check
        Log("check.check_for_world", "No world loaded. Want to start a game?");
        Log("check.check_argument_count", "Expected at least {0} argument(s) {1}, but got {2}");
        Log("check.parse.float_invalid", "'{0}' is not a valid floating-point number! (2, 0.7, 14.1, etc.)");
        Log("check.parse.int_invalid", "'{0}' is not a valid integer!");
        Log("check.string.null_or_empty", "Input string cannot be null or empty");

        // Log - Update
        Log("update.no_repo", "No GitHub repository specified for {0}, skipping update check");
        Log("update.failed", "{0} failed to check for updates");
        Log("update.no_version", "{0} failed to read latest version number");
        Log("update.available", "{0} has a new version available! {1} -> {2}");
        Log("update.up_to_date", "{0} is up to date ({1})");

        // Log - TextUtil
        Log("text.font_not_found", "Font '{0}' not found");

        // Log - BetterLocale
        Log("better_locale.placeholder_out_of_range", "Placeholder {{{1}}} is out of range for key '{0}' (args.Length={2})");
    }
}