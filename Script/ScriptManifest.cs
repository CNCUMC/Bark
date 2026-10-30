using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Script;

public class ScriptManifest
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("version")] public string Version { get; set; } = string.Empty;

    [JsonProperty("author")]
    public Dictionary<string, string> Author { get; set; } = new();

    [JsonProperty("description")] public string Description { get; set; } = string.Empty;
    [JsonProperty("bark_version")] public string BarkVersion { get; set; } = string.Empty;
    [JsonProperty("game_version")] public string GameVersion { get; set; } = string.Empty;

    [JsonProperty("dependencies")]
    public List<ModDependency> Dependencies { get; set; } = [];

    [JsonProperty("repository")] public string? Repository { get; set; }
    [JsonProperty("network_sync")] public string? NetworkSync { get; set; }

    [JsonIgnore] public string Directory { get; set; } = string.Empty;
    [JsonIgnore] public string EntryFile { get; set; } = string.Empty;
    [JsonIgnore] public ScriptLanguage Language { get; set; }
    [JsonIgnore] public ScriptEngine? Engine { get; set; }
}

public class ModDependency
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;
    [JsonProperty("version")] public string Version { get; set; } = string.Empty;
}

public enum ScriptLanguage
{
    None,
    JavaScript,
    Lua
}
