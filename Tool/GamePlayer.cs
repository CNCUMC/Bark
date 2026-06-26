using System;
using System.Diagnostics.CodeAnalysis;
using Bark.BetterCCL;
using BepInEx.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class GamePlayer
{
    public const int MaxInventorySlots = 8;
    private static readonly ManualLogSource Logger = Plugin.Logger;

    public static void Tp(Vector2 vector2)
    {
        GameWorld.CheckForWorld(Logger);

        if (PlayerCamera.main.body == null)
            throw new InvalidOperationException(Locale("log.player.body_null"));

        PlayerCamera.main.body.transform.position = vector2;
        PlayerCamera.main.transform.position = vector2;
    }

    public static void Tp(float x, float y)
    {
        Tp(new Vector2(x, y));
    }

    public static void PickItem(string item, int slot, bool force = false)
    {
        GameWorld.CheckForWorld(Logger);

        if (string.IsNullOrWhiteSpace(item))
            throw new ArgumentException(
                Locale("player.item.null_or_empty"), nameof(item));

        if (slot
            is < 0
            or >= MaxInventorySlots)
            throw new ArgumentOutOfRangeException(nameof(slot), slot,
                Locale("slot.out_of_range", MaxInventorySlots));

        if (PlayerCamera.main.body == null)
            throw new InvalidOperationException(Locale("player.body_null"));

        var body = PlayerCamera.main.body;
        var position = body.transform.position;

        var createdObject = Utils.Create(item, position, 0.0f);
        if (createdObject == null)
            throw new InvalidOperationException(
                Locale("player.load_item.fail", item));

        var itemComponent = createdObject.GetComponent<Item>();
        if (itemComponent == null)
        {
            Object.Destroy(createdObject);
            throw new InvalidOperationException(
                Locale("player.load_item.missing_component", item));
        }

        body.PickUpItem(itemComponent, slot, force);
    }

    private static string Locale(string key, params object[] args)
    {
        return BetterLocale.Other("log." + key, args);
    }
}