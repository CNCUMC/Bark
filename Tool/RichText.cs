using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class RichText
{
    public static string Color(string text, string color)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
        return string.IsNullOrEmpty(color)
            ? text
            : $"<color={color}>{text}</color>";
    }

    public static string Color(string text, Color color)
    {
        return Color(text, ColorUtility.ToHtmlStringRGB(color));
    }

    public static string Hex(string text, string hex)
    {
        if (string.IsNullOrEmpty(hex)) return text;
        if (!hex.StartsWith("#")) hex = "#" + hex;
        return Color(text, hex);
    }

    public static string Blue(string text)
    {
        return Color(text, "blue");
    }

    public static string Red(string text)
    {
        return Color(text, "red");
    }

    public static string Green(string text)
    {
        return Color(text, "green");
    }

    public static string Yellow(string text)
    {
        return Color(text, "yellow");
    }

    public static string White(string text)
    {
        return Color(text, "white");
    }

    public static string Black(string text)
    {
        return Color(text, "black");
    }

    public static string Cyan(string text)
    {
        return Color(text, "cyan");
    }

    public static string Magenta(string text)
    {
        return Color(text, "magenta");
    }

    public static string Gray(string text)
    {
        return Color(text, "gray");
    }

    public static string Orange(string text)
    {
        return Color(text, "orange");
    }

    public static string Purple(string text)
    {
        return Color(text, "purple");
    }

    public static string Pink(string text)
    {
        return Color(text, "pink");
    }

    public static string Brown(string text)
    {
        return Color(text, "brown");
    }

    public static string Alpha(string text, string alphaHex)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
        if (string.IsNullOrEmpty(alphaHex)) return text;

        if (!alphaHex.StartsWith("#")) alphaHex = "#" + alphaHex;

        return $"<alpha={alphaHex}>{text}<alpha=#FF>";
    }

    public static string Alpha(string text, float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        var hexValue = ((int)(alpha * 255)).ToString("X2");
        return Alpha(text, hexValue);
    }

    public static string Alpha(string text, byte alpha)
    {
        return Alpha(text, alpha.ToString("X2"));
    }

    public static string Alpha(string text, int alpha)
    {
        return Alpha(text, (byte)Mathf.Clamp(alpha, 0, 255));
    }

    public static string Bold(string text)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
        return $"<b>{text}</b>";
    }

    public static string Italic(string text)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
        return $"<i>{text}</i>";
    }

    public static string Size(string text, int size)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
        return $"<size={size}>{text}</size>";
    }
}
