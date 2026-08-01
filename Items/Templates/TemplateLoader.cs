using System;
using System.Collections.Generic;
using Bark.Tool;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

// 物品模板注册表与解析器。
// 注册途径：
//   C# 端   → 继承 ItemTemplate 基类，调用 Register()
//   脚本端  → TemplateLoader.Register / RegisterFromJson
// 物品 JSON 中通过 "template": { "type": "gun", ... } 引用已注册的模板，
// TemplateLoader 负责将模板默认值与用户字段合并。
public static class TemplateLoader
{
    // 已注册的模板，key 为模板名（对应用户 JSON 中 template.type 的值）
    private static readonly Dictionary<string, JObject> _templates = new();

    // 返回所有已注册的模板名称
    internal static IEnumerable<string> RegisteredNames => _templates.Keys;

    // ---- 注册 ----

    // 注册一个模板（JObject 形式，适用于脚本 / JSON 注册）。
    // 同名模板会被后注册的覆盖。
    public static void Register(string name, JObject template)
    {
        if (name is null) throw new ArgumentNullException(nameof(name));

        _templates[name] = template ?? throw new ArgumentNullException(nameof(template));
        LogUtil.Info("template.registered", name);
    }

    // 从 JSON 字符串注册模板，内部调用 JObject.Parse + Register
    public static void RegisterFromJson(string name, string json)
    {
        if (json is null) throw new ArgumentNullException(nameof(json));

        var obj = JObject.Parse(json);
        Register(name, obj);
    }

    // ---- 解析与合并 ----

    // 解析 template 引用并与用户物品 JSON 合并。
    // userObj      - 用户物品 JObject（会被原地修改：template 字段被移除）
    // templateObj  - JSON 中的 template 对象，含 type 和模板参数
    // 返回值：合并后的 JObject；若模板未注册或解析失败返回 null，调用方应回退到原始 JSON
    public static JObject? ResolveAndMerge(JObject userObj, JObject templateObj)
    {
        var type = templateObj["type"]?.Value<string>();
        if (string.IsNullOrEmpty(type))
        {
            LogUtil.Warning("template.missing_type");
            return null;
        }

        if (!_templates.TryGetValue(type, out var templateDefaults))
        {
            LogUtil.Warning("template.not_registered", type);
            return null;
        }

        // 提取模板参数（type 之外的所有字段，如 ammo_type、capacity 等）
        var templateParams = (JObject)templateObj.DeepClone();
        templateParams.Remove("type");

        try
        {
            var merged = (JObject)templateDefaults.DeepClone();

            // 用户模板参数覆盖到 merged.template 子对象
            var mergedTemplate = merged["template"] as JObject;
            if (mergedTemplate != null && templateParams.Count > 0)
                JsonUtil.Merge(mergedTemplate, templateParams);
            mergedTemplate?.Remove("type");

            // 移除用户 JSON 中的 template 字段，合并其余字段到根级
            userObj.Remove("template");
            JsonUtil.Merge(merged, userObj);

            return merged;
        }
        catch (Exception ex)
        {
            LogUtil.Warning("template.merge_error", type, ex.Message);
            return null;
        }
    }
}
