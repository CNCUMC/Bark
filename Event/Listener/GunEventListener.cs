using System.Collections;
using System.Collections.Generic;
using Bark.Events;
using Bark.Tool;
using HarmonyLib;
using UnityEngine;

namespace Bark.Event.Listener;

[HarmonyPatch(typeof(GunScript))]
public static class GunEventListener
{
    private const float PollInterval = 0.2f;
    private static readonly Dictionary<GunScript, GunStateSnapshot> GunStates = new();

    private static Coroutine? _jamCoroutine;
    private static MonoBehaviour? _runner;

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;
        _jamCoroutine = runner.StartCoroutine(PollGunJam());
    }

    internal static void Stop()
    {
        if (_runner == null) return;

        if (_jamCoroutine != null)
        {
            _runner.StopCoroutine(_jamCoroutine);
            _jamCoroutine = null;
        }

        GunStates.Clear();
        _runner = null;
    }

    [HarmonyPatch("Fire")]
    [HarmonyPostfix]
    private static void FirePostfix(GunScript __instance, bool suicide)
    {
        var item = __instance.GetComponent<Item>();
        if (item == null) return;

        EventUtil.Trigger(new GunFireEvent
        {
            GunItem = item,
            Suicide = suicide
        });
    }

    [HarmonyPatch("TryRack")]
    [HarmonyPostfix]
    private static void TryRackPostfix(GunScript __instance)
    {
        if (__instance == null) return;
        var item = __instance.GetComponent<Item>();
        if (item == null) return;

        EventUtil.Trigger(new GunRackEvent
        {
            GunItem = item,
            Racked = __instance.racked
        });
    }

    [HarmonyPatch("ToggleSafety")]
    private static void ToggleSafetyPostfix(GunScript __instance)
    {
        if (__instance == null) return;
        var item = __instance.GetComponent<Item>();
        if (item == null) return;

        EventUtil.Trigger(new GunSafetyToggleEvent
        {
            GunItem = item,
            Safe = __instance.safe
        });
    }

    [HarmonyPatch("LoadMag")]
    [HarmonyPrefix]
    private static void LoadMagPrefix(GunScript __instance, out GunLoadState __state)
    {
        __state = new GunLoadState
        {
            HadMag = __instance.hasMag,
            RoundsInMag = __instance.roundsInMag,
            RoundInChamber = __instance.roundInChamber
        };
    }


    [HarmonyPatch("LoadMag")]
    [HarmonyPostfix]
    private static void LoadMagPostfix(GunScript __instance, AmmoScript ammo, GunLoadState __state)
    {
        if (ammo == null) return;

        var stateChanged = __instance.hasMag != __state.HadMag
                           || __instance.roundsInMag != __state.RoundsInMag
                           || __instance.roundInChamber != __state.RoundInChamber;

        if (!stateChanged) return;

        var item = __instance.GetComponent<Item>();
        if (item == null) return;

        var roundsDelta = __instance.roundsInMag - __state.RoundsInMag
                          + (__instance.roundInChamber != __state.RoundInChamber ? 1 : 0);

        EventUtil.Trigger(new GunLoadAmmoEvent
        {
            GunItem = item,
            AmmoItemId = ammo.itemType == AmmoScript.AmmoItemType.Magazine
                ? AmmoScript.AmmoTypeToMagazine(ammo.ammoType)
                : AmmoScript.AmmoTypeToItem(ammo.ammoType),
            Rounds = roundsDelta > 0 ? roundsDelta : ammo.rounds
        });
    }

    [HarmonyPatch("UnloadMag")]
    [HarmonyPostfix]
    private static void UnloadMagPostfix(GunScript __instance)
    {
        if (__instance == null) return;
        var item = __instance.GetComponent<Item>();
        if (item == null) return;

        var roundsToUnload = __instance.roundsInMag;
        if (roundsToUnload <= 0) return;

        EventUtil.Trigger(new GunUnloadEvent
        {
            GunItem = item,
            RoundsUnloaded = roundsToUnload
        });
    }

    private static IEnumerator PollGunJam()
    {
        yield return new WaitForSeconds(1f);

        while (_jamCoroutine != null)
        {
            yield return new WaitForSeconds(PollInterval);

            var allGuns = Object.FindObjectsOfType<GunScript>();
            foreach (var gun in allGuns)
            {
                if (!gun || !gun.GetComponent<Item>()) continue;
                PollGunForJam(gun);
            }
        }
    }

    private static void PollGunForJam(GunScript gun)
    {
        GunStates.TryGetValue(gun, out var prev);

        var current = new GunStateSnapshot(gun);

        if (prev == null)
        {
            GunStates[gun] = current;
            return;
        }

        switch (prev.Racked)
        {
            case false when current.Racked && prev.RoundInChamber == current.RoundInChamber:
            {
                if (prev.RoundInChamber != GunScript.RoundInChamber.None) TriggerJam(gun);

                break;
            }
            case true when current is { Racked: false, HasMag: true, RoundsInMag: > 0 }
                           && prev.RoundInChamber == GunScript.RoundInChamber.None
                           && current.RoundInChamber == GunScript.RoundInChamber.None:
                TriggerJam(gun);
                break;
        }

        GunStates[gun] = current;
    }

    private static void TriggerJam(GunScript gun)
    {
        var item = gun.GetComponent<Item>();
        if (!item) return;

        EventUtil.Trigger(new GunJamEvent
        {
            GunItem = item
        });
    }

    private class GunLoadState
    {
        public bool HadMag;
        public GunScript.RoundInChamber RoundInChamber;
        public int RoundsInMag;
    }

    private class GunStateSnapshot(GunScript gun)
    {
        public readonly bool HasMag = gun.hasMag;
        public readonly bool Racked = gun.racked;
        public readonly GunScript.RoundInChamber RoundInChamber = gun.roundInChamber;
        public readonly int RoundsInMag = gun.roundsInMag;
    }
}