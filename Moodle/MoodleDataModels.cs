using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bark.Moodle;

// 自定义 Moodle JSON 数据模型。
// 放在 ModName/Moodle/*.json，每个文件一个 Moodle。
// JSON 字段一律使用 snake_case。

public class MoodleDef
{
    // 强度（影响游戏内 Moodle 图标的显示大小/优先级）
    [JsonProperty("intensity")]
    public int Intensity = 1;

    // 名称（需本地化，对应 locale 中 moodle.{key}.name）
    [JsonProperty("name")]
    public string Name = string.Empty;

    // 描述（需本地化，对应 locale 中 moodle.{key}.description）
    [JsonProperty("description")]
    public string Description = string.Empty;

    // 是否为严重状态（影响 UI 显示强度）
    [JsonProperty("critical")]
    public bool Critical;

    // 治疗时是否可被清除。默认 false，即 heal 时自动移除该 Moodle；设为 true 则 heal 后依然保留
    [JsonProperty("can_heal")]
    public bool CanHeal;

    // 仅消耗品显示
    [JsonProperty("chipped_only")]
    public bool ChippedOnly;

    // 重要（true 显示在主区域，false 显示在侧边栏）
    [JsonProperty("important")]
    public bool Important = true;

    // Moodle 唯一 key。不填时自动用 mod_id.moodle_name（snake_case 化）
    [JsonProperty("key")]
    public string? Key;

    // 持续时间（秒），到期后自动消失。默认 0.75 秒。
    [JsonProperty("hold_seconds")]
    public float HoldSeconds = 0.75f;

    // 脚本触发定义：get / iterate / lose 三个阶段各自可指定脚本文件列表
    [JsonProperty("script")]
    public MoodleScriptDef? Script;

    // ---- 图标来源（三选一） ----

    // 方式 1：游戏内置图标 ID（如 "bleeding"、"hunger"，对应 MoodleManager.icons 中的 key）
    [JsonProperty("icon_id")]
    public string? IconId;

    // 方式 2：自定义精灵图路径（相对于模组目录，如 "Assets/Moodle/bleeding.png"）
    // 与 icon_id 互斥，优先使用 icon_id
    [JsonProperty("icon_asset")]
    public string? IconAsset;

    // 数值越大精灵越大，1=16 PPU 基准
    [JsonProperty("sprite_scale")]
    public float SpriteScale = 0.5f;

    // 方式 3：动画 Moodle（使用已注册的 SpriteAnimation）
    // 启用后 icon_id / icon_asset 均被忽略
    [JsonProperty("animated")]
    public bool Animated;

    // 动画 ID（仅 animated=true 时有效）
    [JsonProperty("animation_id")]
    public string? AnimationId;
}

// Moodle 脚本触发定义：动作名 → 脚本文件列表（路径相对于模组目录）。
// 支持的动作键：get / iterate / lose
public class MoodleScriptDef
{
    [JsonProperty("get")]
    public List<string> Get { get; set; } = [];

    [JsonProperty("iterate")]
    public List<string> Iterate { get; set; } = [];

    [JsonProperty("lose")]
    public List<string> Lose { get; set; } = [];
}
