using System.Reflection;
using Bark.BetterCCL;
using BepInEx.Logging;

namespace Bark.Base;

// 语言生成器基类 — 注册默认本地化文本到 LocaleFallback
// BetterLocale 查不到翻译时 LocaleFallback 返回默认值并标记，Flush() 时写入
public abstract class ModLangGenBase
{
    private bool _isInitialized;
    private ManualLogSource _log = null!;
    protected abstract string LanguageCode { get; }

    public int Count { get; private set; }

    internal void Initialize(ManualLogSource logger, Assembly pluginAssembly)
    {
        if (_isInitialized) return;
        _log = logger;
        BuildLocaleData();
        _isInitialized = true;
        _log.LogInfo($"[{LanguageCode}] Registered {Count} defaults");
    }

    protected abstract void BuildLocaleData();

    protected void Other(string key, string value)
    {
        if (string.IsNullOrEmpty(key)) return;
        BetterLocale.SetDefault(LanguageCode, key, value);
        Count++;
    }
}