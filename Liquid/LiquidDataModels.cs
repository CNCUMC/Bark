using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Liquid;

public class LiquidDef
{
    [JsonProperty("color")] public string Color = "#1E1E1E";
    [JsonProperty("description")] public string Description = string.Empty;
    [JsonProperty("health_usable")] public bool HealthUsable;
    [JsonProperty("injectable")] public bool Injectable;
    [JsonProperty("injection_sickness_multiplier")]
    public float InjectionSicknessMultiplier = 1f;
    [JsonProperty("locale_from_item")] public bool LocaleFromItem;
    [JsonProperty("qualities")] public Dictionary<string, float>? Qualities;
    [JsonProperty("value_per_liter")] public float ValuePerLiter;
    [JsonProperty("vomiting_threshold")] public float VomitingThreshold;
}