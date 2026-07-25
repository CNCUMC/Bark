***English*** | [简体中文](../zh-CN/recipe.md)

# Custom Recipes

Create a `Recipe/` folder inside your mod directory and add JSON files to register custom crafting recipes. Bark automatically scans and registers them with CUCoreLib's `RecipeRegistry` when the mod loads.

## Directory Structure

```
ScriptMod/Mods/
  MyMod/
    Recipe/
      bandage.json       ← one recipe per JSON file
      antidote.json
```

## JSON Format

A complete recipe JSON:

```json
{
  "id": "my_custom_item",
  "int": 5,
  "category": "Medicine",
  "amount": 3,
  "is_liquid": false,
  "result_condition": 1.0,
  "is_repair": false,
  "dont_drain_result_liquid": false,
  "items": [
    {
      "specific": false,
      "specific_id": "",
      "quality": "foliage",
      "quality_condition": 1.0,
      "minimum_condition": 0.5,
      "destroy_item": true,
      "is_liquid": false,
      "ignored_id": ""
    },
    {
      "specific": true,
      "specific_id": "bandage",
      "minimum_condition": 0.9,
      "destroy_item": true
    }
  ]
}
```

## Field Reference

### Recipe Result

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `id` | string | (required) | Result item ID |
| `int` | int | 0 | Intelligence required for crafting |
| `category` | string | `"Materials"` | Blueprint category: `Materials`, `Tools`, `Medicine`, `Utilities`, `Food` |
| `amount` | int | 1 | Items produced per craft |
| `is_liquid` | bool | false | Whether the result is a liquid |
| `result_condition` | float | 1.0 | Default condition (1.0 = 100%) |
| `is_repair` | bool | false | Whether this is a repair recipe |
| `dont_drain_result_liquid` | bool | false | Don't consume ingredient liquids |
| `replace_original_recipe` | bool | false | Replace vanilla recipe with the same ID |

### Ingredients (`items[i]`)

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `specific` | bool | false | `true` = match exact item ID, `false` = match by `quality` |
| `specific_id` | string | `""` | Target item ID when `specific=true` |
| `quality` | string | `""` | Crafting quality keyword — any item with this quality counts |
| `quality_condition` | float | 1.0 | Amount of quality to consume |
| `minimum_condition` | float | 0.9 | Minimum condition for the ingredient |
| `destroy_item` | bool | true | Whether the ingredient is consumed after crafting |
| `is_liquid` | bool | false | Whether this ingredient is a liquid |
| `ignored_id` | string | `""` | Specific item ID to exclude |

### Supported Crafting Qualities

The `quality` field accepts the same values as the vanilla crafting system:

`foliage` `cutting` `rippable` `dressing` `disinfectant` `water`
`blood` `nails` `fat` `opiate` `heatsource` `firestarter` `flammable`
`flour` `produce` `condiment` `hammering`

## C# API

`RecipeLoader` exposes the following public interface:

```csharp
// Get all loaded recipes: Dictionary<modId, List<RecipeEntry>>
var entries = RecipeLoader.LoadedRecipes;

foreach (var entry in entries["myMod"])
{
    Console.WriteLine($"{entry.Id} from {entry.FileName}");
}
```

`RecipeEntry` fields:
- `Id`: recipe result item ID
- `FileName`: source JSON file name (e.g. `"bandage.json"`)

## Hot Reload

After modifying JSON files in `Recipe/`, run `reload` in the console to reload all mod recipes. `RecipeLoader` clears the mod's old recipes before registering new ones, preventing duplicates.

## Integration with Items

The recipe `id` can be a vanilla item or a [custom item defined under `Assets/Item/`](items.md). Together they form a complete custom item ecosystem.
