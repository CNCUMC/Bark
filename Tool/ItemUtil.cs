using System.Globalization;
using System.IO;
using Bark.ScriptApi;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Tool;

[ScriptApi]
public static class ItemUtil
{
    // 从 PNG 文件加载 Sprite，失败返回 null。
    // importScale: 放大倍数，1.0=默认大小，2.0=两倍大，0.5=一半。
    // 内部会将 16 PPU 作为基准值缩放。
    public static Sprite? LoadSprite(string path, float importScale = 1f)
    {
        if (!File.Exists(path))
            return null;

        var bytes = File.ReadAllBytes(path);
        var texture = new Texture2D(2, 2);
        if (!texture.LoadImage(bytes))
            return null;

        // Point 过滤避免像素风格精灵模糊
        texture.filterMode = FilterMode.Point;

        // 基准 16 PPU，importScale 越大精灵越大
        var pixelsPerUnit = 16f / importScale;
        var pivot = new Vector2(0.5f, 0.5f);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot, pixelsPerUnit);
    }

    // RGB(A) 颜色 → Unity Color
    public static Color HexToColor(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return Color.white;

        hex = hex.TrimStart('#');
        if (hex.Length < 6)
            return Color.white;

        var r = (byte)int.Parse(hex[..2], NumberStyles.HexNumber);
        var g = (byte)int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
        var b = (byte)int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
        var a = hex.Length >= 8
            ? (byte)int.Parse(hex.Substring(6, 2), NumberStyles.HexNumber)
            : (byte)255;

        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    public static void SetCondition(Item? item, float condition)
    {
        item?.SetCondition(Mathf.Clamp01(condition));
    }

    // amount: 修复量（正值修复，负值扣耐久）。默认 1f 即完全修复。
    public static void Repair(Item? item, float amount = 1f)
    {
        if (item is null) return;
        SetCondition(item, Mathf.Clamp01(item.condition + amount));
    }

    public static void SetFavourited(Item? item, bool favourited)
    {
        if (item != null) item.favourited = favourited;
    }

    public static void Destroy(Item? item)
    {
        if (item == null) return;
        if (item.ParentContainer() != null) item.transform.SetParent(null, true);
        Object.Destroy(item.gameObject);
    }

    [ScriptMethod]
    public static void SetCondition(string itemId, float condition)
    {
        if (InventoryUtil.FindById(itemId, out var item))
            SetCondition(item, condition);
    }

    [ScriptMethod]
    public static void Repair(string itemId, float amount = 1f)
    {
        if (InventoryUtil.FindById(itemId, out var item))
            Repair(item, amount);
    }

    [ScriptMethod]
    public static void SetFavourited(string itemId, bool favourited)
    {
        if (InventoryUtil.FindById(itemId, out var item))
            SetFavourited(item, favourited);
    }

    [ScriptMethod]
    public static void Destroy(string itemId)
    {
        if (InventoryUtil.FindById(itemId, out var item))
            Destroy(item);
    }
}