using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Tile;

public class TileDef
{
    [JsonProperty("collider_type")] public string? ColliderType;
    [JsonProperty("color")] public string? Color;
    [JsonProperty("custom_data")] public Dictionary<string, object>? CustomData;
    [JsonProperty("description")] public string? Description;
    [JsonProperty("drops")] public TileDropDef[]? Drops;
    [JsonProperty("generation_style")] public string[]? GenerationStyle;
    [JsonProperty("health")] public float Health = 100f;
    [JsonProperty("hit_sound")] public string HitSound = "stone";
    [JsonProperty("hit_sound_clip")] public string? HitSoundClip;
    [JsonProperty("metallic")] public bool Metallic;
    [JsonProperty("name")] public string Name = string.Empty;
    [JsonProperty("no_variation")] public bool NoVariation;
    [JsonProperty("script")] public TileScriptDef? Script;
    [JsonProperty("sleep_quality")] public string? SleepQuality;
    [JsonProperty("slippery")] public bool Slippery;
    [JsonProperty("spawn_amount")] public float SpawnAmount;
    [JsonProperty("spawn_layers")] public int[]? SpawnLayers;
    [JsonProperty("sprite_import_scale")] public float SpriteImportScale = 2f;
    [JsonProperty("step_sound")] public string StepSound = "Gravel";
    [JsonProperty("toxicity")] public float Toxicity;
}

public class TileScriptDef
{
    [JsonProperty("on_damaging")] public List<string> OnDamaging = [];
    [JsonProperty("on_destroyed")] public List<string> OnDestroyed = [];
    [JsonProperty("on_exist")] public List<string> OnExist = [];
    [JsonProperty("on_place")] public List<string> OnPlace = [];
}

public class TileDropDef
{
    [JsonProperty("chance")] public float Chance = 1f;
    [JsonProperty("condition_max")] public float ConditionMax = 1f;
    [JsonProperty("condition_min")] public float ConditionMin;
    [JsonProperty("id")] public string Id = string.Empty;
}
