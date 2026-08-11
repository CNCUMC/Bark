using System.Collections.Generic;
using Bark.Audio;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Bark.Items.Templates;

// 玩偶属性数据容器，由 PlushTemplate 从 ItemDef.CustomData 填充，存入内部注册表。
public class PlushData
{
    // 自定义吱吱音效路径（相对模组目录，如 "Assets/Audio/plush_squeak.wav"）。
    // 纯文件名（不含 / 或 \）自动补全为 "Assets/Audio/filename"。
    // 通过 AudioManager 加载（Bark Audio 属性），支持 .wav/.mp3/.aif 等格式。
    // 空字符串表示使用游戏默认音效（PlushScript.selectedSound）。
    public string SqueakSound = "";

    // 所属模组的根目录（绝对路径），用于解析 Assets/Audio/ 下的音效文件。
    // 由 ItemLoader 在 CachePlushItem 时注入。
    public string ModDir = "";

    // 预加载的吱吱音效 AudioClip（若 SqueakSound 非空且加载成功）。
    [JsonIgnore] public AudioClip? SqueakClip;
}

// 玩偶物品模板：预设玩具玩偶的通用默认值 + 运行时玩偶注册表 + 查询 API。
// 声音属性复用 Bark 的 Audio 属性（squeak_sound），可配置模组自定义吱吱音效。
//
// ---- 物品 JSON 用法 ----
// "template": { "type": "plush" }
// "template": {
//   "type": "plush",
//   "squeak_sound": "Assets/Audio/plush_squeak.wav"   // 自定义吱吱音效（可省略，用游戏默认）
// }
//
// ---- 可覆盖的顶级字段 ----
// "category": "utility",
// "weight": 0.15,                 // 重量
// "value": 5,                     // 价值
// "recognition": 6,               // 识别等级
// "destroy_at_zero_condition": true,
// "tags": "belttool",
// "sprite.slot_rotation": 0,      // 物品栏旋转角度
//
// ---- 脚本端查询 ----
// PlushTemplate.IsPlush(itemId)        → bool
// PlushTemplate.GetPlushData(itemId)   → PlushData / null
public class PlushTemplate : ItemTemplate
{
    // ==================== Registry ====================

    private static readonly Dictionary<string, PlushData> Registry = new();

    // ==================== Template ====================

    public override string Name => "plush";

    public override JObject BuildDefaults()
    {
        return new JObject
        {
            // ---- ItemDef 顶级字段（对标原版 plushie 的 ItemInfo） ----
            ["origin_prefab"] = "plushie",
            ["category"] = "utility",
            ["destroy_at_zero_condition"] = true,
            ["weight"] = 0.15,
            ["scale_weight_with_condition"] = false,
            ["value"] = 5,
            ["recognition"] = 6,
            ["tags"] = "belttool",

            // ---- SpriteDef 嵌套字段 ----
            ["sprite"] = new JObject
            {
                ["slot_rotation"] = 0f
            },

            // ---- template 子对象（玩偶专属属性） ----
            ["template"] = new JObject
            {
                // 布尔标记 "plush": true 是 CachePlushItem 的类型识别标志，必须保留。
                ["plush"] = true,
                // 自定义吱吱音效（Bark Audio 属性），空字符串用游戏默认音效
                ["squeak_sound"] = ""
            }
        };
    }

    // ItemLoader 回调：检测 template 中 plush 标记则缓存。
    // template 可为 null（非模板注册物品）。
    // modDir 用于解析 Assets/Audio/ 下的自定义吱吱音效路径。
    public static void CachePlushItem(string itemId, JObject? template, string modDir)
    {
        if (template is null) return;
        if (!template.TryGetValue("plush", out var flag) || !flag.Value<bool>()) return;

        var data = PlushDataFromJObject(template);
        data.ModDir = modDir;

        // 若配置了自定义吱吱音效，预加载 AudioClip
        if (!string.IsNullOrEmpty(data.SqueakSound))
            data.SqueakClip = AudioManager.LoadModAudio(modDir, data.SqueakSound);

        Registry[itemId] = data;
    }

    // ItemLoader 回调：模组热重载时清除玩偶条目
    public static void RemovePlushItem(string itemId)
    {
        Registry.Remove(itemId);
    }

    private static PlushData PlushDataFromJObject(JObject t)
    {
        return new PlushData
        {
            SqueakSound = (string?)t["squeak_sound"] ?? ""
        };
    }

    // ==================== Query API ====================

    public static bool IsPlush(string itemId)
    {
        return Registry.ContainsKey(itemId);
    }

    public static PlushData? GetPlushData(string itemId)
    {
        return Registry.GetValueOrDefault(itemId);
    }

    // ==================== 音效 ====================

    // 播放玩偶吱吱声：
    //   配置了自定义 squeak_sound → 用 Bark Audio 播放自定义音效
    //   否则 → 调用 PlushScript.Squeak() 播放游戏默认音效
    // 返回 true 表示已用自定义音效播放（应拦截默认），false 表示未配置自定义音效。
    public static bool PlayCustomSqueak(PlushScript plush)
    {
        if (plush == null || !plush) return false;

        var item = plush.GetComponent<Item>();
        if (item == null || string.IsNullOrEmpty(item.id)) return false;

        var data = GetPlushData(item.id);
        if (data is not { SqueakClip: not null }) return false;

        Sound.Play(data.SqueakClip, plush.transform.position, true, false);
        return true;
    }

    // 安装 Harmony 补丁：拦截 PlushScript.Squeak()，
    // 当玩偶配置了自定义 squeak_sound 时，用 Bark Audio 播放替代默认音效。
    public static void ApplySqueakHook()
    {
        var method = AccessTools.Method(typeof(PlushScript), "Squeak");
        if (method == null) return;

        try
        {
            var harmony = new Harmony("Bark.PlushTemplate.Squeak");
            harmony.Patch(method,
                prefix: new HarmonyMethod(typeof(PlushTemplate), nameof(OnSqueakPrefix)));
        }
        catch
        {
            // ignored
        }
    }

    // 若玩偶配置了自定义吱吱音效，播放它并拦截游戏默认 Squeak
    private static bool OnSqueakPrefix(PlushScript __instance)
    {
        return !PlayCustomSqueak(__instance);
    }
}
