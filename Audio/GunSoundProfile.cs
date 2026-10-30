using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bark.Tool;
using CUCoreLib.Helpers;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Bark.Audio;

public class SoundEntry
{
    public string File = "";
    public float Pitch = 1f;
    public float Volume = 1f;
    public float Weight = 1f;
}

// {
//   "fire": [
//     { "file": "ak47_shot_1.wav", "volume": 0.9, "pitch": 1.0, "weight": 3 },
//     { "file": "ak47_shot_2.wav", "volume": 0.85, "pitch": 0.97, "weight": 2 }
//   ],
//   "rack": [
//     { "file": "ak47_rack.wav", "volume": 0.6, "pitch": 1.0 }
//   ],
//   "unrack": null,
//   "load_mag": [],
//   "unload_mag": []
// }
public class GunSoundProfile
{
    [JsonIgnore] private readonly Dictionary<string, AudioClip> _clipCache = new();
    [JsonProperty("fire")] public List<SoundEntry>? Fire;
    [JsonProperty("jam")] public List<SoundEntry>? Jam;
    [JsonProperty("load_mag")] public List<SoundEntry>? LoadMag;
    [JsonProperty("load_shell")] public List<SoundEntry>? LoadShell;
    [JsonProperty("rack")] public List<SoundEntry>? Rack;
    [JsonProperty("safety")] public List<SoundEntry>? Safety;
    [JsonProperty("trigger")] public List<SoundEntry>? Trigger;
    [JsonProperty("unload_mag")] public List<SoundEntry>? UnloadMag;
    [JsonProperty("unrack")] public List<SoundEntry>? Unrack;

    public static GunSoundProfile? Load(string modDir, string profileName)
    {
        if (string.IsNullOrEmpty(modDir) || string.IsNullOrEmpty(profileName))
            return null;

        var jsonPath = Path.Combine(modDir, "Audio", profileName + ".json");
        if (!File.Exists(jsonPath))
        {
            LogUtil.Warning("sound_profile.file_not_found", jsonPath);
            return null;
        }

        try
        {
            var json = File.ReadAllText(jsonPath);
            var profile = JsonConvert.DeserializeObject<GunSoundProfile>(json);
            if (profile == null) return null;

            profile.PreloadClips(modDir, profileName);

            LogUtil.Info("sound_profile.loaded", profileName, jsonPath);
            return profile;
        }
        catch (Exception ex)
        {
            LogUtil.Error("sound_profile.load_failed", profileName, ex.Message);
            return null;
        }
    }

    public AudioClip? GetRandomClip(List<SoundEntry>? entries)
    {
        if (entries == null || entries.Count == 0)
            return null;

        if (entries.Count == 1)
            return GetCachedClip(entries[0]);

        var totalWeight = entries.Sum(e => e.Weight);

        if (totalWeight <= 0f)
            return GetCachedClip(entries[0]);

        var roll = Random.value * totalWeight;
        var accum = 0f;
        foreach (var e in entries)
        {
            accum += e.Weight;
            if (roll <= accum)
                return GetCachedClip(e);
        }

        return GetCachedClip(entries[^1]);
    }

    public void PlayRandom(List<SoundEntry>? entries, Vector2 position)
    {
        if (entries == null || entries.Count == 0)
            return;

        if (entries.Count == 1)
        {
            var entry = entries[0];
            var clip = GetCachedClip(entry);
            if (clip == null) return;

            if (Math.Abs(entry.Volume - 1f) < 0.001f && Math.Abs(entry.Pitch - 1f) < 0.001f)
                Sound.Play(clip, position, true, false);
            else
                PlayWithSource(clip, position, entry.Volume, entry.Pitch);

            return;
        }

        var totalWeight = entries.Sum(e => e.Weight);

        if (totalWeight <= 0f)
        {
            PlayEntryWithSource(entries[0], position);
            return;
        }

        var roll = Random.value * totalWeight;
        var accum = 0f;
        foreach (var e in entries)
        {
            accum += e.Weight;
            if (!(roll <= accum)) continue;
            PlayEntryWithSource(e, position);
            return;
        }

        PlayEntryWithSource(entries[^1], position);
    }

    private void PreloadClips(string modDir, string profileName)
    {
        var allEntries = new List<SoundEntry?>();
        CollectEntries(Fire, allEntries);
        CollectEntries(Rack, allEntries);
        CollectEntries(Unrack, allEntries);
        CollectEntries(LoadMag, allEntries);
        CollectEntries(LoadShell, allEntries);
        CollectEntries(UnloadMag, allEntries);
        CollectEntries(Trigger, allEntries);
        CollectEntries(Jam, allEntries);
        CollectEntries(Safety, allEntries);

        var seen = new HashSet<string>();
        foreach (var entry in allEntries)
        {
            if (entry is not { File: var file } || string.IsNullOrEmpty(file) || !seen.Add(file))
                continue;

            var path = Path.Combine(modDir, "Assets", "Audio", file);
            var clip = AssetLoader.LoadAudioFromFile(path);
            if (clip != null)
                _clipCache[file] = clip;
            else
                LogUtil.Warning("sound_profile.clip_load_failed", profileName, file);
        }
    }

    private static void CollectEntries(List<SoundEntry>? entries, List<SoundEntry?> target)
    {
        if (entries == null) return;
        target.AddRange(entries);
    }

    private AudioClip? GetCachedClip(SoundEntry entry)
    {
        if (string.IsNullOrEmpty(entry.File))
            return null;
        return _clipCache.GetValueOrDefault(entry.File);
    }

    private void PlayEntryWithSource(SoundEntry entry, Vector2 position)
    {
        var clip = GetCachedClip(entry);
        if (clip == null) return;

        if (Math.Abs(entry.Volume - 1f) < 0.001f && Math.Abs(entry.Pitch - 1f) < 0.001f)
            Sound.Play(clip, position, true, false);
        else
            PlayWithSource(clip, position, entry.Volume, entry.Pitch);
    }

    private static void PlayWithSource(AudioClip clip, Vector2 position, float volume, float pitch)
    {
        var go = new GameObject("Bark_TempAudio")
        {
            transform =
            {
                position = position
            }
        };

        var source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
        source.spatialBlend = 1f;
        source.Play();

        Object.Destroy(go, clip.length / Mathf.Max(pitch, 0.1f));
    }
}