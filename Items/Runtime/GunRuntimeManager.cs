using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Bark.Audio;
using Bark.Events;
using Bark.Items.Templates;
using Bark.Tool;
using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Items.Runtime;

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
public static class GunRuntimeManager
{
    // 直装枪械默认管容量（霰弹枪等无弹匣枪械）
    private const int DefaultDirectCapacity = 6;

    // 所有 patch 共用同一个 Harmony 实例，以便一次性 Unpatch。
    private static Harmony? _harmony;

    // ============================================================
    // 生命周期
    // ============================================================

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

            // Fire Postfix
            var fire = AccessTools.Method(gunScriptType, "Fire");
            if (fire != null)
            {
                _harmony.Patch(fire,
                    postfix: new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnFirePostfix)));
            }

            // Update Transpiler（替换抛壳物品 ID）
            var update = AccessTools.Method(gunScriptType, "Update");
            if (update != null)
            {
                _harmony.Patch(update,
                    transpiler: new HarmonyMethod(typeof(GunRuntimeManager), nameof(TranspileUpdate)));
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

    // ============================================================
    // GunScript.Start Postfix：将模板 GunData 写入 GunScript 组件
    // ============================================================
    //
    // GunScript 的非序列化字段（knockBack / firingMode / feedType / structureDamage 等）
    // 全部来自 Unity 预制体。模板物品使用通用预制体（如 "rifle"）时，这些值会继承
    // 预制体的默认值，而非模板 JSON 中声明的值。
    //
    // 此 Postfix 在预制体实例化后立即运行，用模板数据覆盖 GunScript 的关键字段，
    // 确保每把模板枪械的行为完全由 JSON 模板数据控制。
    //
    // GunScript.Fire 内部使用 conditionLossPerShot * 0.01f 计算耐久损耗。
    // 模板的 ConditionLossPerShot 是直接的耐久扣除量，因此需要反向换算：
    //   conditionLossPerShot = ConditionLossPerShot / 0.01f

    private static void OnGunStartPostfix(GunScript __instance)
    {
        var item = __instance.GetComponent<Item>();
        if (item == null) return;
        if (!GunTemplate.IsGun(item.id)) return;

        var gunData = GunTemplate.GetGunData(item.id);
        if (gunData == null) return;

        // 射击模式
        __instance.firingMode = gunData.FiringMode switch
        {
            "auto" => GunScript.FiringMode.Auto,
            "pump" => GunScript.FiringMode.Pump,
            _ => GunScript.FiringMode.SemiAuto
        };

        // 供弹方式：优先使用新字段 feed_type，兼容旧 Direct 布尔字段
        __instance.feedType = gunData.FeedType switch
        {
            "direct" => GunScript.FeedType.Direct,
            "revolver" => (GunScript.FeedType)2,
            _ => gunData.Direct ? GunScript.FeedType.Direct : GunScript.FeedType.Mag
        };

        // 弹药类型枚举：从模板 string 标签映射到游戏枚举。
        // 尝试 string → enum 直接映射（如 "Rifle" "Shotgun" "Pistol"），
        // 失败时按口径前缀映射（7_62*/5_56* → Rifle, 12gauge → Shotgun）。
        if (Enum.TryParse<GunScript.AmmoType>(gunData.AmmoType, true, out var parsedAmmoType))
        {
            __instance.ammoType = parsedAmmoType;
        }
        else if (gunData.AmmoType.StartsWith("12"))
        {
            __instance.ammoType = GunScript.AmmoType.Shotgun;
        }
        else if (gunData.AmmoType.StartsWith("7_") || gunData.AmmoType.StartsWith("5_"))
        {
            __instance.ammoType = GunScript.AmmoType.Rifle;
        }
        else
        {
            __instance.ammoType = GunScript.AmmoType.Pistol;
        }

        // 弹道 / 伤害 / 音效数值
        __instance.knockBack = gunData.Knockback;
        __instance.structureDamage = gunData.StructureDamage;
        __instance.animalDamage = gunData.AnimalDamage;
        __instance.loudness = gunData.Loudness;
        __instance.shotsPerFire = gunData.ShotsPerFire;
        __instance.verticalSpread = gunData.VerticalSpread;

        // 耐久损耗（反向换算以适配 GunScript.Fire 的 *0.01f 公式）
        __instance.conditionLossPerShot = gunData.ConditionLossPerShot / 0.01f;

        // 直装枪械的管容量
        if (gunData is { Direct: true, Capacity: > 0 })
        {
            __instance.magCapacity = gunData.Capacity;
        }

        // 动态添加的 GunScript（非预制体自带）核心运行时字段初始化为安全默认值
        __instance.racked = true;
        if (__instance.barrel == null)
        {
            var (bx, by) = GetBarrelOffset(__instance);
            CreateBarrelChild(__instance.gameObject, __instance, bx, by);
        }

        // GunScript.Update() 每帧使用 normalSprite/rackedSprite/normalSpriteNoMag/rackedSpriteNoMag
        // 覆盖 SpriteRenderer.sprite，但模板物品使用 geofruit 等通用预制体，这四个字段为 null。
        // 把 Item 当前的 sprite 拷贝进去，防止 Update 把 SpriteRenderer.sprite 设成 null，
        // 从而引发 HandleCurrentlyHeldIcon / InvButton.UpdateGraphic 的 NRE。
        var sr = __instance.GetComponent<SpriteRenderer>();
        if (sr?.sprite != null)
        {
            __instance.normalSprite = sr.sprite;
            __instance.rackedSprite = sr.sprite;
            __instance.normalSpriteNoMag = sr.sprite;
            __instance.rackedSpriteNoMag = sr.sprite;
        }
        else
        {
            // SpriteRenderer 无可渲染精灵时，从枪械预制体获取默认精灵作为回退。
            // HandleGunMenu 读取这些字段时如果为 null 会导致 NRE。
            var fallbackSprite = LoadDefaultGunSprite(__instance.ammoType);
            if (fallbackSprite != null)
            {
                __instance.normalSprite = fallbackSprite;
                __instance.rackedSprite = fallbackSprite;
                __instance.normalSpriteNoMag = fallbackSprite;
                __instance.rackedSpriteNoMag = fallbackSprite;
            }
        }

        // 最终兜底：使用 Unity 内置 4x4 白色纹理创建精灵，防止 HandleGunMenu 空引用崩溃。
        // 正常流程不应走到这里，仅作安全网使用。
        if (__instance.normalSprite == null)
        {
            LogUtil.Warning("gun_runtime.gun_init_no_sprite", item.id);
            var tex = new Texture2D(4, 4, TextureFormat.ARGB32, false) { hideFlags = HideFlags.DontSave };
            var pixels = new Color32[16];
            for (var i = 0; i < 16; i++) pixels[i] = new Color32(64, 64, 64, 255);
            tex.SetPixels32(pixels);
            tex.Apply();
            __instance.normalSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
            __instance.rackedSprite = __instance.normalSprite;
            __instance.normalSpriteNoMag = __instance.normalSprite;
            __instance.rackedSpriteNoMag = __instance.normalSprite;
        }

        // ---- 运行时状态初始化（缺失会导致无法射击且 HUD 无内容） ----

        // 膛内状态：GunScript.RoundInChamber 只有 Round / None 两个值，
        // 不含按弹药类型区分。参考 NewGun 模组的 ConfigureGun：
        //   startChambered ? RoundInChamber.Round : RoundInChamber.None
        // 设为 None 会导致 GunScript.Update() 不渲染 HUD 且阻止手动击发。
        __instance.roundInChamber = gunData.StartChambered
            ? GunScript.RoundInChamber.Round
            : GunScript.RoundInChamber.None;

        // 保险状态：按模板配置，默认 false 允许直接射击
        __instance.safe = gunData.StartSafe;

        // 拉膛/退膛状态标记
        __instance.lastRacked = false;

        // 半自动/全自动枪机循环延迟（pump 模式忽略此字段）
        __instance.desiredGasTime = gunData.DesiredGasTime;

        // 枪声：优先使用模板自定义路径，为空则按 ammoType 回退默认音效。
        if (!string.IsNullOrEmpty(gunData.FireSound))
        {
            __instance.fireSound = AudioManager.LoadModAudio(gunData.ModDir, gunData.FireSound);
        }
        if (__instance.fireSound == null)
        {
            __instance.fireSound = __instance.ammoType switch
            {
                GunScript.AmmoType.Shotgun => Resources.Load<AudioClip>("sounds/shotgunshot"),
                GunScript.AmmoType.Rifle => Resources.Load<AudioClip>("sounds/rifleshot")
                    ?? Resources.Load<AudioClip>("sounds/shotgunshot"),
                _ => Resources.Load<AudioClip>("sounds/pistolshot"),
            };
        }

        // 拉膛 / 回膛音效（自定义路径优先，为空则 GunScript.Update 使用默认 "gunrack"/"gununrack"）
        // 没有回膛音效但有上膛音效时，回退使用上膛音效。
        if (!string.IsNullOrEmpty(gunData.RackSound))
            __instance.customRack = AudioManager.LoadModAudio(gunData.ModDir, gunData.RackSound);
        if (!string.IsNullOrEmpty(gunData.UnrackSound))
            __instance.customUnrack = AudioManager.LoadModAudio(gunData.ModDir, gunData.UnrackSound);
        else if (__instance.customRack != null)
            __instance.customUnrack = __instance.customRack;

        // 弹匣供弹枪：出厂预装满弹匣
        if (!gunData.Direct && gunData.FeedType != "revolver")
        {
            var cap = gunData.Capacity > 0 ? gunData.Capacity : DefaultDirectCapacity;
            if (gunData.Capacity <= 0)
                LogUtil.Warning("gun_runtime.gun_init_capacity_zero", item.id, gunData.FeedType, DefaultDirectCapacity);

            __instance.magCapacity = cap;
            __instance.hasMag = true;
            __instance.roundsInMag = cap;

            var state = GunMagTracker.GetOrCreate(item);
            state.RoundsInMag = cap;
            // 记录对应的弹匣物品 ID，确保卸弹时能正确生成弹匣物品。
            // 按 mag_type → ammo_type 顺序查找已注册的弹匣模板。
            var defaultMagIds = MagTemplate.FindMagsByType(gunData.MagType);
            if (defaultMagIds.Count == 0)
            {
                LogUtil.Warning("gun_runtime.gun_init_no_mag_by_type", item.id, gunData.MagType, gunData.AmmoType);
                defaultMagIds = MagTemplate.FindMagsByAmmoType(gunData.AmmoType);
            }
            if (defaultMagIds.Count > 0)
            {
                var foundMagData = MagTemplate.GetMagData(defaultMagIds[0]);
                if (foundMagData != null && foundMagData.MagType != gunData.MagType)
                    LogUtil.Warning("gun_runtime.gun_init_mag_type_mismatch", item.id, gunData.MagType, defaultMagIds[0], foundMagData.MagType, gunData.MagType);
            }
            state.MagItemId = defaultMagIds.Count > 0 ? defaultMagIds[0] : null;
        }
        else if (gunData.FeedType == "revolver")
        {
            // 转轮枪：弹容量用 Capacity，出厂满弹
            var cap = gunData.Capacity > 0 ? gunData.Capacity : 6;
            __instance.magCapacity = cap;
            __instance.hasMag = false;
            __instance.roundsInMag = cap;
        }
        else
        {
            // 直装枪：仅设置管容量，出厂空管
            var cap = gunData.Capacity > 0 ? gunData.Capacity : DefaultDirectCapacity;
            __instance.magCapacity = cap;
        }

        // muzzleParticle：参考 NewGun.CreateMuzzle —— 从游戏预制体克隆枪口粒子，
        // 避免空 ParticleSystem 组件渲染紫色方块。
        if (__instance.muzzleParticle == null)
        {
            __instance.muzzleParticle = CloneMuzzleFromPrefab(__instance.ammoType, __instance.transform);
        }

        LogUtil.Info("gun_runtime.gun_init", item.id, gunData.MagType, gunData.AmmoType, gunData.FeedType, gunData.Capacity);
    }

    // 从游戏 pistol/shotgun 预制体克隆枪口粒子。
    // 参考 NewGun.CreateMuzzle：无弹药类型的完整映射。
    private static ParticleSystem CloneMuzzleFromPrefab(GunScript.AmmoType ammoType, Transform parent)
    {
        try
        {
            var prefabName = ammoType switch
            {
                GunScript.AmmoType.Shotgun => "shotgun",
                GunScript.AmmoType.Rifle => "rifle",
                _ => "pistol",
            };
            var prefab = Resources.Load(prefabName) as GameObject;
            if (prefab == null) goto fallback;

            var childIdx = ammoType == GunScript.AmmoType.Shotgun ? 1 : 2;
            if (prefab.transform.childCount <= childIdx) goto fallback;

            var src = prefab.transform.GetChild(childIdx).gameObject;
            var clone = Object.Instantiate(src, parent, false);
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localRotation = Quaternion.identity;

            var ps = clone.GetComponent<ParticleSystem>();
            if (ps != null) return ps;
        }
        catch
        {
            // ignored
        }

        fallback:
        // 回退：创建空占位防止 NRE，禁用发射避免紫色方块
        var fallbackPs = parent.gameObject.AddComponent<ParticleSystem>();
        fallbackPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var emission = fallbackPs.emission;
        emission.enabled = false;
        return fallbackPs;
    }

    // 从枪械预制体获取默认精灵，用于 GunScript 精灵字段的回退。
    // 当 CCL 自定义 PNG 精灵尚未就绪时，防止 HandleGunMenu 空引用崩溃。
    private static Sprite? LoadDefaultGunSprite(GunScript.AmmoType ammoType)
    {
        var prefabName = ammoType switch
        {
            GunScript.AmmoType.Shotgun => "shotgun",
            GunScript.AmmoType.Rifle => "rifle",
            _ => "pistol",
        };
        var prefab = Resources.Load(prefabName) as GameObject;
        return prefab?.GetComponent<SpriteRenderer>()?.sprite;
    }

    // ============================================================
    // HandleGunMenu Prefix：守卫自定义枪械的所有潜在 null 字段
    // ============================================================
    //
    // HandleGunMenu 在 PlayerCamera.LateUpdate 中每帧调用，直接读取：
    //   a) PlayerCamera 的 UI 精灵字段（gunNormalSprite / gunRackedSprite 等）
    //   b) GunScript 的非序列化字段（barrel / roundInChamber 等）
    //
    // 自定义枪械通过 geofruit 预制体 + 运行时 AddComponent<GunScript>() 创建，
    // OnGunStartPostfix 已补齐 GunScript 自身的字段，但 PlayerCamera 的 UI 字段
    // 在首次持握自定义枪械时可能尚未从 GunScript 同步，导致 NullReferenceException。
    //
    // 此 Prefix 在原始 HandleGunMenu 方法体执行前统一检查所有潜在 null 路径，
    // 发现 null 则记录日志并跳过本帧（返回 false），避免整个游戏崩溃。

    private static bool OnHandleGunMenuPrefix(PlayerCamera __instance)
    {
        if (__instance == null) return false;

        try
        {
            var pc = Traverse.Create(__instance);

            // 1. 检查 body
            var body = pc.Field("body").GetValue();
            if (body == null) return false;

            var bodyTr = Traverse.Create(body);

            // 2. 检查是否持枪（复制 HandleGunMenu:2461 的条件链）
            var handSlot = bodyTr.Field("handSlot").GetValue<int>();
            if (!bodyTr.Method("HoldingItem", handSlot).GetValue<bool>())
                return true;

            var item = bodyTr.Method("GetItem", handSlot).GetValue();
            if (item == null) return false;

            // 3. 检查 Stats.HasTag("gun")
            var stats = Traverse.Create(item).Property("Stats").GetValue();
            if (stats == null) return false;
            if (!Traverse.Create(stats).Method("HasTag", "gun").GetValue<bool>())
                return true;

            // 4. 检查 GetComponent<GunScript>()
            var gun = ((Component)item).GetComponent<GunScript>();
            if (gun == null)
            {
                LogUtil.Warning("gun_runtime.handle_gun_menu_null_gunscript");
                return false;
            }

            // 5. GunScript.barrel — HandleGunMenu:2470 访问 barrel.transform.position
            if (gun.barrel == null)
            {
                LogUtil.Warning("gun_runtime.handle_gun_menu_null_barrel");
                return false;
            }

            // 6. PlayerCamera UI 字段（HandleGunMenu:2465-2469）
            return CheckPlayerCameraUIFields(pc);
        }
        catch (Exception ex)
        {
            LogUtil.Warning("gun_runtime.handle_gun_menu_prefix_error", ex.GetType().Name, ex.Message);
            return false;
        }
    }

    // 检查 HandleGunMenu 中使用的 PlayerCamera UI 字段。
    // 这些是 PlayerCamera 的序列化字段，正常应由场景预制体赋值。
    // 如果为 null 说明 UI 未完全初始化（自定义枪械首次持握时的时序问题）。
    // 返回 true 表示全部有效，false 表示有 null 字段。
    private static bool CheckPlayerCameraUIFields(Traverse pc)
    {
        // HandleGunMenu:2465 gunRackImage.sprite = component.racked ? gunRackedSprite : gunNormalSprite
        if (pc.Field("gunRackImage").GetValue() == null) { Log("gunRackImage"); return false; }
        if (pc.Field("gunRackedSprite").GetValue() == null) { Log("gunRackedSprite"); return false; }
        if (pc.Field("gunNormalSprite").GetValue() == null) { Log("gunNormalSprite"); return false; }

        // HandleGunMenu:2466 gunMagButton.interactable = ...
        if (pc.Field("gunMagButton").GetValue() == null) { Log("gunMagButton"); return false; }

        // HandleGunMenu:2467 gunSafeImage.sprite = component.safe ? gunSafeSprite : gunUnsafeSprite
        if (pc.Field("gunSafeImage").GetValue() == null) { Log("gunSafeImage"); return false; }
        if (pc.Field("gunSafeSprite").GetValue() == null) { Log("gunSafeSprite"); return false; }
        if (pc.Field("gunUnsafeSprite").GetValue() == null) { Log("gunUnsafeSprite"); return false; }

        // HandleGunMenu:2468 gunBulletImage.sprite = gunBulletSprites[...]
        if (pc.Field("gunBulletImage").GetValue() == null) { Log("gunBulletImage"); return false; }
        if (pc.Field("gunBulletSprites").GetValue() == null) { Log("gunBulletSprites"); return false; }

        // HandleGunMenu:2469 gunCrosshair.gameObject.SetActive(!component.safe)
        if (pc.Field("gunCrosshair").GetValue() == null) { Log("gunCrosshair"); return false; }

        return true;

        static void Log(string field) =>
            LogUtil.Warning("gun_runtime.handle_gun_menu_null_pc_field", field);
    }

    // CCL 创建模板物品后，按 Bark 模板类型动态补加 GunScript / AmmoScript。
    // CCL 的 CreateTemplate 已为 Wearable/BatteryItem 做了同样的 AddComponent 模式。
    // 模板对象 SetActive(false) 缓存，后续 InstantiateReturn 克隆自动继承这些组件。
    //
    // 只影响 Bark.RegisterFromMod 注册的模板物品（GunTemplate/MagTemplate/AmmunitionTemplate），
    // 不会触及原版物品的初始化流程。
    private static void OnTemplateCreatedPostfix(GameObject __result, string id)
    {
        if (__result == null)
            return;

        // 从模板身上的 Item 组件读取 CCL 已标准化后的物品 ID，
        // 优先使用组件上的 id（CreateTemplate.component1.id）确保与模板缓存精确一致。
        var item = __result.GetComponent<Item>();
        var itemId = item is not null ? item.id : id;
        if (string.IsNullOrEmpty(itemId))
            return;

        // 枪械模板：补加 GunScript + 枪管子对象
        // 原版枪械预制体自带名为 "barrel" 的子 Transform，GunScript.barrel 指向它。
        // 通用预制体（geofruit/rifle）没有此子对象，导致：
        //   1. barrel 为 null → HandleGunMenu NRE
        //   2. 用 transform 替代 → 子弹从枪中心射出，偏移异常
        // 在模板身上创建 barrel 子对象后，Unity 序列化系统会在 clone 时
        // 自动重映射引用，clone 的 GunScript.barrel 正确指向 clone 的 barrel 子对象。
        if (GunTemplate.IsGun(itemId) && __result.GetComponent<GunScript>() == null)
        {
            var gun = __result.AddComponent<GunScript>();
            var (bx, by) = GetBarrelOffset(gun);
            CreateBarrelChild(__result, gun, bx, by);
        }

        // 弹匣模板：补加 AmmoScript（itemType=Magazine）
        if (MagTemplate.IsMag(itemId) && __result.GetComponent<AmmoScript>() == null)
        {
            var magData = MagTemplate.GetMagData(itemId);
            var am = __result.AddComponent<AmmoScript>();
            am.itemType = AmmoScript.AmmoItemType.Magazine;
            if (magData != null)
            {
                am.maxRounds = magData.Capacity;
                am.rounds = magData.Capacity; // 出厂满弹匣
                am.ammoType = Enum.TryParse<GunScript.AmmoType>(magData.AmmoType, true, out var parsedMagAmmo)
                    ? parsedMagAmmo
                    : GunScript.AmmoType.Pistol;
            }
        }

        // 弹药模板：补加 AmmoScript（itemType=Round）
        if (AmmunitionTemplate.IsAmmo(itemId) && __result.GetComponent<AmmoScript>() == null)
        {
            var am = __result.AddComponent<AmmoScript>();
            am.itemType = AmmoScript.AmmoItemType.Round;
        }
    }

    // 为枪械创建 barrel 子 Transform 作为枪口引用点。
    // 原版枪械预制体自带此子对象，GunScript.barrel 指向它；
    // GunScript.Fire 用 barrel.position 作为子弹生成位置，用 transform.right 作为射击方向。
    // HandleGunMenu 用 barrel.position + transform.right * distance 放置准星。
    // 两者在同一射线上，barrel 只影响生成点/准星位置，不影响方向。
    // geofruit/rifle 通用预制体无此子对象，使用时 barrel=transform 会导致：
    //   - 子弹从枪中心而非枪口射出 → 弹道偏移
    //   - 如果 barrel 不是序列化字段 → clone 后为 null
    //
    // 偏移量由枪械模板的 barrel_offset.{x,y} 控制，不同枪型可自行配置。
    private static void CreateBarrelChild(GameObject gunObj, GunScript gun, float offsetX, float offsetY)
    {
        // 查找已有的 barrel 子对象（部分预制体可能自带）
        var existing = gunObj.transform.Find("barrel");
        if (existing != null)
        {
            gun.barrel = existing;
            return;
        }

        var barrelChild = new GameObject("barrel");
        barrelChild.transform.SetParent(gunObj.transform);

        barrelChild.transform.localPosition = new Vector3(offsetX, offsetY, 0f);
        barrelChild.transform.localRotation = Quaternion.identity;
        barrelChild.transform.localScale = Vector3.one;

        gun.barrel = barrelChild.transform;
    }

    // 从 Item 组件获取物品 ID，查找 GunData 中的 barrel_offset，
    // 返回枪口偏移量。若无法获取则返回默认步枪偏移 (0.5, 0)。
    private static (float x, float y) GetBarrelOffset(Component? component)
    {
        var item = component?.GetComponent<Item>();
        if (item is null) return (0.5f, 0f);

        var gunData = GunTemplate.GetGunData(item.id);
        if (gunData is null) return (0.5f, 0f);

        return (gunData.BarrelOffsetX, gunData.BarrelOffsetY);
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

            // 开火音效（为空则保持现有效果）
            if (!string.IsNullOrEmpty(gunData.FireSound))
            {
                var clip = AudioManager.LoadModAudio(gunData.ModDir, gunData.FireSound);
                if (clip != null) gun.fireSound = clip;
            }

            // 拉膛 / 回膛音效
            // 没有回膛音效但有上膛音效时，回退使用上膛音效。
            if (!string.IsNullOrEmpty(gunData.RackSound))
                gun.customRack = AudioManager.LoadModAudio(gunData.ModDir, gunData.RackSound);
            if (!string.IsNullOrEmpty(gunData.UnrackSound))
                gun.customUnrack = AudioManager.LoadModAudio(gunData.ModDir, gunData.UnrackSound);
            else if (gun.customRack != null)
                gun.customUnrack = gun.customRack;
        }
    }

    // 自定义弹匣退弹：拦截 AmmoScript.UnloadRound，
    // 将硬编码的原版弹药映射替换为模板弹药物品（通过 AmmunitionTemplate 查找匹配的 ammo_type）。
    private static bool OnUnloadRoundPrefix(AmmoScript __instance)
    {
        if (__instance.itemType != AmmoScript.AmmoItemType.Magazine) return true;
        if (__instance.rounds <= 0) return false;

        var item = __instance.GetComponent<Item>();
        if (item == null || !MagTemplate.IsMag(item.id)) return true;

        var magData = MagTemplate.GetMagData(item.id);
        if (magData == null) return true;

        // 按弹匣的 ammo_type 标签查找匹配的自定义弹药
        var ammoIds = AmmunitionTemplate.FindAmmoByType(magData.AmmoType);
        if (ammoIds.Count == 0) return true; // 无自定义弹药，走原版逻辑

        var ammoItemId = ammoIds[0];
        var spawned = CustomInstantiate.InstantiateReturn(ammoItemId, __instance.transform.position, Quaternion.identity);
        if (spawned == null) return true;

        PlayerCamera.main.body.AutoPickUpItem(spawned.GetComponent<Item>());
        Sound.Play("gunloadshell", __instance.transform.position);
        --__instance.rounds;

        return false; // 阻止原版 UnloadRound
    }

    // 自定义弹匣装弹：用 ammo_type 字符串标签做兼容性检查，
    // 替代原版 AmmoScript.ammoType 枚举比较（自定义物品的枚举值不可靠）。
    // 自定义弹匣只接受模板注册且标签匹配的弹药，原版弹药一律阻止。
    private static bool OnLoadRoundPrefix(AmmoScript __instance, AmmoScript ammo)
    {
        if (__instance.itemType != AmmoScript.AmmoItemType.Magazine) return true;
        if (ammo.itemType != AmmoScript.AmmoItemType.Round) return true;

        var magItem = __instance.GetComponent<Item>();
        if (magItem == null || !MagTemplate.IsMag(magItem.id)) return true;

        // 自定义弹匣：弹药必须有 Item 组件且已模板注册
        var ammoItem = ammo.GetComponent<Item>();
        if (ammoItem == null) return false;
        if (!AmmunitionTemplate.IsAmmo(ammoItem.id)) return false;

        // 字符串标签兼容性检查
        var magAmmoType = MagTemplate.GetAmmoType(magItem.id);
        var ammoType = AmmunitionTemplate.GetAmmoType(ammoItem.id);
        if (magAmmoType != ammoType) return false; // 不匹配，阻止装弹

        // 容量检查
        if (__instance.rounds >= __instance.maxRounds) return false;

        ++__instance.rounds;
        Sound.Play("gunloadshell", __instance.transform.position);
        Object.Destroy(ammo.gameObject);

        return false; // 阻止原版 LoadRound
    }

    private static bool OnLoadMagPrefix(GunScript __instance, AmmoScript ammo)
    {
        if (ammo == null) return true;

        var gunItem = __instance.GetComponent<Item>();
        if (gunItem == null) return true;

        // 非模板枪械 → 走原生逻辑
        if (!GunTemplate.IsGun(gunItem.id)) return true;

        var gunData = GunTemplate.GetGunData(gunItem.id);
        if (gunData == null) return true;

        var ammoItem = ammo.GetComponent<Item>();
        var ammoItemId = ammoItem?.id;

        LogUtil.Info("gun_runtime.load_mag_attempt", gunItem.id, ammoItemId ?? "null", ammo.itemType, MagTemplate.IsMag(ammoItemId ?? string.Empty));

        switch (ammo.itemType)
        {
            // ============================================================
            // 弹匣装填
            // ============================================================
            case AmmoScript.AmmoItemType.Magazine when string.IsNullOrEmpty(ammoItemId):
            // 验证模板
            case AmmoScript.AmmoItemType.Magazine when !MagTemplate.IsMag(ammoItemId):
                return true;
            // 验证 mag_type 兼容
            case AmmoScript.AmmoItemType.Magazine when MagTemplate.GetMagType(ammoItemId) != gunData.MagType:
                LogUtil.Warning("gun_runtime.load_mag_incompatible_mag_type", ammoItemId, MagTemplate.GetMagType(ammoItemId), gunData.MagType);
                return false;
            // 验证 ammo_type 兼容
            case AmmoScript.AmmoItemType.Magazine when MagTemplate.GetAmmoType(ammoItemId) != gunData.AmmoType:
                LogUtil.Warning("gun_runtime.load_mag_incompatible_ammo_type", ammoItemId, MagTemplate.GetAmmoType(ammoItemId), gunData.AmmoType);
                return false;
            case AmmoScript.AmmoItemType.Magazine:
            {
                var capacity = MagTemplate.GetCapacity(ammoItemId);
                var rounds = ammo.rounds;
                if (rounds > capacity) rounds = capacity;

                // 设置 GunScript 原生状态
                __instance.hasMag = true;
                __instance.roundsInMag = rounds;

                // 追踪模板状态
                var state = GunMagTracker.GetOrCreate(gunItem);
                state.MagItemId = ammoItemId;
                state.RoundsInMag = rounds;
                state.AmmoItemId = null; // 弹药类型从弹匣的 ammo_type 推断

                // 播放音效
                Sound.Play("gunloadmag", __instance.transform.position);

                // 销毁弹药 GameObject
                Object.Destroy(ammo.gameObject);

                // 手动触发事件（因为返回 false 会跳过 GunEventListener 的 Postfix）
                EventUtil.Trigger(new GunLoadAmmoEvent
                {
                    GunItem = gunItem,
                    AmmoItemId = ammoItemId,
                    Rounds = rounds
                });

                LogUtil.Info("gun_runtime.load_mag", gunItem.id, ammoItemId, rounds);
                return false;
            }
            // ============================================================
            // 散装弹药装填（仅 Direct=true 枪械）
            // ============================================================
            case AmmoScript.AmmoItemType.Round when !gunData.Direct:
                // 弹匣供弹枪不能直接装散装子弹
                return false;
            case AmmoScript.AmmoItemType.Round when string.IsNullOrEmpty(ammoItemId):
            // 验证 ammo_type 兼容
            case AmmoScript.AmmoItemType.Round when !AmmunitionTemplate.IsAmmo(ammoItemId):
                return true;
            case AmmoScript.AmmoItemType.Round when AmmunitionTemplate.GetAmmoType(ammoItemId) != gunData.AmmoType:
                LogUtil.Warning("gun_runtime.incompatible_ammo_type",
                    AmmunitionTemplate.GetAmmoType(ammoItemId), gunData.AmmoType);
                return false;
            case AmmoScript.AmmoItemType.Round:
            {
                var state = GunMagTracker.GetOrCreate(gunItem);
                var capacity = gunData.Capacity > 0 ? gunData.Capacity : DefaultDirectCapacity;

                if (state.RoundsInMag >= capacity)
                {
                    // 已装满
                    return false;
                }

                // 逐发装填
                if (!__instance.hasMag)
                {
                    __instance.hasMag = true;
                    __instance.roundsInMag = 1;
                }
                else
                {
                    __instance.roundsInMag++;
                }

                state.RoundsInMag = __instance.roundsInMag;
                state.AmmoItemId = ammoItemId;

                Sound.Play("gunloadmag", __instance.transform.position);
                Object.Destroy(ammo.gameObject);

                EventUtil.Trigger(new GunLoadAmmoEvent
                {
                    GunItem = gunItem,
                    AmmoItemId = ammoItemId,
                    Rounds = 1
                });

                LogUtil.Info("gun_runtime.load_round", gunItem.id, ammoItemId, state.RoundsInMag);
                return false;
            }
            default:
                return true;
        }
    }

    // ============================================================
    // UnloadMag Prefix：拦截并覆盖原生的卸弹逻辑
    // ============================================================

    private static bool OnUnloadMagPrefix(GunScript __instance)
    {
        var gunItem = __instance.GetComponent<Item>();
        if (gunItem == null) return true;

        // 非模板枪械 → 走原生逻辑
        if (!GunTemplate.IsGun(gunItem.id)) return true;

        var gunData = GunTemplate.GetGunData(gunItem.id);
        if (gunData == null) return true;

        var state = GunMagTracker.Get(gunItem);

        if (gunData.Direct)
        {
            // ============================================================
            // 直装枪械：逐发卸出
            // ============================================================
            var roundsToUnload = state?.RoundsInMag ?? __instance.roundsInMag;
            var ammoItemId = state?.AmmoItemId;

            if (roundsToUnload > 0 && !string.IsNullOrEmpty(ammoItemId))
            {
                var body = __instance.GetComponentInParent<Body>();
                if (body != null)
                {
                    // 生成散装弹药
                    var go = Utils.Create(ammoItemId, body.transform.position, 0f);
                    if (go != null)
                    {
                        var ammoComp = go.GetComponent<AmmoScript>();
                        if (ammoComp != null) ammoComp.rounds = roundsToUnload;
                        body.AutoPickUpItem(go.GetComponent<Item>());
                    }
                }
            }

            // 清除状态
            __instance.hasMag = false;
            __instance.roundsInMag = 0;
            GunMagTracker.Remove(gunItem);

            EventUtil.Trigger(new GunUnloadEvent
            {
                GunItem = gunItem,
                RoundsUnloaded = roundsToUnload
            });

            LogUtil.Info("gun_runtime.unload_direct", gunItem.id, roundsToUnload);
            return false;
        }

        // ============================================================
        // 弹匣供弹枪械：卸下弹匣
        // ============================================================
        var magItemId = state?.MagItemId;
        var magRounds = state?.RoundsInMag ?? __instance.roundsInMag;

        LogUtil.Info("gun_runtime.on_unload_mag", magItemId ?? "null", state?.RoundsInMag ?? 0, __instance.roundsInMag);

        // 即使 magRounds==0 也要生成弹匣物品（destroy_at_zero_condition=false 控制销毁）。
        if (!string.IsNullOrEmpty(magItemId))
        {
            var body = __instance.GetComponentInParent<Body>();
            if (body != null)
            {
                var go = Utils.Create(magItemId, body.transform.position, 0f);
                if (go != null)
                {
                    var ammoComp = go.GetComponent<AmmoScript>();
                    if (ammoComp != null)
                    {
                        ammoComp.rounds = magRounds;
                        // 用模板容量覆盖预制体的 maxRounds
                        if (MagTemplate.IsMag(magItemId))
                            ammoComp.maxRounds = MagTemplate.GetCapacity(magItemId);
                    }

                    body.AutoPickUpItem(go.GetComponent<Item>());
                }
            }
        }

        // 如果 tracker 状态丢失（换背包/丢弃后实例 ID 变化导致 GetInstanceID 不同），
        // 用枪械的 mag_type 和 ammo_type 回退查找匹配弹匣，避免「弹匣消失但没生成物品」。
        if (string.IsNullOrEmpty(magItemId) && __instance is { hasMag: true, roundsInMag: > 0 })
        {
            var fallbackMagIds = MagTemplate.FindMagsByType(gunData.MagType);
            LogUtil.Info("gun_runtime.unload_mag_fallback_mag_type", gunData.MagType, fallbackMagIds.Count);
            if (fallbackMagIds.Count > 0)
                magItemId = fallbackMagIds[0];
            else
            {
                var altIds = MagTemplate.FindMagsByAmmoType(gunData.AmmoType);
                LogUtil.Info("gun_runtime.unload_mag_fallback_ammo_type", gunData.AmmoType, altIds.Count);
                if (altIds.Count > 0) magItemId = altIds[0];
            }

            if (!string.IsNullOrEmpty(magItemId))
            {
                var body = __instance.GetComponentInParent<Body>();
                if (body != null)
                {
                    var go = Utils.Create(magItemId, body.transform.position, 0f);
                    if (go != null)
                    {
                        var ammoComp = go.GetComponent<AmmoScript>();
                        if (ammoComp != null)
                        {
                            ammoComp.rounds = magRounds;
                            if (MagTemplate.IsMag(magItemId))
                                ammoComp.maxRounds = MagTemplate.GetCapacity(magItemId);
                        }
                        body.AutoPickUpItem(go.GetComponent<Item>());
                    }
                }
            }
        }

        // 清除状态
        __instance.hasMag = false;
        __instance.roundsInMag = 0;
        GunMagTracker.Remove(gunItem);

        Sound.Play("gununloadmag", __instance.transform.position);

        EventUtil.Trigger(new GunUnloadEvent
        {
            GunItem = gunItem,
            RoundsUnloaded = magRounds
        });

        LogUtil.Info("gun_runtime.unload_mag", gunItem.id, magItemId ?? "unknown", magRounds);
        return false;
    }

    // ============================================================
    // Fire Postfix：消耗弹药、记录弹壳类型、应用耐久损耗
    // ============================================================

    private static void OnFirePostfix(GunScript __instance)
    {
        var gunItem = __instance.GetComponent<Item>();
        if (gunItem == null) return;

        // 非模板枪械 → 不处理
        if (!GunTemplate.IsGun(gunItem.id)) return;

        var gunData = GunTemplate.GetGunData(gunItem.id);
        if (gunData == null) return;

        var state = GunMagTracker.GetOrCreate(gunItem);

        // 消耗一发（如果 Tracker 追踪的余弹为 0，用 GunScript 的值兜底）
        if (state.RoundsInMag > 0)
        {
            state.RoundsInMag--;
        }
        else if (__instance.roundsInMag > 0)
        {
            state.RoundsInMag = __instance.roundsInMag - 1;
        }

        // 同步到 GunScript（游戏原生的 Update 也依赖这个值来膛装下一发）
        __instance.roundsInMag = state.RoundsInMag;

        // 确定弹壳类型
        var casingType = ResolveAmmoCasingType(state, gunData);
        if (casingType != null)
        {
            state.PendingCasingType = casingType;
        }

        // 注意：耐久损耗已在 GunScript.Start Postfix 中完成配置，
        // GunScript.Fire 内部会通过 conditionLossPerShot * 0.01f 自动扣除。
    }

    // 推断当前弹药对应的弹壳类型
    private static string? ResolveAmmoCasingType(GunMagTracker.MagState state, GunData gunData)
    {
        // 优先：直接记录的弹药物品 ID
        if (!string.IsNullOrEmpty(state.AmmoItemId))
        {
            var ct = AmmunitionTemplate.GetCasingType(state.AmmoItemId);
            if (ct != null) return ct;
        }

        // 其次：通过弹匣的 ammo_type 匹配任意弹药
        if (!string.IsNullOrEmpty(state.MagItemId))
        {
            var magAmmoType = MagTemplate.GetAmmoType(state.MagItemId);
            var ammoIds = AmmunitionTemplate.FindAmmoByType(magAmmoType);
            if (ammoIds.Count > 0)
            {
                var ct = AmmunitionTemplate.GetCasingType(ammoIds[0]);
                if (ct != null) return ct;
            }
        }

        // 兜底：通过枪械的 ammo_type 匹配任意弹药
        {
            var ammoIds = AmmunitionTemplate.FindAmmoByType(gunData.AmmoType);
            if (ammoIds.Count <= 0) return null;
            var ct = AmmunitionTemplate.GetCasingType(ammoIds[0]);
            if (ct != null) return ct;
        }

        return null;
    }

    // ============================================================
    // Update Transpiler：替换抛壳逻辑，用 Utils.Create 直接创建自定义弹壳
    // ============================================================
    //
    // GunScript.Update() 中 rack 事件原始抛壳 IL：
    //   ldstr "casing" → call Resources.Load → [pos/rot args] → call Object.Instantiate
    //   → isinst GameObject → GetComponent<Rigidbody2D> → velocity
    //
    // 问题：Resources.Load 只能加载 Unity Resources 目录下的预制体，
    // 自定义弹壳通过 CCL 的 CustomInstantiate 注册，Resources.Load 返回 null。
    //
    // 修复：将 true 分支（弹壳路径）的 ldstr "casing" ~ Object.Instantiate
    // 替换为 ldarg.0 + call DoSpawnCasing(GunScript)，直接通过 Utils.Create
    // 创建自定义弹壳物品并返回 GameObject。
    // 然后修改 br.s 跳转目标直接跳到 Instantiate 之后，
    // 这样自定义弹壳路径完全绕过 Resources.Load + Instantiate。
    private static IEnumerable<CodeInstruction> TranspileUpdate(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var doSpawnMethod = AccessTools.Method(typeof(GunRuntimeManager), nameof(DoSpawnCasing));

        // Step 1: 找到 ldstr "casing"（true 分支）
        var casingIdx = -1;
        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand is string s && s == "casing")
            {
                casingIdx = i;
                break;
            }
        }

        if (casingIdx < 0) return codes;

        // Step 2: 确认下一条是 br/br.s（跳过 false 分支直达 ternary 汇合点）
        var brIdx = casingIdx + 1;
        if (brIdx >= codes.Count) return codes;
        if (codes[brIdx].opcode != OpCodes.Br && codes[brIdx].opcode != OpCodes.Br_S) return codes;

        // Step 3: 找到 call Object.Instantiate（在 Resources.Load + pos/rot args 之后）
        var instantiateIdx = -1;
        for (var i = brIdx + 1; i < codes.Count; i++)
        {
            var op = codes[i].opcode;
            if (op != OpCodes.Call && op != OpCodes.Callvirt) continue;
            if (codes[i].operand is not System.Reflection.MethodBase m) continue;
            if (m.Name == "Instantiate" && m.DeclaringType == typeof(Object))
            {
                instantiateIdx = i;
                break;
            }
        }

        if (instantiateIdx < 0) return codes;

        // Step 4: 将 br.s 跳转目标从 ternary 汇合点改为 Instantiate 之后
        // true 分支（弹壳）直接跳到 Instantiate 之后，绕过 Resources.Load + args + Instantiate
        // false 分支（实弹）保持原路径不变：AmmoTypeToItem → Resources.Load → Instantiate
        var postInstantiateIdx = instantiateIdx + 1;
        if (postInstantiateIdx >= codes.Count) return codes;
        var postInstantiateLabel = new Label();
        codes[postInstantiateIdx].labels.Add(postInstantiateLabel);
        codes[brIdx] = new CodeInstruction(OpCodes.Br, postInstantiateLabel);

        // Step 5: 将 ldstr "casing" 替换为 ldarg.0 + call DoSpawnCasing
        // DoSpawnCasing 返回 Object（GameObject），与 Instantiate 返回类型一致，
        // 下游 isinst GameObject → GetComponent<Rigidbody2D> → velocity 无需改动
        codes[casingIdx] = new CodeInstruction(OpCodes.Ldarg_0);
        codes.Insert(casingIdx + 1, new CodeInstruction(OpCodes.Call, doSpawnMethod));

        return codes;
    }

    // Transpiler 回调：直接创建自定义弹壳 GameObject。
    // 由 Update Transpiler 动态注入到 GunScript.Update 的 true 分支（弹壳路径）。
    // 若无法匹配自定义弹壳则退回原版 Resources.Load("casing") + Instantiate 兜底。
    // 注意：此方法由 Transpiler 注入到 Update 中，请勿在此处使用 LogUtil 以避免热路径日志洪水。
    private static Object? DoSpawnCasing(GunScript gun)
    {
        if (gun == null) return null;

        var item = gun.GetComponent<Item>();
        if (item == null) return null;

        // 通过模板查找自定义弹壳物品
        string? casingId = null;
        var state = GunMagTracker.Get(item);
        if (state?.PendingCasingType != null)
        {
            var casingIds = CasingTemplate.FindCasingsByType(state.PendingCasingType);
            casingId = casingIds.Count > 0 ? casingIds[0] : null;
            state.PendingCasingType = null; // 消费一次
        }

        // 自定义弹壳：通过 CCL 的 Utils.Create 创建
        if (casingId != null)
        {
            var go = Utils.Create(casingId, gun.transform.position, gun.transform.rotation.eulerAngles.z);
            if (go != null) return go;
        }

        // 兜底：原版 "casing" 预制体
        var prefab = Resources.Load("casing");
        return prefab != null
            ? Object.Instantiate(prefab, gun.transform.position, gun.transform.rotation)
            : null;
    }
}