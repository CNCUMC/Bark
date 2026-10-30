using System;
using System.Text;
using Bark.ScriptApi;
using CUCoreLib.Helpers;

namespace Bark.Tool;

[ScriptApi]
public static class CompressUtil
{
    [ScriptMethod]
    public static string CompressGZip(string text)
    {
        var bytes = CUCoreUtils.CompressGZip(Encoding.UTF8.GetBytes(text));
        return bytes == null
            ? string.Empty
            : Convert.ToBase64String(bytes);
    }

    [ScriptMethod]
    public static string DecompressGZip(string base64)
    {
        if (string.IsNullOrEmpty(base64)) return string.Empty;
        var bytes = CUCoreUtils.DecompressGZip(Convert.FromBase64String(base64));
        return bytes == null
            ? string.Empty
            : Encoding.UTF8.GetString(bytes);
    }

    [ScriptMethod]
    public static string CompressDeflate(string text)
    {
        var bytes = CUCoreUtils.CompressDeflate(Encoding.UTF8.GetBytes(text));
        return bytes == null
            ? string.Empty
            : Convert.ToBase64String(bytes);
    }

    [ScriptMethod]
    public static string DecompressDeflate(string base64)
    {
        if (string.IsNullOrEmpty(base64)) return string.Empty;
        var bytes = CUCoreUtils.DecompressDeflate(Convert.FromBase64String(base64));
        return bytes == null
            ? string.Empty
            : Encoding.UTF8.GetString(bytes);
    }
}
