using System;
using System.Collections.Generic;
using System.Reflection.Emit;
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
            // [临时注释] 装弹/退弹/开火补丁暂注，待逐步恢复。
            // ============================================================

            // Start Postfix：将模板 GunData 写入 GunScript 组件 + 补全运行时关键字段。
            var start = AccessTools.Method(gunScriptType, "Start");
            if (start != null)
            {
                _harmony.Patch(start,
                    postfix: new HarmonyMethod(typeof(GunRuntimeManager), nameof(OnGunStartPostfix)));
            }

            /*
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
            */

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

        // 供弹方式
        __instance.feedType = gunData.Direct ? GunScript.FeedType.Direct : GunScript.FeedType.Mag;

        // 弹药类型枚举：从模板 string 标签映射到游戏枚举，失败时回退 Pistol
        __instance.ammoType = Enum.TryParse<GunScript.AmmoType>(gunData.AmmoType, true, out var parsedAmmoType)
            ? parsedAmmoType
            : GunScript.AmmoType.Pistol;

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
        __instance.barrel = __instance.transform;

        // GunScript.Update() 每帧使用 normalSprite/rackedSprite/normalSpriteNoMag/rackedSpriteNoMag
        // 覆盖 SpriteRenderer.sprite，但模板物品使用 geofruit 等通用预制体，这四个字段为 null。
        // 把 Item 当前的 sprite 拷贝进去，防止 Update 把 SpriteRenderer.sprite 设成 null，
        // 从而引发 HandleCurrentlyHeldIcon / InvButton.UpdateGraphic 的 NRE。
        var sr = __instance.GetComponent<SpriteRenderer>();
        if (sr is not null && sr.sprite is not null)
        {
            __instance.normalSprite = sr.sprite;
            __instance.rackedSprite = sr.sprite;
            __instance.normalSpriteNoMag = sr.sprite;
            __instance.rackedSpriteNoMag = sr.sprite;
        }
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

        // 枪械模板：补加 GunScript
        if (GunTemplate.IsGun(itemId) && __result.GetComponent<GunScript>() == null)
        {
            __result.AddComponent<GunScript>();
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
        Sound.Play("gunloadshell", (Vector2)__instance.transform.position);
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
        Sound.Play("gunloadshell", (Vector2)__instance.transform.position);
        Object.Destroy((Object)ammo.gameObject);

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
                LogUtil.Warning("gun_runtime.incompatible_mag", ammoItemId, gunData.MagType);
                return false;
            // 验证 ammo_type 兼容
            case AmmoScript.AmmoItemType.Magazine when MagTemplate.GetAmmoType(ammoItemId) != gunData.AmmoType:
                LogUtil.Warning("gun_runtime.incompatible_ammo_type",
                    MagTemplate.GetAmmoType(ammoItemId), gunData.AmmoType);
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

        if (!string.IsNullOrEmpty(magItemId) && magRounds > 0)
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
    // Update Transpiler：替换抛壳时的物品 ID
    // ============================================================

    // Transpiler：查找 GunScript.Update() 中用于生成弹壳的 ldstr "casing" 指令，
    // 替换为 ldarg.0 + call ResolveCasingItemId(GunScript)，
    // 使得抛壳时生成模板匹配的弹壳物品而非硬编码的 "casing"。
    private static IEnumerable<CodeInstruction> TranspileUpdate(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var resolveMethod = AccessTools.Method(typeof(GunRuntimeManager), nameof(ResolveCasingItemId));

        for (var i = 0; i < codes.Count; i++)
        {
            var code = codes[i];

            // 查找 ldstr "casing"
            if (code.opcode != OpCodes.Ldstr || code.operand is not string str || str != "casing") continue;
            // 防御：确保替换位置的后续指令不会因堆栈变化而出错
            // ldstr "casing" 在堆栈上放置一个 string。
            // 替换为：ldarg.0（this GunScript） + call ResolveCasingItemId(GunScript) → 返回 string。
            // 堆栈结果相同（一个 string），不影响后续指令。
            code.opcode = OpCodes.Ldarg_0;
            code.operand = null;
            codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, resolveMethod));
            break; // 只替换第一处（Update 中抛壳应只有一处）
        }

        return codes;
    }

    // Transpiler 回调：根据当前枪械的 PendingCasingType 返回模板匹配的弹壳物品 ID。
    // 由 Update 的 Transpiler 动态调用。若未找到匹配或 PendingCasingType 为空，返回 "casing" 兜底。
    // 注意：此方法由 Transpiler 注入到 GunScript.Update 中，请勿在此处使用 LogUtil 以避免热路径日志洪水。
    public static string ResolveCasingItemId(GunScript gun)
    {
        if (gun == null) return "casing";

        var item = gun.GetComponent<Item>();
        if (item == null) return "casing";

        var state = GunMagTracker.Get(item);
        if (state == null) return "casing";

        var pending = state.PendingCasingType;
        if (pending == null) return "casing";

        // 通过模板查找匹配的弹壳物品
        var casingIds = CasingTemplate.FindCasingsByType(pending);
        var casingId = casingIds.Count > 0 ? casingIds[0] : null;

        // 消费标记（只消费一次）
        state.PendingCasingType = null;

        return casingId ?? "casing";
    }
}