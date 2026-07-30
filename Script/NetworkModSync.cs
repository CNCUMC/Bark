using System;
using System.Collections;
using System.Collections.Generic;
using Bark.Tool;
using CUCoreLib.Helpers;
using CUCoreLib.Networking;
using Newtonsoft.Json.Linq;

namespace Bark.Script;

// 多人游戏脚本模组同步：
// 主机（Server/Client）注册处理器返回模组列表，客户端比对后从 GitHub 下载缺失模组
// 基于 CUCoreLib.Networking.MultiplayerApi 软兼容层，KrokMP 不安装时零开销
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
            {
                mods.Add(new JObject
                {
                    ["id"] = manifest.Id,
                    ["name"] = manifest.Name,
                    ["version"] = manifest.Version,
                    ["repository"] = manifest.Repository ?? string.Empty,
                    ["network_sync"] = manifest.NetworkSync ?? SyncOptional
                });
            }

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
        var missingMods = new List<(string id, string name, string version, string repository)>();

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

            // 已经加载，跳过
            if (localMods.ContainsKey(id))
                continue;

            // 没有 GitHub 仓库地址，无法自动下载
            if (string.IsNullOrWhiteSpace(repository))
            {
                LogUtil.Info("network_sync.no_repo", name);
                continue;
            }

            missingMods.Add((id, name, version, repository));
        }

        if (missingMods.Count == 0)
        {
            LogUtil.Info("network_sync.already_match", localMods.Count);
            return;
        }

        LogUtil.Info("network_sync.found_missing", missingMods.Count, localMods.Count);

        // 启动协程逐个下载缺失模组
        CUCoreUtils.StartCoroutine(
            DownloadAndReloadCoroutine(missingMods));
    }

    // 逐个下载缺失模组，全部完成后触发重载
    private static IEnumerator DownloadAndReloadCoroutine(
        List<(string id, string name, string version, string repository)> missingMods)
    {
        var downloaded = 0;
        var failed = 0;

        foreach (var mod in missingMods)
        {
            var complete = false;
            var success = false;
            var errorMsg = string.Empty;

            LogUtil.Info("network_sync.downloading", mod.name, mod.version);
            ModSyncDownloader.DownloadLatestZip(
                mod.repository, mod.id, _modsPath,
                (ok, err) =>
                {
                    complete = true;
                    success = ok;
                    errorMsg = err ?? string.Empty;
                });

            // 等待下载完成（协程挂起等待回调）
            while (!complete)
                yield return null;

            if (success)
            {
                downloaded++;
                LogUtil.Info("network_sync.downloaded", mod.name);
            }
            else
            {
                failed++;
                LogUtil.Warning("network_sync.download_failed", mod.name, errorMsg);
            }
        }

        LogUtil.Info("network_sync.summary", downloaded, failed);

        if (downloaded <= 0) yield break;
        LogUtil.Info("network_sync.reloading");
        Plugin._scriptModLoader?.ReloadAll();
    }
}