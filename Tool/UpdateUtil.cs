using System;
using System.Collections;
using Bark.BetterCCL;
using BepInEx.Logging;
using CUCoreLib.Helpers;
using UnityEngine.Networking;

namespace Bark.Tool;

public static class UpdateUtil
{
    private const string GitHubApiUrl = "https://api.github.com/repos/{0}/releases/latest";

    public static void Check(string githubRepo, string modName, string currentVersion, ManualLogSource logger)
    {
        if (string.IsNullOrWhiteSpace(githubRepo))
        {
            LogUtil.Warning(LocaleLog("update.no_repo", modName), logger);
            return;
        }

        CUCoreUtils.StartCoroutine(CheckRoutine(githubRepo, modName, currentVersion, logger));
    }

    private static IEnumerator CheckRoutine(
        string githubRepo, string modName, string currentVersion, ManualLogSource logger)
    {
        var url = string.Format(GitHubApiUrl, githubRepo);
        using var request = UnityWebRequest.Get(url);
        request.SetRequestHeader("User-Agent", modName);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            LogUtil.Warning(LocaleLog("update.failed", modName), logger);
            yield break;
        }

        var latestTag = TryExtractTagName(request.downloadHandler.text);
        if (string.IsNullOrWhiteSpace(latestTag))
        {
            LogUtil.Warning(LocaleLog("update.no_version", modName), logger);
            yield break;
        }

        if (IsNewer(currentVersion, latestTag!))
            LogUtil.Warning(LocaleLog("update.available", modName, currentVersion, latestTag!), logger);
        else
            LogUtil.Info(LocaleLog("update.up_to_date", modName, currentVersion), logger);
    }

    private static string? TryExtractTagName(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        const string key = """
                           "tag_name":"
                           """;
        var start = json.IndexOf(key, StringComparison.Ordinal);
        if (start < 0) return null;

        start += key.Length;
        var end = json.IndexOf('"', start);
        return end <= start ? null : json.Substring(start, end - start);
    }

    private static bool IsNewer(string current, string latest)
    {
        var cur = NormalizeVersion(current);
        var lat = NormalizeVersion(latest);
        return Version.TryParse(cur, out var cv) &&
               Version.TryParse(lat, out var lv) &&
               lv > cv;
    }

    private static string NormalizeVersion(string version)
    {
        return version.Trim().TrimStart('v', 'V');
    }

    private static string LocaleLog(string text, params object[] args)
    {
        return BetterLocale.GetLog(text, args);
    }
}