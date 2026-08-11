using System;
using System.Linq;
using Bark.Events;
using Bark.Tool;
using HarmonyLib;

namespace Bark.Event.Listener;

// 水晶事件监听器：通过 Harmony 补丁拦截所有水晶效果子类的 Touched/Hit 方法，
// 以及水晶敌人的攻击/死亡方法，触发水晶相关事件。
// 水晶效果子类均为 internal，通过反射从程序集获取，避免 typeof 直接引用。
public static class CrystalEventListener
{
    internal static void Listen()
    {
        var effectBase = typeof(CrystalEffect);
        var effectTypes = effectBase.Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && effectBase.IsAssignableFrom(t))
            .Where(t => t != effectBase)
            .ToList();

        foreach (var effectType in effectTypes)
        {
            PatchTouched(effectType);
            PatchHit(effectType);
        }

        PatchEnemy();
    }

    internal static void Stop()
    {
    }

    private static void PatchTouched(Type effectType)
    {
        var method = AccessTools.Method(effectType, "Touched");
        if (method == null) return;

        try
        {
            var harmony = new Harmony($"Bark.Crystal.Touched.{effectType.Name}");
            harmony.Patch(method,
                postfix: new HarmonyMethod(typeof(CrystalEventListener), nameof(OnCrystalTouchedPostfix)));
        }
        catch
        {
            // ignored
        }
    }

    private static void PatchHit(Type effectType)
    {
        var method = AccessTools.Method(effectType, "Hit");
        if (method == null) return;

        try
        {
            var harmony = new Harmony($"Bark.Crystal.Hit.{effectType.Name}");
            harmony.Patch(method,
                postfix: new HarmonyMethod(typeof(CrystalEventListener), nameof(OnCrystalHitPostfix)));
        }
        catch
        {
            // ignored
        }
    }

    private static void PatchEnemy()
    {
        // CrystalEnemy.Lunge()：突刺攻击
        var lunge = AccessTools.Method(typeof(CrystalEnemy), "Lunge");
        if (lunge != null)
        {
            try
            {
                var harmony = new Harmony("Bark.Crystal.EnemyAttack");
                harmony.Patch(lunge,
                    postfix: new HarmonyMethod(typeof(CrystalEventListener), nameof(OnCrystalEnemyAttackPostfix)));
            }
            catch
            {
                // ignored
            }
        }

        // CrystalEnemy.AnimalDeath()：死亡
        var death = AccessTools.Method(typeof(CrystalEnemy), "AnimalDeath");
        if (death == null) return;

        try
        {
            var harmony = new Harmony("Bark.Crystal.EnemyDeath");
            harmony.Patch(death,
                postfix: new HarmonyMethod(typeof(CrystalEventListener), nameof(OnCrystalEnemyDeathPostfix)));
        }
        catch
        {
            // ignored
        }
    }

    // ============================================================
    // 水晶效果
    // ============================================================

    private static void OnCrystalTouchedPostfix(CrystalEffect __instance)
    {
        if (__instance.crystal == null) return;

        EventUtil.Trigger(new CrystalTouchEvent
        {
            EffectType = __instance.GetType().Name,
            Crystal = __instance.crystal
        });
    }

    private static void OnCrystalHitPostfix(CrystalEffect __instance)
    {
        if (__instance.crystal == null) return;

        EventUtil.Trigger(new CrystalHitEvent
        {
            EffectType = __instance.GetType().Name,
            Crystal = __instance.crystal
        });
    }

    // ============================================================
    // 水晶敌人
    // ============================================================

    private static void OnCrystalEnemyAttackPostfix(CrystalEnemy __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new CrystalEnemyAttackEvent { Enemy = __instance });
    }

    private static void OnCrystalEnemyDeathPostfix(CrystalEnemy __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new CrystalEnemyDeathEvent { Enemy = __instance });
    }
}