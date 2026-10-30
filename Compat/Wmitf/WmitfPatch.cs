using System;
using System.Collections.Generic;
using Bark.Script;
using Bark.Tool;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace Bark.Compat.Wmitf;

internal static class WmitfPatch
{
    public static bool IsLoaded => Chainloader.PluginInfos.ContainsKey("com.jimmyking.whatmodisthisfrom");
    private static readonly Dictionary<ushort, string> TileOwnerOverrides = new();

    public static void Apply(Harmony harmony)
    {
        if (!IsLoaded)
        {
            return;
        }

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

        if (!TileOwnerOverrides.TryGetValue(tileIndex, out var scriptModGuid)) return;
        modGuid = scriptModGuid;
    }

    public static void RegisterItem(string itemId, string modId)
    {
        if (!IsLoaded) return;
        WMITF.WMITF.RegisterItem(itemId, modId);
    }

    public static void RegisterLiquid(string liquidId, string modId)
    {
        if (!IsLoaded) return;
        WMITF.WMITF.RegisterLiquid(liquidId, modId);
    }

    public static void RegisterTile(ushort tileIndex, string modId)
    {
        TileOwnerOverrides[tileIndex] = modId;

        if (!IsLoaded) return;
        WMITF.WMITF.RegisterTile(tileIndex, modId);
    }

    public static void RegisterRecipe(string resultId, string modId)
    {
        if (!IsLoaded) return;
        WMITF.WMITF.RegisterRecipe(resultId, modId);
    }
}