using System;
using System.IO;

namespace Bark.ScriptApi;

public class ScriptApi(string id, string version, string name)
{
    // 游戏根目录下的 ScriptMod/Logs/
    private static readonly string LogsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptMod", "Logs");

    public InventorApi Inventor { get; } = new();
    public LogApi Log { get; } = new(name, LogsDir);
    public ScriptInfo ScriptInfo { get; } = new() { Id = id, Version = version, Name = name };
    public WorldApi World { get; } = new();
}