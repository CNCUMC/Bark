***English*** | [简体中文](../../zh-CN/csharp-api/save.md)

# SaveLoader

SaveLoader wraps CUCoreLib's `SaveRegistry.RegisterGlobalProvider` with logging, input validation, and a simplified
Provider base class.

> ℹ️ This is a C#-only API. The save system is based on the `ICustomSaveProvider` interface and requires C#
> implementation.

## Key Namespace

Following `BetterLocale`'s key management pattern, the save system separates `nameSpace` and `key`. The final
registration key is `nameSpace.key`:

```csharp
// ✅ Recommended: explicit namespace
"bark.economy"        // nameSpace="bark", key="economy"
"mymod.quests"        // nameSpace="mymod", key="quests"
"mymod.player_stats"  // nameSpace="mymod", key="player_stats"

// ❌ Avoid: no namespace, easy to conflict
"economy"
"data"
```

> ⚠️ Both `nameSpace` and `key` must not be empty or whitespace, otherwise throws `ArgumentException`.

## Two Approaches

### Approach 1: Implement ICustomSaveProvider Directly (Flexible)

If you need full control over JToken serialization, implement the interface and register with
`SaveLoader.RegisterGlobalProvider`:

```csharp
using Bark.Save;
using CUCoreLib.Saving;
using Newtonsoft.Json.Linq;

public sealed class MyModSaveProvider : ICustomSaveProvider
{
    public int GetVersion() => 1;

    public JToken Capture()
    {
        var obj = new JObject
        {
            ["gold"] = MyGoldStorage.Amount,
            ["quest_progress"] = MyQuestTracker.Progress
        };
        return obj;
    }

    public void Restore(JToken payload, int version, SaveRestoreContext context)
    {
        var gold = payload["gold"]?.Value<int>() ?? 0;
        var progress = payload["quest_progress"]?.Value<int>() ?? 0;
        MyGoldStorage.Amount = gold;
        MyQuestTracker.Progress = progress;
    }
}

// Register in Plugin.Awake()
SaveLoader.RegisterGlobalProvider("mymod", "economy", new MyModSaveProvider());
```

### Approach 2: Inherit BaseSaveProvider\<T\> (Recommended)

Use the generic base class for automatic JToken serialization/deserialization:

```csharp
using Bark.Save;

// 1. Define your save data model
public class EconomySaveData
{
    public int Gold { get; set; }
    public List<string> UnlockedRecipes { get; set; } = new();
}

// 2. Inherit BaseSaveProvider<T>
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

// Register in Plugin.Awake()
var provider = new EconomySaveProvider();
provider.Register(); // same as SaveLoader.RegisterGlobalProvider("mymod", "economy", provider)
```

## Version Migration

Increment `GetVersion()` when your data format changes, and handle compatibility in `RestoreData` using
`context.Version`:

```csharp
public override int GetVersion() => 2; // upgraded from 1 to 2

protected override void RestoreData(EconomySaveData data, SaveRestoreContext context)
{
    if (context.Version < 2)
    {
        // v1 didn't have UnlockedRecipes, provide defaults
        data.UnlockedRecipes ??= new List<string>();
    }

    MyGoldStorage.Amount = data.Gold;
    MyRecipeManager.SetUnlocked(data.UnlockedRecipes);
}
```

## API Reference

### SaveLoader

| Method                                                                                | Description                                                                                                 |
|---------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------|
| `RegisterGlobalProvider(string nameSpace, string key, ICustomSaveProvider? provider)` | Register a save provider. nameSpace and key must be non-empty, provider non-null. Final key = nameSpace.key |
| `Unregister(string fullKey)`                                                          | Remove from local tracking (note: SaveRegistry may not support runtime unregistration)                      |
| `Clear()`                                                                             | Clear all tracking records (called automatically on reload)                                                 |

### BaseSaveProvider\<T\>

| Member                                                          | Description                                         |
|-----------------------------------------------------------------|-----------------------------------------------------|
| `BaseSaveProvider(string nameSpace, string key)`                | Constructor, final registration key = nameSpace.key |
| `Register()`                                                    | Register this provider with SaveRegistry            |
| `abstract int GetVersion()`                                     | Save data version number                            |
| `abstract T CaptureData()`                                      | Called on save, return your data object             |
| `abstract void RestoreData(T data, SaveRestoreContext context)` | Called on load, restore your data                   |

## Notes

- Save/load is triggered automatically by the game engine. Do not call `Capture()` / `Restore()` manually
- Data captured in `CaptureData()` must be JSON-serializable (primitives, collections, simple POCOs)
- Avoid expensive operations inside `CaptureData()` / `RestoreData()`
- `BaseSaveProvider<T>`'s `Capture()` / `Restore()` have built-in try-catch with error logging
- `SaveRestoreContext` contains a `Version` field representing the actual version in the save file
