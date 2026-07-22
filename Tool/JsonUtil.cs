using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bark.Tool;

public static class JsonUtil
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    // 将对象序列化为 JSON 字符串
    public static string Serialize(object? value, bool indented = true)
    {
        var formatting = indented ? Formatting.Indented : Formatting.None;
        return JsonConvert.SerializeObject(value, formatting);
    }

    // 将 JSON 字符串反序列化为指定类型
    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        return JsonConvert.DeserializeObject<T>(json, Settings);
    }

    // 将 JSON 字符串反序列化为指定类型（非泛型）
    public static object? Deserialize(string json, Type type)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonConvert.DeserializeObject(json, type, Settings);
    }

    // 解析 JSON 字符串为 JObject
    public static JObject Parse(string json)
    {
        return JObject.Parse(json);
    }

    // 安全解析 JSON 字符串为 JObject，失败返回 null
    public static JObject? TryParse(string json)
    {
        try
        {
            return JObject.Parse(json);
        }
        catch
        {
            return null;
        }
    }

    // 解析 JSON 字符串为 JArray
    public static JArray ParseArray(string json)
    {
        return JArray.Parse(json);
    }

    // 安全解析 JSON 字符串为 JArray，失败返回 null
    public static JArray? TryParseArray(string json)
    {
        try
        {
            return JArray.Parse(json);
        }
        catch
        {
            return null;
        }
    }

    // 从文件读取并解析为 JObject
    public static JObject ReadFile(string path)
    {
        var text = File.ReadAllText(path);
        return JObject.Parse(text);
    }

    // 安全从文件读取并解析为 JObject，失败返回 null
    public static JObject? TryReadFile(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            var text = File.ReadAllText(path);
            return JObject.Parse(text);
        }
        catch
        {
            return null;
        }
    }

    // 从文件读取并反序列化为指定类型
    public static T? ReadFile<T>(string path)
    {
        var text = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(text, Settings);
    }

    // 将对象写入 JSON 文件
    public static void WriteFile(string path, object value, bool indented = true)
    {
        var formatting = indented ? Formatting.Indented : Formatting.None;
        var json = JsonConvert.SerializeObject(value, formatting);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, json + Environment.NewLine);
    }

    // 将 JObject 写入 JSON 文件
    public static void WriteFile(string path, JObject obj)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, obj.ToString(Formatting.Indented) + Environment.NewLine);
    }

    // 从 JObject 中安全获取值
    public static T? Get<T>(JObject obj, string key, T? defaultValue = default)
    {
        var token = obj[key];
        return token == null ? defaultValue : token.ToObject<T>();
    }

    // 从 JObject 中安全获取嵌套值（支持点号路径，如 "a.b.c"）
    public static T? GetNested<T>(JObject obj, string path, T? defaultValue = default)
    {
        var token = obj.SelectToken(path);
        return token == null ? defaultValue : token.ToObject<T>();
    }

    // 合并两个 JObject（source 的值覆盖 target）
    public static JObject Merge(JObject target, JObject source)
    {
        target.Merge(source, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Replace,
            MergeNullValueHandling = MergeNullValueHandling.Ignore
        });
        return target;
    }

    // 将对象转换为 JObject
    public static JObject FromObject(object value)
    {
        return JObject.FromObject(value);
    }

    // 将 JSON 字符串格式化（美化输出）
    public static string PrettyPrint(string json)
    {
        var obj = JToken.Parse(json);
        return obj.ToString(Formatting.Indented);
    }
}
