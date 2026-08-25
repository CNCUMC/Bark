using System;
using System.Collections.Generic;
using Bark.Script;
using Bark.Tool;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace Bark.Compat.Wmitf;

internal static class WmitfPatch
{
    // WMITF 是否已加载
    public static bool IsLoaded => Chainloader.PluginInfos.ContainsKey("com.jimmyking.whatmodisthisfrom");

    // 物块 tileIndex → 脚本模组 ID 的覆盖映射（用于修正 CCL TileRegistry.TryGetOwnerModGuid 返回 Bark GUID 的问题）
    private static readonly Dictionary<ushort, string> TileOwnerOverrides = new();

    public static void Apply(Harmony harmony)
    {
        if (!IsLoaded)
        {
            return;
        }

        // (目标程序集类型名, 方法名, 前缀方法名, 后缀方法名, 参数类型)
        var patches = new (string TypeName, string Method, string? Prefix, string? Postfix, Type[]? ParamTypes)[]
        {
            ("WhatModIsThisFrom.Patches", "GetModName",
                nameof(ScriptModNamePrefix), null, null),
            ("WMITF.WMITF", "IsOwnerLoaded",
                nameof(IsOwnerLoadedPrefix), null, null),
            ("WhatModIsThisFrom.InspectorOverlay", "PatchesName",
                nameof(ScriptModNamePrefix), null, null),
            ("CUCoreLib.Registries.TileRegistry", "TryGetOwnerModGuid",
                null, nameof(TileRegistryTryGetOwnerPostfix), [typeof(ushort), typeof(string).MakeByRefType()]),
        };

        foreach (var (typeName, method, prefix, postfix, paramTypes) in patches)
        {
            TryPatch(harmony, typeName, method, prefix, postfix, paramTypes);
        }
    }

    // 统一的补丁应用辅助方法
    private static void TryPatch(Harmony harmony, string typeName, string method,
        string? prefix, string? postfix, Type[]? paramTypes)
    {
        try
        {
            var type = AccessTools.TypeByName(typeName);
            if (type is null)
            {
                LogUtil.Warning("wmitf.patch.not_found", method, typeName);
                return;
            }

            var target = AccessTools.Method(type, method, paramTypes);
            if (target is null)
            {
                if (type.FullName != null) LogUtil.Warning("wmitf.patch.not_found", method, type.FullName);
                return;
            }

            var pre = prefix is not null
                ? new HarmonyMethod(AccessTools.DeclaredMethod(typeof(WmitfPatch), prefix))
                : null;
            var post = postfix is not null
                ? new HarmonyMethod(AccessTools.DeclaredMethod(typeof(WmitfPatch), postfix))
                : null;

            harmony.Patch(target, prefix: pre, postfix: post);
            LogUtil.Info("wmitf.patch.applied", method);
        }
        catch (Exception ex)
        {
            LogUtil.Error("wmitf.patch.failed", method, ex);
        }
    }

    // GetModName / PatchesName 共用前缀：按 GUID 查脚本模组显示名
    private static bool ScriptModNamePrefix(string guid, ref string __result)
    {
        if (string.IsNullOrWhiteSpace(guid))
            return true;

        if (!ScriptModLoader.LoadedScriptMods.TryGetValue(guid, out var manifest))
            return true;

        __result = manifest.Name;
        return false;
    }

    private static bool IsOwnerLoadedPrefix(string ownerModGuid, ref bool __result)
    {
        if (Chainloader.PluginInfos.ContainsKey(ownerModGuid))
            return true;

        if (string.IsNullOrWhiteSpace(ownerModGuid)
            || !ScriptModLoader.LoadedScriptMods.ContainsKey(ownerModGuid))
            return true;

        __result = true;
        return false;
    }

    private static void TileRegistryTryGetOwnerPostfix(
        ushort tileIndex,
        ref string modGuid,
        ref bool __result)
    {
        if (!__result || string.IsNullOrEmpty(modGuid) || modGuid != Plugin.Guid)
            return;

        // CCL 返回了 Bark GUID，检查 TileOwnerOverrides 是否有覆盖
        if (!TileOwnerOverrides.TryGetValue(tileIndex, out var scriptModGuid)) return;
        modGuid = scriptModGuid;
    }

    // 注册物品到 WMITF（供 ItemLoader 调用）
    public static void RegisterItem(string itemId, string modId)
    {
        if (!IsLoaded) return;
        WMITF.WMITF.RegisterItem(itemId, modId);
    }

    // 注册液体到 WMITF（供 ItemLoader 调用）
    public static void RegisterLiquid(string liquidId, string modId)
    {
        if (!IsLoaded) return;
        WMITF.WMITF.RegisterLiquid(liquidId, modId);
    }

    // 注册物块到 WMITF（供 TileLoader 调用）
    public static void RegisterTile(ushort tileIndex, string modId)
    {
        // 记录覆盖映射（用于修正 CCL TileRegistry 返回 Bark GUID 的问题）
        TileOwnerOverrides[tileIndex] = modId;

        if (!IsLoaded) return;
        WMITF.WMITF.RegisterTile(tileIndex, modId);
    }

    // 注册配方到 WMITF（供 RecipeLoader 调用）
    public static void RegisterRecipe(string resultId, string modId)
    {
        if (!IsLoaded) return;
        WMITF.WMITF.RegisterRecipe(resultId, modId);
    }
}