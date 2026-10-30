namespace Bark.Items;

public static class ItemScriptContext
{
    public static Item? CurrentItem { get; internal set; }
    public static string? CurrentAction { get; internal set; }
    public static float? CurrentTriggerValue { get; internal set; }
    public static float? CurrentTriggerThreshold { get; internal set; }
    public static string? CurrentTriggerOperator { get; internal set; }

    internal static void ClearTriggerContext()
    {
        CurrentTriggerValue = null;
        CurrentTriggerThreshold = null;
        CurrentTriggerOperator = null;
    }
}
