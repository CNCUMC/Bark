using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Tile;

// 自定义物块 JSON 数据模型。字段与 CUCoreLib CustomTileDefinition 一一对应，
// JSON 字段一律使用 snake_case。物块索引在 mod.json 的 tiles 映射中声明，
// 不属于 CustomTileDefinition。
public class TileDef
{
    // 碰撞器类型: "Grid" / "Sprite" / "None"，默认 "Grid"
    [JsonProperty("collider_type")] public string? ColliderType;

    // 精灵图着色，支持 RGBA hex 如 "#FF0000" 或 "#FF0000FF"，默认白色
    [JsonProperty("color")] public string? Color;

    // ---- 扩展 ----

    // 自定义元数据，可通过 TileRegistry.TryGetCustomData 读取
    [JsonProperty("custom_data")] public Dictionary<string, object>? CustomData;

    // 物块描述文本（注册为 other.{id}_description 的本地化条目）
    [JsonProperty("description")] public string? Description;

    // ---- 掉落 ----

    // 破坏掉落物品定义
    [JsonProperty("drops")] public TileDropDef[]? Drops;

    // 自动生成的形状样式，支持组合: ["Vein", "Outskirt"] 等
    // 可用值: Vein / HeavyVeins / Singular / Stripe / Inner / Outskirt
    [JsonProperty("generation_style")] public string[]? GenerationStyle;

    // 生命值（破坏该方块所需的伤害量）
    [JsonProperty("health")] public float Health = 100f;

    // 打击音效 ID（如 "stone"、"metal"、"rock"）
    [JsonProperty("hit_sound")] public string HitSound = "stone";

    // 打击音效 AudioClip（优先级高于 hit_sound）
    [JsonProperty("hit_sound_clip")] public string? HitSoundClip;

    // 启用金属伤害行为
    [JsonProperty("metallic")] public bool Metallic;

    // 物块显示名称（注册为 other.ID 的本地化条目）
    [JsonProperty("name")] public string Name = string.Empty;

    // 禁用原版视觉随机变化（翻转等）
    [JsonProperty("no_variation")] public bool NoVariation;

    // 物块脚本（可选），定义各触发动作对应的脚本文件
    [JsonProperty("script")] public TileScriptDef? Script;

    // 睡眠质量: "Excellent" / "Good" / "Mediocre" / "Bad" / "Awful"
    [JsonProperty("sleep_quality")] public string? SleepQuality;

    // 启用滑动行为
    [JsonProperty("slippery")] public bool Slippery;

    // ---- 自动生成 ----

    // 生成数量乘数。0f 禁用自动生成，1f 等同铜矿，2f 翻倍
    [JsonProperty("spawn_amount")] public float SpawnAmount;

    // 允许生成的游戏层（1-based），如 [2, 4, 5]。默认全部层
    [JsonProperty("spawn_layers")] public int[]? SpawnLayers;

    // 精灵图导入放大倍数，默认 1.0
    [JsonProperty("sprite_import_scale")] public float SpriteImportScale = 2f;

    // 行走音效 ID（如 "Gravel"、"Rock"）
    [JsonProperty("step_sound")] public string StepSound = "Gravel";

    // 毒性（辐射）值
    [JsonProperty("toxicity")] public float Toxicity;
}

// 物块脚本触发定义：动作名 → 脚本文件列表（路径相对于模组目录）。
// 支持的动作键：on_place / on_exist / on_damaging / on_destroyed
public class TileScriptDef
{
    [JsonProperty("on_damaging")] public List<string> OnDamaging = [];

    [JsonProperty("on_destroyed")] public List<string> OnDestroyed = [];

    [JsonProperty("on_exist")] public List<string> OnExist = [];

    [JsonProperty("on_place")] public List<string> OnPlace = [];
}

// 物块掉落物品定义
public class TileDropDef
{
    // 掉落几率 0f~1f
    [JsonProperty("chance")] public float Chance = 1f;

    // 掉落物品最高耐久
    [JsonProperty("condition_max")] public float ConditionMax = 1f;

    // 掉落物品最低耐久
    [JsonProperty("condition_min")] public float ConditionMin;

    // 掉落物品 ID
    [JsonProperty("id")] public string Id = string.Empty;
}