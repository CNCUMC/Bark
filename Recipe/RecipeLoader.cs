using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bark.Compat.Wmitf;
using Bark.Script;
using Bark.Tool;
using CUCoreLib.Registries;

namespace Bark.Recipe;

public class RecipeEntry(string id, string fileName)
{
    public string FileName = fileName;
    public string Id = id;
}

public static class RecipeLoader
{
    public static readonly Dictionary<string, List<RecipeEntry>> LoadedRecipes = new();

    private static readonly MethodInfo? s_clearOwnerEntries =
        typeof(RecipeRegistry).GetMethod(
            "ClearOwnerEntries", BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly FieldInfo? s_registeredRecipesField =
        typeof(RecipeRegistry).GetField(
            "RegisteredRecipes", BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly FieldInfo? s_registeredRecipeKeysField =
        typeof(RecipeRegistry).GetField(
            "RegisteredRecipeKeys", BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly MethodInfo? s_buildRecipeKeyMethod =
        typeof(RecipeRegistry).GetMethod(
            "BuildRecipeKey", BindingFlags.NonPublic | BindingFlags.Static);

    public static void RegisterFromMod(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        RegisterFromDirectory(manifest.Id, manifest.Directory);
    }

    public static void RegisterFromDirectory(string modId, string modDir)
    {
        if (modId is null)
            throw new ArgumentNullException(nameof(modId));
        if (modDir is null)
            throw new ArgumentNullException(nameof(modDir));

        ClearModRecipes(modId);

        var recipeDir = Path.Combine(modDir, "Recipe");
        if (!Directory.Exists(recipeDir))
            return;

        var jsonFiles =
            Directory.GetFiles(recipeDir, "*.json", SearchOption.AllDirectories);
        if (jsonFiles.Length == 0)
            return;

        var loadedList = new List<RecipeEntry>();
        var loadedCount = 0;

        using (RecipeRegistry.BeginOwnerRegistration(modId))
        {
            foreach (var jsonFile in jsonFiles)
                try
                {
                    var entry = LoadAndRegister(jsonFile);
                    if (entry == null) continue;
                    loadedCount++;
                    loadedList.Add(entry);
                }
                catch (Exception ex)
                {
                    LogUtil.Error("recipe.load_error", jsonFile, modId, ex.Message);
                }
        }

        LoadedRecipes[modId] = loadedList;

        foreach (var entry in loadedList)
            WmitfPatch.RegisterRecipe(entry.Id, modId);

        if (loadedCount > 0)
            LogUtil.Message("recipe.loaded_count", modId, loadedCount);
    }

    public static void UnregisterOwner(string modId)
    {
        if (modId is null)
            throw new ArgumentNullException(nameof(modId));
        ClearModRecipes(modId);
    }

    private static RecipeEntry? LoadAndRegister(string jsonFile)
    {
        RecipeDef? def;
        try
        {
            def = JsonUtil.ReadFile<RecipeDef>(jsonFile);
        }
        catch (Exception ex)
        {
            LogUtil.Warning("recipe.parse_failed", jsonFile, ex.Message);
            return null;
        }

        if (def is null || string.IsNullOrWhiteSpace(def.Id))
        {
            LogUtil.Warning("recipe.missing_id", jsonFile);
            return null;
        }

        var recipeItems = new List<RecipeItem>();
        recipeItems.AddRange(def.Items.Select(ing => new RecipeItem(ing.MinimumCondition)
        {
            specific = ing.Specific,
            specificId = ing.SpecificId,
            isLiquid = ing.IsLiquid,
            quality = string.IsNullOrEmpty(ing.Quality)
                ? null
                : new CraftingQuality(ing.Quality.ToLowerInvariant(),
                    ing.QualityCondition),
            minimumCondition = ing.MinimumCondition,
            destroyItem = ing.DestroyItem,
            ignoredId = ing.IgnoredId
        }));

        if (!Enum.TryParse<Recipes.RecipeCategory>(def.Category, true, out var category))
            category = Recipes.RecipeCategory.Materials;

        var recipe = new global::Recipe
        {
            INT = def.INT,
            result = new RecipeResult
            {
                id = def.Id,
                amount = def.Amount,
                isLiquid = def.IsLiquid,
                resultCondition = def.ResultCondition,
                dontDrainResultLiquid = def.DontDrainResultLiquid
            },
            category = category,
            isRepair = def.IsRepair,
            items = recipeItems
        };

        if (def.ReplaceOriginalRecipe)
            RemoveRecipesByResultId(def.Id);

        RecipeRegistry.Register(recipe);
        LogUtil.Message("recipe.registered", def.Id);

        var fileName = Path.GetFileName(jsonFile);
        return new RecipeEntry(def.Id, fileName);
    }

    private static void RemoveRecipesByResultId(string resultId)
    {
        if (s_registeredRecipesField?.GetValue(null) is not List<global::Recipe>
            registeredRecipes)
            return;

        var toRemove = registeredRecipes
            .Where(r =>
                string.Equals(r?.result?.id, resultId,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (toRemove.Count == 0)
            return;

        registeredRecipes.RemoveAll(toRemove.Contains);

        if (s_registeredRecipeKeysField?.GetValue(null) is HashSet<string> keys)
            foreach (var key in toRemove.Select(recipe =>
                    s_buildRecipeKeyMethod?.Invoke(null, [recipe]))
                .OfType<string>())
                keys.Remove(key);

        RemoveFromRecipesList(toRemove);

        LogUtil.Info("recipe.replaced", resultId, toRemove.Count);
    }

    private static void RemoveFromRecipesList(List<global::Recipe> toRemove)
    {
        if (Recipes.recipes is null)
            return;

        var keysToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in toRemove
            .Select(recipe => s_buildRecipeKeyMethod?.Invoke(null, [recipe]))
            .OfType<string>())
            keysToRemove.Add(key);

        if (keysToRemove.Count > 0)
            ((List<global::Recipe>)Recipes.recipes).RemoveAll(r =>
            {
                var key = s_buildRecipeKeyMethod?.Invoke(null, [r]) as string;
                return key != null && keysToRemove.Contains(key);
            });
    }

    private static void ClearModRecipes(string ownerId)
    {
        s_clearOwnerEntries?.Invoke(null, [ownerId, null!]);
        LoadedRecipes.Remove(ownerId);
    }
}
