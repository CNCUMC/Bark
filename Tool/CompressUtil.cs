using System;
using System.Text;
using Bark.ScriptApi;
using CUCoreLib.Helpers;

namespace Bark.Tool;

// 数据压缩工具：封装 CUCoreUtils 的 GZip / Deflate 压缩。
// 为脚本友好，提供以 base64 字符串为载体的版本（byte[] 在 Lua/JS 脚本中难以直接处理）。
[ScriptApi]
public static class CompressUtil
{
    // GZip 压缩字符串，返回 base64 编码
    [ScriptMethod]
    public static string CompressGZip(string text)
    {
        var bytes = CUCoreUtils.CompressGZip(Encoding.UTF8.GetBytes(text));
        return bytes == null
            ? string.Empty 
            : Convert.ToBase64String(bytes);
    }

    // GZip 解压 base64 字符串，返回原始文本
    [ScriptMethod]
    public static string DecompressGZip(string base64)
    {
        if (string.IsNullOrEmpty(base64)) return string.Empty;
        var bytes = CUCoreUtils.DecompressGZip(Convert.FromBase64String(base64));
        return bytes == null
            ? string.Empty
            : Encoding.UTF8.GetString(bytes);
    }

    // Deflate 压缩字符串，返回 base64 编码
    [ScriptMethod]
    public static string CompressDeflate(string text)
    {
        var bytes = CUCoreUtils.CompressDeflate(Encoding.UTF8.GetBytes(text));
        return bytes == null
            ? string.Empty 
            : Convert.ToBase64String(bytes);
    }

    // Deflate 解压 base64 字符串，返回原始文本
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
