***English*** | [简体中文](../../zh-CN/csharp-api/update.md)

# UpdateUtil

UpdateUtil checks GitHub Releases for a newer version and outputs a notice in the BepInEx console.

> ℹ️ This is a C#-only API.

## Check for Updates

```csharp
using Bark.Tool;

// Call at the end of Plugin.Awake()
UpdateUtil.Check(
    "CNCUMC/Bark",      // GitHub repo, format "user/repo"
    "Bark",              // Mod display name (used in logs)
    "2.0.0",             // Current version
    Logger               // BepInEx ManualLogSource
);
```

## Behavior

- Sends GET to `https://api.github.com/repos/{repo}/releases/latest`
- Compares `tag_name` against current version (semver comparison, strips leading `v`)
- Only alerts when latest > current
- Logs a warning on network/parse errors, does not affect gameplay

## Sample Output

```
[Info   :Bark] Bark is up to date (2.0.0)
[Warning:Bark] Bark update available! 1.1.1 -> 2.0.0 
[Warning:Bark] Bark unable to check for updates
```
