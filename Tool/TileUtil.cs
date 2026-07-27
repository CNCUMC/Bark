using System;
using System.Collections.Generic;
using System.Reflection;
using Bark.Constant;
using Bark.ScriptApi;
using CUCoreLib.Registries;

namespace Bark.Tool;

// 物块工具：字符串物块 ID 与索引的互查，脚本侧可使用字符串操作世界方块。
public static class TileUtil
{
    // 原版物块 LocaleKey → ushort 索引（构建一次，只读）
    private static readonly Dictionary<string, ushort> s_vanillaLookup = BuildVanillaLookup();

    // TileRegistry.RegisteredDefinitionIds 字段缓存（运行时动态读取）
    private static readonly FieldInfo? s_registeredIdsField = typeof(TileRegistry).GetField(
        "RegisteredDefinitionIds", BindingFlags.NonPublic | BindingFlags.Static);

    // 将物块 ID 字符串解析为 ushort 索引。若未找到则记警告并抛出。
    [ScriptMethod]
    public static ushort ResolveIndex(string tileId)
    {
        if (TryResolveIndex(tileId, out var index))
            return index;
        LogUtil.Warning("world.tile_not_found", tileId);
        throw new ArgumentException($"Tile '{tileId}' is not registered.", nameof(tileId));
    }

    // 尝试将物块 ID 字符串解析为 ushort 索引，返回是否成功。
    // 查找顺序：原版 Blocks → TileRegistry 自定义物块
    public static bool TryResolveIndex(string tileId, out ushort index)
    {
        if (!string.IsNullOrEmpty(tileId))
        {
            // 1. 原版物块（Blocks 常量 LocaleKey 匹配，大小写不敏感）
            if (s_vanillaLookup.TryGetValue(tileId, out index))
                return true;

            // 2. 自定义物块（TileRegistry.RegisteredDefinitionIds）
            if (s_registeredIdsField?.GetValue(null) is Dictionary<string, ushort> customIds
                && customIds.TryGetValue(tileId, out index))
                return true;
        }

        index = 0;
        return false;
    }

    // 扫描 Blocks 常量，构建 LocaleKey → Id 映射
    private static Dictionary<string, ushort> BuildVanillaLookup()
    {
        var dict = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in typeof(Blocks).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType != typeof(Blocks) || field.GetValue(null) is not Blocks block)
                continue;
            if (!string.IsNullOrEmpty(block.LocaleKey))
                dict[block.LocaleKey] = block.Id;
        }

        return dict;
    }
}
