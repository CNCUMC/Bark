using System;
using Bark.BetterCCL;
using Bark.ScriptApi;
using CUCoreLib.Helpers;
using UnityEngine;

namespace Bark.Tool;

// 玩家通用操作：传送、物品、警告
public static class PlayerUtil
{
    public const int MaxInventorySlots = 8;

    public static Body Body => PlayerCamera.main.body!;

    [ScriptMethod]
    public static Vector2 GetPosition()
    {
        CheckUtil.CheckBody(Plugin.Logger);
        return Body.transform.position;
    }

    public static void Teleport(Vector2 pos)
    {
        CheckUtil.CheckBody(Plugin.Logger);
        Body.transform.position = pos;
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
        var pos = Body.transform.position;
        var go = Utils.Create(item, pos, 0f) ??
                 throw new InvalidOperationException(LocaleLog("player.load_item.fail", item));
        var cmp = go.GetComponent<Item>() ??
                  throw new InvalidOperationException(LocaleLog("player.load_item.missing_component", item));
        Body.PickUpItem(cmp, slot, force);
    }

    [ScriptMethod]
    public static void Alert(string text, bool important, float delay = 0f)
    {
        if (string.IsNullOrWhiteSpace(text) || Body == null) return;
        if (delay <= 0f) CUCoreUtils.Alert(text, important);
        else
            CUCoreUtils.Alert(text, important, delay);
    }

    private static string LocaleLog(string key, params object[] args)
    {
        return BetterLocale.GetLog(key, args);
    }
}
