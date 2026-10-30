using System;
using System.Collections.Generic;
using Bark.BetterCCL;
using Bark.Tool;
using CUCoreLib.Registries;
using CUCoreLib.Saving;
using Newtonsoft.Json.Linq;

namespace Bark.Save;

public class SaveProviderEntry(string key, Type providerType)
{
    public string Key = key;
    public Type ProviderType = providerType;
}

public static class SaveLoader
{
    public static readonly Dictionary<string, SaveProviderEntry> RegisteredProviders =
        new();

    public static void RegisterGlobalProvider(string nameSpace, string key,
        ICustomSaveProvider? provider)
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

    public static void Unregister(string fullKey)
    {
        if (RegisteredProviders.Remove(fullKey))
            LogUtil.Info("save.provider_unregistered", fullKey);
    }

    public static void Clear()
    {
        RegisteredProviders.Clear();
    }
}

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

    public abstract int GetVersion();

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

    void ICustomSaveProvider.Restore(JToken payload, int version,
        SaveRestoreContext context)
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

    public void Register()
    {
        SaveRegistry.RegisterGlobalProvider(_fullKey, this);
        SaveLoader.RegisteredProviders[_fullKey] =
            new SaveProviderEntry(_fullKey, GetType());
        LogUtil.Info("save.provider_registered", _fullKey);
    }

    protected abstract T CaptureData();

    protected abstract void RestoreData(T data, SaveRestoreContext context);
}
