namespace Bark.Script;

public static class ScriptCallContext
{
    internal static string? CurrentModId { get; set; }

    public static string ResolveItemId(string itemId)
    {
        if (itemId.Contains('.') || CurrentModId == null)
            return itemId;
        return $"{CurrentModId}.{itemId}";
    }
}