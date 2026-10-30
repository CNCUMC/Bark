using System.Collections.Generic;
using Bark.Events;
using Bark.Tool;
using HarmonyLib;

namespace Bark.Event.Listener;

public static class SystemEventListener
{
    private static readonly Dictionary<int, bool> TurretShootStates = new();
    private static readonly HashSet<int> ExplodedTurrets = [];
    private static readonly HashSet<int> SpentCannons = [];
    private static int _skillOldLevel = -1;

    internal static void Listen()
    {
    }

    internal static void Stop()
    {
        TurretShootStates.Clear();
        ExplodedTurrets.Clear();
        SpentCannons.Clear();
    }

    [HarmonyPatch(typeof(MindwipeScript), "WipeRoutine")]
    [HarmonyPrefix]
    private static void MindwipeScriptWipeRoutinePrefix(MindwipeScript __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new MindwipeEvent());
    }

    [HarmonyPatch(typeof(RadiationLine), "Activate")]
    [HarmonyPostfix]
    private static void RadiationLineActivatePostfix(RadiationLine __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new RadiationStartEvent());
    }

    [HarmonyPatch(typeof(SaveSystem), "SaveGame")]
    [HarmonyPostfix]
    private static void SaveSystemSaveGamePostfix()
    {
        EventUtil.Trigger(new GameSaveEvent());
    }

    private static int ReadSkillLevel(Skills skills, int stat)
    {
        return stat switch
        {
            0 => Traverse.Create(skills).Field("STR").GetValue<int>(),
            1 => Traverse.Create(skills).Field("RES").GetValue<int>(),
            _ => Traverse.Create(skills).Field("INT").GetValue<int>()
        };
    }

    [HarmonyPatch(typeof(TurretScript), "Update")]
    [HarmonyPostfix]
    private static void TurretScriptUpdatePostfix(TurretScript __instance)
    {
        if (__instance == null || !__instance) return;

        var id = __instance.GetInstanceID();

        var didShoot = Traverse.Create(__instance).Field("didShoot").GetValue<bool>();
        TurretShootStates.TryGetValue(id, out var prevShoot);
        TurretShootStates[id] = didShoot;
        if (didShoot && !prevShoot)
            EventUtil.Trigger(new TurretShootEvent { Turret = __instance });

        var build = __instance.GetComponent<BuildingEntity>();
        if (build != null && build.health <= 0f && ExplodedTurrets.Add(id))
            EventUtil.Trigger(new TurretExplodeEvent { Turret = __instance });
    }

    [HarmonyPatch(typeof(WorldGeneration), "RegenerateWorld", typeof(bool))]
    [HarmonyPrefix]
    private static void WorldGenerationRegenerateWorldPrefix(WorldGeneration __instance, bool twice)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new WorldRegenerateEvent { Twice = twice });
    }

    [HarmonyPatch(typeof(SawbladeScript), "OnCollisionEnter2D")]
    [HarmonyPostfix]
    private static void SawbladeScriptOnCollisionEnter2DPostfix(SawbladeScript __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new SawbladeHitEvent { Sawblade = __instance });
    }

    [HarmonyPatch(typeof(SoundCannon), "Update")]
    [HarmonyPostfix]
    private static void SoundCannonUpdatePostfix(SoundCannon __instance)
    {
        if (__instance == null || !__instance) return;

        var spent = Traverse.Create(__instance).Field("spent").GetValue<bool>();
        var id = __instance.GetInstanceID();
        if (spent && SpentCannons.Add(id))
            EventUtil.Trigger(new SoundCannonShootEvent { Cannon = __instance });
    }

    [HarmonyPatch(typeof(Skills))]
    public static class SkillPatch
    {
        [HarmonyPatch("AddExp", typeof(int), typeof(float))]
        [HarmonyPrefix]
        private static void AddExpPrefix(Skills __instance, int stat)
        {
            _skillOldLevel = ReadSkillLevel(__instance, stat);
        }

        [HarmonyPatch("AddExp", typeof(int), typeof(float))]
        [HarmonyPostfix]
        private static void AddExpPostfix(Skills __instance, int stat)
        {
            var newLevel = ReadSkillLevel(__instance, stat);
            if (newLevel <= _skillOldLevel) return;

            EventUtil.Trigger(new SkillLevelUpEvent
            {
                Stat = stat,
                OldLevel = _skillOldLevel,
                NewLevel = newLevel
            });
        }
    }

    [HarmonyPatch(typeof(TraderScript))]
    public static class TraderScriptPatch
    {
        [HarmonyPatch("MeetPlayer")]
        [HarmonyPostfix]
        private static void MeetPlayerPostfix(TraderScript __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new TraderMeetEvent
            {
                Trader = __instance,
                Character = __instance.character,
                Reputation = __instance.reputation
            });
        }

        [HarmonyPatch("TryHaggle")]
        [HarmonyPostfix]
        private static void TryHagglePostfix(TraderScript __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new TraderHaggleEvent
            {
                Trader = __instance,
                Reputation = __instance.reputation
            });
        }

        [HarmonyPatch("AnimalDeath")]
        [HarmonyPostfix]
        private static void AnimalDeathPostfix(TraderScript __instance)
        {
            if (__instance == null || !__instance) return;
            EventUtil.Trigger(new TraderDeathEvent { Trader = __instance });
        }
    }
}