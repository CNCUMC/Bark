using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using CUCoreLib.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bark.Script;

// Bark 自建 KrokMP 4.0.1 反射网络层。
// 背景：CUCoreLib 1.0.3 的 MultiplayerBridge.TryResolveRuntime() 用严格类型匹配反射解析
// Server_SendTo 的第三参为 uint，但 KrokMP 4.0.1 改成了 knetid 结构体，导致解析失败、
// IsAvailable 恒为 false，Bark 依赖它的模组同步（NetworkModSync/HostModFetcher）从未启动。
// 本类绕开 CUCoreLib，纯反射直接对接 KrokMP 4.0.1，消息协议（channel/kind/requestId/sender/payload
// + JSON -> GZip -> Base64，消息 ID 56420/56421）与 CUCoreLib 保持一致，保证互通且不破坏现有逻辑。
// KrokMP 未加载时 IsAvailable=false，调用方守卫即可，零开销。
public static class BarkKrokBridge
{
    // 与 CUCoreLib MultiplayerBridge 保持一致的消息 ID：客户端->服务器 request/event，服务器->客户端 response
    private const ushort RequestMessageId = 56420;
    private const ushort ResponseMessageId = 56421;

    // 信封字段名（与 CUCoreLib 一致，保证互通）
    private const string ChannelField = "channel";
    private const string KindField = "kind";
    private const string RequestIdField = "requestId";
    private const string SenderField = "sender";
    private const string PayloadField = "payload";

    private static readonly Dictionary<string, Func<JToken, JToken>> ServerHandlers =
        new(StringComparer.Ordinal);

    private static readonly Dictionary<string, Action<JToken>> ClientHandlers =
        new(StringComparer.Ordinal);

    private static readonly Dictionary<string, Action<JToken>> PendingResponses =
        new(StringComparer.Ordinal);

    private static bool _initialized;

    // 程序集与类型缓存（反射句柄，初始化前为 null）
    private static Type? _netType;
    private static Type? _netDataReaderType;
    private static Type? _netDataWriterType;
    private static Type? _knetidType;
    private static Type? _deliveryMethodType;
    private static Type? _serverMainType;

    // 方法/成员句柄（反射句柄，初始化前为 null）
    private static MethodInfo? _createWriterMethod;
    private static MethodInfo? _clientSendMethod;
    private static MethodInfo? _serverSendToClientsMethod;
    private static MethodInfo? _registerServerReceiverMethod;
    private static MethodInfo? _registerClientReceiverMethod;
    private static MethodInfo? _writerPutStringMethod;
    private static MethodInfo? _readerGetStringMethod;
    private static PropertyInfo? _serverMainAllClientIdsProperty;
    private static FieldInfo? _netPlayerLocalPlayerField;

    // 缓存的接收委托（KrokMP 的 ShutdownReset 会清空 SERVER/CLIENT_MESSAGE_HANDLERS，
    // 需在需要时幂等重新注册）
    private static Delegate? _serverReceiverDelegate;
    private static Delegate? _clientReceiverDelegate;

    private static object? _reliableOrdered;
    private static object? _reliableUnordered;

    // 当前是否成功解析 KrokMP 并注册了接收器；false 表示网络层不可用
    public static bool IsAvailable { get; private set; }

    // 角色判断（反射读 KrokMP Net 静态属性）；不可用时返回 false
    public static bool IsRunning => GetNetBool("running");
    public static bool IsClient => GetNetBool("is_client");
    public static bool IsServer => GetNetBool("is_server");
    public static bool IsHost => GetNetBool("is_host");

    // 是否已连接（客户端连上服务器后为 true；用于替代 CUCoreLib.IsInWorld() 在客机上不准确的问题）
    public static bool IsConnected => GetNetBool("is_connected");

    // 客机本地玩家是否已创建（NetPlayer.LOCAL_PLAYER 非空）。
    // KrokMP 的 Client_Send 在 LOCAL_PLAYER == null 时会触发 "SENDING A PACKET TOO EARLY" 并断开连接，
    // 因此客机发送消息前必须等待本地玩家就绪（进世界后创建）。
    public static bool HasLocalPlayer
    {
        get
        {
            try
            {
                return _netPlayerLocalPlayerField?.GetValue(null) != null;
            }
            catch
            {
                return false;
            }
        }
    }

    // 启动时解析 KrokMP 并缓存反射句柄；任一关键句柄缺失时降级为不可用并记录诊断日志
    public static bool Initialize()
    {
        if (_initialized)
            return IsAvailable;

        _initialized = true;
        IsAvailable = TryResolveRuntime();
        return IsAvailable;
    }

    // 服务端 handler：收到 kind=request 时按 channel 分派，返回的 JToken 作为 response 回发给请求客户端
    public static void RegisterServerHandler(string channel, Func<JToken, JToken> handler)
    {
        if (string.IsNullOrWhiteSpace(channel))
            return;

        ServerHandlers[channel.Trim()] = handler;
    }

    // 客户端 handler：收到 kind=event 时按 channel 分派
    public static void RegisterClientHandler(string channel, Action<JToken> handler)
    {
        if (string.IsNullOrWhiteSpace(channel))
            return;

        ClientHandlers[channel.Trim()] = handler;
    }

    // 客户端 -> 服务器：请求-响应式调用。onResponse 在响应到达时于 KrokMP 接收线程触发，
    // 调用方应自行用协程/主线程同步（现有 FetchCoroutine 即按此模式等待标志位）。
    public static bool RequestServer(string channel, object? payload, Action<JToken> onResponse)
    {
        if (string.IsNullOrWhiteSpace(channel))
            return false;

        var requestId = Guid.NewGuid().ToString("N");
        PendingResponses[requestId] = onResponse;

        return SendMessage(RequestMessageId, channel, "request", payload, requestId, 0U, null);
    }

    // 客户端 -> 服务器：单向事件（无响应回调）
    public static bool SendToServer(string channel, object? payload)
    {
        return SendMessage(RequestMessageId, channel, "event", payload, null, 0U, null);
    }

    // 服务器 -> 所有已连接客户端：广播 event（供主机 sr 重载触发增量文件同步等场景）
    // targets 取 ServerMain.AllClientIds（IReadOnlyList<knetid>），走 Server_SendToClients 多播。
    public static bool BroadcastToClients(string channel, object? payload)
    {
        if (string.IsNullOrWhiteSpace(channel) || !IsAvailable || !(IsServer || IsHost))
            return false;

        object? allIds;
        try
        {
            allIds = _serverMainAllClientIdsProperty?.GetValue(null, null);
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"BarkKrokBridge failed to read ServerMain.AllClientIds: {ex}");
            return false;
        }

        if (allIds is null)
            return false;

        var envelope = new JObject
        {
            [ChannelField] = channel.Trim(),
            [KindField] = "event",
            [RequestIdField] = string.Empty,
            [SenderField] = 0U,
            [PayloadField] = NormalizePayload(payload)
        };

        return SendEnvelope(ResponseMessageId, envelope, true, 0U, allIds);
    }

    // 服务器 -> 指定客户端：发送 response（内部供服务端 handler 回包使用）
    private static bool SendEnvelopeToClient(
        uint clientId, string channel, string kind, JToken payload, string? requestId)
    {
        var envelope = new JObject
        {
            [ChannelField] = channel,
            [KindField] = kind,
            [RequestIdField] = requestId ?? string.Empty,
            [SenderField] = 0U,
            [PayloadField] = payload
        };

        return SendEnvelope(ResponseMessageId, envelope, true, clientId, null);
    }

    // 发送信封：客户端 -> 服务器走 Client_Send；服务器 -> 客户端走 Server_SendToClients
    private static bool SendEnvelope(
        ushort messageId, JObject envelope, bool reliable, uint clientId, object? targets)
    {
        if (!IsAvailable || !TryBuildWriter(messageId, envelope, out var writer))
            return false;

        // 确保接收器已注册（KrokMP ShutdownReset 可能已清空），否则对方收到消息查不到 handler
        EnsureReceiversRegistered();

        var delivery = reliable ? _reliableOrdered : _reliableUnordered;
        try
        {
            if (targets is not null)
            {
                // 多播：传 IEnumerable<knetid>
                _serverSendToClientsMethod!.Invoke(null, [delivery, writer!, targets]);
                return true;
            }

            if (clientId != 0U || IsHost)
            {
                // 单播：构造单元素 List<knetid>，走多播重载，规避 Server_SendTo 的 knetid 转换难题
                var single = BuildClientIdList([clientId]);
                _serverSendToClientsMethod!.Invoke(null, [delivery, writer!, single]);
                return true;
            }

            // 客户端 -> 服务器
            _clientSendMethod!.Invoke(null, [delivery, writer!]);
            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning(
                $"BarkKrokBridge failed to send message ({ChannelField}='{(envelope[ChannelField]?.Value<string>() ?? "?")}'): {ex}");
            return false;
        }
    }

    // 构造信封并通过 MyLiteNetLibExtensions.Put 写入 NetDataWriter
    private static bool SendMessage(
        ushort messageId, string channel, string kind, object? payload,
        string? requestId, uint clientId, object? targets)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(channel))
            return false;

        var envelope = new JObject
        {
            [ChannelField] = channel.Trim(),
            [KindField] = kind,
            [RequestIdField] = requestId ?? string.Empty,
            [SenderField] = 0U,
            [PayloadField] = NormalizePayload(payload)
        };

        return SendEnvelope(messageId, envelope, true, clientId, targets);
    }

    private static JToken? NormalizePayload(object? payload)
    {
        return payload switch
        {
            null => null,
            JToken token => token,
            _ => JToken.FromObject(payload)
        };
    }

    // 构造 writer 并写入 JSON -> UTF8 -> GZip -> Base64
    private static bool TryBuildWriter(ushort messageId, JObject envelope, out object? writer)
    {
        writer = null;
        if (_createWriterMethod is null)
            return false;

        try
        {
            writer = _createWriterMethod.Invoke(null, [messageId]);
            if (writer is null)
                return false;

            var json = JsonConvert.SerializeObject(envelope, Formatting.None);
            var bytes = Encoding.UTF8.GetBytes(json);
            var compressed = CUCoreUtils.CompressGZip(bytes);
            if (compressed is null)
                return false;

            var base64 = Convert.ToBase64String(compressed);
            if (_writerPutStringMethod is not null)
            {
                _writerPutStringMethod.Invoke(null, [writer, base64, true]);
                return true;
            }

            var putFallback = writer.GetType().GetMethod("Put", [typeof(string)]);
            if (putFallback is not null)
            {
                putFallback.Invoke(writer, [base64]);
                return true;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"BarkKrokBridge failed to build message: {ex}");
        }

        writer = null;
        return false;
    }

    // 从 NetDataReader 读出 base64 字符串并解封为信封
    private static bool TryReadEnvelope(object reader, out JObject? envelope)
    {
        envelope = null;

        try
        {
            var base64 = ReadString(reader);
            if (string.IsNullOrWhiteSpace(base64))
                return false;

            var bytes = CUCoreUtils.DecompressGZip(Convert.FromBase64String(base64));
            if (bytes is null)
                return false;

            var json = Encoding.UTF8.GetString(bytes);
            envelope = JObject.Parse(json);
            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"BarkKrokBridge failed to read message: {ex}");
            return false;
        }
    }

    private static string? ReadString(object reader)
    {
        if (_readerGetStringMethod is not null)
        {
            // reader, out string, bool；第二个元素由 Get 反射填充
            var args = new[] { reader, null!, true };
            _readerGetStringMethod.Invoke(null, args);
            return args[1] as string;
        }

        var getFallback = reader.GetType().GetMethod("GetString",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return getFallback?.Invoke(reader, null) as string;
    }

    // 服务端收到客户端 request：按 channel 分派，handler 返回值回发给请求客户端
    private static void HandleServerMessageObject(object senderBox, object reader)
    {
        var senderId = UnboxId(senderBox);
        HandleEnvelope(senderId, reader, serverSide: true);
    }

    // 客户端收到服务端 response/event
    private static void HandleClientMessageObject(object senderBox, object reader)
    {
        var senderId = UnboxId(senderBox);
        HandleEnvelope(senderId, reader, serverSide: false);
    }

    private static void HandleEnvelope(uint senderClientId, object reader, bool serverSide)
    {
        // TryReadEnvelope 返回 true 时信封必非空
        if (!TryReadEnvelope(reader, out var envelope))
            return;

        var channel = envelope!.Value<string>(ChannelField);
        if (string.IsNullOrWhiteSpace(channel))
            return;

        var kind = envelope.Value<string>(KindField) ?? "event";
        var payload = envelope[PayloadField];
        var requestId = envelope.Value<string>(RequestIdField);

        if (string.Equals(kind, "response", StringComparison.Ordinal))
        {
            // 客户端收到响应：按 requestId 取回调
            if (string.IsNullOrWhiteSpace(requestId) ||
                !PendingResponses.Remove(requestId, out var callback))
                return;

            callback(payload!);
        }
        else if (serverSide)
        {
            // 服务端收到请求：调用 handler 并回包
            if (!ServerHandlers.TryGetValue(channel, out var handler))
                return;

            try
            {
                var result = handler(payload!);
                if (result is null || string.IsNullOrWhiteSpace(requestId))
                    return;

                _ = SendEnvelopeToClient(senderClientId, channel, "response", result, requestId);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogWarning($"BarkKrokBridge server handler failed for '{channel}': {ex}");
            }
        }
        else
        {
            // 客户端收到事件
            if (!ClientHandlers.TryGetValue(channel, out var handler))
                return;

            try
            {
                handler(payload!);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogWarning($"BarkKrokBridge client handler failed for '{channel}': {ex}");
            }
        }
    }

    // 反射解析 KrokMP 4.0.1 并缓存句柄
    private static bool TryResolveRuntime()
    {
        try
        {
            var krokAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(asm => string.Equals(asm.GetName().Name, "KrokoshaCasualtiesMP",
                    StringComparison.OrdinalIgnoreCase));
            if (krokAsm is null)
            {
                Plugin.Logger.LogInfo("BarkKrokBridge: KrokoshaCasualtiesMP not loaded, script mod sync disabled.");
                return false;
            }

            _netType = krokAsm.GetType("KrokoshaCasualtiesMP.Net");
            _serverMainType = krokAsm.GetType("KrokoshaCasualtiesMP.ServerMain");
            _knetidType = krokAsm.GetType("KrokoshaCasualtiesMP.knetid");
            var netPlayerType = krokAsm.GetType("KrokoshaCasualtiesMP.NetPlayer");
            _netPlayerLocalPlayerField = netPlayerType?.GetField("LOCAL_PLAYER",
                BindingFlags.Public | BindingFlags.Static);
            if (_netType is null || _knetidType is null)
            {
                Plugin.Logger.LogWarning(
                    "BarkKrokBridge: critical types missing (Net/knetid), script mod sync disabled.");
                return false;
            }

            // LiteNetLib 类型
            var liteNetAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(asm => string.Equals(asm.GetName().Name, "LiteNetLib",
                    StringComparison.OrdinalIgnoreCase));
            if (liteNetAsm is null)
            {
                Plugin.Logger.LogWarning("BarkKrokBridge: LiteNetLib not loaded, script mod sync disabled.");
                return false;
            }

            _netDataReaderType = liteNetAsm.GetType("LiteNetLib.Utils.NetDataReader");
            _netDataWriterType = liteNetAsm.GetType("LiteNetLib.Utils.NetDataWriter");
            if (_netDataReaderType is null || _netDataWriterType is null)
            {
                Plugin.Logger.LogWarning(
                    "BarkKrokBridge: LiteNetLib reader/writer types missing, script mod sync disabled.");
                return false;
            }

            // 发送方法。CreateWriter 存在 (in Enum) 与 (ushort) 两个 1 参重载，
            // 若用 FindStaticMethodByArity 按元数查找会因重载不明确返回 null，
            // 因此这里精确匹配 ushort 版本；Client_Send 只有 (DeliveryMethod, NetDataWriter) 一个 2 参重载。
            _createWriterMethod = FindCreateWriter(_netType);
            _clientSendMethod = FindStaticMethodByArity(_netType, "Client_Send", 2);
            if (_createWriterMethod is null || _clientSendMethod is null)
            {
                Plugin.Logger.LogWarning(
                    "BarkKrokBridge: CreateWriter/Client_Send not found, script mod sync disabled.");
                return false;
            }

            // DeliveryMethod 枚举类型取 Client_Send 第一个参数类型
            var clientSendParams = _clientSendMethod.GetParameters();
            if (clientSendParams.Length != 2)
            {
                Plugin.Logger.LogWarning("BarkKrokBridge: unexpected Client_Send signature, script mod sync disabled.");
                return false;
            }

            // Client_Send 第一参是 in DeliveryMethod，元数据里为 DeliveryMethod&（ByRef），需去引用取底层枚举
            var deliveryParam = clientSendParams[0].ParameterType;
            if (deliveryParam.IsByRef)
                deliveryParam = deliveryParam.GetElementType() ?? deliveryParam;

            _deliveryMethodType = deliveryParam;
            if (!_deliveryMethodType.IsEnum)
            {
                Plugin.Logger.LogWarning("BarkKrokBridge: could not resolve DeliveryMethod, script mod sync disabled.");
                return false;
            }

            // 服务器 -> 客户端多播：Server_SendToClients 第三参为 IEnumerable<knetid> 的重载
            _serverSendToClientsMethod = FindServerSendToClientsEnumerable(_netType, _knetidType);
            if (_serverSendToClientsMethod is null)
            {
                Plugin.Logger.LogWarning(
                    "BarkKrokBridge: Server_SendToClients(IEnumerable) not found, script mod sync disabled.");
                return false;
            }

            // 字符串读写：KrokMP MyLiteNetLibExtensions.Put/Get
            var extType = krokAsm.GetType("KrokoshaCasualtiesMP.MyLiteNetLibExtensions");
            _writerPutStringMethod = extType is null
                ? null
                : FindPutString(extType, _netDataWriterType);
            _readerGetStringMethod = extType is null
                ? null
                : FindGetString(extType, _netDataReaderType);
            if (_writerPutStringMethod is null || _readerGetStringMethod is null)
            {
                // 允许降级到 NetDataWriter.Put / NetDataReader.GetString 原生方法
                _writerPutStringMethod ??= FindNativeWriterPut(_netDataWriterType);
                _readerGetStringMethod ??= FindNativeReaderGetString(_netDataReaderType);
            }

            // 注册接收器：RegisterServerReceiver/RegisterClientReceiver 是 internal 静态方法
            _registerServerReceiverMethod = FindStaticMethodByArity(_netType, "RegisterServerReceiver", 2);
            _registerClientReceiverMethod = FindStaticMethodByArity(_netType, "RegisterClientReceiver", 2);
            if (_registerServerReceiverMethod is null || _registerClientReceiverMethod is null)
            {
                Plugin.Logger.LogWarning(
                    "BarkKrokBridge: RegisterServerReceiver/RegisterClientReceiver not found, script mod sync disabled.");
                return false;
            }

            // 服务器客户端集合：ServerMain.AllClientIds（静态属性，IReadOnlyList<knetid>）
            _serverMainAllClientIdsProperty = _serverMainType?.GetProperty("AllClientIds",
                BindingFlags.Public | BindingFlags.Static);

            // 可靠性枚举值
            _reliableOrdered = Enum.Parse(_deliveryMethodType, "ReliableOrdered");
            _reliableUnordered = Enum.Parse(_deliveryMethodType, "ReliableUnordered");

            // 生成并缓存匹配 (knetid, ref NetDataReader) 的接收委托，供幂等注册复用。
            // GetMethod 查找的是本类已有的私有静态方法，必非空，用 ! 断言。
            var handlerType = _registerServerReceiverMethod.GetParameters()[1].ParameterType;
            _serverReceiverDelegate = BuildReceiverDelegate(handlerType, typeof(BarkKrokBridge)
                .GetMethod(nameof(HandleServerMessageObject), BindingFlags.NonPublic | BindingFlags.Static)!);
            _clientReceiverDelegate = BuildReceiverDelegate(handlerType, typeof(BarkKrokBridge)
                .GetMethod(nameof(HandleClientMessageObject), BindingFlags.NonPublic | BindingFlags.Static)!);
            if (_serverReceiverDelegate is null || _clientReceiverDelegate is null)
            {
                Plugin.Logger.LogWarning(
                    "BarkKrokBridge: could not build receiver delegates, script mod sync disabled.");
                return false;
            }

            EnsureReceiversRegistered();

            IsAvailable = true;
            Plugin.Logger.LogInfo(
                "BarkKrokBridge: KrokoshaCasualtiesMP detected, script mod sync network layer ready.");
            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"BarkKrokBridge init failed: {ex}");
            return false;
        }
    }

    // 幂等注册接收器：KrokMP 的 ShutdownReset() 会 Clear() SERVER/CLIENT_MESSAGE_HANDLERS，
    // 而 Bark 只在 Plugin.Awake 注册一次。若接收器被清空，本方法重新注册（反射检查字典避免重复 Add 冲突）。
    // 注意：不检查 IsAvailable——初始化时 IsAvailable 尚未置 true，但反射句柄已就绪，必须能注册。
    public static void EnsureReceiversRegistered()
    {
        if (_registerServerReceiverMethod is null || _registerClientReceiverMethod is null ||
            _netType is null)
            return;

        try
        {
            var serverHandlers = _netType?.GetField("SERVER_MESSAGE_HANDLERS",
                BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as System.Collections.IDictionary;
            var clientHandlers = _netType?.GetField("CLIENT_MESSAGE_HANDLERS",
                BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as System.Collections.IDictionary;

            if (serverHandlers is not null && !serverHandlers.Contains(RequestMessageId) &&
                _serverReceiverDelegate is not null)
            {
                _registerServerReceiverMethod.Invoke(null, [RequestMessageId, _serverReceiverDelegate]);
            }

            if (clientHandlers is not null && !clientHandlers.Contains(ResponseMessageId) &&
                _clientReceiverDelegate is not null)
            {
                _registerClientReceiverMethod.Invoke(null, [ResponseMessageId, _clientReceiverDelegate]);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"BarkKrokBridge: EnsureReceiversRegistered failed: {ex.Message}");
        }
    }

    // 生成匹配 KrokoshaHandleNamedMessageDelegate(knetid, ref NetDataReader) 的委托：
    // 把 knetid 装箱、ref reader 解引用，统一以 object 形式转发到 handlerMethod(uint, object)。
    private static Delegate? BuildReceiverDelegate(Type delegateType, MethodInfo handlerMethod)
    {
        try
        {
            var invoke = delegateType.GetMethod("Invoke");
            var invokeParams = invoke?.GetParameters();
            if (invoke is null || invokeParams is null || invokeParams.Length != 2)
                return null;

            var senderParamType = invokeParams[0].ParameterType; // knetid
            var readerParamType = invokeParams[1].ParameterType; // ref NetDataReader
            var readerByVal = readerParamType.IsByRef
                ? readerParamType.GetElementType()
                : readerParamType;
            if (readerByVal is null)
                return null;

            // DynamicMethod 签名必须与 delegate 一致：(knetid, ref NetDataReader) -> void
            var dm = new DynamicMethod("Bark_MP_Receiver_" + handlerMethod.Name, typeof(void),
                [senderParamType, readerParamType], typeof(BarkKrokBridge).Module, true);
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // knetid 结构体
            il.Emit(OpCodes.Box, senderParamType); // 装箱为 object
            il.Emit(OpCodes.Ldarg_1); // ref NetDataReader
            il.Emit(OpCodes.Ldind_Ref); // 解引用为 NetDataReader 引用
            il.Emit(OpCodes.Call, handlerMethod); // handlerMethod(object, object)
            il.Emit(OpCodes.Ret);

            return dm.CreateDelegate(delegateType);
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"BarkKrokBridge could not build receiver delegate: {ex}");
            return null;
        }
    }

    // 按名 + 元数查找静态方法（宽松匹配，规避 CUCoreLib 严格类型匹配问题）
    private static MethodInfo? FindStaticMethodByArity(Type type, string name, int paramCount)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        MethodInfo? found = null;
        foreach (var m in type.GetMethods(flags))
        {
            if (m.Name != name || m.GetParameters().Length != paramCount)
                continue;
            if (found is not null)
                return null; // 出现多个重载则视为不明确
            found = m;
        }

        return found;
    }

    // 精确匹配 Net.CreateWriter(ushort msgid)（允许 in/ByRef 修饰），
    // 避开与 CreateWriter(in Enum msgid) 的同元数重载歧义。
    private static MethodInfo? FindCreateWriter(Type netType)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        foreach (var m in netType.GetMethods(flags))
        {
            if (m.Name != "CreateWriter")
                continue;

            var ps = m.GetParameters();
            if (ps.Length != 1)
                continue;

            var p0 = ps[0].ParameterType;
            if (p0.IsByRef)
                p0 = p0.GetElementType();
            if (p0 == typeof(ushort))
                return m;
        }

        return null;
    }

    // 找第三参为 IEnumerable<elemType> 的 Server_SendToClients 重载
    private static MethodInfo? FindServerSendToClientsEnumerable(Type netType, Type elemType)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        foreach (var m in netType.GetMethods(flags))
        {
            if (m.Name != "Server_SendToClients")
                continue;
            var ps = m.GetParameters();
            if (ps.Length != 3)
                continue;

            var third = ps[2].ParameterType;
            if (third.IsByRef)
                third = third.GetElementType();
            if (third is not { IsGenericType: true })
                continue;
            if (!typeof(IEnumerable).IsAssignableFrom(third))
                continue;

            var genArg = third.GetGenericArguments()[0];
            if (genArg == elemType)
                return m;
        }

        return null;
    }

    // 找 KrokMP MyLiteNetLibExtensions.Put(writer, string, bool)
    private static MethodInfo? FindPutString(Type extType, Type writerType)
    {
        return (from m in extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            where m.Name == "Put"
            let ps = m.GetParameters()
            where ps.Length == 3 && ps[0].ParameterType == writerType && ps[1].ParameterType == typeof(string) &&
                  ps[2].ParameterType == typeof(bool)
            select m).FirstOrDefault();
    }

    // 找 KrokMP MyLiteNetLibExtensions.Get(reader, out string, bool)
    private static MethodInfo? FindGetString(Type extType, Type readerType)
    {
        return (from m in extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            where m.Name == "Get"
            let ps = m.GetParameters()
            where ps.Length == 3 && ps[0].ParameterType == readerType && ps[1].IsOut &&
                  ps[1].ParameterType == typeof(string).MakeByRefType() && ps[2].ParameterType == typeof(bool)
            select m).FirstOrDefault();
    }

    // 原生 NetDataWriter.Put(string) 兜底
    private static MethodInfo? FindNativeWriterPut(Type writerType)
    {
        return writerType.GetMethod("Put", [typeof(string)]);
    }

    // 原生 NetDataReader.GetString() 兜底
    private static MethodInfo? FindNativeReaderGetString(Type readerType)
    {
        return readerType.GetMethod("GetString",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    // 读取 Net 静态 bool 属性（running/is_client/is_server/is_host）
    private static bool GetNetBool(string memberName)
    {
        if (!IsAvailable || _netType is null)
            return false;

        try
        {
            var prop = _netType.GetProperty(memberName, BindingFlags.Public | BindingFlags.Static);
            return prop is not null && prop.PropertyType == typeof(bool) &&
                   prop.GetValue(null, null) is true;
        }
        catch
        {
            return false;
        }
    }

    // 构造目标客户端列表：元素类型为 knetid 的 List
    private static object BuildClientIdList(IEnumerable<uint> ids)
    {
        var elem = _knetidType ?? typeof(uint);
        var listType = typeof(List<>).MakeGenericType(elem);
        // Activator.CreateInstance 理论上可能返回 null，但 List<T> 构造不会；此处用 ! 断言非空
        var list = (IList)Activator.CreateInstance(listType)!;
        foreach (var id in ids)
            list.Add(BoxId(id, elem));
        return list;
    }

    // 将 ID 值拆成 uint：原生整型直接转；结构体读首个 primitive 字段，或经隐式/显式转换
    internal static uint UnboxId(object value)
    {
        var t = value.GetType();
        if (t.IsPrimitive)
        {
            try
            {
                return Convert.ToUInt32(value);
            }
            catch
            {
                return 0;
            }
        }

        try
        {
            foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!field.FieldType.IsPrimitive)
                    continue;
                return Convert.ToUInt32(field.GetValue(value));
            }
        }
        catch
        {
            // 继续尝试转换运算符
        }

        try
        {
            foreach (var op in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (op.Name != "op_Implicit" && op.Name != "op_Explicit")
                    continue;
                var opParams = op.GetParameters();
                if (op.ReturnType.IsPrimitive && opParams.Length == 1 && opParams[0].ParameterType == t)
                    return Convert.ToUInt32(op.Invoke(null, [value]));
            }
        }
        catch
        {
            // 无法转换，返回 0
        }

        return 0;
    }

    // 将 uint 装回目标 ID 类型：原生整型直接转；结构体用单参构造函数或隐式转换运算符
    internal static object BoxId(uint value, Type targetType)
    {
        if (targetType == typeof(uint))
            return value;
        if (targetType == typeof(ushort))
            return (ushort)value;
        if (targetType == typeof(ulong))
            return (ulong)value;
        if (targetType == typeof(int))
            return (int)value;
        if (targetType == typeof(long))
            return (long)value;

        try
        {
            foreach (var ctor in targetType.GetConstructors())
            {
                var ctorParams = ctor.GetParameters();
                if (ctorParams is [{ ParameterType.IsPrimitive: true }])
                    return ctor.Invoke([Convert.ChangeType(value, ctorParams[0].ParameterType)])!;
            }
        }
        catch
        {
            // 继续尝试隐式转换运算符
        }

        try
        {
            foreach (var op in targetType.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (op.Name != "op_Implicit")
                    continue;
                var opParams = op.GetParameters();
                if (op.ReturnType == targetType && opParams is [{ ParameterType.IsPrimitive: true }])
                    return op.Invoke(null, [Convert.ChangeType(value, opParams[0].ParameterType)])!;
            }
        }
        catch
        {
            // 无法转换，原样返回
        }

        return value;
    }
}