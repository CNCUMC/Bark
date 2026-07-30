using System;
using System.Collections.Generic;
using Bark.BetterCCL;
using Bark.Tool;
using CUCoreLib.Registries;
using CUCoreLib.Saving;
using Newtonsoft.Json.Linq;

namespace Bark.Save;

// 已注册保存 Provider 的记录项
public class SaveProviderEntry(string key, Type providerType)
{
    // 完整注册键（nameSpace.key）
    public string Key = key;

    // Provider 类型（用于调试/热重载追踪）
    public Type ProviderType = providerType;
}

// 保存系统封装：提供 SaveRegistry.RegisterGlobalProvider 的包装，
// 加上日志、验证以及简化的 Provider 基类。
// 对标 BetterLocale.SetDefault 的命名空间模式：nameSpace 和 key 分离，
// 最终注册键 = nameSpace.key（如 "bark.my_provider"、"mymod.economy"）。
public static class SaveLoader
{
    // 已注册的保存 Provider 列表
    public static readonly Dictionary<string, SaveProviderEntry> RegisteredProviders = new();

    // 注册全局自定义保存 Provider
    // nameSpace: 命名空间，通常为 Plugin.NameSpace 或模组 ID（如 "bark"、"mymod"）
    // key: Provider 名称（如 "economy"、"quests"），最终注册键 = nameSpace.key
    public static void RegisterGlobalProvider(string nameSpace, string key, ICustomSaveProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(nameSpace))
        {
            var msg = BetterLocale.GetLog("bark.save.namespace_empty");
            throw new ArgumentException(msg, nameof(nameSpace));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            var msg = BetterLocale.GetLog("bark.save.key_empty");
            throw new ArgumentException(msg, nameof(key));
        }

        if (provider is null)
        {
            var msg = BetterLocale.GetLog("bark.save.provider_null");
            throw new ArgumentNullException(nameof(provider), msg);
        }

        var fullKey = $"{nameSpace}.{key}";
        SaveRegistry.RegisterGlobalProvider(fullKey, provider);

        RegisteredProviders[fullKey] = new SaveProviderEntry(fullKey, provider.GetType());
        LogUtil.Info("save.provider_registered", fullKey);
    }

    // 取消注册（从本地追踪中移除，注意 SaveRegistry 可能不支持运行时取消）
    // fullKey: 完整注册键（nameSpace.key）
    public static void Unregister(string fullKey)
    {
        if (RegisteredProviders.Remove(fullKey))
            LogUtil.Info("save.provider_unregistered", fullKey);
    }

    // 清除所有追踪记录（通常在热重载前调用）
    public static void Clear()
    {
        RegisteredProviders.Clear();
    }
}

// 简化的保存 Provider 基类：自动处理 JToken 序列化/反序列化，
// 子类只需实现 GetVersion / CaptureData / RestoreData。
// T 为你的存档数据类型（必须是 JSON 可序列化的）。
// nameSpace 和 key 分离，最终注册键 = nameSpace.key。
//
// 使用示例：
//   public sealed class MySaveProvider : BaseSaveProvider<MySaveData>
//   {
//       public MySaveProvider() : base("mymod", "mydata") { }
//       public override int GetVersion() => 1;
//       protected override MySaveData CaptureData() => new() { ... };
//       protected override void RestoreData(MySaveData data, SaveRestoreContext context) { ... }
//   }
public abstract class BaseSaveProvider<T> : ICustomSaveProvider where T : class
{
    private readonly string _fullKey;

    // nameSpace: 命名空间，如 "bark"、"mymod"
    // key: Provider 名称，如 "economy"、"quests"，最终注册键 = nameSpace.key
    protected BaseSaveProvider(string nameSpace, string key)
    {
        if (string.IsNullOrWhiteSpace(nameSpace))
        {
            var msg = BetterLocale.GetLog("bark.save.namespace_empty");
            throw new ArgumentException(msg, nameof(nameSpace));
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            var msg = BetterLocale.GetLog("bark.save.key_empty");
            throw new ArgumentException(msg, nameof(key));
        }

        _fullKey = $"{nameSpace}.{key}";
    }

    // 负载结构的版本号。数据格式变更时递增。
    public abstract int GetVersion();

    // ---- ICustomSaveProvider 实现 ----

    JToken ICustomSaveProvider.Capture()
    {
        try
        {
            var data = CaptureData();
            return JToken.FromObject(data);
        }
        catch (Exception ex)
        {
            LogUtil.Error("save.capture_error", _fullKey, ex.Message);
            return new JObject();
        }
    }

    void ICustomSaveProvider.Restore(JToken payload, int version, SaveRestoreContext context)
    {
        try
        {
            var data = payload.ToObject<T>();
            if (data != null)
                RestoreData(data, context);
        }
        catch (Exception ex)
        {
            LogUtil.Error("save.restore_error", _fullKey, ex.Message);
        }
    }

    // 注册当前 Provider 到 SaveRegistry
    public void Register()
    {
        SaveRegistry.RegisterGlobalProvider(_fullKey, this);
        SaveLoader.RegisteredProviders[_fullKey] = new SaveProviderEntry(_fullKey, GetType());
        LogUtil.Info("save.provider_registered", _fullKey);
    }

    // 保存时调用：返回要保存的自定义数据对象
    // 基类自动将其序列化为 JToken
    protected abstract T CaptureData();

    // 加载时调用：将保存的数据恢复到运行时状态
    // data: 反序列化后的数据对象
    // context: 保存恢复上下文
    protected abstract void RestoreData(T data, SaveRestoreContext context);
}