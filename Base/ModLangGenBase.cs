using Bark.BetterCCL;
using BepInEx.Logging;

namespace Bark.Base;

public abstract class ModLangGenBase
{
    private bool _isInitialized;
    private ManualLogSource _log = null!;
    protected abstract string LanguageCode { get; }

    public int Count { get; private set; }

    public void Initialize(ManualLogSource logger)
    {
        if (_isInitialized) return;
        _log = logger;
        BuildLocaleData();
        _isInitialized = true;
        _log.LogInfo($"[{LanguageCode}] Registered {Count} defaults");
    }

    protected abstract void BuildLocaleData();

    protected void Add(string category, string key, string value)
    {
        if (string.IsNullOrEmpty(key)) return;
        BetterLocale.SetDefault(LanguageCode, category, key, value);
        Count++;
    }

    protected void Item(string key, string value) => Add("item", "item." + key, value);
    protected void Building(string key, string value) => Add("building", "building." + key, value);
    protected void Moodle(string key, string value) => Add("moodle", "moodle." + key, value);
    protected void Other(string key, string value) => Add("other", key, value);
    protected void Log(string key, string value) => Add("log", "log." + key, value);
    protected void Command(string key, string value) => Add("command", "command." + key, value);
    protected void Option(string key, string value, string description)
    {
        Add("option", $"gameset{key}", value);
        Add("option", $"gameset{key}dsc", description);
    }
}
