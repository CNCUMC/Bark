using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Bark.Tool;
using CUCoreLib.Helpers;
using Newtonsoft.Json.Linq;

namespace Bark.Script;

// 主机 sr 重载触发的增量文件同步：
// 主机向所有已连接客户端广播同步请求；客户端上报自己已加载模组的文件 hash 清单；
// 主机以本地（主机）文件为准对比，只把 hash 不同的文件（客户端缺失或与主机不一致）推给客户端；
// 客户端逐文件分片拉取并覆盖到本地模组目录，全部完成后重载。
// 未修改的文件（hash 一致）不传输，实现"修改过的同步、没修改的不同步"。
public static class ScriptFileSync
{
    private const string SyncChannel = "bark.modsync.files.sync"; // 主机 -> 客户端：广播同步请求
    private const string ReportChannel = "bark.modsync.files.report"; // 客户端 -> 主机：上报文件 hash 清单
    private const string FetchChannel = "bark.modsync.files.fetch"; // 客户端 -> 主机：分片拉取单个文件

    // 单分片原始字节上限（与 HostModFetcher 一致，经 JSON+GZip+base64 封装后单包不超底层消息上限）
    private const int ChunkSize = 8 * 1024;

    private static bool _initialized;

    // 注册网络 handler（客户端上报 + 主机对比 + 主机文件分片）。网络不可用时注册无害。
    public static void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        BarkKrokBridge.RegisterClientHandler(SyncChannel, OnSyncRequested);
        BarkKrokBridge.RegisterServerHandler(ReportChannel, OnFilesReported);
        BarkKrokBridge.RegisterServerHandler(FetchChannel, OnFileFetchRequested);
    }

    // 主机侧入口：sr 重载后调用，向所有客户端广播增量同步请求（携带可同步模组列表）
    public static void TriggerSync()
    {
        if (!BarkKrokBridge.IsAvailable || !(BarkKrokBridge.IsServer || BarkKrokBridge.IsHost))
            return;

        var mods = new JArray();
        foreach (var manifest in ScriptModLoader.LoadedScriptMods.Values)
        {
            if (string.Equals(manifest.NetworkSync, NetworkModSync.SyncServerOnly, StringComparison.Ordinal))
                continue;
            mods.Add(manifest.Id);
        }

        if (mods.Count == 0)
            return;

        LogUtil.Info("script_file_sync.broadcasting", mods.Count);
        BarkKrokBridge.BroadcastToClients(SyncChannel, new JObject { ["mods"] = mods });
    }

    // 客户端：收到主机广播，枚举本地已加载模组的文件 hash 并上报给主机
    private static void OnSyncRequested(JToken payload)
    {
        if (!BarkKrokBridge.IsAvailable || !BarkKrokBridge.IsClient)
            return;

        var report = new JObject();
        if (payload["mods"] is not JArray requestedMods)
            return;

        foreach (var token in requestedMods)
        {
            var modId = token?.Value<string>();
            if (string.IsNullOrWhiteSpace(modId))
                continue;

            // 只对客户端已加载的模组上报（增量更新已存在文件）
            if (!ScriptModLoader.LoadedScriptMods.TryGetValue(modId, out var manifest) ||
                string.IsNullOrEmpty(manifest.Directory) || !Directory.Exists(manifest.Directory))
                continue;

            var fileHashes = new JObject();
            foreach (var filePath in EnumerateFiles(manifest.Directory))
            {
                var rel = GetRelativePath(manifest.Directory, filePath);
                var hash = ComputeFileHash(filePath);
                if (hash != null)
                    fileHashes[rel] = hash;
            }

            if (fileHashes.Count > 0)
                report[modId] = fileHashes;
        }

        if (report.Count == 0)
            return;

        BarkKrokBridge.RequestServer(ReportChannel, new JObject { ["mods"] = report }, ProcessDiffResponse);
    }

    // 主机：对比客户端上报的文件 hash 与主机本地文件，返回 hash 不同的文件路径列表
    private static JToken OnFilesReported(JToken request)
    {
        if (request["mods"] is not JObject reported)
            return ErrorResponse("Missing mods in report");

        var result = new JObject { ["mods"] = new JObject() };
        var resultMods = (JObject)result["mods"]!;

        foreach (var (modId, value) in reported)
        {
            if (value is not JObject clientFiles)
                continue;

            if (!ScriptModLoader.LoadedScriptMods.TryGetValue(modId, out var manifest)
                || string.IsNullOrEmpty(manifest.Directory)
                || !Directory.Exists(manifest.Directory))
                continue;

            // 主机本地文件 hash（以主机为准）
            var hostHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var filePath in EnumerateFiles(manifest.Directory))
            {
                var rel = GetRelativePath(manifest.Directory, filePath);
                var hash = ComputeFileHash(filePath);
                if (hash != null)
                    hostHashes[rel] = hash;
            }

            var diff = new JArray();
            foreach (var kv in from kv in hostHashes
                     let clientHash = clientFiles[kv.Key]?.Value<string>()
                     where !string.Equals(clientHash, kv.Value, StringComparison.OrdinalIgnoreCase)
                     select kv)
            {
                diff.Add(kv.Key);
            }

            if (diff.Count > 0)
                resultMods[modId] = diff;
        }

        return result;
    }

    // 客户端：解析差异列表并启动协程逐文件分片拉取
    private static void ProcessDiffResponse(JToken response)
    {
        if (response["mods"] is not JObject mods)
            return;

        var totalFiles = 0;
        foreach (var mod in mods)
            if (mod.Value is JArray files)
                totalFiles += files.Count;

        if (totalFiles == 0)
            return;

        CUCoreUtils.StartCoroutine(DownloadDiffCoroutine(mods));
    }

    private static IEnumerator DownloadDiffCoroutine(JObject mods)
    {
        foreach (var (modId, value) in mods)
        {
            if (value is not JArray files)
                continue;

            foreach (var fileToken in files)
            {
                var rel = fileToken?.Value<string>();
                if (string.IsNullOrWhiteSpace(rel))
                    continue;

                yield return DownloadFileCoroutine(modId, rel);
            }
        }

        LogUtil.Info("script_file_sync.completed");
        Plugin._scriptModLoader?.ReloadAll();
    }

    // 客户端：分片拉取单个文件内容并落地到本地模组目录
    private static IEnumerator DownloadFileCoroutine(string modId, string rel)
    {
        var buffer = new List<byte>(ChunkSize * 8);
        var complete = false;
        var isDone = false;
        var errorMsg = string.Empty;

        while (!isDone)
        {
            complete = false;
            errorMsg = string.Empty;

            var request = new JObject { ["modId"] = modId, ["path"] = rel, ["offset"] = buffer.Count };
            var sent = BarkKrokBridge.RequestServer(FetchChannel, request, response =>
            {
                complete = true;
                ParseFileChunk(response, buffer, out errorMsg, out isDone);
            });

            if (!sent)
            {
                LogUtil.Warning("script_file_sync.fetch_failed", modId, rel, "send failed");
                yield break;
            }

            while (!complete)
                yield return null;

            if (string.IsNullOrEmpty(errorMsg)) continue;
            LogUtil.Warning("script_file_sync.fetch_failed", modId, rel, errorMsg);
            yield break;
        }

        // 落地：写入客户端已加载模组目录（覆盖同名文件）
        if (!ScriptModLoader.LoadedScriptMods.TryGetValue(modId, out var manifest) ||
            string.IsNullOrEmpty(manifest.Directory))
        {
            LogUtil.Warning("script_file_sync.write_failed", modId, rel, "mod not loaded on client");
            yield break;
        }

        try
        {
            var targetDir = Path.GetDirectoryName(Path.Combine(manifest.Directory, rel));
            if (!string.IsNullOrEmpty(targetDir))
                Directory.CreateDirectory(targetDir);
            File.WriteAllBytes(Path.Combine(manifest.Directory, rel), [.. buffer]);
            LogUtil.Info("script_file_sync.file_updated", modId, rel);
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_file_sync.write_failed", modId, rel, ex.Message);
        }
    }

    // 解析单个文件分片响应，追加到 buffer
    private static void ParseFileChunk(
        JToken response, List<byte> buffer, out string errorMsg, out bool isDone)
    {
        errorMsg = string.Empty;
        isDone = false;

        if (response is not JObject obj)
        {
            errorMsg = "Invalid fetch response";
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
            errorMsg = "Empty chunk";
            return;
        }

        try
        {
            buffer.AddRange(Convert.FromBase64String(chunkBase64));
        }
        catch (FormatException)
        {
            errorMsg = "Invalid chunk base64";
            return;
        }

        isDone = obj["done"]?.Value<bool>() ?? false;
    }

    // 主机：返回单个文件指定偏移的分片
    private static JToken OnFileFetchRequested(JToken request)
    {
        var modId = request["modId"]?.Value<string>();
        var rel = request?["path"]?.Value<string>();
        var offset = request?["offset"]?.Value<int>() ?? 0;

        if (string.IsNullOrWhiteSpace(modId) || string.IsNullOrWhiteSpace(rel))
            return ErrorResponse("Missing modId/path");

        if (!ScriptModLoader.LoadedScriptMods.TryGetValue(modId, out var manifest) ||
            string.IsNullOrEmpty(manifest.Directory) || !Directory.Exists(manifest.Directory))
            return ErrorResponse($"Mod '{modId}' not found on host");

        var fullPath = Path.Combine(manifest.Directory, rel);
        if (!File.Exists(fullPath))
            return ErrorResponse($"File '{rel}' not found on host");

        try
        {
            var total = new FileInfo(fullPath).Length;
            if (offset < 0 || offset > total)
                return ErrorResponse("Invalid offset");

            var length = Math.Min(ChunkSize, total - offset);
            var chunk = new byte[length];
            using (var fs = File.OpenRead(fullPath))
            {
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Read(chunk, 0, (int)length);
            }

            var done = offset + length >= total;
            return new JObject
            {
                ["modId"] = modId,
                ["path"] = rel,
                ["offset"] = offset,
                ["total"] = total,
                ["done"] = done,
                ["chunk"] = Convert.ToBase64String(chunk)
            };
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to read file: {ex.Message}");
        }
    }

    private static JObject ErrorResponse(string message) => new() { ["error"] = message };

    // 递归枚举目录下所有文件
    private static IEnumerable<string> EnumerateFiles(string directory)
    {
        return Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories);
    }

    // 计算文件 SHA256（hex 小写）；netstandard2.1 无 Convert.ToHexString，用 BitConverter 兼容
    private static string? ComputeFileHash(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(stream);
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }
        catch (Exception)
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
            return fullPath[baseDir.Length..].Replace(Path.DirectorySeparatorChar, '/');
        return fullPath.Replace(Path.DirectorySeparatorChar, '/');
    }
}