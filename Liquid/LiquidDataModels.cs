using System.Collections.Generic;
using Bark.Items;
using Newtonsoft.Json;

namespace Bark.Liquid;

// 纯液体定义 JSON 数据模型。
// 注：bodyNature / OD / duration 等液体使用效果由 ICEnecoCustomItemAPI 的 CustomLiquidData 处理，
// 此模型仅包含 Bark 独有的基础字段。
public class LiquidDef
{
    // 液体 ID 由 JSON 文件名决定，不在 JSON 内指定 id 字段。
    [JsonProperty("description")]
    public string description = string.Empty;

    [JsonProperty("color")]
    public string color = "#FFFFFF";

    [JsonProperty("value_per_liter")]
    public float valuePerLiter;

    [JsonProperty("health_usable")]
    public bool healthUsable;

    [JsonProperty("injectable")]
    public bool injectable;

    [JsonProperty("injection_sickness_multiplier")]
    public float injectionSicknessMultiplier = 1f;

    [JsonProperty("locale_from_item")]
    public bool localeFromItem;

    [JsonProperty("qualities")]
    public Dictionary<string, float>? qualities;

    [JsonProperty("vomiting_threshold")]
    public float vomitingThreshold;
}

// 液体容器物品 JSON 数据模型，继承 ItemDef 并增加液体相关字段。
public class LiquidItemDef : ItemDef
{
    [JsonProperty("capacity")]
    public float capacity;

    [JsonProperty("auto_fill")]
    public bool autoFill = true;

    // liquidId -> amount (ml)
    [JsonProperty("default_liquid")]
    public Dictionary<string, float>? defaultLiquid;
}
