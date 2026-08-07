using System;
using System.Collections;
using System.Collections.Generic;
using Bark.BetterCCL;
using Bark.Tool;
using CUCoreLib.Helpers;
using CUCoreLib.Networking;
using Newtonsoft.Json.Linq;

namespace Bark.Script;

// 多人游戏脚本模组同步：
// 主机（Server/Client）注册处理器返回模组列表，客户端比对后从 GitHub 下载缺失模组；
// 无 GitHub 仓库或 GitHub 下载失败时，回退为向主机直接请求整个模组目录打包数据。
// 基于 CUCoreLib.Networking.MultiplayerApi 软兼容层，KrokMP 未安装时零开销。
public static class NetworkModSync
{
    // 多人同步模式常量
    public const string SyncOptional = "optional";
    public const string SyncRequired = "required";
    public const string SyncServerOnly = "server_only";

    // 自定义 channel 名称
    private const string ModListChannel = "bark.modsync.list";

    private static string _modsPath = string.Empty;
    private static bool _initialized;
    private static bool _initialSyncRequested;

    // 初始化：注册服务端处理器 + 调度客户端首次同步
    // 必须在 ScriptModLoader.LoadAll() 之后调用（需要访问 LoadedScriptMods）
    public static void Initialize(string modsPath)
    {
        if (_initialized)
            return;

        _initialized = true;
        _modsPath = modsPath;

        // 主机侧注册 fetch 回退通道
        HostModFetcher.Initialize();

        // 检查 CUCoreLib networking 是否可用（KrokMP 已安装）
        if (!MultiplayerBridge.IsAvailable)
        {
            // KrokMP 未加载，调度重试（跟 MultiplayerBridge 同样的延迟 resolve 机制）
            ScheduleRetry();
            return;
        }

        RegisterHandlers();
        ScheduleInitialSync();
    }

    private static void RegisterHandlers()
    {
        // 服务端：返回主机已加载的脚本模组列表
        MultiplayerApi.RegisterServerHandler(ModListChannel, _ =>
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

        CUCoreUtils.CallWhen(
            () => MultiplayerApi.IsAvailable && MultiplayerApi.IsClient && CUCoreUtils.IsInWorld(),
            () =>
            {
                LogUtil.Info("network_sync.requesting");
                // 加入主机：先禁用客机本地所有脚本，改用主机同步（GitHub / 主机 fetch）得到的模组
                ScriptModLoader.DisableAllScripts();
                MultiplayerApi.RequestServer(ModListChannel, null, OnServerModListReceived);
            });
    }

    // 重试：等待 MultiplayerBridge 就绪后注册（用于 KrokMP 延迟加载的情况）
    private static void ScheduleRetry()
    {
        CUCoreUtils.CallWhen(
            () => MultiplayerBridge.IsAvailable,
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

            // GitHub 优先（仅当 source 为 GitHub 时尝试）
            if (mod.Source == ModSource.GitHub && !string.IsNullOrWhiteSpace(mod.Repository))
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
