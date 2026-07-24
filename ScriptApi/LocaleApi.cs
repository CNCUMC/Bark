namespace Bark.ScriptApi;

// 脚本侧本地化 API：通过 LocaleBridge 委托访问 ScriptLocaleManager
public class LocaleApi(string id, string name)
{
    private readonly string _modName = name;

    public string Get(string key)
    {
        var getter = LocaleBridge.Getter;
        return getter is not null ? getter(ExpandKey(key)) : $"[{key}]";
    }

    public string GetFormatted(string key, params object[] args)
    {
        var getter = LocaleBridge.FormattedGetter;
        return getter is not null ? getter(ExpandKey(key), args) : $"[{key}]";
    }

    public string GetFrom(string modId, string key)
    {
        var getter = LocaleBridge.Getter;
        return getter is not null ? getter($"{key}.{modId}") : $"[{key}.{modId}]";
    }

    public string GetFormattedFrom(string modId, string key, params object[] args)
    {
        var getter = LocaleBridge.FormattedGetter;
        return getter is not null ? getter($"{key}.{modId}", args) : $"[{key}.{modId}]";
    }

    public bool HasKey(string key)
    {
        var checker = LocaleBridge.KeyChecker;
        return checker is not null && checker(ExpandKey(key));
    }

    private string ExpandKey(string key)
    {
        if (key.Contains($".{id}."))
            return key;

        var dotIndex = key.IndexOf('.');
        if (dotIndex <= 0) return $"log.{id}.{key}";
        var prefix = key[..dotIndex];
        var keyName = key[(dotIndex + 1)..];
        return $"{prefix}.{id}.{keyName}";
    }
}
