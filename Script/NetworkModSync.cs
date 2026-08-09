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

// 多人游戏脚本模组同步：
// 主机（Server/Client）注册处理器返回模组列表，客户端比对后从 GitHub 下载缺失模组；
// 无 GitHub 仓库或 GitHub 下载失败时，回退为向主机直接请求整个模组目录打包数据。
// 网络底层走 Bark 自建 BarkKrokBridge（直接反射 KrokMP 4.0.1），绕开 CUCoreLib MultiplayerBridge
// 因 knetid 类型不兼容导致的 IsAvailable 恒 false 问题；KrokMP 未安装时零开销。
public static class NetworkModSync
{
    // 多人同步模式常量
    public const string SyncOptional = "optional";
    public const string SyncRequired = "required";
    public const string SyncServerOnly = "server_only";

    // 自定义 channel 名称
    private const string ModListChannel = "bark.modsync.list";

    // 已下载 zip 的 hash 缓存文件名（位于 Plugin.BarkCachePath），
    // 用于判断本地 Mods/{modId}.zip 是否仍是上次同步下载的内容，避免重复下载。
    private const string SyncHashCacheFile = "sync_scripts_hash.txt";

    private static string _modsPath = string.Empty;
    private static bool _initialized;
    private static bool _initialSyncRequested;

    // modId -> zip SHA256 的缓存（惰性从磁盘加载）
    private static readonly Dictionary<string, string> SyncHashCache = new(StringComparer.OrdinalIgnoreCase);
    private static bool _syncHashCacheLoaded;

    // 初始化：注册服务端处理器 + 调度客户端首次同步
    // 必须在 ScriptModLoader.LoadAll() 之后调用（需要访问 LoadedScriptMods）
    public static void Initialize(string modsPath)
    {
        if (_initialized)
            return;

        _initialized = true;
        _modsPath = modsPath;

        // 解析 KrokMP 并注册接收器（反射一次性完成并缓存句柄）
        BarkKrokBridge.Initialize();

        // 主机侧注册 fetch 回退通道
        HostModFetcher.Initialize();

        // 检查自建 KrokMP 网络层是否可用
        if (!BarkKrokBridge.IsAvailable)
        {
            // KrokMP 未加载或反射解析失败，调度重试
            ScheduleRetry();
            return;
        }

        RegisterHandlers();
        ScheduleInitialSync();
    }

    private static void RegisterHandlers()
    {
        // 服务端：返回主机已加载的脚本模组列表
        BarkKrokBridge.RegisterServerHandler(ModListChannel, _ =>
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

    // 调度客户端首次同步：等待条件就绪后请求主机模组列表
    private static void ScheduleInitialSync()
    {
        if (_initialSyncRequested)
            return;

        _initialSyncRequested = true;

        // 触发条件：网络层就绪 + 客机 + 已连接服务器 + 本地玩家已创建。
        // 关键1：KrokMP 的 Client_Send 在 NetPlayer.LOCAL_PLAYER == null（本地玩家未创建，进世界前）时
        //       会触发 "SENDING A PACKET TOO EARLY" 并 Net.ShutdownReset() 断开连接，导致请求发不出去。
        // 关键2：连接刚建立时 KrokMP 的连接栈（connectionMapping 到房主）可能尚未就绪，
        //       Client_Send 会静默丢弃消息。因此请求需带延迟 + 重试，确保连接栈就绪后能送达。
        // 不使用 CUCoreLib.IsInWorld()——它在 KrokMP 客机上始终为 false（worldExists 判断不准确）。
        CUCoreUtils.CallWhen(
            () => BarkKrokBridge.IsAvailable && BarkKrokBridge.IsClient &&
                  BarkKrokBridge.IsConnected && BarkKrokBridge.HasLocalPlayer,
            () => CUCoreUtils.StartCoroutine(RetrySyncCoroutine()));
    }

    // 客机同步请求协程：延迟后发送，超时未收到响应则重试，直到成功或达到上限
    private static IEnumerator RetrySyncCoroutine()
    {
        const int maxAttempts = 5;
        const float sendDelay = 2f;   // 每次请求前延迟，给 KrokMP 连接栈就绪时间
        const float timeout = 6f;     // 等待响应的超时秒数

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            // 给 KrokMP 连接栈（connectionMapping 到房主）就绪时间
            yield return new WaitForSecondsRealtime(sendDelay);

            LogUtil.Info("network_sync.requesting");
            // 加入主机：先禁用客机本地所有脚本，改用主机同步（GitHub / 主机 fetch）得到的模组
            ScriptModLoader.DisableAllScripts();

            var done = false;
            BarkKrokBridge.RequestServer(ModListChannel, null, response =>
            {
                done = true;
                OnServerModListReceived(response);
            });

            // 等待响应或超时
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

    // 重试：等待 BarkKrokBridge 就绪后注册（用于 KrokMP 延迟加载或首次反射解析失败的情况）
    private static void ScheduleRetry()
    {
        CUCoreUtils.CallWhen(
            () => BarkKrokBridge.IsAvailable,
            () =>
            {
                HostModFetcher.Initialize();
                RegisterHandlers();
                ScheduleInitialSync();
                LogUtil.Info("network_sync.ready");
            });
    }

    // 客户端收到主机模组列表后的处理
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

            // 跳过服务端专属模组（客户端不需要）
            if (string.Equals(networkSync, SyncServerOnly, StringComparison.Ordinal))
            {
                LogUtil.Info("network_sync.skip_server_only", name);
                continue;
            }

            // 本地已存在：仅做版本校验
            if (localMods.TryGetValue(id, out var local))
            {
                if (!string.Equals(local.Version, version, StringComparison.Ordinal))
                {
                    // 版本不一致：required 强制重下（优先主机源，因本地版本不可信），optional 仅告警
                    if (string.Equals(networkSync, SyncRequired, StringComparison.Ordinal))
                    {
                        LogUtil.Warning("network_sync.version_mismatch_required", name, local.Version, version);
                        toDownload.Add(new MissingMod(id, name, version, repository, networkSync, ModSource.Host));
                    }
                    else
                    {
                        LogUtil.Warning("network_sync.version_mismatch", name, local.Version, version);
                    }
                }

                continue;
            }

            // 本地缺失：无 repository 直接归为「主机回退」，否则先试 GitHub
            var source = string.IsNullOrWhiteSpace(repository) ? ModSource.Host : ModSource.GitHub;
            toDownload.Add(new MissingMod(id, name, version, repository, networkSync, source));
        }

        if (toDownload.Count == 0)
        {
            LogUtil.Info("network_sync.already_match", localMods.Count);
            return;
        }

        LogUtil.Info("network_sync.found_missing", toDownload.Count, localMods.Count);

        // 启动协程逐个下载缺失模组
        CUCoreUtils.StartCoroutine(
            DownloadAndReloadCoroutine(toDownload));
    }

    // 逐个下载缺失模组，全部完成后触发重载
    private static IEnumerator DownloadAndReloadCoroutine(List<MissingMod> missingMods)
    {
        var downloaded = 0;
        var failed = 0;
        var requiredFailed = new List<MissingMod>();

        foreach (var mod in missingMods)
        {
            var ok = false;
            var usedHost = false;

            // 已下载缓存命中：Mods/{id}.zip 存在且 hash 与上次同步一致 → 跳过网络下载
            if (TryGetCachedZip(mod.Id))
            {
                ok = true;
                LogUtil.Info("network_sync.zip_cache_hit", mod.Name);
            }

            // GitHub 优先（仅当 source 为 GitHub 时尝试）
            if (!ok && mod.Source == ModSource.GitHub && !string.IsNullOrWhiteSpace(mod.Repository))
            {
                LogUtil.Info("network_sync.downloading", mod.Name, mod.Version);
                yield return DownloadGitHubCoroutine(mod, result => ok = result);
            }

            // GitHub 失败或无仓库：回退主机 fetch
            if (!ok)
            {
                usedHost = true;
                LogUtil.Info("network_sync.host_fetching", mod.Name, mod.Version);
                yield return DownloadHostCoroutine(mod, result => ok = result);
            }

            if (ok)
            {
                // 下载成功（含缓存命中）后记录 zip hash，供下次同步判断
                RecordZipHash(mod.Id);
                downloaded++;
                LogUtil.Info(usedHost ? "network_sync.host_fetched" : "network_sync.downloaded", mod.Name);
            }
            else
            {
                failed++;
                LogUtil.Warning("network_sync.download_failed", mod.Name,
                    usedHost ? "host fetch failed" : "github download failed");
                if (string.Equals(mod.NetworkSync, SyncRequired, StringComparison.Ordinal))
                    requiredFailed.Add(mod);
            }
        }

        LogUtil.Info("network_sync.summary", downloaded, failed);

        // required 模组最终失败：拒绝进入（安全降级，见 HandleRequiredFailure）
        if (requiredFailed.Count > 0)
        {
            HandleRequiredFailure(requiredFailed);
            yield break;
        }

        if (downloaded <= 0) yield break;
        LogUtil.Info("network_sync.reloading");
        Plugin._scriptModLoader?.ReloadAll();
    }

    // GitHub 下载协程（同步等待 UnityWebRequest 回调）
    private static IEnumerator DownloadGitHubCoroutine(MissingMod mod, Action<bool> onResult)
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

    // 主机 fetch 下载协程（同步等待网络回调）
    private static IEnumerator DownloadHostCoroutine(MissingMod mod, Action<bool> onResult)
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

    // required 模组同步失败：不重载（避免脚本状态不一致），并通知玩家需断开
    private static void HandleRequiredFailure(List<MissingMod> failedMods)
    {
        foreach (var mod in failedMods)
            LogUtil.Error("network_sync.required_failed", mod.Name, mod.Version);

        LogUtil.Error("network_sync.required_block_enter");
        // 安全降级：不调用 ReloadAll，保持本地状态；通过游戏内提示要求玩家断开重连或安装模组
        PlayerUtil.Alert(
            BetterLocale.GetLog($"{Plugin.NameSpace}.network_sync.required_block_enter"),
            true);
    }

    // 判断 Mods/{modId}.zip 是否仍为上次同步下载的内容（存在且 hash 与缓存一致）
    private static bool TryGetCachedZip(string modId)
    {
        EnsureSyncHashCacheLoaded();
        if (!SyncHashCache.TryGetValue(modId, out var cachedHash))
            return false;

        var zipPath = ZipPathFor(modId);
        if (!File.Exists(zipPath))
            return false;

        var actualHash = ComputeFileHash(zipPath);
        return actualHash != null &&
               string.Equals(actualHash, cachedHash, StringComparison.OrdinalIgnoreCase);
    }

    // 计算 Mods/{modId}.zip 的 SHA256 并写入缓存文件
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

    private static string SyncHashCachePath => Path.Combine(Plugin.BarkCachePath, SyncHashCacheFile);

    private static string ZipPathFor(string modId) => Path.Combine(_modsPath, "Mods", $"{modId}.zip");

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

    // 缺失模组记录（含下载源与同步模式）
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

    // 下载源：优先 GitHub，失败或无仓库时回退主机
    private enum ModSource
    {
        GitHub,
        Host
    }
}
