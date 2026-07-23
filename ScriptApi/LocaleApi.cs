using Bark.Script;

namespace Bark.ScriptApi;

public class LocaleApi(string id, string name)
{
    private readonly string _modName = name;

    public string Get(string key)
    {
        return ScriptLocaleManager.Get(ExpandKey(key));
    }

    public string GetFormatted(string key, params object[] args)
    {
        return ScriptLocaleManager.GetFormatted(ExpandKey(key), args);
    }

    public string GetFrom(string modId, string key)
    {
        return ScriptLocaleManager.Get($"{key}.{modId}");
    }

    public string GetFormattedFrom(string modId, string key, params object[] args)
    {
        return ScriptLocaleManager.GetFormatted($"{key}.{modId}", args);
    }

    public bool HasKey(string key)
    {
        return ScriptLocaleManager.HasKey(ExpandKey(key));
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