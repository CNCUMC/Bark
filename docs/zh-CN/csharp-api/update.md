# UpdateUtil

UpdateUtil 检查 GitHub Releases 是否有新版本，并在 BepInEx 控制台输出提示。

> ℹ️ 这是 C# 专用 API。

## 检查更新

```csharp
using Bark.Tool;

// 在 Plugin.Awake() 末尾调用
UpdateUtil.Check(
    "CNCUMC/Bark",      // GitHub 仓库，格式 "用户/仓库名"
    "Bark",             // 模组显示名称（用于日志）
    "2.0.0",            // 当前版本号
    Logger              // BepInEx ManualLogSource
);
```

## 行为

- 发 GET 请求到 `https://api.github.com/repos/{repo}/releases/latest`
- 对比 `tag_name` 和当前版本号（语义化版本比较，去掉前缀的 `v`）
- 只有 latest > current 时才提示更新
- 网络请求失败或解析失败时输出警告，不影响游戏运行

## 输出示例

```
[Info   :Bark] Bark 已是最新版本 (2.0.0)
[Warning:Bark] Bark 有新版本可用！1.1.1 -> 2.0.0 
[Warning:Bark] Bark 无法检查更新
```

