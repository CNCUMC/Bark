using System;
using System.Collections.Generic;
using Bark.Audio;
using Bark.Items.Templates;
using Bark.Tool;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Items.Runtime.Gun;

public static partial class GunRuntimeManager
{
    private const int DefaultDirectCapacity = 6;
    private static Harmony? _harmony;
    private static readonly Dictionary<string, float> TinnitusMultiplierOverrides = new();
    private static float _preFireHearingLoss = -1f;

    private static AudioClip? _silentClip;

    private static AudioClip GetSilentClip()
    {
        if (_silentClip == null)
            _silentClip = AudioClip.Create("Bark_Silent", 1, 1, 44100, false);
        return _silentClip;
    }

    private static (Item? item, GunData? data) TryGetTemplateGun(Component? component)
    {
        if (component == null) return (null, null);
        var gunItem = component.GetComponent<Item>();
        if (gunItem == null || !GunTemplate.IsGun(gunItem.id)) return (null, null);
        var gunData = GunTemplate.GetGunData(gunItem.id);
        return (gunItem, gunData);
    }

    internal static float GetEffectiveTinnitusMultiplier(string gunItemId)
    {
        if (TinnitusMultiplierOverrides.TryGetValue(gunItemId, out var overrideValue))
            return overrideValue;
        var gunData = GunTemplate.GetGunData(gunItemId);
        return gunData?.TinnitusMultiplier ?? 0.1f;
    }

    internal static void SetTinnitusMultiplierOverride(string gunItemId,
        float? multiplier)
    {
        if (multiplier.HasValue)
            TinnitusMultiplierOverrides[gunItemId] = multiplier.Value;
        else
            TinnitusMultiplierOverrides.Remove(gunItemId);
    }

    // 安装所有 Harmony 补丁。幂等：多次调用不会重复 Patch。
    public static void Apply()
    {
        if (_harmony != null) return;

        _harmony = new Harmony("Bark.Runtime.GunRuntimeManager");
        var gunScriptType = AccessTools.TypeByName("GunScript");
        if (gunScriptType == null)
        {
            LogUtil.Warning("gun_runtime.gunscript_not_found");
            return;
        }

        try
        {
            var start = AccessTools.Method(gunScriptType, "Start");
            if (start != null)
                _harmony.Patch(start,
                    postfix: new HarmonyMethod(typeof(GunRuntimeManager),
                        nameof(OnGunStartPostfix)));

            var loadMag = AccessTools.Method(gunScriptType, "LoadMag");
            if (loadMag != null)
                _harmony.Patch(loadMag,
                    new HarmonyMethod(typeof(GunRuntimeManager),
                        nameof(OnLoadMagPrefix)));

            var unloadMag = AccessTools.Method(gunScriptType, "UnloadMag");
            if (unloadMag != null)
                _harmony.Patch(unloadMag,
                    new HarmonyMethod(typeof(GunRuntimeManager),
                        nameof(OnUnloadMagPrefix)));

            var fire = AccessTools.Method(gunScriptType, "Fire");
            if (fire != null)
            {
                _harmony.Patch(fire,
                    new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnFirePrefix)));
                _harmony.Patch(fire,
                    postfix: new HarmonyMethod(typeof(GunRuntimeManager),
                        nameof(OnFirePostfix)));
                _harmony.Patch(fire,
                    transpiler: new HarmonyMethod(typeof(GunRuntimeManager),
                        nameof(TranspileFire)));
            }

            var update = AccessTools.Method(gunScriptType, "Update");
            if (update != null)
                _harmony.Patch(update,
                    transpiler: new HarmonyMethod(typeof(GunRuntimeManager),
                        nameof(TranspileUpdate)));

            var toggleSafety = AccessTools.Method(gunScriptType, "ToggleSafety");
            if (toggleSafety != null)
                _harmony.Patch(toggleSafety,
                    new HarmonyMethod(typeof(GunRuntimeManager),
                        nameof(OnToggleSafetyPrefix)));

            var customInstantiateType = AccessTools.TypeByName("CustomInstantiate");
            if (customInstantiateType != null)
            {
                var getOrCreateTemplate =
                    AccessTools.Method(customInstantiateType, "GetOrCreateTemplate");
                if (getOrCreateTemplate != null)
                    _harmony.Patch(getOrCreateTemplate,
                        postfix: new HarmonyMethod(typeof(GunRuntimeManager),
                            nameof(OnTemplateCreatedPostfix)));
            }

            var itemRegistryPatchesType = AccessTools.TypeByName("ItemRegistryPatches");
            var applyLight = itemRegistryPatchesType != null
                ? AccessTools.Method(itemRegistryPatchesType, "ApplyLight")
                : null;
            if (applyLight != null)
                _harmony.Patch(applyLight,
                    postfix: new HarmonyMethod(typeof(GunRuntimeManager),
                        nameof(OnApplyLightPostfix)));

            var ammoScriptType = AccessTools.TypeByName("AmmoScript");
            if (ammoScriptType != null)
            {
                var unloadRound = AccessTools.Method(ammoScriptType, "UnloadRound");
                if (unloadRound != null)
                    _harmony.Patch(unloadRound,
                        new HarmonyMethod(typeof(GunRuntimeManager),
                            nameof(OnUnloadRoundPrefix)));

                var loadRound = AccessTools.Method(ammoScriptType, "LoadRound");
                if (loadRound != null)
                    _harmony.Patch(loadRound,
                        new HarmonyMethod(typeof(GunRuntimeManager),
                            nameof(OnLoadRoundPrefix)));
            }

            var playerCameraType = typeof(PlayerCamera);
            var handleGunMenu = AccessTools.Method(playerCameraType, "HandleGunMenu");
            if (handleGunMenu != null)
                _harmony.Patch(handleGunMenu,
                    new HarmonyMethod(typeof(GunRuntimeManager),
                        nameof(OnHandleGunMenuPrefix)));

            LogUtil.Info("gun_runtime.patches_applied");
        }
        catch (Exception ex)
        {
            LogUtil.Error("gun_runtime.patch_failed", ex.Message);
        }
    }

    public static void Unapply()
    {
        if (_harmony == null) return;
        _harmony.UnpatchSelf();
        _harmony = null;
    }

    private static void OnApplyLightPostfix(Item item)
    {
        if (item == null) return;
        if (!ItemLoader.LightRotations.TryGetValue(item.id, out var rotation)) return;

        var lightChild = item.transform.Find("Light");
        if (lightChild == null)
            lightChild = FindChildRecursive(item.transform, "Light");
        if (lightChild == null)
            lightChild = FindChildRecursive(item.transform, "CustomLight");
        if (lightChild == null) return;

        lightChild.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }

    private static Transform? FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindChildRecursive(child, name);
            if (found != null) return found;
        }

        return null;
    }

    public static void RefreshAllBarrelOffsets()
    {
        var guns = Object.FindObjectsOfType<GunScript>();
        foreach (var gun in guns)
        {
            var item = gun?.GetComponent<Item>();
            if (item is null) continue;

            var gunData = GunTemplate.GetGunData(item.id);
            if (gunData is null) continue;

            gun?.barrel?.localPosition =
                new Vector3(gunData.BarrelOffsetX, gunData.BarrelOffsetY, 0f);

            if (gunData.SoundProfile?.Fire is { Count: > 0 })
            {
                gun?.fireSound = GetSilentClip();
            }
            else if (!string.IsNullOrEmpty(gunData.FireSound))
            {
                var clip = AudioManager.LoadModAudio(gunData.ModDir, gunData.FireSound);
                if (clip != null) gun?.fireSound = clip;
            }

            if (gunData.SoundProfile?.Rack is { Count: > 0 })
                gun?.customRack =
                    gunData.SoundProfile.GetRandomClip(gunData.SoundProfile.Rack);
            else if (!string.IsNullOrEmpty(gunData.RackSound))
                gun?.customRack =
                    AudioManager.LoadModAudio(gunData.ModDir, gunData.RackSound);

            if (gunData.SoundProfile?.Unrack is { Count: > 0 })
                gun?.customUnrack =
                    gunData.SoundProfile.GetRandomClip(gunData.SoundProfile.Unrack);
            else if (!string.IsNullOrEmpty(gunData.UnrackSound))
                gun?.customUnrack =
                    AudioManager.LoadModAudio(gunData.ModDir, gunData.UnrackSound);
            else if (gun?.customRack != null) gun.customUnrack = gun.customRack;
        }
    }
}
