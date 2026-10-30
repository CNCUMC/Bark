using System.Collections.Generic;
using System.Linq;

namespace Bark.Items.Runtime.Gun;

public static class GunMagTracker
{
    private static readonly Dictionary<Item, MagState> States = new();

    public static MagState? Get(Item gunItem)
    {
        return States.GetValueOrDefault(gunItem);
    }

    public static MagState GetOrCreate(Item gunItem)
    {
        if (States.TryGetValue(gunItem, out var state)) return state;
        state = new MagState();
        States[gunItem] = state;

        return state;
    }

    public static void Remove(Item gunItem)
    {
        States.Remove(gunItem);
    }

    public static void ClearAll()
    {
        States.Clear();
    }

    public static void CleanupDestroyed()
    {
        var toRemove = (
            from kv in States
            where kv.Key == null || kv.Key.gameObject == null
            select kv.Key!).ToList();

        foreach (var item in toRemove)
            States.Remove(item);
    }

    public class MagState
    {
        public string? AmmoItemId;
        public string? MagItemId;
        public string? PendingCasingType;
        public int RoundsInMag;
    }
}
