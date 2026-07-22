using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Bark.Script;

// 脚本模组清单文件（mod.json）的数据模型
public class ScriptManifest
{
    // 唯一标识符，推荐反向域名格式
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    // 显示名称
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    // 语义化版本号
    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;

    // 贡献者字典，key 为角色（程序、美术、翻译等），value 为名字
    [JsonProperty("author")]
    public Dictionary<string, string> Author { get; set; } = new();

    // 描述
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    // Bark 版本要求，用于兼容性检查（semver range）
    [JsonProperty("bark_version")]
    public string BarkVersion { get; set; } = string.Empty;

    // 兼容的游戏版本（semver range）
    [JsonProperty("game_version")]
    public string GameVersion { get; set; } = string.Empty;

    // 依赖的其他模组列表
    [JsonProperty("dependencies")]
    public List<ModDependency> Dependencies { get; set; } = [];

    // 模组所在目录路径（运行时填充）
    [JsonIgnore]
    public string Directory { get; set; } = string.Empty;

    // 入口文件路径（运行时填充，根据扩展名确定语言）
    [JsonIgnore]
    public string EntryFile { get; set; } = string.Empty;

    // 脚本语言类型（运行时根据入口文件扩展名确定）
    [JsonIgnore]
    public ScriptLanguage Language { get; set; }

    // 引擎组件引用（运行时填充，用于生命周期管理）
    [JsonIgnore]
    public MonoBehaviour? Engine { get; set; }
}

// 模组依赖声明
public class ModDependency
{
    // 依赖的模组 ID
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    // 版本要求（semver range）
    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;
}

// 脚本语言类型
public enum ScriptLanguage
{
    JavaScript,
    Lua,
    Python
}
