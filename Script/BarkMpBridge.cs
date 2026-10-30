using System;
using System.Reflection;
using CUCoreLib.Networking;
using Newtonsoft.Json.Linq;

namespace Bark.Script;

public static class BarkMpBridge
{
    private static MemberInfo? _localPlayerMember;
    public static bool IsAvailable => MultiplayerApi.IsAvailable;
    public static bool IsRunning => MultiplayerApi.IsRunning;
    public static bool IsClient => MultiplayerApi.IsClient;
    public static bool IsServer => MultiplayerApi.IsServer;
    public static bool IsHost => MultiplayerApi.IsHost;
    public static bool IsConnected => MultiplayerBridge.IsConnected;

    public static bool HasLocalPlayer
    {
        get
        {
            if (!IsClient) return true;

            try
            {
                if (!field)
                {
                    _localPlayerMember =
                        MultiplayerApi.FindKrokMpMember("ScavPlayer", "LOCAL_PLAYER");
                    field = true;
                }

                var member = _localPlayerMember;
                if (member is null) return true;

                var value = member switch
                {
                    FieldInfo fi => fi.GetValue(null),
                    PropertyInfo { CanRead: true } pi => pi.GetValue(null),
                    _ => null
                };

                return value != null;
            }
            catch
            {
                return true;
            }
        }
    }

    public static void Initialize()
    {
        MultiplayerBridge.Initialize();
    }

    public static void RegisterServerHandler(string channel, Func<JToken, JToken> handler)
    {
        MultiplayerApi.RegisterServerHandler(channel, handler);
    }

    public static void RegisterClientHandler(string channel, Action<JToken> handler)
    {
        MultiplayerApi.RegisterClientHandler(channel, handler);
    }

    public static bool RequestServer(string channel, object? payload,
        Action<JToken> onResponse)
    {
        return MultiplayerApi.RequestServer(channel, payload, onResponse);
    }

    public static bool SendToServer(string channel, object? payload)
    {
        return MultiplayerApi.SendToServer(channel, payload);
    }

    public static bool BroadcastToClients(string channel, object? payload)
    {
        return MultiplayerApi.Broadcast(channel, payload, includeHost: false);
    }
}
