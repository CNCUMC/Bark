using System.Collections.Generic;
using Bark.Audio;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Bark.Items.Templates;

public class PlushData
{
    public string ModDir = "";
    [JsonIgnore] public AudioClip? SqueakClip;
    public string SqueakSound = "";
}

// "template": {
//   "type": "plush",
//   "squeak_sound": "Assets/Audio/plush_squeak.wav"   // 自定义吱吱音效（可省略，用游戏默认）
// }

public class PlushTemplate : ItemTemplate
{
    private static readonly Dictionary<string, PlushData> Registry = new();
    public override string Name => "plush";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            ["origin_prefab"] = "plushie",
            ["category"] = "utility",
            ["destroy_at_zero_condition"] = true,
            ["weight"] = 0.15,
            ["scale_weight_with_condition"] = false,
            ["value"] = 5,
            ["recognition"] = 6,
            ["tags"] = "belttool",

            ["sprite"] = new JObject
            {
                ["slot_rotation"] = 0f
            },

            ["template"] = new JObject
            {
                ["plush"] = true,
                ["squeak_sound"] = ""
            }
        };
    }

    public static void CachePlushItem(string itemId, JObject? template, string modDir)
    {
        if (template is null) return;
        if (!template.TryGetValue("plush", out var flag) || !flag.Value<bool>()) return;

        var data = PlushDataFromJObject(template);
        data.ModDir = modDir;

        if (!string.IsNullOrEmpty(data.SqueakSound))
            data.SqueakClip = AudioManager.LoadModAudio(modDir, data.SqueakSound);

        Registry[itemId] = data;
    }

    public static void RemovePlushItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    private static PlushData PlushDataFromJObject(JObject t)
    {
        return new PlushData
        {
            SqueakSound = (string?)t["squeak_sound"] ?? ""
        };
    }

    public static bool IsPlush(string itemId)
    {
        return Registry.ContainsKey(itemId);
    }

    public static PlushData? GetPlushData(string itemId)
    {
        return Registry.GetValueOrDefault(itemId);
    }

    public static bool PlayCustomSqueak(PlushScript plush)
    {
        if (plush == null || !plush) return false;

        var item = plush.GetComponent<Item>();
        if (item == null || string.IsNullOrEmpty(item.id)) return false;

        var data = GetPlushData(item.id);
        if (data is not { SqueakClip: not null }) return false;

        Sound.Play(data.SqueakClip, plush.transform.position, true, false);
        return true;
    }

    public static void ApplySqueakHook()
    {
        var method = AccessTools.Method(typeof(PlushScript), "Squeak");
        if (method == null) return;

        try
        {
            var harmony = new Harmony("Bark.PlushTemplate.Squeak");
            harmony.Patch(method,
                new HarmonyMethod(typeof(PlushTemplate), nameof(OnSqueakPrefix)));
        }
        catch
        {
            // ignored
        }
    }

    private static bool OnSqueakPrefix(PlushScript __instance)
    {
        return !PlayCustomSqueak(__instance);
    }
}
