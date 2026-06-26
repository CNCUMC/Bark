using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class RichTextUtil
{
    public static string Color(string text, string color) => string.IsNullOrEmpty(text) ? text ?? string.Empty : string.IsNullOrEmpty(color) ? text : $"<color={color}>{text}</color>";
    public static string Color(string text, Color color) => Color(text, ColorUtility.ToHtmlStringRGB(color));
    public static string Hex(string text, string hex) { if (string.IsNullOrEmpty(hex)) return text; if (!hex.StartsWith("#")) hex = "#" + hex; return Color(text, hex); }
    public static string Blue(string t) => Color(t, "blue"); public static string Red(string t) => Color(t, "red");
    public static string Green(string t) => Color(t, "green"); public static string Yellow(string t) => Color(t, "yellow");
    public static string White(string t) => Color(t, "white"); public static string Black(string t) => Color(t, "black");
    public static string Cyan(string t) => Color(t, "cyan"); public static string Magenta(string t) => Color(t, "magenta");
    public static string Gray(string t) => Color(t, "gray"); public static string Orange(string t) => Color(t, "orange");
    public static string Purple(string t) => Color(t, "purple"); public static string Pink(string t) => Color(t, "pink");
    public static string Brown(string t) => Color(t, "brown");
    public static string Alpha(string text, string a) { if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(a)) return text ?? string.Empty; if (!a.StartsWith("#")) a = "#" + a; return $"<alpha={a}>{text}<alpha=#FF>"; }
    public static string Alpha(string text, float a) => Alpha(text, ((int)(Mathf.Clamp01(a) * 255)).ToString("X2"));
    public static string Alpha(string text, byte a) => Alpha(text, a.ToString("X2"));
    public static string Bold(string t) => string.IsNullOrEmpty(t) ? t ?? string.Empty : $"<b>{t}</b>";
    public static string Italic(string t) => string.IsNullOrEmpty(t) ? t ?? string.Empty : $"<i>{t}</i>";
    public static string Size(string t, int s) => string.IsNullOrEmpty(t) ? t ?? string.Empty : $"<size={s}>{t}</size>";
}
