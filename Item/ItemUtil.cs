using System.IO;
using UnityEngine;

namespace Bark.Items;

// 自定义物品框架工具方法
public static class ItemUtil
{
    // 从 PNG 文件加载 Sprite，失败返回 null
    public static Sprite? LoadSprite(string path)
    {
        if (!File.Exists(path))
            return null;

        var bytes = File.ReadAllBytes(path);
        var texture = new Texture2D(2, 2);
        if (!texture.LoadImage(bytes))
            return null;

        var pivot = new Vector2(0.5f, 0.5f);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot);
    }

    // RGB 颜色 → Unity Color
    public static Color HexToColor(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return Color.white;

        hex = hex.TrimStart('#');
        if (hex.Length < 6)
            return Color.white;

        var r = (byte)int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        var g = (byte)int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        var b = (byte)int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        var a = hex.Length >= 8
            ? (byte)int.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber)
            : (byte)255;

        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }
}
