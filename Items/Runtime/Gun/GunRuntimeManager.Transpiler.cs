using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Bark.Items.Templates;
using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Items.Runtime.Gun;

public static partial class GunRuntimeManager
{
    private static IEnumerable<CodeInstruction> TranspileUpdate(
        IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var doSpawnMethod =
            AccessTools.Method(typeof(GunRuntimeManager), nameof(DoSpawnCasing));

        var casingIdx = -1;
        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode != OpCodes.Ldstr
                || codes[i].operand is not string s
                || s != "casing") continue;
            casingIdx = i;
            break;
        }

        if (casingIdx < 0) return codes;

        var instantiateIdx = -1;
        for (var i = casingIdx + 1; i < codes.Count; i++)
        {
            var op = codes[i].opcode;
            if (op != OpCodes.Call && op != OpCodes.Callvirt) continue;
            if (codes[i].operand is not MethodBase m) continue;
            if (m.Name != "Instantiate" || m.DeclaringType != typeof(Object)) continue;
            instantiateIdx = i;
            break;
        }

        if (instantiateIdx < 0) return codes;

        var postInstantiateIdx = instantiateIdx + 1;
        if (postInstantiateIdx >= codes.Count) return codes;
        var postInstantiateLabel = new Label();
        codes[postInstantiateIdx].labels.Add(postInstantiateLabel);

        var casingLabels = codes[casingIdx].labels;
        var newHead = new CodeInstruction(OpCodes.Ldarg_0);
        newHead.labels.AddRange(casingLabels);
        codes[casingIdx] = newHead;
        codes.Insert(casingIdx + 1, new CodeInstruction(OpCodes.Call, doSpawnMethod));
        codes.Insert(casingIdx + 2,
            new CodeInstruction(OpCodes.Br, postInstantiateLabel));

        var doRoundMethod =
            AccessTools.Method(typeof(GunRuntimeManager), nameof(DoSpawnRound));
        var ammoIdx = -1;
        for (var i = casingIdx - 1; i >= 0; i--)
        {
            if (codes[i].opcode != OpCodes.Call
                && codes[i].opcode != OpCodes.Callvirt) continue;
            if (codes[i].operand is not MethodBase am) continue;
            if (am.Name != "AmmoTypeToItem") continue;
            ammoIdx = i;
            break;
        }

        if (doRoundMethod != null && ammoIdx > 0)
        {
            var mergeBrIdx = ammoIdx + 1;
            if (mergeBrIdx < codes.Count
                && (codes[mergeBrIdx].opcode == OpCodes.Br
                    || codes[mergeBrIdx].opcode == OpCodes.Br_S))
                codes[mergeBrIdx] = new CodeInstruction(OpCodes.Br, postInstantiateLabel);

            codes[ammoIdx] = new CodeInstruction(OpCodes.Call, doRoundMethod);

            if (codes[ammoIdx - 1].opcode == OpCodes.Ldfld)
                codes.RemoveAt(ammoIdx - 1);
        }

        var doTriggerMethod = typeof(GunRuntimeManager).GetMethod(
            nameof(DoPlayTriggerSound),
            BindingFlags.NonPublic | BindingFlags.Static);
        var doJamMethod = typeof(GunRuntimeManager).GetMethod(nameof(DoPlayJamSound),
            BindingFlags.NonPublic | BindingFlags.Static);

        if (doTriggerMethod == null || doJamMethod == null) return codes;
        {
            for (var i = codes.Count - 1; i >= 0; i--)
            {
                if (codes[i].opcode != OpCodes.Ldstr
                    || codes[i].operand is not string str)
                    continue;

                var targetMethod = str switch
                {
                    "guntrigger" => doTriggerMethod,
                    "gunjam" => doJamMethod,
                    _ => null
                };
                if (targetMethod == null) continue;

                var playIdx = -1;
                for (var j = i + 1; j < Math.Min(i + 12, codes.Count); j++)
                {
                    if ((codes[j].opcode != OpCodes.Call
                            && codes[j].opcode != OpCodes.Callvirt)
                        || codes[j].operand is not MethodInfo m
                        || m.DeclaringType?.Name != "Sound"
                        || m.Name != "Play") continue;
                    playIdx = j;
                    break;
                }

                if (playIdx <= i) continue;
                var range = playIdx - i + 1;
                codes.RemoveRange(i, range);
                codes.InsertRange(i, [
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, targetMethod)
                ]);
            }
        }

        return codes;
    }

    private static IEnumerable<CodeInstruction> TranspileFire(
        IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var doJamMethod = typeof(GunRuntimeManager).GetMethod(nameof(DoPlayJamSound),
            BindingFlags.NonPublic | BindingFlags.Static);
        if (doJamMethod == null) return codes;

        for (var i = codes.Count - 1; i >= 0; i--)
        {
            if (codes[i].opcode != OpCodes.Ldstr || codes[i].operand is not "gunjam")
                continue;

            var playIdx = -1;
            for (var j = i + 1; j < Math.Min(i + 12, codes.Count); j++)
            {
                if ((codes[j].opcode != OpCodes.Call
                        && codes[j].opcode != OpCodes.Callvirt)
                    || codes[j].operand is not MethodInfo m
                    || m.DeclaringType?.Name != "Sound"
                    || m.Name != "Play") continue;
                playIdx = j;
                break;
            }

            if (playIdx <= i) continue;
            var range = playIdx - i + 1;
            codes.RemoveRange(i, range);
            codes.InsertRange(i, [
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, doJamMethod)
            ]);
        }

        return codes;
    }

    private static Object? DoSpawnCasing(GunScript gun)
    {
        if (gun == null) return null;

        var item = gun.GetComponent<Item>();
        if (item == null) return null;

        string? casingId = null;
        var state = GunMagTracker.Get(item);
        if (state?.PendingCasingType != null)
        {
            var casingIds = CasingTemplate.FindCasingsByType(state.PendingCasingType);
            casingId = casingIds.Count > 0
                ? casingIds[0]
                : null;
            state.PendingCasingType = null;
        }

        if (casingId != null)
        {
            var go = Utils.Create(casingId, gun.transform.position,
                gun.transform.rotation.eulerAngles.z);
            if (go != null) return go;
        }

        var prefab = Resources.Load("casing");
        return prefab != null
            ? Object.Instantiate(prefab, gun.transform.position, gun.transform.rotation)
            : null;
    }

    private static Object? DoSpawnRound(GunScript gun)
    {
        if (gun == null) return null;

        var item = gun.GetComponent<Item>();
        if (item == null) return null;

        var (_, gunData) = TryGetTemplateGun(gun);
        if (gunData != null)
        {
            var ammoIds = AmmunitionTemplate.FindAmmoByType(gunData.AmmoType);
            if (ammoIds.Count > 0)
            {
                var go = CustomInstantiate.InstantiateReturn(
                    ammoIds[0], gun.transform.position, Quaternion.identity);
                if (go != null) return go;
            }
        }

        var prefab = Resources.Load(AmmoScript.AmmoTypeToItem(gun.ammoType));
        return prefab != null
            ? Object.Instantiate(prefab, gun.transform.position, gun.transform.rotation)
            : null;
    }
}
