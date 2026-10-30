using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Bark.Tool;
using CUCoreLib.Helpers;
using Newtonsoft.Json.Linq;

namespace Bark.Script;

public static class HostModFetcher
{
    public const string FetchChannel = "bark.modsync.fetch";
    private const int ChunkSize = 8 * 1024;

    private static readonly Dictionary<string, byte[]> ServerCache =
        new(StringComparer.OrdinalIgnoreCase);

    public static void Initialize()
    {
        if (!BarkMpBridge.IsAvailable)
            return;

        BarkMpBridge.RegisterServerHandler(FetchChannel, OnFetchRequested);
    }

    public static void RequestFromHost(string modId, string modsPath,
        Action<bool, string?> onComplete)
    {
        if (modId is null) throw new ArgumentNullException(nameof(modId));
        if (modsPath is null) throw new ArgumentNullException(nameof(modsPath));
        if (onComplete is null) throw new ArgumentNullException(nameof(onComplete));

        if (!BarkMpBridge.IsAvailable || !BarkMpBridge.IsClient)
        {
            onComplete(false, "Multiplayer is not available on client");
            return;
        }

        CUCoreUtils.StartCoroutine(FetchCoroutine(modId, modsPath, onComplete));
    }

    private static IEnumerator FetchCoroutine(string modId, string modsPath,
        Action<bool, string?> onComplete)
    {
        var buffer = new List<byte>(ChunkSize * 8);
        var complete = false;
        var success = false;
        var errorMsg = string.Empty;
        var isDone = false;

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

            var sent = BarkMpBridge.RequestServer(FetchChannel, request, response =>
            {
                complete = true;
                ParseFetchResponse(response, buffer, out success, out errorMsg,
                    out isDone);
            });

            if (!sent)
            {
                onComplete(false, "Failed to send fetch request to host");
                yield break;
            }

            while (!complete)
                yield return null;

            if (success) continue;
            onComplete(false, errorMsg);
            yield break;
        }

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

    private static void ParseFetchResponse(
        JToken response, List<byte> buffer, out bool success, out string errorMsg,
        out bool isDone)
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

    private static JToken OnFetchRequested(JToken request)
    {
        var modId = request["id"]?.Value<string>();
        var offset = request["offset"]?.Value<int>() ?? 0;

        if (string.IsNullOrWhiteSpace(modId))
            return ErrorResponse("Missing mod id");

        if (!ScriptModLoader.LoadedScriptMods.TryGetValue(modId, out var manifest)
            || string.IsNullOrEmpty(manifest.Directory)
            || !Directory.Exists(manifest.Directory))
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

    private static byte[]? GetCachedZip(string modId, string directory)
    {
        if (ServerCache.TryGetValue(modId, out var cached))
            return cached;

        var packed = PackDirectory(modId, directory);
        if (packed != null)
            ServerCache[modId] = packed;
        return packed;
    }

    private static byte[]? PackDirectory(string modId, string directory)
    {
        try
        {
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var filePath in Directory.EnumerateFiles(directory, "*",
                    SearchOption.AllDirectories))
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

    private static string GetRelativePath(string basePath, string fullPath)
    {
        var baseDir =
            basePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            return fullPath[baseDir.Length..];
        return fullPath;
    }
}
