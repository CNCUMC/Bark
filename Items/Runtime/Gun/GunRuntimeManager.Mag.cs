using Bark.Events;
using Bark.Items.Templates;
using Bark.Tool;
using CUCoreLib.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Items.Runtime.Gun;

public static partial class GunRuntimeManager
{
    private static bool OnUnloadRoundPrefix(AmmoScript __instance)
    {
        if (__instance.itemType != AmmoScript.AmmoItemType.Magazine) return true;
        if (__instance.rounds <= 0) return false;

        var item = __instance.GetComponent<Item>();
        if (item == null || !MagTemplate.IsMag(item.id)) return true;

        var magData = MagTemplate.GetMagData(item.id);
        if (magData == null) return true;

        var ammoIds = AmmunitionTemplate.FindAmmoByType(magData.AmmoType);
        if (ammoIds.Count == 0) return true;

        var ammoItemId = ammoIds[0];
        var spawned =
            CustomInstantiate.InstantiateReturn(ammoItemId, __instance.transform.position,
                Quaternion.identity);
        if (spawned == null) return true;

        PlayerCamera.main.body.AutoPickUpItem(spawned.GetComponent<Item>());
        Sound.Play("gunloadshell", __instance.transform.position);
        --__instance.rounds;

        return false;
    }

    private static bool OnLoadRoundPrefix(AmmoScript __instance, AmmoScript ammo)
    {
        if (__instance.itemType != AmmoScript.AmmoItemType.Magazine) return true;
        if (ammo.itemType != AmmoScript.AmmoItemType.Round) return true;

        var magItem = __instance.GetComponent<Item>();
        if (magItem == null || !MagTemplate.IsMag(magItem.id)) return true;

        var ammoItem = ammo.GetComponent<Item>();
        if (ammoItem == null) return false;
        if (!AmmunitionTemplate.IsAmmo(ammoItem.id)) return false;

        var magAmmoType = MagTemplate.GetAmmoType(magItem.id);
        var ammoType = AmmunitionTemplate.GetAmmoType(ammoItem.id);
        if (magAmmoType != ammoType) return false;

        if (__instance.rounds >= __instance.maxRounds) return false;

        ++__instance.rounds;
        Sound.Play("gunloadshell", __instance.transform.position);
        Object.Destroy(ammo.gameObject);

        return false;
    }

    private static bool OnLoadMagPrefix(GunScript __instance, AmmoScript ammo)
    {
        if (ammo == null) return true;

        var (gunItem, gunData) = TryGetTemplateGun(__instance);
        if (gunItem == null || gunData == null) return true;

        var ammoItem = ammo.GetComponent<Item>();
        var ammoItemId = ammoItem?.id;

        switch (ammo.itemType)
        {
            case AmmoScript.AmmoItemType.Magazine when string.IsNullOrEmpty(ammoItemId):
            case AmmoScript.AmmoItemType.Magazine when !MagTemplate.IsMag(ammoItemId):
                return true;
            case AmmoScript.AmmoItemType.Magazine
                when MagTemplate.GetMagType(ammoItemId) != gunData.MagType:
                LogUtil.Warning("gun_runtime.load_mag_incompatible_mag_type", ammoItemId,
                    MagTemplate.GetMagType(ammoItemId), gunData.MagType);
                return false;
            case AmmoScript.AmmoItemType.Magazine
                when MagTemplate.GetAmmoType(ammoItemId) != gunData.AmmoType:
                LogUtil.Warning("gun_runtime.load_mag_incompatible_ammo_type", ammoItemId,
                    MagTemplate.GetAmmoType(ammoItemId), gunData.AmmoType);
                return false;
            case AmmoScript.AmmoItemType.Magazine:
            {
                var capacity = MagTemplate.GetCapacity(ammoItemId);
                var rounds = ammo.rounds;
                if (rounds > capacity) rounds = capacity;

                __instance.hasMag = true;
                __instance.magCapacity = capacity;
                __instance.roundsInMag = rounds;

                var state = GunMagTracker.GetOrCreate(gunItem);
                state.MagItemId = ammoItemId;
                state.RoundsInMag = rounds;
                state.AmmoItemId = null;

                if (gunData.SoundProfile?.LoadMag is { Count: > 0 })
                    gunData.SoundProfile.PlayRandom(gunData.SoundProfile.LoadMag,
                        __instance.transform.position);
                else
                    Sound.Play("gunloadmag", __instance.transform.position);

                Object.Destroy(ammo.gameObject);

                EventUtil.Trigger(new GunLoadAmmoEvent
                {
                    GunItem = gunItem,
                    AmmoItemId = ammoItemId,
                    Rounds = rounds
                });

                return false;
            }
            case AmmoScript.AmmoItemType.Round when !gunData.Direct:
                return false;
            case AmmoScript.AmmoItemType.Round when string.IsNullOrEmpty(ammoItemId):
            case AmmoScript.AmmoItemType.Round
                when !AmmunitionTemplate.IsAmmo(ammoItemId):
                return true;
            case AmmoScript.AmmoItemType.Round
                when AmmunitionTemplate.GetAmmoType(ammoItemId) != gunData.AmmoType:
                LogUtil.Warning("gun_runtime.incompatible_ammo_type",
                    AmmunitionTemplate.GetAmmoType(ammoItemId), gunData.AmmoType);
                return false;
            case AmmoScript.AmmoItemType.Round:
            {
                var state = GunMagTracker.GetOrCreate(gunItem);
                var capacity = gunData.Capacity > 0
                    ? gunData.Capacity
                    : DefaultDirectCapacity;

                if (state.RoundsInMag >= capacity)
                    return false;

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

    private static bool OnUnloadMagPrefix(GunScript __instance)
    {
        var (gunItem, gunData) = TryGetTemplateGun(__instance);
        if (gunItem == null || gunData == null) return true;

        var state = GunMagTracker.Get(gunItem);

        if (gunData.Direct)
        {
            var roundsToUnload = state?.RoundsInMag ?? __instance.roundsInMag;
            var ammoItemId = state?.AmmoItemId;

            if (roundsToUnload > 0 && !string.IsNullOrEmpty(ammoItemId))
            {
                var body = __instance.GetComponentInParent<Body>();
                if (body != null)
                {
                    var go = Utils.Create(ammoItemId, body.transform.position, 0f);
                    if (go != null)
                    {
                        var ammoComp = go.GetComponent<AmmoScript>();
                        if (ammoComp != null) ammoComp.rounds = roundsToUnload;
                        body.AutoPickUpItem(go.GetComponent<Item>());
                    }
                }
            }

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

        var magItemId = state?.MagItemId;
        var magRounds = state?.RoundsInMag ?? __instance.roundsInMag;

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

        if (string.IsNullOrEmpty(magItemId)
            && __instance is { hasMag: true, roundsInMag: > 0 })
        {
            var fallbackMagIds = MagTemplate.FindMagsByType(gunData.MagType);
            if (fallbackMagIds.Count > 0)
            {
                magItemId = fallbackMagIds[0];
            }
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

        __instance.hasMag = false;
        __instance.roundsInMag = 0;
        GunMagTracker.Remove(gunItem);

        if (gunData.SoundProfile?.UnloadMag is { Count: > 0 })
            gunData.SoundProfile.PlayRandom(gunData.SoundProfile.UnloadMag,
                __instance.transform.position);
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
