using System.IO;
using BepInEx;
using CUCoreLib.Helpers;
using UnityEngine;

namespace Bark.Audio;

// 模组音频管理器：统一管理自定义音效加载。
// Plugin 启动时调用 Initialize(this) 初始化。
// 两种加载路径：
// 1. LoadCustomAudio() — 从插件目录 BepInEx/plugins/Bark/ 加载（通用音效）
// 2. LoadModAudio() — 从脚本模组目录 {ModDir}/Assets/Audio/ 加载（模组专属音效）
// AssetLoader 按完整路径自动缓存，重复调用不产生额外 IO。
// 支持格式：.wav .mp1 .mp2 .mp3 .cue .aif .aiff（不支持 .ogg）。
public static class AudioManager
{
    private static BaseUnityPlugin? _plugin;

    // 初始化，在 Plugin.Awake 中调用以提供插件实例（用于路径解析）。
    public static void Initialize(BaseUnityPlugin plugin)
    {
        _plugin = plugin;
    }

    // 释放插件引用，通常在 Plugin.OnDestroy 中调用。
    public static void Shutdown()
    {
        _plugin = null;
    }

    // 从插件目录加载音效（BepInEx/plugins/Bark/）。
    // relativePath 如 "Audio/ambient.wav"。
    // 返回 null 表示加载失败，调用方应回退到默认音效。
    public static AudioClip? LoadCustomAudio(string relativePath)
    {
        if (_plugin is null || string.IsNullOrEmpty(relativePath))
            return null;

        return AssetLoader.LoadAudioFromPluginFolder(_plugin, relativePath);
    }

    // 从脚本模组目录加载音效（{ModDir}/Assets/Audio/）。
    // modDir 为模组根目录（绝对路径），relativePath 为音效相对路径。
    // relativePath 含 / 或 \ 时直接拼到 modDir 后面，纯文件名则自动补全 "Assets/Audio/"。
    // 返回 null 表示加载失败，调用方应回退到默认音效。
    public static AudioClip? LoadModAudio(string modDir, string relativePath)
    {
        if (string.IsNullOrEmpty(modDir) || string.IsNullOrEmpty(relativePath))
            return null;

        // 纯文件名无路径分隔符时，自动补全 Assets/Audio/ 前缀。
        var path = relativePath;
        if (path.IndexOf('/') < 0 && path.IndexOf('\\') < 0)
            path = Path.Combine("Assets", "Audio", path);

        var absolutePath = Path.Combine(modDir, path);
        return AssetLoader.LoadAudioFromFile(absolutePath);
    }
}