using System;
using System.Collections;
using Bark.BetterCCL;
using BepInEx.Logging;
using CUCoreLib.Helpers;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Bark.Tool;

public static class UpdateUtil
{
    private const string GitHubApiUrl = "https://api.github.com/repos/{0}/releases/latest";

    public static void Check(string githubRepo, string modName, string currentVersion, ManualLogSource logger)
    {
        if (string.IsNullOrWhiteSpace(githubRepo))
        {
            logger.LogWarning("[" + nameof(UpdateUtil) + "] " +
                              BetterLocale.GetLog("update.no_repo", modName));
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
            logger.LogWarning("[" + nameof(UpdateUtil) + "] " +
                              BetterLocale.GetLog("update.failed", modName));
            yield break;
        }

        var latestTag = TryExtractTagName(request.downloadHandler.text);
        if (string.IsNullOrWhiteSpace(latestTag))
        {
            logger.LogWarning("[" + nameof(UpdateUtil) + "] " +
                              BetterLocale.GetLog("update.no_version", modName));
            yield break;
        }

        if (IsNewer(currentVersion, latestTag!))
        {
            var message = BetterLocale.GetLog("update.available", modName, currentVersion, latestTag!);
            logger.LogWarning(message);
            yield return NotifyConsole(message, true);
        }
        else
        {
            logger.LogInfo("[" + nameof(UpdateUtil) + "] " +
                           BetterLocale.GetLog("update.uptodate", modName, currentVersion));
        }
    }

    private static string? TryExtractTagName(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        const string key = "\"tag_name\":\"";
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

    private static IEnumerator NotifyConsole(string message, bool warning = false)
    {
        ConsoleScript? console = null;
        var attempts = 0;
        while (!console && attempts < 50)
        {
            console = ConsoleScript.instance
                ? ConsoleScript.instance
                : Object.FindObjectOfType<ConsoleScript>();
            if (console) continue;
            attempts++;
            yield return new WaitForSecondsRealtime(0.2f);
        }

        if (!console) yield break;
        var consoleMessage = warning
            ? $"<color=#FFA500>{message}</color>"
            : message;
        CUCoreUtils.ConsoleLog(console, consoleMessage);
    }
}
