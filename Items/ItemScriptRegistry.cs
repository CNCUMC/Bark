using System;
using System.Collections.Generic;
using System.IO;
using Bark.Script;
using Bark.Tool;

namespace Bark.Items;

// 物品脚本映射存储：itemId → (ScriptEngine, 脚本文件路径列表按动作分组)
// 供 ItemScriptRunner 在物品事件触发时查找并执行脚本。
public static class ItemScriptRegistry
{
    // itemId → 脚本映射记录
    private static readonly Dictionary<string, ScriptEntry> Entries = new();

    // 注册一个物品的脚本映射。itemId 为物品 ID，scriptDef 为 JSON 反序列化的脚本定义，
    // engine 为模组的 ScriptEngine（每个模组一个），modId 为模组 ID，modDir 为模组目录。
    public static void Register(string itemId, ItemScriptDef scriptDef, ScriptEngine engine, string modId, string modDir)
    {
        if (string.IsNullOrEmpty(itemId))
            throw new ArgumentNullException(nameof(itemId));
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

        Entries[itemId] = new ScriptEntry(engine, scriptDef, modId, modDir);
    }

    // 注销指定物品的脚本映射（热重载时调用）
    public static void Unregister(string itemId)
    {
        Entries.Remove(itemId);
    }

    // 获取指定物品的脚本映射，未注册时返回 null
    public static ScriptEntry? GetEntry(string itemId)
    {
        return Entries.GetValueOrDefault(itemId);
    }

    // 判断脚本定义是否所有动作都为空
    private static bool IsEmpty(ItemScriptDef def)
    {
        return def.Use.Count == 0
               && def.UseInHand.Count == 0
               && def.Equip.Count == 0
               && def.Unequip.Count == 0
               && def.UseOnLimb.Count == 0
               && def.Attack.Count == 0;
    }
}

// 单个物品的脚本映射记录：包含引擎引用、模组 ID、脚本文件路径列表（按动作分组）、模组目录
public class ScriptEntry(ScriptEngine engine, ItemScriptDef scriptDef, string modId, string modDir)
{
    public ScriptEngine Engine = engine;
    public readonly string ModId = modId;
    public readonly List<string> Use = scriptDef.Use;
    public readonly List<string> UseInHand = scriptDef.UseInHand;
    public readonly List<string> Equip = scriptDef.Equip;
    public readonly List<string> Unequip = scriptDef.Unequip;
    public readonly List<string> UseOnLimb = scriptDef.UseOnLimb;
    public readonly List<string> Attack = scriptDef.Attack;
    public readonly string ModDir = modDir;

    // 将相对路径解析为绝对路径（基准为模组目录）
    public string ResolvePath(string relativePath)
    {
        return Path.Combine(ModDir, relativePath);
    }
}
