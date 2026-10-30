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

public static class ScriptFileSync
{
    private const string SyncChannel = "bark.modsync.files.sync";
    private const string ReportChannel = "bark.modsync.files.report";
    private const string FetchChannel = "bark.modsync.files.fetch";

    private const int ChunkSize = 8 * 1024;

    private static bool _initialized;
    private static string _modsPath = string.Empty;

    public static void Initialize(string modsPath)
    {
        if (_initialized)
            return;

        _initialized = true;
        _modsPath = modsPath;

        BarkMpBridge.RegisterClientHandler(SyncChannel, OnSyncRequested);
        BarkMpBridge.RegisterServerHandler(ReportChannel, OnFilesReported);
        BarkMpBridge.RegisterServerHandler(FetchChannel, OnFileFetchRequested);
    }

    public static void TriggerSync()
    {
        if (!BarkMpBridge.IsAvailable || !(BarkMpBridge.IsServer || BarkMpBridge.IsHost))
            return;

        var mods = new JArray();
        foreach (var manifest in ScriptModLoader.LoadedScriptMods.Values)
        {
            if (string.Equals(manifest.NetworkSync, NetworkModSync.SyncServerOnly,
                StringComparison.Ordinal))
                continue;
            mods.Add(manifest.Id);
        }

        if (mods.Count == 0)
            return;

        LogUtil.Info("script_file_sync.broadcasting", mods.Count);
        BarkMpBridge.BroadcastToClients(SyncChannel, new JObject { ["mods"] = mods });
    }

    private static void OnSyncRequested(JToken payload)
    {
        if (!BarkMpBridge.IsAvailable || !BarkMpBridge.IsClient)
            return;

        var report = new JObject();
        if (payload["mods"] is not JArray requestedMods)
            return;

        foreach (var token in requestedMods)
        {
            var modId = token?.Value<string>();
            if (string.IsNullOrWhiteSpace(modId))
                continue;

            if (ScriptModLoader.LoadedScriptMods.TryGetValue(modId, out var manifest)
                && !string.IsNullOrEmpty(manifest.Directory)
                && Directory.Exists(manifest.Directory))
            {
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
            else
            {
                report[modId] = null;
            }
        }

        if (report.Count == 0)
            return;

        BarkMpBridge.RequestServer(ReportChannel, new JObject { ["mods"] = report },
            ProcessDiffResponse);
    }

    private static JToken OnFilesReported(JToken request)
    {
        if (request["mods"] is not JObject reported)
            return ErrorResponse("Missing mods in report");

        var result = new JObject { ["mods"] = new JObject() };
        var resultMods = (JObject)result["mods"]!;

        foreach (var (modId, value) in reported)
        {
            if (!ScriptModLoader.LoadedScriptMods.TryGetValue(modId, out var manifest)
                || string.IsNullOrEmpty(manifest.Directory)
                || !Directory.Exists(manifest.Directory))
                continue;

            var hostHashes =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var filePath in EnumerateFiles(manifest.Directory))
            {
                var rel = GetRelativePath(manifest.Directory, filePath);
                var hash = ComputeFileHash(filePath);
                if (hash != null)
                    hostHashes[rel] = hash;
            }

            var diff = new JArray();
            if (value is JObject clientFiles)
                foreach (var kv in
                    from kv in hostHashes
                    let clientHash = clientFiles[kv.Key]?.Value<string>()
                    where !string.Equals(clientHash, kv.Value,
                        StringComparison.OrdinalIgnoreCase)
                    select kv)
                    diff.Add(kv.Key);
            else
                foreach (var rel in hostHashes.Keys)
                    diff.Add(rel);

            if (diff.Count > 0)
                resultMods[modId] = diff;
        }

        return result;
    }

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

            var request = new JObject
                { ["modId"] = modId, ["path"] = rel, ["offset"] = buffer.Count };
            var sent = BarkMpBridge.RequestServer(FetchChannel, request, response =>
            {
                complete = true;
                ParseFileChunk(response, buffer, out errorMsg, out isDone);
            });

            if (!sent)
            {
                LogUtil.Warning("script_file_sync.fetch_failed", modId, rel,
                    "send failed");
                yield break;
            }

            while (!complete)
                yield return null;

            if (string.IsNullOrEmpty(errorMsg)) continue;
            LogUtil.Warning("script_file_sync.fetch_failed", modId, rel, errorMsg);
            yield break;
        }

        var modDir = ScriptModLoader.LoadedScriptMods.TryGetValue(modId, out var manifest)
            && !string.IsNullOrEmpty(manifest.Directory)
                ? manifest.Directory
                : Path.Combine(_modsPath, "Mods", modId);

        try
        {
            var targetDir = Path.GetDirectoryName(Path.Combine(modDir, rel));
            if (!string.IsNullOrEmpty(targetDir))
                Directory.CreateDirectory(targetDir);
            File.WriteAllBytes(Path.Combine(modDir, rel), [.. buffer]);
            LogUtil.Info("script_file_sync.file_updated", modId, rel);
        }
        catch (Exception ex)
        {
            LogUtil.Warning("script_file_sync.write_failed", modId, rel, ex.Message);
        }
    }

    private static void ParseFileChunk(
        JToken response,
        List<byte> buffer,
        out string errorMsg,
        out bool isDone)
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

        var done = obj["done"]?.Value<bool>() ?? false;
        var chunkBase64 = obj["chunk"]?.Value<string>();

        if (string.IsNullOrEmpty(chunkBase64))
        {
            if (done)
            {
                isDone = true;
                return;
            }

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

        isDone = done;
    }

    private static JToken OnFileFetchRequested(JToken request)
    {
        var modId = request["modId"]?.Value<string>();
        var rel = request["path"]?.Value<string>();
        var offset = request["offset"]?.Value<int>() ?? 0;

        if (string.IsNullOrWhiteSpace(modId) || string.IsNullOrWhiteSpace(rel))
            return ErrorResponse("Missing modId/path");

        if (!ScriptModLoader.LoadedScriptMods.TryGetValue(modId, out var manifest)
            || string.IsNullOrEmpty(manifest.Directory)
            || !Directory.Exists(manifest.Directory))
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
                var read = 0;
                while (read < (int)length)
                {
                    var n = fs.Read(chunk, read, (int)length - read);
                    if (n <= 0)
                        return ErrorResponse("Incomplete read");
                    read += n;
                }
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

    private static JObject ErrorResponse(string message)
    {
        return new JObject { ["error"] = message };
    }

    private static IEnumerable<string> EnumerateFiles(string directory)
    {
        return Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories);
    }

    private static string? ComputeFileHash(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(stream);
            return BitConverter.ToString(bytes).Replace("-", string.Empty)
                .ToLowerInvariant();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string GetRelativePath(string basePath, string fullPath)
    {
        var baseDir =
            basePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            return fullPath[baseDir.Length..].Replace(Path.DirectorySeparatorChar, '/');
        return fullPath.Replace(Path.DirectorySeparatorChar, '/');
    }
}
