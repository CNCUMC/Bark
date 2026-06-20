using System;
using System.Diagnostics.CodeAnalysis;
using BepInEx;
using CUCoreLib.Registries;
using UnityEngine;

namespace Bark.Tool;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[BepInDependency("KrokoshaCasualtiesMP", BepInDependency.DependencyFlags.SoftDependency)]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class Multiplayer
{
    private static Type _krokoshaType;
    private static bool _initialized;

    public static bool IsNetworkRunning
    {
        get
        {
            Initialize();
            if (_krokoshaType == null)
                return false;

            var prop = _krokoshaType.GetProperty("network_system_is_running");
            return prop != null && (bool)prop.GetValue(null, null);
        }
    }

    public static bool IsClient
    {
        get
        {
            Initialize();
            if (_krokoshaType == null)
                return false;

            var prop = _krokoshaType.GetProperty("is_client");
            return prop != null && (bool)prop.GetValue(null, null);
        }
    }

    private static void Initialize()
    {
        if (_initialized)
            return;

        _krokoshaType = Type.GetType("KrokoshaScavMultiplayer, KrokoshaCasualtiesMP");
        _initialized = true;
    }

    public static void Tp(Vector2 vector2)
    {
        World.CheckForWorld();

        if (!IsNetworkRunning)
        {
            Player.Tp(vector2);
            return;
        }

        if (PlayerCamera.main.body == null)
            throw new InvalidOperationException(Locale("tool.player.body_null"));

        PlayerCamera.main.body.transform.position = vector2;
        PlayerCamera.main.transform.position = vector2;
    }

    public static void Tp(float x, float y)
    {
        Tp(new Vector2(x, y));
    }

    public static void Tp(string playerName, Vector2 vector2)
    {
        World.CheckForWorld();

        if (string.IsNullOrWhiteSpace(playerName))
            throw new ArgumentException(Locale("tool.multiplayer.player_name.null_or_empty"), nameof(playerName));

        if (!IsNetworkRunning)
        {
            Player.Tp(vector2);
            return;
        }

        if (PlayerCamera.main.body == null)
            throw new InvalidOperationException(Locale("tool.player.body_null"));

        bool success;
        var actualName = playerName;

        if (playerName == "@a")
        {
            PerformActionOnAllPlayers(plr => { TeleportPlayer(plr, vector2); });
            success = true;
        }
        else
        {
            success = PerformActionOnPlayer(playerName, plr =>
            {
                actualName = GetPlayerName(plr);
                TeleportPlayer(plr, vector2);
            });
        }

        LogToConsole(success
            ? Locale("tool.multiplayer.teleport.success", actualName, vector2)
            : Locale("tool.multiplayer.teleport.fail", actualName));
    }

    public static void Tp(string playerName, float x, float y)
    {
        Tp(playerName, new Vector2(x, y));
    }

    private static void TeleportPlayer(object player, Vector2 position)
    {
        if (player == null)
            return;

        try
        {
            var bodyProp = player.GetType().GetProperty("playerbody");
            var body = bodyProp?.GetValue(player, null);

            if (body != null)
            {
                var bodyTransform = ((Component)body).transform;
                if (bodyTransform != null) bodyTransform.position = position;
            }

            var playerTransformProp = player.GetType().GetProperty("transform");
            var playerTransform = playerTransformProp?.GetValue(player, null) as Transform;
            if (playerTransform != null) playerTransform.position = position;
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to teleport player: {ex.Message}", Plugin.Logger);
        }
    }

    private static bool PerformActionOnPlayer(string playerName, Action<object> action)
    {
        try
        {
            var serverMainType = Type.GetType("ServerMain, Assembly-CSharp");
            if (serverMainType == null)
                return false;

            var method = serverMainType.GetMethod("_PerformActionOnPlayersByName");
            if (method == null)
                return false;

            var executed = false;
            var wrapper = new Action<object>(plr =>
            {
                action(plr);
                executed = true;
            });

            var parameters = new object[] { playerName, wrapper, null, true };
            method.Invoke(null, parameters);

            return executed;
        }
        catch
        {
            return false;
        }
    }

    private static void PerformActionOnAllPlayers(Action<object> action)
    {
        try
        {
            var serverMainType = Type.GetType("ServerMain, Assembly-CSharp");
            if (serverMainType == null)
                return;

            var method = serverMainType.GetMethod("_PerformActionOnPlayersByName");
            if (method == null)
                return;

            var parameters = new object[] { "@a", action, null, true };
            method.Invoke(null, parameters);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to perform action on all players: {ex.Message}", Plugin.Logger);
        }
    }

    public static string GetPlayerName(object player)
    {
        if (player == null)
            return "Unknown";

        try
        {
            var prop = player.GetType().GetProperty("playername");
            return prop?.GetValue(player, null) as string ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private static void LogToConsole(string message)
    {
        try
        {
            var conType = Type.GetType("Con, Assembly-CSharp");
            if (conType != null)
            {
                var method = conType.GetMethod("LogToConsole");
                if (method != null)
                {
                    method.Invoke(null, [message]);
                    return;
                }
            }
        }
        catch
        {
            // Ignore: fallback to BepInEx log when game console is unavailable
        }

        Plugin.Logger.LogInfo(message);
    }

    private static string Locale(string key, params object[] args)
    {
        var text = LocaleRegistry.Get("other", key, key);
        return args.Length > 0 ? string.Format(text, args) : text;
    }
}
