using System;
using System.Collections.Generic;
using Bark.Events;
using Bark.Tool;
using HarmonyLib;

namespace Bark.Event.Listener;

// 系统事件监听器：通过 Harmony 补丁拦截精神抹除、辐射线、存档、技能升级、商人、
// 炮塔、世界重生、电锯、声波炮等系统级方法，触发对应事件。
public static class SystemEventListener
{
    // 炮塔开火标记（instanceId → didShoot）
    private static readonly Dictionary<int, bool> TurretShootStates = new();

    // 炮塔爆炸标记（instanceId）
    private static readonly HashSet<int> ExplodedTurrets = [];

    // 声波炮已发射标记（instanceId）
    private static readonly HashSet<int> SpentCannons = [];

    // 技能升级前等级缓存
    private static int _skillOldLevel = -1;

    internal static void Listen()
    {
        PatchMindwipe();
        PatchRadiation();
        PatchSave();
        PatchSkill();
        PatchTrader();
        PatchTurret();
        PatchWorldRegenerate();
        PatchSawblade();
        PatchSoundCannon();
    }

    internal static void Stop()
    {
        TurretShootStates.Clear();
        ExplodedTurrets.Clear();
        SpentCannons.Clear();
    }

    private static void Patch(Type type, string methodName, string harmonyId, string? prefix, string? postfix,
        Type[]? argTypes = null)
    {
        var method = argTypes != null
            ? AccessTools.Method(type, methodName, argTypes)
            : AccessTools.Method(type, methodName);
        if (method == null) return;

        try
        {
            var harmony = new Harmony(harmonyId);
            harmony.Patch(method,
                prefix: prefix != null ? new HarmonyMethod(typeof(SystemEventListener), prefix) : null,
                postfix: postfix != null ? new HarmonyMethod(typeof(SystemEventListener), postfix) : null);
        }
        catch
        {
            // ignored
        }
    }

    // ============================================================
    // 精神抹除
    // ============================================================

    private static void PatchMindwipe()
    {
        // WipeRoutine 协程被启动即表示抹除开始
        Patch(typeof(MindwipeScript), "WipeRoutine", "Bark.Mindwipe.Start", nameof(OnMindwipePrefix), null);
    }

    private static void OnMindwipePrefix(MindwipeScript __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new MindwipeEvent());
    }

    // ============================================================
    // 辐射线
    // ============================================================

    private static void PatchRadiation()
    {
        Patch(typeof(RadiationLine), "Activate", "Bark.Radiation.Start", null, nameof(OnRadiationPostfix));
    }

    private static void OnRadiationPostfix(RadiationLine __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new RadiationStartEvent());
    }

    // ============================================================
    // 游戏存档
    // ============================================================

    private static void PatchSave()
    {
        Patch(typeof(SaveSystem), "SaveGame", "Bark.Save.Game", null, nameof(OnSavePostfix));
    }

    private static void OnSavePostfix()
    {
        EventUtil.Trigger(new GameSaveEvent());
    }

    // ============================================================
    // 技能升级
    // ============================================================

    private static void PatchSkill()
    {
        Patch(typeof(Skills), "AddExp", "Bark.Skill.LevelUp", nameof(OnSkillPrefix), nameof(OnSkillPostfix),
            [typeof(int), typeof(float)]);
    }

    private static void OnSkillPrefix(Skills __instance, int stat)
    {
        _skillOldLevel = ReadSkillLevel(__instance, stat);
    }

    private static void OnSkillPostfix(Skills __instance, int stat)
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

    private static int ReadSkillLevel(Skills skills, int stat)
    {
        return stat switch
        {
            0 => Traverse.Create(skills).Field("STR").GetValue<int>(),
            1 => Traverse.Create(skills).Field("RES").GetValue<int>(),
            _ => Traverse.Create(skills).Field("INT").GetValue<int>()
        };
    }

    // ============================================================
    // 商人
    // ============================================================

    private static void PatchTrader()
    {
        Patch(typeof(TraderScript), "MeetPlayer", "Bark.Trader.Meet", null, nameof(OnTraderMeetPostfix));
        Patch(typeof(TraderScript), "TryHaggle", "Bark.Trader.Haggle", null, nameof(OnTraderHagglePostfix));
        Patch(typeof(TraderScript), "AnimalDeath", "Bark.Trader.Death", null, nameof(OnTraderDeathPostfix));
    }

    private static void OnTraderMeetPostfix(TraderScript __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new TraderMeetEvent
        {
            Trader = __instance,
            Character = __instance.character,
            Reputation = __instance.reputation
        });
    }

    private static void OnTraderHagglePostfix(TraderScript __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new TraderHaggleEvent
        {
            Trader = __instance,
            Reputation = __instance.reputation
        });
    }

    private static void OnTraderDeathPostfix(TraderScript __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new TraderDeathEvent { Trader = __instance });
    }

    // ============================================================
    // 炮塔
    // ============================================================

    private static void PatchTurret()
    {
        Patch(typeof(TurretScript), "Update", "Bark.Turret.Shoot", null, nameof(OnTurretUpdatePostfix));
    }

    private static void OnTurretUpdatePostfix(TurretScript __instance)
    {
        if (__instance == null || !__instance) return;

        var id = __instance.GetInstanceID();

        // 开火检测：didShoot 从 false → true
        var didShoot = Traverse.Create(__instance).Field("didShoot").GetValue<bool>();
        TurretShootStates.TryGetValue(id, out var prevShoot);
        TurretShootStates[id] = didShoot;
        if (didShoot && !prevShoot)
            EventUtil.Trigger(new TurretShootEvent { Turret = __instance });

        // 爆炸检测：build.health <= 0
        var build = __instance.GetComponent<BuildingEntity>();
        if (build != null && build.health <= 0f && ExplodedTurrets.Add(id))
            EventUtil.Trigger(new TurretExplodeEvent { Turret = __instance });
    }

    // ============================================================
    // 世界重生
    // ============================================================

    private static void PatchWorldRegenerate()
    {
        Patch(typeof(WorldGeneration), "RegenerateWorld", "Bark.World.Regenerate", nameof(OnWorldRegeneratePrefix),
            null, [typeof(bool)]);
    }

    private static void OnWorldRegeneratePrefix(WorldGeneration __instance, bool twice)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new WorldRegenerateEvent { Twice = twice });
    }

    // ============================================================
    // 电锯
    // ============================================================

    private static void PatchSawblade()
    {
        Patch(typeof(SawbladeScript), "OnCollisionEnter2D", "Bark.Sawblade.Hit", null, nameof(OnSawbladePostfix));
    }

    private static void OnSawbladePostfix(SawbladeScript __instance)
    {
        if (__instance == null || !__instance) return;
        EventUtil.Trigger(new SawbladeHitEvent { Sawblade = __instance });
    }

    // ============================================================
    // 声波炮
    // ============================================================

    private static void PatchSoundCannon()
    {
        Patch(typeof(SoundCannon), "Update", "Bark.SoundCannon.Shoot", null, nameof(OnSoundCannonUpdatePostfix));
    }

    private static void OnSoundCannonUpdatePostfix(SoundCannon __instance)
    {
        if (__instance == null || !__instance) return;

        var spent = Traverse.Create(__instance).Field("spent").GetValue<bool>();
        var id = __instance.GetInstanceID();
        if (spent && SpentCannons.Add(id))
            EventUtil.Trigger(new SoundCannonShootEvent { Cannon = __instance });
    }
}
