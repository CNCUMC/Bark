using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Moodle;

public class MoodleDef
{
    [JsonProperty("animated")] public bool Animated;
    [JsonProperty("animation_id")] public string? AnimationId;
    [JsonProperty("can_heal")] public bool CanHeal;
    [JsonProperty("chipped_only")] public bool ChippedOnly;
    [JsonProperty("critical")] public bool Critical;
    [JsonProperty("description")] public string Description = string.Empty;
    [JsonProperty("hold_seconds")] public float HoldSeconds = 0.75f;
    [JsonProperty("icon_asset")] public string? IconAsset;
    [JsonProperty("icon_id")] public string? IconId;
    [JsonProperty("important")] public bool Important = true;
    [JsonProperty("intensity")] public int Intensity = 1;
    [JsonProperty("key")] public string? Key;
    [JsonProperty("name")] public string Name = string.Empty;
    [JsonProperty("script")] public MoodleScriptDef? Script;
    [JsonProperty("sprite_scale")] public float SpriteScale = 0.5f;
}

public class MoodleScriptDef
{
    [JsonProperty("get")] public List<string> Get { get; set; } = [];
    [JsonProperty("iterate")] public List<string> Iterate { get; set; } = [];
    [JsonProperty("lose")] public List<string> Lose { get; set; } = [];
}
