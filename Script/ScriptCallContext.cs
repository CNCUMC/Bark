namespace Bark.Script;

// 脚本调用上下文：追踪当前正在执行的脚本模组 ID。
// 在 PuerTS Eval 用户脚本前由 ScriptEngine 设置，Eval 结束后清除。
// 工具方法通过 ResolveItemId() 自动将裸物品 ID 补全为 {modId}.{itemId} 命名空间格式。
public static class ScriptCallContext
{
    // 当前正在执行脚本的模组 ID。无上下文时为 null。
    internal static string? CurrentModId { get; set; }

    // 解析物品 ID：若已有命名空间前缀（含 '.'）、或无脚本上下文，则原样返回；
    // 否则补全为 {CurrentModId}.{itemId}。
    public static string ResolveItemId(string itemId)
    {
        if (itemId.Contains('.') || CurrentModId == null)
            return itemId;
        return $"{CurrentModId}.{itemId}";
    }
}
