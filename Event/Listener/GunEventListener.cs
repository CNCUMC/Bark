using System.Collections;
using System.Collections.Generic;
using Bark.Events;
using Bark.Tool;
using HarmonyLib;
using UnityEngine;

namespace Bark.Event.Listener;

// 枪械事件监听器：通过 Harmony 补丁拦截 GunScript 方法，
// 触发 GunFireEvent / GunRackEvent / GunSafetyToggleEvent / GunLoadAmmoEvent / GunUnloadEvent / GunJamEvent。
public static class GunEventListener
{
    // 轮询间隔（秒）
    private const float PollInterval = 0.2f;

    // 卡壳检测：追踪每个 GunScript 实例的上一帧 racked, roundInChamber, roundsInMag 状态
    private static readonly Dictionary<GunScript, GunStateSnapshot> GunStates = new();

    private static Coroutine? _jamCoroutine;
    private static MonoBehaviour? _runner;

    internal static void Listen(MonoBehaviour runner)
    {
        _runner = runner;

        TryPatchMethod("Fire", nameof(OnFire));
        TryPatchMethod("TryRack", nameof(OnTryRack));
        TryPatchMethod("ToggleSafety", nameof(OnToggleSafety));
        TryPatchMethod("LoadMag", nameof(OnLoadMagPrefix), nameof(OnLoadMagPostfix));
        TryPatchMethod("UnloadMag", nameof(OnUnloadMag));

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

    // ============================================================
    // Harmony 补丁
    // ============================================================

    private static void TryPatchMethod(string methodName, string prefix, string? postfix = null)
    {
        var method = AccessTools.Method(typeof(GunScript), methodName);
        if (method == null) return;

        try
        {
            var harmony = new Harmony($"Bark.GunEventListener.{methodName}");
            var prefixMethod = new HarmonyMethod(typeof(GunEventListener), prefix);
            var postfixMethod = postfix != null
                ? new HarmonyMethod(typeof(GunEventListener), postfix)
                : null;

            harmony.Patch(method, prefixMethod, postfixMethod);
            LogUtil.Info("gun_event.patch_ok", $"GunScript.{methodName}");
        }
        catch
        {
            // ignored
        }
    }

    // ============================================================
    // 开火
    // ============================================================

    private static void OnFire(GunScript __instance, bool suicide)
    {
        var item = __instance.GetComponent<Item>();
        if (item == null) return;

        EventUtil.Trigger(new GunFireEvent
        {
            GunItem = item,
            Suicide = suicide
        });
    }

    // ============================================================
    // 拉栓
    // ============================================================

    private static void OnTryRack(GunScript __instance)
    {
        if (__instance == null) return;
        var item = __instance.GetComponent<Item>();
        if (item == null) return;

        // TryRack() 只是 toggle racked，实际拉栓 / 复位在 Update 中处理
        EventUtil.Trigger(new GunRackEvent
        {
            GunItem = item,
            Racked = __instance.racked
        });
    }

    // ============================================================
    // 保险
    // ============================================================

    private static void OnToggleSafety(GunScript __instance)
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

    // ============================================================
    // 装弹（prefix 捕获 ammo 信息，postfix 判断是否成功）
    // ============================================================

    private static void OnLoadMagPrefix(GunScript __instance, out GunLoadState __state)
    {
        __state = new GunLoadState
        {
            HadMag = __instance.hasMag,
            RoundsInMag = __instance.roundsInMag,
            RoundInChamber = __instance.roundInChamber
        };
    }

    private static void OnLoadMagPostfix(GunScript __instance, AmmoScript ammo, GunLoadState __state)
    {
        if (ammo == null) return;

        // 判断是否成功装填：弹药物品已被销毁（== null）或者状态发生了变化
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

    // 装弹状态快照（prefix → postfix）
    private class GunLoadState
    {
        public bool HadMag;
        public int RoundsInMag;
        public GunScript.RoundInChamber RoundInChamber;
    }

    // ============================================================
    // 卸弹
    // ============================================================

    private static void OnUnloadMag(GunScript __instance)
    {
        if (__instance == null) return;
        var item = __instance.GetComponent<Item>();
        if (item == null) return;

        // UnloadMag() 中 Sound.Play("gununloadmag") 在弹匣卸下之前调用，
        // 此时 roundsInMag 还未归零 → 用 prefix 在调用前就已触发
        // 所以这里 roundsInMag 仍保持卸弹前的值
        var roundsToUnload = __instance.roundsInMag;
        if (roundsToUnload <= 0) return;

        EventUtil.Trigger(new GunUnloadEvent
        {
            GunItem = item,
            RoundsUnloaded = roundsToUnload
        });
    }

    // ============================================================
    // 卡壳轮询：检测 racked / roundInChamber 是否按预期变化
    // ============================================================

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
            // racked 从 false → true：拉栓动作，弹膛中的弹药应被抛出
            // 若拉栓后 roundInChamber 没变化 → 卡壳
            case false when current.Racked && prev.RoundInChamber == current.RoundInChamber:
            {
                // 拉栓时弹膛非空但未抛出 → 卡壳
                if (prev.RoundInChamber != GunScript.RoundInChamber.None)
                {
                    TriggerJam(gun);
                }

                break;
            }
            // racked 从 true → false：枪栓复位，应从弹匣推入一发
            // 若有弹匣且有子弹但弹膛仍为空 → 卡壳
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

    // 枪械状态快照（每帧比较用）
    private class GunStateSnapshot(GunScript gun)
    {
        public readonly bool Racked = gun.racked;
        public readonly bool HasMag = gun.hasMag;
        public readonly int RoundsInMag = gun.roundsInMag;
        public readonly GunScript.RoundInChamber RoundInChamber = gun.roundInChamber;
    }
}
