using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Liquid;

// 纯液体定义 JSON 数据模型。
// 此模型仅包含 Bark 独有的基础字段。
public class LiquidDef
{
    // 液体 ID 由 JSON 文件名决定，不在 JSON 内指定 id 字段。
    [JsonProperty("description")]
    public string Description = string.Empty;

    [JsonProperty("color")]
    public string Color = "#FFFFFF";

    [JsonProperty("value_per_liter")]
    public float ValuePerLiter;

    [JsonProperty("health_usable")]
    public bool HealthUsable;

    [JsonProperty("injectable")]
    public bool Injectable;

    [JsonProperty("injection_sickness_multiplier")]
    public float InjectionSicknessMultiplier = 1f;

    [JsonProperty("locale_from_item")]
    public bool LocaleFromItem;

    [JsonProperty("qualities")]
    public Dictionary<string, float>? Qualities;

    [JsonProperty("vomiting_threshold")]
    public float VomitingThreshold;
}
