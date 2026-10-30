using System.IO;
using BepInEx;
using CUCoreLib.Helpers;
using UnityEngine;

namespace Bark.Audio;

public static class AudioManager
{
    private static BaseUnityPlugin? _plugin;

    public static void Initialize(BaseUnityPlugin plugin)
    {
        _plugin = plugin;
    }

    public static void Shutdown()
    {
        _plugin = null;
    }

    public static AudioClip? LoadCustomAudio(string relativePath)
    {
        if (_plugin is null || string.IsNullOrEmpty(relativePath))
            return null;

        return AssetLoader.LoadAudioFromPluginFolder(_plugin, relativePath);
    }

    public static AudioClip? LoadModAudio(string modDir, string relativePath)
    {
        if (string.IsNullOrEmpty(modDir) || string.IsNullOrEmpty(relativePath))
            return null;

        var path = relativePath;
        if (path.IndexOf('/') < 0 && path.IndexOf('\\') < 0)
            path = Path.Combine("Assets", "Audio", path);

        var absolutePath = Path.Combine(modDir, path);
        return AssetLoader.LoadAudioFromFile(absolutePath);
    }
}
