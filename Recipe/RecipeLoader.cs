using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bark.Script;
using Bark.Tool;
using CUCoreLib.Registries;

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

        if (def is null || string.IsNullOrWhiteSpace(def.id))
        {
            LogUtil.Warning("recipe.missing_id", jsonFile);
            return null;
        }

        // 构建材料列表
        var recipeItems = new List<RecipeItem>();
        recipeItems.AddRange(def.items.Select(ing => new RecipeItem(ing.minimumCondition)
        {
            specific = ing.specific,
            specificId = ing.specificId,
            isLiquid = ing.isLiquid,
            quality = string.IsNullOrEmpty(ing.quality)
                ? null
                : new CraftingQuality(ing.quality.ToLowerInvariant(), ing.qualityCondition),
            minimumCondition = ing.minimumCondition,
            destroyItem = ing.destroyItem,
            ignoredId = ing.ignoredId,
        }));

        // 解析蓝图分类枚举
        if (!Enum.TryParse<Recipes.RecipeCategory>(def.category, true, out var category))
            category = Recipes.RecipeCategory.Materials;

        var recipe = new global::Recipe
        {
            INT = def.INT,
            result = new RecipeResult
            {
                id = def.id,
                amount = def.amount,
                isLiquid = def.isLiquid,
                resultCondition = def.resultCondition,
                dontDrainResultLiquid = def.dontDrainResultLiquid,
            },
            category = category,
            isRepair = def.isRepair,
            items = recipeItems,
        };

        RecipeRegistry.Register(recipe);
        LogUtil.Message("recipe.registered", def.id);

        var fileName = Path.GetFileName(jsonFile);
        return new RecipeEntry(def.id, fileName);
    }

    // 清除指定模组之前注册的配方（内部调 RecipeRegistry.ClearOwnerEntries）
    private static void ClearModRecipes(string ownerId)
    {
        s_clearOwnerEntries?.Invoke(null, [ownerId, null!]);
        LoadedRecipes.Remove(ownerId);
    }
}
