using System;
using System.Diagnostics.CodeAnalysis;
using Bark.Audio;
using Bark.Items.Templates;
using Bark.Tool;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Items.Runtime.Gun;

public static partial class GunRuntimeManager
{
    private static void OnGunStartPostfix(GunScript __instance)
    {
        var item = __instance.GetComponent<Item>();
        if (item == null) return;
        if (!GunTemplate.IsGun(item.id)) return;

        var gunData = GunTemplate.GetGunData(item.id);
        if (gunData == null) return;

        ApplyFiringConfig(__instance, gunData);
        MapAmmoType(__instance, gunData);
        ApplyBallisticsData(__instance, gunData);
        ApplySpriteFields(__instance, item);
        ApplySoundFields(__instance, gunData);
        InitMagazineState(__instance, item, gunData);
        ApplyChamberInitState(__instance, gunData);
        EnsureMuzzleParticle(__instance);
    }

    private static void ApplyFiringConfig(GunScript gun, GunData gunData)
    {
        gun.firingMode = gunData.FiringMode switch
        {
            "auto" => GunScript.FiringMode.Auto,
            "pump" => GunScript.FiringMode.Pump,
            _ => GunScript.FiringMode.SemiAuto
        };

        gun.feedType = gunData.FeedType switch
        {
            "direct" => GunScript.FeedType.Direct,
            "revolver" => (GunScript.FeedType)2,
            _ => gunData.Direct
                ? GunScript.FeedType.Direct
                : GunScript.FeedType.Mag
        };
    }

    private static void MapAmmoType(GunScript gun, GunData gunData)
    {
        gun.ammoType = ParseGunType(gunData.GunType);
    }

    private static GunScript.AmmoType ParseGunType(string gunType)
    {
        return Enum.TryParse<GunScript.AmmoType>(gunType, true, out var parsed)
            ? parsed
            : GunScript.AmmoType.Rifle;
    }

    private static void ApplyBallisticsData(GunScript gun, GunData gunData)
    {
        gun.knockBack = gunData.Knockback;
        gun.structureDamage = gunData.StructureDamage;
        gun.animalDamage = gunData.AnimalDamage;
        gun.loudness = gunData.Loudness;
        gun.shotsPerFire = gunData.ShotsPerFire;
        gun.verticalSpread = gunData.VerticalSpread;

        gun.conditionLossPerShot = gunData.ConditionLossPerShot / 0.01f;

        if (gunData is { Direct: true, Capacity: > 0 })
            gun.magCapacity = gunData.Capacity;
    }

    private static void ApplySpriteFields(GunScript gun, Item item)
    {
        gun.racked = true;

        if (gun.barrel == null)
        {
            var (bx, by) = GetBarrelOffset(gun);
            CreateBarrelChild(gun.gameObject, gun, bx, by);
        }

        var sr = gun.GetComponent<SpriteRenderer>();
        if (sr?.sprite != null)
        {
            gun.normalSprite = sr.sprite;
            gun.rackedSprite = sr.sprite;
            gun.normalSpriteNoMag = sr.sprite;
            gun.rackedSpriteNoMag = sr.sprite;
        }
        else
        {
            var fallbackSprite = LoadDefaultGunSprite(gun.ammoType);
            if (fallbackSprite != null)
            {
                gun.normalSprite = fallbackSprite;
                gun.rackedSprite = fallbackSprite;
                gun.normalSpriteNoMag = fallbackSprite;
                gun.rackedSpriteNoMag = fallbackSprite;
            }
        }

        if (gun.normalSprite == null)
        {
            LogUtil.Warning("gun_runtime.gun_init_no_sprite", item.id);
            var tex = new Texture2D(4, 4, TextureFormat.ARGB32, false)
                { hideFlags = HideFlags.DontSave };
            var pixels = new Color32[16];
            for (var i = 0; i < 16; i++) pixels[i] = new Color32(64, 64, 64, 255);
            tex.SetPixels32(pixels);
            tex.Apply();
            gun.normalSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4),
                new Vector2(0.5f, 0.5f), 100f);
            gun.rackedSprite = gun.normalSprite;
            gun.normalSpriteNoMag = gun.normalSprite;
            gun.rackedSpriteNoMag = gun.normalSprite;
        }
    }

    [SuppressMessage("ReSharper", "Unity.UnknownResource")]
    private static void ApplySoundFields(GunScript gun, GunData gunData)
    {
        if (gunData.SoundProfile?.Fire is { Count: > 0 })
            gun.fireSound = GetSilentClip();
        else if (!string.IsNullOrEmpty(gunData.FireSound))
            gun.fireSound = AudioManager.LoadModAudio(gunData.ModDir, gunData.FireSound);
        if (gun.fireSound == null)
            gun.fireSound = gun.ammoType switch
            {
                GunScript.AmmoType.Shotgun => Resources.Load<AudioClip>(
                    "sounds/shotgunshot"),
                GunScript.AmmoType.Rifle => Resources.Load<AudioClip>("sounds/rifleshot")
                    ?? Resources.Load<AudioClip>("sounds/shotgunshot"),
                _ => Resources.Load<AudioClip>("sounds/pistolshot")
            };

        if (gunData.SoundProfile?.Rack is { Count: > 0 })
            gun.customRack =
                gunData.SoundProfile.GetRandomClip(gunData.SoundProfile.Rack);
        else if (!string.IsNullOrEmpty(gunData.RackSound))
            gun.customRack = AudioManager.LoadModAudio(gunData.ModDir, gunData.RackSound);

        if (gunData.SoundProfile?.Unrack is { Count: > 0 })
            gun.customUnrack =
                gunData.SoundProfile.GetRandomClip(gunData.SoundProfile.Unrack);
        else if (!string.IsNullOrEmpty(gunData.UnrackSound))
            gun.customUnrack =
                AudioManager.LoadModAudio(gunData.ModDir, gunData.UnrackSound);
        else if (gun.customRack != null) gun.customUnrack = gun.customRack;
    }

    private static void InitMagazineState(GunScript gun, Item item, GunData gunData)
    {
        if (!gunData.Direct && gunData.FeedType != "revolver")
        {
            var state = GunMagTracker.GetOrCreate(item);
            state.RoundsInMag = 0;

            var defaultMagIds = MagTemplate.FindMagsByType(gunData.MagType);
            if (defaultMagIds.Count == 0)
            {
                if (!string.IsNullOrEmpty(gunData.MagType))
                    LogUtil.Warning("gun_runtime.gun_init_no_mag_by_type", item.id,
                        gunData.MagType, gunData.AmmoType);
                defaultMagIds = MagTemplate.FindMagsByAmmoType(gunData.AmmoType);
            }

            if (defaultMagIds.Count == 0)
            {
                var cap = gunData.Capacity > 0
                    ? gunData.Capacity
                    : DefaultDirectCapacity;
                gun.magCapacity = cap;
                gun.hasMag = false;
                gun.roundsInMag = 0;
                state.MagItemId = null;
                return;
            }

            gun.magCapacity = 0;
            gun.hasMag = false;
            gun.roundsInMag = 0;

            var foundMagData = MagTemplate.GetMagData(defaultMagIds[0]);
            if (foundMagData != null && foundMagData.MagType != gunData.MagType)
                LogUtil.Warning("gun_runtime.gun_init_mag_type_mismatch", item.id,
                    gunData.MagType,
                    defaultMagIds[0], foundMagData.MagType, gunData.MagType);

            state.MagItemId = defaultMagIds[0];
        }
        else if (gunData.FeedType == "revolver")
        {
            var cap = gunData.Capacity > 0
                ? gunData.Capacity
                : 6;
            gun.magCapacity = cap;
            gun.hasMag = false;
            gun.roundsInMag = cap;
        }
        else
        {
            var cap = gunData.Capacity > 0
                ? gunData.Capacity
                : DefaultDirectCapacity;
            gun.magCapacity = cap;
        }
    }

    private static void ApplyChamberInitState(GunScript gun, GunData gunData)
    {
        gun.roundInChamber = gunData.StartChambered
            ? GunScript.RoundInChamber.Round
            : GunScript.RoundInChamber.None;
        gun.safe = gunData.StartSafe;
        gun.lastRacked = false;
        gun.desiredGasTime = gunData.DesiredGasTime;
    }

    private static void EnsureMuzzleParticle(GunScript gun)
    {
        if (gun.muzzleParticle == null)
            gun.muzzleParticle = CloneMuzzleFromPrefab(gun.ammoType, gun.transform);
    }

    private static ParticleSystem CloneMuzzleFromPrefab(GunScript.AmmoType ammoType,
        Transform parent)
    {
        try
        {
            var prefabName = ammoType switch
            {
                GunScript.AmmoType.Shotgun => "shotgun",
                GunScript.AmmoType.Rifle => "rifle",
                _ => "pistol"
            };
            var prefab = Resources.Load(prefabName) as GameObject;
            if (prefab == null) goto fallback;

            var childIdx = ammoType == GunScript.AmmoType.Shotgun
                ? 1
                : 2;
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
        var fallbackPs = parent.gameObject.AddComponent<ParticleSystem>();
        fallbackPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var emission = fallbackPs.emission;
        emission.enabled = false;
        return fallbackPs;
    }

    private static Sprite? LoadDefaultGunSprite(GunScript.AmmoType ammoType)
    {
        var prefabName = ammoType switch
        {
            GunScript.AmmoType.Shotgun => "shotgun",
            GunScript.AmmoType.Rifle => "rifle",
            _ => "pistol"
        };
        var prefab = Resources.Load(prefabName) as GameObject;
        return prefab?.GetComponent<SpriteRenderer>()?.sprite;
    }

    private static bool OnHandleGunMenuPrefix(PlayerCamera __instance)
    {
        if (__instance == null) return false;

        try
        {
            var pc = Traverse.Create(__instance);

            var body = pc.Field("body").GetValue();
            if (body == null) return false;

            var bodyTr = Traverse.Create(body);

            var handSlot = bodyTr.Field("handSlot").GetValue<int>();
            if (!bodyTr.Method("HoldingItem", handSlot).GetValue<bool>())
                return true;

            var item = bodyTr.Method("GetItem", handSlot).GetValue();
            if (item == null) return false;

            var stats = Traverse.Create(item).Property("Stats").GetValue();
            if (stats == null) return false;
            if (!Traverse.Create(stats).Method("HasTag", "gun").GetValue<bool>())
                return true;

            var gun = ((Component)item).GetComponent<GunScript>();
            if (gun == null)
            {
                LogUtil.Warning("gun_runtime.handle_gun_menu_null_gunscript");
                return false;
            }

            if (gun.barrel == null)
            {
                LogUtil.Warning("gun_runtime.handle_gun_menu_null_barrel");
                return false;
            }

            return CheckPlayerCameraUIFields(pc);
        }
        catch (Exception ex)
        {
            LogUtil.Warning("gun_runtime.handle_gun_menu_prefix_error", ex.GetType().Name,
                ex.Message);
            return false;
        }
    }

    private static bool CheckPlayerCameraUIFields(Traverse pc)
    {
        if (pc.Field("gunRackImage").GetValue() == null)
        {
            Log("gunRackImage");
            return false;
        }

        if (pc.Field("gunRackedSprite").GetValue() == null)
        {
            Log("gunRackedSprite");
            return false;
        }

        if (pc.Field("gunNormalSprite").GetValue() == null)
        {
            Log("gunNormalSprite");
            return false;
        }

        if (pc.Field("gunMagButton").GetValue() == null)
        {
            Log("gunMagButton");
            return false;
        }

        if (pc.Field("gunSafeImage").GetValue() == null)
        {
            Log("gunSafeImage");
            return false;
        }

        if (pc.Field("gunSafeSprite").GetValue() == null)
        {
            Log("gunSafeSprite");
            return false;
        }

        if (pc.Field("gunUnsafeSprite").GetValue() == null)
        {
            Log("gunUnsafeSprite");
            return false;
        }

        if (pc.Field("gunBulletImage").GetValue() == null)
        {
            Log("gunBulletImage");
            return false;
        }

        if (pc.Field("gunBulletSprites").GetValue() == null)
        {
            Log("gunBulletSprites");
            return false;
        }

        if (pc.Field("gunCrosshair").GetValue() != null) return true;
        Log("gunCrosshair");
        return false;

        static void Log(string field)
        {
            LogUtil.Warning("gun_runtime.handle_gun_menu_null_pc_field", field);
        }
    }

    private static void OnTemplateCreatedPostfix(GameObject __result, string id)
    {
        if (__result == null)
            return;

        var item = __result.GetComponent<Item>();
        var itemId = item is not null
            ? item.id
            : id;
        if (string.IsNullOrEmpty(itemId))
            return;

        if (GunTemplate.IsGun(itemId) && __result.GetComponent<GunScript>() == null)
        {
            var gun = __result.AddComponent<GunScript>();
            var (bx, by) = GetBarrelOffset(gun);
            CreateBarrelChild(__result, gun, bx, by);
        }

        if (MagTemplate.IsMag(itemId) && __result.GetComponent<AmmoScript>() == null)
        {
            var magData = MagTemplate.GetMagData(itemId);
            var am = __result.AddComponent<AmmoScript>();
            am.itemType = AmmoScript.AmmoItemType.Magazine;
            if (magData != null)
            {
                am.maxRounds = magData.Capacity;
                am.rounds = magData.Capacity;
                am.ammoType = Enum.TryParse<GunScript.AmmoType>(magData.AmmoType, true,
                    out var parsedMagAmmo)
                    ? parsedMagAmmo
                    : GunScript.AmmoType.Pistol;
            }
        }

        if (AmmunitionTemplate.IsAmmo(itemId)
            && __result.GetComponent<AmmoScript>() == null)
        {
            var am = __result.AddComponent<AmmoScript>();
            am.itemType = AmmoScript.AmmoItemType.Round;
        }
    }

    private static void CreateBarrelChild(GameObject gunObj, GunScript gun, float offsetX,
        float offsetY)
    {
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

    private static (float x, float y) GetBarrelOffset(Component? component)
    {
        var item = component?.GetComponent<Item>();
        if (item is null) return (0.5f, 0f);

        var gunData = GunTemplate.GetGunData(item.id);
        if (gunData is null) return (0.5f, 0f);

        return (gunData.BarrelOffsetX, gunData.BarrelOffsetY);
    }
}
