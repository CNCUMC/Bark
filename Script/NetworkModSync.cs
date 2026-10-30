using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Bark.BetterCCL;
using Bark.Tool;
using CUCoreLib.Helpers;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Bark.Script;

public static class NetworkModSync
{
    public const string SyncOptional = "optional";
    public const string SyncRequired = "required";
    public const string SyncServerOnly = "server_only";

    private const string ModListChannel = "bark.modsync.list";
    private const string SyncHashCacheFile = "sync_scripts_hash.txt";
    private static string _modsPath = string.Empty;
    private static bool _initialized;
    private static bool _initialSyncRequested;

    private static readonly Dictionary<string, string> SyncHashCache =
        new(StringComparer.OrdinalIgnoreCase);

    private static bool _syncHashCacheLoaded;

    private static string SyncHashCachePath =>
        Path.Combine(Plugin.BarkCachePath, SyncHashCacheFile);

    public static void Initialize(string modsPath)
    {
        if (_initialized)
            return;

        _initialized = true;
        _modsPath = modsPath;

        BarkMpBridge.Initialize();
        HostModFetcher.Initialize();

        if (!BarkMpBridge.IsAvailable)
        {
            ScheduleRetry();
            return;
        }

        RegisterHandlers();
        ScheduleInitialSync();
    }

    private static void RegisterHandlers()
    {
        BarkMpBridge.RegisterServerHandler(ModListChannel, _ =>
        {
            var mods = new JArray();
            foreach (var manifest in ScriptModLoader.LoadedScriptMods.Values)
                mods.Add(new JObject
                {
                    ["id"] = manifest.Id,
                    ["name"] = manifest.Name,
                    ["version"] = manifest.Version,
                    ["repository"] = manifest.Repository ?? string.Empty,
                    ["network_sync"] = manifest.NetworkSync ?? SyncOptional
                });

            return new JObject { ["mods"] = mods };
        });
    }

    private static void ScheduleInitialSync()
    {
        if (_initialSyncRequested)
            return;

        _initialSyncRequested = true;

        CUCoreUtils.CallWhen(
            () => BarkMpBridge.IsAvailable
                && BarkMpBridge.IsClient
                && BarkMpBridge.IsConnected
                && BarkMpBridge.HasLocalPlayer,
            () => CUCoreUtils.StartCoroutine(RetrySyncCoroutine()));
    }

    private static IEnumerator RetrySyncCoroutine()
    {
        const int maxAttempts = 5;
        const float sendDelay = 2f;
        const float timeout = 6f;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            yield return new WaitForSecondsRealtime(sendDelay);

            LogUtil.Info("network_sync.requesting");
            ScriptModLoader.DisableAllScripts();

            var done = false;
            BarkMpBridge.RequestServer(ModListChannel, null, response =>
            {
                done = true;
                OnServerModListReceived(response);
            });

            var waited = 0f;
            while (!done && waited < timeout)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            if (done)
            {
                LogUtil.Info("network_sync.retry_done", attempt);
                yield break;
            }

            LogUtil.Warning("network_sync.retry", attempt, maxAttempts);
        }
    }

    private static void ScheduleRetry()
    {
        CUCoreUtils.CallWhen(
            () => BarkMpBridge.IsAvailable,
            () =>
            {
                HostModFetcher.Initialize();
                RegisterHandlers();
                ScheduleInitialSync();
                LogUtil.Info("network_sync.ready");
            });
    }

    private static void OnServerModListReceived(JToken response)
    {
        if (response["mods"] is not JArray hostMods || hostMods.Count == 0)
        {
            LogUtil.Info("network_sync.no_host_mods");
            return;
        }

        var localMods = ScriptModLoader.LoadedScriptMods;
        var toDownload = new List<MissingMod>();

        foreach (var hostMod in hostMods)
        {
            var id = hostMod["id"]?.Value<string>();
            var name = hostMod["name"]?.Value<string>() ?? id ?? "?";
            var version = hostMod["version"]?.Value<string>() ?? "?";
            var repository = hostMod["repository"]?.Value<string>() ?? string.Empty;
            var networkSync = hostMod["network_sync"]?.Value<string>() ?? SyncOptional;

            if (string.IsNullOrWhiteSpace(id))
                continue;

            if (string.Equals(networkSync, SyncServerOnly, StringComparison.Ordinal))
            {
                LogUtil.Info("network_sync.skip_server_only", name);
                continue;
            }

            if (localMods.TryGetValue(id, out var local))
            {
                if (!string.Equals(local.Version, version, StringComparison.Ordinal))
                {
                    if (string.Equals(networkSync, SyncRequired,
                        StringComparison.Ordinal))
                    {
                        LogUtil.Warning("network_sync.version_mismatch_required", name,
                            local.Version, version);
                        toDownload.Add(new MissingMod(id, name, version, repository,
                            networkSync, ModSource.Host));
                    }
                    else
                    {
                        LogUtil.Warning("network_sync.version_mismatch", name,
                            local.Version, version);
                    }
                }

                continue;
            }

            var source = string.IsNullOrWhiteSpace(repository)
                ? ModSource.Host
                : ModSource.GitHub;
            toDownload.Add(new MissingMod(id, name, version, repository, networkSync,
                source));
        }

        if (toDownload.Count == 0)
        {
            LogUtil.Info("network_sync.already_match", localMods.Count);
            return;
        }

        LogUtil.Info("network_sync.found_missing", toDownload.Count, localMods.Count);

        CUCoreUtils.StartCoroutine(
            DownloadAndReloadCoroutine(toDownload));
    }

    private static IEnumerator DownloadAndReloadCoroutine(List<MissingMod> missingMods)
    {
        var downloaded = 0;
        var failed = 0;
        var requiredFailed = new List<MissingMod>();

        foreach (var mod in missingMods)
        {
            var ok = false;
            var usedHost = false;

            if (TryGetCachedZip(mod.Id))
            {
                ok = true;
                LogUtil.Info("network_sync.zip_cache_hit", mod.Name);
            }

            if (!ok
                && mod.Source == ModSource.GitHub
                && !string.IsNullOrWhiteSpace(mod.Repository))
            {
                LogUtil.Info("network_sync.downloading", mod.Name, mod.Version);
                yield return DownloadGitHubCoroutine(mod, result => ok = result);
            }

            if (!ok)
            {
                usedHost = true;
                LogUtil.Info("network_sync.host_fetching", mod.Name, mod.Version);
                yield return DownloadHostCoroutine(mod, result => ok = result);
            }

            if (ok)
            {
                RecordZipHash(mod.Id);
                downloaded++;
                LogUtil.Info(usedHost
                    ? "network_sync.host_fetched"
                    : "network_sync.downloaded", mod.Name);
            }
            else
            {
                failed++;
                LogUtil.Warning("network_sync.download_failed", mod.Name,
                    usedHost
                        ? "host fetch failed"
                        : "github download failed");
                if (string.Equals(mod.NetworkSync, SyncRequired,
                    StringComparison.Ordinal))
                    requiredFailed.Add(mod);
            }
        }

        LogUtil.Info("network_sync.summary", downloaded, failed);

        if (requiredFailed.Count > 0)
        {
            HandleRequiredFailure(requiredFailed);
            yield break;
        }

        if (downloaded <= 0) yield break;
        LogUtil.Info("network_sync.reloading");
        Plugin._scriptModLoader?.ReloadAll();
    }

    private static IEnumerator DownloadGitHubCoroutine(MissingMod mod,
        Action<bool> onResult)
    {
        var complete = false;
        var success = false;
        var errorMsg = string.Empty;

        ModSyncDownloader.DownloadLatestZip(
            mod.Repository, mod.Id, _modsPath,
            (ok, err) =>
            {
                complete = true;
                success = ok;
                errorMsg = err ?? string.Empty;
            });

        while (!complete)
            yield return null;

        if (!success)
            LogUtil.Warning("network_sync.github_failed", mod.Name, errorMsg);

        onResult(success);
    }

    private static IEnumerator DownloadHostCoroutine(MissingMod mod,
        Action<bool> onResult)
    {
        var complete = false;
        var success = false;
        var errorMsg = string.Empty;

        HostModFetcher.RequestFromHost(
            mod.Id, _modsPath,
            (ok, err) =>
            {
                complete = true;
                success = ok;
                errorMsg = err ?? string.Empty;
            });

        while (!complete)
            yield return null;

        if (!success)
            LogUtil.Warning("network_sync.host_fetch_failed", mod.Name, errorMsg);

        onResult(success);
    }

    private static void HandleRequiredFailure(List<MissingMod> failedMods)
    {
        foreach (var mod in failedMods)
            LogUtil.Error("network_sync.required_failed", mod.Name, mod.Version);

        LogUtil.Error("network_sync.required_block_enter");
        PlayerUtil.Alert(
            BetterLocale.GetLog($"{Plugin.NameSpace}.network_sync.required_block_enter"),
            true);
    }

    private static bool TryGetCachedZip(string modId)
    {
        EnsureSyncHashCacheLoaded();
        if (!SyncHashCache.TryGetValue(modId, out var cachedHash))
            return false;

        var zipPath = ZipPathFor(modId);
        if (!File.Exists(zipPath))
            return false;

        var actualHash = ComputeFileHash(zipPath);
        return actualHash != null
            && string.Equals(actualHash, cachedHash, StringComparison.OrdinalIgnoreCase);
    }

    private static void RecordZipHash(string modId)
    {
        try
        {
            EnsureSyncHashCacheLoaded();
            var zipPath = ZipPathFor(modId);
            if (!File.Exists(zipPath))
                return;

            var hash = ComputeFileHash(zipPath);
            if (hash is null)
                return;

            SyncHashCache[modId] = hash;

            var sb = new StringBuilder();
            foreach (var kv in SyncHashCache)
                sb.Append(kv.Key).Append('=').Append(kv.Value).Append('\n');

            Directory.CreateDirectory(Plugin.BarkCachePath);
            File.WriteAllText(SyncHashCachePath, sb.ToString());
        }
        catch (Exception ex)
        {
            LogUtil.Warning("network_sync.hash_cache_write_failed", ex.Message);
        }
    }

    private static void EnsureSyncHashCacheLoaded()
    {
        if (_syncHashCacheLoaded)
            return;

        _syncHashCacheLoaded = true;
        try
        {
            if (!File.Exists(SyncHashCachePath))
                return;

            foreach (var line in File.ReadAllLines(SyncHashCachePath))
            {
                var idx = line.IndexOf('=');
                if (idx <= 0)
                    continue;

                var id = line[..idx].Trim();
                var hash = line[(idx + 1)..].Trim();
                if (id.Length > 0 && hash.Length > 0)
                    SyncHashCache[id] = hash;
            }
        }
        catch (Exception ex)
        {
            LogUtil.Warning("network_sync.hash_cache_read_failed", ex.Message);
        }
    }

    private static string ZipPathFor(string modId)
    {
        return Path.Combine(_modsPath, "Mods", $"{modId}.zip");
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

    private sealed class MissingMod(
        string id,
        string name,
        string version,
        string repository,
        string networkSync,
        ModSource source)
    {
        public string Id { get; } = id;
        public string Name { get; } = name;
        public string Version { get; } = version;
        public string Repository { get; } = repository;
        public string NetworkSync { get; } = networkSync;
        public ModSource Source { get; } = source;
    }

    private enum ModSource
    {
        GitHub,
        Host
    }
}
