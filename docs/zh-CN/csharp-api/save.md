[English](../../en-US/csharp-api/save.md) | ***简体中文***

# SaveLoader

SaveLoader 封装了 CUCoreLib 的 `SaveRegistry.RegisterGlobalProvider`，提供日志追踪、参数校验和简化的 Provider 基类。

> ℹ️ 这是 C# 专用 API。保存系统基于 `ICustomSaveProvider` 接口，需要 C# 实现，脚本模组不直接使用。

## 注册键命名空间

对标 `BetterLocale` 的 key 管理方式，保存系统要求 `nameSpace` 和 `key` 分离，最终注册键 = `nameSpace.key`：

```csharp
// ✅ 推荐：显式指定命名空间
"bark.economy"        // nameSpace="bark", key="economy"
"mymod.quests"        // nameSpace="mymod", key="quests"
"mymod.player_stats"  // nameSpace="mymod", key="player_stats"

// ❌ 避免：不指定 nameSpace，容易冲突
"economy"
"data"
```

> ⚠️ `nameSpace` 和 `key` 均不能为空或空白，否则抛出 `ArgumentException`。

## 两种使用方式

### 方式一：直接实现 ICustomSaveProvider（灵活）

如果你的保存逻辑需要完全控制 JToken 序列化细节，直接实现接口并用 `SaveLoader.RegisterGlobalProvider` 注册：

```csharp
using Bark.Save;
using CUCoreLib.Saving;
using Newtonsoft.Json.Linq;

public sealed class MyModSaveProvider : ICustomSaveProvider
{
    public int GetVersion() => 1;

    public JToken Capture()
    {
        // 保存时返回你的数据
        var obj = new JObject
        {
            ["gold"] = MyGoldStorage.Amount,
            ["quest_progress"] = MyQuestTracker.Progress
        };
        return obj;
    }

    public void Restore(JToken payload, int version, SaveRestoreContext context)
    {
        // 加载时恢复数据
        var gold = payload["gold"]?.Value<int>() ?? 0;
        var progress = payload["quest_progress"]?.Value<int>() ?? 0;
        MyGoldStorage.Amount = gold;
        MyQuestTracker.Progress = progress;
    }
}

// 在 Plugin.Awake() 中注册
SaveLoader.RegisterGlobalProvider("mymod", "economy", new MyModSaveProvider());
```

### 方式二：继承 BaseSaveProvider\<T\>（推荐）

使用泛型基类，自动处理 JToken 序列化/反序列化，你只需关心自己的数据模型：

```csharp
using Bark.Save;

// 1. 定义存档数据模型
public class EconomySaveData
{
    public int Gold { get; set; }
    public List<string> UnlockedRecipes { get; set; } = new();
}

// 2. 继承 BaseSaveProvider<T>
public sealed class EconomySaveProvider : BaseSaveProvider<EconomySaveData>
{
    public EconomySaveProvider() : base("mymod", "economy") { }

    public override int GetVersion() => 1;

    protected override EconomySaveData CaptureData()
    {
        return new EconomySaveData
        {
            Gold = MyGoldStorage.Amount,
            UnlockedRecipes = MyRecipeManager.GetUnlocked()
        };
    }

    protected override void RestoreData(EconomySaveData data, SaveRestoreContext context)
    {
        MyGoldStorage.Amount = data.Gold;
        MyRecipeManager.SetUnlocked(data.UnlockedRecipes);
    }
}

// 在 Plugin.Awake() 中注册
var provider = new EconomySaveProvider();
provider.Register(); // 等价于 SaveLoader.RegisterGlobalProvider("mymod", "economy", provider)
```

## 版本迁移

数据格式变更时递增 `GetVersion()` 返回值，并在 `RestoreData` 中根据 `context.Version`（实际存档版本号）做兼容处理：

```csharp
public override int GetVersion() => 2; // 从 1 升级到 2

protected override void RestoreData(EconomySaveData data, SaveRestoreContext context)
{
    if (context.Version < 2)
    {
        // v1 没有 UnlockedRecipes，给默认值
        data.UnlockedRecipes ??= new List<string>();
    }

    MyGoldStorage.Amount = data.Gold;
    MyRecipeManager.SetUnlocked(data.UnlockedRecipes);
}
```

## API 参考

### SaveLoader

| 方法                                                                                  | 说明                                                                                 |
|---------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------|
| `RegisterGlobalProvider(string nameSpace, string key, ICustomSaveProvider? provider)` | 注册保存 Provider，nameSpace 和 key 均非空、provider 非 null，最终键 = nameSpace.key |
| `Unregister(string fullKey)`                                                          | 从本地追踪列表中移除（注意 SaveRegistry 可能不支持运行时取消注册）                   |
| `Clear()`                                                                             | 清空所有追踪记录（热重载时自动调用）                                                 |

### BaseSaveProvider\<T\>

| 成员                                                            | 说明                                 |
|-----------------------------------------------------------------|--------------------------------------|
| `BaseSaveProvider(string nameSpace, string key)`                | 构造函数，最终注册键 = nameSpace.key |
| `Register()`                                                    | 注册当前 Provider 到 SaveRegistry    |
| `abstract int GetVersion()`                                     | 存档数据版本号                       |
| `abstract T CaptureData()`                                      | 保存时调用，返回数据对象             |
| `abstract void RestoreData(T data, SaveRestoreContext context)` | 加载时调用，恢复数据                 |

## 注意事项

- 保存/加载由游戏引擎在适当时机自动触发，你无需手动调用 `Capture()` / `Restore()`
- `CaptureData()` 中捕获的数据必须是 JSON 可序列化的（基本类型、集合、简单 POCO）
- 不要在 `CaptureData()` / `RestoreData()` 中执行耗时操作
- `BaseSaveProvider<T>` 的 `Capture()` / `Restore()` 已内置 try-catch 和错误日志
- `SaveRestoreContext` 包含 `Version` 字段，表示实际存档中的版本号
