using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Bark.Tool;
using CUCoreLib.Helpers;
using Newtonsoft.Json.Linq;

namespace Bark.Script;

// 主机直推模组回退通道：当模组无 GitHub repository 或 GitHub 下载失败时，
// 客户端通过该通道向主机请求整个模组目录的 zip 打包数据（分片传输），落地后由 ScriptModLoader 重载。
// 网络底层走 Bark 自建 BarkKrokBridge（直接反射 KrokMP 4.0.1），KrokMP 未安装时零开销。
public static class HostModFetcher
{
    // 专用 channel：客户端发送 { id, offset }，服务端返回单个分片
    public const string FetchChannel = "bark.modsync.fetch";

    // 单分片原始字节上限。
    // CUCoreLib 对响应还会再走一层 JSON + GZip + base64 封装，最终单条网络消息体积约为原始字节数的 1.7~2 倍。
    // 取较小值以确保单包不超过 LiteNetLib 等底层传输的单条消息上限，避免大分片响应无法回传导致下载挂死。
    private const int ChunkSize = 8 * 1024;

    // 服务端缓存：modId -> 整目录 zip 字节。首次请求时懒打包，避免每次请求重复压缩。
    private static readonly Dictionary<string, byte[]> ServerCache = new(StringComparer.OrdinalIgnoreCase);

    // 初始化：注册 fetch handler。
    // 注意：无条件注册（RegisterServerHandler 仅写字典，客户端/未建房间时也无害）。
    // 之前误加了 IsServer/IsHost 检查——它在 Plugin.Awake（建房间前）时为 false，导致主机建房间后
    // fetch handler 未注册、客户端拉取模组时收到 "(no handler)"。
    public static void Initialize()
    {
        if (!BarkKrokBridge.IsAvailable)
            return;

        BarkKrokBridge.RegisterServerHandler(FetchChannel, OnFetchRequested);
    }

    // 客户端入口：从主机拉取整目录 zip 并写入 {modsPath}/Mods/{modId}.zip
    // 回调在 Unity 主线程执行（通过协程等待网络响应）
    public static void RequestFromHost(string modId, string modsPath, Action<bool, string?> onComplete)
    {
        if (modId is null) throw new ArgumentNullException(nameof(modId));
        if (modsPath is null) throw new ArgumentNullException(nameof(modsPath));
        if (onComplete is null) throw new ArgumentNullException(nameof(onComplete));

        if (!BarkKrokBridge.IsAvailable || !BarkKrokBridge.IsClient)
        {
            onComplete(false, "Multiplayer is not available on client");
            return;
        }

        CUCoreUtils.StartCoroutine(FetchCoroutine(modId, modsPath, onComplete));
    }

    // 客户端协程：循环请求分片直到 done
    private static IEnumerator FetchCoroutine(string modId, string modsPath, Action<bool, string?> onComplete)
    {
        var buffer = new List<byte>(ChunkSize * 8);
        var complete = false;
        var success = false;
        var errorMsg = string.Empty;
        var isDone = false;

        // 以服务端返回的 done 标志作为终止条件；offset 始终等于已累积字节数，
        // 避免中间分片时 offset == buffer.Count 被误判为"最后一分片"而提前终止，
        // 导致只下载了第一个分片、zip 不完整。
        while (!isDone)
        {
            var request = new JObject
            {
                ["id"] = modId,
                ["offset"] = buffer.Count
            };

            complete = false;
            success = false;
            errorMsg = string.Empty;

            var sent = BarkKrokBridge.RequestServer(FetchChannel, request, response =>
            {
                complete = true;
                ParseFetchResponse(response, buffer, out success, out errorMsg, out isDone);
            });

            if (!sent)
            {
                onComplete(false, "Failed to send fetch request to host");
                yield break;
            }

            // 挂起等待网络回调
            while (!complete)
                yield return null;

            if (success) continue;
            onComplete(false, errorMsg);
            yield break;
        }

        // 写入 Mods/{modId}.zip，交由 ScriptModLoader.ReloadAll 解压加载
        try
        {
            var modsDir = Path.Combine(modsPath, "Mods");
            Directory.CreateDirectory(modsDir);
            var zipPath = Path.Combine(modsDir, $"{modId}.zip");
            File.WriteAllBytes(zipPath, [.. buffer]);
            onComplete(true, null);
        }
        catch (Exception ex)
        {
            onComplete(false, $"Failed to write mod zip: {ex.Message}");
        }
    }

    // 解析单个分片响应，追加到 buffer；返回是否成功、错误信息与服务端 done 标志
    private static void ParseFetchResponse(
        JToken response, List<byte> buffer, out bool success, out string errorMsg, out bool isDone)
    {
        success = false;
        errorMsg = string.Empty;
        isDone = false;

        if (response is not JObject obj)
        {
            errorMsg = "Invalid fetch response from host";
            return;
        }

        var error = obj["error"]?.Value<string>();
        if (!string.IsNullOrEmpty(error))
        {
            errorMsg = error;
            return;
        }

        var chunkBase64 = obj["chunk"]?.Value<string>();
        if (string.IsNullOrEmpty(chunkBase64))
        {
            errorMsg = "Host returned empty chunk";
            return;
        }

        var chunk = TryFromBase64(chunkBase64);
        if (chunk == null)
        {
            errorMsg = "Host returned invalid base64 chunk";
            return;
        }

        buffer.AddRange(chunk);

        isDone = obj["done"]?.Value<bool>() ?? false;
        success = true;
    }

    // 服务端 handler：按 offset 返回模组目录 zip 的一个分片
    private static JToken OnFetchRequested(JToken request)
    {
        var modId = request["id"]?.Value<string>();
        var offset = request["offset"]?.Value<int>() ?? 0;

        if (string.IsNullOrWhiteSpace(modId))
            return ErrorResponse("Missing mod id");

        if (!ScriptModLoader.LoadedScriptMods.TryGetValue(modId, out var manifest) ||
            string.IsNullOrEmpty(manifest.Directory) || !Directory.Exists(manifest.Directory))
            return ErrorResponse($"Mod '{modId}' not found on host");

        var zipBytes = GetCachedZip(modId, manifest.Directory);
        if (zipBytes == null)
            return ErrorResponse($"Failed to pack mod '{modId}' on host");

        var total = zipBytes.Length;
        if (offset < 0 || offset > total)
            return ErrorResponse("Invalid offset");

        var length = Math.Min(ChunkSize, total - offset);
        var chunk = new byte[length];
        Array.Copy(zipBytes, offset, chunk, 0, length);

        var done = offset + length >= total;
        return new JObject
        {
            ["id"] = modId,
            ["offset"] = offset,
            ["total"] = total,
            ["done"] = done,
            ["chunk"] = Convert.ToBase64String(chunk)
        };
    }

    // 懒打包并缓存模组目录 zip
    private static byte[]? GetCachedZip(string modId, string directory)
    {
        if (ServerCache.TryGetValue(modId, out var cached))
            return cached;

        var packed = PackDirectory(modId, directory);
        if (packed != null)
            ServerCache[modId] = packed;
        return packed;
    }

    // 将整个模组目录打包为 zip 字节（内存中，不落盘）
    private static byte[]? PackDirectory(string modId, string directory)
    {
        try
        {
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var filePath in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                {
                    var relative = GetRelativePath(directory, filePath);
                    if (string.IsNullOrEmpty(relative))
                        continue;
                    var entry = archive.CreateEntry(relative.Replace('\\', '/'));
                    using var entryStream = entry.Open();
                    using var fileStream = File.OpenRead(filePath);
                    fileStream.CopyTo(entryStream);
                }
            }

            return ms.ToArray();
        }
        catch (Exception ex)
        {
            LogUtil.Warning("network_sync.host_pack_failed", modId, ex.Message);
            return null;
        }
    }

    private static JObject ErrorResponse(string message)
    {
        return new JObject { ["error"] = message };
    }

    private static byte[]? TryFromBase64(string s)
    {
        try
        {
            return Convert.FromBase64String(s);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    // 计算相对路径（兼容 .NET Standard 2.1 无 Path.GetRelativePath）
    private static string GetRelativePath(string basePath, string fullPath)
    {
        var baseDir = basePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                      Path.DirectorySeparatorChar;
        if (fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            return fullPath[baseDir.Length..];
        return fullPath;
    }
}