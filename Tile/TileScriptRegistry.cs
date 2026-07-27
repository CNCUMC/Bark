using System;
using System.Collections.Generic;
using Bark.Script;
using Bark.Tool;

namespace Bark.Tile;

// 物块脚本映射存储：tileId → (ScriptEngine, 脚本文件路径列表按动作分组)
// 供 TileScriptRunner 在物块事件触发时查找并执行脚本。
public static class TileScriptRegistry
{
    // tileId → 脚本映射记录
    private static readonly Dictionary<string, TileScriptEntry> Entries = new();

    // 注册一个物块的脚本映射
    public static void Register(string tileId, TileScriptDef scriptDef, ScriptEngine engine, string modId, string modDir)
    {
        if (string.IsNullOrEmpty(tileId))
            throw new ArgumentNullException(nameof(tileId));
        if (scriptDef is null)
            throw new ArgumentNullException(nameof(scriptDef));
        if (engine is null)
            throw new ArgumentNullException(nameof(engine));
        if (string.IsNullOrEmpty(modId))
            throw new ArgumentNullException(nameof(modId));
        if (string.IsNullOrEmpty(modDir))
            throw new ArgumentNullException(nameof(modDir));

        if (IsEmpty(scriptDef))
            return;

        Entries[tileId] = new TileScriptEntry(engine, scriptDef, modId, modDir);
    }

    // 注销指定物块的脚本映射（热重载时调用）
    public static void Unregister(string tileId)
    {
        Entries.Remove(tileId);
    }

    // 获取指定物块的脚本映射，未注册时返回 null
    public static TileScriptEntry? GetEntry(string tileId)
    {
        return Entries.GetValueOrDefault(tileId);
    }

    // 判断脚本定义是否所有动作都为空
    private static bool IsEmpty(TileScriptDef def)
    {
        return def.OnPlace.Count == 0
               && def.OnExist.Count == 0
               && def.OnDamaging.Count == 0
               && def.OnDestroyed.Count == 0;
    }
}

// 单个物块的脚本映射记录
public class TileScriptEntry(ScriptEngine engine, TileScriptDef scriptDef, string modId, string modDir)
{
    public readonly ScriptEngine Engine = engine;
    public readonly string ModId = modId;
    public readonly List<string> OnPlace = scriptDef.OnPlace;
    public readonly List<string> OnExist = scriptDef.OnExist;
    public readonly List<string> OnDamaging = scriptDef.OnDamaging;
    public readonly List<string> OnDestroyed = scriptDef.OnDestroyed;
    public readonly string ModDir = modDir;
}
