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

// Partial：IL Transpiler — GunScript.Update/Fire 的 IL 织入
public static partial class GunRuntimeManager
{
    // ============================================================
    // Update Transpiler：替换抛壳逻辑，用 Utils.Create 直接创建自定义弹壳
    // ============================================================
    //
    // GunScript.Update() 中 rack 事件原始抛壳 IL（基于当前游戏版本 MVID AB887C07）：
    //   三元运算 roundInChamber == Casing ? "casing" : AmmoTypeToItem(ammoType)
    //   由 beq 分支实现：true 分支压入 ldstr "casing"，false 分支压入 AmmoTypeToItem 结果，
    //   两者汇合后统一 call Resources.Load(string) → [pos/rot args] → call Object.Instantiate
    //
    // 问题：Resources.Load 只能加载 Unity Resources 目录下的预制体，
    // 自定义弹壳通过 CCL 的 CustomInstantiate 注册，Resources.Load 返回 null。
    //
    // 修复：将 true 分支起点（ldstr "casing"）改写为
    //   ldarg.0 → call DoSpawnCasing(GunScript) → br <Instantiate 之后>
    // DoSpawnCasing 直接通过 Utils.Create 创建自定义弹壳 GameObject 并返回。
    // 这样 true 分支完全绕过 Resources.Load + 参数准备 + Instantiate；
    // false 分支（实弹 AmmoTypeToItem → Resources.Load）保持原路径不变。
    private static IEnumerable<CodeInstruction> TranspileUpdate(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var doSpawnMethod = AccessTools.Method(typeof(GunRuntimeManager), nameof(DoSpawnCasing));

        // Step 1: 找到 ldstr "casing"（true 分支起点，由 beq 跳转至此）
        var casingIdx = -1;
        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode != OpCodes.Ldstr || codes[i].operand is not string s || s != "casing") continue;
            casingIdx = i;
            break;
        }

        if (casingIdx < 0) return codes;

        // Step 2: 找到 call Object.Instantiate（在 Resources.Load + pos/rot args 之后）
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

        // Step 3: 在 Instantiate 之后插入跳转目标 label
        var postInstantiateIdx = instantiateIdx + 1;
        if (postInstantiateIdx >= codes.Count) return codes;
        var postInstantiateLabel = new Label();
        codes[postInstantiateIdx].labels.Add(postInstantiateLabel);

        // Step 4: 将 ldstr "casing" 改写为 ldarg.0 + call DoSpawnCasing + br <postInstantiate>
        // true 分支执行 DoSpawnCasing 得到 GameObject 后，跳过 Resources.Load 直抵 Instantiate 之后；
        // 下游 isinst GameObject → GetComponent<Rigidbody2D> → velocity 无需改动。
        // 保留原 ldstr "casing" 上的标签（来自 beq 跳转），转移到首条新指令。
        var casingLabels = codes[casingIdx].labels;
        var newHead = new CodeInstruction(OpCodes.Ldarg_0);
        newHead.labels.AddRange(casingLabels);
        codes[casingIdx] = newHead;
        codes.Insert(casingIdx + 1, new CodeInstruction(OpCodes.Call, doSpawnMethod));
        codes.Insert(casingIdx + 2, new CodeInstruction(OpCodes.Br, postInstantiateLabel));

        // Step 5: 处理 false 分支（实弹 roundInChamber == Round）。
        // 原 IL：ldarg.0 → ldfld ammoType → call AmmoScript.AmmoTypeToItem → br L_merge
        // 改写为：ldarg.0（保留）→ call DoSpawnRound → br postInst，
        // 使退实弹时走 CustomInstantiate 创建自定义弹药，绕过 Resources.Load + Instantiate。
        var doRoundMethod = AccessTools.Method(typeof(GunRuntimeManager), nameof(DoSpawnRound));
        var ammoIdx = -1;
        for (var i = casingIdx - 1; i >= 0; i--)
        {
            if (codes[i].opcode != OpCodes.Call && codes[i].opcode != OpCodes.Callvirt) continue;
            if (codes[i].operand is not MethodBase am) continue;
            if (am.Name != "AmmoTypeToItem") continue;
            ammoIdx = i;
            break;
        }

        if (doRoundMethod != null && ammoIdx > 0)
        {
            // false 分支末尾的 br L_merge → br postInst
            var mergeBrIdx = ammoIdx + 1;
            if (mergeBrIdx < codes.Count &&
                (codes[mergeBrIdx].opcode == OpCodes.Br || codes[mergeBrIdx].opcode == OpCodes.Br_S))
                codes[mergeBrIdx] = new CodeInstruction(OpCodes.Br, postInstantiateLabel);

            // call AmmoTypeToItem → call DoSpawnRound（复用栈上已压入的 this）
            codes[ammoIdx] = new CodeInstruction(OpCodes.Call, doRoundMethod);

            // 删除前一条 ldfld ammoType（其压入的 ammoType 不再需要）
            if (codes[ammoIdx - 1].opcode == OpCodes.Ldfld)
                codes.RemoveAt(ammoIdx - 1);
        }

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

    // Transpiler 回调：枪膛退实弹（roundInChamber == Round 的 rack 分支）时创建自定义弹药 GameObject。
    // 由 Update Transpiler 动态注入到 false 分支，替代原版 AmmoTypeToItem → Resources.Load → Instantiate。
    // 非模板枪或无匹配自定义弹药时回退原版逻辑。
    private static Object? DoSpawnRound(GunScript gun)
    {
        if (gun == null) return null;

        var item = gun.GetComponent<Item>();
        if (item == null) return null;

        // 模板枪：按 ammo_type 查找匹配的自定义弹药
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

        // 兜底：原版弹药
        var prefab = Resources.Load(AmmoScript.AmmoTypeToItem(gun.ammoType));
        return prefab != null
            ? Object.Instantiate(prefab, gun.transform.position, gun.transform.rotation)
            : null;
    }
}