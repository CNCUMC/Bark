using Bark.Events;
using Bark.Tool;
using HarmonyLib;

namespace Bark.Event.Listener;

// 水晶事件监听器：通过 Harmony 补丁拦截水晶效果基类声明的虚方法 Touched/Hit，
// 以及水晶敌人的攻击/死亡方法，触发水晶相关事件。
// 只 patch 基类声明的虚方法一次，所有 internal 子类共享此补丁。
public static class CrystalEventListener
{
    internal static void Listen()
    {
    }

    internal static void Stop()
    {
    }

    [HarmonyPatch(typeof(CrystalEffect))]
    public static class CrystalEffectPatch
    {
        [HarmonyPatch("Touched")]
        [HarmonyPostfix]
        private static void TouchedPostfix(CrystalEffect __instance)
        {
            if (__instance.crystal == null) return;

            EventUtil.Trigger(new CrystalTouchEvent
            {
                EffectType = __instance.GetType().Name,
                Crystal = __instance.crystal
            });
        }

        [HarmonyPatch("Hit")]
        [HarmonyPostfix]
        private static void HitPostfix(CrystalEffect __instance)
        {
            if (__instance.crystal == null) return;

            EventUtil.Trigger(new CrystalHitEvent
            {
                EffectType = __instance.GetType().Name,
                Crystal = __instance.crystal
            });
        }
    }

    [HarmonyPatch(typeof(CrystalEnemy))]
    public static class CrystalEnemyPatch
    {
        [HarmonyPatch("Lunge")]
        [HarmonyPostfix]
        private static void LungePostfix(CrystalEnemy __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new CrystalEnemyAttackEvent { Enemy = __instance });
        }

        [HarmonyPatch("AnimalDeath")]
        [HarmonyPostfix]
        private static void AnimalDeathPostfix(CrystalEnemy __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new CrystalEnemyDeathEvent { Enemy = __instance });
        }
    }
}