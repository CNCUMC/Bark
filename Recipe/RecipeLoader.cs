using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bark.Script;
using Bark.Tool;
using CUCoreLib.Registries;
using UnityEngine;

namespace Bark.Recipe;

// 已加载配方的记录项
public class RecipeEntry(string id, string fileName)
{
    // 配方产物 ID
    public string Id = id;

    // 配方来源文件名（如 "bandage.json"）
    public string FileName = fileName;
}

// 自定义合成表加载器：扫描 ModDir/Recipe/*.json，
// 转为 CUCoreLib Recipe 对象并注册到 RecipeRegistry。
public static class RecipeLoader
{
    // 模组已加载的配方列表（modId → 配方记录）
    public static readonly Dictionary<string, List<RecipeEntry>> LoadedRecipes = new();

    // ClearOwnerEntries 是 internal，缓存 MethodInfo 供热重载时清除旧配方
    private static readonly MethodInfo? s_clearOwnerEntries = typeof(RecipeRegistry).GetMethod(
        "ClearOwnerEntries", BindingFlags.NonPublic | BindingFlags.Static);

    // RegisteredRecipes (internal static List<Recipe>)，用于替换原版配方时清除旧条目
    private static readonly FieldInfo? s_registeredRecipesField = typeof(RecipeRegistry).GetField(
        "RegisteredRecipes", BindingFlags.NonPublic | BindingFlags.Static);

    // RegisteredRecipeKeys (private static HashSet<string>)，替换原版配方时清除旧 key
    private static readonly FieldInfo? s_registeredRecipeKeysField = typeof(RecipeRegistry).GetField(
        "RegisteredRecipeKeys", BindingFlags.NonPublic | BindingFlags.Static);

    // BuildRecipeKey (internal static)，替换原版配方时生成旧配方的 key 以清除
    private static readonly MethodInfo? s_buildRecipeKeyMethod = typeof(RecipeRegistry).GetMethod(
        "BuildRecipeKey", BindingFlags.NonPublic | BindingFlags.Static);

    public static void RegisterFromMod(ScriptManifest manifest)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        // 重载时先清除该模组之前注册的配方
        ClearModRecipes(manifest.Id);

        var recipeDir = Path.Combine(manifest.Directory, "Recipe");
        if (!Directory.Exists(recipeDir))
            return;

        var jsonFiles = Directory.GetFiles(recipeDir, "*.json", SearchOption.TopDirectoryOnly);
        if (jsonFiles.Length == 0)
            return;

        var loadedList = new List<RecipeEntry>();
        var loadedCount = 0;

        // 标记配方所有权，以便热重载时清除
        using (RecipeRegistry.BeginOwnerRegistration(manifest.Id))
        {
            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    var entry = LoadAndRegister(jsonFile);
                    if (entry == null) continue;
                    loadedCount++;
                    loadedList.Add(entry);
                }
                catch (Exception ex)
                {
                    LogUtil.Error("recipe.load_error", jsonFile, manifest.Id, ex.Message);
                }
            }
        }

        LoadedRecipes[manifest.Id] = loadedList;

        if (loadedCount > 0)
            LogUtil.Message("recipe.loaded_count", manifest.Id, loadedCount);
    }

    // 加载并注册单个配方 JSON，成功时返回配方记录，失败返回 null
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

        // 构建材料列表
        var recipeItems = new List<RecipeItem>();
        recipeItems.AddRange(def.Items.Select(ing => new RecipeItem(ing.MinimumCondition)
        {
            specific = ing.Specific,
            specificId = ing.SpecificId,
            isLiquid = ing.IsLiquid,
            quality = string.IsNullOrEmpty(ing.Quality)
                ? null
                : new CraftingQuality(ing.Quality.ToLowerInvariant(), ing.QualityCondition),
            minimumCondition = ing.MinimumCondition,
            destroyItem = ing.DestroyItem,
            ignoredId = ing.IgnoredId,
        }));

        // 解析蓝图分类枚举
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
                dontDrainResultLiquid = def.DontDrainResultLiquid,
            },
            category = category,
            isRepair = def.IsRepair,
            items = recipeItems,
        };

        // 替换原版同名合成表
        if (def.ReplaceOriginalRecipe)
            RemoveRecipesByResultId(def.Id);

        RecipeRegistry.Register(recipe);
        LogUtil.Message("recipe.registered", def.Id);

        var fileName = Path.GetFileName(jsonFile);
        return new RecipeEntry(def.Id, fileName);
    }

    // 移除所有 result.id 匹配的配方（反射访问 RecipeRegistry 内部数据结构）
    private static void RemoveRecipesByResultId(string resultId)
    {
        if (s_registeredRecipesField?.GetValue(null) is not List<global::Recipe> registeredRecipes)
            return;

        // 找出 result.id 匹配的旧配方
        var toRemove = registeredRecipes
            .Where(r => string.Equals(r?.result?.id, resultId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (toRemove.Count == 0)
            return;

        // 从 RegisteredRecipes 中移除并清理关联 key
        registeredRecipes.RemoveAll(toRemove.Contains);

        if (s_registeredRecipeKeysField?.GetValue(null) is HashSet<string> keys)
        {
            foreach (var key in toRemove.Select(recipe => s_buildRecipeKeyMethod?.Invoke(null, [recipe])).OfType<string>())
            {
                keys.Remove(key);
            }
        }

        // 从 Recipes.recipes（原版注入列表）中移除
        RemoveFromRecipesList(toRemove);

        LogUtil.Info("recipe.replaced", resultId, toRemove.Count);
    }

    // 从 Recipes.recipes 中移除匹配的配方
    private static void RemoveFromRecipesList(List<global::Recipe> toRemove)
    {
        if (Recipes.recipes is null)
            return;

        var keysToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in toRemove.Select(recipe => s_buildRecipeKeyMethod?.Invoke(null, [recipe])).OfType<string>())
        {
            keysToRemove.Add(key);
        }

        if (keysToRemove.Count > 0)
        {
            ((List<global::Recipe>)Recipes.recipes).RemoveAll(r =>
            {
                var key = s_buildRecipeKeyMethod?.Invoke(null, [r]) as string;
                return key != null && keysToRemove.Contains(key);
            });
        }
    }

    // 清除指定模组之前注册的配方（内部调 RecipeRegistry.ClearOwnerEntries）
    private static void ClearModRecipes(string ownerId)
    {
        s_clearOwnerEntries?.Invoke(null, [ownerId, null!]);
        LoadedRecipes.Remove(ownerId);
    }
}