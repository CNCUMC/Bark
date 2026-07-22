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

    public static string Serialize(object? value, bool indented = true)
    {
        var formatting = indented
            ? Formatting.Indented 
            : Formatting.None;
        return JsonConvert.SerializeObject(value, formatting);
    }

    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        return JsonConvert.DeserializeObject<T>(json, Settings);
    }

    public static object? Deserialize(string json, Type type)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonConvert.DeserializeObject(json, type, Settings);
    }

    public static JObject Parse(string json)
    {
        return JObject.Parse(json);
    }

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

    public static JArray ParseArray(string json)
    {
        return JArray.Parse(json);
    }

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

    public static JObject ReadFile(string path)
    {
        var text = File.ReadAllText(path);
        return JObject.Parse(text);
    }

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

    public static T? ReadFile<T>(string path)
    {
        var text = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(text, Settings);
    }

    public static void WriteFile(string path, object value, bool indented = true)
    {
        var formatting = indented ? Formatting.Indented : Formatting.None;
        var json = JsonConvert.SerializeObject(value, formatting);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, json + Environment.NewLine);
    }

    public static void WriteFile(string path, JObject obj)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, obj.ToString(Formatting.Indented) + Environment.NewLine);
    }

    public static T? Get<T>(JObject obj, string key, T? defaultValue = default)
    {
        var token = obj[key];
        return token == null ? defaultValue : token.ToObject<T>();
    }

    public static T? GetNested<T>(JObject obj, string path, T? defaultValue = default)
    {
        var token = obj.SelectToken(path);
        return token == null ? defaultValue : token.ToObject<T>();
    }

    public static JObject Merge(JObject target, JObject source)
    {
        target.Merge(source, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Replace,
            MergeNullValueHandling = MergeNullValueHandling.Ignore
        });
        return target;
    }

    public static JObject FromObject(object value)
    {
        return JObject.FromObject(value);
    }

    public static string PrettyPrint(string json)
    {
        var obj = JToken.Parse(json);
        return obj.ToString(Formatting.Indented);
    }
}
