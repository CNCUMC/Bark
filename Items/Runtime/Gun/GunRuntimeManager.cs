using System;
using System.Collections.Generic;
using Bark.Audio;
using Bark.Items.Templates;
using Bark.Tool;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Items.Runtime.Gun;

// 枪械运行时管理器：通过 Harmony 补丁覆盖 GunScript 的原生装弹/卸弹/开火逻辑，
// 用模板标签匹配替换硬编码的弹药类型枚举，打通 Gun → Mag → Ammo → Casing 四层模板链路。
//
// 补丁策略：
// - LoadMag：Prefix 拦截模板枪械，验证 ammo_type/mag_type 兼容性 + 容量限制，跟踪 GunMagTracker 状态。
// - UnloadMag：Prefix 拦截模板枪械，读取 GunMagTracker 状态生成模板弹匣物品，清除追踪。
// - Update：Transpiler 替换抛壳时的 ldstr "casing" → ResolveCasingItemId(This) 调用。
// - Fire：Postfix 消耗 GunMagTracker 余弹，记录 PendingCasingType，应用模板损耗。
//
// 与 GunEventListener 的共存：
// GunRuntimeManager 的 Prefix 在模板枪械上返回 false → 跳过原方法和 GunEventListener 的 Postfix。
// GunRuntimeManager 在跳过原生后手动触发对应事件，确保脚本侧仍能收到事件。
public static partial class GunRuntimeManager
{
    // 直装枪械默认管容量（霰弹枪等无弹匣枪械）
    private const int DefaultDirectCapacity = 6;

    // 所有 patch 共用同一个 Harmony 实例，以便一次性 Unpatch。
    private static Harmony? _harmony;

    // 耳鸣倍率运行时覆盖表：脚本端可通过 GunRuntimeApi.SetTinnitusMultiplier 设置，
    // 键为枪械物品 ID，值为覆盖的 tinnitus_multiplier（null 表示使用模板默认值）。
    private static readonly Dictionary<string, float> TinnitusMultiplierOverrides = new();

    // Fire Prefix → Postfix 间传递的 preFire hearingLoss。
    // Unity 主线程顺序执行，无需线程安全。
    private static float _preFireHearingLoss = -1f;

    // 静音 AudioClip：当 SoundProfile 接管开火音效时，将 gun.fireSound 设为此值，
    // 确保 GunScript.Fire() 内的 Sound.Play(fireSound, ...) 不会崩溃（Sound.Play 需要有效 clip）。
    // OnFirePostfix 中再播放 profile 的真实音效。
    private static AudioClip? _silentClip;
    private static AudioClip GetSilentClip()
    {
        if (_silentClip == null)
            _silentClip = AudioClip.Create("Bark_Silent", 1, 1, 44100, false);
        return _silentClip;
    }

    // 提取重复的模板枪械守卫链：gunItem/gunData 双重 null 检查。
    // 用于 LoadMag/UnloadMag/Fire/Safety/Sound 等 Prefix/Postfix/Transpiler 回调。
    // 返回 null 表示非模板枪械或无有效 GunData，调用方应返回 true（运行原逻辑）。
    private static (Item? item, GunData? data) TryGetTemplateGun(Component? component)
    {
        if (component == null) return (null, null);
        var gunItem = component.GetComponent<Item>();
        if (gunItem == null || !GunTemplate.IsGun(gunItem.id)) return (null, null);
        var gunData = GunTemplate.GetGunData(gunItem.id);
        return (gunItem, gunData);
    }

    // ============================================================
    // 生命周期
    // ============================================================

    // 获取指定枪械的耳鸣倍率（运行时覆盖优先，否则取模板默认值）
    internal static float GetEffectiveTinnitusMultiplier(string gunItemId)
    {
        if (TinnitusMultiplierOverrides.TryGetValue(gunItemId, out var overrideValue))
            return overrideValue;
        var gunData = GunTemplate.GetGunData(gunItemId);
        return gunData?.TinnitusMultiplier ?? 0.1f;
    }

    // 脚本端运行时设置耳鸣倍率覆盖，传 null 清除覆盖恢复模板默认
    internal static void SetTinnitusMultiplierOverride(string gunItemId, float? multiplier)
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
            // ============================================================
            // barrel/精灵字段的初始化在 OnTemplateCreatedPostfix 中完成（模板创建时
            // 直接创建 barrel 子对象）。不再尝试 patch Awake（Unity 魔法方法不可 patch）。
            // ============================================================

            // Start Postfix：将模板 GunData 写入 GunScript 组件 + 补全运行时关键字段。
            var start = AccessTools.Method(gunScriptType, "Start");
            if (start != null)
            {
                _harmony.Patch(start,
                    postfix: new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnGunStartPostfix)));
            }

            // LoadMag Prefix
            var loadMag = AccessTools.Method(gunScriptType, "LoadMag");
            if (loadMag != null)
            {
                _harmony.Patch(loadMag,
                    prefix: new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnLoadMagPrefix)));
            }

            // UnloadMag Prefix
            var unloadMag = AccessTools.Method(gunScriptType, "UnloadMag");
            if (unloadMag != null)
            {
                _harmony.Patch(unloadMag,
                    prefix: new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnUnloadMagPrefix)));
            }

            // Fire Prefix + Postfix
            var fire = AccessTools.Method(gunScriptType, "Fire");
            if (fire != null)
            {
                _harmony.Patch(fire,
                    prefix: new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnFirePrefix)));
                _harmony.Patch(fire,
                    postfix: new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnFirePostfix)));
                // Transpiler：替换 Fire() 中的 ldstr "gunjam" 为 DoPlayJamSound
                _harmony.Patch(fire,
                    transpiler: new HarmonyMethod(typeof(GunRuntimeManager), nameof(TranspileFire)));
            }

            // Update Transpiler（替换抛壳物品 ID + trigger/jam 音效）
            var update = AccessTools.Method(gunScriptType, "Update");
            if (update != null)
            {
                _harmony.Patch(update,
                    transpiler: new HarmonyMethod(typeof(GunRuntimeManager), nameof(TranspileUpdate)));
            }

            // ToggleSafety Prefix：拦截自定义枪械的保险开关，播放 SoundProfile.Safety 音效
            var toggleSafety = AccessTools.Method(gunScriptType, "ToggleSafety");
            if (toggleSafety != null)
            {
                _harmony.Patch(toggleSafety,
                    prefix: new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnToggleSafetyPrefix)));
            }

            // GetOrCreateTemplate Postfix：在 CCL 创建模板时动态添加运行时组件。
            // 这是让游戏识别自定义枪械为枪的关键补丁——geofruit 预制体上没有 GunScript。
            var customInstantiateType = AccessTools.TypeByName("CustomInstantiate");
            if (customInstantiateType != null)
            {
                var getOrCreateTemplate = AccessTools.Method(customInstantiateType, "GetOrCreateTemplate");
                if (getOrCreateTemplate != null)
                {
                    _harmony.Patch(getOrCreateTemplate,
                        postfix: new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnTemplateCreatedPostfix)));
                }
            }

            // ApplyLight Postfix：CUCoreLib 创建光源子物体后，按 JSON light.rotation 旋转光源。
            // CUCoreLib 1.0.3 的 LightProperties 无 Rotation 字段，Bark 在物品注册时记录 rotation，
            // 此处拿到光源子物体 transform 设置其 localRotation，克隆到实例后保留。
            var itemRegistryPatchesType = AccessTools.TypeByName("ItemRegistryPatches");
            var applyLight = itemRegistryPatchesType != null
                ? AccessTools.Method(itemRegistryPatchesType, "ApplyLight")
                : null;
            if (applyLight != null)
            {
                _harmony.Patch(applyLight,
                    postfix: new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnApplyLightPostfix)));
            }

            // UnloadRound Prefix：自定义弹匣退弹时生成模板弹药物品，而非原版硬编码弹药。
            var ammoScriptType = AccessTools.TypeByName("AmmoScript");
            if (ammoScriptType != null)
            {
                var unloadRound = AccessTools.Method(ammoScriptType, "UnloadRound");
                if (unloadRound != null)
                {
                    _harmony.Patch(unloadRound,
                        prefix: new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnUnloadRoundPrefix)));
                }

                // LoadRound Prefix：自定义弹匣装弹时用字符串 ammo_type 标签做兼容性检查。
                var loadRound = AccessTools.Method(ammoScriptType, "LoadRound");
                if (loadRound != null)
                {
                    _harmony.Patch(loadRound,
                        prefix: new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnLoadRoundPrefix)));
                }
            }

            // HandleGunMenu Prefix：防止自定义枪械字段为 null 导致 NRE。
            // HandleGunMenu 在 PlayerCamera.LateUpdate 中每帧调用，直接读取
            // PlayerCamera 的 UI 精灵字段和 GunScript 的非序列化字段。
            // 自定义枪械通过 geofruit 预制体 + 运行时 AddComponent 创建，
            // 部分 PlayerCamera UI 字段可能在首次持枪时尚未从 GunScript 同步。
            var playerCameraType = typeof(PlayerCamera);
            var handleGunMenu = AccessTools.Method(playerCameraType, "HandleGunMenu");
            if (handleGunMenu != null)
            {
                _harmony.Patch(handleGunMenu,
                    prefix: new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnHandleGunMenuPrefix)));
            }

            LogUtil.Info("gun_runtime.patches_applied");
        }
        catch (Exception ex)
        {
            LogUtil.Error("gun_runtime.patch_failed", ex.Message);
        }
    }

    // 卸载所有补丁
    public static void Unapply()
    {
        if (_harmony == null) return;
        _harmony.UnpatchSelf();
        _harmony = null;
    }

    // ApplyLight Postfix：CUCoreLib 创建光源子物体后，按 JSON light.rotation 旋转光源。
    // CUCoreLib 1.0.3 的 LightProperties 无 Rotation 字段，无法通过 data 层传旋转，
    // 故在此直接旋转光源子物体 transform；克隆到物品实例后保留该局部旋转。
    private static void OnApplyLightPostfix(Item item)
    {
        if (item == null) return;
        if (!ItemLoader.LightRotations.TryGetValue(item.id, out var rotation)) return;

        // 光源子物体：CUCoreLib 1.0.3 实际命名为 "Light"（反编译片段里的 "CustomLight" 是 nightly 版）。
        // 优先找直接子级，找不到则递归遍历所有层级。
        var lightChild = item.transform.Find("Light");
        if (lightChild == null)
            lightChild = FindChildRecursive(item.transform, "Light");
        if (lightChild == null)
            lightChild = FindChildRecursive(item.transform, "CustomLight");
        if (lightChild == null) return;

        lightChild.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }

    // 递归查找指定名字的子物体
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

    // 热重载后刷新所有已存在枪械实例的可热更属性（枪口位置 + 音效）。
    // 模板 JSON 变更后调用此方法，已刷出的枪无需重新创建即可生效。
    // 由 ItemLoader.RegisterFromMod 在模板重载完成后自动调用。
    public static void RefreshAllBarrelOffsets()
    {
        var guns = Object.FindObjectsOfType<GunScript>();
        foreach (var gun in guns)
        {
            if (gun is null) continue;

            var item = gun.GetComponent<Item>();
            if (item is null) continue;

            var gunData = GunTemplate.GetGunData(item.id);
            if (gunData is null) continue;

            // 枪口位置
            gun.barrel?.localPosition = new Vector3(gunData.BarrelOffsetX, gunData.BarrelOffsetY, 0f);

            // 开火音效：profile 优先 → 模板路径 → 保持现有效果
            if (gunData.SoundProfile?.Fire is { Count: > 0 })
            {
                gun.fireSound = GetSilentClip();
            }
            else if (!string.IsNullOrEmpty(gunData.FireSound))
            {
                var clip = AudioManager.LoadModAudio(gunData.ModDir, gunData.FireSound);
                if (clip != null) gun.fireSound = clip;
            }

            // 拉膛 / 回膛音效：profile 优先 → 模板路径 → 保持现有效果
            if (gunData.SoundProfile?.Rack is { Count: > 0 })
            {
                gun.customRack = gunData.SoundProfile.GetRandomClip(gunData.SoundProfile.Rack);
            }
            else if (!string.IsNullOrEmpty(gunData.RackSound))
            {
                gun.customRack = AudioManager.LoadModAudio(gunData.ModDir, gunData.RackSound);
            }

            if (gunData.SoundProfile?.Unrack is { Count: > 0 })
            {
                gun.customUnrack = gunData.SoundProfile.GetRandomClip(gunData.SoundProfile.Unrack);
            }
            else if (!string.IsNullOrEmpty(gunData.UnrackSound))
            {
                gun.customUnrack = AudioManager.LoadModAudio(gunData.ModDir, gunData.UnrackSound);
            }
            else if (gun.customRack != null)
            {
                gun.customUnrack = gun.customRack;
            }
        }
    }
}