using System;
using System.IO;

namespace Bark.ScriptApi;

public class ScriptApi(string id, string version, string name)
{
    private static readonly string LogsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptMod", "Logs");

    public EventApi events { get; } = new(id);
    public InventorApi Inventor { get; } = new();
    public ItemApi Item { get; } = new();
    public LimbApi Limb { get; } = new();
    public LocaleApi Locale { get; } = new(id, name);
    public LogApi Log { get; } = new(name, LogsDir);
    public PlayerApi Player { get; } = new();
    public ScriptInfo ScriptInfo { get; } = new() { Id = id, Version = version, Name = name };
    public SkillApi Skill { get; } = new();
    public WorldApi World { get; } = new();
}