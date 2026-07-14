using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Bark.Tool;

public static class TextUtil
{
    private static bool _tmpUnifontSearched;
    private static bool _unifontSearched;
    private static bool _retroGamingTmpSearched;
    private static bool _retroGamingSearched;

    public static TMP_FontAsset? UnifontTMP
    {
        get
        {
            if (field != null) return field;
            if (_tmpUnifontSearched) return null;

            _tmpUnifontSearched = true;
            field = Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
                .FirstOrDefault(f => f.name.Contains("unifont"));

            if (field == null)
                LogUtil.Warning("text.font_not_found", "Unifont");

            return field;
        }
    }  
    
    public static TMP_FontAsset? RetroGamingTMP
    {
        get
        {
            if (field != null) return field;
            if (_retroGamingTmpSearched) return null;

            _retroGamingTmpSearched = true;
            field = Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
                .FirstOrDefault(f => f.name.Contains("Retro Gaming"));

            if (field == null)
                LogUtil.Warning("text.font_not_found", "Retro Gaming");

            return field;
        }
    }

    public static Font? Unifont
    {
        get
        {
            if (field != null) return field;
            if (_unifontSearched) return null;

            _unifontSearched = true;
            field = Resources.FindObjectsOfTypeAll<Font>()
                .FirstOrDefault(f => f.name.Contains("unifont"));

            if (field == null)
                LogUtil.Warning("text.font_not_found", "unifont");

            return field;
        }
    }
    
    public static Font? RetroGaming
    {
        get
        {
            if (field != null) return field;
            if (_retroGamingSearched) return null;

            _retroGamingSearched = true;
            field = Resources.FindObjectsOfTypeAll<Font>()
                .FirstOrDefault(f => f.name.Contains("Retro Gaming"));

            if (field == null)
                LogUtil.Warning("text.font_not_found", "Retro Gaming");

            return field;
        }
    }

    public static string Color(string text, string color)
    {
        return string.IsNullOrEmpty(text) ? text :
            string.IsNullOrEmpty(color) ? text : $"<color={color}>{text}</color>";
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

    public static string Blue(string text) => Color(text, "blue");
    public static string Red(string text) => Color(text, "red");
    public static string Green(string text) => Color(text, "green");
    public static string Yellow(string text) => Color(text, "yellow");
    public static string White(string text) => Color(text, "white");
    public static string Black(string text) => Color(text, "black");
    public static string Cyan(string text) => Color(text, "cyan");
    public static string Magenta(string text) => Color(text, "magenta");
    public static string Gray(string text) => Color(text, "gray");
    public static string Orange(string text) => Color(text, "orange");
    public static string Purple(string text) => Color(text, "purple");
    public static string Pink(string text) => Color(text, "pink");
    public static string Brown(string text) => Color(text, "brown");

    public static string Alpha(string text, string alpha)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(alpha)) return text;
        if (!alpha.StartsWith("#")) alpha = "#" + alpha;
        return $"<alpha={alpha}>{text}<alpha=#FF>";
    }

    public static string Alpha(string text, float alpha)
    {
        return Alpha(text, ((int)(Mathf.Clamp01(alpha) * 255)).ToString("X2"));
    }

    public static string Alpha(string text, byte alpha)
    {
        return Alpha(text, alpha.ToString("X2"));
    }

    public static string Bold(string text) =>
        string.IsNullOrEmpty(text) ? text : $"<b>{text}</b>";

    public static string Italic(string text) =>
        string.IsNullOrEmpty(text) ? text : $"<i>{text}</i>";

    public static string Unline(string text) =>
        string.IsNullOrEmpty(text) ? text : $"<u>{text}</u>";

    public static string Delete(string text) =>
        string.IsNullOrEmpty(text) ? text : $"<s>{text}</s>";

    public static string Size(string text, int size) =>
        string.IsNullOrEmpty(text) ? text : $"<size={size}>{text}</size>";

    public static string SimpleMarkDown(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        text = Regex.Replace(text, @"\*\*\*(.+?)\*\*\*", Bold(Italic("$1")));
        text = Regex.Replace(text, @"\*\*(.+?)\*\*", Bold("$1"));
        text = Regex.Replace(text, @"\*(.+?)\*", Italic("$1"));
        text = Regex.Replace(text, @"__(.+?)__", Unline("$1"));
        text = Regex.Replace(text, @"~~(.+?)~~", Delete("$1"));

        return text;
    }
}
