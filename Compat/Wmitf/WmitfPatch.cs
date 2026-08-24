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
            LogUtil.Info("wmitf.verify.not_loaded");
            return;
        }

        try
        {
            var patchesType = AccessTools.TypeByName("WhatModIsThisFrom.Patches");
            var getModName = patchesType is null
                ? null
                : AccessTools.Method(patchesType, "GetModName");
            if (getModName is not null)
            {
                var prefix =
                    new HarmonyMethod(AccessTools.DeclaredMethod(typeof(WmitfPatch), nameof(GetModNamePrefix)));
                harmony.Patch(getModName, prefix: prefix);
                LogUtil.Info("wmitf.patch.get_mod_name.applied");
            }
            else
            {
                LogUtil.Warning("wmitf.verify.method_not_found", "GetModName", patchesType?.FullName ?? "NULL");
            }
        }
        catch (Exception ex)
        {
            LogUtil.Error("wmitf.patch.get_mod_name.failed", ex.Message);
        }

        try
        {
            var wmitfType = AccessTools.TypeByName("WMITF.WMITF");
            var isOwnerLoaded = wmitfType is null
                ? null
                : AccessTools.Method(wmitfType, "IsOwnerLoaded");
            if (isOwnerLoaded is not null)
            {
                var prefix =
                    new HarmonyMethod(AccessTools.DeclaredMethod(typeof(WmitfPatch), nameof(IsOwnerLoadedPrefix)));
                harmony.Patch(isOwnerLoaded, prefix: prefix);
                LogUtil.Info("wmitf.patch.is_owner_loaded.applied");
            }
            else
            {
                LogUtil.Warning("wmitf.verify.method_not_found", "IsOwnerLoaded", wmitfType?.FullName ?? "NULL");
            }
        }
        catch (Exception ex)
        {
            LogUtil.Error("wmitf.patch.is_owner_loaded.failed", ex.Message);
        }

        try
        {
            var overlayType = AccessTools.TypeByName("WhatModIsThisFrom.InspectorOverlay");
            var patchesName = overlayType is null
                ? null
                : AccessTools.Method(overlayType, "PatchesName");
            if (patchesName is not null)
            {
                var prefix =
                    new HarmonyMethod(AccessTools.DeclaredMethod(typeof(WmitfPatch), nameof(PatchesNamePrefix)));
                harmony.Patch(patchesName, prefix: prefix);
                LogUtil.Info("wmitf.patch.patches_name.applied");
            }
            else
            {
                LogUtil.Warning("wmitf.verify.method_not_found", "PatchesName", overlayType?.FullName ?? "NULL");
            }
        }
        catch (Exception ex)
        {
            LogUtil.Error("wmitf.patch.patches_name.failed", ex.Message);
        }

        try
        {
            var tileRegistryType = AccessTools.TypeByName("CUCoreLib.Registries.TileRegistry");
            var tryGetOwner = tileRegistryType is null
                ? null
                : AccessTools.Method(tileRegistryType, "TryGetOwnerModGuid",
                    [typeof(ushort), typeof(string).MakeByRefType()]);
            if (tryGetOwner is not null)
            {
                var postfix = new HarmonyMethod(AccessTools.DeclaredMethod(typeof(WmitfPatch),
                    nameof(TileRegistryTryGetOwnerPostfix)));
                harmony.Patch(tryGetOwner, postfix: postfix);
                LogUtil.Info("wmitf.patch.tile_owner_override.applied");
            }
            else
            {
                LogUtil.Warning("wmitf.verify.method_not_found", "TileRegistry.TryGetOwnerModGuid(ushort)",
                    tileRegistryType?.FullName ?? "NULL");
            }
        }
        catch (Exception ex)
        {
            LogUtil.Error("wmitf.patch.tile_owner_override.failed", ex.Message);
        }
    }

    private static bool GetModNamePrefix(string guid, ref string __result)
    {
        if (string.IsNullOrWhiteSpace(guid))
            return true;

        if (!ScriptModLoader.LoadedScriptMods.TryGetValue(guid, out var manifest))
        {
            return true;
        }

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

    private static bool PatchesNamePrefix(string guid, ref string __result)
    {
        if (string.IsNullOrWhiteSpace(guid))
            return true;

        if (!ScriptModLoader.LoadedScriptMods.TryGetValue(guid, out var manifest))
            return true;

        __result = manifest.Name;
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