using System;
using System.Collections;
using System.IO;
using System.Linq;
using CUCoreLib.Helpers;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace Bark.Script;

// GitHub Release .zip 下载器：从 GitHub 仓库的最新 Release 中下载 .zip asset
public static class ModSyncDownloader
{
    private const string GitHubApiTemplate = "https://api.github.com/repos/{0}/releases/latest";

    // 下载单个模组的 zip 包到 {modsPath}/Mods/{modId}.zip
    // 回调在 Unity 主线程执行
    public static void DownloadLatestZip(
        string repository,
        string modId,
        string modsPath,
        Action<bool, string?> onComplete)
    {
        if (string.IsNullOrWhiteSpace(repository))
        {
            onComplete(false, "Repository URL is empty");
            return;
        }

        var repo = ParseRepo(repository);
        if (!repo.HasValue)
        {
            onComplete(false, $"Invalid repository URL: {repository}");
            return;
        }

        CUCoreUtils.StartCoroutine(DownloadCoroutine(repo.Value, modId, modsPath, onComplete));
    }

    private static IEnumerator DownloadCoroutine(
        (string owner, string name) repo,
        string modId,
        string modsPath,
        Action<bool, string?> onComplete)
    {
        // 1. 获取最新 Release 的 .zip asset URL
        var apiUrl = string.Format(GitHubApiTemplate, $"{repo.owner}/{repo.name}");
        using var apiRequest = UnityWebRequest.Get(apiUrl);
        apiRequest.SetRequestHeader("User-Agent", "Bark-ModSync");
        apiRequest.SetRequestHeader("Accept", "application/vnd.github.v3+json");

        yield return apiRequest.SendWebRequest();

        if (apiRequest.result != UnityWebRequest.Result.Success)
        {
            onComplete(false, $"GitHub API request failed (HTTP {apiRequest.responseCode}): {apiRequest.error}");
            yield break;
        }

        var zipUrl = ExtractZipAssetUrl(apiRequest.downloadHandler.text);
        if (zipUrl == null)
        {
            onComplete(false, $"No .zip asset found in latest release of {repo.owner}/{repo.name}");
            yield break;
        }

        // 2. 下载 zip 文件
        using var downloadRequest = UnityWebRequest.Get(zipUrl);
        downloadRequest.SetRequestHeader("User-Agent", "Bark-ModSync");

        yield return downloadRequest.SendWebRequest();

        if (downloadRequest.result != UnityWebRequest.Result.Success)
        {
            onComplete(false, $"Download failed (HTTP {downloadRequest.responseCode}): {downloadRequest.error}");
            yield break;
        }

        var data = downloadRequest.downloadHandler.data;
        if (data == null || data.Length == 0)
        {
            onComplete(false, "Downloaded zip is empty");
            yield break;
        }

        // 3. 写入 ScriptMod/Mods/{modId}.zip
        try
        {
            var modsDir = Path.Combine(modsPath, "Mods");
            Directory.CreateDirectory(modsDir);
            var zipPath = Path.Combine(modsDir, $"{modId}.zip");
            File.WriteAllBytes(zipPath, data);
            onComplete(true, null);
        }
        catch (Exception ex)
        {
            onComplete(false, $"Failed to write zip file: {ex.Message}");
        }
    }

    // 从 GitHub Release JSON 中提取 .zip asset 的 browser_download_url
    private static string? ExtractZipAssetUrl(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var release = JObject.Parse(json);
            if (release["assets"] is not JArray assets || assets.Count == 0)
                return null;

            foreach (var asset in assets)
            {
                var name = asset?["name"]?.Value<string>();
                if (name == null || !name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) continue;
                var url = asset?["browser_download_url"]?.Value<string>();
                if (!string.IsNullOrWhiteSpace(url))
                    return url;
            }

            return assets
                .Select(asset => new
                    { Name = asset?["name"]?.Value<string>(), Url = asset?["browser_download_url"]?.Value<string>() })
                .FirstOrDefault(a =>
                    a.Name != null && a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(a.Url))
                ?.Url;
        }
        catch
        {
            return null;
        }
    }

    // 解析 GitHub 仓库地址: "https://github.com/user/repo" 或 "user/repo" → (owner, name)
    private static (string owner, string name)? ParseRepo(string repository)
    {
        var repo = repository.Trim().TrimEnd('/');

        // 处理 "https://github.com/user/repo" 格式
        if (repo.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase))
        {
            var path = repo["https://github.com/".Length..];
            return ParseOwnerRepo(path);
        }

        if (!repo.StartsWith("http://github.com/", StringComparison.OrdinalIgnoreCase)) return ParseOwnerRepo(repo);
        {
            var path = repo["http://github.com/".Length..];
            return ParseOwnerRepo(path);
        }

        // 处理 "user/repo" 格式
    }

    private static (string owner, string name)? ParseOwnerRepo(string path)
    {
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            return null;
        return (parts[0], parts[1]);
    }
}