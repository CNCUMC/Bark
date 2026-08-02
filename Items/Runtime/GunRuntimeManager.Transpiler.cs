using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Bark.Items.Templates;
using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Items.Runtime;

// Partial：IL Transpiler — GunScript.Update/Fire 的 IL 织入
public static partial class GunRuntimeManager
{
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
            if (codes[i].opcode != OpCodes.Ldstr || codes[i].operand is not string s || s != "casing") continue;
            casingIdx = i;
            break;
        }

        if (casingIdx < 0) return codes;

        // Step 2: 确认下一条是 br/br.s（跳过 false 分支直达 ternary 汇合点）
        var brIdx = casingIdx + 1;
        if (brIdx >= codes.Count || codes[brIdx].opcode != OpCodes.Br && codes[brIdx].opcode != OpCodes.Br_S)
            return codes;

        // Step 3: 找到 call Object.Instantiate（在 Resources.Load + pos/rot args 之后）
        var instantiateIdx = -1;
        for (var i = brIdx + 1; i < codes.Count; i++)
        {
            var op = codes[i].opcode;
            if (op != OpCodes.Call && op != OpCodes.Callvirt) continue;
            if (codes[i].operand is not MethodBase m) continue;
            if (m.Name != "Instantiate" || m.DeclaringType != typeof(Object)) continue;
            instantiateIdx = i;
            break;
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

        // --- Stage 2: 替换硬编码的 trigger/jam 音效 ---
        // GunScript.Update() 中有：
        //   1 个 ldstr "guntrigger" → Sound.Play(string, Vector2)
        //   2 个 ldstr "gunjam"     → Sound.Play(string, Vector2, bool)
        // 用 DoPlayTriggerSound / DoPlayJamSound 回调替换，使 SoundProfile 字段生效。
        var doTriggerMethod = typeof(GunRuntimeManager).GetMethod(nameof(DoPlayTriggerSound),
            BindingFlags.NonPublic | BindingFlags.Static);
        var doJamMethod = typeof(GunRuntimeManager).GetMethod(nameof(DoPlayJamSound),
            BindingFlags.NonPublic | BindingFlags.Static);

        if (doTriggerMethod == null || doJamMethod == null) return codes;
        {
            for (var i = codes.Count - 1; i >= 0; i--)
            {
                if (codes[i].opcode != OpCodes.Ldstr || codes[i].operand is not string str)
                    continue;

                var targetMethod = str switch
                {
                    "guntrigger" => doTriggerMethod,
                    "gunjam" => doJamMethod,
                    _ => null
                };
                if (targetMethod == null) continue;

                // 找到随后最近的 Sound.Play 调用
                var playIdx = -1;
                for (var j = i + 1; j < Math.Min(i + 12, codes.Count); j++)
                {
                    if ((codes[j].opcode != OpCodes.Call && codes[j].opcode != OpCodes.Callvirt)
                        || codes[j].operand is not MethodInfo m
                        || m.DeclaringType?.Name != "Sound"
                        || m.Name != "Play") continue;
                    playIdx = j;
                    break;
                }

                if (playIdx <= i) continue;
                // 将 ldstr → Sound.Play 整段替换为 ldarg.0 + call DoPlayXSound
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

    // Transpiler: 替换 GunScript.Fire() 中硬编码的 ldstr "gunjam" → DoPlayJamSound。
    // Fire() 有 1 个 ldstr "gunjam" → Sound.Play(string, Vector2, bool)，
    // 只有自动/半自动模式卡壳时触发。
    private static IEnumerable<CodeInstruction> TranspileFire(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var doJamMethod = typeof(GunRuntimeManager).GetMethod(nameof(DoPlayJamSound),
            BindingFlags.NonPublic | BindingFlags.Static);
        if (doJamMethod == null) return codes;

        for (var i = codes.Count - 1; i >= 0; i--)
        {
            if (codes[i].opcode != OpCodes.Ldstr || codes[i].operand is not "gunjam")
                continue;

            // 找到随后最近的 Sound.Play 调用
            var playIdx = -1;
            for (var j = i + 1; j < Math.Min(i + 12, codes.Count); j++)
            {
                if ((codes[j].opcode != OpCodes.Call && codes[j].opcode != OpCodes.Callvirt)
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
