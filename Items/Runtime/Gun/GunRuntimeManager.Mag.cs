using Bark.Events;
using Bark.Items.Templates;
using Bark.Tool;
using CUCoreLib.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Items.Runtime.Gun;

// Partial：弹匣装卸 + 弹药脚本补丁
public static partial class GunRuntimeManager
{
    // ============================================================
    // AmmoScript.UnloadRound Prefix：自定义弹匣退弹
    // ============================================================
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

    // ============================================================
    // LoadMag Prefix：拦截并覆盖原生的装弹逻辑
    // ============================================================
    // 模板枪械的装弹/卸弹走模板标签匹配，跳过原版枚举类型比较；
    // 非模板枪械走原版流程（前缀返回 true）。

    private static bool OnLoadMagPrefix(GunScript __instance, AmmoScript ammo)
    {
        if (ammo == null) return true;

        var (gunItem, gunData) = TryGetTemplateGun(__instance);
        if (gunItem == null || gunData == null) return true;

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
                LogUtil.Warning("gun_runtime.load_mag_incompatible_mag_type", ammoItemId,
                    MagTemplate.GetMagType(ammoItemId), gunData.MagType);
                return false;
            // 验证 ammo_type 兼容
            case AmmoScript.AmmoItemType.Magazine when MagTemplate.GetAmmoType(ammoItemId) != gunData.AmmoType:
                LogUtil.Warning("gun_runtime.load_mag_incompatible_ammo_type", ammoItemId,
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

                // 播放音效：profile 优先，否则默认 "gunloadmag"
                if (gunData.SoundProfile?.LoadMag is { Count: > 0 })
                    gunData.SoundProfile.PlayRandom(gunData.SoundProfile.LoadMag, __instance.transform.position);
                else
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

                // 播放音效：profile 优先，否则默认 "gunloadshell"（逐发装弹）
                if (gunData.SoundProfile?.LoadShell is { Count: > 0 })
                    gunData.SoundProfile.PlayRandom(gunData.SoundProfile.LoadShell,
                        __instance.transform.position);
                else
                    Sound.Play("gunloadshell", __instance.transform.position);
                Object.Destroy(ammo.gameObject);

                EventUtil.Trigger(new GunLoadAmmoEvent
                {
                    GunItem = gunItem,
                    AmmoItemId = ammoItemId,
                    Rounds = 1
                });

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
        var (gunItem, gunData) = TryGetTemplateGun(__instance);
        if (gunItem == null || gunData == null) return true;

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

            return false;
        }

        // ============================================================
        // 弹匣供弹枪械：卸下弹匣
        // ============================================================
        var magItemId = state?.MagItemId;
        var magRounds = state?.RoundsInMag ?? __instance.roundsInMag;

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
            if (fallbackMagIds.Count > 0)
                magItemId = fallbackMagIds[0];
            else
            {
                var altIds = MagTemplate.FindMagsByAmmoType(gunData.AmmoType);
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

        // 播放音效：profile 优先，否则默认 "gununloadmag"
        if (gunData.SoundProfile?.UnloadMag is { Count: > 0 })
            gunData.SoundProfile.PlayRandom(gunData.SoundProfile.UnloadMag, __instance.transform.position);
        else
            Sound.Play("gununloadmag", __instance.transform.position);

        EventUtil.Trigger(new GunUnloadEvent
        {
            GunItem = gunItem,
            RoundsUnloaded = magRounds
        });

        return false;
    }
}
