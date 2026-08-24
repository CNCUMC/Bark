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

// 枪械音效配置项：单个音效文件 + 播放参数。
public class SoundEntry
{
    // 音效文件名（相对于 ModDir/Assets/Audio/，如 "ak47_shot_1.wav"）
    public string File = "";

    // 音高/播放速度 0.5 ~ 2.0
    public float Pitch = 1f;

    // 音量 0.0 ~ 1.0
    public float Volume = 1f;

    // 随机权重（多音效同时存在时，权重越高越容易被选中）
    public float Weight = 1f;
}

// 枪械音效配置档案：定义一把枪在所有场景下使用的音效。
// JSON 文件位置：{ModDir}/Audio/{profileName}.json
// 枪械模板中 "sound": "ak47" 即加载 Audio/ak47.json。
//
// JSON 结构示例：
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
    // 预加载的 AudioClip 缓存（file → clip），由 Load 时填充
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

    // 从 JSON 文件加载 SoundProfile 并预加载所有 AudioClip。
    // modDir:   模组根目录（绝对路径），如 "ScriptMod/Mods/hello_world_js/"
    // profileName: 档案名，如 "ak47"（不含 .json）
    // 返回 null 表示加载失败。
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

            // 预加载所有引用的 AudioClip
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

    // 从条目列表中按权重随机选取一个 AudioClip。
    // 返回 null 表示列表为空或全部加载失败。
    public AudioClip? GetRandomClip(List<SoundEntry>? entries)
    {
        if (entries == null || entries.Count == 0)
            return null;

        // 单条目直接返回
        if (entries.Count == 1)
            return GetCachedClip(entries[0]);

        // 权重随机
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

        return GetCachedClip(entries[^1]); // 兜底
    }

    // 随机选取并播放一组音效条目。
    // 使用 Unity AudioSource 播放以支持音量和音高控制。
    public void PlayRandom(List<SoundEntry>? entries, Vector2 position)
    {
        if (entries == null || entries.Count == 0)
            return;

        // 如果只有一个条目且音高/音量为默认值，直接用游戏的 Sound.Play 更好
        // （保留游戏的 3D 音效衰减逻辑）
        if (entries.Count == 1)
        {
            var entry = entries[0];
            var clip = GetCachedClip(entry);
            if (clip == null) return;

            if (Math.Abs(entry.Volume - 1f) < 0.001f && Math.Abs(entry.Pitch - 1f) < 0.001f)
                // 默认参数 → 直接用 Sound.Play
                Sound.Play(clip, position, true, false);
            else
                // 自定义参数 → AudioSource
                PlayWithSource(clip, position, entry.Volume, entry.Pitch);

            return;
        }

        // 多条目：权重随机
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

    // 预加载 profile 中所有引用的音频文件
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

    // 使用临时 AudioSource 播放，支持音量和音高控制。
    // 播放完毕后自动销毁 GameObject。
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

        // 按实际播放时长（受 pitch 影响）销毁
        Object.Destroy(go, clip.length / Mathf.Max(pitch, 0.1f));
    }
}