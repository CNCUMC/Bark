using System;
using System.Collections.Generic;
using Bark.Tool;
using Newtonsoft.Json.Linq;

namespace Bark.Items.Templates;

public static class TemplateLoader
{
    private static readonly Dictionary<string, JObject> _templates = new();
    internal static IEnumerable<string> RegisteredNames => _templates.Keys;

    public static void Register(string name, JObject template)
    {
        if (name is null) throw new ArgumentNullException(nameof(name));

        _templates[name] = template ?? throw new ArgumentNullException(nameof(template));
        LogUtil.Info("template.registered", name);
    }

    public static void RegisterFromJson(string name, string json)
    {
        if (json is null) throw new ArgumentNullException(nameof(json));

        var obj = JObject.Parse(json);
        Register(name, obj);
    }

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

        var templateParams = (JObject)templateObj.DeepClone();
        templateParams.Remove("type");

        try
        {
            var merged = (JObject)templateDefaults.DeepClone();

            var mergedTemplate = merged["template"] as JObject;
            if (mergedTemplate != null && templateParams.Count > 0)
                JsonUtil.Merge(mergedTemplate, templateParams);
            mergedTemplate?.Remove("type");

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
