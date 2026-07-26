using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Tile;

// 自定义物块 JSON 数据模型。字段与 CUCoreLib CustomTileDefinition 一一对应，
// JSON 字段一律使用 snake_case。
public class TileDef
{
    // 物块索引（必须 >= 36，且与其他模组不冲突）
    [JsonProperty("tile_index")]
    public int TileIndex;

    // 稳定的本地化键值，同时作为默认 Unity 瓦片名
    [JsonProperty("id")]
    public string Id = string.Empty;

    // 物块显示名称（注册为 other.ID 的本地化条目）
    [JsonProperty("name")]
    public string Name = string.Empty;

    // 可选的 Unity 对象名，不填则沿用 id
    [JsonProperty("tile_name")]
    public string? TileName;

    // 精灵图着色，支持 RGBA hex 如 "#FF0000" 或 "#FF0000FF"，默认白色
    [JsonProperty("color")]
    public string? Color;

    // 碰撞器类型: "Grid" / "Sprite" / "None"，默认 "Grid"
    [JsonProperty("collider_type")]
    public string? ColliderType;

    // 生命值（破坏该方块所需的伤害量）
    [JsonProperty("health")]
    public float Health = 100f;

    // 打击音效 ID（如 "stone"、"metal"、"rock"）
    [JsonProperty("hit_sound")]
    public string HitSound = "stone";

    // 打击音效 AudioClip（优先级高于 hit_sound）
    [JsonProperty("hit_sound_clip")]
    public string? HitSoundClip;

    // 行走音效 ID（如 "Gravel"、"Rock"）
    [JsonProperty("step_sound")]
    public string StepSound = "Gravel";

    // 睡眠质量: "Excellent" / "Good" / "Mediocre" / "Bad" / "Awful"
    [JsonProperty("sleep_quality")]
    public string? SleepQuality;

    // 禁用原版视觉随机变化（翻转等）
    [JsonProperty("no_variation")]
    public bool NoVariation;

    // 启用金属伤害行为
    [JsonProperty("metallic")]
    public bool Metallic;

    // 毒性（辐射）值
    [JsonProperty("toxicity")]
    public float Toxicity;

    // 启用滑动行为
    [JsonProperty("slippery")]
    public bool Slippery;

    // ---- 自动生成 ----

    // 生成数量乘数。0f 禁用自动生成，1f 等同铜矿，2f 翻倍
    [JsonProperty("spawn_amount")]
    public float SpawnAmount;

    // 允许生成的游戏层（1-based），如 [2, 4, 5]。默认全部层
    [JsonProperty("spawn_layers")]
    public int[]? SpawnLayers;

    // 自动生成的形状样式，支持组合: ["Vein", "Outskirt"] 等
    // 可用值: Vein / HeavyVeins / Singular / Stripe / Inner / Outskirt
    [JsonProperty("generation_style")]
    public string[]? GenerationStyle;

    // ---- 掉落 ----

    // 破坏掉落物品定义
    [JsonProperty("drops")]
    public TileDropDef[]? Drops;

    // ---- 扩展 ----

    // 自定义元数据，可通过 TileRegistry.TryGetCustomData 读取
    [JsonProperty("custom_data")]
    public Dictionary<string, object>? CustomData;

    // 精灵图导入放大倍数，默认 8.0
    [JsonProperty("sprite_import_scale")]
    public float SpriteImportScale = 8f;
}

// 物块掉落物品定义
public class TileDropDef
{
    // 掉落物品 ID
    [JsonProperty("id")]
    public string Id = string.Empty;

    // 掉落几率 0f~1f
    [JsonProperty("chance")]
    public float Chance = 1f;

    // 掉落物品最低耐久
    [JsonProperty("condition_min")]
    public float ConditionMin;

    // 掉落物品最高耐久
    [JsonProperty("condition_max")]
    public float ConditionMax = 1f;
}
