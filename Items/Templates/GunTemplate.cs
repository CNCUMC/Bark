using System.Collections.Generic;
using Bark.Audio;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

public class GunData
{
    public string AmmoType = "7_62x51mm";
    public float AnimalDamage = 25f;
    public float BarrelOffsetX = 0.5f;
    public float BarrelOffsetY;
    public int Capacity;
    public float ConditionLossPerShot = 0.01f;
    public float DesiredGasTime = 0.1f;
    public bool Direct;
    public string FeedType = "mag";
    public string FireSound = "";
    public string FiringMode = "semi_auto";
    public string GunType = "rifle";
    public float Knockback = 0.5f;
    public float Loudness = 25f;
    public string MagType = "pistol_mag";
    public string ModDir = "";
    public string RackSound = "";
    public int ShotsPerFire = 1;
    public string Sound = "";
    [JsonIgnore] public GunSoundProfile? SoundProfile;
    public bool StartChambered = true;
    public bool StartSafe;
    public float StructureDamage = 12f;
    public float TinnitusMultiplier = 0.1f;
    public string UnrackSound = "";
    public float VerticalSpread = 0.05f;
}

// "template": {
//   "type": "gun",
//   "ammo_type": "7_62x51mm",
//   "mag_type": "ar15_mag",
//   "firing_mode": "auto",
//   "origin_prefab": "rifle",
//   "barrel_offset": { "x": 0.5, "y": 0.0 },
//   "fire_sound": "Assets/Audio/ak47_shot.wav",
//   "rack_sound": "Assets/Audio/ak47_rack.wav",
//   "unrack_sound": ""
// }

public class GunTemplate : ItemTemplate
{
    private static readonly Dictionary<string, GunData> Registry = new();
    public override string Name => "gun";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            ["origin_prefab"] = "rifle",
            ["category"] = "utility",
            ["only_hold_in_hands"] = true,

            ["template"] = new JObject
            {
                ["gun"] = true,
                ["ammo_type"] = "7_62x51mm",
                ["firing_mode"] = "semi_auto",
                ["mag_type"] = "rifle_mag",
                ["direct"] = false,
                ["knockback"] = 0.5,
                ["structure_damage"] = 12.0,
                ["animal_damage"] = 25.0,
                ["loudness"] = 25.0,
                ["tinnitus_multiplier"] = 0.1,
                ["shots_per_fire"] = 1,
                ["vertical_spread"] = 0.05,
                ["condition_loss_per_shot"] = 0.01,
                ["capacity"] = 0,
                ["desired_gas_time"] = 0.1,
                ["start_safe"] = false,
                ["start_chambered"] = true,
                ["feed_type"] = "mag",
                ["barrel_offset"] = new JObject { ["x"] = 0.5, ["y"] = 0.0 },
                ["sound"] = "",
                ["fire_sound"] = "",
                ["rack_sound"] = "",
                ["unrack_sound"] = ""
            }
        };
    }

    public static void CacheGunItem(string itemId, JObject? template, string modDir)
    {
        if (template is null) return;
        if (!template.TryGetValue("gun", out var flag) || !flag.Value<bool>()) return;
        var data = GunDataFromJObject(template);
        data.ModDir = modDir;

        if (!string.IsNullOrEmpty(data.Sound))
            data.SoundProfile = GunSoundProfile.Load(modDir, data.Sound);

        Registry[itemId] = data;
    }

    public static void RemoveGunItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    private static GunData GunDataFromJObject(JObject t)
    {
        return new GunData
        {
            AmmoType = (string?)t["ammo_type"] ?? "7_62x51mm",
            GunType = (string?)t["gun_type"] ?? "rifle",
            FiringMode = (string?)t["firing_mode"] ?? "semi_auto",
            MagType = (string?)t["mag_type"] ?? "rifle_mag",
            Direct = t.TryGetValue("direct", out var dv) && dv.Value<bool>(),
            Knockback = (float?)t["knockback"] ?? 0.5f,
            StructureDamage = (float?)t["structure_damage"] ?? 12f,
            AnimalDamage = (float?)t["animal_damage"] ?? 25f,
            Loudness = (float?)t["loudness"] ?? 25f,
            TinnitusMultiplier = (float?)t["tinnitus_multiplier"] ?? 0.1f,
            ShotsPerFire = (int?)t["shots_per_fire"] ?? 1,
            VerticalSpread = (float?)t["vertical_spread"] ?? 0.05f,
            ConditionLossPerShot = (float?)t["condition_loss_per_shot"] ?? 0.01f,
            Capacity = (int?)t["capacity"] ?? 0,
            DesiredGasTime = (float?)t["desired_gas_time"] ?? 0.1f,
            StartSafe = t.TryGetValue("start_safe", out var ss) && ss.Value<bool>(),
            StartChambered = !t.TryGetValue("start_chambered", out var sc)
                || sc.Value<bool>(),
            FeedType = (string?)t["feed_type"] ?? "mag",
            BarrelOffsetX = ParseBarrelOffset(t, "x", 0.5f),
            BarrelOffsetY = ParseBarrelOffset(t, "y", 0f),
            FireSound = (string?)t["fire_sound"] ?? "",
            RackSound = (string?)t["rack_sound"] ?? "",
            UnrackSound = (string?)t["unrack_sound"] ?? "",
            Sound = (string?)t["sound"] ?? ""
        };
    }

    private static float ParseBarrelOffset(JObject t, string axis, float defaultValue)
    {
        if (t["barrel_offset"] is not JObject offset) return defaultValue;
        return (float?)offset[axis] ?? defaultValue;
    }

    public static IEnumerable<string> GetAllGunIds()
    {
        return Registry.Keys;
    }

    public static bool IsGun(string itemId)
    {
        return Registry.ContainsKey(itemId);
    }

    public static GunData? GetGunData(string itemId)
    {
        return Registry.GetValueOrDefault(itemId);
    }

    public static string GetAmmoType(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.AmmoType
            : "7_62x51mm";
    }

    public static string GetFiringMode(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.FiringMode
            : "semi_auto";
    }

    public static bool IsDirect(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d) && d.Direct;
    }

    public static string GetMagType(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.MagType
            : "pistol_mag";
    }

    public static float GetKnockback(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.Knockback
            : 0.5f;
    }

    public static float GetStructureDamage(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.StructureDamage
            : 12f;
    }

    public static float GetAnimalDamage(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.AnimalDamage
            : 25f;
    }

    public static float GetLoudness(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.Loudness
            : 25f;
    }

    public static int GetShotsPerFire(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.ShotsPerFire
            : 1;
    }

    public static float GetVerticalSpread(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.VerticalSpread
            : 0.05f;
    }

    public static float GetConditionLossPerShot(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.ConditionLossPerShot
            : 0.01f;
    }

    public static int GetCapacity(string itemId)
    {
        return Registry.TryGetValue(itemId, out var d)
            ? d.Capacity
            : 0;
    }
}
