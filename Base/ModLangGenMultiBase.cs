using System.Collections.Generic;
using System.Linq;
using Bark.BetterCCL;
using BepInEx.Logging;

namespace Bark.Base;

public abstract class ModLangGenMultiBase
{
    private bool _isInitialized;
    private ManualLogSource _log = null!;
    protected abstract string NameSpace { get; }
    protected abstract IEnumerable<string> LanguageCodes { get; }

    public int Count { get; private set; }

    public void Initialize(ManualLogSource logger)
    {
        if (_isInitialized) return;
        _log = logger;
        BuildLocaleData();
        _isInitialized = true;
        _log.LogInfo($"[Multi] Registered {Count} defaults");
    }

    protected abstract void BuildLocaleData();

    protected void Add(string category, string key, params string?[] values)
    {
        if (string.IsNullOrEmpty(key)) return;
        var langCodes = LanguageCodes.ToList();
        for (var i = 0; i < values.Length && i < langCodes.Count; i++)
        {
            var v = values[i];
            if (v == null) continue;
            BetterLocale.SetDefault(langCodes[i], NameSpace, category, key, v);
            Count++;
        }
    }

    protected void Other(string key, params string[] values)
    {
        Add("other", key, values);
    }

    protected void Log(string key, params string[] values)
    {
        Add("log", key, values);
    }

    protected void Command(string key, params string[] values)
    {
        Add("command", key, values);
    }

    protected void Item(string key, params string[] values)
    {
        AddPaired("item", key, values);
    }

    protected void Building(string key, params string[] values)
    {
        AddPaired("build", key, values);
    }

    protected void Moodle(string key, params string[] values)
    {
        AddPaired("moodle", key, values);
    }

    protected void Option(string key, params string[] values)
    {
        AddPaired("option", key, values);
    }

    protected void Liquid(string key, params string[] values)
    {
        AddPaired("liquid", key, values);
    }

    protected void Title(string key, params string[] values)
    {
        AddPaired("title", key, values);
    }

    private void AddPaired(string category, string key, params string?[] values)
    {
        var langCodes = LanguageCodes.ToList();
        for (var langIdx = 0; langIdx < langCodes.Count; langIdx++)
        {
            var labelIdx = langIdx * 2;
            var descIdx = langIdx * 2 + 1;

            if (labelIdx < values.Length)
            {
                var label = values[labelIdx];
                if (label != null)
                {
                    BetterLocale.SetDefault(langCodes[langIdx], NameSpace, category, key, label);
                    Count++;
                }
            }

            if (descIdx >= values.Length) continue;
            var desc = values[descIdx];
            if (desc == null) continue;
            BetterLocale.SetDefault(langCodes[langIdx], NameSpace, category, key + "dsc", desc);
            Count++;
        }
    }
}