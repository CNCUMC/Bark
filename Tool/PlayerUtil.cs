using System;
using Bark.BetterCCL;
using Bark.ScriptApi;
using CUCoreLib.Helpers;
using UnityEngine;

namespace Bark.Tool;

// 玩家通用操作：传送、物品、警告
[ScriptApi]
public static class PlayerUtil
{
    public const int MaxInventorySlots = 8;

    [ScriptMethod]
    public static Vector2 GetPosition()
    {
        CheckUtil.CheckBody(Plugin.Logger);
        return BodyUtil.Body.transform.position;
    }

    public static void Teleport(Vector2 pos)
    {
        CheckUtil.CheckBody(Plugin.Logger);
        BodyUtil.Body.transform.position = pos;
    }

    [ScriptMethod]
    public static void Teleport(float x, float y)
    {
        Teleport(new Vector2(x, y));
    }

    [ScriptMethod]
    public static void PickUpItem(string item, int slot, bool force = false)
    {
        CheckUtil.CheckBody(Plugin.Logger);
        CheckUtil.CheckNotNullOrEmpty(item, nameof(item));
        if (slot is < 0 or >= MaxInventorySlots)
            throw new ArgumentOutOfRangeException(nameof(slot), slot,
                LocaleLog("player.slot.out_of_range", MaxInventorySlots));
        var pos = BodyUtil.Body.transform.position;
        var go = Utils.Create(item, pos, 0f) ??
                 throw new InvalidOperationException(LocaleLog("player.load_item.fail", item));
        var cmp = go.GetComponent<Item>() ??
                  throw new InvalidOperationException(LocaleLog("player.load_item.missing_component", item));
        BodyUtil.Body.PickUpItem(cmp, slot, force);
    }

    [ScriptMethod]
    public static void Alert(string text, bool important, float delay = 0f)
    {
        if (string.IsNullOrWhiteSpace(text) || BodyUtil.Body == null) return;
        if (delay <= 0f) CUCoreUtils.Alert(text, important);
        else
            CUCoreUtils.Alert(text, important, delay);
    }

    // 播放指定音效。默认在玩家位置播放。
    [ScriptMethod]
    public static void PlaySound(string soundName, float x = float.NaN, float y = float.NaN)
    {
        if (string.IsNullOrEmpty(soundName) || BodyUtil.Body is not { transform: var t }) return;
        var pos = float.IsNaN(x) || float.IsNaN(y)
            ? (Vector2)t.position
            : new Vector2(x, y);
        Sound.Play(soundName, pos);
    }

    // 在玩家脚下生成物品，自动捡起。count 为 0 时不限。
    [ScriptMethod]
    public static void CreateAndPickup(string itemId, int count = 1)
    {
        if (string.IsNullOrEmpty(itemId) || BodyUtil.Body is not { transform: var t } body) return;

        var pos = t.position;
        var actual = count > 0 ? count : 1;
        for (var i = 0; i < actual; i++)
        {
            var go = Utils.Create(itemId, pos, 0f);
            if (go == null) continue;
            var cmp = go.GetComponent<Item>();
            if (cmp != null) body.AutoPickUpItem(cmp);
        }
    }

    private static string LocaleLog(string key, params object[] args)
    {
        return BetterLocale.GetLog(key, args);
    }
}